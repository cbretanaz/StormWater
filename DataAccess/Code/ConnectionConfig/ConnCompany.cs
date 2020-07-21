using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Lib = CoP.Enterprise.Utilities;
using log = CoP.Enterprise.DALLog;

namespace CoP.Enterprise.Data
{
    public class ConnCompanys : List<ConnCompany>
    {
        public bool Contains(string companyName)
        {
            return this.Any(comp =>
              string.Compare(comp.CompanyName, companyName, true) == 0);
        }

        public ConnCompany this[string companyName]
        {
            get
            {
                foreach (var comp in
                    this.Where(comp =>
                        string.Compare(comp.CompanyName,
                        companyName, true) == 0))
                {
                    return comp;
                }
                throw new CoPDataConfigurationException(string.Format(
                    "Cannot locate Company Configuration section " +
                    "for Company: {0}", companyName));
            }
        }
    }

    public class ConnCompany
    {
        #region private state fields

        #endregion private state fields

        #region public properties

        [XmlAttribute(DataType = "string", AttributeName = "companyName")]
        public string CompanyName { get; set; }

        [XmlArrayItem(ElementName = "ConnApp")]
        public ConnApps ConnApps { get; set; }

        #endregion public properties

        #region indexers
        public ConnApp this[string applicationName]
        { get { return ConnApps[applicationName]; } }
        public ConnSpec this[string applicationName, APPENV appEnv]
        {
            get
            {
                foreach (var con in this[applicationName].ConnSpecs.Where(
                            con => con.Environments.Contains(appEnv)))
                    return con;
                throw new CoPDataConfigurationException(string.Format(
                    "Cannot locate ConnSpec Configuration section " +
                    "for Application.Environment: {0}.{1}",
                    applicationName, appEnv));
            }
        }
        #endregion indexers
    }
}