using System;
using System.Configuration;
using System.Xml.Serialization;
using CoP.Enterprise;
using cfgMgr = System.Configuration.ConfigurationManager;
using ser = CoP.Enterprise.Serialization;

namespace BES.SWMM.PAC
{
    [XmlRoot("PACConfig")]
    public class PACConfig
    {
        private static readonly object locker = new object();

        #region singleton stuff
        private static string TypeName => "PACConfig";
        private static PACConfig pacCfg;

        public static PACConfig Make()
        {
            if (pacCfg != null) return pacCfg;
            // -----------------------------------------
            var pC = cfgMgr.GetSection("PACConfig");
            lock (locker)
            {
                pacCfg = pC as PACConfig ?? throw new CoPException(
                             "Unable to load PACConfig section.");
                pacCfg.Environment = cfgMgr.AppSettings["Environment"];
                return pacCfg;
            }
        }

        public object this[int idx]
        {
            get => null;
            set {; }
        }
        #endregion singleton stuff

        [XmlAttribute(DataType = "string", AttributeName = "configType")]
        public string ConfigType { get; set; }
        public string Environment { get; set; }
        public VersionInfo VersionInfo { get; set; }
        public EngineSettings EngineSettings { get; set; }

        public void UpdateFrom(PACConfig other)
        {
            VersionInfo = other.VersionInfo;
            EngineSettings = other.EngineSettings;
        }

        public static void Save()
        {
            var cfgFilSpec = cfgMgr.OpenExeConfiguration(ConfigurationUserLevel.None).
                Sections[TypeName].SectionInformation.ConfigSource;

            lock (locker)
                ser.Serialize2File(pacCfg,
                    cfgFilSpec, ser.Formatter.Xml);
        }
    }
    public class VersionInfo
    {
        [XmlAttribute(DataType = "string", AttributeName = "pacVersion")]
        public string PACVersion { get; set; }

        [XmlAttribute(DataType = "date", AttributeName = "qaReleaseDate")]
        public DateTime QAReleaseDate { get; set; }
        [XmlAttribute(DataType = "string", AttributeName = "prodReleaseDate")]
        public string prdRelCfg { get; set; }
        [XmlIgnore]
        public DateTime? ProductionReleaseDate =>
            string.IsNullOrEmpty(prdRelCfg) ||
            !DateTime.TryParse(prdRelCfg, out var prodRelDt) ?
                (DateTime?)null : prodRelDt;
    }

    public class EngineSettings
    {
        [XmlAttribute(AttributeName = "timeStepMinutes", DataType = "int")]
        public int TimeStepMinutes { get; set; }
        public int TimeStep => TimeStepMinutes * 60;
        [XmlAttribute(AttributeName = "mediaPorosity", DataType = "decimal")]
        public decimal mediaPorosity { get; set; }
        [XmlAttribute(AttributeName = "infiltrationRate", DataType = "decimal")]
        public decimal InfilitrationRate { get; set; }

        [XmlAttribute(AttributeName = "orificeCoefficient", DataType = "decimal")]
        public decimal OrificeCoefficient { get; set; }
        [XmlAttribute(AttributeName = "flowControlRequired", DataType = "boolean")]
        public bool FlowControlRequired { get; set; }
        [XmlAttribute(AttributeName = "rainfallTimesteps", DataType = "int")]
        public int MaxRainfallTimesteps { get; set; }
        [XmlAttribute(AttributeName = "maxTimesteps", DataType = "int")]
        public int MaxTimesteps { get; set; }
        [XmlAttribute(AttributeName = "headAverageLookbackTimesteps", DataType = "int")]
        public int HeadAverageLookbackTimesteps { get; set; }
    }
}
