using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;

using System.Net.Sockets;

namespace ZeroconfService
{
    /// <summary>
    /// An exception that is thrown when a <see cref="NetService">NetService</see>
    /// or <see cref="NetServiceBrowser">NetServiceBrowser</see> dll error occurs.
    /// </summary>
    public class DNSServiceException : Exception
    {
        string s = null;
        string f = null;
        DNSServiceErrorType e = DNSServiceErrorType.kDNSServiceErr_NoError;

        internal DNSServiceException(string s)
        {
            this.s = s;
        }

        internal DNSServiceException(string function, DNSServiceErrorType error)
        {
            e = error;
            f = function;
            s = String.Format("An error occured in the function '{0}': {1}",
                function, error);
        }

        /// <summary>
        /// Creates a returns a string representation of the current exception
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return s;
        }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        public override string Message
        {
            get { return s; }
        }

        /// <summary>
        /// Gets the function name (if possible) that returned the underlying error
        /// </summary>
        public string Function { get { return f; } }
        /// <summary>
        /// Gets the <see cref="DNSServiceErrorType">DNSServiceErrorType</see> error
        /// that was returned by the underlying function.
        /// </summary>
        public DNSServiceErrorType ErrorType { get { return e; } }
    }

    /// <summary>
    /// The base class used by the <see cref="NetServiceBrowser">NetServiceBrowser</see>
    /// and <see cref="NetService">NetService</see> classes. This class primarily
    /// abstracts the asynchronous functionality of its derived classes.
    /// </summary>
    /// <remarks>
    /// It should not be necessary to derive from this class directly.
    /// </remarks>
    public abstract class DNSService
    {
        private UnixSocket socket;
        /// <summary>
        /// Pointer to the internal DNSService object
        /// </summary>
        protected IntPtr sdRef;
        /// <summary>
        /// True when we are attempting to stop the current asynchronous task.
        /// </summary>
        protected bool stopping = false;

        private bool inPoll = false;
        private EventWaitHandle stoppingPoll = new EventWaitHandle(false, EventResetMode.AutoReset);

        private void PollInvokeable()
        {
            try
            {
                mDNSImports.DNSServiceProcessResult(sdRef);
            }
            catch (Exception e)
            {
                Console.WriteLine("Got an exception on DNSServiceProcessResult (Unamanaged, so via user callback?)\n{0}{1}", e, e.StackTrace);
            }
        }

        private bool mAllowApplicationForms = true;
        /// <summary>
        /// Allows the application to attempt to post async replies over the
        /// application "main loop" by using the message queue of the first available
        /// open form (window). This is retrieved through
        /// <see cref="System.Windows.Forms.Application.OpenForms">Application.OpenForms</see>.
        /// </summary>
        public bool AllowApplicationForms
        {
            get { return mAllowApplicationForms; }
            set { mAllowApplicationForms = value; }
        }

        System.ComponentModel.ISynchronizeInvoke mInvokeableObject = null;
        /// <summary>
        /// Set the <see cref="System.ComponentModel.ISynchronizeInvoke">ISynchronizeInvoke</see>
        /// object to use as the invoke object. When returning results from asynchronous calls,
        /// the Invoke method on this object will be called to pass the results back
        /// in a thread safe manner.
        /// </summary>
        /// <remarks>
        /// This is the recommended way of using the DNSService class. It is recommended
        /// that you pass your main <see cref="System.Windows.Forms.Form">form</see> (window) in.
        /// </remarks>
        public System.ComponentModel.ISynchronizeInvoke InvokeableObject
        {
            get { return mInvokeableObject; }
            set { mInvokeableObject = value; }
        }

        private bool mAllowMultithreadedCallbacks = false;
        /// <summary>
        /// If this is set to true, <see cref="AllowApplicationForms">AllowApplicationForms</see>
        /// is set to false and <see cref="InvokeableObject">InvokeableObject</see> is set
        /// to null, any time an asynchronous method needs to invoke a method in the
        /// main loop, it will instead run the method in its own thread.
        /// </summary>
        /// <remarks>
        /// <para>The thread safety of this property depends on the thread safety of
        /// the underlying dnssd.dll functions. Although it is not recommended, there
        /// are no known problems with this library using this method.
        /// </para>
        /// <para>
        /// However, if your application uses Windows.Forms or any other non-thread safe
        /// library, you will have to do your own invoking.
        /// </para>
        /// </remarks>
        public bool AllowMultithreadedCallbacks
        {
            get { return mAllowMultithreadedCallbacks; }
            set { mAllowMultithreadedCallbacks = value; }
        }

        internal void InheritInvokeOptions(DNSService fromService)
        {
            AllowApplicationForms = fromService.AllowApplicationForms;
            InvokeableObject = fromService.InvokeableObject;
            AllowMultithreadedCallbacks = fromService.AllowMultithreadedCallbacks;
        }

        private System.ComponentModel.ISynchronizeInvoke GetInvokeObject()
        {
            if (mInvokeableObject != null) return mInvokeableObject;

            if (mAllowApplicationForms)
            {
                /* need to post it to self over control thread */
                FormCollection forms = System.Windows.Forms.Application.OpenForms;

                if (forms != null && forms.Count > 0)
                {
                    Control control = forms[0];
                    return control;
                }
            }
            return null;
        }

        /// <summary>
        /// Calls a method using the objects invokable object.
        /// </summary>
        /// <param name="method">The method to call.</param>
        /// <param name="args">The arguments to call the object with.</param>
        /// <returns>The result returned from method, or null if the method
        /// could not be invoked.</returns>
        protected object Invoke(Delegate method, params object[] args)
        {
            System.ComponentModel.ISynchronizeInvoke invokeable;
            
            invokeable = GetInvokeObject();

            if (invokeable != null)
            {
                return invokeable.Invoke(method, args);
            }

            if (mAllowMultithreadedCallbacks)
            {
                method.DynamicInvoke(args);
            }

            return null;
        }

        private void AsyncPollCallback(IAsyncResult result)
        {
            bool ret = socket.EndPoll(result);

            if (stopping)
            {
                inPoll = false;
                stoppingPoll.Set();
                return; /* if we're stopping, don't begin a new poll */
            }

            if (ret)
            {
                MethodInvoker cb = new MethodInvoker(PollInvokeable);

                Invoke(cb, null);
            }

            inPoll = true;
            AsyncCallback callback = new AsyncCallback(AsyncPollCallback);
            socket.BeginPoll(-1, SelectMode.SelectRead, callback);
        }

        /// <summary>
        /// Tries, and waits, for the socket to stop polling
        /// </summary>
        protected void WaitStop()
        {
            if (inPoll)
            {
                stopping = true;
                socket.Stop();
                stoppingPoll.WaitOne();
            }
        }

        /// <summary>
        /// Starts polling the DNSService socket, and delegates
        /// data back to the primary DNSService API when data arrives
        /// on the socket.
        /// </summary>
        protected void SetupWatchSocket()
        {
            Int32 socketId = mDNSImports.DNSServiceRefSockFD(sdRef);
            socket = new UnixSocket(socketId);

            AsyncCallback callback = new AsyncCallback(AsyncPollCallback);

            stopping = false;
            inPoll = true;
            IAsyncResult ar = socket.BeginPoll(-1, SelectMode.SelectRead, callback);
        }

    }
}
