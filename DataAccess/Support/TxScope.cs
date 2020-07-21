using System;
//using Oracle.DataAccess.Client;
using log = BPA.Log.DALLog;
using cnUtil = BPA.Data.Utilities;

namespace BPA.Data
{
    public class TxScope : IDisposable
    {
        #region private fields
        private BPAConnection conn;
        private TxScope outerScop;

        #endregion private fields

        #region properties
        public BPAConnection Connection
        {
            get { return conn; }
            set
            {
                conn = value;
                if (OuterScope != null)
                    OuterScope.Connection = value;
            }
        }

        public TxEnlist EnlistTransaction { get; set; }

        public TxScope OuterScope
        {
            get { return outerScop; }
            set
            {
                outerScop = value;
                if (outerScop != null)
                    Connection = outerScop.Connection;
            }
        }
        public bool IsInTransaction { get { return conn != null && conn.IsInTransaction; } }
        public string Name { get; set; }

        #endregion properties

        #region private ctors
        private TxScope(TxEnlist enlist) : this(null, enlist) { }
        private TxScope(BPAConnection connection, TxEnlist enlist)
        { 
            Connection = connection;
            EnlistTransaction = enlist;
        }
        #endregion ctor
        
        #region TxScope Factory overloads
        public static TxScope GetScope() { return GetScope(null, TxEnlist.Use); }
        public static TxScope GetScope(TxEnlist txEnlist) 
        { return GetScope(null, txEnlist); }
        public static TxScope GetScope(TxScope outerScope, TxEnlist txEnlist) 
        { return GetScope(outerScope, txEnlist, null); }
        public static TxScope GetScope(TxScope outerScope, TxEnlist txEnlist, string name)
        {
            var newScope = new TxScope(txEnlist)
            {OuterScope = outerScope, Name = name};
            return newScope;
        }
        #endregion TxScope Factory overloads

        #region IDisposable
        private bool disposed;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool userDisposing)
        {
            if (disposed) return;
            if (conn == null ||
                (OuterScope != null && userDisposing))
            {
                disposed = true;
                return;
            }
            // ----------------------------------------
            if (conn.HasTransaction) conn.Rollback();
            conn.Dispose();
            disposed = true;
        }
        ~TxScope() { Dispose(false); }
        #endregion IDisposable

        #region Tx Control methods
        public void StartTransaction()
        {
            if (conn == null || conn.Connection == null)
                throw new BPADataException("No valid Connection");
                if (conn.Connection.State == System.Data.ConnectionState.Closed)
                    conn.Open();
                if (!conn.HasTransaction)
                    conn.BeginTransaction();
            //}
            //catch (OracleException oX)
            //{
            //    throw new OraException(
            //      "BPAConnection.BeginTransaction failed, " +
            //      oX.Message, oX);
            //}
        }
        public void Complete() 
        {
            if (conn != null && 
              conn.IsInTransaction && 
              OuterScope == null) 
                conn.Complete(); 
        }
        public void Rollback()
        {
            if (conn != null && 
                conn.IsInTransaction) 
                conn.Rollback();
        }
        #endregion Tx Control methods
    }

    public enum TxEnlist { None, Use, New }
}