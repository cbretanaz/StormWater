using System;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Xml;
using BPA.Enterprise;
using Oracle.DataAccess.Client;
using System.Security.Permissions;
using log = BPA.Log.DALLog;
using lib = BPA.Enterprise.Utilities;
using cnUtil = BPA.Data.Utilities;
using Cnfg = System.Configuration.ConfigurationSettings;
using CfgMgr = System.Configuration.ConfigurationManager;

[assembly:SecurityPermissionAttribute(SecurityAction.RequestMinimum, UnmanagedCode=true)]
namespace BPA.Data
{
    public static partial class Utilities
    {
        #region protected Oracle Parameter Factory Overloads
        private static OracleDbType GetOracleDbType(object o)
        {
            if (o is string) return OracleDbType.Varchar2;
            if (o is DateTime) return OracleDbType.Date;
            if (o is Int64) return OracleDbType.Int64;
            if (o is Int32) return OracleDbType.Int32;
            if (o is Int16) return OracleDbType.Int16;
            if (o is byte) return OracleDbType.Byte;
            if (o is decimal) return OracleDbType.Decimal;
            if (o is float) return OracleDbType.Single;
            if (o is double) return OracleDbType.Double;
            return OracleDbType.Varchar2;
        }

        public static OracleParameter OraParamFactory<T>(string parameterName, T? value) where T : struct
        { return OraParamFactory(parameterName, value, ParameterDirection.Input); }
        public static OracleParameter OraParamFactory<T>(string parameterName, T? value, ParameterDirection dir) where T : struct
        { return OraParamFactory(parameterName, GetOracleDbType(value.GetValueOrDefault()), dir, (value.HasValue ? value.Value : (T?)null)); }
        // ----------------------------------------------------------------------------------
        public static OracleParameter OraParamFactory(string parameterName, bool value)
        { return OraParamFactory(parameterName, OracleDbType.Varchar2, ParameterDirection.Input, (value ? "Y" : "N")); }
        public static OracleParameter OraParamFactory(string parameterName, bool value, ParameterDirection dir)
        { return OraParamFactory(parameterName, OracleDbType.Varchar2, dir, (value ? "Y" : "N")); }
        // ----------------------------------------------------------------------------------
        public static OracleParameter OraParamFactory(string parameterName, string value)
        { return OraParamFactory(parameterName, value, ParameterDirection.Input); }
        public static OracleParameter OraParamFactory(string parameterName, string value, ParameterDirection dir)
        { return OraParamFactory(parameterName, OracleDbType.Varchar2, dir, value); }
        // ----------------------------------------------------------------------------------
        public static OracleParameter OraParamFactory(string parameterName, DateTime value)
        { return OraParamFactory(parameterName, value, ParameterDirection.Input); }
        public static OracleParameter OraParamFactory(string parameterName, DateTime value, ParameterDirection dir)
        { return OraParamFactory(parameterName, OracleDbType.Date, dir, value); }
        // ----------------------------------------------------------------------------------
        public static OracleParameter OraParamFactory(string parameterName, byte value)
        { return OraParamFactory(parameterName, value, ParameterDirection.Input); }
        public static OracleParameter OraParamFactory(string parameterName, byte value, ParameterDirection dir)
        { return OraParamFactory(parameterName, OracleDbType.Byte, dir, value); }
        // ----------------------------------------------------------------------------------
        public static OracleParameter OraParamFactory(string parameterName, Int16 value)
        { return OraParamFactory(parameterName, value, ParameterDirection.Input); }
        public static OracleParameter OraParamFactory(string parameterName, Int16 value, ParameterDirection dir)
        { return OraParamFactory(parameterName, OracleDbType.Int16, dir, value); }
        // ----------------------------------------------------------------------------------
        public static OracleParameter OraParamFactory(string parameterName, Int32 value)
        { return OraParamFactory(parameterName, value, ParameterDirection.Input); }
        public static OracleParameter OraParamFactory(string parameterName, Int32 value, ParameterDirection dir)
        { return OraParamFactory(parameterName, OracleDbType.Int32, dir, value); }
        // ----------------------------------------------------------------------------------
        public static OracleParameter OraParamFactory(string parameterName, Int64 value)
        { return OraParamFactory(parameterName, value, ParameterDirection.Input); }
        public static OracleParameter OraParamFactory(string parameterName, Int64 value, ParameterDirection dir)
        { return OraParamFactory(parameterName, OracleDbType.Int64, dir, value); }
        // ----------------------------------------------------------------------------------
        public static OracleParameter OraParamFactory(string parameterName, double value)
        { return OraParamFactory(parameterName, value, ParameterDirection.Input); }
        public static OracleParameter OraParamFactory(string parameterName, double value, ParameterDirection dir)
        { return OraParamFactory(parameterName, OracleDbType.Decimal, dir, value); }
        // ----------------------------------------------------------------------------------
        public static OracleParameter OraParamFactory(string parameterName, float value)
        { return OraParamFactory(parameterName, value, ParameterDirection.Input); }
        public static OracleParameter OraParamFactory(string parameterName, float value, ParameterDirection dir)
        { return OraParamFactory(parameterName, OracleDbType.Decimal, dir, value); }
        // ----------------------------------------------------------------------------------
        public static OracleParameter OraParamFactory(string parameterName, decimal value)
        { return OraParamFactory(parameterName, value, ParameterDirection.Input); }
        public static OracleParameter OraParamFactory(string parameterName, decimal value, ParameterDirection dir)
        { return OraParamFactory(parameterName, OracleDbType.Decimal, dir, value); }
        // ----------------------------------------------------------------------------------
        public static OracleParameter OraParamFactory(string parameterName, byte[] value)
        { return OraParamFactory(parameterName, value, ParameterDirection.Input); }
        public static OracleParameter OraParamFactory(string parameterName, byte[] value, ParameterDirection dir)
        { return OraParamFactory(parameterName, OracleDbType.Blob, dir, value); }
        // ----------------------------------------------------------------------------------
        public static OracleParameter OraParamFactory(string parameterName, string value, bool IsClob)
        {
            return OraParamFactory(parameterName, (IsClob ? OracleDbType.Clob : OracleDbType.Varchar2),
                      ParameterDirection.Input, value);
        }
        // ----------------------------------------------------------------------------------
        public static OracleParameter OraOutParamFactory(string parameterName, OracleDbType parmType)
        { return OraParamFactory(parameterName, parmType, ParameterDirection.Output, null); }
        // ----------------------------------------------------------------------------------
        private static OracleParameter OraParamFactory(string parameterName,
            OracleDbType ParamType, ParameterDirection Direction, object value)
        { return OraParamFactory(parameterName, ParamType, null, Direction, value); }
        private static OracleParameter OraParamFactory(string parameterName,
            OracleDbType paramType, int? Size, ParameterDirection direction, object value)
        {
            try
            {
                var oP = new OracleParameter(parameterName, paramType) { Direction = direction };
                if (Size.HasValue) oP.Size = Size.Value;
                if (value != null) oP.Value = value;
                return oP;
            }
            catch (OracleException oX)
            { throw new OraException("OraParamFactory failed, " + oX.Message, oX); }
        }
        #endregion Oracle Parameter Factory Overloads
    }

}