using System;
using System.Collections;
using System.Net;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.Text; 
using System.Security.Permissions;
using System.Web.Services.Protocols;
using System.Xml;
using cryp = CoP.Enterprise.Crypto;
using log = CoP.Enterprise.DALLog;
using cnUtil = CoP.Enterprise.Data.Utilities;
using Cnfg = System.Configuration.ConfigurationSettings; 
using CfgMgr = System.Configuration.ConfigurationManager;

[assembly:SecurityPermissionAttribute(SecurityAction.RequestMinimum, UnmanagedCode=true)]
namespace CoP.Enterprise.Data
{
    public static partial class Utilities
    {
        #region Connection Data
        #region Oracle specific Settings
        public static string GetTnsAlias(string AppName, APPENV env) {
            return GetTnsAlias(COMPANY, AppName, env); }
        public static string GetTnsAlias(string Company,  string AppName, APPENV env)
        { return DecryptServerName(CONNCFGSECTION[Company, AppName, env].TnsAlias); }
        public static string GetOracleDataSource(string AppName, APPENV env)
        { return GetOracleDataSource(COMPANY, AppName, env); }
        public static string GetOracleDataSource(string Company, string AppName, APPENV env)
        { return GetServerName(Company, AppName, env);}
        public static string GetOracleSID(string Company, string AppName, APPENV env)
        { return CONNCFGSECTION[Company, AppName, env].OracleSID; }
        public static string GetHostName(string AppName, APPENV env)
        { return GetServerName(COMPANY, AppName, env); }
        public enum OraEnlist { True, False, Dynamic }
        #endregion Oracle specific Settings

        #region server/Catalog/Database
        public static string GetServerName(string AppName, APPENV env)
        { return GetServerName(COMPANY, AppName, env); }
        public static string GetServerName(string Company,
            string AppName, APPENV env)
        {
            var conStgs = CONNCFGSECTION[Company, AppName, env];
            return DecryptServerName(
                string.IsNullOrEmpty(conStgs.ServerName) ?
                  conStgs.DataSource : conStgs.ServerName);
        }
        public static string GetCatalog(string AppName, APPENV env)
        { return GetCatalog(COMPANY, AppName, env); }
        public static string GetCatalog(string Company,
            string AppName, APPENV env)
        { return GetDatabaseName(Company, AppName, env); }
        public static string GetDatabaseName(string AppName, APPENV env)
        { return GetDatabaseName(COMPANY, AppName, env); }
        public static string GetDatabaseName(string Company,
            string AppName, APPENV env)
        { return CONNCFGSECTION[Company, AppName, env].Catalog; }
        #endregion server/Catalog/Database

        #region userId/Password
        public static string GetUserId(string AppName, APPENV env)
        { return GetUserId(COMPANY, AppName, env); }
        public static string GetUserId(string Company,
            string AppName, APPENV env)
        { return CONNCFGSECTION[Company, AppName, env].Logon; }
        public static string GetPassword(string AppName, APPENV env)
        { return GetPassword(COMPANY, AppName, env); }
        public static string GetPassword(string Company,
            string AppName, APPENV env)
        {
            var password = CONNCFGSECTION[Company, AppName, env].Password;
            if (string.IsNullOrEmpty(password)) return string.Empty;
            // ------------------------------------------------------
            if (password.Length > 0)
            {
                string sKy = CfgMgr.AppSettings["encryptionKey"],
                       sIv = CfgMgr.AppSettings["encryptionIV"];
                password = cryp.DecryptTripleDES(password, sKy, sIv);
            }
            return (password);
        }
        #endregion userId/Password

        #region connection pool settings
        public static int? GetMinPoolSize(string Company,
            string AppName, APPENV env)
        { return CONNCFGSECTION[Company, AppName, env].MinPoolSize; }
        public static int? GetMaxPoolSize(string Company,
            string AppName, APPENV env)
        { return CONNCFGSECTION[Company, AppName, env].MaxPoolSize; }
        public static int? GetPoolSizeIncrement(string Company,
            string AppName, APPENV env)
        { return CONNCFGSECTION[Company, AppName, env].PoolSizeIncrement; }
        public static int? GetPoolSizeDecrement(string Company,
            string AppName, APPENV env)
        { return CONNCFGSECTION[Company, AppName, env].PoolSizeDecrement; }
        public struct PoolSettings
        {
            #region private fields
            private int? minPSiz;
            private int? maxPSiz;
            private int? incr;
            private int? decr;
            #endregion private fields

            public static PoolSettings Null;
            public static PoolSettings Default = new PoolSettings(15, 100, 5, 2);

            #region public properties
            public int? MinPoolSize { get { return minPSiz; } set { minPSiz = value; } }
            public int? MaxPoolSize { get { return maxPSiz; } set { maxPSiz = value; } }
            public int? IncrementPool { get { return incr; } set { incr = value; } }
            public int? DecrementPool { get { return decr; } set { decr = value; } }
            public bool HasValue { get { return MaxPoolSize.HasValue || IncrementPool.HasValue || DecrementPool.HasValue; } }
            public bool IsNull { get { return !HasValue; } }
            #endregion public properties

            public PoolSettings(
                int? minPoolSize, int? maxPoolSize, 
                int? increment, int? decrement)
            {
                minPSiz = minPoolSize;
                maxPSiz = maxPoolSize;
                incr = increment;
                decr = decrement;
            }
        }
        #endregion connection pool settings
        #endregion

        public struct ConnectionString
        {
            #region Build Connection String Overloads
            #region Generic CompanyAppEnvironment Connection String Overloads
            /// <summary>
            /// 
            /// </summary>
            /// <param name="AppName"></param>
            /// <param name="Environment"></param>
            /// <returns></returns>
            public static string BuildConnectionString(
                string AppName, APPENV Environment)
            { return BuildConnectionString(COMPANY, AppName, 
                                            Environment); }
            /// <summary>
            /// Generates and returns Connection String based on config setting
            /// </summary>
            /// <param name="company">Software Publisher Name</param>
            /// <param name="appName">Application Name</param>
            /// <param name="appEnv">Run Mode: {member of CoP.Data.Data.APPENV}, Test A/D, QA, or PROD</param>
            /// <returns>Full Qualified Connection String</returns>
            public static string BuildConnectionString(
                string company, string appName,
                APPENV appEnv)
            {
                log.Write(log.Level.Debug, string.Format(
                    "CoP.Data.Utilities.BuildConnectionString({0}.{1}.{2})",
                    company, appName, appEnv), eventSource);
                var cApp = CONNCFGSECTION[company, appName];
                if (cApp == null)
                    throw new ArgumentNullException(string.Format(
                        "Cannot find Configuration section for {0}:{1}",
                         company, appName));
                var cSpc = CONNCFGSECTION[company, appName, appEnv];
                if (cSpc == null)
                    throw new ArgumentNullException(string.Format(
                        "Cannot find Configuration section for {0}:{1}:{2}",
                         company, appName, appEnv));

                var Vendor = (DBVendor)Enum.Parse(typeof(DBVendor),
                                    cApp.VendorName, true);
                string userId = cSpc.Logon,
                     password = cSpc.Password;


                var connTO = cSpc.ConnectionTimeout;
                var stgs = new PoolSettings(cSpc.MinPoolSize, cSpc.MaxPoolSize,
                            cSpc.PoolSizeIncrement, cSpc.PoolSizeDecrement);

                if (!string.IsNullOrEmpty(password))
                {
                    string sKy = CfgMgr.AppSettings["encryptionKey"],
                           sIv = CfgMgr.AppSettings["encryptionIV"];
                    password = cryp.DecryptTripleDES(password, sKy, sIv);
                }
                // -------------------------------------------------
                switch (Vendor)
                {
                    case DBVendor.SQLServer:
                        var Server = GetServerName(company, appName, appEnv);
                        var Database = cSpc.Catalog;
                        return BuildConnectionString(Server, Database, 
                            cSpc.Port, userId, password, connTO, stgs);

                    case DBVendor.Oracle:
                        var tnsAlias = GetTnsAlias(company, appName, appEnv);
                        var dataSource = GetOracleDataSource(company, appName, appEnv);
                        var hostName = GetServerName(company, appName, appEnv);
                        var sid = GetOracleSID(company, appName, appEnv);
                        var enlist = cSpc.Enlist;
                        return
                            (!string.IsNullOrEmpty(hostName) && !string.IsNullOrEmpty(sid) && cSpc.Port.HasValue) ?
                                BuildConnectionString(hostName, cSpc.Port.Value,
                                            sid, userId, password, connTO, enlist, stgs) :
                            !string.IsNullOrEmpty(tnsAlias) ?
                                BuildConnectionString(tnsAlias, userId, password, enlist, stgs) :
                            !string.IsNullOrEmpty(dataSource) ?
                                BuildConnectionString(dataSource, userId, password,
                                            enlist, stgs) : string.Empty;

                    case DBVendor.WebService:
                    case DBVendor.Remoting:
                        throw new CoPDataConfigurationException(
                            "Connections strings cannot be generated for " +
                            "Web Services or Remoting Services.");

                    default: throw new CoPDataConfigurationException(
                                   "Unknown Vendor Type ");
                }
            }
            #endregion Generic COmpanyAppEnvironment Connection String Overloads

            #region SQLServer ConnectionString Overloads
            /// <summary>
            /// Constructs SQL Server specific integrated security
            /// Connection string using specified Server and database name
            /// </summary>
            /// <param name="sqlServer">sql Server Name</param>
            /// <param name="database">Database (Catalog) Name</param>
            /// <returns>Full Qualified Connection String</returns>
            public static string BuildConnectionString(
                string sqlServer, string database)
            { return BuildConnectionString(sqlServer, database, null, PoolSettings.Null); }
            /// <summary>
            /// Constructs SQL Server specific integrated security
            /// Connection string using specified Server, 
            /// port number and dataabse name
            /// </summary>
            /// <param name="sqlServer">sql Server Name</param>
            /// <param name="port">port number if not 1433</param>
            /// <param name="database">Database (Catalog) Name</param>
            /// <returns>Full Qualified Connection String</returns>
            public static string BuildConnectionString(
                string sqlServer, string database, int? port)
            { return BuildConnectionString(sqlServer, database, port, PoolSettings.Null); }
            /// <summary>
            /// Constructs SQL Server specific Connection string 
            /// using specified Server, database name and credentials
            /// </summary>
            /// <param name="sqlServer">sql Server Name</param>
            /// <param name="database">Database (Catalog) Name</param>
            /// <param name="userID">sql Server Logon UserID</param>
            /// <param name="password">sql Server Logon Password</param>
            /// <returns>Full Qualified Connection String</returns>
            public static string BuildConnectionString(
                string sqlServer, string database, 
                string userID, string password)
            {
                return BuildConnectionString(sqlServer, database, null,
                  userID, password, PoolSettings.Null);
            }
            /// <summary>
            /// Constructs SQL Server specific Connection string using 
            /// specified Server, database name, port number and credentials
            /// </summary>
            /// <param name="sqlServer">sql Server Name</param>
            /// <param name="database">Database (Catalog) Name</param>
            /// <param name="port">port number if not 1433</param>
            /// <param name="userID">sql Server Logon UserID</param>
            /// <param name="password">sql Server Logon Password</param>
            /// <returns>Full Qualified Connection String</returns>
            public static string BuildConnectionString(
                string sqlServer, string database, int? port,
                string userID, string password)
            {
                return BuildConnectionString(sqlServer, database, port,
                  userID, password, PoolSettings.Null);
            }
            /// <summary>
            /// Constructs SQL Server specific Connection string using 
            /// specified Server, database name, pool settings and credentials
            /// </summary>
            /// <param name="sqlServer">sql Server Name</param>
            /// <param name="database">Database (Catalog) Name</param>
            /// <param name="userID">sql Server Logon UserID</param>
            /// <param name="password">sql Server Logon Password</param>
            /// <param name="stgs">struct containing min/max Pool size, 
            /// Increment, and Decrement Pool size settingss</param>
            /// <returns>Full Qualified Connection String</returns>
            public static string BuildConnectionString(
                string sqlServer, string database, 
                string userID, string password,
                PoolSettings stgs)
            {
                return BuildConnectionString(sqlServer, database, 
                         null,userID, password, null, stgs);
            }
            /// <summary>
            /// Constructs SQL Server specific Connection string using 
            /// specified Server, database and port number,
            /// pool settings and credentials
            /// </summary>
            /// <param name="sqlServer">sql Server Name</param>
            /// <param name="database">Database (Catalog) Name</param>
            /// <param name="port">port number if not 1433</param>
            /// <param name="userID">sql Server Logon UserID</param>
            /// <param name="password">sql Server Logon Password</param>
            /// <param name="stgs">Connection Pool settings, including
            ///  min/max pool sice and increment/decrement values
            /// Increment, and Decrement Pool size settings</param>
            /// <returns>Full Qualified Connection String</returns>
            public static string BuildConnectionString(
                string sqlServer, string database, int? port,
                string userID, string password,
                PoolSettings stgs)
            {
                return BuildConnectionString(sqlServer, database, port,
                         userID, password, null, stgs);
            }
            /// <summary>
            /// Constructs SQL Server specific integrated security
            /// Connection string using specified  
            /// Server, database and pool settings  
            /// </summary>
            /// <param name="sqlServer">sql Server Name</param>
            /// <param name="database">Database (Catalog) Name</param>
            /// <param name="stgs">Connection Pool settings, including
            ///  min/max pool sice and increment/decrement values </param>
            /// <returns>Full Qualified Connection String</returns>
            public static string BuildConnectionString(
                string sqlServer, string database, 
                PoolSettings stgs)
            { return BuildConnectionString(sqlServer, database, null, stgs); }
            /// <summary>
            /// Constructs SQL Server specific integrated security
            /// Connection string using specified  
            /// Server, database, port number and pool settings  
            /// </summary>
            /// <param name="sqlServer">sql Server Name</param>
            /// <param name="database">Database (Catalog) Name</param>
            /// <param name="port">port number if not 1433</param>
            /// <param name="stgs">Connection Pool settings, including
            ///  min/max pool sice and increment/decrement values </param>
            /// <returns>Full Qualified Connection String</returns>
            public static string BuildConnectionString(
                string sqlServer, string database, int? port,
                PoolSettings stgs)
            {
                var sb = new StringBuilder(45);
                sb.Append(string.Format(
                    "Data Source={0}{2};Initial Catalog={1};" +
                    "Trusted_Connection=True;" +
                    "MultipleActiveResultSets=True;" +
                    "Persist Security Info=false;",
                    sqlServer, database, 
                    port.HasValue? port.Value.ToString("','0"): string.Empty));
                AppendPoolSettings(sb, stgs);
                return sb.ToString();
            }
            /// <summary>
            /// Constructs SQL Server specific 
            /// Connection string using specified  
            /// Server, database, pool settings, 
            /// connection timeout and credentials
            /// </summary>
            /// <param name="sqlServer"></param>
            /// <param name="database"></param>
            /// <param name="userID">sql Server Logon UserID</param>
            /// <param name="password">sql Server Logon Password</param>
            /// <param name="connTimeOut">Maximum seconds ot wait for connection</param>
            /// <param name="stgs">Connection Pool settings, including
            ///  min/max pool sice and increment/decrement values </param>
            /// <returns>Full Qualified Connection String</returns>
            public static string BuildConnectionString(
                string sqlServer, string database, 
                string userID, string password,
                int? connTimeOut, PoolSettings stgs)
            { return BuildConnectionString(sqlServer, database, null, 
                userID, password, connTimeOut, stgs);}  
            /// <summary>
            /// Constructs SQL Server specific 
            /// Connection string using specified  
            /// Server, database, port number, pool settings, 
            /// connection timeout and credentials
            /// </summary>
            /// <param name="sqlServer"></param>
            /// <param name="database"></param>
            /// <param name="port">port number if not 1433</param>
            /// <param name="userID">UserId Logon</param>
            /// <param name="password">unencrypted password</param>
            /// <param name="connTimeOut">Maximum seconds ot wait for connection</param>
            /// <param name="stgs">Connection Pool settings, including
            ///  min/max pool sice and increment/decrement values </param>
            /// <returns>Full Qualified Connection String</returns>
            public static string BuildConnectionString(
                string sqlServer, string database, int? port,
                string userID, string password,
                int? connTimeOut, PoolSettings stgs)
            {
                var sb = new StringBuilder(120);
                sb.Append(string.Format(
                    "Data Source={0}{2};Initial Catalog={1};" +
                    "Persist Security Info=false;" +
                    "MultipleActiveResultSets=True;",
                    sqlServer, database,
                    port.HasValue ? port.Value.ToString("','0") : string.Empty));
                if (connTimeOut.HasValue)
                    sb.Append(string.Format("Connection Timeout={0};",
                        connTimeOut.Value));
                AppendSecurity(sb, userID, password);
                AppendPoolSettings(sb, stgs);
                return sb.ToString();
            }
            #endregion SQLServer ConnectionString Overloads

            #region Oracle Connection String overloads
            /// <summary>
            /// Generates and returns connection string 
            /// </summary>
            /// <param name="oracleDataSource">Oracle Data Source (Service Name) 
            /// as listed in tnsnames.ora file</param>
            /// <returns>Full Qualified Connection String</returns>
            public static string BuildConnectionString(string oracleDataSource)
            { return BuildConnectionString(oracleDataSource, PoolSettings.Null); }

            /// <summary>
            /// Generates and returns connection string 
            /// </summary>
            /// <param name="tnsAlias">Oracle Data Source (Service Name) 
            /// as listed in tnsnames.ora file</param>
            /// <param name="stgs">Connection Pool settings</param>
            /// <returns>Full Qualified Connection String</returns>
            public static string BuildConnectionString(
                string tnsAlias, PoolSettings stgs)
            {
                var sb = new StringBuilder(120);
                sb.Append("Data Source=" + tnsAlias);
                sb.Append("; Integrated Security = SSPI");
                AppendPoolSettings(sb, stgs);
                return sb.ToString();
            }
            /// <summary>
            /// Generates and returns connection string 
            /// </summary>
            /// <param name="oracleDataSource">Oracle Data Source (Service Name) as 
            /// listed in tnsnames.ora file</param>
            /// <param name="userID">Oracle Logon UserID</param>
            /// <param name="password">Oracle Logon Password</param>
            /// <returns>Full Qualified Connection String</returns>
            public static string BuildConnectionString(
                string oracleDataSource,
                string userID, string password)
            {
                return BuildConnectionString(
                      oracleDataSource, userID, password,
                      OraEnlist.True, PoolSettings.Null);
            }
            /// <summary>
            /// Generates and returns connection string 
            /// </summary>
            /// <param name="serverName">Server or host name</param>
            /// <param name="port">Port number if not 1521</param>
            /// <param name="serviceName">Oracle Data Source (Service Name) as 
            /// listed in tnsnames.ora file</param>
            /// <param name="userID">Oracle Logon UserID</param>
            /// <param name="password">Oracle Logon Password</param>
            /// <param name="enlist">whether to enlist in existing distributed transaction</param>
            /// <param name="stgs">Connection Pool settings</param>
            /// <returns>Full Qualified Connection String</returns>
            public static string BuildConnectionString(
                string serverName, int? port, string serviceName,
                string userID, string password,
                OraEnlist enlist, PoolSettings stgs)
            { return BuildConnectionString(serverName, port, 
                serviceName, userID, password, null, enlist, stgs); }
            /// <summary>
            /// Generates and returns connection string 
            /// </summary>
            /// <param name="serverName">Server or host name</param>
            /// <param name="port">Port number if not 1521</param>
            /// <param name="serviceName">Oracle Data Source (Service Name) as 
            /// listed in tnsnames.ora file</param>
            /// <param name="userID">Oracle Logon UserID</param>
            /// <param name="password">Oracle Logon Password</param>
            /// <param name="connTimeOut">seconds</param>
            /// <param name="enlist">whether to enlist in existing distributed transaction</param>
            /// <param name="stgs">Connection Pool settings</param>
            /// <returns>Full Qualified Connection String</returns>
            public static string BuildConnectionString(
                string serverName, int? port, string serviceName,
                string userID, string password, int? connTimeOut,
                OraEnlist enlist, PoolSettings stgs)
            {
                const string conStr =
                    "Data Source=" +
                      "(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)" +
                      "(HOST={0})(PORT={1})))(CONNECT_DATA=" +
                      "(SERVER=DEDICATED)(SERVICE_NAME={2})));enlist={3};";

                var sb =
                    new StringBuilder(string.Format(conStr,
                    serverName, (port.HasValue? port.Value: 1521),
                    serviceName, enlist.ToString().ToLower()));
                // ------------------------------------------------------------
                if (connTimeOut.HasValue)
                    sb.Append(string.Format("Connection Timeout={0};",
                        connTimeOut.Value));
                AppendSecurity(sb, userID, password);
                AppendPoolSettings(sb, stgs);
                return sb.ToString();
            }
            /// <summary>
            /// Generates and returns connection string 
            /// </summary>
            /// <param name="tnsAlias">Oracle Data Source (Service Name) 
            /// as listed in tnsnames.ora file</param>
            /// <param name="userID">Oracle Logon UserID</param>
            /// <param name="password">Oracle Logon Password</param>
            /// <param name="enlist">whether to enlist in existing distributed transaction</param>
            /// <param name="stgs">Connection Pool settings</param>
            /// <returns>Full Qualified Connection String</returns>
            public static string BuildConnectionString(
                string tnsAlias, string userID, string password,
                OraEnlist enlist, PoolSettings stgs)
            { return BuildConnectionString(tnsAlias,
                        userID, password, null, enlist, stgs); }
            /// <summary>
            /// Generates and returns connection string 
            /// </summary>
            /// <param name="tnsAlias">Oracle Data Source (Service Name) 
            /// as listed in tnsnames.ora file</param>
            /// <param name="userID">Oracle Logon UserID</param>
            /// <param name="password">Oracle Logon Password</param>
            /// <param name="connTimeOut"></param>
            /// <param name="enlist">whether to enlist in existing distributed transaction</param>
            /// <param name="stgs">Connection Pool settings</param>
            /// <returns>Full Qualified Connection String</returns>
            public static string BuildConnectionString(string tnsAlias,
                string userID, string password, int? connTimeOut,
                OraEnlist enlist, PoolSettings stgs)
            {
                var sb = new StringBuilder(
                    string.Format("Data Source={0}; enlist={1};",
                    tnsAlias, enlist.ToString().ToLower()));
                if (connTimeOut.HasValue)
                    sb.Append(string.Format("Connection Timeout={0};",
                        connTimeOut.Value));
                AppendSecurity(sb, userID, password);
                AppendPoolSettings(sb, stgs);
                return sb.ToString();
            }
            #endregion Oracle Connection String overloads

            #region connectionstring helpers
            private static void AppendSecurity(StringBuilder connString, string userId, string password)
            {
                connString.Append(
                    (string.IsNullOrEmpty(userId) ||
                     string.IsNullOrEmpty(password)) ?
                    "Integrated Security=SSPI;" :
                    string.Format("User Id={0};Password={1};", userId, password));
            }
            private static void AppendPoolSettings(StringBuilder connString, PoolSettings stgs)
            {
                if (!stgs.HasValue) return;
                if (stgs.MinPoolSize.HasValue)
                    connString.Append(string.Format(
                      "Min Pool Size={0};", stgs.MinPoolSize.Value));
                if (stgs.MaxPoolSize.HasValue)
                    connString.Append(string.Format(
                      "Max Pool Size={0};", stgs.MaxPoolSize.Value));
                if (stgs.IncrementPool.HasValue)
                    connString.Append(string.Format(
                      "Incr Pool Size={0};", stgs.IncrementPool.Value));
                if (stgs.DecrementPool.HasValue)
                    connString.Append(string.Format(
                      "Decr Pool Size={0};", stgs.DecrementPool.Value));
            }
            #endregion connectionstring helpers
            #endregion Build Connection String Overloads

            #region private Build ConnectionSpecification Overloads
            /// <summary>
            /// Generates and returns sql Server Connection Specification
            /// </summary>
            /// <param name="SqlServer">sql Server Name</param>
            /// <param name="Database">Database (Catalog) Name</param>
            /// <param name="UserID">sql Server Logon UserID</param>
            /// <param name="Password">sql Server Logon Password</param>
            /// <returns>Full Qualified Connection String</returns>
            internal static ConnectionSpec GetConnSpec(
                string SqlServer, string Database,
                string UserID, string Password)
            {
                return new ConnectionSpec(
                    BuildConnectionString(SqlServer, Database, 
                                null, UserID, Password),
                    DBVendor.SQLServer);
            }
            /// <summary>
            /// Generates and returns sql Server Connection Specification
            /// </summary>
            /// <param name="SqlServer">sql Server Name</param>
            /// <param name="Database">Database (Catalog) Name</param>
            /// <param name="port">Port number, if not 1433</param>
            /// <param name="UserID">sql Server Logon UserID</param>
            /// <param name="Password">sql Server Logon Password</param>
            /// <returns>Full Qualified Connection String</returns>
            internal static ConnectionSpec GetConnSpec(
                string SqlServer, string Database, int? port,
                string UserID, string Password)
            {
                return new ConnectionSpec(
                    BuildConnectionString(SqlServer, Database, 
                                    port, UserID, Password),
                    DBVendor.SQLServer);
            }
            /// <summary>
            /// Generates and returns sql Server Connection Specification
            /// struct with connection string property
            /// </summary>
            /// <param name="SqlServer">sql Server name/IP Address</param>
            /// <param name="Database">Database (Catalog) Name</param>
            /// <returns>sql Server Connection Specification</returns>
            internal static ConnectionSpec GetConnSpec(
                string SqlServer, string Database)
            {
                return new ConnectionSpec(
                    BuildConnectionString(SqlServer, Database),
                    DBVendor.SQLServer);
            }            
            /// <summary>
            /// Generates and returns sql Server Connection Specification
            /// struct with connection string property
            /// </summary>
            /// <param name="SqlServer">sql Server name/IP Address</param>
            /// <param name="Database">Database (Catalog) Name</param>
            /// <param name="port">Port number, if not 1433</param>
            /// <returns>sql Server Connection Specification</returns>
            internal static ConnectionSpec GetConnSpec(
                string SqlServer, string Database, int? port)
            {
                return new ConnectionSpec(
                    BuildConnectionString(SqlServer, Database, port),
                    DBVendor.SQLServer);
            }
            /// <summary>
            /// Generates and returns Oracle Connection Specification
            /// struct with connection string property
            /// </summary>
            /// <param name="OracleDataSource">Oracle Data Source (Service Name) as 
            /// listed in tnsnames.ora file</param>
            /// <returns>Oracle Connection Specification</returns>
            internal static ConnectionSpec GetConnSpec(
                string OracleDataSource)
            {
                return new ConnectionSpec(
                    BuildConnectionString(OracleDataSource),
                    DBVendor.Oracle);
            }
            /// <summary>
            /// Generates and returns Oracle Connection Specification
            /// struct with connection string property
            /// </summary>
            /// <param name="OracleDataSource">Oracle Data Source (Service Name) as 
            /// listed in tnsnames.ora file</param>
            /// <param name="UserID">Oracle Logon UserID</param>
            /// <param name="Password">Oracle Logon Password</param>
            /// <returns>Oracle Connection Specification</returns>
            internal static ConnectionSpec GetConnSpec(
                string OracleDataSource,
                string UserID, string Password)
            {
                return new ConnectionSpec(
                    BuildConnectionString(OracleDataSource, UserID, Password),
                    DBVendor.Oracle);
            }
            internal static ConnectionSpec GetConnSpec(
                string AppName, APPENV Environment)
            { return GetConnSpec(COMPANY, AppName, Environment); }
            /// <summary>
            /// Generates and returns Connection String based on config setting
            /// </summary>
            /// <param name="Company">Software Publisher Name</param>
            /// <param name="AppName">Application Name</param>
            /// <param name="Environment">Run Mode: {member of CoP.Data.Data.APPENV}, Test A/D, QA, or PROD</param>
            /// <returns>Full Qualified Connection String</returns>
            internal static ConnectionSpec GetConnSpec(
                string Company, string AppName,
                APPENV Environment)
            {
                var Vendor = (DBVendor)Enum.Parse(typeof(DBVendor),
                              CONNCFGSECTION[Company, AppName].VendorName, true);
                // -------------------------------------------------
                return new ConnectionSpec(
                    BuildConnectionString(Company, AppName, Environment), Vendor);
            }
            #endregion Build ConnectionSpecification Overloads
        }

        #region web service / remoting methods
        public static void InitializeWebService(
            SoapHttpClientProtocol webService, string appName, APPENV env)
        { InitializeWebService(webService, COMPANY, appName, env); }
        public static void InitializeWebService(
            SoapHttpClientProtocol webService, string company,
                    string appName, APPENV env)
        {
            log.Write(log.Level.Debug, string.Format(
                "Utilities.InitializeWebService({0}.{1}.{2})",
                company, appName, env), eventSource);

            var spec = CONNCFGSECTION[company, appName, env];
            webService.Url = BuildConnectionUrl(company, appName, env);
            webService.Credentials =
                string.IsNullOrEmpty(spec.Logon) || string.IsNullOrEmpty(spec.Password) ?
                CredentialCache.DefaultCredentials :
                new NetworkCredential(spec.Logon,
                cryp.DecryptTripleDES(spec.Password));
        }
        public static string BuildConnectionUrl(string appName, APPENV env)
        { return BuildConnectionUrl(COMPANY, appName, env); }
        public static string BuildConnectionUrl(string company,
            string appName, APPENV env)
        {
            var app = CONNCFGSECTION[company, appName];
            var vendor =
                (DBVendor)Enum.Parse(typeof(DBVendor),
                       app.VendorName, true);
            var spec = app[env];
            // ------------------------------------------------
            return
                vendor == DBVendor.WebService?
                    BuildWebServiceUrl(spec.Protocol, 
                        DecryptServerName(spec.ServerName), 
                        spec.Port, spec.FileName) :
                vendor == DBVendor.Remoting?
                    BuildRemotingUrl(spec.Protocol,
                        DecryptServerName(spec.ServerName),
                        (spec.Port.HasValue ? spec.Port.Value : 0),
                        spec.Uri) : null;
        }
        private static string BuildWebServiceUrl(
            string protocol, string server,
            int? port, string fileName)
        {
            var sb = new StringBuilder(120);
            sb.Append(protocol + @"://");
            sb.Append(server);
            if (port.HasValue)
                sb.Append(":" + port.Value);
            sb.Append("/" + fileName);
            return sb.ToString();
        }
        #endregion web service methods

        #region remoting methods
        private static string BuildRemotingUrl(
            string protocol, string server,
            int port, string uri)
        {
            var sb = new StringBuilder(120);
            sb.Append(protocol + "://");
            sb.Append(server + ":" + port);
            sb.Append("/" + uri);
            return sb.ToString();
        }
        public static T BuildRemotingChannel<T>(
            string company, string appName, APPENV env)
        {
            var spec = CONNCFGSECTION[company, appName, env];
            var url = BuildConnectionUrl(company, appName, env);
            var secMod = spec.RemotingSecurityMode;
            var prop = new Hashtable();
            prop["name"] = string.Empty;
            prop["timeout"] = spec.RemotingTimeout;
            if (secMod == RemSecMode.Identify)
            {
                prop["secure"] = true;
                prop["tokenImpersonationLevel"] =
                    TokenImpersonationLevel.Identification;
                prop["useDefaultCredentials"] = true;
            }
            var chan = new TcpClientChannel(prop, null);
            ChannelServices.RegisterChannel(chan, true);
            return (T)Activator.GetObject(typeof(T), url);
        }
        #endregion remoting methods

        #region WCF stuff
        public static string BuildWcfUrl(
            string company, string appName, APPENV env)
        {
            var app = CONNCFGSECTION[company, appName];
            var spec = app[env];
            // --------------------------------------
            return BuildWcfUrl(DecryptServerName(spec.ServerName), 
                    spec.Port, spec.Uri, spec.WcfBinding, spec.SSL);
        }
        
        public static WcfBinding GetWcfBinding(
            string company, string appName, APPENV env)
        { return CONNCFGSECTION[company, appName][env].WcfBinding; }
        
        public static Binding GetServiceBinding(
            string company, string appName, APPENV env)
        {
            var conSpec = CONNCFGSECTION[company, appName][env];
            return GetServiceBinding(conSpec);
        }

        public static string GetServerSideUPN(
             string company, string appName, APPENV env)
        {
            var conSpec = CONNCFGSECTION[company, appName][env];
            return conSpec.ServerSideUPN;
        }
        
        public static int? GetMaxItemsInObjectGraph(
            string company, string appName, APPENV env)
        {
            var conSpec = CONNCFGSECTION[company, appName][env];
            return conSpec.MaxItemsInObjectGraph;
        }

        private static string BuildWcfUrl(
            string server, int? port, string uri,
            WcfBinding binding, bool ssl)
        {
            var isHttp = binding == WcfBinding.BasicHttp ||
                         binding == WcfBinding.WSHttp;
            if (!isHttp && !port.HasValue)
                throw new ArgumentNullException(
                    "Port", "Port is required");
            var sb = new StringBuilder(120);
            sb.Append(WcfProtocol(binding, ssl) + "://");
            sb.Append(server);
            if (port.HasValue) sb.Append(":" + port.Value);
            sb.Append("/" + uri);
            return sb.ToString();
        }
        
        private static string WcfProtocol(
            WcfBinding binding, bool ssl)
        {
            switch (binding)
            {
                case WcfBinding.BasicHttp:
                case WcfBinding.WSHttp: return "http" + (ssl ? "s" : "");
                case WcfBinding.NetTcp: return "net.tcp";
                case WcfBinding.NetNamedPipe: return "net.pipe";
                case WcfBinding.WSDualHttp: return "http";
                case WcfBinding.NetMsmq: return "net.msmq";
                case WcfBinding.Custom: return "net.tcp";
                default: return string.Empty;
            }
        }
        
        private static Binding GetServiceBinding(ConnSpec conSpec)
        {
            var to = new TimeSpan(1, 0, 0);
            Binding bnd;
            switch (conSpec.WcfBinding)
            {
                #region BasicHttp
                case WcfBinding.BasicHttp:
                    var httpBnd = new BasicHttpBinding
                    {
                        MaxReceivedMessageSize = conSpec.MaxReceivedMessageSize,
                        MaxBufferPoolSize = conSpec.MaxBufferPoolSize,
                        BypassProxyOnLocal = conSpec.BypassProxyOnLocal,
                        AllowCookies = conSpec.AllowCookies,
                        HostNameComparisonMode = conSpec.HostNameComparisonMode,
                        MessageEncoding = conSpec.MessageEncoding,
                        TextEncoding = conSpec.TextEncoding,
                        TransferMode = conSpec.TransferMode,
                        UseDefaultWebProxy = conSpec.UseDefaultWebProxy,
                        ReaderQuotas = GetReaderQuotas(conSpec),
                        Security = {
                            Mode = conSpec.BasicSecurityMode,
                            Transport = {
                                ClientCredentialType = conSpec.TransportCredentialType,
                                ProxyCredentialType = conSpec.ProxyCredentialType } }
                    };
                    //--------------------------------------------------------
                    httpBnd.Security.Message.ClientCredentialType = conSpec.BasicHttpMessageCredentialType;
                    httpBnd.Security.Message.AlgorithmSuite = conSpec.SecurityAlgorithmSuite;
                    bnd = httpBnd;
                    break;
                #endregion BasicHttp

                #region WSHttp
                case WcfBinding.WSHttp:
                    var wsBnd = new WSHttpBinding
                    {
                        MaxReceivedMessageSize = conSpec.MaxReceivedMessageSize,
                        MaxBufferPoolSize = conSpec.MaxBufferPoolSize,
                        AllowCookies = conSpec.AllowCookies,
                        BypassProxyOnLocal = conSpec.BypassProxyOnLocal,
                        MessageEncoding = conSpec.MessageEncoding,
                        HostNameComparisonMode = conSpec.HostNameComparisonMode,
                        TextEncoding = conSpec.TextEncoding,
                        UseDefaultWebProxy = conSpec.UseDefaultWebProxy,
                        ReaderQuotas = GetReaderQuotas(conSpec),
                        ReliableSession = {
                                Enabled = conSpec.ReliableSession,
                                InactivityTimeout = conSpec.ReliableSessionTimeout.Value,
                                Ordered = conSpec.RequireOrderedDelivery
                            },
                        Security = {
                            Mode = conSpec.SecurityMode, 
                            Transport = {
                                    ClientCredentialType = conSpec.TransportCredentialType,
                                    ProxyCredentialType = conSpec.ProxyCredentialType } }
                    };
                    //-------------------------------------------------------------
                    wsBnd.Security.Message.ClientCredentialType = conSpec.MessageCredentialType;
                    wsBnd.Security.Message.AlgorithmSuite = conSpec.SecurityAlgorithmSuite;
                    wsBnd.Security.Message.EstablishSecurityContext = conSpec.EstablishSecurityContext;
                    bnd = wsBnd;
                    break;
                #endregion WSHttp

                #region WSDualHttp
                case WcfBinding.WSDualHttp: 
                    var dualbnd = new WSDualHttpBinding
                    {
                        MaxReceivedMessageSize = conSpec.MaxReceivedMessageSize,
                        MaxBufferPoolSize = conSpec.MaxBufferPoolSize,
                        BypassProxyOnLocal = false,
                        HostNameComparisonMode = HostNameComparisonMode.StrongWildcard,
                        MessageEncoding = WSMessageEncoding.Text,
                        TextEncoding = Encoding.UTF8,
                        UseDefaultWebProxy = true,
                        TransactionFlow = false,
                        ReaderQuotas = GetReaderQuotas(conSpec)
                    };
                    dualbnd.Security.Mode = conSpec.WSDualSecurityMode;
                    dualbnd.Security.Message.ClientCredentialType = conSpec.MessageCredentialType;
                    dualbnd.Security.Message.AlgorithmSuite = SecurityAlgorithmSuite.Default;
                    bnd = dualbnd;
                    break;
                #endregion WSDualHttp

                #region NetTcp
                case WcfBinding.NetTcp:
                    var tcpBnd = new NetTcpBinding
                    {
                        MaxReceivedMessageSize = conSpec.MaxReceivedMessageSize,
                        MaxBufferPoolSize = conSpec.MaxBufferPoolSize,
                        MaxBufferSize = conSpec.MaxBufferSize,
                        HostNameComparisonMode = HostNameComparisonMode.StrongWildcard,
                        MaxConnections = conSpec.MaxConnections,
                        ReaderQuotas = GetReaderQuotas(conSpec),
                        Security = {Mode = conSpec.TcpSecurityMode},
                        ReliableSession = {Enabled = conSpec.ReliableSession}
                    };
                    if (conSpec.ReliableSessionTimeout.HasValue)
                        tcpBnd.ReliableSession.InactivityTimeout = conSpec.ReliableSessionTimeout.Value;
                    tcpBnd.Security.Transport.ClientCredentialType = conSpec.TcpClientCredentialType;
                    tcpBnd.Security.Message.ClientCredentialType = conSpec.MessageCredentialType;
                    tcpBnd.Security.Message.AlgorithmSuite = SecurityAlgorithmSuite.Default;
                    bnd = tcpBnd;
                    break;

                case WcfBinding.Custom:
                    var customBinding = new CustomBinding();
                    customBinding.Elements.Add(SecurityBindingElement.CreateUserNameOverTransportBindingElement());
                    customBinding.Elements.Add(new WindowsStreamSecurityBindingElement());
                    customBinding.Elements.Add(new TcpTransportBindingElement());

                    //{
                    //    MaxReceivedMessageSize = conSpec.MaxReceivedMessageSize,
                    //    MaxBufferPoolSize = conSpec.MaxBufferPoolSize,
                    //    MaxBufferSize = conSpec.MaxBufferSize,
                    //    HostNameComparisonMode = HostNameComparisonMode.StrongWildcard,
                    //    MaxConnections = conSpec.MaxConnections,
                    //    ReaderQuotas = GetReaderQuotas(conSpec),
                    //    Security = { Mode = conSpec.TcpSecurityMode },
                    //    ReliableSession = { Enabled = conSpec.ReliableSession }
                    //};
                    //if (conSpec.ReliableSessionTimeout.HasValue)
                    //    customBinding.ReliableSession.InactivityTimeout = conSpec.ReliableSessionTimeout.Value;
                    //customBinding.Security.Transport.ClientCredentialType = conSpec.TcpClientCredentialType;
                    //customBinding.Security.Message.ClientCredentialType = conSpec.MessageCredentialType;
                    //customBinding.Security.Message.AlgorithmSuite = SecurityAlgorithmSuite.Default;
                    bnd = customBinding;
                    break;

                #endregion NetTcp

                #region NetNamedPipe
                case WcfBinding.NetNamedPipe: 
                    var npBnd = new NetNamedPipeBinding
                    {
                        MaxReceivedMessageSize = conSpec.MaxReceivedMessageSize,
                        MaxBufferPoolSize = conSpec.MaxBufferPoolSize,
                        MaxConnections = conSpec.MaxConnections,
                        MaxBufferSize = conSpec.MaxBufferSize,
                        HostNameComparisonMode = HostNameComparisonMode.StrongWildcard,
                        ReaderQuotas = GetReaderQuotas(conSpec)
                    };
                    npBnd.Security.Mode = conSpec.NamedPipeSecurityMode;
                    npBnd.Security.Transport.ProtectionLevel = conSpec.NamedPipeProtectionLevel;
                    bnd = npBnd;
                    break;
                #endregion NetNamedPipe

                #region NetMsmq
                case WcfBinding.NetMsmq: 
                    var msmqBnd = new NetMsmqBinding
                    {
                        Durable = conSpec.Durable,
                        MaxReceivedMessageSize = conSpec.MaxReceivedMessageSize,
                        MaxBufferPoolSize  = conSpec.MaxBufferPoolSize,
                        MaxRetryCycles = conSpec.MaxRetryCycles
                    };
                    bnd = msmqBnd;
                    break;
                #endregion NetMsmq

                default: return null; 
            }
            bnd.SendTimeout = bnd.OpenTimeout = 
                bnd.CloseTimeout = bnd.ReceiveTimeout = to;
            return bnd;
        }
        
        private static XmlDictionaryReaderQuotas GetReaderQuotas(ConnSpec conSpec)
        {
            return new XmlDictionaryReaderQuotas {
                MaxStringContentLength = conSpec.MaxStringContentLength,
                MaxDepth = conSpec.MaxDepth,
                MaxArrayLength = conSpec.MaxArrayLength,
                MaxBytesPerRead = conSpec.MaxBytesPerRead,
                MaxNameTableCharCount = conSpec.MaxNameTableCharCount};
        }
        #endregion WCF stuff

        private static string DecryptServerName(string svrNm)
        {
            return !string.IsNullOrEmpty(svrNm) && svrNm.StartsWith("{~") && svrNm.EndsWith("~}") ?
                 cryp.DecryptTripleDES(svrNm.Substring(2, svrNm.Length - 4),
                     CfgMgr.AppSettings["encryptionKey"],
                     CfgMgr.AppSettings["encryptionIV"]) : svrNm;
        }
    }
}
