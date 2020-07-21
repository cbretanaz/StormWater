using System;
using System.Collections;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using System.Runtime.Remoting.Channels.Tcp;
using System.Security.Principal;
using CoP.Enterprise;
using CoP.Enterprise.Data;
using lib = CoP.Enterprise.Utilities;
using cnUtil = CoP.Enterprise.Data.Utilities;

namespace CoP.DataAccess
{
    public sealed class RemotingConnection<T> : IDisposable 
        where T : MarshalByRefObject
    {
        public T Service { get; set; }
        private readonly TcpClientChannel tcpCh;
        private readonly HttpClientChannel httpCh;
        private RemotingConnection(RemotingProtocol remProt, 
            string url, IDictionary propBag)
        {
            switch (remProt)
            {
                case (RemotingProtocol.Tcp):
                    tcpCh = new TcpClientChannel(propBag, null);
                    ChannelServices.RegisterChannel(tcpCh, false);
                    break;
                case (RemotingProtocol.Http):
                    httpCh = new HttpClientChannel(propBag, null);
                    ChannelServices.RegisterChannel(httpCh, false);
                    break;
                default: break;
            }
            Service = (T)Activator.GetObject(typeof(T), url);
        }

        public static RemotingConnection<T> Factory(string company, 
            string application, APPENV environment)
        {
            var cSpc = cnUtil.CONNCFGSECTION[company, application, environment];
            IDictionary propBag = new Hashtable();
            propBag["port"] = cSpc.Port;
            propBag["name"] = string.Format("{0}:{1}", cSpc.ServerName, cSpc.Port);
            propBag["connectionTimeout"] = cSpc.RemotingTimeout.ToString();
            if (cSpc.RemotingSecurityMode == RemSecMode.Identify)
            {
                propBag["secure"] = true;
                propBag["tokenImpersonationLevel"] =
                    TokenImpersonationLevel.Identification;
                propBag["useDefaultCredentials"] = true;
            }
            RemotingProtocol remProt;
            if (lib.EnumTryParse(cSpc.Protocol, out remProt))
                remProt = RemotingProtocol.Http;
            var url = cnUtil.BuildConnectionUrl(company, application, environment);
            return new RemotingConnection<T>(remProt, url, propBag);
        }

        #region IDisposable
        private bool disposed;
        /// <summary>
        /// 
        /// </summary>
        //public void Dispose() { Dispose(true); }
        public void Dispose()
        {
            if (disposed) return;
            // -----------------
            if (tcpCh != null)  ChannelServices.UnregisterChannel(tcpCh);
            if (httpCh != null) ChannelServices.UnregisterChannel(httpCh);
            disposed = true;
            GC.SuppressFinalize(this);
        }
        //~RemotingConnection() { Dispose(false); }

        #endregion IDisposable
    }
    public enum RemotingProtocol {Tcp, Http}
}