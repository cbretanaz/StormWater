using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection;
using System.Security.Permissions;
using System.Runtime.Serialization;
using System.Threading;
using Lib = CoP.Enterprise.Utilities;
using log = CoP.Enterprise.DALLog;
using cnUtil = CoP.Enterprise.Data.Utilities;
using Cnfg = System.Configuration.ConfigurationSettings;
using CfgMgr = System.Configuration.ConfigurationManager;

[assembly:SecurityPermissionAttribute(SecurityAction.RequestMinimum, UnmanagedCode=true)]
namespace CoP.Enterprise.Data
{
    #region supporting types
    public sealed class CoPConnection: IDisposable
    {
        #region private fields
        private IDbConnection con;
        #endregion private fields

        #region properties
        public IDbConnection Connection { get { return con; } set { con = value; } }
        
        #endregion properties

        #region IDisposable
        private bool disposed;
        public void Dispose() { Dispose(true); }
        private void Dispose(bool userDisposing)
        {
            if (disposed || con == null ||
                con.State == ConnectionState.Closed ||
                userDisposing) return;

            //if (HasTransaction && IsInTransaction) tx.Rollback();

            con.Dispose();
            disposed = true;
            GC.SuppressFinalize(this);
        }
        #endregion IDisposable

        #region public ctors
        public CoPConnection(IDbConnection connection)
        { Connection = connection; }

        public CoPConnection(string connectionString, DBVendor vendor)
        { Connection = Utilities.ConnectionFactory(connectionString, vendor); }
        #endregion public ctors

        public void Open()
        {
            if (con == null)
                throw new CoPDataException(
                    "No valid connection object");
            if (con.State == ConnectionState.Open) return;
            Connection.Open();
        }

        #region Transaction members
        //private bool isInTx;
        //private IDbTransaction tx;
        //public IDbTransaction Transaction { get { return tx; } set { tx = value; } }
        //public bool HasTransaction { get { return Transaction != null; } }
        //public bool IsInTransaction
        //{
        //    get { return HasTransaction && isInTx; }
        //    set { isInTx = value; }
        //}

        //public void BeginTransaction(IsolationLevel isoLevel = IsolationLevel.ReadCommitted)
        //{
        //    if (con == null || con.State != ConnectionState.Open)
        //        throw new CoPDataException("No open connection");
        //    //if (!HasTransaction) Transaction =
        //    Connection.BeginTransaction(isoLevel);
        //}
        //public void Complete()
        //{
        //    if (ValidateTransaction())
        //        Transaction.Commit();
        //    IsInTransaction = false;
        //    Transaction = null;
        //}
        //public void Rollback()
        //{
        //    if (ValidateTransaction())
        //        Transaction.Rollback();
        //    IsInTransaction = false;
        //    Transaction = null;
        //}
        //private bool ValidateTransaction()
        //{
        //    return con != null &&
        //        con.State == ConnectionState.Open &&
        //        IsInTransaction;
        //}
        #endregion Transaction members
    }

    #region Data Exceptions
    [Serializable]
    public class OraException : DbException
    {
        public OraException(string sMessage,
            Exception innerException)
            : base(sMessage, innerException) { }
        public OraException(string sMessage)
            : base(sMessage) { }
        public OraException() { }

        #region Serializeable Code
        public OraException(
           SerializationInfo info, StreamingContext context)
            :
           base(info, context) { }
        #endregion Serializeable Code
    }

    [Serializable]
    public class DataLayerException : ApplicationException
    {
        public DataLayerException(string sMessage, Exception innerException)
            : base(sMessage, innerException) { }
        public DataLayerException(string sMessage)
            : base(sMessage) { }
        public DataLayerException() { }

        #region Serializeable Code
        public DataLayerException(
           SerializationInfo info, StreamingContext context)
            :
           base(info, context) { }
        #endregion Serializeable Code
    }

    [Serializable]
    public class DataValueNullException : DataLayerException
    {
        public DataValueNullException(string sMessage, Exception innerException)
            : base(sMessage, innerException) { }
        public DataValueNullException(string sMessage)
            : base(sMessage) { }
        public DataValueNullException() { }

        #region Serializeable Code
        public DataValueNullException(
           SerializationInfo info, StreamingContext context)
            :
           base(info, context) { }
        #endregion Serializeable Code
    }
    #endregion Data Exceptions

    internal struct ConnectionSpec
    {
        #region private fields
        private string connStr;
        private DBVendor vendor;
        #endregion private fields

        #region public propertys
        public string ConnectionString
        {
            get { return connStr; }
            set { connStr = value; }
        }
        public DBVendor Vendor
        {
            get { return vendor; }
            set { vendor = value; }
        }
        #endregion public propertys

        #region ctors
        public ConnectionSpec(string ConnStr, DBVendor Vendor)
        {
            connStr = ConnStr;
            vendor = Vendor;
        }
        #endregion ctors
    }

    public struct SPParameter
    {
        public string Name;
        public Type pType;
        public object pValue;
    }

    public class DbParamSortedList : SortedList<string,IDbDataParameter>
    {
    }

    public class DbParamList : List<IDbDataParameter>
    {
        private DbParamList() {}
        public static DbParamList Make(IEnumerable<SqlParameter> parms)
        {
            var prmLst = new DbParamList();
            prmLst.AddRange(parms);
            return prmLst;
        }

        public static DbParamList Make(params SqlParameter[] parms)
        {
            var prmLst = new DbParamList();
            prmLst.AddRange(parms);
            return prmLst;
        }

        public void AddSQLParm(string parmName, bool value)
        { Add(new SqlParameter(parmName, value ? "1" : "0")); }

        public void AddSQLParm(string parmName, bool? value)
        {
            if (!value.HasValue)
            {
                throw new ArgumentNullException(
                    "Null value passed to AddSQLParm<>()");
            }
            Add(new SqlParameter(parmName, value.Value ? "1" : "0"));
        } 

        public void AddSQLParm<T>(string parmName, T value)
        {
            var type = typeof(T);
            if (type.IsEnum) Add(new SqlParameter(parmName, 
                Convert.ChangeType(value, Enum.GetUnderlyingType(type))));
                
            else Add(new SqlParameter(parmName, value));
        } 

        public void AddSQLParm<T>(string parmName, T? value,
            bool ignoreNull = false) where T : struct
        {
            var type = typeof(T);

            if (!value.HasValue)
            {
                if (ignoreNull) return;
                throw new ArgumentNullException(
                    "Null value passed to AddSQLParm<>()");
            }
            // ---------------------------------------

            if (type.IsEnum) Add(new SqlParameter(parmName, 
                Convert.ChangeType(value.Value, Enum.GetUnderlyingType(type))));
            else Add(new SqlParameter(parmName, value.Value));
        }

        public void AddSQLTableParm<T>(string parmName, IEnumerable<T> values)
        {
            var parm = new SqlParameter(parmName, CreateDataTable(values))
            {
                SqlDbType = SqlDbType.Structured,
                TypeName = "dbo.keyIds"
            };
            Add(parm);
        }

        internal static DataTable CreateDataTable<T>(IEnumerable<T> values)
        {
            var dt = new DataTable();
            var props = typeof (T).GetProperties();
            if (props.Length > 0)
            {
                foreach (var col in props)
                    dt.Columns.Add(col.Name, col.PropertyType);
                foreach (var id in values)
                {
                    var newRow = dt.NewRow();
                    foreach (var prop in id.GetType().GetProperties())
                        newRow[prop.Name] = prop.GetValue(id, null);
                    dt.Rows.Add(newRow);
                }
            }
            else
            {
                dt.Columns.Add("ids");
                foreach (var id in values) dt.Rows.Add(id);
            }
            return dt;
        }

    }

    public class DbParamDict : Dictionary<string,IDbDataParameter>
    {
        public static DbParamDict Make() { return new DbParamDict(); }

        public void AddSQLParm<T>(string parmName, T value)
        {
            var type = typeof(T);
            if (type.IsEnum) Add(parmName, new SqlParameter(parmName,
                    Convert.ChangeType(value, Enum.GetUnderlyingType(type))));

            else Add(parmName, new SqlParameter(parmName, value));
        }

        public void AddSQLParm(string parmName, bool value)
        { Add(parmName, new SqlParameter(parmName, value ? "1" : "0")); } 

        public void AddSQLParm<T>(string parmName, T? value, 
            bool ignoreNull = false) where T : struct
        {
            var typ = typeof (T);
            if (!value.HasValue)
            {
                if (ignoreNull) return;
                throw new ArgumentNullException(
                    "Null value passed to AddSQLParm<>()");
            }
            // ------------------------------------------
            if (typ.IsEnum) 
                Add(parmName, new SqlParameter(parmName,
                    Convert.ChangeType(value.Value, Enum.GetUnderlyingType(typ))));
            else Add(parmName, new SqlParameter(parmName, value.Value));

        }

        public void AddInOutIntParm<T>(string parmName, T value)
        {
            if (typeof(T) == typeof(int))
                Add(parmName, new SqlParameter(parmName,
                    SqlDbType.Int, 4, ParameterDirection.InputOutput,
                    true, 4, 0, null, DataRowVersion.Current, value));
            if (typeof(T) == typeof(int?))
                Add(parmName, new SqlParameter(parmName,
                    SqlDbType.Int, 4, ParameterDirection.InputOutput,
                    true, 4, 0, null, DataRowVersion.Current, value));
            else if (typeof(T) == typeof(string))
                Add(parmName, new SqlParameter(parmName,
                    SqlDbType.VarChar, 4, ParameterDirection.InputOutput,
                    true, 4, 0, null, DataRowVersion.Current, value));
            else if (typeof(T) == typeof(decimal))
                Add(parmName, new SqlParameter(parmName,
                    SqlDbType.Decimal, 4, ParameterDirection.InputOutput,
                    true, 4, 0, null, DataRowVersion.Current, value));
            else if (typeof(T) == typeof(DateTime))
                Add(parmName, new SqlParameter(parmName,
                    SqlDbType.DateTime, 4, ParameterDirection.InputOutput,
                    true, 4, 0, null, DataRowVersion.Current, value));
        }
        public void AddOutParm<T>(string parmName)
        {
            if (typeof(T) == typeof(int))
                Add(parmName, new SqlParameter(parmName,
                    SqlDbType.Int, 4, ParameterDirection.Output,
                    true, 4, 0, null, DataRowVersion.Current, null));
            if (typeof(T) == typeof(int?))
                Add(parmName, new SqlParameter(parmName,
                    SqlDbType.Int, 4, ParameterDirection.Output,
                    true, 4, 0, null, DataRowVersion.Current, null));
            else if (typeof(T) == typeof(string))
                Add(parmName, new SqlParameter(parmName,
                    SqlDbType.VarChar, 4, ParameterDirection.Output,
                    true, 4, 0, null, DataRowVersion.Current, null));
            else if (typeof(T) == typeof(decimal))
                Add(parmName, new SqlParameter(parmName,
                    SqlDbType.Decimal, 4, ParameterDirection.Output,
                    true, 4, 0, null, DataRowVersion.Current, null));
            else if (typeof(T) == typeof(DateTime))
                Add(parmName, new SqlParameter(parmName,
                    SqlDbType.DateTime, 4, ParameterDirection.Output,
                    true, 4, 0, null, DataRowVersion.Current, null));
        }

        public void AddSQLTableParm<T>(string parmName, IEnumerable<T> values, string typeName = "dbo.keyIds")
        {
            var parm = new SqlParameter(parmName, DbParamList.CreateDataTable(values))
            {
                SqlDbType = SqlDbType.Structured,
                TypeName = typeName
            };
            Add(parmName, parm);
        }
    }

    public enum RemSecMode
    { Unknown = 0, Anonymous = 1, Identify = 2, ByRole = 3 }
    public enum DBVendor : byte { SQLServer, Oracle, WebService, Remoting, WCF }
    public enum WcfBinding : byte 
    { None, BasicHttp, NetTcp, NetNamedPipe, 
        WSHttp, WSDualHttp, NetMsmq, Custom }
    #endregion supporting types
}