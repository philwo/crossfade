using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace ZeroconfService
{
    enum DNSServiceFlags : uint
    {
        kDNSServiceFlagsMoreComing = 0x1,
        kDNSServiceFlagsAdd = 0x2,
        kDNSServiceFlagsDefault = 0x4,

        kDNSServiceFlagsBrowseDomains = 0x40,
        kDNSServiceFlagsRegistrationDomains = 0x80,
    }
    /// <summary>
    /// The error type used by the underlying dnssd.dll. These errors can
    /// be wrapped in <see cref="DNSServiceException">DNSServiceException</see> exceptions.
    /// </summary>
    public enum DNSServiceErrorType : int
    {
        kDNSServiceErr_NoError = 0,
        kDNSServiceErr_Unknown = -65537,       /* 0xFFFE FFFF */
        kDNSServiceErr_NoSuchName = -65538,
        kDNSServiceErr_NoMemory = -65539,
        kDNSServiceErr_BadParam = -65540,
        kDNSServiceErr_BadReference = -65541,
        kDNSServiceErr_BadState = -65542,
        kDNSServiceErr_BadFlags = -65543,
        kDNSServiceErr_Unsupported = -65544,
        kDNSServiceErr_NotInitialized = -65545,
        kDNSServiceErr_AlreadyRegistered = -65547,
        kDNSServiceErr_NameConflict = -65548,
        kDNSServiceErr_Invalid = -65549,
        kDNSServiceErr_Firewall = -65550,
        kDNSServiceErr_Incompatible = -65551,        /* client library incompatible with daemon */
        kDNSServiceErr_BadInterfaceIndex = -65552,
        kDNSServiceErr_Refused = -65553,
        kDNSServiceErr_NoSuchRecord = -65554,
        kDNSServiceErr_NoAuth = -65555,
        kDNSServiceErr_NoSuchKey = -65556,
        kDNSServiceErr_NATTraversal = -65557,
        kDNSServiceErr_DoubleNAT = -65558,
        kDNSServiceErr_BadTime = -65559
    }

    class mDNSImports
    {
        /* root loop stuff */
        [DllImport("dnssd.dll")]
        public static extern Int32 DNSServiceRefSockFD(IntPtr sdRef);

        [DllImport("dnssd.dll")]
        public static extern DNSServiceErrorType DNSServiceProcessResult(IntPtr sdRef);

        /* deallocate any DNSService */
        [DllImport("dnssd.dll")]
        public static extern void DNSServiceRefDeallocate(IntPtr sdRef);

        /* DNSService Discovery */
        public delegate void DNSServiceBrowseReply(IntPtr sdRef,
            DNSServiceFlags flags,
            UInt32 interfaceIndex,
            DNSServiceErrorType errorCode,
          [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))]String serviceName,
          [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))]String regtype,
          [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))]String replyDomain,
            IntPtr context);

        [DllImport("dnssd.dll")]
        public static extern DNSServiceErrorType DNSServiceBrowse(out IntPtr sdRef,
            DNSServiceFlags flags,
            UInt32 interfaceIndex,
           [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))]String regtype,
           [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))]String domain,
            DNSServiceBrowseReply callBack,
            IntPtr context);

        /* DNSService Resolving */
        public delegate void DNSServiceResolveReply(IntPtr sdRef,
            DNSServiceFlags flags,
            UInt32 interfaceIndex,
            DNSServiceErrorType errorCode,
           [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))]String fullname,
           [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))]String hosttarget,
            UInt16 port,
            UInt16 txtLen,
           [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 7)]byte[] txtRecord,
            IntPtr context);

        [DllImport("dnssd.dll")]
        public static extern DNSServiceErrorType DNSServiceResolve(out IntPtr sdRef,
            DNSServiceFlags flags,
            UInt32 interfaceIndex,
           [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))]String name,
           [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))]String regtype,
           [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))]String domain,
            DNSServiceResolveReply callBack,
            IntPtr context);

        /* Domain searching */

        public delegate void DNSServiceDomainEnumReply(
            IntPtr sdRef,
            DNSServiceFlags flags,
            UInt32 interfaceIndex,
            DNSServiceErrorType errorCode,
           [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))]String replyDomain,
            IntPtr context);

        [DllImport("dnssd.dll")]
        public static extern DNSServiceErrorType DNSServiceEnumerateDomains(
            out IntPtr sdRef,
            DNSServiceFlags flags,
            UInt32 interfaceIndex,
            DNSServiceDomainEnumReply callBack,
            IntPtr context);

        /* service registration */
        public delegate void DNSServiceRegisterReply(
            IntPtr sdRef,
            DNSServiceFlags flags,
            DNSServiceErrorType errorCode,
           [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))]String name,
           [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))]String regtype,
           [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))]String domain,
            IntPtr context);

        [DllImport("dnssd.dll")]
        public static extern DNSServiceErrorType DNSServiceRegister(
            out IntPtr sdRef,
            DNSServiceFlags flags,
            UInt32 interfaceIndex,
           [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))]String name,
           [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))]String regtype,
           [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))]String domain,
           [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))]String host,
            UInt16 port,
            UInt16 txtLen,
            //const void *txtRecord,    /* may be NULL */
            byte[] txtRecord,
            DNSServiceRegisterReply callBack,
            IntPtr context);

    }

    class UTF8Marshaler : ICustomMarshaler
    {
        private string cookie;
        private int nativeDataSize = 0;

        public UTF8Marshaler(string cookie)
        {
            this.cookie = cookie;
        }

        public Object MarshalNativeToManaged(IntPtr pNativeData)
        {
            if (pNativeData == IntPtr.Zero) return null;

            List<byte> bytes = new List<byte>();
            byte readbyte;
            int i = 0;
            while ((readbyte = Marshal.ReadByte(pNativeData, i)) != 0)
            {
                bytes.Add(readbyte);
                i++;
            }

            byte[] utf8bytes = bytes.ToArray();

            Encoding u8e = Encoding.UTF8;
            
            return u8e.GetString(utf8bytes);
        }

        public IntPtr MarshalManagedToNative(Object managedObject)
        {
            String inString = (String)managedObject;

            if (inString == null) return IntPtr.Zero;

            Encoding u8e = Encoding.UTF8;
            byte[] utf8bytes = u8e.GetBytes(inString);

            IntPtr ptr = Marshal.AllocHGlobal(utf8bytes.Length + 1);

            for (int i = 0; i < utf8bytes.Length; i++)
                Marshal.WriteByte(ptr, i, utf8bytes[i]);
            Marshal.WriteByte(ptr, utf8bytes.Length, 0);

            nativeDataSize = utf8bytes.Length + 1;

            return ptr;
        }

        public int GetNativeDataSize()
        {
            return nativeDataSize;
        }

        public void CleanUpManagedData(Object managedObject)
        {
        }

        public void CleanUpNativeData(IntPtr pNativeData)
        {
            if (pNativeData == IntPtr.Zero) return;
            Marshal.FreeHGlobal(pNativeData);
        }

        public static ICustomMarshaler GetInstance(String cookie)
        {
            return new UTF8Marshaler(cookie);
        }
    }
}
