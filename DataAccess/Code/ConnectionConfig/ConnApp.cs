using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using lib = CoP.Enterprise.Utilities;
using log = CoP.Enterprise.DALLog;

namespace CoP.Enterprise.Data
{
    public class ConnApps : List<ConnApp>
    {
        public bool Contains(string applicationName)
        {
            return this.Any(app => string.Compare(
              app.ApplicationName, applicationName, true) == 0);
        }

        public ConnApp this[string applicationName]
        {
            get
            {
                foreach (var app in
                    this.Where(app => string.Compare(app.ApplicationName,
                        applicationName, true) == 0))
                    return app;
                throw new CoPDataConfigurationException(string.Format(
                    "Cannot locate ConnSpec Configuration section " +
                    "for Application: {0}", applicationName));
            }
        }
    }

    public class ConnApp
    {
        #region private state fields
        private string appNm;
        // --------------------

        #endregion private state fields

        #region public properties
        [XmlAttribute(DataType = "string",
            AttributeName = "applicationName")]
        public string ApplicationName
        {
            get { return appNm; }
            set { ConnSpecs.ApplicationName = appNm = value; }
        }

        [XmlAttribute(DataType = "string",
            AttributeName = "vendorName")]
        public string VendorName { get; set; }

        // -----------------------------
        public ConnSpecs ConnSpecs { get; set; }

        #endregion public properties

        public ConnSpec this[string environment]
        { get { return this[lib.GetAppMode(environment)]; } }
        public ConnSpec this[APPENV appEnv]
        {
            get
            {
                foreach (var con in ConnSpecs.Where(
                    con => con.Environments.Contains(appEnv)))
                    return con;
                throw new CoPDataConfigurationException(string.Format(
                    "Cannot locate ConnSpec Configuration section " +
                    "for Environment: {0}", appEnv));
            }
        }
    }
}