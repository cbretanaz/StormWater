using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security.Principal;

namespace CoP.Enterprise
{
    public enum ImpersonatorLogonType
    {
        /// <summary>
        /// This logon type is intended for users who will be interactively using the computer, such as a user being logged on  
        /// by a terminal server, remote shell, or similar process.
        /// This logon type has the additional expense of caching logon information for disconnected operations; 
        /// therefore, it is inappropriate for some client/server applications,
        /// such as a mail server.
        /// </summary>
        InteractiveLogon = 2,

        /// <summary>
        /// This logon type is intended for high performance servers to authenticate plaintext passwords.
        /// The LogonUser function does not cache credentials for this logon type.
        /// </summary>
        NetworkLogon = 3,

        /// <summary>
        /// This logon type is intended for batch servers, where processes may be executing on behalf of a user without 
        /// their direct intervention. This type is also for higher performance servers that process many plaintext
        /// authentication attempts at a time, such as mail or Web servers. 
        /// The LogonUser function does not cache credentials for this logon type.
        /// </summary>
        BatchLogon = 4,

        /// <summary>
        /// Indicates a service-type logon. The account provided must have the service privilege enabled. 
        /// </summary>
        ServiceLogon = 5,

        /// <summary>
        /// This logon type is for GINA DLLs that log on users who will be interactively using the computer. 
        /// This logon type can generate a unique audit record that shows when the workstation was unlocked. 
        /// </summary>
        LogonUnlock = 7,

        /// <summary>
        /// This logon type preserves the name and password in the authentication package, which allows the server to make 
        /// connections to other network servers while impersonating the client. A server can accept plaintext credentials 
        /// from a client, call LogonUser, verify that the user can access the system across the network, and still 
        /// communicate with other servers.
        /// </summary>
        NetworkCleartextLogon = 8,

        /// <summary>
        /// This logon type allows the caller to clone its current token and specify new credentials for outbound connections.
        /// The new logon session has the same local identifier but uses different credentials for other network connections. 
        /// </summary>
        NewCredentialsLogon = 9,
    }

    [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
    public class Impersonator: IDisposable
    {
        #region Declarations
        private readonly string username;
        private readonly string password;
        private readonly string domain;
        private readonly ImpersonatorLogonType logonTyp;
        // this will hold the security context 
        // for reverting back to the client after
        // impersonation operations are complete
        private WindowsImpersonationContext impersonationContext;
        #endregion Declarations

        #region Constructors
        public Impersonator(string UserName, 
            string Domain, string Password,
            ImpersonatorLogonType logonType 
            = ImpersonatorLogonType.InteractiveLogon)
        {
            username = UserName;
            domain = Domain;
            password = Password;
            logonTyp = logonType;
        }
        #endregion Constructors

        #region Public Methods
        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public static Impersonator Impersonate(
            string userName, string domain, string password,
            ImpersonatorLogonType logonType = ImpersonatorLogonType.InteractiveLogon)
        {
            var imp = new Impersonator(userName, domain, password, logonType);
            imp.Impersonate(imp.logonTyp);
            return imp;
        }
        public void Impersonate(ImpersonatorLogonType logonType = ImpersonatorLogonType.InteractiveLogon)
        { impersonationContext = Logon(logonType).Impersonate(); }
        public void Undo() { impersonationContext.Undo(); }
        #endregion Public Methods

        #region Private Methods
        private WindowsIdentity Logon(ImpersonatorLogonType logonType)
        {
            var handle = IntPtr.Zero;

            const int LOGON32_PROVIDER_DEFAULT = 0;
            const int LOGON32_LOGON_NEW_CREDENTIALS = 9;
            const int LOGON32_PROVIDER_WINNT50 = 5;
            const int SecurityImpersonation = 2;

            // attempt to authenticate domain user account
            try
            {
                if (!LogonUser(username, domain,
                    password, (int)logonType, 
                    LOGON32_PROVIDER_DEFAULT, ref handle))
                    throw new LogonException(
                        "User logon failed. GetLastWin32Error Number: " +
                        Marshal.GetLastWin32Error());

                // ----------------------------------
                var dupHandle = IntPtr.Zero;
                if (!DuplicateToken(handle,
                    SecurityImpersonation,
                    ref dupHandle))
                    throw new LogonException(
                        "Logon failed attemting to duplicate handle");
                // Logon Succeeded ! return new WindowsIdentity instance
                return (new WindowsIdentity(handle));
            }
            // close the open handle to the authenticated account
            finally { CloseHandle(handle); }
        }

        #region external Win32 API functions
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool
            LogonUser(string lpszUsername, string lpszDomain,
                    string lpszPassword, int dwLogonType,
                    int dwLogonProvider, ref IntPtr phToken);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool CloseHandle(IntPtr handle);
        // --------------------------------------------

        [DllImport("advapi32.dll", CharSet = CharSet.Auto,
             SetLastError = true)]
        public static extern bool DuplicateToken(
            IntPtr ExistingTokenHandle,
            int SECURITY_IMPERSONATION_LEVEL,
            ref IntPtr DuplicateTokenHandle);
        // --------------------------------------------
        #endregion external Win32 API functions
        #endregion Private Methods

        #region IDisposable
        private bool disposed;
        public void Dispose() { Dispose(true); }
        public void Dispose(bool isDisposing)
        {
            if (disposed) return;
            if (isDisposing) Undo();
            // -----------------
            disposed = true;
            GC.SuppressFinalize(this);
        }
        ~Impersonator() { Dispose(false); }

        #endregion IDisposable
    }
}
