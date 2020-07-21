using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Principal;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Threading;
using System.Transactions;
using System.Web;
using System.Web.Hosting;
using CfgMgr = System.Configuration.ConfigurationManager;
using System.Security.Cryptography;
using System.Reflection;
using System.Windows.Forms;

namespace CoP.Enterprise
{
    public enum APPENV
    {
        NA,                                    // Not Applicable 
        LOCAL, LOCALHOME, TESTHOME,            // for use with LocalHost servers
        LOCALWORK, LOCALWORK1, LOCALWORK2,     // LocalHost with various Users
        LOCALWORK3, LOCALWORK4, LOCALWORK5,    // LocalHost with various Users
        DEV, DEV1, DEV2, DEV3, DEV4,           // Development Environments
        REF, REF1, REF2, REF3, REF4,           // Refactor Environments
        UNITTEST, UNITTEST1, UNITTEST2,        // Unit Test Environments
        UT, UT1, UT2, UT3, UT4, UT5,           // Unit Test Environments
        HOME, HOME0, HOME1, HOME2, HOME3,      // My own machine at home                               
        CHECK, CHECK0, CHECK1, CHECK2, CHECK3, // My wkstn at work
        DEPLOY, DEPLOY0, DEPLOY1, DEPLOY2,     // for dev/testing of Deployment scripts
        QA, QA0, QA1, QA2, QA3, QA4, QA5, QA6, // QA test environments
        TEST, TEST0, TEST1, TEST2, TEST3,      // Test enviroments for deployment testing
        CEX, CEX0, CEX1, CEX2, CEX3, CEX4,     // Customer Exposure Environments
        UAT, UAT0, UAT1, UAT2, UAT3, UAT4,     // User Acceptance Testing
        CT, CT0, CT1, CT2, CT3, CT4, CT5,      // Client (user) acceptance test, User Training
        STG, STG0, STG1, STG2, STG3, STG4,     // Staging environment
        ITE, ITE0, ITE1, ITE2, ITE3, ITE4,     // Integrated Test Environment 012...
        HF, HF0, HF1, HF2, HF3, HF4,           // Hot Fix environments
        PROD, PROD0, PROD1, PROD2,             // Production (Live) environments 
        ADC, ADC1, CODE                        // Code database
        // Alternate Data Center (Production) environments...
    }

    /// <summary>
    /// Collection of general and Enterprise specific static functions
    ///  for use throughout Enterprise applications
    /// </summary>
    public static class Utilities
    {
        private const string COMPANY = "CoP";  // Default Company Name		
        private const StringComparison icic
             = StringComparison.InvariantCultureIgnoreCase;

        private static readonly string sNL = Environment.NewLine;

        #region AppEnvironmentMode
        /// <summary>
        /// Configuration setting that specifies the current application environment
        /// </summary>
        public static string APPENVIRONMENTSTR => 
            !string.IsNullOrEmpty(CfgMgr.AppSettings["environment"]) ? 
            CfgMgr.AppSettings["environment"] :
            !string.IsNullOrEmpty(CfgMgr.AppSettings[COMPANY + "_Environment"]) ? 
                CfgMgr.AppSettings["CoPEnvironment"] :
                null;
        /// <summary>
        /// Current Application Environment this code is running in
        /// </summary>
        public static APPENV APPENVIRONMENT => GetAppMode(APPENVIRONMENTSTR);
        /* **************************************************
             * You can add the following in either your web.config,  
             * <ApplicationName>.config, or machine.config, 
             * depending on how global these Database connections need to be. 
             * Whichever config file you choose, it will contain an Xml Element named <configuration>,
             *   and that will contain an Xml Element named <configSections>
             Inside of the <configSections> element, add the following Xml snippet... 
          
            <sectionGroup name="YourCompanyName">
              <section name="YourApplicationName"
                       type="System.Configuration.DictionarySectionHandler, system, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b77a5c561934e089, Custom=null" />
            </sectionGroup>
          
           As an example, ...
           
            <sectionGroup name="CoP">
              <section name="MDMR"
                       type="System.Configuration.DictionarySectionHandler, system, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b77a5c561934e089, Custom=null" />
            </sectionGroup>

            
           Then later on, inside the <configuration> element (at the same level as 
               <configSections>), add the following... 

                <YourCompanyName>
                    <YourApplicationName>
                        <add key="PROD_DBVendor" value="SQLServer" />
                        <add key="PROD_Server" value="serverName" />
                        <add key="PROD_Database" value="databaseName" />
                        <add key="PROD_UserID" value="logon" />
                        <add key="PROD_Password" value="Z0sl+pVR8CU=" />

                        <add key="QA_DBVendor" value="Oracle" />
                        <add key="QA_DataSource" value="qtmr" />
                        <add key="QA_UserID" value="logon" />
                        <add key="QA_Password" value="s8CZ+0lpVRU=" />
                    
                    </YourApplicationName>
                </YourCompanyName>
                
             ...where the keynames are concatenations of the AppMode (PROD, DEV, DEV2, QA, etc., as from below) 
                Plus an underscore, plus the string "Server", "Database", "UserID", and "Password", respectively.
                Add a set of 4 keys for each AppMode (database connection) you want to be available to the application.
                (Only add the "Server" and "Database" keys for connections which should use integrated security.

            
            ======================================================================================================

            Another example, for Encryption Keys and Initial Vectors, using configSource attribute
            1.  In app config, <configSections> element, add 
                <sectionGroup name="CityOfPortland">
                    <section name="CoPEncryption"
                       type="System.Configuration.DictionarySectionHandler, system, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b77a5c561934e089, Custom=null" />
                </sectionGroup>

            then later inside <configuration> element, add
                <CoPEncryption configSource="Config\CoPEncryptionSettings.config" />

            and create and store this file in config subdirectory
            <CityOfPortland>
                <CoPEncryption>
                    <add key="PROD_KEY" value="N0w 1$ th3 t1m3 4 all g00d M3n T0 c0m3 to th3 a1d 0f th31r c0uNtry!"/>
                    <add key="PROD_IV" value="@HTMF,G$U,G$AG!(3184-12)z"/>
                </CoPEncryption>
            </CityOfPortland>

            
            
            *********************************************************************************************************/

        /// <summary>
        /// Converts  string Environment setting to APPENV Enum value 
        /// </summary>
        /// <param name="sAppMode">Application environment string</param>
        /// <returns>APPENV Enum for specified Application environment string</returns>
        public static APPENV GetAppMode(string sAppMode)
        { return EnumTryParse(sAppMode, out APPENV appEnv)? appEnv : APPENV.NA; }
        /// <summary>
        /// Converts APPENV Enum to string
        /// </summary>
        /// <param name="env">APPENV Enum value</param>
        /// <returns>Application environment string for specified APPENV Enum value</returns>
        public static string GetAppMode(APPENV env) { return (env.ToString()); }

        #endregion AppMode

        #region NT Logging code
        /// <summary>
        /// Function to add Log entry to event log
        /// </summary>
        /// <param name="msg">Message to post in event log</param>
        /// <param name="severity">Severity of event - 
        /// member of EventLogEntryType</param>
        /// <param name="source">Source of event, 
        /// normally application name</param>
        /// <param name="sLog">Which Event Log to write to</param>
        /// <param name="eventID">ID of this event - 
        /// normally defined by Application</param>
        /// <param name="categoryID">Category of event</param>
        public static void NTLog(string msg,
            EventLogEntryType severity,
            string source, string sLog,
            int eventID, short categoryID = 0)
        {
            // Create the source, if it does not already exist.
            if (source.Length == 0)
            {
                source = "Not Specified";
                sLog = "";
            }
            else if (sLog.Length == 0) sLog = source + "Log";

            try
            {
                if (EventLog.SourceExists(source))
                {
                    var sSourceLogName = EventLog.LogNameFromSourceName(
                        source, Environment.MachineName);
                    if (!String.Equals(sSourceLogName, sLog, icic))
                    {
                        EventLog.DeleteEventSource(source, Environment.MachineName);
                        EventLog.CreateEventSource(source, sLog);
                    }
                }
                else EventLog.CreateEventSource(source, sLog);
            }
            catch (SecurityException) {/*Ignore*/ return; }
            // Create an EventLog instance and assign its source.
            var mLog = new EventLog(sLog, Environment.MachineName, source)
                        {Source = source};
            // ********* Write entry to the event log. **********   
            mLog.WriteEntry(msg, severity, eventID, categoryID);
        }

        /// <summary>
        /// Function to add Log entry to event log
        /// </summary>
        /// <param name="msg">Message to post in event log</param>
        /// <param name="severity">Severity of event - 
        /// member of EventLogEntryType</param>
        /// <param name="source">Source of event, 
        /// normally application name</param>
        /// <param name="eventID">ID of this event - 
        /// normally defined by Application</param>
        /// <param name="categoryID">Category of event</param>
        public static void NTLog(string msg,
            EventLogEntryType severity,
            string source, int eventID,
            short categoryID)
        {
            var sLog = "";
            // Create the source, if it does not already exist.
            if (source.Length == 0)
                source = "Not Specified";
            else
                sLog = source + "Log";
            NTLog(msg, severity, source,
                sLog, eventID, categoryID);
        }

        /// <summary>
        /// Function to add Log entry to event log
        /// </summary>
        /// <param name="msg">Message to post in event log</param>
        /// <param name="severity">Severity of event - 
        /// member of EventLogEntryType</param>
        /// <param name="source">Source of event, 
        /// normally application name</param>
        /// <param name="eventID">ID of this event - 
        /// normally defined by Application</param>
        public static void NTLog(string msg,
            EventLogEntryType severity,
            string source, int eventID)
        {
            var sLog = string.Empty;
            // Create the source, if it does not already exist.
            if (source.Length == 0)
                source = "Not Specified";
            else
                sLog = source + "Log";
            NTLog(msg, severity, source, sLog, eventID, 0);
        }

        /// <summary>
        /// Function to add Log entry to event log
        /// </summary>
        /// <param name="msg">Message to post in event log</param>
        /// <param name="severity">Severity of event - 
        /// member of EventLogEntryType</param>
        /// <param name="source">Source of event, 
        /// normally application name</param>
        public static void NTLog(string msg,
            EventLogEntryType severity,
            string source)
        {
            var sLog = string.Empty;
            // Create the source, if it does not already exist.
            if (source.Length == 0)
                source = "Not Specified";
            else
                sLog = source + "Log";
            NTLog(msg, severity, source, sLog, 0);
        }
        #endregion

        #region NT Security: Logon, AD functions, etc
        #region Direct OS LogonUser Code
        [DllImport("advapi32.dll")]
        private static extern bool LogonUser(String lpszUsername,
            string lpszDomain, string lpszPassword, int dwLogonType,
            int dwLogonProvider, out int phToken);

        [DllImport("Kernel32.dll")]
        private static extern int GetLastError();

        [Obsolete]
        public static bool LogOnXP(String sDomain, String sUser, String sPassword)
        {
            var attmpts = 0;

            while (attmpts < 2)
            {
                int token1;
                if (LogonUser(sUser, sDomain, sPassword, 3, 0, out token1))
                    return (true);
                int ret;
                switch (ret = GetLastError())
                {
                    case (126):
                        if (attmpts++ > 2)
                            throw new LogonException(
                                "Specified module could not be found. error code: " + ret);
                        break;

                    case (1314):
                        throw new LogonException(
                            "Specified module could not be found. error code: " + ret);

                    case (1326):
                        throw new LogonException(
                            "Unknown user name or bad password.");

                    default:
                        throw new LogonException(
                            "Unexpected Logon Failure. Contact Administrator");
                }
            }
            return (false);
        }
        #endregion Direct Logon Code
        private static string CurrentUserContainer => UserContainer(Environment.UserName);
        private static string UserContainer(string userName)
        {
            using (var rootEntry = new DirectoryEntry(
                string.Format("LDAP://{0}", Environment.UserDomainName),
                null, null, AuthenticationTypes.Secure))
            using (var directorySearcher =
                new DirectorySearcher(rootEntry,
                    string.Format("(sAMAccountName={0})", userName)))
            {
                var sr = directorySearcher.FindOne();
                if (sr == null) return null;
                using (var userEntry = sr.GetDirectoryEntry())
                {
                    var dn = (string)userEntry.Properties["distinguishedName"].Value;
                    return dn.Substring(1 + dn.IndexOf(","));
                }
            }
        }
        /// <summary>
        /// Current User Information from Security System
        /// </summary>
        public static UserInfo CurrentUserInfo => 
            UserInfo.From(UserPrincipal.FindByIdentity(
            new PrincipalContext(ContextType.Domain,
                Environment.UserDomainName, CurrentUserContainer),
            IdentityType.SamAccountName, Environment.UserName));
        /// <summary>
        /// Security System Information for specified user 
        /// </summary>
        /// <param name="userName"></param>
        /// <returns>UserInfo</returns>
        public static UserInfo GetUserInfo(string userName)
        {
            if (userName.Contains(@"\"))
                userName = userName.Split('\\')[1];
            var ctx = new PrincipalContext(
                ContextType.Domain, Environment.UserDomainName, 
                UserContainer(userName));
            return UserInfo.From(
                UserPrincipal.FindByIdentity(
                    ctx, userName));
        }
        #endregion NT Security: Logon, AD functions, etc

        #region TryMethods
        /// <summary>
        /// Attempt to execute a method (function)
        /// </summary>
        /// <param name="target">for non-static methods, a reference to an object
        /// (an instance of class) that the method should be called on.
        /// (not required for static methods)</param>
        /// <param name="mi">MethodInfo object for Method to attempt</param>
        /// <param name="args">Array of arguments to pass to method</param>
        /// <param name="maxTrys">Number of times to attempt the specified Method</param>
        /// <returns>Sucessful return value of method</returns>
        public static object TryMethod(object target,
            MethodInfo mi, object[] args, int maxTrys)
        {
            return TryMethod(target, mi, args,
                typeof(Exception), maxTrys);
        }
        /// <summary>
        /// Attempt to execute a method (function) that has a  return value 
        /// </summary>
        /// <param name="target">for non-static methods, a reference to an object
        /// (an instance of class) that the method should be called on.
        /// (not required for static methods)</param>
        /// <param name="mi">MethodInfo object for Method to attempt</param>
        /// <param name="args">Array of arguments to pass to method</param>
        /// <param name="exceptionType">Exceptions to trap</param>
        /// <param name="maxTrys">Number of times to attempt the specified Method</param>
        /// <returns>Sucessful return value of method</returns>
        public static object TryMethod(object target,
            MethodInfo mi, object[] args,
            Type exceptionType, int maxTrys)
        {
            while (true)
            {
                try { return (mi.Invoke(target, args)); }
                catch (Exception eX)
                {
                    if (!exceptionType.IsInstanceOfType(eX)
                        || --maxTrys == 0) throw;
                }
            }
        }
        #endregion TryMethods

        #region HardLink/SymbolicLink Code
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool
            CreateHardLink(
                string newFileSpec,
                string existingFileSpec, 
                IntPtr unused);

        private enum SymbolicLink { File=0, Directory = 1}
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CreateSymbolicLink(string linkName, string tgtName, SymbolicLink flags);

        /// <summary>
        /// Create a "HardLink" to an existing file system file object
        /// </summary>
        /// <param name="newFileSpec"></param>
        /// <param name="existingFileSpec"></param>
        public static void CreateHardLink(string newFileSpec, string existingFileSpec)
        {
            if (!CreateHardLink(newFileSpec, existingFileSpec, IntPtr.Zero))
                throw new CoPException(
                    $"Could not create hardLink: [{newFileSpec}] for file: [{existingFileSpec}],{sNL}Windows Error Code=" +
                    Marshal.GetLastWin32Error());
        }

        public static void CreateFilSymLink(string linkName, string tgtName)
        {
           if(!CreateSymbolicLink(linkName, tgtName, SymbolicLink.File))
               throw new CoPInvalidOperationException(
                   $"Cannot create Sym Link for {tgtName}.");
        }
        // ----------------------------
        private struct HFILEINFO
        {
            public uint nLinks { get; set; }
            public uint dwAttributes { get; set; }
            public uint ftCreateU { get; set; }
            public uint ftCreateL { get; set; }
            public uint ftAccessU { get; set; }
            public uint ftAccessL { get; set; }
            public uint ftWriteU { get; set; }
            public uint ftWriteL { get; set; }
            public uint vsn { get; set; }
            public uint nSizeHigh { get; set; }
            public uint nSizeLow { get; set; }
            public uint nFileIndexHigh { get; set; }
            public uint nFileIndexLow { get; set; }
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint
            GetFileInformationByHandle(IntPtr handle, out HFILEINFO fi);

        private static HFILEINFO GetFileInfo(string path)
        {
            try
            {
                HFILEINFO fi; // = new HFILEINFO();
                using (FileStream fs = File.Open(path, FileMode.Open,
                                FileAccess.Read, FileShare.ReadWrite))
                    if (GetFileInformationByHandle((fs.SafeFileHandle.DangerousGetHandle()), out fi) == 0)
                        throw new CoPException("Could not retrieve File Info," +
                                "Windows code=" + Marshal.GetLastWin32Error());
                return fi;
            }
            catch (IOException ioX)
            {
                throw new CoPException("Could not open File " +
                        path + Environment.NewLine +
                        "Error: " + ioX.Message, ioX);
            }
        }
        public static int HardLinkCount(string fileSpec)
        {
            var hFi = GetFileInfo(fileSpec);
            return ((int)hFi.nLinks);
        }
        #endregion HardLink Code

        #region Directory/File utilities
        /// <summary>
        /// Create Directory if it does not exist
        /// </summary>
        /// <param name="pathSpecification"></param>
        public static void ValidateDirectory(string pathSpecification)
        {
            Directory.SetCurrentDirectory(ApplicationPath);
            try
            {
                if (!Directory.Exists(pathSpecification))
                    Directory.CreateDirectory(pathSpecification);
            }
            catch (DirectoryNotFoundException dnfX)
            {
                throw new CoPIOException(
                    $"Cannot find or create Folder: {pathSpecification}.", dnfX);
            }
            catch (UnauthorizedAccessException uaX)
            {
                var wId = WindowsIdentity.GetCurrent();
                throw new CoPIOException(string.Format(
                  "Cannot find or create Folder: {0}, " +
                  "{1} does not have sufficient user authority.",
                  pathSpecification,
                  (wId == null ? "Current User" : wId.Name)), uaX);
            }
            catch (PathTooLongException tlX)
            {
                throw new CoPIOException(string.Format(
                    "Cannot create Folder: {0}, Path " +
                    "Specification exceeds 248 characters.",
                    pathSpecification), tlX);
            }
            catch (IOException ioX)
            {
                throw new CoPIOException(string.Format(
                  "Cannot create Folder: {0}, " +
                  "This is a read only directory.",
                  pathSpecification), ioX);
            }
            catch (ArgumentNullException anX)
            {
                throw new CoPIOException(string.Format(
                    "Cannot create Folder: {0}, Invalid directory.",
                    pathSpecification), anX);
            }
            catch (ArgumentException aX)
            {
                throw new CoPIOException(string.Format(
                  "Cannot create Folder: {0}, " +
                  "Invalid directory.",
                  pathSpecification), aX);
            }
            catch (NotSupportedException nsX)
            {
                throw new CoPIOException(string.Format(
                    "Cannot create Folder: {0}, " +
                    "Specifiied Path contains invalid colon character.",
                    pathSpecification), nsX);
            }
        }
        /// <summary>
        /// Generates a filespec one higher (sequentially) than specified fileSpec
        /// </summary>
        /// <param name="filSpec">existing file specificationm (with path)</param>
        /// <returns>incremented file specification</returns>
        public static string IncrementFileSpec(string filSpec)
        {
            var parts = ExtractFileName(filSpec).Split('.');
            var path = ExtractPath(filSpec);
            var outVal = string.IsNullOrWhiteSpace(path)? 
                            string.Empty: path + @"\";
            var foundDate = false;
            for (var p=0; p<parts.Length-1; p++)
            {
                outVal += parts[p] + ".";
                if (foundDate || !IsDate(parts[p])) continue;
                // -----------------------------------------
                foundDate = true;
                if (int.TryParse(parts[p + 1], out var j))
                {
                    outVal += ++j + ".";
                    p++;
                }
                else outVal += "1.";
            }
            outVal += parts[parts.Length - 1];
            return outVal;
        }
        #endregion Directory/File utilities

        #region Misc Code
        /// <summary>
        /// Name of Current executing Assembly 
        /// </summary>
        public static string ASSEMBLYNAME => 
            Assembly.GetExecutingAssembly()
                .CodeBase.Replace("file:///", string.Empty)
                .Replace('/', '\\');
        /// <summary>
        /// Folder (path specification) of Current executing Assembly.
        /// </summary>
        public static readonly string ASSEMBLYFOLDER =
                ASSEMBLYNAME.Remove(ASSEMBLYNAME.LastIndexOf(@"\", icic) + 1);
        /// <summary>
        /// Data Directory to be used to store application data files, 
        /// as specified in Configuration file AppSettings section
        /// </summary>
        public static readonly string DATAFOLDER = CfgMgr.AppSettings["DataDirectory"] ;
        /// <summary>
        /// Validates that specified assmebly (file Specification) exitss on disk
        /// </summary>
        /// <param name="assemblyFileSpec">Fully qualified file specification of assembly to check</param>
        /// <exception cref="CoPException"></exception>
        public static void CheckAssemblyExists(string assemblyFileSpec)
        {
            if (!File.Exists(assemblyFileSpec))
                throw new CoPException(
                    $"Assembly [{assemblyFileSpec}] cannot be located.");
        }
        /// <summary>
        /// Resolves string folder specification (absolute or relative) according to application rules
        /// </summary>
        /// <param name="folderName">folder specification to resolve</param>
        /// <returns>Resolved folder specification</returns>
        public static string ResolveFolder(string folderName)
        {
            if (folderName.StartsWith(@"\\")) return folderName;
            // -------------------------------------------------
            var driveLtr = folderName.Substring(0, 1).ToUpper()[0];
            if (driveLtr >= 'A' && driveLtr <= 'Z' &&
                string.Equals(folderName.Substring(1, 2), @":\", icic))
                return folderName;
            // -------------------------------------------------
            return ASSEMBLYFOLDER + folderName;
        }

        /// <summary>
        /// Resolves string data folder specification (absolute or relative) according to application rules
        /// </summary>
        /// <param name="folderName">folder specification to resolve</param>
        /// <returns>Resolved data folder specification</returns>
        public static string ResolveDataFolder(string folderName)
        {
            if(IsAbsoluteFolder(folderName)) return folderName;
            // -------------------------------------------------------
            const string backSlsh = @"\";
            var dataFolder = DATAFOLDER.Substring(0, 
                    DATAFOLDER.Length - (DATAFOLDER.EndsWith(backSlsh) ? 1 : 0));
            var fldrName = folderName.Substring(folderName.StartsWith(backSlsh)? 1: 0);
            fldrName = fldrName.Substring(0, fldrName.Length - (fldrName.EndsWith(backSlsh) ? 1 : 0));
            return dataFolder + backSlsh + fldrName;
        }
        /// <summary>
        /// Determines whether folder Specification is absolute [True], or relative
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns>True if specification is absolute</returns>
        public static bool IsAbsoluteFolder(string folderName)
        {
            if (folderName.StartsWith(@"\\")) return true;
            var driveLtr = folderName.Substring(0, 1).ToUpper()[0];
            return driveLtr >= 'A' && driveLtr <= 'Z' &&
                   string.Equals(folderName.Substring(1, 2), @":\", icic);
        }
        /// <summary>
        /// Convert underlying integral enum value to actual Enum
        /// </summary>
        /// <typeparam name="TE">Name of Enum to use</typeparam>
        /// <param name="enumType">Type of Enum</param>
        /// <param name="value">integer value of enum to check</param>
        /// <returns>actual Enum of type TE</returns>
        /// <exception cref="ArgumentException">If Generic type TE is specified type "enumType"</exception>
        public static TE GetEnumValue<TE>(Type enumType, int value) where TE : struct
        {
            if (!enumType.IsEnum) throw new ArgumentException("Not an Enum");
            if (!(typeof(TE) == enumType))
                throw new ArgumentException(
                    $"Type {enumType} is not an {typeof(TE)}");
            return (TE)Enum.Parse(enumType, value.ToString());
        }
        /// <summary>
        /// Path specification of folder of startup application.
        /// </summary>
        public static string ApplicationPath => Application.StartupPath;
        /// <summary>
        /// Extracts File Name from specified File Specification  
        /// </summary>
        /// <param name="fileSpec">Full file Specification of file including path</param>
        /// <returns>File name</returns>
        public static string ExtractFileName(string fileSpec)
        { return Path.GetFileName(fileSpec); }
        /// <summary>
        /// Extracts Path (folder) specification from specified File Specification
        /// </summary>
        /// <param name="fileSpec">Full file Specification of file including path</param>
        /// <returns>full path specification of folder file is in</returns>
        public static string ExtractPath(string fileSpec)
        { return Path.GetDirectoryName(fileSpec); }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filSpec">File Specification t obe examined</param>
        /// <returns>DateTime of embedded date string in filSpec</returns>
        public static DateTime ExtractReportDate(string filSpec)
        {
            var filNm = ExtractFileName(filSpec);
            var ndxPDF = filNm.LastIndexOf(".pdf", icic);
            var tmp = filNm.Remove(ndxPDF, 4);
            var ndxDt = tmp.LastIndexOf(".", icic);
            tmp = tmp.Remove(0, ndxDt);
            ndxDt = tmp.LastIndexOf(".", icic)+1;
            tmp = tmp.Substring(ndxDt);
            if (tmp.Contains("T"))
                tmp = tmp.Remove(0, tmp.IndexOf("T", icic)+1);
            if (!DateTime.TryParse(tmp, out var dt))
                throw new CoPInvalidOperationException("Report File Name does nto contain a valid datetime.");
            return dt;
        }
        /// <summary>
        /// Extracts first non-null parameter passed to function
        /// or null if all are null
        /// </summary>
        /// <typeparam name="T">Type of return object</typeparam>
        /// <param name="Os">enumerable list of nullable objects of type T</param>
        /// <returns>first non-null list member</returns>
        public static T Coalesce<T>(IEnumerable<T> Os) where T : class
        { return Os.FirstOrDefault(t => t != null); }
        /// <summary>
        /// Extracts first non-null parameter passed to function
        /// or null if all are null
        /// </summary>
        /// <typeparam name="T">Type of return object</typeparam>
        /// <param name="Os">param array of nullable objects of type T</param>
        /// <returns>first non-null parameter</returns>
        public static T Coalesce<T>(params T[] Os) where T : class
        { return Os.FirstOrDefault(t => t != null); }

        /// <summary>
        /// Extracts first non-null parameter object passed to function
        /// or null if all are null
        /// </summary>
        /// <param name="Os">param array of nullable objects</param>
        /// <returns>first non-null parameter as object</returns>
        public static object Coalesce(params object[] Os)
        { return Os.FirstOrDefault(o => o != null); }
        /// <summary>
        /// Determines if string can be converted to numeric value
        /// </summary>
        /// <param name="s">string to be examined</param>
        /// <returns>true if string s can be converted to numeric value</returns>
        public static bool IsNumeric(string s)
        {
            const NumberStyles sty = NumberStyles.Any;
            return (double.TryParse(s, sty, null, out double d));
        }
        /// <summary>
        /// Determines if arbitrary object can be converted to numeric value
        /// </summary>
        /// <param name="o">object to be examined</param>
        /// <returns>true if object o can be converted to numeric value</returns>
        public static bool IsNumeric(object o)
        {
            const NumberStyles sty = NumberStyles.Any;
            return o != null &&
                double.TryParse(o.ToString(), sty, null, out double d);
        }
        /// <summary>
        /// Determines if arbitrary object can be converted to integral value
        /// </summary>
        /// <param name="o">object to be examined</param>
        /// <returns>true if object o can be converted to integral value</returns>
        public static bool IsInteger(object o)
        {
            const NumberStyles sty = NumberStyles.Any;
            double d;
            if (o == null || (!double.TryParse(o.ToString(), sty, null, out d))) return (false);
            return (d == Math.Floor(d));
        }
        /// <summary>
        /// Determines if arbitrary object can be converted to a date
        /// </summary>
        /// <param name="o">object to be examined</param>
        /// <returns>true if object o can be converted to date</returns>
        public static bool IsDate(object o)
        {
            return o != null && 
                DateTime.TryParse(o.ToString(), out var dt);
        }
        /// <summary>
        /// Create Timespan object from a startdate and an endDate
        /// </summary>
        /// <param name="startDt">beginning of timespan</param>
        /// <param name="endDt">end of timespan</param>
        /// <returns>timespan of size equivilent to timespan between start and end dates
        /// resolves differences between time zones</returns>
        public static TimeSpan DSTTimeSpan(DateTime startDt, DateTime endDt)
        {
            DateTime strtUtc = 
              startDt.Kind == DateTimeKind.Utc?   startDt:
              startDt.Kind == DateTimeKind.Local? startDt.ToUniversalTime():
                                                  startDt.FromPacificTime(),
                     endUtc = 
              endDt.Kind == DateTimeKind.Utc ?  endDt :
              endDt.Kind == DateTimeKind.Local? endDt.ToUniversalTime():
                                                endDt.FromPacificTime();
            return endUtc - strtUtc;
        }
        /// <summary>
        /// Calculates the number of months between arbitrary dates
        /// </summary>
        /// <param name="startDt">beginning of timespan</param>
        /// <param name="endDt">end of timespan</param>
        /// <returns>number of month boundaries crossed getting from start to end date</returns>
        public static short MonthsDifference(this DateTime startDt, DateTime endDt)
        { return (short)(endDt.Month - startDt.Month + 12*(endDt.Year - startDt.Year)); }
        public static string ExtractNumbers(string sIn) { return (ExtractNumbers(sIn, false)); }
        public static string ExtractNumbers(string sIn, bool DecimalOK)
        {
            var sb = new StringBuilder(sIn.Length);
            foreach (var c in sIn)
                if (char.IsNumber(c)) sb.Append(c);
            return (sb.ToString());
        }

        public static int BitCount(int x)
        { return ((x == 0) ? 0 : ((x < 0) ? 1 : 0) + BitCount(x << 1)); }

        #region Minimum / Maximum methods
        public static T Minimum<T>(params T[] values) 
            where T : struct, IComparable<T>
        {
            var rV = values[0];
            foreach (var v in values.Where
                (v => v.CompareTo(rV) < 0))
                rV = v;
            return rV;
        }
        /// <summary>
        /// selects the smallest of a collection of longs (64 bit integers)
        /// </summary>
        /// <param name="values">param array of long (64 bit) integers</param>
        /// <returns></returns>
        public static long Minimum(params long[] values) { return Minimum<long>(values); }
        /// <summary>
        /// selects the smallest of a collection of integers
        /// </summary>
        /// <param name="values">param array of integers</param>
        /// <returns></returns>
        public static int Minimum(params int[] values) { return Minimum<int>(values); }
        /// <summary>
        /// selects the smallest of a collection of shorts (16 bit integers)
        /// </summary>
        /// <param name="values">param array of short (16 bit) integers</param>
        /// <returns></returns>
        public static short Minimum(params short[] values) { return Minimum<short>(values); }
        /// <summary>
        /// selects the largest of a collection of objects of type T 
        /// (T must be value type implementing IComparable<T>
        /// </summary>
        /// <typeparam name="T">struct type implementing IComparable<T></typeparam>
        /// <param name="values">param arrray of T objects</param>
        /// <returns>Maximum T object in param array</returns>
        public static T Maximum<T>(params T[] values) 
            where T : struct, IComparable<T>
        {
            var rV = values[0];
            foreach (var v in values.Where
                (v => v.CompareTo(rV) > 0))
                rV = v;
            return rV;
        }
        public static Int64 Maximum(params Int64[] values) { return Maximum<Int64>(values); }
        public static Int32 Maximum(params Int32[] values) { return Maximum<Int32>(values); }
        public static Int16 Maximum(params Int16[] values) { return Maximum<Int16>(values); }

        public static T Median<T>(T v1, T v2, T v3) 
            where T: struct, IComparable<T>
        { return Median(new[] {v1, v2, v3}); }
        public static T Median<T>(IEnumerable<T> values) 
            where T : struct, IComparable<T>
        {
            var list = new List<T>(values);
            list.Sort();
            return list[list.Count/2];
        }
        public static double Delta(double x, double y)
        { return Math.Abs(y - x);}
        public static bool Within(double x, double y, double epsilon)
        { return Delta(x, y) < epsilon; }

        #endregion Minimum / Maximum methods

        public static long IntPower(long x, int power)
        {
            return (power == 0) ? x :
                ((power & 0x1) == 0 ? x : 1) *
                    IntPower(x, power >> 1);
        }

        public static int MaxShift(uint inVal)
        {
            if (inVal == 0) return 0;
            var retVal = 1;
            while ((inVal >>= 1) > 0) retVal++;
            return retVal;
        }

        public static string GetMD5(Stream strm)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            var hash64 = md5.ComputeHash(strm);
            var sb = new StringBuilder();
            foreach (var t in hash64)
                sb.Append(t.ToString("X"));
            return sb.ToString();
        }

        public static string GetMD5(string value)
        { return (GetMD5(Encoding.ASCII.GetBytes(value.ToCharArray()))); }
        public static string GetMD5(byte[] data)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            var hash64 = md5.ComputeHash(data);

            var sb = new StringBuilder();
            foreach (var t in hash64)
                sb.Append(t.ToString("X"));
            return sb.ToString();
        }
        [Obsolete("This method should be replaced by static WaitHandle.WaitAll()", false)]
        public static void WaitAllParallel(WaitHandle[] handles)
        {
            if (handles == null)
                throw new ArgumentNullException("handles",
                    "WaitHandle[] handles was null");
            var actThreadCount = handles.Length;
            var locker = new object();
            foreach (var wh in handles)
            {
                var qwH = wh;
                ThreadPool.QueueUserWorkItem(
                     delegate
                     {
                         try { qwH.WaitOne(); }
                         finally { lock(locker) --actThreadCount; }
                     });
            }
            while (actThreadCount > 0) Thread.Sleep(80);
        }

        [Obsolete("This method should be replaced by static WaitHandle.WaitAll()", true)]
        public static void WaitAll(IEnumerable<WaitHandle> handles)
        {
            if (handles == null)
                throw new ArgumentNullException("handles",
                    "WaitHandle[] handles was null");
            foreach (var wh in handles) wh.WaitOne();
        }

        public static string Ordinal(int number, bool usewords = false)
        {
            if (usewords && number == 1) return "first";
            if (usewords && number == 2) return "second";
            if (usewords && number == 3) return "third";
            // ------------------------------
            var is111213 = number % 100 > 10 && number % 100 < 14;
            if (is111213) return number.ToString("0th");
            // ------------------------------------------
            switch (number % 10)
            {
                case 1: return (number.ToString("0st"));
                case 2: return (number.ToString("0nd"));
                case 3: return (number.ToString("0rd"));
                default: return (number.ToString("0th"));
            }
        }
        public static void ExceedsThreshold<T>(int threshold,
            Action<T> action, T parameter, params bool[] bools)
        { if (ExceedsThreshold(threshold, bools)) action(parameter); }
        public static bool ExceedsThreshold(int threshold, params bool[] bools)
        {
            var trueCnt = 0;
            return bools.Any(b => b && (++trueCnt > threshold));
        }
        public static bool EnumTryParse<E>(string enumVal, out E resOut) 
            where E : struct
        {
            if (string.IsNullOrEmpty(enumVal)) 
            { resOut = default(E); return false; }
            // --------------------------------

            var enumValFxd = enumVal.Replace(' ', '_');
            if (Enum.IsDefined(typeof(E), enumValFxd))
            {
                resOut = (E)Enum.Parse(typeof(E), 
                    enumValFxd, true);
                return true;
            }
            foreach (var value in
                Enum.GetNames(typeof (E)).Where(value => 
                    value.Equals(enumValFxd, 
                    StringComparison.OrdinalIgnoreCase)))
            {
                resOut = (E)Enum.Parse(typeof(E), value);
                return true;
            }
            resOut = default(E);
            return false;
        }

        public static void SleepUntilAllowed(
            int allowedStartMinute, int allowedStopMinute)
        {
            var delayMs = DelayMilliseconds(DateTime.Now,
                allowedStartMinute, allowedStopMinute);
            if (delayMs > 0) Thread.Sleep(delayMs);
        }

        internal static int DelayMilliseconds(DateTime refTime,
            int allowedStartMinute, int allowedStopMinute)
        {
            int nowMin = refTime.Minute,
                strt = allowedStartMinute,
                stop = allowedStopMinute;
            bool runXHr = strt > stop,
               pastStrt = nowMin >= strt,
                 b4Stop = nowMin < stop;
            var runNow = (strt == stop) ||
                 (runXHr && (pastStrt || b4Stop)) ||
                (!runXHr && pastStrt && b4Stop);
            return runNow ? 0 :
                (60000 * ((strt - nowMin + 60) % 60))
                - 1000 * refTime.Second - refTime.Millisecond;
        }
        public static bool InHttpContext => HttpContext.Current != null;

        public static string MapPath(string pathSpec)
        {
            if (pathSpec.StartsWith(@"\"))
                pathSpec = pathSpec.Substring(0, pathSpec.Length - 1);
            if (Directory.Exists(pathSpec)) return pathSpec;
            // -------------------------------------------------

            var basDir = CfgMgr.AppSettings["BaseDirectory"];
            return !string.IsNullOrEmpty(basDir)?
                    (basDir.EndsWith(@"\") ? basDir : basDir + @"\") + pathSpec :
                InHttpContext? HttpContext.Current.Server.MapPath(pathSpec):
                   (HostingEnvironment.ApplicationPhysicalPath?? string.Empty) + pathSpec;
        }
        public static bool IsAdministrator => 
            new WindowsPrincipal(WindowsIdentity.GetCurrent())
            .IsInRole(WindowsBuiltInRole.Administrator);

        public static void Exchange<T>(T objA, T objB)
        {  Switch(objA, objB); }
        public static void Switch<T>(T objA, T objB)
        {
            T tmp = objA;
            objA = objB;
            objB = tmp;
        }

        /// <summary>
        /// Determines if specified Port Number is currently In Use (Has a listenrer channel)
        /// </summary>
        /// <param name="port">port number to check</param>
        /// <returns>true/false</returns>
        public static bool IsPortInUse(int port)
        {
            var globProps = IPGlobalProperties.GetIPGlobalProperties();
            var activeListeners = globProps.GetActiveTcpListeners();
            return activeListeners.Any(conn => conn.Port == port);
        }

        public static bool IsNullable<T>(T obj)
        {
            if (obj == null) return true; // obvious
            var type = typeof(T);
            if (!type.IsValueType) return true; // ref-type
            return Nullable.GetUnderlyingType(type) != null;
        }

        #region caps lock, num Lock
        private const int VK_CAPITAL = 0x14;
        private const int VK_NUMLOCK = 0x90;
        [DllImport("user32.dll", 
            CharSet = CharSet.Auto, ExactSpelling = true, 
            CallingConvention = CallingConvention.Winapi)]
        private static extern short GetKeyState(int keyCode);
        public static bool CapsLock => (GetKeyState(VK_CAPITAL) & 0xffff) != 0;

        public static bool NumLock => (GetKeyState(VK_NUMLOCK) & 0xffff) != 0;

        #endregion

        #endregion

        #region TransactionScope
        public static TransactionScope GetTxScope(DependentTransaction depTx,
            TransactionScopeOption txScopeOption = TransactionScopeOption.Required, 
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, 
            int timeoutSeconds = 180)
        {
            return GetTxScope(depTx, txScopeOption,
                new TransactionOptions
                { IsolationLevel = isolationLevel,
                  Timeout = TimeSpan.FromSeconds(timeoutSeconds)});
        }
        public static TransactionScope GetTxScope(DependentTransaction depTx, 
            TransactionScopeOption txScopOtpion, TransactionOptions options)
        { return depTx != null ?
                new TransactionScope(depTx) :
                new TransactionScope(txScopOtpion, options);}
        #endregion TransactionScope

        #region ProcessIsAlreadyRunning & OS Imports for limitToOne
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        //private const int SW_HIDE = 0;
        //private const int SW_SHOWNORMAL = 1;
        //private const int SW_SHOWMINIMIZED = 2;
        //private const int SW_SHOWMAXIMIZED = 3;
        //private const int SW_SHOWNOACTIVATE = 4;
        private const int SW_RESTORE = 9;
        //private const int SW_SHOWDEFAULT = 10;
        private static readonly bool lim2One =
            !string.IsNullOrEmpty(CfgMgr.AppSettings["limitToSingleInstance"]) &&
            bool.Parse(CfgMgr.AppSettings["limitToSingleInstance"]);
        /// <summary>
        /// Determines and reports whether another instance of the current application is already running, 
        /// If it is, brings other instance to focus in foreground.
        /// </summary>
        public static bool ProcessIsAlreadyRunning
        {
            get
            {
                var thisProc = Process.GetCurrentProcess();
                // get all running instances of this application by Curr Process name
                var procs = Process.GetProcessesByName(thisProc.ProcessName);

                // if there is only one process...
                if (procs.Length <= 1) return false;

                // get the other process window handle:
                // if other process id is OUR process ID...
                // then the other process is index 1, otherwise index 0
                var othProcHndl = 
                    procs[procs[0].Id == thisProc.Id ? 1 : 0]
                        .MainWindowHandle;

                // if iconic, we need to restore the window
                if (IsIconic(othProcHndl))
                    ShowWindowAsync(othProcHndl, SW_RESTORE);
                // Bring it to the foreground
                SetForegroundWindow(othProcHndl);
                return true;
            }
        }

        #endregion OS Imports
    }

    public class CustomCursor
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetIconInfo(IntPtr hIcon, 
            ref IconInfo pIconInfo);

        [DllImport("user32.dll")]
        private static extern IntPtr CreateIconIndirect(ref IconInfo icon);

        public static Cursor CreateCursor(Bitmap bmp, 
            int xHotSpot, int yHotSpot)
        {
            var ptr = bmp.GetHicon();
            var tmp = new IconInfo();
            GetIconInfo(ptr, ref tmp);
            tmp.xHotspot = xHotSpot;
            tmp.yHotspot = yHotSpot;
            tmp.fIcon = false;
            ptr = CreateIconIndirect(ref tmp);
            return new Cursor(ptr);
        }
    }

    public static class Prompt
    {
        public static string ShowDialog(string text, string caption)
        {
            var prmt = new Form
            {
                Width = 500,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterParent
            };
            var txtLbl = new Label {Left = 50, Top = 20, AutoSize = true, Text = text};
            var tb = new TextBox {Left = 50, Width = 400,
                                  Top = txtLbl.Top + txtLbl.Height + 10};
            var cnfrmBtn = new Button {Left = 350, Top = 70, Width = 100, Text = "OK",
                                     DialogResult = DialogResult.OK};
            cnfrmBtn.Click += (sender, e) => { prmt.Close(); };
            prmt.Controls.Add(tb);
            prmt.Controls.Add(cnfrmBtn);
            prmt.Controls.Add(txtLbl);
            prmt.AcceptButton = cnfrmBtn;

            return prmt.ShowDialog() == DialogResult.OK ? tb.Text : string.Empty;
        }
    }

internal struct IconInfo
    {
        public bool fIcon;
        public int xHotspot;
        public int yHotspot;
        public IntPtr hbmMask;
        public IntPtr hbmColor;
    }

    public class QueueStack<T> : List<T>
    {
        #region Queue - Add at End, remove at beginning
        public void Enqueue(T val) { Add(val); }
        public void EnqueueRange(IEnumerable<T> items) { AddRange(items); } 
        public T Dequeue()
        {
            var val = base[0];
            RemoveAt(0);
            return val;
        }
        public IEnumerable<T> FifoX { get { while (Count > 0) yield return Dequeue(); } }
        public IEnumerable<T> Fifo { get { for (var i = 0; i < Count; i++) yield return base[i]; } }
        #endregion Queue

        #region Stack Add and remove at end Push, Pop
        public void Push(T val) { Add(val); }
        public void PushRange(IEnumerable<T> items) { AddRange(items); }
        public T Pop()
        {
            var val = base[Count-1];
            RemoveAt(Count - 1);
            return val;
        }
        public IEnumerable<T> LifoX { get { while (Count > 0) yield return Pop(); } }
        public IEnumerable<T> Lifo { get { for (var i = Count - 1; i >= 0; i--) yield return base[i]; } }
        #endregion Stack
    }

    public class ProgressEventArg: EventArgs
    {
        public int Progress { get; private set; }
        public string SubTask { get; private set; }
        public string Message { get; private set; }

        private ProgressEventArg(int progress, string subTask, string message)
        {
            Progress = progress;
            SubTask = subTask;
            Message = message;
        }
        public static ProgressEventArg Make(int progress, string subTask, string message)
        { return new ProgressEventArg(progress, subTask, message); }
    }


    public class ValueEventArg<T> : EventArgs where T : struct
    {
        public T Value { get; private set; }
        private ValueEventArg(T value) { Value = value; }
        public static ValueEventArg<T> Make(T value)
        { return new ValueEventArg<T>(value); }
    }

    public class ReferenceEventArg<T> : EventArgs where T : class
    {
        public T Object { get; private set; }
        private ReferenceEventArg(T obj) { Object = obj; }
        public static ReferenceEventArg<T> Make(T obj)
        { return new ReferenceEventArg<T>(obj); }
    }

    public enum HashAlg
    { MD5, RIPED160, SHA256, SHA384, SHA512}
}
