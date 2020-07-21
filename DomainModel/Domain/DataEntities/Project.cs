using System;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace BES.SWMM.PAC
{
    [Serializable]
    public class Project
    {
        #region Dirty Binding & state Properties
        [XmlIgnore]
        public bool IsBinding { get; set; }
        [XmlIgnore]
        public bool IsDirty { get; set; }
        public void SetIsBinding(bool isBndng)
        {
            IsBinding = isBndng;
            Catchments.SetIsBinding(isBndng);
        }
        #endregion Dirty Binding & state Properties

        #region Project Properties
        [XmlAttribute(AttributeName="name", DataType="string")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "address1", DataType = "string")]
        public string Address1 { get; set; }
        [XmlAttribute(AttributeName = "address2", DataType = "string")]
        public string Address2 { get; set; }
        [XmlAttribute(AttributeName = "city", DataType = "string")]
        public string City { get; set; }
        [XmlAttribute(AttributeName = "state", DataType = "string")]
        public string State { get; set; }
        [XmlAttribute(AttributeName = "z1p", DataType = "string")]
        public string Zip { get; set; }
        [XmlAttribute(AttributeName = "permit", DataType = "string")]
        public string Permit { get; set; }
        [XmlAttribute(AttributeName = "designer", DataType = "string")]
        public string Designer { get; set; }
        [XmlAttribute(AttributeName = "company", DataType = "string")]
        public string Company { get; set; }
        [XmlAttribute(AttributeName = "summary", DataType = "string")]
        public string Summary { get; set; }
        [XmlAttribute(AttributeName = "created", DataType = "string")]
        public DateTime CreatedUTC { get; set; }
        [XmlAttribute(AttributeName = "modified", DataType = "string")]
        public DateTime ModifiedUTC { get; set; }
        public Catchments Catchments { get; set; }
        #endregion Project Properties

        #region factory/ctors
        //public Project() { } // disable instantiation
        public static Project Empty => new Project();

        public static Project Make(string name) { return new Project {Name = name}; }
        public static Project Make(string name, string addr1, string addr2,
            string city, string state, string zip,
            string permit, string desgnr, string company, string summary,
            DateTime crtd, DateTime modfd, Catchments catchments = null)
        {
            return new Project
            {
                Name = name, Address1 = addr1, Address2 = addr2,
                City = city, State = state, Zip = zip, 
                Permit = permit, Designer = desgnr,
                Company = company, Summary = summary,
                CreatedUTC = crtd, ModifiedUTC = modfd,
                Catchments = catchments?? Catchments.Empty
            };
        }
        #endregion factory/ctors
    }
}