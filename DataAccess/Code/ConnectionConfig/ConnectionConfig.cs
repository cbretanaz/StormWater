using System.Xml.Serialization;

namespace CoP.Enterprise.Data
{
    [XmlRoot("ConnectionConfig")]
    public class ConnectionConfig
    {
        [XmlArrayItem(ElementName = "ConnCompany")]
        public ConnCompanys ConnCompanys { get; set; }

        public ConnApp this[string CompanyName, string AppName] 
            => ConnCompanys[CompanyName][AppName];

        public ConnSpec this[string CompanyName, string AppName, APPENV env]
        {
            get
            {
                if (!ConnCompanys.Contains(CompanyName))
                    throw new CoPDataConfigurationException(
                        "Cannot locate ConnSpec Configuration " +
                        $"section for Company: {CompanyName}");
                if (!ConnCompanys[CompanyName].ConnApps.Contains(AppName))
                    throw new CoPDataConfigurationException(
                        "Cannot locate ConnSpec Configuration section " +
                        $"for Company.Application: {CompanyName}.{AppName}");
                if (!ConnCompanys[CompanyName].ConnApps.Contains(AppName))
                    throw new CoPDataConfigurationException(
                        "Cannot locate ConnSpec Configuration section " +
                        $"for Company.Application.Environment: {CompanyName}.{AppName}.{env}");

                return ConnCompanys[CompanyName][AppName, env];
            }
        }
    }
}