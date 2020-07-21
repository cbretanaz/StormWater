using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using BES.SWMM.PAC.Annotations;
using Newtonsoft.Json;

namespace BES.SWMM.PAC
{
    [Serializable]
    public class BelowGradeProperties : Constants, INotifyPropertyChanged, IHaveOrifice//, IXmlSerializable
    {
        #region Dirty Binding & state Properties
        [XmlIgnore]
        public bool IsBinding { get; set; }
        [XmlIgnore]
        public bool IsDirty { get; set; }
        [XmlIgnore]
        public bool IsAnyDirty => IsDirty || Orifice.IsDirty;
        #endregion Dirty Binding & state Properties

        #region ControlArray Determinents
        private static readonly Dictionary<Configuration, BGP> SlopedControls =
            new Dictionary<Configuration, BGP>
            {
                {cfgA, BGP.IP|BGP.C2S},
                {cfgB, BGP.IP|BGP.C2S|BGP.RSD|BGP.RP|BGP.RAW},
                {cfgC, BGP.IP|BGP.RSD|BGP.RP|BGP.UDH|BGP.ORF|BGP.RAW}
                ,
                {cfgD, BGP.RSD|BGP.RP|BGP.UDH|BGP.ORF|BGP.RAW},
                {cfgE, BGP.IP|BGP.C2S|BGP.RSD|BGP.RP|BGP.RAW},
                {cfgF, BGP.IP|BGP.C2S|BGP.RSD|BGP.RP|BGP.UDH|BGP.RAW}
            };
        private static readonly Dictionary<Configuration, BGP> FlatPlanterControls =
            new Dictionary<Configuration, BGP>
            {
                {cfgA, BGP.IP|BGP.C2S},
                {cfgB, BGP.IP|BGP.C2S|BGP.RSD|BGP.RP|BGP.RAW},
                {cfgC, BGP.IP|BGP.RSD|BGP.RP|BGP.UDH|BGP.ORF|BGP.RAW},
                {cfgD, BGP.RSD|BGP.RP|BGP.UDH|BGP.ORF|BGP.RAW},
                {cfgE, BGP.IP|BGP.C2S|BGP.RSD|BGP.RP|BGP.RAW},
                {cfgF, BGP.IP|BGP.C2S|BGP.RSD|BGP.RP|BGP.UDH|BGP.RAW}
            };
        private static readonly Dictionary<Configuration, BGP> RectBasinControls =
            new Dictionary<Configuration, BGP>
            {
                {cfgA, BGP.IP|BGP.C2S},
                {cfgB, BGP.IP|BGP.C2S|BGP.RSD|BGP.RP|BGP.RAW},
                {cfgC, BGP.IP|BGP.RSD|BGP.RP|BGP.UDH|BGP.ORF|BGP.RAW},
                {cfgD, BGP.RSD|BGP.RP|BGP.UDH|BGP.ORF|BGP.RAW},
                {cfgE, BGP.IP|BGP.C2S|BGP.RSD|BGP.RP|BGP.RAW},
                {cfgF, BGP.IP|BGP.C2S|BGP.RSD|BGP.RP|BGP.UDH|BGP.RAW}
            };
        private static readonly Dictionary<Configuration, BGP> AmoebaBasinControls =
            new Dictionary<Configuration, BGP>
            {
                {cfgA, BGP.IP|BGP.C2S},
                {cfgB, BGP.IP|BGP.C2S|BGP.RSD|BGP.RP|BGP.RAW},
                {cfgC, BGP.IP|BGP.RSD|BGP.RP|BGP.UDH|BGP.ORF|BGP.RAW},
                {cfgD, BGP.RSD|BGP.RP|BGP.UDH|BGP.ORF|BGP.RAW},
                {cfgE, BGP.IP|BGP.C2S|BGP.RSD|BGP.RP|BGP.RAW},
                {cfgF, BGP.IP|BGP.C2S|BGP.RSD|BGP.RP|BGP.UDH|BGP.RAW}
            };
        private static readonly Dictionary<Configuration, BGP> UserBasinControls =
            new Dictionary<Configuration, BGP>
            {
                {cfgA, BGP.IP|BGP.C2S},
                {cfgB, BGP.IP|BGP.C2S|BGP.RSD|BGP.RP|BGP.RAW},
                {cfgC, BGP.IP|BGP.RSD|BGP.RP|BGP.UDH|BGP.ORF|BGP.RAW},
                {cfgD, BGP.RSD|BGP.RP|BGP.UDH|BGP.ORF|BGP.RAW},
                {cfgE, BGP.IP|BGP.C2S|BGP.RSD|BGP.RP|BGP.RAW},
                {cfgF, BGP.IP|BGP.C2S|BGP.RSD|BGP.RP|BGP.UDH|BGP.RAW}
            };
        #endregion ControlArray Determinents

        #region Fields/Properties        
        [XmlIgnore] public BGP SerializeProperties;

        [XmlIgnore] public decimal InfiltrationPct => (InfiltrationPercentage ?? 0m) / 100m;
        public bool ShouldSerializexmlInfiltrationPercentage()
        { return SerializeProperties.HasFlag(BGP.IP) && infPcnt.HasValue; }
        // --------------------------------------------------------------
        [XmlIgnore] protected decimal? infPcnt;
        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute(AttributeName = "infiltrationPercent", DataType = "decimal")]
        public decimal xmlInfiltrationPercentage
        {
            get => infPcnt.Value;
            set
            {
                if (value.Equals(infPcnt)) return;
                infPcnt = value;
                NotifyPropertyChanged(nameof(InfiltrationPercentage));
            }
        }
        /// <summary>
        /// This is the percentage of the bottom area that allows infiltration to the subsurface soil.
        /// This value should be 100%, [1.0], unless there is an obstruction (e.g., a partial liner)
        /// preventing infiltration from a portion of the bottom area. (N/A for Config D)
        /// </summary>
        [XmlIgnore]
        public decimal? InfiltrationPercentage
        {
            get => infPcnt;
            set => SetField(ref infPcnt, value);
        }


        [XmlIgnore]private decimal? rba;
        [XmlAttribute(AttributeName = "rockBottomArea", DataType = "decimal")]
        public decimal xmlRockStorageBottomArea
        {
            get => rba.Value;
            set=> SetField(ref rba, value);
        }
        /// <summary>
        /// This is the area of the rock gallery, in square feet.
        /// It is an optional entry and can be up to 1/3 the area of the bottom of the surface storage.
        /// If this entry is filled in, it overwrites the PAC-calculated rock storage area.
        /// If this entry is left blank, the PAC calculates a partial rock storage area based
        /// upon the dimensions of the bottom of the surface storage.
        /// </summary>
        [XmlIgnore]
        public decimal? RockBottomArea
        {
            get => rba;
            set => SetField(ref rba, value, nameof(RockBottomArea));
        }
        public bool ShouldSerializexmlRockStorageBottomArea()
        { return SerializeProperties.HasFlag(BGP.RAW) && rba.HasValue; }
        // -------------------------------------------------------
        
        [XmlIgnore] private decimal? rW;
        [XmlAttribute(AttributeName = "rockWidth", DataType = "decimal")]
        public decimal xmlRockWidth
        {
            get => rW.Value;
            set => SetField(ref rW, value, nameof(RockWidth));
        }
        /// <summary>
        /// This is the width of the rock gallery, in feet, as measured perpendicular to the longitudinal slope.
        /// It is only required (and only displayed) if the user is overwriting the PAC-calculated
        /// rock storage area with their own entry (i.e., if the “Rock Area Overwrite” is not blank)
        /// [Also referred to as Rock Width Overwrite]
        /// </summary>
        [XmlIgnore]public decimal? RockWidth
        {
            get => rW;
            set => SetField(ref rW, value, nameof(RockWidth));
        }
        public bool ShouldSerializexmlRockWidth()
        { return SerializeProperties.HasFlag(BGP.RAW) && rW.HasValue; }
        // -------------------------------------------------------

        [XmlIgnore] private decimal? rsd;
        [XmlAttribute(AttributeName = "rockStorageDepth", DataType = "decimal")]
        public decimal xmlRockStorageDepth
        {
            get => rsd.Value;
            set => SetField(ref rsd, value, nameof(RockStorageDepth));
        }
        /// <summary>
        /// This is the thickness of the rock gallery, in inches.
        /// A typical rock storage depth is 12 inches. 
        /// </summary>
        [XmlIgnore]public decimal? RockStorageDepth
        {
            get => rsd;
            set => SetField(ref rsd, value, nameof(RockStorageDepth));
        }
        public bool ShouldSerializexmlRockStorageDepth()
        { return SerializeProperties.HasFlag(BGP.RSD) && rsd.HasValue; }
        // -------------------------------------------------------

        [XmlIgnore] private decimal? por;
        [XmlAttribute(AttributeName = "porosity", DataType = "decimal")]
        public decimal xmlRockPorosity
        {
            get => por.Value;
            set => SetField(ref por, value, nameof(RockPorosity));
        }
        /// <summary>
        /// The percentage of the total volume occupied by the rock that can be filled with Stormwater.
        /// </summary>
        [XmlIgnore]public decimal? RockPorosity
        {
            get => por;
            set => SetField(ref por, value);
        }
        public bool ShouldSerializexmlRockPorosity()
        { return SerializeProperties.HasFlag(BGP.RP) && por.HasValue; }

        [XmlIgnore] private decimal? udHgt;
        [XmlAttribute(AttributeName = "underdrainHeight", DataType = "decimal")]
        public decimal xmlUnderdrainHeight
        {
            get => udHgt.Value;
            set => SetField(ref udHgt, value, nameof(UnderdrainHeight));
        }
        /// <summary>
        /// This is the vertical distance, in inches, from the bottom of the facility
        /// (i.e., where the facility meets the subsurface) to the invert elevation of the underdrain.
        /// </summary>
        [XmlIgnore]public decimal? UnderdrainHeight
        {
            get => udHgt;
            set => SetField(ref udHgt, value);
        }
        public bool ShouldSerializexmlUnderdrainHeight()
        { return SerializeProperties.HasFlag(BGP.UDH) && udHgt.HasValue; }
        // ----------------------------------------------------------------

        [XmlIgnore] private bool? ctch2Sml;
        [XmlAttribute(AttributeName = "catchment2Small", DataType = "boolean")]
        public bool xmlCatchmentTooSmall
        {
            get => ctch2Sml != null && ctch2Sml.Value;
            set => SetField(ref ctch2Sml, value, nameof(CatchmentTooSmall));
        }

        /// <summary>
        /// 
        /// </summary>
        [XmlIgnore]
        public bool? CatchmentTooSmall
        {
            get => ctch2Sml;
            set => SetField(ref ctch2Sml, value);
        }
        public bool ShouldSerializexmlCatchmentTooSmall()
        { return SerializeProperties.HasFlag(BGP.C2S) && ctch2Sml.HasValue; }


        private Orifice orf;
        public Orifice Orifice
        {
            get => orf;
            set
            {
                if (value == null && orf == null) return;
                if(value != null && value.Equals(orf)) return;
                orf = value;
                NotifyPropertyChanged(nameof(Orifice), 
                    false);
            }
        }
        #endregion Fields/Properties        

        #region factory/ctors
        public static BelowGradeProperties Empty => new BelowGradeProperties();
        public static BelowGradeProperties Default => new BelowGradeProperties
        {
            SerializeProperties = BGP.All,
            RockBottomArea = 200m,
            RockPorosity = 0.3m,
            RockStorageDepth = 18m,
            RockWidth = 1m,
            UnderdrainHeight = 4m,
            InfiltrationPercentage = 100m,
            Orifice = Orifice.Default
        };

        public static BelowGradeProperties Make(BelowGradeProperties bgP,
            FacilityCategory cat, Configuration cfg)
        {
            return new BelowGradeProperties
            {
                SerializeProperties = GetSerializableProperties(cat, cfg),
                InfiltrationPercentage = bgP.InfiltrationPercentage,
                RockBottomArea = bgP.RockBottomArea,
                RockStorageDepth = bgP.RockStorageDepth,
                RockWidth = bgP.RockWidth,
                RockPorosity = bgP.RockPorosity,
                UnderdrainHeight = bgP.UnderdrainHeight,
                Orifice = Configuration.NeedsOrf.HasFlag(cfg)?
                          bgP.Orifice ?? Orifice.Default: null
            };
        }

        public static BelowGradeProperties Make(decimal infPcnt,
            decimal? rockBottomArea,
            decimal? rockStorageDepth, decimal? rockWidth,
            decimal? porosity, decimal? strgDepBelowPipe,
            Orifice orifice, BGP serzProp = BGP.All)
        {
            return new BelowGradeProperties
            {
                SerializeProperties = serzProp,
                InfiltrationPercentage = infPcnt,
                RockBottomArea = rockBottomArea,
                RockStorageDepth = rockStorageDepth,
                RockWidth = rockWidth,
                RockPorosity = porosity, 
                UnderdrainHeight = strgDepBelowPipe, 
                Orifice = orifice?? Orifice.Default
            };
        }
        #endregion factory/ctors

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void SetField<T>(ref T field, T value,
            [CallerMemberName]string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            NotifyPropertyChanged(propertyName);
        }
        [NotifyPropertyChangedInvocator]
        protected virtual void NotifyPropertyChanged(
            [CallerMemberName] string propertyName = null,
            bool includeXmlProperty = true)
        {
            PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(propertyName));
            if (includeXmlProperty) PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs($"xml{propertyName}"));
        }

        public void SetIsBinding(bool isBndng)
        {
            IsBinding = isBndng;
            Orifice.SetIsBinding(isBndng);
        }
        #endregion INotifyPropertyChanged implementation

        #region IXmlSerializable
        public XmlSchema GetSchema() { return null; }

        public void ReadXml(XmlReader reader)
        {
            throw new NotImplementedException();
        }

        public void WriteXml(XmlWriter writer)
        {
            throw new NotImplementedException();
        }
        #endregion IXmlSerializable

        public void SetSerializableProperties(FacilityCategory cat, Configuration cfg)
        {  SerializeProperties = GetSerializableProperties(cat, cfg);  }

        public static BGP GetSerializableProperties(FacilityCategory cat, Configuration cfg)
        {
            return cfg == Configuration.Null ? BGP.All :
                cat == slpdFclty ? SlopedControls[cfg] :
                cat == fltPntr ? FlatPlanterControls[cfg] :
                cat == rectBasin ? RectBasinControls[cfg] :
                cat == amoebaBasin ? AmoebaBasinControls[cfg] :
                cat == usrBasin ? UserBasinControls[cfg] : BGP.All;
        }
    }
}