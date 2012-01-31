using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

using System.Windows.Forms;

namespace ZeroconfService
{
    /// <summary>
    /// <para>
    /// The NetServiceBrowser class enables the user of the class to find
    /// published services on a network using multicast DNS. The user uses
    /// and instance of the NetServiceBrowser, called a <B>network service browser</B>,
    /// to find such Printers, HTTP and FTP servers.
    /// </para>
    /// <para>
    /// A <B>network service browser</B> can be used to obtain a list of possible
    /// domains or services. A <see cref="NetService">NetService</see> is then obtained for each discovered
    /// service. You can perform multiple searches at a time by using multiple
    /// <B>network service browsers</B>.
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Network searches are performed asynchronously and are returned to your application
    /// via events fired from within this class. Events are typically fired in
    /// your application's main run loop, see <see cref="DNSService">DNSService</see> for information
    /// about controlling asynchronous events.
    /// </para>
    /// </remarks>
    public sealed class NetServiceBrowser : DNSService
    {
        /// <summary>
        /// Represents the method that will handle <see cref="DidFindService">DidFindService</see>
        /// events from a <see cref="NetServiceBrowser">NetServiceBrowser</see> instance.
        /// </summary>
        /// <param name="browser">Sender of this event.</param>
        /// <param name="service"><see cref="NetService">NetService</see> found by the the browser. This object
        /// can be used to obtain more information about the service.</param>
        /// <param name="moreComing">True when more services will be arriving shortly.</param>
        /// <remarks>
        /// <para>The target uses this delegate to compile a list of services. It should wait
        /// until moreComing is false before updating the user interface, so as to avoid
        /// flickering.
        /// </para>
        /// <para>
        /// The <c>service</c> object inherits its <see cref="DNSService">DNSService</see>
        /// invokable options from <b>browser</b>.
        /// </para>
        /// </remarks>
        public delegate void ServiceFound(NetServiceBrowser browser, NetService service, bool moreComing);

        /// <summary>
        /// Occurs when a <see cref="NetService">NetService</see> was found.
        /// </summary>
        public event ServiceFound DidFindService;

        /// <summary>
        /// Represents the method that will handle <see cref="DidRemoveService">DidRemoveService</see>
        /// events from a <see cref="NetServiceBrowser">NetServiceBrowser</see> instance.
        /// </summary>
        /// <param name="browser">Sender of this event.</param>
        /// <param name="service"><see cref="NetService">NetService</see> to be removed from the browser.
        /// This object is the same instance as was reported by the <see cref="ServiceFound">ServiceFound</see>
        /// event.</param>
        /// <param name="moreComing">True when more services will be made unavailable shortly.</param>
        /// <remarks>
        /// The target uses this delegate to compile a list of services. It should wait
        /// until moreComing is false before updating the user interface, so as to avoid
        /// flickering.
        /// </remarks>
        public delegate void ServiceRemoved(NetServiceBrowser browser, NetService service, bool moreComing);

        /// <summary>
        /// Occurs when a <see cref="NetService">NetService</see> is no longer available.
        /// </summary>
        public event ServiceRemoved DidRemoveService;


        /// <summary>
        /// Represents the method that will handle <see cref="DidFindDomain">DidFindDomain</see>
        /// events from a <see cref="NetServiceBrowser">NetServiceBrowser</see> instance.
        /// </summary>
        /// <param name="browser">Sender of this event.</param>
        /// <param name="domainName">Name of the domain found.</param>
        /// <param name="moreComing">True when more domains will be arriving shortly.</param>
        /// <remarks>
        /// The target uses this delegate to compile a list of domains. It should wait
        /// until moreComing is false before updating the user interface, so as to
        /// avoid flickering.
        /// </remarks>
        public delegate void DomainFound(NetServiceBrowser browser, string domainName, bool moreComing);

        /// <summary>
        /// Occurs when a domain has been found.
        /// </summary>
        public event DomainFound DidFindDomain;

        /// <summary>
        /// Represents the method that will handle <see cref="DidRemoveDomain">DidRemoveDomain</see>
        /// events from a <see cref="NetServiceBrowser">NetServiceBrowser</see> instance.
        /// </summary>
        /// <param name="browser">Sender of this event.</param>
        /// <param name="domainName">Name of the domain that is no longer available.</param>
        /// <param name="moreComing">True when more domains will be made unavailble shortly.</param>
        /// <remarks>
        /// The target uses this delegate to compile a list of domains. It should wait
        /// until moreComing is false before updating the user interface, so as to
        /// avoid flickering.
        /// </remarks>
        public delegate void DomainRemoved(NetServiceBrowser browser, string domainName, bool moreComing);

        /// <summary>
        /// Occurs when a domain is no longer available.
        /// </summary>
        public event DomainRemoved DidRemoveDomain;

        private List<NetService> foundServices;
        private mDNSImports.DNSServiceBrowseReply browseReplyCb;
        private mDNSImports.DNSServiceDomainEnumReply domainSearchReplyCb;
        private GCHandle gchSelf;

        /// <summary>
        /// Initialize a new instance of the <see cref="NetServiceBrowser">NetServiceBrowser</see> class.
        /// </summary>
        public NetServiceBrowser()
        {
            foundServices = new List<NetService>();
        }

        /// <summary>
        /// Starts a search for services of a given type within a given domain.
        /// </summary>
        /// <param name="type">Type of service to search for.</param>
        /// <param name="domain">Domain name in which to search.</param>
        /// <remarks>
        /// The <I>domain</I> argument can be an explicity domain name, the
        /// generic "local." (including the trailing period) domain name or
        /// an empty string ("") which represents the default registration domain.
        /// </remarks>
        public void SearchForService(String type, String domain)
        {
            Stop();

            browseReplyCb = new mDNSImports.DNSServiceBrowseReply(BrowseReply);
            gchSelf = GCHandle.Alloc(this);

            DNSServiceErrorType err;
            err = mDNSImports.DNSServiceBrowse(out sdRef, 0, 0, type, domain, browseReplyCb, (IntPtr)gchSelf);

            if (err != DNSServiceErrorType.kDNSServiceErr_NoError)
            {
                throw new DNSServiceException("DNSServiceBrowse", err);
            }

            SetupWatchSocket();
        }

        private void SearchForDomains(DNSServiceFlags flags)
        {
            Stop();

            domainSearchReplyCb = new mDNSImports.DNSServiceDomainEnumReply(DomainSearchReply);
            gchSelf = GCHandle.Alloc(this);

            DNSServiceErrorType err;
            err = mDNSImports.DNSServiceEnumerateDomains(out sdRef, flags, 0, domainSearchReplyCb, (IntPtr)gchSelf);

            if (err != DNSServiceErrorType.kDNSServiceErr_NoError)
            {
                throw new DNSServiceException("DNSServiceEnumerateDomains", err);
            }

            SetupWatchSocket();
        }

        /// <summary>
        /// Starts a search for domains visible to the host.
        /// </summary>
        public void SearchForBrowseableDomains()
        {
            SearchForDomains(DNSServiceFlags.kDNSServiceFlagsBrowseDomains);
        }

        /// <summary>
        /// Starts a search for domains in which the host may register services.
        /// </summary>
        /// <remarks>
        /// Most clients do not need to use this method. It is normally sufficient
        /// to use the empty string ("") to registers a service in any available domain.
        /// </remarks>
        public void SearchForRegistrationDomains()
        {
            SearchForDomains(DNSServiceFlags.kDNSServiceFlagsRegistrationDomains);
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
            browseReplyCb = null; /* garbage collected */
            domainSearchReplyCb = null;

            if (gchSelf.IsAllocated)
            {
                gchSelf.Free();
            }
        }

        private static void BrowseReply(IntPtr sdRef,
            DNSServiceFlags flags,
            UInt32 interfaceIndex,
            DNSServiceErrorType errorCode,
            String serviceName,
            String regtype,
            String replyDomain,
            IntPtr context)
        {
            GCHandle gch = (GCHandle)context;
            NetServiceBrowser c = (NetServiceBrowser)gch.Target;

            bool moreComing = ((flags & DNSServiceFlags.kDNSServiceFlagsMoreComing) != 0);

            if ((flags & DNSServiceFlags.kDNSServiceFlagsAdd) != 0)
            { /* add */
                NetService newService = new NetService(replyDomain, regtype, serviceName);
                newService.InheritInvokeOptions(c);

                c.foundServices.Add(newService);

                if (c.DidFindService != null)
                    c.DidFindService(c, newService, moreComing);
            }
            else
            { /* remove */

                foreach (NetService service in c.foundServices)
                {
                    if (service.Name == serviceName &&
                        service.Type == regtype &&
                        service.Domain == replyDomain)
                    {
                        c.foundServices.Remove(service);
                        if (c.DidRemoveService != null)
                            c.DidRemoveService(c, service, moreComing);
                        break;
                    }
                }

            }
        }

        private static void DomainSearchReply(IntPtr sdRef,
            DNSServiceFlags flags,
            UInt32 interfaceIndex,
            DNSServiceErrorType errorCode,
            String replyDomain,
            IntPtr context)
        {
            GCHandle gch = (GCHandle)context;
            NetServiceBrowser c = (NetServiceBrowser)gch.Target;

            bool moreComing = ((flags & DNSServiceFlags.kDNSServiceFlagsMoreComing) != 0);

            if ((flags & DNSServiceFlags.kDNSServiceFlagsAdd) != 0)
            { /* add */
                if (c.DidFindDomain != null)
                    c.DidFindDomain(c, replyDomain, moreComing);
            }
            else
            { /* remove */
                if (c.DidRemoveDomain != null)
                    c.DidRemoveDomain(c, replyDomain, moreComing);
            }
        }

    } /* end class */
}
