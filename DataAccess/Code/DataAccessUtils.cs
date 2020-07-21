using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using log = CoP.Enterprise.DALLog;
using lib = CoP.Enterprise.Utilities;
using cnUtil = CoP.Enterprise.Data.Utilities;
using Cnfg = System.Configuration.ConfigurationSettings;
using CfgMgr = System.Configuration.ConfigurationManager;

namespace CoP.Enterprise.Data
{
    public static partial class Utilities
    {
        private const CommandBehavior closeConn
            = CommandBehavior.CloseConnection; 

        #region DataSet methods
        #region public AddtoDataSet Overloads
        /// <summary>
        /// Add one or more additional DataTables to existing DataSet
        /// Connects to specified database using integrated security
        /// </summary>
        /// <param name="ds">DataSet to add new DatTable to... (Can be empty)</param>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a procedure or block of select statements 
        /// that returns one or more result sets}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <param name="UserID">logon or User Id to use in connectiong to database</param>
        /// <param name="Password">Clear text password to use in connectiong to database</param>
        /// <returns>DataSet pass in, with one or more additional 
        /// DataTables containing requested data</returns>       
        public static void GetDataSet(ref DataSet ds,
            string sql, string Server, string Database,
            string UserID, string Password)
        {
            GetDataSet(ref ds, sql, null,
                  cnUtil.ConnectionString.GetConnSpec(Server, Database,
                  UserID, Password));
        }
        /// <summary>
        /// Add one or more additional DataTables to existing DataSet
        /// Connects to specified database using integrated security
        /// </summary>
        /// <param name="ds">DataSet to add new DatTable to... (Can be empty)</param>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a procedure or block of select statements 
        /// that returns one or more result sets}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <param name="port">Port number, if not 1433</param>
        /// <param name="UserID">logon or User Id to use in connectiong to database</param>
        /// <param name="Password">Clear text password to use in connectiong to database</param>
        /// <returns>DataSet pass in, with one or more additional 
        /// DataTables containing requested data</returns>       
        public static void GetDataSet(ref DataSet ds,
            string sql, string Server, string Database, int? port,
            string UserID, string Password)
        {
            GetDataSet(ref ds, sql, null,
                  ConnectionString.GetConnSpec(Server, Database, 
                            port, UserID, Password));
        }
        /// <summary>
        /// Add one or more additional DataTables to existing DataSet
        /// Connects to specified database using integrated security
        /// </summary>
        /// <param name="ds">DataSet to add new DatTable to... (Can be empty)</param>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a procedure or block of select statements 
        /// that returns one or more result sets}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <param name="UserID">logon or User Id to use in connectiong to database</param>
        /// <param name="Password">Clear text password to use in connectiong to database</param>
        /// <param name="tableName">optional tableName to assign to DataTables</param>
        /// <returns>DataSet pass in, with one or more additional 
        /// DataTables containing requested data</returns>       
        public static void GetDataSet(ref DataSet ds,
            string sql, string Server, string Database,
            string UserID, string tableName, string Password)
        {
            GetDataSet(ref ds, sql, tableName,
                  ConnectionString.GetConnSpec(Server, Database,
                  UserID, Password));
        }
        /// <summary>
        /// Add one or more additional DataTables to existing DataSet
        /// Connects to specified database using integrated security
        /// </summary>
        /// <param name="ds">DataSet to add new DatTable to... (Can be empty)</param>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a procedure or block of select statements 
        /// that returns one or more result sets}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <param name="port">Port number, if not 1433</param>
        /// <param name="UserID">logon or User Id to use in connectiong to database</param>
        /// <param name="Password">Clear text password to use in connectiong to database</param>
        /// <param name="tableName">optional tableName to assign to DataTables</param>
        /// <returns>DataSet pass in, with one or more additional 
        /// DataTables containing requested data</returns>       
        public static void GetDataSet(ref DataSet ds,
            string sql, string Server, string Database, int? port,
            string UserID, string tableName, string Password)
        {
            GetDataSet(ref ds, sql, tableName,
                  ConnectionString.GetConnSpec(Server, Database,
                                    port, UserID, Password));
        }
        /// <summary>
        /// Add one or more additional DataTables to existing DataSet
        /// Connects to specified database using integrated security
        /// </summary>
        /// <param name="ds">DataSet to add new DatTable to... (Can be empty)</param>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a procedure or block of select statements 
        /// that returns one or more result sets}</param>
        /// <param name="AppName">Application name specifying Database to connect to</param>
        /// <param name="Environment">Specifies environment instance of database to connect</param>
        /// <returns>DataSet pass in, with one or more additional 
        /// DataTables containing requested data</returns>       
        public static void GetDataSet(ref DataSet ds,
            string sql, string AppName, APPENV Environment)
        { GetDataSet(ref ds, sql, COMPANY, AppName, Environment); }
        /// <summary>
        /// Add one or more additional DataTables to existing DataSet
        /// Connects to specified database using integrated security
        /// </summary>
        /// <param name="ds">DataSet to add new DatTable to... (Can be empty)</param>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a procedure or block of select statements 
        /// that returns one or more result sets}</param>
        /// <param name="Company">Specifies Company Name element in Application configuration file</param>
        /// <param name="AppName">Application name specifying Database to connect to</param>
        /// <param name="Environment">Specifies environment instance of database to connect</param>
        /// <returns>DataSet pass in, with one or more additional DataTables added.</returns>
        public static void GetDataSet(ref DataSet ds,
            string sql, string Company,
            string AppName, APPENV Environment)
        {
            GetDataSet(ref ds, sql, null,
                  ConnectionString.GetConnSpec(Company, AppName, Environment));
        }
        /// <summary>
        /// Add one or more additional DataTables t oexisting DataSet
        /// Connects to specified database using integrated security
        /// </summary>
        /// <param name="ds">DataSet to add new DatTable to... (Can be empty)</param>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a procedure or block of select statements 
        /// that returns one or more result sets}</param>
        /// <param name="tableName">optional name to assign to database object</param>
        /// <param name="Company">Specifies Company Name element in Application configuration file</param>
        /// <param name="AppName">Application name specifying Database to connect to</param>
        /// <param name="Environment">Specifies environment instance of database to connect</param>
        /// <returns>DataSet passed in, with one or more additional DataTables added.</returns>
        public static void GetDataSet(ref DataSet ds,
            string sql, string tableName, string Company,
            string AppName, APPENV Environment)
        {
            GetDataSet(ref ds, sql, tableName,
                  ConnectionString.GetConnSpec(Company, AppName, Environment));
        }
        /// <summary>
        /// Add one or more additional DataTables to existing DataSet
        /// Connects to specified database using integrated security
        /// </summary>
        /// <param name="ds">DataSet to add new DatTable to... (Can be empty)</param>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a procedure or block of select statements 
        /// that returns one or more result sets}</param>
        /// <param name="tableName">optional tableName to assign to DataTable</param>
        /// <param name="CoPConnection">CoPConnection object - 
        /// (wrapper for SqlConnection or OracleConnection)</param>
        /// <returns>Dataset with one or more DataTables containing requested data</returns>
        public static void GetDataSet(DataSet ds, string sql,
            string tableName, CoPConnection CoPConnection)
        {
            string sTNNdx;
            if (String.IsNullOrEmpty(tableName))
                tableName = "Table" + ds.Tables.Count;
            else
                while (ds.Tables.Contains(tableName))
                    tableName = (lib.IsInteger(sTNNdx = tableName.Substring(tableName.Length - 3))) ?
                                tableName.Replace(sTNNdx, ((Int32.Parse(sTNNdx)) + 1).ToString()) :
                                tableName + "0001";
            // ---------------------------------------------------------
            var dt = GetDataTable(sql, tableName, CoPConnection);
            ds.Tables.Add(dt);
        }
        #endregion AddtoDataSet Overloads

        #region public GetdataSet Overloads
        #region GetDataSet based on sql statements
        /// <summary>
        /// Create and return DataSet using specified sql Statement
        /// Connects to database specified by Configuration settings 
        /// based on specified parameters
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a procedure call or block of one or more 
        /// select statement(s) that returns one or more result sets}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <param name="UserID">logon or User Id to use in connectiong to database</param>
        /// <param name="Password">Clear text password to use in connectiong to database</param>
        /// <returns>Dataset with one or more DataTables containing requested data</returns>
        public static DataSet GetDataSet(string sql,
            string Server, string Database,
            string UserID, string Password)
        {
            return GetDataSet(sql,
                  ConnectionString.GetConnSpec(Server, Database,
                  UserID, Password));
        }
        /// <summary>
        /// Create and return DataSet using specified sql Statement
        /// Connects to database specified by Configuration settings 
        /// based on specified parameters
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a procedure call or block of one or more 
        /// select statement(s) that returns one or more result sets}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <param name="port">Port number, if not 1433</param>
        /// <param name="UserID">logon or User Id to use in connectiong to database</param>
        /// <param name="Password">Clear text password to use in connectiong to database</param>
        /// <returns>Dataset with one or more DataTables containing requested data</returns>
        public static DataSet GetDataSet(string sql,
            string Server, string Database, int? port,
            string UserID, string Password)
        {
            return GetDataSet(sql,
                  ConnectionString.GetConnSpec(Server, Database,
                                port, UserID, Password));
        }
        /// <summary>
        /// Create and return DataSet using specified sql Statement
        /// Connects to database specified by Configuration settings 
        /// based on specified parameters using integrated security
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a procedure call or block of one or more 
        /// select statement(s) that returns one or more result sets}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <returns>Dataset with one or more DataTables containing requested data</returns>
        public static DataSet GetDataSet(string sql,
            string Server, string Database)
        { return GetDataSet(sql, ConnectionString.GetConnSpec(Server, Database)); }
        /// <summary>
        /// Create and return DataSet using specified sql Statement
        /// Connects to database specified by Configuration settings 
        /// based on specified parameters using integrated security
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a procedure call or block of one or more 
        /// select statement(s) that returns one or more result sets}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <param name="port">Port number, if not 1433</param>
        /// <returns>Dataset with one or more DataTables containing requested data</returns>
        public static DataSet GetDataSet(string sql, 
            string Server, string Database, int? port)
        { return GetDataSet(sql, ConnectionString.GetConnSpec(
                            Server, Database, port)); }

        /// <summary>
        /// Create and return DataSet using specified sql Statement
        /// Connects to database specified by Configuration settings 
        /// based on specified parameters
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a procedure call or block of one or more 
        /// select statement(s) that returns one or more result sets}</param>
        /// <param name="AppName">Application name specifying Database to connect to</param>
        /// <param name="Environment">Specifies environment instance of database to connect</param>
        /// <returns>Dataset with one or more DataTables containing requested data</returns>
        public static DataSet GetDataSet(string sql,
                    string AppName, APPENV Environment)
        { return GetDataSet(sql, ConnectionString.GetConnSpec(
            COMPANY, AppName, Environment)); }
        /// <summary>
        /// Create and return DataSet using specified sql Statement
        /// Connects to database specified by Configuration settings 
        /// based on specified parameters
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a procedure call or block of one or more 
        /// select statement(s) that returns one or more result sets}</param>
        /// <param name="Company">Specifies Company Name element in Application configuration file</param>
        /// <param name="AppName">Application name specifying Database to connect to</param>
        /// <param name="Environment">Specifies environment instance of database to connect</param>
        /// <returns>Dataset with one or more DataTables containing requested data</returns>
        public static DataSet GetDataSet(
            string sql, string Company,
            string AppName, APPENV Environment)
        { return GetDataSet(sql, 
            ConnectionString.GetConnSpec(
            Company, AppName, Environment));
        }
        #endregion GetDataSet based on sql statements

        #region GetDataset from Stored Proc overloads
        /// Create and return DataSet using specified stored proc
        /// Connects to server.database using integrated securiy
        /// <param name="procName">stored procedure name</param>
        /// <param name="parms">parameter collection</param>
        /// <param name="server">Server Name </param>
        /// <param name="database">database or catalog name</param>
        /// <returns>ADO.Net dataset</returns>
        public static DataSet GetDataSet(string procName,
            IEnumerable<IDbDataParameter> parms, string server, string database)
        { return GetDataSet(procName, parms,
                    ConnectionString.GetConnSpec(server, database),
                    DEFTIMEOUT); }

        /// <summary>
        /// Create and return DataSet using specified stored proc
        /// Connects to server.database using specified credentials
        /// </summary>
        /// <param name="procName">stored procedure name</param>
        /// <param name="parms">parameter collection</param>
        /// <param name="server">Server Name </param>
        /// <param name="database">database or catalog name</param>
        /// <param name="userId">SQL Server Logon</param>
        /// <param name="password">SQL Server Logon password</param>
        /// <returns>ADO.Net dataset</returns>
        public static DataSet GetDataSet(string procName,
            IEnumerable<IDbDataParameter> parms, 
            string server, string database,
            string userId, string password)
        { return GetDataSet(procName, parms,
                    ConnectionString.GetConnSpec(server, database, userId, password),
                    DEFTIMEOUT); }

        /// <summary>
        /// Create and return DataSet using specified stored proc
        /// Connects to server.database using specified credentials
        /// </summary>
        /// <param name="procName">stored procedure name</param>
        /// <param name="parms">parameter collection</param>
        /// <param name="timeOut">execution timeout in seconds</param>
        /// <param name="server">Server Name </param>
        /// <param name="database">database or catalog name</param>
        /// <param name="userId">SQL Server Logon</param>
        /// <param name="password">SQL Server Logon password</param>
        /// <returns>ADO.Net dataset</returns>
        public static DataSet GetDataSet(string procName,
            IEnumerable<IDbDataParameter> parms, int timeOut,
            string server, string database,
            string userId, string password)
        { return GetDataSet(procName, parms,
                    ConnectionString.GetConnSpec(server, database, userId, password),
                    timeOut); }

        /// <summary>
        /// Create and return DataSet using specified stored proc
        /// Connects to database as specified in configuration file for Appname/Environment
        /// </summary>
        /// <param name="procName">stored procedure name</param>
        /// <param name="parms">parameter collection</param>
        /// <param name="appName">Application name</param>
        /// <param name="environment">environment specified in config file</param>
        /// <returns>ADO.Net dataset</returns>
        public static DataSet GetDataSet(string procName,
            IEnumerable<IDbDataParameter> parms, string appName,
            APPENV environment)
        { return GetDataSet(procName, parms,
                    ConnectionString.GetConnSpec(appName, environment),
                    DEFTIMEOUT); }

        /// <summary>
        /// Create and return DataSet using specified stored proc
        /// Connects to database as specified in configuration file for Company/Appname/Environment
        /// </summary>
        /// <param name="procName">stored procedure name</param>
        /// <param name="parms">parameter collection</param>
        /// <param name="company">Company Name specified in config file</param>
        /// <param name="appName">Application name specified in config file</param>
        /// <param name="environment">environment specified in config file</param>
        /// <returns>ADO.Net dataset</returns>
        public static DataSet GetDataSet(string procName,
            IEnumerable<IDbDataParameter> parms, 
            string company, string appName,
            APPENV environment)
        { return GetDataSet(procName, parms,
              ConnectionString.GetConnSpec(company, appName, environment), 
              DEFTIMEOUT); }

        /// <summary>
        /// Create and return DataSet using specified stored proc
        /// Connects to database as specified in configuration file for Company/Appname/Environment
        /// </summary>
        /// <param name="procName">stored procedure name</param>
        /// <param name="parms">parameter collection</param>
        /// <param name="timeOut">execution timeout in seconds</param>
        /// <param name="company">Company Name specified in config file</param>
        /// <param name="appName">Application name specified in config file</param>
        /// <param name="environment">environment specified in config file</param>
        /// <returns>ADO.Net dataset</returns>
        public static DataSet GetDataSet(string procName,
            IEnumerable<IDbDataParameter> parms, int timeOut,
            string company, string appName, APPENV environment)
        { return GetDataSet(procName, parms,
                ConnectionString.GetConnSpec(company, appName, environment), 
                timeOut); }

        /// <summary>
        /// Create and return DataSet using specified stored proc
        /// Connects to database using provided connection object
        /// </summary>
        /// <param name="procName">stored procedure name</param>
        /// <param name="parms">parameter collection</param>
        /// <param name="timeout">execution timeout in seconds</param>
        /// <param name="CoPConnection">CoP COnnection object</param>
        /// <returns>ADO.Net dataset</returns>
        public static DataSet GetDataSet(string procName,
            IEnumerable<IDbDataParameter> parms,
            CoPConnection CoPConnection, int timeout = DEFTIMEOUT)
        {
            var ds=new DataSet("DataSet");
            using (var cn = CoPConnection.Connection)
            {
                if (cn.State == ConnectionState.Closed) cn.Open();
                var da = AdapterFactory(procName, parms, cn, timeout);
                da.Fill(ds);
                return (ds);
            }
        }

        #endregion GetDataset from Stored Proc overloads
        #endregion public GetdataSet Overloads

        #region internal GetdataSet Overloads
        internal static void GetDataSet(ref DataSet ds,
            string sql, string tableName, ConnectionSpec ConnSpec)
        {
            string sTNNdx;
            if (String.IsNullOrEmpty(tableName))
                tableName = "Table" + ds.Tables.Count;
            else
                while (ds.Tables.Contains(tableName))
                    if (lib.IsInteger(sTNNdx = tableName.Substring(tableName.Length - 3)))
                        tableName = tableName.Replace(sTNNdx, ((Int32.Parse(sTNNdx)) + 1).ToString());
                    else
                        tableName += "0001";
            // ------------------------------
            var dt = GetDataTable(sql, ConnSpec, tableName);
            ds.Tables.Add(dt);
        }

        internal static DataSet GetDataSet(string sql, ConnectionSpec ConnSpec)
        {
            using (var cn=ConnectionFactory(ConnSpec))
            {
                if (cn.State == ConnectionState.Closed) cn.Open();
                var ds = new DataSet("DataSet");
                var da=AdapterFactory(sql, cn);
                da.Fill(ds);
                return ds;
            }
        }

        internal static DataSet GetDataSet(string procName,
            IEnumerable<IDbDataParameter> parms, ConnectionSpec ConnSpec,
            int timeOut)
        {
            using (var cn = ConnectionFactory(ConnSpec))
            {
                if (cn.State == ConnectionState.Closed) cn.Open();
                var ds = new DataSet("dataset");
                var da = AdapterFactory(procName, parms, cn, timeOut);
                da.Fill(ds);
                return (ds);
            }
        }
        internal static DataSet GetDataSet(string sql, 
            CoPConnection CoPConnection)
        {
            using (var cn = CoPConnection.Connection)
            {
                if (cn.State == ConnectionState.Closed) cn.Open();
                var ds = new DataSet("DS");
                var da = AdapterFactory(sql, cn);
                da.Fill(ds);
                return ds;
            }
        }
        #endregion internal GetdataSet Overloads
        #endregion DataSet methods

        #region DataTable Methods
        #region public GetDataTable overloads
        #region Get DataTable based on SQL Statements
        /// <summary>
        /// Create and return DataTable using specified sql Statement
        /// Connects to specified database using integrated security
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a result set}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <returns>DataTable containing requested data</returns>       
        public static DataTable GetDataTable(string sql,
            string Server, string Database)
        {
            return GetDataTable(sql,
                ConnectionString.GetConnSpec(Server, Database),
                null, DEFTIMEOUT);
        }
        /// <summary>
        /// Create and return DataTable using specified sql Statement
        /// Connects to specified database using integrated security
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a result set}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <param name="port">Port number if not 1433</param>
        /// <returns>DataTable containing requested data</returns>       
        public static DataTable GetDataTable(string sql,
            string Server, string Database, int? port)
        {
            return GetDataTable(sql,
                    ConnectionString.GetConnSpec(Server, Database, port),
                    null, DEFTIMEOUT);
        }
        /// <summary>
        /// Create and return DataTable using specified sql Statement
        /// Connects to database specified by Configuration settings 
        /// based on specified parameters
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a result set}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <param name="UserID">logon or User Id to use in connectiong to database</param>
        /// <param name="Password">Clear text password to use in connectiong to database</param>
        /// <returns>DataTable containing requested data</returns>
        public static DataTable GetDataTable(string sql,
            string Server, string Database,
            string UserID, string Password)
        {
            return GetDataTable(sql,
                  ConnectionString.GetConnSpec(Server, Database,
                  UserID, Password), null, DEFTIMEOUT);
        }
        /// <summary>
        /// Create and return DataTable using specified sql Statement
        /// Connects to database specified by Configuration settings 
        /// based on specified parameters
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a result set}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <param name="port">Port number if not 1433</param>
        /// <param name="UserID">logon or User Id to use in connectiong to database</param>
        /// <param name="Password">Clear text password to use in connectiong to database</param>
        /// <returns>DataTable containing requested data</returns>
        public static DataTable GetDataTable(string sql,
            string Server, string Database, int? port,
            string UserID, string Password)
        {
            return GetDataTable(sql,
                  ConnectionString.GetConnSpec(Server, Database, 
                                port,UserID, Password), 
                  DEFTIMEOUT);
        }
        /// <summary>
        /// Create and return DataTable using specified sql Statement
        /// Connects to database specified by Configuration settings 
        /// based on specified parameters
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a result set}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <param name="UserID">logon or User Id to use in connectiong to database</param>
        /// <param name="Password">Clear text password to use in connectiong to database</param>
        /// <param name="timeOut">optional ADO.Net timeout setting {Defaults to 45 secs}</param>
        /// <returns>DataTable containing requested data</returns>
        public static DataTable GetDataTable(string sql,
            string Server, string Database, 
            string UserID, string Password, 
            int timeOut = DEFTIMEOUT)
        {
            return GetDataTable(sql,
                ConnectionString.GetConnSpec(Server, Database,
                UserID, Password), null, timeOut);
        }
        /// <summary>
        /// Create and return DataTable using specified sql Statement
        /// Connects to database specified by Configuration settings 
        /// based on specified parameters
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a result set}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <param name="port">Port number if not 1433</param>
        /// <param name="UserID">logon or User Id to use in connectiong to database</param>
        /// <param name="Password">Clear text password to use in connectiong to database</param>
        /// <param name="timeOut">optional ADO.Net timeout setting {Defaults to 45 secs}</param>
        /// <returns>DataTable containing requested data</returns>
        public static DataTable GetDataTable(string sql,
            string Server, string Database, int? port,
            string UserID, string Password, 
            int timeOut = DEFTIMEOUT)
        {
            return GetDataTable(sql,
                ConnectionString.GetConnSpec(Server, Database, 
                                port,UserID, Password),
                null, timeOut);
        }
        /// <summary>
        /// Create and return DataTable using specified sql Statement
        /// Connects to database specified by Configuration settings 
        /// based on specified parameters
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a result set}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <param name="UserID">logon or User Id to use in connectiong to database</param>
        /// <param name="Password">Clear text password to use in connectiong to database</param>
        /// <param name="tableName">optional name to assign to database object</param>
        /// <returns>DataTable containing requested data</returns>
        public static DataTable GetDataTable(string sql,
            string Server, string Database,
            string UserID, string Password,
            string tableName)
        {
            return GetDataTable(sql,
                ConnectionString.GetConnSpec(Server, Database,
                UserID, Password), tableName, DEFTIMEOUT);
        }
        /// <summary>
        /// Create and return DataTable using specified sql Statement
        /// Connects to database specified by Configuration settings 
        /// based on specified parameters
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a result set}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <param name="port">Port number if not 1433</param>
        /// <param name="UserID">logon or User Id to use in connectiong to database</param>
        /// <param name="Password">Clear text password to use in connectiong to database</param>
        /// <param name="tableName">optional name to assign to database object</param>
        /// <returns>DataTable containing requested data</returns>
        public static DataTable GetDataTable(string sql,
            string Server, string Database, int? port, 
            string UserID, string Password,
            string tableName)
        {
            return GetDataTable(sql,
                ConnectionString.GetConnSpec(Server, Database,
                                port, UserID, Password),
                tableName, DEFTIMEOUT);
        }
        /// <summary>
        /// Create and return DataTable using specified sql Statement
        /// Connects to database specified by Configuration settings 
        /// based on specified parameters
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a result set}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <param name="UserID">logon or User Id to use in connectiong to database</param>
        /// <param name="Password">Clear text password to use in connectiong to database</param>
        /// <param name="tableName">optional name to assign to database object</param>
        /// <param name="timeOut">optional ADO.Net timeout setting {Defaults to 45 secs}</param>
        /// <returns>DataTable containing requested data</returns>
        public static DataTable GetDataTable(string sql,
            string Server, string Database,
            string UserID, string Password,
            string tableName, int timeOut = DEFTIMEOUT)
        {
            return GetDataTable(sql,
                  ConnectionString.GetConnSpec(Server, Database,
                  UserID, Password), tableName, timeOut);
        }
        /// <summary>
        /// Create and return DataTable using specified sql Statement
        /// Connects to database specified by Configuration settings 
        /// based on specified parameters
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a result set}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <param name="port">Port number, if not 1433</param>
        /// <param name="UserID">logon or User Id to use in connectiong to database</param>
        /// <param name="Password">Clear text password to use in connectiong to database</param>
        /// <param name="tableName">optional name to assign to database object</param>
        /// <param name="timeOut">optional ADO.Net timeout setting {Defaults to 45 secs}</param>
        /// <returns>DataTable containing requested data</returns>
        public static DataTable GetDataTable(string sql,
            string Server, string Database, int? port,
            string UserID, string Password,
            string tableName, int timeOut = DEFTIMEOUT)
        {
            return GetDataTable(sql,
                  ConnectionString.GetConnSpec(Server, Database,
                                    port, UserID, Password), 
                  tableName, timeOut);
        }

        /// <summary>
        /// Create and return DataTable using specified sql Statement
        /// Connects to database specified by Configuration settings 
        /// based on specified Application Name and Environment
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a result set}</param>
        /// <param name="AppName">Application name specifying Database to connect to</param>
        /// <param name="Environment">Specifies environment instance of database to connect</param>
        /// <returns>DataTable containing requested data</returns>
        public static DataTable GetDataTable(
            string sql, string AppName,
            APPENV Environment)
        { return GetDataTable(sql, COMPANY,
                   AppName, Environment); }
        /// <summary>
        /// Create and return DataTable using specified sql Statement
        /// Connects to database specified by Configuration settings 
        /// based on specified Company Name, Application Name and Environment
        /// as delineated in applications connections 
        /// configuration file  {DbConnSpecs.config}
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a result set}</param>
        /// <param name="Company">Specifies Company Name element in Application configuration file</param>
        /// <param name="AppName">Application name specifying Database to connect to</param>
        /// <param name="Environment">Specifies environment instance of database to connect</param>
        /// <returns>DataTable containing requested data</returns>
        public static DataTable GetDataTable(
            string sql, string Company,
            string AppName, APPENV Environment)
        { return GetDataTable(sql,
                  ConnectionString.GetConnSpec(Company, AppName,
                  Environment), null, DEFTIMEOUT); }
        /// <summary>
        /// Create and return DataTable using specified sql Statement
        /// Connects to database specified by Configuration settings 
        /// based on specified Company Name, Application Name and Environment
        /// as delineated in applications connections 
        /// configuration file  {DbConnSpecs.config}
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a result set}</param>
        /// <param name="Company">Specifies Company Name element in Application configuration file</param>
        /// <param name="AppName">Application name specifying Database to connect to</param>
        /// <param name="Environment">Specifies environment instance of database to connect</param>
        /// <param name="timeOut">optional ADO.Net timeout setting {Defaults to 45 secs}</param>
        /// <returns>DataTable containing requested data</returns>
        public static DataTable GetDataTable(
            string sql, string Company, string AppName,
            APPENV Environment, int timeOut)
        {
            return GetDataTable(sql,
                  ConnectionString.GetConnSpec(Company, AppName,
                  Environment), null, timeOut);
        }
        /// <summary>
        /// Create and return DataTable using specified sql Statement
        /// Connects to database specified by Configuration settings 
        /// based on specified Company Name, Application Name and Environment
        /// as delineated in applications connections 
        /// configuration file  {DbConnSpecs.config}
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a result set}</param>
        /// <param name="Company">Specifies Company Name element in Application configuration file</param>
        /// <param name="AppName">Application name specifying Database to connect to</param>
        /// <param name="Environment">Specifies environment instance of database to connect</param>
        /// <param name="tableName">optional name to assign to database object</param>
        /// <returns>DataTable containing requested data</returns>
        public static DataTable GetDataTable(
            string sql, string Company, string AppName,
            APPENV Environment, string tableName)
        {
            return GetDataTable(sql,
                   ConnectionString.GetConnSpec(Company, AppName, Environment),
                   tableName, DEFTIMEOUT);
        }
        /// <summary>
        /// Create and return DataTable using specified sql Statement
        /// Connects to database specified by Configuration settings 
        /// based on specified Company Name, Application Name and Environment
        /// as delineated in applications connections 
        /// configuration file  {DbConnSpecs.config}
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a result set}</param>
        /// <param name="Company">Specifies Company Name element in Application configuration file</param>
        /// <param name="AppName">Application name specifying Database to connect to</param>
        /// <param name="Environment">Specifies environment instance of database to connect</param>
        /// <param name="tableName">optional name to assign to database object</param>
        /// <param name="timeOut">optional ADO.Net timeout setting {Defaults to 45 secs}</param>
        /// <returns>DataTable containing requested data</returns>
        public static DataTable GetDataTable(
            string sql, string Company, string AppName,
            APPENV Environment, string tableName, int timeOut)
        {
            return GetDataTable(sql,
                  ConnectionString.GetConnSpec(Company, AppName, Environment),
                  tableName, timeOut);
        }
        /// <summary>
        /// Create and return DataTable using specified sql Statement
        /// using connection provided by CoPConnection object
        /// </summary>
        /// <param name="sql">sql Statment to execute {Should be a select statement or 
        /// stored procedure that returns a result set}</param>
        /// <param name="CoPConnection">CoPConnection object - 
        /// (wrapper for SqlConnection or OracleConnection)</param>
        /// <returns>DataTable containing requested data</returns>
        public static DataTable GetDataTable(string sql, CoPConnection CoPConnection)
        { return GetDataTable(sql, DEFTIMEOUT, null, CoPConnection); }

        /// <summary>
        /// Create and return DataTable using specified sql Statement
        /// using connection provided by CoPConnection object
        /// </summary>
        /// <param name="sql">sql Statment to execute {Should be a select statement or 
        /// stored procedure that returns a result set}</param>
        /// <param name="tableName">optional name to assign to database table object</param>
        /// <param name="CoPConnection">CoPConnection object - 
        /// (wrapper for SqlConnection or OracleConnection)</param>
        /// <returns>DataTable containing requested data</returns>
        public static DataTable GetDataTable(string sql,
            string tableName, CoPConnection CoPConnection)
        { return GetDataTable(sql, DEFTIMEOUT, tableName, CoPConnection); }
        /// <summary>
        /// Create and return DataTable using specified sql Statement
        /// using connection provided by CoPConnection object
        /// </summary>
        /// <param name="sql">sql Statment to execute {Should be a select statement or 
        /// stored procedure that returns a result set}</param>
        /// <param name="timeout">optional ADO.Net timeout setting {Defaults to 45 secs}</param>
        /// <param name="tableName">optional name to assign to database object</param>
        /// <param name="CoPConnection">CoPConnection object - 
        /// (wrapper for SqlConnection or OracleConnection)</param>
        /// <returns>DataTable containing requested data</returns>
        public static DataTable GetDataTable(string sql, int timeout,
            string tableName, CoPConnection CoPConnection)
        {
            using (var cn = CoPConnection.Connection)
            {
                if (cn.State == ConnectionState.Closed) cn.Open();
                var dt = String.IsNullOrEmpty(tableName) ?
                        new DataTable("Table"): new DataTable(tableName);
                var da = AdapterFactory(sql, cn, null, timeout);
                da.Fill(dt);
                //if (CoPConnection.HasTransaction) CoPConnection.IsInTransaction=true;
                return (dt);
            }
        }

        #endregion Get DataTable based on SQL Statements

        #region stored procs Returning DataTable
        public static DataTable GetDataTable(string procName,
            IEnumerable<IDbDataParameter> parms, string server, string database)
        {
            return GetDataTable(procName, parms,
            ConnectionString.GetConnSpec(server, database),
            null, DEFTIMEOUT);
        }
        public static DataTable GetDataTable(string procName,
            IEnumerable<IDbDataParameter> parms,
            string server, string database,
            string userId, string password)
        {
            return GetDataTable(procName, parms,
              ConnectionString.GetConnSpec(server, database, userId, password),
              null, DEFTIMEOUT);
        }
        public static DataTable GetDataTable(string procName,
            IEnumerable<IDbDataParameter> parms, int timeOut,
            string server, string database,
            string userId, string password)
        {
            return GetDataTable(procName, parms,
              ConnectionString.GetConnSpec(server, database, userId, password),
              null, timeOut);
        }
        public static DataTable GetDataTable(string procName,
            IEnumerable<IDbDataParameter> parms, string tableName,
            string server, string database, string userId, string password)
        {
            return GetDataTable(procName, parms,
                ConnectionString.GetConnSpec(server, database, userId, password),
                tableName, DEFTIMEOUT);
        }
        public static DataTable GetDataTable(string procName,
            IEnumerable<IDbDataParameter> parms, string tableName, int timeOut,
            string server, string database, string userId, string password)
        {
            return GetDataTable(procName, parms,
                  ConnectionString.GetConnSpec(server, database, userId, password),
                  tableName, timeOut);
        }
        public static DataTable GetDataTable(string procName,
            IEnumerable<IDbDataParameter> parms, string appName,
            APPENV environment)
        {
            return GetDataTable(procName, parms,
                    ConnectionString.GetConnSpec(appName, environment),
                    null, DEFTIMEOUT);
        }
        public static DataTable GetDataTable(string procName,
            IEnumerable<IDbDataParameter> parms, string company, string appName,
            APPENV environment)
        {
            if (parms == null) throw new ArgumentNullException("parms");
            return GetDataTable(procName, parms,
              ConnectionString.GetConnSpec(company, appName, environment),
              null, DEFTIMEOUT);
        }

        public static DataTable GetDataTable(string procName,
            IEnumerable<IDbDataParameter> parms, string tableName, int timeOut,
            string company, string appName, APPENV environment)
        {
            return GetDataTable(procName, parms,
                ConnectionString.GetConnSpec(company, appName, environment),
                tableName, timeOut);
        }
        public static DataTable GetDataTable(string procName,
            IEnumerable<IDbDataParameter> parms,
            CoPConnection CoPConnection, int timeOut = DEFTIMEOUT)
        { return GetDataTable(procName, parms, timeOut, null, CoPConnection); }

        public static DataTable GetDataTable(string procName,
            IEnumerable<IDbDataParameter> parms, int timeout, 
            string tableName, CoPConnection CoPConnection)
        {
            using (var cn = CoPConnection.Connection)
            {
                if(cn.State == ConnectionState.Closed) cn.Open();
                var dt = string.IsNullOrEmpty(tableName)? 
                             new DataTable("Table"): new DataTable(tableName);
                var da = AdapterFactory(procName, parms, cn, timeout);
                da.Fill(dt);
                return (dt);
            }
        }

        #endregion stored procs Returning DataTable

        #endregion public GetDataTable overloads

        #region internal overloads
        internal static DataTable GetDataTable(string sql,
            ConnectionSpec ConnSpec, int timeOut)
        { return (GetDataTable(sql, null, ConnSpec, null, timeOut)); }
        // -------------------------------------
        internal static DataTable GetDataTable(
            string sql, ConnectionSpec ConnSpec, string tableName)
        { return (GetDataTable(sql, null, ConnSpec, tableName, DEFTIMEOUT)); }
        internal static DataTable GetDataTable(
            string sql, ConnectionSpec ConnSpec)
        { return (GetDataTable(sql, ConnSpec, null)); }
        // -------------------------------------
        internal static DataTable GetDataTable(string sql,
            ConnectionSpec ConnSpec, string tableName, int timeOut)
        { return (GetDataTable(sql, null, ConnSpec, tableName, timeOut)); }
        internal static DataTable GetDataTable(string sql,
            IEnumerable<IDbDataParameter> parms, ConnectionSpec ConnSpec,
            string tableName, int timeOut)
        {
            using (var cn = ConnectionFactory(ConnSpec))
            {
                if (cn.State == ConnectionState.Closed) cn.Open();
                var dt = string.IsNullOrEmpty(tableName) ?
                    new DataTable("Table") : new DataTable(tableName);
                var da = AdapterFactory(sql, parms, cn, timeOut);
                da.Fill(dt);
                return (dt);
            }
        }
        #endregion internal overloads
        #endregion DataTable Methods

        #region DataReader Methods
        #region public GetdataReader Overloads
        /// <summary>
        /// Create and return SqlDataReader or OracleReader using specified sql Statement
        /// Connects to database specified by Configuration settings 
        /// based on specified parameters
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a result set}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <param name="UserID">logon or User Id to use in connectiong to database</param>
        /// <param name="Password">Clear text password to use in connectiong to database</param>
        /// <returns>A Database specific derived class of DBReader</returns>
        public static IDataReader GetDataReader(string sql,
            string Server, string Database,
            string UserID, string Password)
        {
            return GetDataReader(sql,
                   ConnectionString.GetConnSpec(Server, Database, 
                                            UserID, Password),
                   closeConn, DEFTIMEOUT);
        }
        /// <summary>
        /// Create and return SqlDataReader or OracleReader using specified sql Statement
        /// Connects to database specified by Configuration settings 
        /// based on specified parameters
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a result set}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <param name="port">Port number, if not 1433</param>
        /// <param name="UserID">logon or User Id to use in connectiong to database</param>
        /// <param name="Password">Clear text password to use in connectiong to database</param>
        /// <returns>A Database specific derived class of DBReader</returns>
        public static IDataReader GetDataReader(string sql,
            string Server, string Database, int? port,
            string UserID, string Password)
        {
            return GetDataReader(sql,
                   ConnectionString.GetConnSpec(Server, Database,
                                            port, UserID, Password),
                   closeConn, DEFTIMEOUT);
        }
        /// <summary>
        /// Create and return SqlDataReader or OracleReader using specified sql Statement
        /// Connects to database specified by Configuration settings 
        /// using integrated security
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a result set}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <returns>A Database specific derived class of DBReader</returns>
        public static IDataReader GetDataReader(string sql,
            string Server, string Database)
        {
            return GetDataReader(sql,
                   ConnectionString.GetConnSpec(Server, Database),
                   closeConn, DEFTIMEOUT);
        }
        /// <summary>
        /// Create and return SqlDataReader or OracleReader using specified sql Statement
        /// Connects to database specified by Configuration settings 
        /// using integrated security
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a result set}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <param name="port">Port number, if not 1433</param>
        /// <returns>A Database specific derived class of DBReader</returns>
        public static IDataReader GetDataReader(string sql,
            string Server, string Database, int? port)
        {
            return GetDataReader(sql,
                   ConnectionString.GetConnSpec(Server, Database, port),
                   closeConn, DEFTIMEOUT);
        }
        /// <summary>
        /// Create and return SqlDataReader or OracleReader using specified sql Statement
        /// Connects to database specified by Configuration settings 
        /// using specified UserId and password 
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a result set}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <param name="UserID">logon or User Id to use in connectiong to database</param>
        /// <param name="Password">Clear text password to use in connectiong to database</param>
        /// <param name="timeOut">optional ADO.Net timeout setting {Defaults to 45 secs}</param>
        /// <returns>A Database specific derived class of DBReader</returns>
        public static IDataReader GetDataReader(string sql,
            string Server, string Database,
            string UserID, string Password, int timeOut)
        {
            return GetDataReader(sql,
                   ConnectionString.GetConnSpec(Server, 
                            Database, UserID, Password),
           closeConn, timeOut);
        }
        /// <summary>
        /// Create and return SqlDataReader or OracleReader using specified sql Statement
        /// Connects to database specified by Configuration settings 
        /// using specified UserId and password 
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a result set}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <param name="port">Port number, if not 1433</param>
        /// <param name="UserID">logon or User Id to use in connectiong to database</param>
        /// <param name="Password">Clear text password to use in connectiong to database</param>
        /// <param name="timeOut">optional ADO.Net timeout setting {Defaults to 45 secs}</param>
        /// <returns>A Database specific derived class of DBReader</returns>
        public static IDataReader GetDataReader(string sql,
            string Server, string Database, int? port,
            string UserID, string Password, int timeOut)
        {
            return GetDataReader(sql,
                   ConnectionString.GetConnSpec(Server,
                            Database, port, UserID, Password),
           closeConn, timeOut);
        }
        /// <summary>
        /// Create and return SqlDataReader or OracleReader using specified sql Statement
        /// Connects to database specified by Configuration settings 
        /// using integrated security
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a result set}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <param name="timeOut">optional ADO.Net timeout setting {Defaults to 45 secs}</param>
        /// <returns>A Database specific derived class of DBReader</returns>
        public static IDataReader GetDataReader(string sql,
            string Server, string Database, int timeOut)
        {
            return GetDataReader(sql,
                   ConnectionString.GetConnSpec(Server, Database),
                   closeConn, timeOut);
        }
        /// <summary>
        /// Create and return SqlDataReader or OracleReader using specified sql Statement
        /// Connects to database specified by Configuration settings 
        /// using integrated security
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a result set}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <param name="port">Port number, if not 1433</param>
        /// <param name="timeOut">optional ADO.Net timeout setting {Defaults to 45 secs}</param>
        /// <returns>A Database specific derived class of DBReader</returns>
        public static IDataReader GetDataReader(string sql,
            string Server, string Database, int? port, int timeOut)
        {
            return GetDataReader(sql,
                   ConnectionString.GetConnSpec(Server, Database, port),
                   closeConn, timeOut);
        }
        /// <summary>
        /// Create and return SqlDataReader or OracleReader using specified sql Statement
        /// Connects to database specified by Configuration settings 
        /// using integrated security
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a result set}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <param name="UserID">logon or User Id to use in connectiong to database</param>
        /// <param name="Password">Clear text password to use in connectiong to database</param>
        /// <param name="CmdBehavior">CommandBehavior enumeration, 
        /// specifying what to do with connection object when 
        /// finished running sql statement {Defaults to 'CloseConnection'}</param>
        /// <returns>A Database specific derived class of DBReader</returns>       
        public static IDataReader GetDataReader(string sql,
            string Server, string Database,
            string UserID, string Password,
            CommandBehavior CmdBehavior)
        {
            return GetDataReader(sql,
                   ConnectionString.GetConnSpec(Server, Database,
                   UserID, Password),
                   CmdBehavior, DEFTIMEOUT);
        }
        /// <summary>
        /// Create and return SqlDataReader or OracleReader using specified sql Statement
        /// Connects to database specified by Configuration settings 
        /// using integrated security
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a result set}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <param name="port">Port number, if not 1433</param>
        /// <param name="UserID">logon or User Id to use in connectiong to database</param>
        /// <param name="Password">Clear text password to use in connectiong to database</param>
        /// <param name="CmdBehavior">CommandBehavior enumeration, 
        /// specifying what to do with connection object when 
        /// finished running sql statement {Defaults to 'CloseConnection'}</param>
        /// <returns>A Database specific derived class of DBReader</returns>       
        public static IDataReader GetDataReader(string sql,
            string Server, string Database, int? port,
            string UserID, string Password,
            CommandBehavior CmdBehavior)
        {
            return GetDataReader(sql,
                   ConnectionString.GetConnSpec(Server, Database,
                                port, UserID, Password),
                   CmdBehavior, DEFTIMEOUT);
        }        
        /// <summary>
        /// Create and return SqlDataReader or OracleReader using specified sql Statement
        /// Connects to database specified by Configuration settings 
        /// using integrated security
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a result set}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <param name="UserID">logon or User Id to use in connectiong to database</param>
        /// <param name="Password">Clear text password to use in connectiong to database</param>
        /// <param name="CmdBehavior">CommandBehavior enumeration, 
        /// specifying what to do with connection object when 
        /// finished running sql statement {Defaults to 'CloseConnection'}</param>
        /// <param name="timeOut">optional ADO.Net timeout setting {Defaults to 45 secs}</param>
        /// <returns>A Database specific derived class of DBReader</returns>       
        public static IDataReader GetDataReader(string sql,
            string Server, string Database,
            string UserID, string Password,
            CommandBehavior CmdBehavior, int timeOut)
        {
            return GetDataReader(sql,
                   ConnectionString.GetConnSpec(Server, Database,
                       UserID, Password),
                   CmdBehavior, timeOut);
        }
        /// <summary>
        /// Create and return SqlDataReader or OracleReader using specified sql Statement
        /// Connects to database specified by Configuration settings 
        /// using integrated security
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a result set}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <param name="port">Port number, if not 1433</param>
        /// <param name="UserID">logon or User Id to use in connectiong to database</param>
        /// <param name="Password">Clear text password to use in connectiong to database</param>
        /// <param name="CmdBehavior">CommandBehavior enumeration, 
        /// specifying what to do with connection object when 
        /// finished running sql statement {Defaults to 'CloseConnection'}</param>
        /// <param name="timeOut">optional ADO.Net timeout setting {Defaults to 45 secs}</param>
        /// <returns>A Database specific derived class of DBReader</returns>       
        public static IDataReader GetDataReader(string sql,
            string Server, string Database, int? port,
            string UserID, string Password,
            CommandBehavior CmdBehavior, int timeOut)
        {
            return GetDataReader(sql,
                   ConnectionString.GetConnSpec(Server, Database,
                                        port, UserID, Password),
                   CmdBehavior, timeOut);
        }
        
        /// <summary>
        /// Create and return SqlDataReader or OracleReader using specified sql Statement
        /// Connects to database specified by Configuration settings 
        /// using integrated security
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a result set}</param>
        /// <param name="AppName">Application name specifying Database to connect to</param>
        /// <param name="Environment">Specifies environment instance of database to connect</param>
        /// <returns>A Database specific derived class of DBReader</returns>
        public static IDataReader GetDataReader(
            string sql, string AppName, APPENV Environment)
        {
            return GetDataReader(sql,
                      ConnectionString.GetConnSpec(COMPANY, AppName, Environment),
                      closeConn, DEFTIMEOUT);
        }
        /// <summary>
        /// Create and return SqlDataReader or OracleReader using specified sql Statement
        /// Connects to database specified by Configuration settings 
        /// using integrated security
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a result set}</param>
        /// <param name="Company">Specifies Company Name element in Application configuration file</param>
        /// <param name="AppName">Application name specifying Database to connect to</param>
        /// <param name="Environment">Specifies environment instance of database to connect</param>
        /// <returns>A Database specific derived class of DBReader</returns>       
        public static IDataReader GetDataReader(
            string sql, string Company,
            string AppName, APPENV Environment)
        {
            return GetDataReader(sql,
                   ConnectionString.GetConnSpec(Company, AppName, Environment),
                   closeConn, DEFTIMEOUT);
        }
        /// <summary>
        /// Create and return SqlDataReader or OracleReader using specified sql Statement
        /// Connects to database specified by Configuration settings 
        /// using integrated security
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a result set}</param>
        /// <param name="Company">Specifies Company Name element in Application configuration file</param>
        /// <param name="AppName">Application name specifying Database to connect to</param>
        /// <param name="Environment">Specifies environment instance of database to connect</param>
        /// <param name="timeOut">optional ADO.Net timeout setting {Defaults to 45 secs}</param>
        /// <returns>A Database specific derived class of DBReader</returns>       
        public static IDataReader GetDataReader(
            string sql, string Company, string AppName,
            APPENV Environment, int timeOut)
        {
            return GetDataReader(sql,
                  ConnectionString.GetConnSpec(Company, AppName, Environment),
                  closeConn, timeOut);
        }
        /// <summary>
        /// Create and return SqlDataReader or OracleReader using specified sql Statement
        /// Connects to database specified by Configuration settings 
        /// using integrated security
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a result set}</param>
        /// <param name="Company">Specifies Company Name element in Application configuration file</param>
        /// <param name="AppName">Application name specifying Database to connect to</param>
        /// <param name="Environment">Specifies environment instance of database to connect</param>
        /// <param name="CmdBehavior">CommandBehavior enumeration, 
        /// specifying what to do with connection object when 
        /// finished running sql statement {Defaults to 'CloseConnection'}</param>
        /// <returns>A Database specific derived class of DBReader</returns>       
        public static IDataReader GetDataReader(
            string sql, string Company,
            string AppName, APPENV Environment,
            CommandBehavior CmdBehavior)
        {
            return GetDataReader(sql,
                   ConnectionString.GetConnSpec(Company, AppName, Environment),
                   CmdBehavior, DEFTIMEOUT);
        }
        /// <summary>
        /// Create and return SqlDataReader or OracleReader using specified sql Statement
        /// Connects to database specified by Configuration settings 
        /// using integrated security
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a result set}</param>
        /// <param name="Company">Specifies Company Name element in Application configuration file</param>
        /// <param name="AppName">Application name specifying Database to connect to</param>
        /// <param name="Environment">Specifies environment instance of database to connect</param>
        /// <param name="CmdBehavior">CommandBehavior enumeration, 
        /// specifying what to do with connection object when 
        /// finished running sql statement {Defaults to 'CloseConnection'}</param>
        /// <param name="timeOut">optional ADO.Net timeout setting {Defaults to 45 secs}</param>
        /// <returns>A Database specific derived class of DBReader</returns>       
        public static IDataReader GetDataReader(
            string sql, string Company,
            string AppName, APPENV Environment,
            CommandBehavior CmdBehavior, int timeOut)
        {
            return GetDataReader(sql,
                ConnectionString.GetConnSpec(Company, AppName, Environment),
                CmdBehavior, timeOut);
        }
        /// <summary>
        /// Create and return SqlDataReader or OracleReader using specified sql Statement
        /// and provided connection object
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a result set}</param>
        /// <param name="CoPConnection">CoPConnection object - 
        /// (wrapper for SqlConnection or OracleConnection)</param>
        /// <returns>A Database specific derived class of DBReader</returns>
        public static IDataReader GetDataReader(
            string sql, CoPConnection CoPConnection)
        { return GetDataReader(sql, DEFTIMEOUT, CoPConnection); }
        /// <summary>
        /// Create and return SqlDataReader or OracleReader using specified sql Statement
        /// and provided connection object
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a result set}</param>
        /// <param name="timeOut">optional ADO.Net timeout setting {Defaults to 45 secs}</param>
        /// <param name="CoPConnection">CoPConnection object - 
        /// (wrapper for SqlConnection or OracleConnection)</param>
        /// <returns>A Database specific derived class of DBReader</returns>
        public static IDataReader GetDataReader(string sql,
            int timeOut, CoPConnection CoPConnection)
        {
            using (var sc = CoPConnection.Connection)
            {
                if (sc.State == ConnectionState.Closed) sc.Open();
                var cmd = CommandFactory(sql, sc);
                cmd.CommandTimeout = timeOut;
                cmd.CommandType = CommandType.Text;
                return (cmd.ExecuteReader(CommandBehavior.Default));
            }
        }
        #endregion public GetDataReader Overloads

        #region internal GetDataReader Overloads
        internal static IDataReader GetDataReader(
            string sql, ConnectionSpec ConnSpec)
        {
            return (GetDataReader(sql, ConnSpec,
                closeConn, DEFTIMEOUT));
        }
        internal static IDataReader GetDataReader(
            string sql, ConnectionSpec ConnSpec, int timeOut)
        {
            return (GetDataReader(sql, ConnSpec,
                   closeConn, timeOut));
        }
        internal static IDataReader GetDataReader(
            string sql, ConnectionSpec ConnSpec,
            CommandBehavior CmdBehavior)
        {
            return (GetDataReader(sql, ConnSpec,
                          CmdBehavior, DEFTIMEOUT));
        }
        internal static IDataReader GetDataReader(
            string sql, ConnectionSpec ConnSpec,
            CommandBehavior CmdBehavior, int timeOut)
        {
            using(var sc = ConnectionFactory(ConnSpec))
            {
                sc.Open();
                var cmd = CommandFactory(sql, sc);
                cmd.CommandTimeout = timeOut;
                cmd.CommandType = CommandType.Text;
                return (cmd.ExecuteReader(CmdBehavior));
            }
        }
        #endregion internal GetDataReader Overloads
        #endregion DataReader Methods

        #region SQLScalar methods
        #region Public GetSQLScalar Overloads
        /// <summary>
        /// Executes sql Statement and returns first row, column value as scalar
        /// connects to specified database using integrated security
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a 
        /// single row, single column result set}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <returns>scalar value of first row, first column of generated Resultset as [Object]</returns>
        public static object GetSQLScalar(string sql,
            string Server, string Database)
        { return GetSQLScalar(sql, ConnectionString.GetConnSpec(Server, Database), DEFTIMEOUT); }
        /// <summary>
        /// Executes sql Statement and returns first row, column value as scalar
        /// connects to specified database using integrated security
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a 
        /// single row, single column result set}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <param name="port">Port number, if not 1433</param>
        /// <returns>scalar value of first row, first column of 
        /// generated Resultset as [Object]</returns>
        public static object GetSQLScalar(string sql,
            string Server, string Database, int? port)
        { return GetSQLScalar(sql, 
            ConnectionString.GetConnSpec(Server, Database, port), 
            DEFTIMEOUT); }
        /// <summary>
        /// Executes sql Statement and returns first row, column value as scalar
        /// connects to specified database using integrated security
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a 
        /// single row, single column result set}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <param name="timeOut">optional ADO.Net timeout setting {Defaults to 45 secs}</param>
        /// <returns>scalar value of first row, first column of generated Resultset as [Object]</returns>
        public static object GetSQLScalar(string sql,
            string Server, string Database, int timeOut)
        { return GetSQLScalar(sql, ConnectionString.GetConnSpec(Server, Database), timeOut); }
        /// <summary>
        /// Executes sql Statement and returns first row, column value as scalar
        /// connects to specified database using integrated security
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a 
        /// single row, single column result set}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <param name="port">Port number, if not 1433</param>
        /// <param name="timeOut">optional ADO.Net timeout setting {Defaults to 45 secs}</param>
        /// <returns>scalar value of first row, first column of generated Resultset as [Object]</returns>
        public static object GetSQLScalar(string sql,
            string Server, string Database, int? port, int timeOut)
        { return GetSQLScalar(sql, 
                ConnectionString.GetConnSpec(Server, Database, port), 
               timeOut); }
        /// <summary>
        /// Executes sql Statement and returns first row, column value as scalar
        /// connects to specified database using specified userId and password
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a 
        /// single row, single column result set}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <param name="UserID">logon or User Id to use in connectiong to database</param>
        /// <param name="Password">Clear text password to use in connectiong to database</param>
        /// <returns>scalar value of first row, first column of generated Resultset as [Object]</returns>
        public static object GetSQLScalar(string sql,
            string Server, string Database,
            string UserID, string Password)
        {
            return GetSQLScalar(sql,
                   ConnectionString.GetConnSpec(Server, Database, UserID, Password),
                   DEFTIMEOUT);
        }
        /// <summary>
        /// Executes sql Statement and returns first row, column value as scalar
        /// connects to specified database using specified userId and password
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a 
        /// single row, single column result set}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <param name="port">Port number, if not 1433</param>
        /// <param name="UserID">logon or User Id to use in connectiong to database</param>
        /// <param name="Password">Clear text password to use in connectiong to database</param>
        /// <returns>scalar value of first row, first column of generated Resultset as [Object]</returns>
        public static object GetSQLScalar(string sql,
            string Server, string Database, int? port,
            string UserID, string Password)
        {
            return GetSQLScalar(sql,
                   ConnectionString.GetConnSpec(Server, Database, port, UserID, Password),
                   DEFTIMEOUT);
        }
        /// <summary>
        /// Executes sql Statement and returns first row, column value as scalar
        /// connects to specified database using specified userId and password
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a 
        /// single row, single column result set}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <param name="UserID">logon or User Id to use in connectiong to database</param>
        /// <param name="Password">Clear text password to use in connectiong to database</param>
        /// <param name="timeOut">optional ADO.Net timeout setting {Defaults to 45 secs}</param>
        /// <returns>scalar value of first row, first column of generated Resultset as [Object]</returns>
        public static object GetSQLScalar(string sql,
            string Server, string Database,
            string UserID, string Password, int timeOut)
        {
            return GetSQLScalar(sql,
                   ConnectionString.GetConnSpec(Server, Database, UserID, Password),
                   timeOut);
        }
        /// <summary>
        /// Executes sql Statement and returns first row, column value as scalar
        /// connects to specified database using specified userId and password
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a 
        /// single row, single column result set}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <param name="port">Port number, if not 1433</param>
        /// <param name="UserID">logon or User Id to use in connectiong to database</param>
        /// <param name="Password">Clear text password to use in connectiong to database</param>
        /// <param name="timeOut">optional ADO.Net timeout setting {Defaults to 45 secs}</param>
        /// <returns>scalar value of first row, first column of generated Resultset as [Object]</returns>
        public static object GetSQLScalar(string sql,
            string Server, string Database, int? port,
            string UserID, string Password, int timeOut)
        {
            return GetSQLScalar(sql,
                   ConnectionString.GetConnSpec(Server, Database, port, UserID, Password),
                   timeOut);
        }

        /// <summary>
        /// Executes sql Statement and returns first row, column value as scalar
        /// Connects to database using provided cconnection object
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a 
        /// single row, single column result set}</param>
        /// <param name="cn">Database Connection 
        /// {can be any connection object which derives from DbConnection}</param>
        /// <param name="tx">Local Transaction if one exists</param>
        /// <param name="timeOut">optional ADO.Net timeout setting {Defaults to 45 secs}</param>
        /// <returns>scalar value of first row, first column of generated Resultset as [Object]</returns>
        public static object GetSQLScalar(string sql,
            DbConnection cn, IDbTransaction tx = null, int timeOut = DEFTIMEOUT)
        {
            var closeConnection=false;
            if (cn.State != ConnectionState.Open)
            {
                cn.Open();
                closeConnection=true;
            }
            var cmd=CommandFactory(sql, cn, tx, timeOut);
            var obj=cmd.ExecuteScalar();
            if (closeConnection) cn.Close();
            return obj;
        }

        /// <summary>
        /// Executes sql Statement and returns first row, column value as scalar
        /// connects to database specified AppName and Environment 
        /// as delinieated in connections.config file
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a 
        /// single row, single column result set}</param>
        /// <param name="AppName">Application name specifying Database to connect to</param>
        /// <param name="Environment">Specifies environment instance of database to connect</param>
        /// <returns>scalar value of first row, first column of generated Resultset as [Object]</returns>
        public static object GetSQLScalar(string sql,
            string AppName, APPENV Environment)
        {
            return GetSQLScalar(sql,
              ConnectionString.GetConnSpec(COMPANY,
              AppName, Environment), DEFTIMEOUT);
        }
        /// <summary>
        /// Executes sql Statement and returns first row, column value as scalar
        /// connects to database using specified AppName and Environment 
        /// as delinieated in connections.config file
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a 
        /// single row, single column result set}</param>
        /// <param name="Company">Specifies Company Name element in Application configuration file</param>
        /// <param name="AppName">Application name specifying Database to connect to</param>
        /// <param name="Environment">Specifies environment instance of database to connect</param>
        /// <returns>scalar value of first row, first column of generated Resultset as [Object]</returns>
        public static object GetSQLScalar(string sql, string Company,
            string AppName, APPENV Environment)
        {
            return GetSQLScalar(sql,
              ConnectionString.GetConnSpec(Company,
              AppName, Environment), DEFTIMEOUT);
        }
        /// <summary>
        /// Executes sql Statement and returns first row, column value as scalar
        /// connects to specified database using integrated security
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a 
        /// single row, single column result set}</param>
        /// <param name="Company">Specifies Company Name element in Application configuration file</param>
        /// <param name="AppName">Application name specifying Database to connect to</param>
        /// <param name="Environment">Specifies environment instance of database to connect</param>
        /// <param name="timeOut">optional ADO.Net timeout setting {Defaults to 45 secs}</param>
        /// <returns>scalar value of first row, first column of generated Resultset as [Object]</returns>
        public static object GetSQLScalar(string sql, string Company,
            string AppName, APPENV Environment, int timeOut)
        {
            return GetSQLScalar(sql,
              ConnectionString.GetConnSpec(Company,
              AppName, Environment), timeOut);
        }
        /// <summary>
        /// Executes sql Statement and returns first row, column value as scalar
        /// using connection provided in CoPConnection object
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a 
        /// single row, single column result set}</param>
        /// <param name="CoPConnection">CoPConnection object - 
        /// (wrapper for SqlConnection or OracleConnection)</param>
        /// <returns>scalar value of first row, first column of generated Resultset as [Object]</returns>
        public static object GetSQLScalar(string sql, CoPConnection CoPConnection)
        { return GetSQLScalar(sql, DEFTIMEOUT, null, CoPConnection); }
        /// <summary>
        /// Executes sql Statement and returns first row, column value as scalar
        /// using connection provided in CoPConnection object
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a 
        /// single row, single column result set}</param>
        /// <param name="parms">optional array of DCoPrameters for Stored proc call</param>
        /// <param name="CoPConnection">CoPConnection object - 
        /// (wrapper for SqlConnection or OracleConnection)</param>
        /// <returns>scalar value of first row, first column of generated Resultset as [Object]</returns>
        public static object GetSQLScalar(string sql,
            IEnumerable<IDbDataParameter> parms, CoPConnection CoPConnection)
        { return GetSQLScalar(sql, DEFTIMEOUT, parms, CoPConnection); }
        /// <summary>
        /// Executes sql Statement and returns first row, column value as scalar
        /// using connection provided in CoPConnection object
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that returns a 
        /// single row, single column result set}</param>
        /// <param name="timeout">optional ADO.Net timeout setting {Defaults to 45 secs}</param>
        /// <param name="parms">optional array of DCoPrameters for Stored proc call</param>
        /// <param name="CoPConnection">CoPConnection object - 
        /// (wrapper for SqlConnection or OracleConnection)</param>
        /// <returns>scalar value of first row, first column of generated Resultset as [Object]</returns>
        public static object GetSQLScalar(string sql, int timeout,
            IEnumerable<IDbDataParameter> parms, CoPConnection CoPConnection)
        {
            using (var cn = CoPConnection.Connection)
            {
                if (cn.State == ConnectionState.Closed) cn.Open();
                var cmd = CommandFactory(sql, cn, parms, timeout);
                var obj = cmd.ExecuteScalar();
                if (cn.State == ConnectionState.Open) cn.Close();
                return obj;
            }
        }

        #endregion Public GetSQLScalar Overloads

        #region internal GetSQLScalar ovverloads
        internal static object GetSQLScalar(string sql, ConnectionSpec ConnSpec)
        { return (GetSQLScalar(sql, ConnSpec, DEFTIMEOUT)); }
        internal static object GetSQLScalar(string sql,
            ConnectionSpec ConnSpec, int timeOut)
        {
            using (var cn = ConnectionFactory(ConnSpec))
            {
                if (cn.State == ConnectionState.Closed) cn.Open();
                var cmd = CommandFactory(sql, cn, timeOut);
                var obj=cmd.ExecuteScalar();
                return obj;
            }
        }

        #endregion internal GetSQLScalar ovverloads
        #endregion SQLScalar methods

        #region SqlExecute Methods
        #region public SqlExecute Overloads
        /// <summary>
        /// Execute specified sql Statement or procedure, return nothing
        /// Connects to specified database using integrated security
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that does not return anything}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        public static void SqlExecute(string sql,
            string Server, string Database)
        { SqlExecute(sql, ConnectionString.GetConnSpec(Server, Database), DEFTIMEOUT); }
        /// <summary>
        /// Execute specified sql Statement or procedure, return nothing
        /// Connects to specified database using integrated security
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that does not return anything}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <param name="port">Port Number, if not 1433</param>
        public static void SqlExecute(string sql,
            string Server, string Database, int? port)
        { SqlExecute(sql, ConnectionString.GetConnSpec(Server, Database, port), DEFTIMEOUT); }
        /// <summary>
        /// Execute specified sql Statement or procedure, return nothing
        /// Connects to specified database using integrated security
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that does not return anything}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <param name="timeOut">optional ADO.Net timeout setting {Defaults to 45 secs}</param>
        public static void SqlExecute(string sql,
            string Server, string Database, int timeOut)
        { SqlExecute(sql, ConnectionString.GetConnSpec(Server, Database), timeOut); }
        /// <summary>
        /// Execute specified sql Statement or procedure, return nothing
        /// Connects to specified database using integrated security
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that does not return anything}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <param name="port">Port number, if not 1433</param>
        /// <param name="timeOut">optional ADO.Net timeout setting {Defaults to 45 secs}</param>
        public static void SqlExecute(string sql,
            string Server, string Database, int? port, int timeOut)
        { SqlExecute(sql, ConnectionString.GetConnSpec(Server, Database, port), timeOut); }
        /// <summary>
        /// Execute specified sql Statement or procedure, return nothing
        /// Connects using specified parameters
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that does not return anything}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <param name="UserID">logon or User Id to use in connectiong to database</param>
        /// <param name="Password">Clear text password to use in connectiong to database</param>
        public static void SqlExecute(string sql,
            string Server, string Database,
            string UserID, string Password)
        {
            SqlExecute(sql,
                   ConnectionString.GetConnSpec(Server, Database, UserID, Password),
                   DEFTIMEOUT);
        }
        /// <summary>
        /// Execute specified sql Statement or procedure, return nothing
        /// Connects using specified parameters
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that does not return anything}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <param name="port">Port number, if not 1433</param>
        /// <param name="UserID">logon or User Id to use in connectiong to database</param>
        /// <param name="Password">Clear text password to use in connectiong to database</param>
        public static void SqlExecute(string sql,
            string Server, string Database, int? port,
            string UserID, string Password)
        {
            SqlExecute(sql,
                   ConnectionString.GetConnSpec(Server, Database, port, UserID, Password),
                   DEFTIMEOUT);
        }        
        /// <summary>
        /// Execute specified sql Statement or procedure, return nothing
        /// Connects using specified parameters
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that does not return anything}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <param name="UserID">logon or User Id to use in connectiong to database</param>
        /// <param name="Password">Clear text password to use in connectiong to database</param>
        /// <param name="timeOut">optional ADO.Net timeout setting {Defaults to 45 secs}</param>
        public static void SqlExecute(string sql,
            string Server, string Database, string UserID,
            string Password, int timeOut)
        {
            SqlExecute(sql, ConnectionString.GetConnSpec(
               Server, Database, UserID, Password), timeOut);
        }
        /// <summary>
        /// Execute specified sql Statement or procedure, return nothing
        /// Connects using specified parameters
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that does not return anything}</param>
        /// <param name="Server">Server name or IP Address:Port number</param>
        /// <param name="Database">Database or Catalog name</param>
        /// <param name="port">Port number, if not 1433</param>
        /// <param name="UserID">logon or User Id to use in connectiong to database</param>
        /// <param name="Password">Clear text password to use in connectiong to database</param>
        /// <param name="timeOut">optional ADO.Net timeout setting {Defaults to 45 secs}</param>
        public static void SqlExecute(string sql,
            string Server, string Database, int? port, 
            string UserID,string Password, int timeOut)
        {
            SqlExecute(sql, ConnectionString.GetConnSpec(
               Server, Database, port, UserID, Password), timeOut);
        }          

        /// <summary>
        /// Execute specified sql Statement or procedure, return nothing
        /// Connects to database using specified AppName and Environment 
        /// as delinieated in connections.config file
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that does not return anything}</param>
        /// <param name="AppName">Application name specifying Database to connect to</param>
        /// <param name="Environment">Specifies environment instance of database to connect</param>
        public static void SqlExecute(string sql,
            string AppName, APPENV Environment)
        {
            SqlExecute(sql, ConnectionString.GetConnSpec(
              COMPANY, AppName, Environment), DEFTIMEOUT);
        }
        /// <summary>
        /// Execute specified sql Statement or procedure, return nothing
        /// Connects to database using specified AppName and Environment 
        /// as delinieated in connections.config file
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that does not return anything}</param>
        /// <param name="Company">Specifies Company Name element in Application configuration file</param>
        /// <param name="AppName">Application name specifying Database to connect to</param>
        /// <param name="Environment">Specifies environment instance of database to connect</param>
        public static void SqlExecute(string sql, string Company,
            string AppName, APPENV Environment)
        {
            SqlExecute(sql, ConnectionString.GetConnSpec(
                   Company, AppName, Environment), DEFTIMEOUT);
        }
        /// <summary>
        /// Execute specified sql Statement or procedure, return nothing
        /// Connects to database using specified AppName and Environment 
        /// as delinieated in connections.config file
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that does not return anything}</param>
        /// <param name="Company">Specifies Company Name element in Application configuration file</param>
        /// <param name="AppName">Application name specifying Database to connect to</param>
        /// <param name="Environment">Specifies environment instance of database to connect</param>
        /// <param name="timeOut">optional ADO.Net timeout setting {Defaults to 45 secs}</param>
        public static void SqlExecute(string sql, string Company,
            string AppName, APPENV Environment, int timeOut)
        {
            SqlExecute(sql,
               ConnectionString.GetConnSpec(Company, AppName, Environment),
               timeOut);
        }
        /// <summary>
        /// Execute specified sql Statement or procedure, return nothing
        /// Connects to database using provided CoPConnection 
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that does not return anything}</param>
        /// <param name="CoPConnection">CoPConnection object - 
        /// (wrapper for SqlConnection or OracleConnection)</param>
        public static void SqlExecute(string sql, CoPConnection CoPConnection)
        { SqlExecute(sql, DEFTIMEOUT, CoPConnection); }
        /// <summary>
        /// Execute specified sql Statement or procedure, return nothing
        /// Connects to database using provided CoPConnection 
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that does not return anything}</param>
        /// <param name="timeout">optional ADO.Net timeout setting {Defaults to 45 secs}</param>
        /// <param name="CoPConnection">CoPConnection object - 
        /// (wrapper for SqlConnection or OracleConnection)</param>
        public static void SqlExecute(string sql, int timeout,
                CoPConnection CoPConnection)
        {
            using (var cn = CoPConnection.Connection)
            {
                if (cn.State == ConnectionState.Closed) cn.Open();
                var cmd = CommandFactory(sql, cn, timeout);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Execute specified sql Statement or procedure, with parameters, return nothing
        /// Connects to database using provided CoPConnection 
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that does not return anything}</param>
        /// <param name="parms">Stored Procedure parameters</param>
        /// <param name="CoPConnection">CoPConnection object - 
        /// (wrapper for SqlConnection or OracleConnection)</param>
        public static void SqlExecute(string sql,
            IEnumerable<IDbDataParameter> parms, CoPConnection CoPConnection)
        { SqlExecute(sql, DEFTIMEOUT, parms, CoPConnection); }
        /// <summary>
        /// Execute specified sql Statement or procedure, with parameters, return nothing
        /// Connects to database using provided CoPConnection 
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that does not return anything}</param>
        /// <param name="timeout">optional ADO.Net timeout setting {Defaults to 45 secs}</param>
        /// <param name="parms">Stored Procedure parameters</param>
        /// <param name="CoPConnection">CoPConnection object - 
        /// (wrapper for SqlConnection or OracleConnection)</param>
        public static void SqlExecute(string sql, int timeout,
            IEnumerable<IDbDataParameter> parms, CoPConnection CoPConnection)
        {
            using (var cn = CoPConnection.Connection)
            {
                if (cn.State == ConnectionState.Closed) cn.Open();
                var cmd = CommandFactory(sql, cn, parms, timeout);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Execute specified sql Statement or procedure, return nothing
        /// Connects using provided connection object
        /// </summary>
        /// <param name="sql">sql Statment to execute 
        /// {Should be a select statement or procedure that does not return anything}</param>
        /// <param name="cn">Database Connection 
        /// {can be any connection object which derives from DbConnection}</param>
        /// <param name="tx">Local Transaction if one exists</param>
        /// <param name="timeOut">optional ADO.Net timeout setting {Defaults to 45 secs}</param>
        public static void SqlExecute(string sql, DbConnection cn,
            IDbTransaction tx = null, int timeOut = DEFTIMEOUT)
        {
            var closeConnection=false;
            if (cn.State != ConnectionState.Open)
            {
                cn.Open();
                closeConnection=true;
            }
            var cmd=CommandFactory(sql, cn, tx, timeOut);
            cmd.ExecuteNonQuery();
            if (closeConnection) cn.Close();
        }

        #endregion public SqlExecute Overloads

        #region internal SqlExecute Overloads

        internal static void SqlExecute(string sql, ConnectionSpec ConnSpec, int timeOut = DEFTIMEOUT)
        {
            using (var cn=ConnectionFactory(ConnSpec))
            {
                if (cn.State == ConnectionState.Closed) cn.Open();
                var cmd = CommandFactory(sql, cn, timeOut);
                cmd.ExecuteNonQuery();
                if (cn.State == ConnectionState.Open) cn.Close();
            }
        }

        #endregion internal SqlExecute Overloads
        #endregion SqlExecute Methods
    }
}