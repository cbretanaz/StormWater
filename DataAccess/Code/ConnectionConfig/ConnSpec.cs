using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Text;
using System.Xml.Serialization;
using Lib = CoP.Enterprise.Utilities;
using dLib = CoP.Enterprise.Data.Utilities;
using log = CoP.Enterprise.DALLog;

namespace CoP.Enterprise.Data
{
    public class ConnSpecs : List<ConnSpec>
    {
        #region private state fields

        #endregion private state fields

        #region public properties

        [XmlIgnore]
        public string ApplicationName { get; set; }

        #endregion public properties

        #region Add override
        public new void Add(ConnSpec connSpec)
        {
            connSpec.ApplcationName = ApplicationName;
            base.Add(connSpec);
        }
        #endregion Add override
    }

    public class ConnSpec
    {
        #region private state fields
        #region general fields
        protected string appNm;
        protected readonly List<APPENV> envs = new List<APPENV>();
        protected string serv;
        protected string datasrc;
        protected int? conTOSecs;
        protected string sPort;
        protected string cat;
        protected string protocol;
        protected string uri;
        protected string filNm;
        protected string logon;
        protected string pwd;
        #endregion general fields

        #region Oracle settings
        protected string tnsAls;
        protected string oSid;
        protected int? conLfTim;
        #endregion Oracle settings

        #region Connection Pool Settings
        protected string sMaxPoolSiz;
        protected string sMinPoolSiz;
        protected string sPoolInc;
        protected string sPoolDec;
        protected string enlst;
        #endregion Connection Pool Settings

        #region Remoting specific fields
        protected string secMode;
        protected int remTO = 120;
        #endregion Remoting state fields

        #region Wcf specific fields
        protected string bnd;
        protected bool ssl;
        protected string cfgRelSesTO;
        protected bool relSes;
        protected TimeSpan? relSesTO;
        protected bool relOrdDel;
        private string xfrMd;
        private string secAlgorSuite;
        private string txprtCredTyp;
        private string msgEncod;
        private string txtEncod;
        private string hostNmCompMd;
        private string msgCredTyp;
        private string prxyCredTyp;
        private string npProtLvl;
        #endregion Wcf specific fields
        #endregion private state fields

        #region public properties
        #region general properties
        [XmlIgnore]
        public string ApplcationName
        {
            get { return appNm; }
            set { appNm = value; }
        }

        #region APPENV code
        [XmlAttribute(DataType = "string",
          AttributeName = "environments")]
        public string cfgEnvironments
        {
            get { return envs.ToString(); }
            set
            {
                var last = ' ';
                var sb = new StringBuilder();
                foreach (var c in value)
                {
                    if (c != ' ' || last != ' ')
                        sb.Append(c);
                    last = c;
                }
                foreach (var c in @",;:/\ ")
                {
                    sb.Replace(" " + c + " ", "|");
                    sb.Replace(" " + c, "|");
                    sb.Replace(c + " ", "|");
                    sb.Replace(c, '|');
                }
                foreach (var s in sb.ToString().Split('|').Where
                    (s => !envs.Contains(Lib.GetAppMode(s))))
                    envs.Add(Lib.GetAppMode(s));
            }
        }
        [XmlIgnore]
        public List<APPENV> Environments { get { return envs; } }
        #endregion APPENV code

        [XmlAttribute(DataType = "string",
          AttributeName = "serverName")]
        public string ServerName
        {
            get
            {
                return serv;
            }
            set { serv = value; }
        }
        [XmlAttribute(DataType = "string",
          AttributeName = "tnsAlias")]
        public string TnsAlias
        {
            get { return tnsAls; }
            set { tnsAls = value; }
        }
        [XmlAttribute(DataType = "int",
          AttributeName = "connectionTimeoutSecs")]
        public int cfgConnTimeOut
        {
            get { return conTOSecs.HasValue ? conTOSecs.Value : 15; }
            set { conTOSecs = value; }
        }
        [XmlIgnore]
        public int? ConnectionTimeout { get { return conTOSecs; } }
        [XmlAttribute(DataType = "int",
          AttributeName = "connectionLifetime")]
        public int cfgConnLifetime
        {
            get { return conLfTim.HasValue ? conLfTim.Value : 0; }
            set { conLfTim = value; }
        }
        [XmlIgnore]
        public int? ConnectionLifetime { get { return conLfTim; } }
        [XmlAttribute(DataType = "string",
          AttributeName = "dataSource")]
        public string DataSource
        {
            get { return datasrc; }
            set { datasrc = value; }
        }
        [XmlAttribute(DataType = "string",
          AttributeName = "oracleSID")]
        public string OracleSID
        {
            get { return oSid; }
            set { oSid = value; }
        }
        [XmlAttribute(DataType = "string",
          AttributeName = "port")]
        public string cfgPort
        {
            get { return sPort; }
            set { sPort = value; }
        }
        [XmlIgnore]
        public int? Port
        {
            get
            {
                int outVal;
                return !string.IsNullOrEmpty(sPort) &&
                       Int32.TryParse(sPort, out outVal) ?
                                outVal : (int?)null;
            }
        }
        [XmlAttribute(DataType = "string",
         AttributeName = "protocol")]
        public string Protocol
        {
            get { return protocol; }
            set { protocol = value; }
        }
        [XmlAttribute(DataType = "string",
         AttributeName = "uri")]
        public string Uri
        {
            get { return uri; }
            set { uri = value; }
        }
        [XmlAttribute(DataType = "string",
         AttributeName = "fileName")]
        public string FileName
        {
            get { return filNm; }
            set { filNm = value; }
        }
        [XmlAttribute(DataType = "string", AttributeName = "userId")]
        public string UserId
        {
            get { return logon; }
            set { logon = value; }
        }

        [XmlAttribute(DataType = "string", AttributeName = "logon")]
        public string Logon
        {
            get { return logon; }
            set { logon = value; }
        }
        [XmlAttribute(DataType = "string",
         AttributeName = "password")]
        public string Password
        {
            get { return pwd; }
            set { pwd = value; }
        }
        #endregion general properties

        #region database Properties
        [XmlIgnore]
        public string DatabaseName
        {
            get { return cat; }
            set { cat = value; }
        }
        [XmlAttribute(DataType = "string",AttributeName = "database")]
        public string cfgDB
        {
            get { return cat; }
            set { cat = value; }
        }
        [XmlAttribute(DataType = "string",AttributeName = "catalog")]
        public string Catalog
        {
            get { return cat; }
            set { cat = value; }
        }

        #region Connection Pool settings
        [XmlAttribute(DataType = "string",
         AttributeName = "maxPoolSize")]
        public string cfgMaxPoolSize
        {
            get { return sMaxPoolSiz; }
            set { sMaxPoolSiz = value; }
        }
        [XmlIgnore]
        public int? MaxPoolSize
        {
            get
            {
                int outVal;
                return !string.IsNullOrEmpty(sMaxPoolSiz) &&
                       Int32.TryParse(sMaxPoolSiz, out outVal) ?
                                outVal : (int?)null;
            }
        }
        [XmlAttribute(DataType = "string",
         AttributeName = "minPoolSize")]
        public string cfgMinPoolSize
        {
            get { return sMinPoolSiz; }
            set { sMinPoolSiz = value; }
        }
        [XmlIgnore]
        public int? MinPoolSize
        {
            get
            {
                int outVal;
                return !string.IsNullOrEmpty(sMinPoolSiz) &&
                       Int32.TryParse(sMinPoolSiz, out outVal) ?
                                outVal : (int?)null;
            }
        }
        // ------------------------------
        [XmlAttribute(DataType = "string",
         AttributeName = "poolSizeIncrement")]
        public string cfgPoolSizInc
        {
            get { return sPoolInc; }
            set { sPoolInc = value; }
        }
        [XmlIgnore]
        public int? PoolSizeIncrement
        {
            get
            {
                int outVal;
                return !string.IsNullOrEmpty(sPoolInc) &&
                       Int32.TryParse(sPoolInc, out outVal) ?
                                outVal : (int?)null;
            }
        }
        // ------------------------------
        [XmlAttribute(DataType = "string",
         AttributeName = "poolSizeDecrement")]
        public string cfgPoolSizDec
        {
            get { return sPoolDec; }
            set { sPoolDec = value; }
        }
        [XmlIgnore]
        public int? PoolSizeDecrement
        {
            get
            {
                int outVal;
                return !string.IsNullOrEmpty(sPoolDec) &&
                       Int32.TryParse(sPoolDec, out outVal) ?
                                outVal : (int?)null;
            }
        }
        [XmlAttribute(DataType = "string",
         AttributeName = "enlist")]
        public string cfgEnlist
        {
            get { return enlst; }
            set { enlst = value; }
        }
        [XmlIgnore]
        public dLib.OraEnlist Enlist
        {
            get
            {
                switch (cfgEnlist.ToLower())
                {
                    case ("false"): return dLib.OraEnlist.False;
                    case ("dynamic"): return dLib.OraEnlist.Dynamic;
                    default: return dLib.OraEnlist.True;
                }
            }
        }
        #endregion Connection Pool settings

        #endregion database Properties

        #region Remoting specific properties
        [XmlAttribute(DataType = "int",
         AttributeName = "remotingTimeout")]
        public int RemotingTimeout
        {
            get { return remTO; }
            set { remTO = value; }
        }
        #endregion Remoting specific properties

        #region Wcf specific Properties
        [XmlAttribute(DataType = "boolean",AttributeName = "ssl")]
        public bool SSL
        {
            get { return ssl; }
            set { ssl = value; }
        }


        [XmlAttribute(DataType = "string",AttributeName = "binding")]
        public string cfgBinding
        {
            get { return bnd; }
            set { bnd = value; }
        }
        [XmlIgnore]
        public WcfBinding WcfBinding
        {
            get
            {
                if (String.IsNullOrEmpty(cfgBinding))
                    return WcfBinding.None;
                switch (cfgBinding.ToUpper())
                {
                    case "BASICHTTPBINDING": return WcfBinding.BasicHttp;
                    case "NETTCPBINDING": return WcfBinding.NetTcp;
                    case "NETNAMEDPIPEBINDING": return WcfBinding.NetNamedPipe;
                    case "WSHTTPBINDING": return WcfBinding.WSHttp;
                    case "WSDUALHTTPBINDING": return WcfBinding.WSDualHttp;
                    case "NETMSMQBINDING": return WcfBinding.NetMsmq;
                    case "CUSTOMBINDING": return WcfBinding.Custom;
                    default: return WcfBinding.None;
                }
            }
        }
        // ----------------------------------------
        [XmlAttribute(DataType = "string", AttributeName = "transferMode")]
        public string cfgXfrMode
        {
            get { return xfrMd; }
            set { xfrMd = value; }
        }
        [XmlIgnore]
        public TransferMode TransferMode
        {
            get
            {
                if (String.IsNullOrEmpty(xfrMd))
                    return TransferMode.Buffered;
                switch (xfrMd.ToUpper())
                {
                    case "BUFFERED": return TransferMode.Buffered;
                    case "STREAMED": return TransferMode.Streamed;
                    case "STREAMEDREQUEST": return TransferMode.StreamedRequest;
                    case "STREAMEDRESPONSE": return TransferMode.StreamedResponse;
                    default: return TransferMode.Buffered;
                }
            }
        }
        // ---------------------------------------


        #region security settings
        [XmlAttribute(DataType = "boolean", AttributeName = "establishSecurityContext")]
        public bool EstablishSecurityContext { get; set; }
        // ----------------------------------------
        [XmlAttribute(DataType = "boolean", AttributeName = "allowCookies")]
        public bool AllowCookies { get; set; }
        // ----------------------------------------
        [XmlAttribute(DataType = "boolean", AttributeName = "bypassProxyOnLocal")]
        public bool BypassProxyOnLocal { get; set; }
        // ----------------------------------------

        [XmlAttribute(DataType = "string", AttributeName = "messageEncoding")]
        public string cfgMsgEncoding
        {
            get { return msgEncod; }
            set { msgEncod = value; }
        }
        [XmlIgnore]
        public WSMessageEncoding MessageEncoding
        {
            get
            {
                if (String.IsNullOrEmpty(msgEncod))
                    return WSMessageEncoding.Text;
                switch (msgEncod.ToUpper())
                {
                    case "MTOM": return WSMessageEncoding.Mtom;
                    case "TEXT": return WSMessageEncoding.Text;
                    default: return WSMessageEncoding.Text;
                }
            }
        }
        // ---------------------------------------

        [XmlAttribute(DataType = "string", AttributeName = "textEncoding")]
        public string cfgTxtEncoding
        {
            get { return txtEncod; }
            set { txtEncod = value; }
        }
        [XmlIgnore]
        public Encoding TextEncoding
        {
            get
            {
                if (String.IsNullOrEmpty(txtEncod))
                    return Encoding.UTF8;
                switch (txtEncod.ToUpper())
                {
                    case "ASCII": return Encoding.ASCII;
                    case "BIGENDIANUNICODE": return Encoding.BigEndianUnicode;
                    case "DEFAULT": return Encoding.Default;
                    case "UNICODE": return Encoding.Unicode;
                    case "UTF32": return Encoding.UTF32;
                    case "UTF7": return Encoding.UTF7;
                    case "UTF8": return Encoding.UTF8;
                    default: return Encoding.UTF8;
                }
            }
        }
        // ---------------------------------------

        [XmlAttribute(DataType = "string", AttributeName = "hostNameComparisonMode")]
        public string cfgHostNmCompMd
        {
            get { return hostNmCompMd; }
            set { hostNmCompMd = value; }
        }
        [XmlIgnore]
        public HostNameComparisonMode HostNameComparisonMode
        {
            get
            {
                if (String.IsNullOrEmpty(hostNmCompMd))
                    return HostNameComparisonMode.WeakWildcard;
                switch (hostNmCompMd.ToUpper())
                {
                    case "EXACT": return HostNameComparisonMode.Exact;
                    case "STRONGWILDCARD": return HostNameComparisonMode.StrongWildcard;
                    case "WEAKWILDCARD": return HostNameComparisonMode.WeakWildcard;
                    default: return HostNameComparisonMode.WeakWildcard;
                }
            }
        }
        // ---------------------------------------

        [XmlAttribute(DataType = "string", AttributeName = "securityAlgorithmSuite")]
        public string cfgsecAlgorithmSuite
        {
            get { return secAlgorSuite; }
            set { secAlgorSuite = value; }
        }
        [XmlIgnore]
        public SecurityAlgorithmSuite SecurityAlgorithmSuite
        {
            get
            {
                if (String.IsNullOrEmpty(secAlgorSuite))
                    return SecurityAlgorithmSuite.Default;          
                switch (secAlgorSuite.ToUpper())
                {
                    case "BASIC128": return SecurityAlgorithmSuite.Basic128;
                    case "BASIC128RSA15": return SecurityAlgorithmSuite.Basic128Rsa15;
                    case "BASIC128SHA256": return SecurityAlgorithmSuite.Basic128Sha256;
                    case "BASIC128SHA256RSA15": return SecurityAlgorithmSuite.Basic128Sha256Rsa15;
                    case "BASIC192": return SecurityAlgorithmSuite.Basic192;
                    case "BASIC192RSA15": return SecurityAlgorithmSuite.Basic192Rsa15;
                    case "BASIC192SHA256": return SecurityAlgorithmSuite.Basic192Sha256;
                    case "BASIC192SHA256RSA15": return SecurityAlgorithmSuite.Basic192Sha256Rsa15;
                    case "BASIC256": return SecurityAlgorithmSuite.Basic256;
                    case "BASIC256RSA15": return SecurityAlgorithmSuite.Basic256Rsa15;
                    case "BASIC256SHA256": return SecurityAlgorithmSuite.Basic256Sha256;
                    case "BASIC256SHA256RSA15": return SecurityAlgorithmSuite.Basic256Sha256Rsa15;
                    case "DEFAULT": return SecurityAlgorithmSuite.Default;
                    default: return SecurityAlgorithmSuite.Default;
                }
            }
        }
        // ---------------------------------------

        
        [XmlAttribute(DataType = "boolean", AttributeName = "reliableSessionEnabled")]
        public bool ReliableSession
        {
            get { return relSes; }
            set { relSes = value; }
        }
        // ----------------------------------------

        [XmlAttribute(DataType = "boolean", AttributeName = "durable")]
        public bool Durable { get; set; }
        // ----------------------------------------

        [XmlAttribute(DataType = "boolean", AttributeName = "useDefaultWebProxy")]
        public bool UseDefaultWebProxy { get; set; }
        // ----------------------------------------
         
        [XmlAttribute(DataType = "string", AttributeName = "reliableSessionTimeout")]
        internal string cfgReliableSessionTimeout
        {
            get { return cfgRelSesTO; }
            set
            {
                cfgRelSesTO = value;
                TimeSpan ts;
                if (TimeSpan.TryParse(value, out ts))
                    relSesTO = ts;
            }
        }
        [XmlIgnore]
        public TimeSpan? ReliableSessionTimeout
        { get { return relSesTO.GetValueOrDefault(TimeSpan.FromMinutes(10)); } }
        // ----------------------------------------
      
        [XmlAttribute(DataType = "boolean", AttributeName = "requireOrderedDelivery")]
        public bool RequireOrderedDelivery
        {
            get { return relOrdDel; }
            set { relOrdDel = value; }
        }
        // ----------------------------------------

        [XmlAttribute(DataType = "string",AttributeName = "transportClientCredentialType")]
        public string cfgTxprtClntCred
        {
            get { return txprtCredTyp; }
            set { txprtCredTyp = value; }
        }
        [XmlIgnore]
        public HttpClientCredentialType TransportCredentialType
        {
            get
            {
                if (WcfBinding != WcfBinding.BasicHttp &&
                    WcfBinding != WcfBinding.WSHttp)
                    throw new CoPDataConfigurationException(
                        "Attempting to assign BasicHttpSecurityMode " +
                        " but binding is not BasicHttp or WsHttp Binding");
                if (string.IsNullOrEmpty(txprtCredTyp)) 
                    return HttpClientCredentialType.None;
                switch (txprtCredTyp.ToUpper())
                {
                    case "NONE":    return HttpClientCredentialType.None;
                    case "BASIC":   return HttpClientCredentialType.Basic;
                    case "CERTIFICATE": return HttpClientCredentialType.Certificate;
                    case "DIGEST":  return HttpClientCredentialType.Digest;
                    case "NTLM":    return HttpClientCredentialType.Ntlm;
                    case "WINDOWS": return HttpClientCredentialType.Windows;
                    default:        return HttpClientCredentialType.None;
                }
            }
        }
        [XmlIgnore]
        public TcpClientCredentialType TcpClientCredentialType
        {
            get
            {
                if (WcfBinding != WcfBinding.NetTcp)
                    throw new CoPDataConfigurationException(
                        "Attempting to assign TcpClientCredentialType " +
                        " but binding is not NetTcp");
                if (string.IsNullOrEmpty(txprtCredTyp))
                    return TcpClientCredentialType.None;
                switch (txprtCredTyp.ToUpper())
                {
                    case "NONE": return TcpClientCredentialType.None;
                    case "CERTIFICATE": return TcpClientCredentialType.Certificate;
                    case "WINDOWS": return TcpClientCredentialType.Windows;
                    default: return TcpClientCredentialType.None;
                }
            }
        }
        // ------------------------------------------------------

        [XmlAttribute(DataType = "string", AttributeName = "messageClientCredentialType")]
        public string cfgMsgClntCred
        {
            get { return msgCredTyp; }
            set { msgCredTyp = value; }
        }
        [XmlIgnore]
        public MessageCredentialType MessageCredentialType
        {
            get
            {
                if (WcfBinding != WcfBinding.WSHttp && WcfBinding != WcfBinding.NetTcp)
                    throw new CoPDataConfigurationException(
                        "Attempting to assign BasicHttpSecurityMode " +
                        " but binding is not WsHttp Binding");
                if (string.IsNullOrEmpty(msgCredTyp))
                    return MessageCredentialType.None;
                switch (msgCredTyp.ToUpper())
                {
                    case "NONE": return MessageCredentialType.None;
                    case "CERTIFICATE": return MessageCredentialType.Certificate;
                    case "ISSUEDTOKEN": return MessageCredentialType.IssuedToken;
                    case "USERNAME": return MessageCredentialType.UserName;
                    case "WINDOWS": return MessageCredentialType.Windows;
                    default: return MessageCredentialType.None;
                }
            }
        }
        [XmlIgnore]
        public BasicHttpMessageCredentialType BasicHttpMessageCredentialType
        {
            get
            {
                if (WcfBinding != WcfBinding.BasicHttp)
                    throw new CoPDataConfigurationException(
                        "Attempting to assign BasicHttpSecurityMode " +
                        " but binding is not BasicHttp");
                return (string.IsNullOrEmpty(msgCredTyp) || msgCredTyp.ToUpper() == "USERNAME")?
                        BasicHttpMessageCredentialType.Certificate:
                        BasicHttpMessageCredentialType.UserName;
            }
        }
        // ------------------------------------------------------

        [XmlAttribute(DataType = "string",AttributeName = "proxyCredentialType")]
        public string cfgPrxyCred
        {
            get { return prxyCredTyp; }
            set { prxyCredTyp = value; }
        }
        [XmlIgnore]
        public HttpProxyCredentialType ProxyCredentialType
        {
            get
            {
                if (WcfBinding != WcfBinding.BasicHttp &&
                    WcfBinding != WcfBinding.WSHttp)
                    throw new CoPDataConfigurationException(
                        "Attempting to assign ProxyCredentialType " +
                        " but binding is not BasicHttp or WsHttp Binding");
                if (string.IsNullOrEmpty(prxyCredTyp))
                    return HttpProxyCredentialType.None;
                switch (prxyCredTyp.ToUpper())
                {
                    case "NONE": return HttpProxyCredentialType.None;
                    case "BASIC": return HttpProxyCredentialType.Basic;
                    case "DIGEST": return HttpProxyCredentialType.Digest;
                    case "NTLM": return HttpProxyCredentialType.Ntlm;
                    case "WINDOWS": return HttpProxyCredentialType.Windows;
                    default: return HttpProxyCredentialType.None;
                }
            }
        }
        // ------------------------------------------------------
        [XmlAttribute(DataType = "string", AttributeName = "serverSideUPN")]
        public string ServerSideUPN { get; set; }

        [XmlAttribute(DataType = "string",AttributeName = "securityMode")]
        public string cfgSecMode
        {
            get { return secMode; }
            set { secMode = value; }
        }
        [XmlIgnore]
        public RemSecMode RemotingSecurityMode
        {
            get
            {
                switch (cfgSecMode.ToLower())
                {
                    case "anonymous": return RemSecMode.Anonymous;
                    case "identity": return RemSecMode.Identify;
                    case "byrole": return RemSecMode.ByRole;
                    default: return RemSecMode.Anonymous;
                }
            }
        }
        [XmlIgnore]
        public BasicHttpSecurityMode BasicSecurityMode
        {
            get
            {
                if(WcfBinding != WcfBinding.BasicHttp)
                    throw new CoPDataConfigurationException(
                        "Attempting to assign BasicHttpSecurityMode " +
                        " but binding is not BasicHttp Binding");
                if (string.IsNullOrEmpty(cfgSecMode))
                    return BasicHttpSecurityMode.None;
                switch (cfgSecMode.ToUpper())
                {
                    case "MESSAGE": return BasicHttpSecurityMode.Message;
                    case "TRANSPORT": return BasicHttpSecurityMode.Transport;
                    case "TRANSPORTCREDENTIALONLY": return BasicHttpSecurityMode.TransportCredentialOnly;
                    case "TRANSPORTWITHMESSAGECREDENTIAL": return BasicHttpSecurityMode.TransportWithMessageCredential;
                    case "NONE": return BasicHttpSecurityMode.None;
                    default: return BasicHttpSecurityMode.None;
                }
            }
        } 
        [XmlIgnore]
        public SecurityMode SecurityMode
        {
            get
            {
                if(WcfBinding != WcfBinding.WSHttp)
                    throw new CoPDataConfigurationException(
                        "Attempting to assign SecurityMode " +
                        " but binding is not WSHttp Binding");
                if (string.IsNullOrEmpty(cfgSecMode))
                    return SecurityMode.None;
                switch (cfgSecMode.ToUpper())
                {
                    case "MESSAGE": return SecurityMode.Message;
                    case "TRANSPORT": return SecurityMode.Transport;
                    case "TRANSPORTWITHMESSAGECREDENTIAL": return SecurityMode.TransportWithMessageCredential;
                    case "NONE": return SecurityMode.None;
                    default: return SecurityMode.None;
                }
            }
        }
        [XmlIgnore]
        public SecurityMode TcpSecurityMode
        {
            get
            {
                if (WcfBinding != WcfBinding.NetTcp)
                    throw new CoPDataConfigurationException(
                        "Attempting to assign SecurityMode " +
                        " but binding is not NetTcp Binding");
                if (string.IsNullOrEmpty(cfgSecMode))
                    return SecurityMode.None;
                switch (cfgSecMode.ToUpper())
                {
                    case "MESSAGE": return SecurityMode.Message;
                    case "TRANSPORT": return SecurityMode.Transport;
                    case "TRANSPORTWITHMESSAGECREDENTIAL": return SecurityMode.TransportWithMessageCredential;
                    case "NONE": return SecurityMode.None;
                    default: return SecurityMode.None;
                }
            }
        }
        [XmlIgnore]
        public WSDualHttpSecurityMode WSDualSecurityMode
        {
            get
            {
                if (WcfBinding != WcfBinding.WSHttp)
                    throw new CoPDataConfigurationException(
                        "Attempting to assign SecurityMode " +
                        " but binding is not WSHttp Binding");
                return (!string.IsNullOrEmpty(cfgSecMode) && cfgSecMode.ToUpper() == "MESSAGE")?
                    WSDualHttpSecurityMode.Message: WSDualHttpSecurityMode.None;
            }
        }
        [XmlIgnore]
        public NetNamedPipeSecurityMode NamedPipeSecurityMode
        {
            get
            {
                if (WcfBinding != WcfBinding.WSHttp)
                    throw new CoPDataConfigurationException(
                        "Attempting to assign SecurityMode " +
                        " but binding is not WSHttp Binding");
                return (!string.IsNullOrEmpty(cfgSecMode) && cfgSecMode.ToUpper() == "TRANSPORT") ?
                    NetNamedPipeSecurityMode.Transport: NetNamedPipeSecurityMode.None ;
            }
        }
        // -----------------------------------------------

        [XmlAttribute(DataType = "string", AttributeName = "namedPipeProtectionLevel")]
        public string cfgnpProtLvl
        {
            get { return npProtLvl; }
            set { npProtLvl = value; }
        }
        [XmlIgnore]
        public ProtectionLevel NamedPipeProtectionLevel
        {
            get
            {
                if (WcfBinding != WcfBinding.NetNamedPipe)
                    throw new CoPDataConfigurationException(
                        "Attempting to assign ProtectionLevel " +
                        " but binding is not NetNamedPipe Binding");

                if (string.IsNullOrEmpty(npProtLvl))
                    return ProtectionLevel.None;
                switch (npProtLvl.ToUpper())
                {
                    case "ENCRYPTANDSIGN": return ProtectionLevel.EncryptAndSign;
                    case "SIGN": return ProtectionLevel.Sign;
                    default: return ProtectionLevel.None;
                }
            }
        }
        // -----------------------------------------------

        
        #endregion security settings

        #region Max Wcf Sizes
        [XmlAttribute(DataType = "int", AttributeName = "maxBufferPoolSize")]
        public int MaxBufferPoolSize { get; set; }
        [XmlAttribute(DataType = "int", AttributeName = "maxBufferSize")]
        public int MaxBufferSize { get; set; }
        [XmlAttribute(DataType = "int", AttributeName = "maxReceivedMessageSize")]
        public int MaxReceivedMessageSize { get; set; }
        [XmlAttribute(DataType = "int", AttributeName = "maxStringContentLength")]
        public int MaxStringContentLength { get; set; }
        [XmlAttribute(DataType = "int", AttributeName = "maxDepth")]
        public int MaxDepth { get; set; }
        [XmlAttribute(DataType = "int", AttributeName = "maxArrayLength")]
        public int MaxArrayLength { get; set; }
        [XmlAttribute(DataType = "int", AttributeName = "maxBytesPerRead")]
        public int MaxBytesPerRead { get; set; }
        [XmlAttribute(DataType = "int", AttributeName = "maxNameTableCharCount")]
        public int MaxNameTableCharCount { get; set; }
        [XmlAttribute(DataType = "int", AttributeName = "maxConnections")]
        public int MaxConnections { get; set; }
        [XmlAttribute(DataType = "int", AttributeName = "maxRetryCycles")]
        public int MaxRetryCycles { get; set; }
        [XmlIgnore]
        public int? MaxItemsInObjectGraph { private set; get; }
        [XmlAttribute(DataType = "int", AttributeName = "maxItemsInObjectGraph")]
        public int MxItmsNObjtGrph
        {
            // 65536 is NOT the documented default value for this property, but the actual default
            // value for this property
            get { return MaxItemsInObjectGraph.HasValue ? MaxItemsInObjectGraph.Value : 65536; }
            set { MaxItemsInObjectGraph = value; }
        }
        #endregion Max Wcf Sizes

        #endregion Wcf specific Properties

        #endregion public properties
    }
}
