using System;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Threading;
using CoP.Enterprise;
using log = CoP.Enterprise.DALLog;
using cnUtil = CoP.Enterprise.Data.Utilities;
using cfgMgr = System.Configuration.ConfigurationManager;

namespace CoP.DataAccess
{
    /// <summary>
    /// WcfWrapper to use within using block to address issue
    /// of attempting to close proxy when in faulted state
    /// </summary>
    /// <typeparam name="I">Interface that Proxy must implement</typeparam>
    public sealed class WcfProxyWrapper<I>: IDisposable where I : class
    {
        static readonly StringComparison icic = StringComparison.InvariantCultureIgnoreCase;
        /// <summary>
        /// Proxy object simulating the remote Wcf Server
        /// which must implement interface specified by I
        /// </summary>
        public I Proxy { get; set; }
        public string Url { get; private set; }
        private string UserId { get; set; }
        private string Password { get; set; }
        
        /// <summary>
        /// Factory overload to construct WcfWrapper
        /// </summary>
        /// <param name="companyName">Company Name to index into Connections Config</param>
        /// <param name="serviceName">Service Name to index into Connections Config</param>
        /// <param name="environment">Environment to index into Connections Config</param>
        /// <returns>Manufactured WcfProxyWrapper</returns>
        public static WcfProxyWrapper<I> Factory(string companyName,
            string serviceName, APPENV environment,
            InstanceContext instCtx = null)
        {  return Factory(companyName, serviceName, environment,
            cnUtil.GetUserId(companyName, serviceName, environment),
            cnUtil.GetPassword(companyName, serviceName, environment),
            instCtx);
        }

        /// <summary>
        /// Factory overload to construct WcfWrapper
        /// </summary>
        /// <param name="companyName">Company Name to index into Connections Config</param>
        /// <param name="serviceName">Service Name to index into Connections Config</param>
        /// <param name="environment">Environment to index into Connections Config</param>
        /// <param name="userId">WCF logon user id</param>
        /// <param name="password">WCF password</param>
        /// <returns>Manufactured WcfProxyWrapper</returns>
        public static WcfProxyWrapper<I> Factory(
            string companyName, string serviceName, 
            APPENV environment, string userId, string password,
            InstanceContext instCtx = null)
        {
            var bndng = cnUtil.GetServiceBinding(companyName, serviceName, environment);
            var url = cnUtil.BuildWcfUrl(companyName, serviceName, environment);
            var maxGrphItms = cnUtil.GetMaxItemsInObjectGraph(companyName, serviceName, environment);
            var svrSideUpn = cnUtil.GetServerSideUPN(companyName, serviceName, environment);
            return Factory(bndng, url, userId, password, 
                instCtx, maxGrphItms, svrSideUpn);
        }

        /// <summary>
        /// Factory overload to construct WcfWrapper
        /// </summary>
        /// <param name="binding">channel binding</param>
        /// <param name="url">url of endpoint</param>
        /// <param name="userId">WCF logon user id</param>
        /// <param name="password">WCF password</param>
        /// <param name="instCtx"></param>
        /// <param name="maxItemsInObjectGraph">Contract behavior size (optional)</param>
        /// <param name="serverSideUpn">AD Account name of  
        /// credentials WCF service is running under</param>
        /// <returns>Manufactured WcfProxyWrapper</returns>
        private static WcfProxyWrapper<I> Factory(
            Binding binding, string url,
            string userId, string password,
            InstanceContext instCtx = null,
            int? maxItemsInObjectGraph = null,
            string serverSideUpn = null)
        {
            var prxyWrpr = new WcfProxyWrapper<I>
            { UserId = userId, Password = password };

            var isNetTcp = binding is NetTcpBinding;
            var epId = isNetTcp && !string.IsNullOrEmpty(serverSideUpn) ?
                EndpointIdentity.CreateUpnIdentity(serverSideUpn) : null;
            var ep = isNetTcp ?
                new EndpointAddress(new Uri(url), epId) :
                new EndpointAddress(url);
            var chnFactory = instCtx != null?
                new DuplexChannelFactory<I>(instCtx, binding, ep) :
                new ChannelFactory<I>(binding, ep);

            if (chnFactory == null) throw new NullReferenceException(
                "Could not instantiate Channel Factory");
            if (chnFactory.Credentials == null) throw new NullReferenceException(
                "Invalid Channel Factory Credentials");
            chnFactory.Credentials.UserName.UserName = prxyWrpr.UserId;
            chnFactory.Credentials.UserName.Password = prxyWrpr.Password;
            // --------------------------------------------------------------
            if (maxItemsInObjectGraph.HasValue)
                foreach (var behavior in chnFactory.Endpoint.Contract.Operations.Select(
                    op => op.Behaviors.Find<DataContractSerializerOperationBehavior>()).Where(
                                                            behavior => behavior != null))
                   behavior.MaxItemsInObjectGraph = maxItemsInObjectGraph.Value;
            // -------------------------------------------------------------------------
            prxyWrpr.Proxy = chnFactory.CreateChannel();

            prxyWrpr.Url = url;
            return prxyWrpr;
        }

        public delegate void WcfServiceDelegate<in T>(T proxy);
        public delegate R WcfServiceDelegateWithReturnValue<in T, R>(T proxy);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="codeBlock"></param>
        /// <returns></returns>
        public R Use<R>(WcfServiceDelegateWithReturnValue<I, R> codeBlock)
        {
            if (!(Proxy is IClientChannel prox))
                throw new CoPException("Could not create Wcf proxy.");

            var plannedOutage = cfgMgr.AppSettings["serviceUnavailable"].Equals("true", icic);

            Exception lastX = null;
            // Attempt call a maximum of 5 times    
            for (var i = 0; i < 5; i++)
            {
                try
                {
                    var r = codeBlock(Proxy);
                    prox.Close();
                    //log.Write(log.Level.Trace, $"Called WCF: {prox.RemoteAddress}.");
                    return r;
                }
                // The following is typically thrown on the client when a 
                // channel is terminated due to the server closing the connection.
                catch (ChannelTerminatedException ctx)
                {
                    if (plannedOutage) throw new PlannedWcfOutageException(ctx.Message, ctx);
                    lastX = ctx;
                    prox.Abort();
                    Thread.Sleep(1000*(i + 1));
                }
                // The following is thrown when a remote endpoint could not be found or reached.  
                // The endpoint may not be found or reachable because the remote endpoint is down,
                // the remote endpoint is unreachable, or because the remote network is unreachable.            
                catch (EndpointNotFoundException nfx)
                {
                    if (plannedOutage) throw new PlannedWcfOutageException(nfx.Message, nfx);
                    lastX = nfx;
                    prox.Abort();
                    Thread.Sleep(1000*(i + 1));
                }
                // The following exception that is thrown when a server is too busy to accept a message.            
                catch (ServerTooBusyException tbx)
                {
                    lastX = tbx;
                    prox.Abort();
                    Thread.Sleep(1000*(i + 1));
                }
                catch (FaultException fX)
                {
                    var pi = fX.GetType().GetProperty("Detail");
                    if (pi != null)
                    {
                        var rd = pi.GetValue(fX);
                        var copFlt = rd.GetType().GetProperty("FaultType");
                        if (copFlt != null)
                        {
                            var flt = copFlt.GetValue(rd);
                            if(Enum.GetName(flt.GetType(), flt) == "MissingData")
                                throw new CoPMissingDataException(fX.Message);
                        }
                    }
                    prox.Abort();
                    throw;
                }
                catch (Exception)
                {
                    prox.Abort();
                    throw;
                }
            }
            // --------------------------------
            prox.Abort();
            throw new CoPException(
                "WCF call failed after 5 retries.",
                lastX);
        }

        /// <summary>
        /// asdasda
        /// </summary>
        /// <param name="codeBlock"></param>
        public void Use(WcfServiceDelegate<I> codeBlock)
        {
            if (!(Proxy is IClientChannel prox)) 
                throw new CoPException("Could not create Wcf proxy.");

            Exception lastX = null;   
            // Attempt call a maximum of 5 times    
            for(var i=0; i<5; i++)  
            {
                try
                {
                    codeBlock(Proxy);
                    prox.Close();
                    break;
                } 
                // The following is typically thrown on the client when a 
                // channel is terminated due to the server closing the connection.
                catch (ChannelTerminatedException ctx) 
                { 
                    lastX = ctx;
                    prox.Abort();                
                    Thread.Sleep(1000 * (i + 1)); 
                }             
                // The following is thrown when a remote endpoint could not be found or reached.  
                // The endpoint may not be found or reachable because the remote endpoint is down,
                // the remote endpoint is unreachable, or because the remote network is unreachable.            
                catch (EndpointNotFoundException nfx)
                {
                    lastX = nfx;
                    prox.Abort();                
                    Thread.Sleep(1000 * (i + 1));             
                }             
                // The following exception that is thrown when a server is too busy to accept a message.            
                catch (ServerTooBusyException tbx)
                {
                    lastX = tbx;                
                    prox.Abort();                 
                    Thread.Sleep(1000 * (i + 1));             
                }             
                catch(Exception)
                {
                    prox.Abort(); 
                    throw;
                }
            }
            if (lastX == null) return;
            // --------------------------------
            prox.Abort();
            throw new CoPException(
                "WCF call failed after 5 retries.",
                lastX);
        } 

        #region IDisposable
        private bool disposed;
        /// <summary>
        /// 
        /// </summary>
        public void Dispose() { Dispose(true); }
        private void Dispose(bool disposeProxy)
        {
            if (disposed) return;
            // -----------------
            if (Proxy != null && disposeProxy)
            {
                // --------------------------------------------
                if (Proxy is ICommunicationObject iComPrxy)
                    if (iComPrxy.State == CommunicationState.Faulted)
                        try { iComPrxy.Abort(); }
                        catch (TimeoutException)       { /* Swallow */ }
                        catch (CommunicationException) { /* Swallow */ }
                    else
                    {
                        try { iComPrxy.Close(); }
                        catch (ChannelTerminatedException)
                        { iComPrxy.Abort(); }
                    }
            }
            disposed = true;
            GC.SuppressFinalize(this);
        }
        ~WcfProxyWrapper() { Dispose(false); }

        #endregion IDisposable
    }
}
