using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Xml;
using CoP.Enterprise.Support;
using log = CoP.Enterprise.DALLog;
using lib = CoP.Enterprise.Utilities;
using cnUtil = CoP.Enterprise.Data.Utilities;
using Cnfg = System.Configuration.ConfigurationSettings;
using CfgMgr = System.Configuration.ConfigurationManager;

namespace CoP.Enterprise.Data
{
    public static partial class Utilities
    {
        #region private static readonly const
        private static readonly CultureInfo ic = CultureInfo.InvariantCulture;
        private const string COMPANY = CoPGlobals.Company;   // Default Company Name		
        private const string eventSource = "Repository";
        private const int DEFTIMEOUT = 45;      // Default Database time out in seconds		
	    private static ConnectionConfig conCfgSec;
        public static string APPENVIRONMENTSTR => 
            !string.IsNullOrEmpty(CfgMgr.AppSettings["environment"]) ? 
                CfgMgr.AppSettings["environment"] :
            !string.IsNullOrEmpty(CfgMgr.AppSettings[COMPANY + "_Environment"]) ? 
                CfgMgr.AppSettings["CoPenvironment"] :
                null;

        public static APPENV APPENVIRONMENT => lib.GetAppMode(APPENVIRONMENTSTR);

        public static ConnectionConfig CONNCFGSECTION
	    {
	        get
	        {
                if (conCfgSec != null) return conCfgSec;
                // -------------------------------------
                var oCfg = CfgMgr.GetSection("ConnectionConfig");
                if (!(oCfg is ConnectionConfig))
                    throw new CoPDataConfigurationException(
                        "Cannot Load Connections Configuration data section.");
	            conCfgSec = oCfg as ConnectionConfig;
                return conCfgSec;
	        }
	    }
        #endregion private static readonly const

        #region SQLString ORAString Overloads
        public static string SQLString(byte InVal)	{return InVal.ToString(ic);}
        public static string SQLString(short InVal) { return InVal.ToString(ic); }
        public static string SQLString(int InVal) { return InVal.ToString(ic); }
        public static string SQLString(long InVal) { return InVal.ToString(ic); }
        public static string SQLString(decimal InVal) { return InVal.ToString(ic); }
		public static string SQLString(bool InVal)	{return (InVal)? "1": "0";}
        public static string SQLString(float InVal) { return InVal.ToString(ic); }
        public static string SQLString(double InVal) { return InVal.ToString(ic); }

        public static string ORAString(DateTime InVal)
        {
            var sOut = new StringBuilder(20);
            sOut.Append("to_date('");
            sOut.Append(InVal.ToString("yyyyMMdd HHmmss"));
            sOut.Append("', 'yyyyMMdd HH24miss')");
            return sOut.ToString();
        }
        public static string SQLString(DateTime InVal)
		{
            var sOut = new StringBuilder(20);
			sOut.Append("'");
			sOut.Append(InVal.ToString("yyyyMMdd HH:mm:ss.fff"));
			sOut.Append("'");
			return sOut.ToString();
		}
		public static string SQLString(TimeSpan InVal)
		{
			var sOut = new StringBuilder(20);
			sOut.Append("'");
			sOut.Append(InVal.ToString());
			sOut.Append("'");
			return sOut.ToString();
		}

		public static string SQLString(string InVal) {return(SQLString(InVal,true));}
        public static string SQLString(string InVal, bool convertDate)
        {
            if (InVal == null) return "null";
            if (convertDate && lib.IsDate(InVal))
                return SQLString(DateTime.Parse(InVal));
            var sOut = new StringBuilder(InVal);
            sOut.Replace("'", "''");
            sOut.Insert(0, "'");
            sOut.Append("'");
            return sOut.ToString();
        }

	    public static string SQLString(object InVal)
		{
			switch (InVal.GetType().FullName)
			{
				case("System.Byte"):	return(SQLString((byte)InVal));
				case("System.Int16"):	return(SQLString((Int16)InVal));
				case("System.Int32"):	return(SQLString((Int32)InVal));
				case("System.Int64"):	return(SQLString((Int64)InVal));
				case("System.Boolean"): return(SQLString((bool)InVal));
				case("System.Single"):	return(SQLString((float)InVal));
				case("System.Double"):	return(SQLString((double)InVal));
				case("System.Decimal"): return(SQLString((decimal)InVal));
				case("System.TimeSpan"): return(SQLString((TimeSpan)InVal));		
				case("System.DateTime"): return(SQLString((DateTime)InVal));
				case("System.Char"):	return(SQLString((string)InVal));
				case("System.String"):	return(SQLString((String)InVal));
				default:				return(SQLString(InVal.ToString()));
			}
		}
		#endregion

        #region SQLServer Parameter Factory Overloads
        private static SqlDbType GetSqlServerDbType(object o) 
        {
            if (o is string) return SqlDbType.VarChar;
            if (o is DateTime) return SqlDbType.Date;
            if (o is Int64) return SqlDbType.BigInt;
            if (o is Int32) return SqlDbType.Int;
            if (o is Int16) return SqlDbType.SmallInt;
            if (o is byte) return SqlDbType.TinyInt;
            if (o is decimal) return SqlDbType.Decimal;
            if (o is float) return SqlDbType.Float;
            if (o is double) return SqlDbType.Real;
            if (o is XmlNode) return SqlDbType.Xml;
            return SqlDbType.VarChar;
        }

        public static SqlParameter SqlServerParamFactory<T>(string parameterName, T? value, 
                ParameterDirection dir = ParameterDirection.Input) where T : struct
        {return SqlServerParamFactory(parameterName, GetSqlServerDbType(value.GetValueOrDefault()),
                                       dir, (value.HasValue ? value.Value : (T?)null)); }
        // ----------------------------------------------------------------------------------
        public static SqlParameter SqlServerParamFactory(string parameterName, bool value)
        { return SqlServerParamFactory(parameterName, SqlDbType.VarChar, 
            ParameterDirection.Input, (value ? "1" : "0")); }
        public static SqlParameter SqlServerParamFactory(string parameterName, bool value, 
            ParameterDirection dir)
        { return SqlServerParamFactory(parameterName, SqlDbType.VarChar, dir, (value ? "1" : "0")); }
        // ----------------------------------------------------------------------------------
        public static SqlParameter SqlServerParamFactory(string parameterName, string value, 
            ParameterDirection dir = ParameterDirection.Input)
        { return SqlServerParamFactory(parameterName, SqlDbType.VarChar, dir, value); }
        // ----------------------------------------------------------------------------------
        public static SqlParameter SqlServerParamFactory(string parameterName, DateTime value, 
            ParameterDirection dir = ParameterDirection.Input)
        { return SqlServerParamFactory(parameterName, SqlDbType.Date, dir, value); }
        // ----------------------------------------------------------------------------------
        public static SqlParameter SqlServerParamFactory(string parameterName, byte value, 
            ParameterDirection dir = ParameterDirection.Input)
        { return SqlServerParamFactory(parameterName, SqlDbType.TinyInt, dir, value); }
        // ----------------------------------------------------------------------------------
        public static SqlParameter SqlServerParamFactory(string parameterName, Int16 value, 
            ParameterDirection dir = ParameterDirection.Input)
        { return SqlServerParamFactory(parameterName, SqlDbType.SmallInt, dir, value); }
        // ----------------------------------------------------------------------------------
        public static SqlParameter SqlServerParamFactory(string parameterName, Int32 value, 
            ParameterDirection dir = ParameterDirection.Input)
        { return SqlServerParamFactory(parameterName, SqlDbType.Int, dir, value); }
        // ----------------------------------------------------------------------------------
        public static SqlParameter SqlServerParamFactory(string parameterName, Int64 value, 
            ParameterDirection dir = ParameterDirection.Input)
        { return SqlServerParamFactory(parameterName, SqlDbType.BigInt, dir, value); }
        // ----------------------------------------------------------------------------------
        public static SqlParameter SqlServerParamFactory(string parameterName, double value, 
            ParameterDirection dir = ParameterDirection.Input)
        { return SqlServerParamFactory(parameterName, SqlDbType.Decimal, dir, value); }
        // ----------------------------------------------------------------------------------
        public static SqlParameter SqlServerParamFactory(string parameterName, float value, 
                        ParameterDirection dir = ParameterDirection.Input)
        { return SqlServerParamFactory(parameterName, SqlDbType.Decimal, dir, value); }
        // ----------------------------------------------------------------------------------
        public static SqlParameter SqlServerParamFactory(string parameterName, decimal value, 
            ParameterDirection dir = ParameterDirection.Input)
        { return SqlServerParamFactory(parameterName, SqlDbType.Decimal, dir, value); }
        // ----------------------------------------------------------------------------------
        public static SqlParameter SqlServerParamFactory(string parameterName, 
            byte[] value, ParameterDirection dir = ParameterDirection.Input)
        { return SqlServerParamFactory(parameterName, SqlDbType.Image, dir, value); }
        // ----------------------------------------------------------------------------------
        public static SqlParameter SqlServerParamFactory(string parameterName, 
           DataTable value, ParameterDirection dir = ParameterDirection.Input)
        { return SqlServerParamFactory(parameterName, SqlDbType.Structured, dir, value); }
        // ----------------------------------------------------------------------------------
        public static SqlParameter SqlServerParamFactory(string parameterName, string value, bool IsClob)
        { return SqlServerParamFactory(parameterName, (IsClob ? SqlDbType.Text : SqlDbType.VarChar),
                      ParameterDirection.Input, value); }
        public static SqlParameter SqlServerParamFactory(string parameterName, XmlNode value)
        { return SqlServerParamFactory(parameterName, SqlDbType.Xml, ParameterDirection.Input, value.OuterXml); }
        // ----------------------------------------------------------------------------------
        public static SqlParameter OraOutParamFactory(string parameterName, SqlDbType parmType)
        { return SqlServerParamFactory(parameterName, parmType, ParameterDirection.Output, null); }
        // ----------------------------------------------------------------------------------
        private static SqlParameter SqlServerParamFactory(string parameterName,
            SqlDbType ParamType, ParameterDirection Direction, object value)
        { return SqlServerParamFactory(parameterName, ParamType, null, Direction, value); }
        private static SqlParameter SqlServerParamFactory(string parameterName,
            SqlDbType paramType, int? Size, ParameterDirection direction, object value)
        {
            var oP = new SqlParameter(parameterName, paramType) {Direction = direction};
            if (Size.HasValue) oP.Size = Size.Value;
            if (value != null) oP.Value = value;
            return oP;
        }

        #endregion SQLServer Parameter Factory Overloads

        #region Misc DataBase Utilities
        public static string GetSProcSQL(string SPName, params SPParameter[] SPParams)
		{
            var sb = new StringBuilder(25);
			sb.Append("Exec " + SPName + " ");
            foreach (var Spp in SPParams)
				sb.Append("@" + Spp.Name + "=" + 
					SQLString(Spp.pValue) + ",");
			return sb.ToString(0, sb.Length-1);
        }

        #region connection factorys
        public static IDbConnection ConnectionFactory(string connStr, DBVendor vendor)
        { return ConnectionFactory(new ConnectionSpec(connStr, vendor)); }
        public static IDbConnection ConnectionFactory(string connStr) 
            { return ConnectionFactory(new ConnectionSpec(connStr, DBVendor.SQLServer)); }
        internal static IDbConnection ConnectionFactory(ConnectionSpec ConnSpec)
        {
            switch (ConnSpec.Vendor)
            {
                case DBVendor.SQLServer:
                    return new SqlConnection(ConnSpec.ConnectionString);
                //case DBVendor.Oracle:
                //    return new OracleConnection(ConnSpec.ConnectionString);
                default: return null;
            }
        }

        #endregion connection factorys

        #region Data Adapter factorys

        public static DbDataAdapter AdapterFactory(
            string sql, IDbConnection conn, 
            IDbTransaction tx = null, int Timeout = DEFTIMEOUT)
        {
            if (conn is SqlConnection)
                return new SqlDataAdapter(CommandFactory(
                    sql, conn, tx, Timeout) as SqlCommand);
            //if (conn is OracleConnection)
            //    return new OracleDataAdapter(CommandFactory(
            //        sql, conn, tx, Timeout) as OracleCommand);
            return null;
        }
        public static DbDataAdapter AdapterFactory(string procName,
            IEnumerable<IDbDataParameter> parms, IDbConnection conn, int timeout)
        { return AdapterFactory(procName, parms, conn, null, timeout); }

        public static DbDataAdapter AdapterFactory(string procName,
            IEnumerable<IDbDataParameter> parms, IDbConnection conn, 
            IDbTransaction tx = null, int timeout = DEFTIMEOUT)
        {
            if (conn is SqlConnection)
                return new SqlDataAdapter(
                    CommandFactory(procName, conn,
                    parms, timeout) as SqlCommand);
            return null;
        }
        #endregion Data Adapter factorys

        #region Command factorys
        public static IDbCommand CommandFactory(string sql,
            IDbConnection conn, int timeout)
        { return CommandFactory(sql, conn, null, null,timeout); }

        public static IDbCommand CommandFactory(string sql, 
            IDbConnection conn, IDbTransaction tx = null, 
            int timeout = DEFTIMEOUT)
        { return CommandFactory(sql, conn, null, tx, timeout); }

        public static IDbCommand CommandFactory(string sql,
            IDbConnection conn, IEnumerable<IDbDataParameter> parms,
            int timeOut)
        { return CommandFactory(sql, conn, parms, null, timeOut); }

        public static IDbCommand CommandFactory(string sql,
            IDbConnection conn, IEnumerable<IDbDataParameter> parms,
            IDbTransaction tx,  int timeOut)
        {
            IDbCommand cmd=null;
            if (conn is SqlConnection connection)
                cmd=new SqlCommand(sql, connection);
            if (cmd != null && cmd.Transaction == null && tx != null)
                cmd.Transaction = tx;
            //   ----------------------------------------------------
            if (cmd == null) return null;
            // --------------------------
            cmd.CommandType = 
                parms == null? CommandType.Text: 
                               CommandType.StoredProcedure;
            if (parms != null)
                foreach (var parm in parms.Where(parm => parm != null))
                    cmd.Parameters.Add(parm);
            cmd.CommandTimeout=timeOut;
            if (cmd.Connection.State != ConnectionState.Open)
                cmd.Connection.Open();
            return cmd;
        }
        #endregion Command factorys

        #endregion Misc DataBase Utilities
    }
}
