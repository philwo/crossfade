using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Runtime.InteropServices;

using System.Windows.Forms;

namespace ZeroconfService
{
    /// <summary>
    /// <para>
    /// The NetService class represents a network service that your application publishes
    /// or uses as a client. This class, along with the browser class
    /// <see cref="NetServiceBrowser">NetServiceBrowser</see> use multicast DNS to
    /// communicate accross the local network.
    /// </para>
    /// <para>
    /// Your application can use this class to either publish information about
    /// a service, or as a client to retrieve information about another service.
    /// </para>
    /// <para>
    /// If you intend to publish a service, you must setup the service to publish and
    /// acquire a port on which the socket will recieve connections. You can then create
    /// a NetService instance to represent your service and publish it.
    /// </para>
    /// <para>
    /// If you intend to resolve a service, you can either use <see cref="NetServiceBrowser">NetServiceBrowser</see>
    /// to discover services of a given type, or you can create a new NetService object
    /// to resolve information about an known existing service.
    /// See <see cref="NetService(string,string,string)">NetService()</see>
    /// for information about creating new net services.
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Network operations are performed asynchronously and are returned to your application
    /// via events fired from within this class. Events are typically fire in your
    /// application's main run loop, see <see cref="DNSService">DNSService</see> for information
    /// about controlling asynchronous events.
    /// </para>
    /// <para>
    /// It is important to note that this class uses the same asynchronous method to
    /// publish records as it does to fire events. So if you are simply publishing a service,
    /// you must still ensure that the <see cref="DNSService">DNSService</see> parent class
    /// is properly placed into a run loop.
    /// </para>
    /// </remarks>
    public sealed class NetService : DNSService
    {
        /// <summary>
        /// Represents the method that will handle <see cref="DidResolveService">DidResolveService</see>
        /// events from a <see cref="NetService">NetService</see> instance.
        /// </summary>
        /// <param name="service">Sender of this event.</param>
        public delegate void ServiceResolved(NetService service);

        /// <summary>
        /// Occurs when a service was resolved.
        /// </summary>
        public event ServiceResolved DidResolveService;

        /// <summary>
        /// Represents the method that will handle <see cref="DidUpdateTXT">DidUpdateTXT</see>
        /// events from a <see cref="NetService">NetService</see> instance.
        /// </summary>
        /// <param name="service">Sender of this event.</param>
        public delegate void ServiceTXTUpdated(NetService service);

        /// <summary>
        /// Occurs when the TXT record for a given service was updated.
        /// </summary>
        /// <remarks>
        /// This event is not fired after you update the TXT record for an event yourself.
        /// </remarks>
        public event ServiceTXTUpdated DidUpdateTXT;

        private mDNSImports.DNSServiceResolveReply resolveReplyCb;
        private mDNSImports.DNSServiceRegisterReply registerReplyCb;
        private GCHandle gchSelf;

        /// <summary>
        /// Initializes a new instance of the NetService class for resolving.
        /// </summary>
        /// <param name="domain">The domain of the service. For the local domain, use <c>"local."</c> not <c>""</c>.</param>
        /// <param name="type"><para>The network service type.</para>
        /// <para>This must include both the transport type (<c>"_tcp."</c> or <c>".udp"</c>)
        /// and the service name prefixed with an underscore(<c>"_"</c>). For example, to search
        /// for an HTTP service on TCP you would use <c>"_http._tcp."</c></para></param>
        /// <param name="name">The name of the service to resolve.</param>
        /// <remarks>
        /// <para>This constructor is the appropriate constructor used to resolve a service.
        /// You can not use this constructor to publish a service.</para>
        /// </remarks>
        public NetService(string domain, string type, string name)
        {
            mDomain = domain;
            mType = type;
            mName = name;
        }

        /// <summary>
        /// Initializes a new instance of the NetService class for publishing.
        /// </summary>
        /// <param name="domain">
        /// <para>The domain of the service. For the local domain, use <c>"local."</c> not <c>""</c>.</para>
        /// <para>To us the default domain, simply parse <c>""</c>.</para>
        /// </param>
        /// <param name="type"><para>The network service type.</para>
        /// <para>This must include both the transport type (<c>"_tcp."</c> or <c>".udp"</c>)
        /// and the service name prefixed with an underscore(<c>"_"</c>). For example, to search
        /// for an HTTP service on TCP you would use <c>"_http._tcp."</c></para></param>
        /// <param name="name">The name of the service. This name must be unique.</param>
        /// <param name="port">The port number on which your service is available.</param>
        /// <remarks>
        /// <para>This constructor is the appropriate constructor used to publish a service.
        /// You can not use this constructor to resolve a service.</para>
        /// </remarks>
        public NetService(string domain, string type, string name, int port)
        {
            mDomain = domain;
            mType = type;
            mName = name;
            mPort = port;
        }

        /// <summary>
        /// Starts a resolve process with a timeout.
        /// </summary>
        /// <param name="seconds">The maximum number of seconds to attempt a resolve.</param>
        public void ResolveWithTimeout(int seconds)
        {
            Stop();

            resolveReplyCb = new mDNSImports.DNSServiceResolveReply(ResolveReply);
            gchSelf = GCHandle.Alloc(this);

            DNSServiceErrorType err;
            err = mDNSImports.DNSServiceResolve(out sdRef, 0, 0, Name, Type, Domain, resolveReplyCb, (IntPtr)gchSelf);

            if (err != DNSServiceErrorType.kDNSServiceErr_NoError)
            {
                throw new DNSServiceException("DNSServiceResolve", err);
            }

            SetupWatchSocket();
        }

        /// <summary>
        /// Stops the currently running search or resolution.
        /// </summary>
        public void Stop()
        {
            /* FIXME: do i need to stop the poll? ... */
            if (sdRef != IntPtr.Zero)
            {
                mDNSImports.DNSServiceRefDeallocate(sdRef);
                sdRef = IntPtr.Zero;
            }
            WaitStop();
            resolveReplyCb = null;
            if (gchSelf.IsAllocated)
            {
                gchSelf.Free();
            }
        }

        private static void ResolveReply(IntPtr sdRef,
            DNSServiceFlags flags,
            UInt32 interfaceIndex,
            DNSServiceErrorType errorCode,
            String fullname,
            String hosttarget,
            UInt16 port,
            UInt16 txtLen,
            byte[] txtRecord,
            IntPtr context)
        {
            GCHandle gch = (GCHandle)context;
            NetService c = (NetService)gch.Target;

            Console.WriteLine("{4} fullname: {0}, hosttarget: {1}, port: {2}, txtLen: {3}",
                fullname, hosttarget, port, txtLen,
                System.Threading.Thread.CurrentThread.ManagedThreadId);

            /* set TXT record data */
            c.TXTRecordData = txtRecord;

            c.mHostName = hosttarget;
            c.mPort = (int)System.Net.IPAddress.NetworkToHostOrder((short)port);

            if (c.DidUpdateTXT != null)
                c.DidUpdateTXT(c);

            AsyncCallback cb = new AsyncCallback(c.AsyncGetHostEntryCallback);

            IAsyncResult ar = System.Net.Dns.BeginGetHostEntry(hosttarget, cb, c);
        }

        private IAsyncResult asyncResultsHostEntry;
        private void AsyncGetHostEntryCallback(IAsyncResult result)
        {
            Console.WriteLine("{0} Finished resolve",
                System.Threading.Thread.CurrentThread.ManagedThreadId);
            
            asyncResultsHostEntry = result;
            NetService c = (NetService)result.AsyncState;
            Invoke(new MethodInvoker(GetHostEntryFinished));
        }

        private void GetHostEntryFinished()
        {
            System.Net.IPHostEntry hostInfo = System.Net.Dns.EndGetHostEntry(asyncResultsHostEntry);
            asyncResultsHostEntry = null;

            ArrayList endpoints = new ArrayList();

            foreach (System.Net.IPAddress address in hostInfo.AddressList)
            {
                Console.WriteLine("{0} With address: {1}", System.Threading.Thread.CurrentThread.ManagedThreadId, address);

                System.Net.IPEndPoint ep = new System.Net.IPEndPoint(address, mPort);
                endpoints.Add(ep);
            }
            mAddresses = endpoints;

            if (DidResolveService != null)
                DidResolveService(this);
        }

        /// <summary>
        /// Returns a <c>byte[]</c> object representing a TXT record
        /// from a given dictionary.
        /// </summary>
        /// <param name="dict">A dictionary containing a TXT record.</param>
        /// <returns>A <c>byte[]</c> object representing TXT data formed from <c>dict</c>.</returns>
        /// <remarks>
        /// <para>The dictionary must contain a set of key / value pairs representing
        /// a TXT record.</para>
        /// <para>The key objects must be <see cref="System.String">String</see> (or <c>string</c>) objects.
        /// These will be converted to UTF-8 format by this method.</para>
        /// <para>The value objects must be <c>byte[]</c> arrays. If these represent
        /// strings, it is highly recommended that they be converted to UTF-8 format.
        /// </para>
        /// </remarks>
        public static byte[] DataFromTXTRecordDictionary(IDictionary dict)
        {
            Encoding u8e = Encoding.UTF8;

            ArrayList entries = new ArrayList(); /* of byte[] */
            int totalDataLength = 0;
            foreach (DictionaryEntry kvp in dict)
            {
                string key = (string)kvp.Key; /* is a string */
                byte[] value = (byte[])kvp.Value;

                byte length = (byte)u8e.GetByteCount(key);
                if (value != null)
                {
                    length += 1; /* for '=' */
                    length += (byte)value.Length;
                }

                byte[] data = new byte[length];

                byte[] keyData = u8e.GetBytes(key);

                Array.Copy(keyData, data, keyData.Length);

                if (value != null)
                {
                    data[keyData.Length] = (byte)'=';
                    Array.Copy(value, 0, data, keyData.Length + 1, value.Length);
                }

                entries.Add(data);

                totalDataLength += length;
            }

            byte[] entireData = new byte[totalDataLength + entries.Count];
            int i = 0;
            foreach (byte[] itemData in entries)
            {
                entireData[i++] = (byte)itemData.Length;
                Array.Copy(itemData, 0, entireData, i, itemData.Length);
                i += itemData.Length;
            }

            return entireData;
        }

        /// <summary>
        /// Returns an <c>IDictionary</c> representing a TXT record.
        /// </summary>
        /// <param name="txtRecord">A <c>byte[]</c> object encoding of a TXT record.</param>
        /// <returns>A dictionary representing a TXT record.</returns>
        public static IDictionary DictionaryFromTXTRecordData(byte[] txtRecord)
        {
            Encoding u8e = Encoding.UTF8;

            Hashtable dict = new Hashtable();

            for (int i = 0; i < txtRecord.Length; i++)
            {
                byte length = txtRecord[i];
                byte[] data = new byte[length];

                byte equalsat = 0;

                for (int j = 0; j < length; j++)
                {
                    data[j] = txtRecord[i + 1 + j];
                    byte equalsbyte = (byte)'=';
                    if (data[j] == equalsbyte)
                    {
                        equalsat = (byte)j;
                    }
                }
                /* data is either:
                 *    key
                 *    key=
                 *    key=value
                 * where 'key' is a UTF-8 string and value is binary data
                 */
                string key;
                byte[] value = null;

                if (equalsat > 0)
                {
                    key = u8e.GetString(data, 0, equalsat);
                    byte valuelen = (byte)(length - (equalsat + 1));
                    value = new byte[valuelen];
                    for (int j = 0; j < valuelen; j++)
                    {
                        value[j] = data[equalsat + 1 + j];
                    }
                }
                else
                {
                    key = u8e.GetString(data);
                }

                dict.Add(key, value);

                i += length;
                Console.WriteLine("txt record: {0}={1}", key, (value != null) ? u8e.GetString(value) : "(no value)");
            }

            return dict;
        }

        /// <summary>
        /// Attempts to advertise the service on the network.
        /// </summary>
        public void Publish()
        {
            Stop();

            registerReplyCb = new mDNSImports.DNSServiceRegisterReply(RegisterReply);
            gchSelf = GCHandle.Alloc(this);

            DNSServiceErrorType err;

            UInt16 txtRecordLen = (UInt16)((TXTRecordData != null) ? TXTRecordData.Length : 0);
            UInt16 port = (UInt16)System.Net.IPAddress.HostToNetworkOrder((short)mPort);
            err = mDNSImports.DNSServiceRegister(out sdRef, 0, 0, Name, Type, Domain, null, port,
                txtRecordLen, TXTRecordData, registerReplyCb, (IntPtr)gchSelf);

            if (err != DNSServiceErrorType.kDNSServiceErr_NoError)
            {
                throw new DNSServiceException("DNSServiceRegister", err);
            }

            SetupWatchSocket();
        }

        private void RegisterReply(IntPtr sdRef,
            DNSServiceFlags flags,
            DNSServiceErrorType errorCode,
            String name,
            String regtype,
            String domain,
            IntPtr context)
        {
            if (errorCode != DNSServiceErrorType.kDNSServiceErr_NoError)
            {
                throw new DNSServiceException("DNSServiceRegisterReply", errorCode);
            }
        }

        private byte[] mTXTRecordData;
        /// <summary>
        /// Gets or sets the TXT record data.
        /// </summary>
        public byte[] TXTRecordData
        {
            get { return mTXTRecordData; }
            set { mTXTRecordData = value; }
        }

        /*
        private System.Net.IPHostEntry mAddresses;
        /// <summary>
        /// Gets a <see cref="System.Net.IPHostEntry">IPHostEntry</see> object
        /// representing the available addresses of the resolved service.
        /// </summary>
        public System.Net.IPHostEntry Addresses
        {
            get { return Addresses; }
        }*/

        /// <summary>
        /// List of <see cref="System.Net.IPEndPoint">IPEndPoint</see>s.
        /// </summary>
        private ArrayList mAddresses;
        /// <summary>
        /// <para>Gets an IList object representing the available addresses of the
        /// resolved service.</para>
        /// <para>The objects in the list are <see cref="System.Net.IPEndPoint">IPEndPoint</see>s</para>
        /// </summary>
        public IList Addresses
        {
            get { return mAddresses; }
        }

        private string mHostName;
        /// <summary>
        /// Gets the host name of the computer providing the service.
        /// </summary>
        public string HostName
        {
            get { return mHostName; }
        }

        private string mName;
        /// <summary>
        /// Gets the name of the service.
        /// </summary>
        public string Name
        {
            get { return mName; }
        }

        private string mType;
        /// <summary>
        /// Gets the type of the service.
        /// </summary>
        public string Type
        {
            get { return mType; }
        }

        private string mDomain;
        /// <summary>
        /// Gets the domain of the service.
        /// </summary>
        /// <remarks>
        /// This can be an explicit domain or it can contain the generic local (<c>"local."</c>) domain.
        /// </remarks>
        public string Domain
        {
            get { return mDomain; }
        }

        private int mPort;
    }
}
