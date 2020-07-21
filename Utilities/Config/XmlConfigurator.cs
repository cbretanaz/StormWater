using System;
using System.Configuration;
using System.Xml;
using System.Xml.Serialization;

namespace CoP.Enterprise.Configuration
{
    public class XmlConfigurator : IConfigurationSectionHandler
    {
        public object Create(object parent, object configContext, XmlNode section)
        {
            try
            {
                var xNav = section.CreateNavigator();
                // -------------------------------------------
                var cfgTypAttrib = (string)(xNav).Evaluate("string(@configType)");
                if (string.IsNullOrEmpty(cfgTypAttrib))
                    throw new ApplicationException(
                        "Unable to read [configType] attribute in " +
                        "custom configuration section.");
                // -------------------------------------------
                var sectionType = Type.GetType(cfgTypAttrib);
                if (sectionType == null)
                    throw new ApplicationException(
                        $"Unrecognized configType string [{cfgTypAttrib}] in " +
                        "custom configuration section.");
                var xs = new XmlSerializer(sectionType);
                return xs.Deserialize(new XmlNodeReader(section));
            }
            catch (Exception X)
            {
                throw new ConfigurationErrorsException(
                    "Invalid or missing configuration section " +
                    "provided to XmlConfigurator " + X.Message + 
                    " " + X.InnerException );
            }
        }
    }
}