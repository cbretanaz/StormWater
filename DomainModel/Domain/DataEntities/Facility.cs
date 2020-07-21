using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using BES.SWMM.PAC.Annotations;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace BES.SWMM.PAC
{
    [Serializable]
    [XmlInclude(typeof(SlopedFacility))]
    [XmlInclude(typeof(FlatPlanter))]
    [XmlInclude(typeof(RectangularBasin))]
    [XmlInclude(typeof(AmoebaBasin))]
    [XmlInclude(typeof(UserDefinedBasin))]
    public abstract class Facility : Constants, INotifyPropertyChanged
    {
        #region Dirty Binding & state Properties
        [XmlIgnore]
        public bool IsBinding { get; set; }
        [XmlIgnore]
        public bool IsDirty { get; set; }
        [XmlIgnore]
        public bool IsAnyDirty => IsDirty || AboveGrade.IsDirty || BelowGrade.IsDirty ||
                    this is SlopedFacility && ((SlopedFacility) this).Segments.IsDirty;
        public void SetIsBinding(bool isBndng)
        {
            IsBinding = isBndng;
            AboveGrade?.SetIsBinding(isBndng);
            BelowGrade?.SetIsBinding(isBndng);
        }
        #endregion Dirty Binding & state Properties

        #region fields
        protected bool pub;
        protected Configuration cfg;
        #endregion fields

        #region Properties
        #region Category
        /// <summary>
        /// This one way the City classifies a facility;
        /// it is based upon the general slopes of the facility.
        /// If the longitudinal slope of the bottom of the facility
        /// is 4% or greater, the facility is a “sloped facility”.
        /// </summary>
        [XmlAttribute(AttributeName = "category", DataType = "string")]
        public string xmlCategory
        {
            get
            {
                switch (Category)
                {
                    case slpdFclty: return "Sloped";
                    case fltPntr: return "FlatPlanter";
                    case rectBasin: return "RectBasin";
                    case amoebaBasin: return "Amoeba";
                    case usrBasin: return "UserDefBasin";
                    default: return null;
                }
            }
        }
        [XmlIgnore] public FacilityCategory Category =>
            this is SlopedFacility ? slpdFclty :
            this is FlatPlanter ? fltPntr :
            this is RectangularBasin ? rectBasin :
            this is AmoebaBasin ? amoebaBasin :
            this is UserDefinedBasin ? usrBasin : FacilityCategory.Null;

        /// <summary>
        /// If the longitudinal slope of the bottom of the facility is 4% or greater,
        /// It is classified as a SlopedFacility
        /// </summary>
        [XmlIgnore] public bool IsSlopedFacility => Category == slpdFclty;
        /// <summary>
        /// If the longitudinal slope of the bottom of the facility is less than 4%,
        /// and ...
        /// It is classified as a Flat Planter, 
        /// </summary>
        [XmlIgnore] public bool IsFlatPlanter => Category == fltPntr;
        [XmlIgnore] public bool IsRectBasin => Category == rectBasin;
        [XmlIgnore] public bool IsAmoebaBasin => Category == amoebaBasin;
        [XmlIgnore] public bool IsUserDefined => Category == usrBasin;
        [XmlIgnore] public bool IsBasin => FacilityCategory.Basin.HasFlag(Category);
        public bool ShouldSerializexmlCategory() { return FacilityCategory.Serialize.HasFlag(Category); }
        #endregion Category

        #region Configuration
        /// <summary> 
        /// This is the combination of main selectable components
        /// that create the facility structure, including
        /// a Rock Gallery, underdrain, liner, and whether the
        /// Rock Overflow from is connected to the Outflows.
        /// </summary>
        [XmlAttribute(AttributeName = "config", DataType = "string")]
        public string xmlConfig
        {
            get
            {
                switch (Configuration)
                {
                    case cfgA: return "A";
                    case cfgB: return "B";
                    case cfgC: return "C";
                    case cfgD: return "D";
                    case cfgE: return "E";
                    case cfgF: return "F";
                    default: return null;
                }
            }
            set
            {
                switch (value)
                {
                    case "A":
                        Configuration = cfgA;
                        break;
                    case "B":
                        Configuration = cfgB;
                        break;
                    case "C":
                        Configuration = cfgC;
                        break;
                    case "D":
                        Configuration = cfgD;
                        break;
                    case "E":
                        Configuration = cfgE;
                        break;
                    case "F":
                        Configuration = cfgF;
                        break;
                    default:
                        Configuration = Configuration.Null;
                        break;
                }
                NotifyPropertyChanged(nameof(Configuration));
            }
        }
        public bool ShouldSerializexmlConfig() { return Configuration.Serialize.HasFlag(Configuration); }
        [XmlIgnore] public Configuration Configuration
        {
            get => cfg;
            set => SetField(ref cfg, value);
        }
        [XmlIgnore] public bool IsCnfgA => Configuration == cfgA;
        [XmlIgnore] public bool IsCnfgB => Configuration == cfgB;
        [XmlIgnore] public bool IsCnfgC => Configuration == cfgC;
        [XmlIgnore] public bool IsCnfgD => Configuration == cfgD;
        [XmlIgnore] public bool IsCnfgE => Configuration == cfgE;
        [XmlIgnore] public bool IsCnfgF => Configuration == cfgF;
        [XmlIgnore] public bool NeedsOrifice => Configuration.NeedsOrf.HasFlag(Configuration);
        #endregion Configuration

        #region Shape
        /// <summary>
        /// This is the shape of the facility from an aerial view.
        /// The facility shape is only required for basins.
        /// </summary>
        protected Shape shp;
        [XmlAttribute(AttributeName = "shape")]
        public string xmlShape
        {
            get
            {
                switch (shp)
                {
                    case Shape.Amoeba: return "Amoeba";
                    case Shape.Rectangular: return "Rect";
                    case Shape.UserDefined: return "User";
                    default: return null;
                }
            }
            set
            {
                switch (value)
                {
                    case "Amoeba":
                        shp = Shape.Amoeba;
                        break;
                    case "Rect":
                        shp = Shape.Rectangular;
                        break;
                    case "User":
                        shp = Shape.UserDefined;
                        break;
                    default:
                        shp = Shape.Null;
                        break;
                }
                NotifyPropertyChanged(nameof(Shape));
            }
        }
        [XmlIgnore] public Shape Shape
        {
            get => shp;
            set => SetField(ref shp, value);
        }

        [XmlIgnore]
        public bool isRectShape
        {
            get => shp == Shape.Rectangular;
            set { if (value) shp = Shape.Rectangular; }
        }
        [XmlIgnore] public bool isAmoebaShape
        {
            get => shp == Shape.Amoeba;
            set { if (value) shp = Shape.Amoeba; }
        }
        [XmlIgnore] public bool isUserDefShape
        {
            get => shp == Shape.UserDefined;
            set { if (value) shp = Shape.UserDefined; }
        }
        public bool ShouldSerializexmlShape() { return Shape.Serialize.HasFlag(shp); }
        #endregion Shape

        #region Above & Below Grade Properties
        [XmlIgnore] protected AboveGradeProperties agp;
        [XmlElement("AboveGradeData")]
        public AboveGradeProperties AboveGrade
        {
            get => agp;
            set
            {
                if (value.Equals(agp)) return;
                agp = value;
            }
        }
        // -------------------------------------------------
        [XmlIgnore] protected BelowGradeProperties bgp;
        [XmlElement("BelowGradeData")]
        public BelowGradeProperties BelowGrade
        {
            get => bgp;
            set
            {
                if (value.Equals(bgp) ) return;
                bgp = value;
            }
        }
        #endregion Above & Below Grade Properties

        //[XmlAttribute(AttributeName = "isSloped", DataType = "boolean")]
        //public bool ShouldSerializeIsSloped() { return true; }
        [XmlIgnore] public bool IsSloped => this is SlopedFacility;

        /// <summary>
        /// This is based upon the location of the facility,
        /// which can either be in the public right-of-way or on a parcel.
        /// Facilities in the public right-of-way have different
        /// design constraints than either public or private facilities on parcels.
        /// </summary>
        [XmlAttribute(AttributeName = "public", DataType = "boolean")]
        public bool IsPublic
        {
            get => pub;
            set => SetField(ref pub, value);
        }
        public bool ShouldSerializeIsPublic() { return true; }

        [XmlIgnore] protected decimal? bSD;
        [XmlAttribute(AttributeName = "blendedSoilDepth")]
        public decimal xmlBlendedSoilDepth
        {
            get => bSD.Value;
            set => SetField(ref bSD, value, nameof(BlendedSoilDepth));
        }
        [XmlIgnore] public decimal? BlendedSoilDepth
        {
            get => bSD;
            set => SetField(ref bSD, value);
        }
        public bool ShouldSerializexmlBlendedSoilDepth() { return bSD.HasValue; }
        #endregion Properties

        #region factory/ctors
        public static Facility Make(FacilityCategory fCat)
        {
            switch (fCat)
            {
                case slpdFclty:
                    return SlopedFacility.Empty;
                case fltPntr:
                    return FlatPlanter.Empty;
                case FacilityCategory.AmoebaBasin:
                    return AmoebaBasin.Empty;
                case rectBasin:
                    return RectangularBasin.Empty;
                case usrBasin:
                    return UserDefinedBasin.Empty;
                default: throw new WrongFacilityException("Invalid FacilityCategory");
            }
        }
        #endregion factory/ctors

        #region functions
        public abstract void UpdateSerializeProperties(Configuration cfg);
        #endregion functions

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
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(propertyName));
        }
        #endregion INotifyPropertyChanged implementation
    }

        #region Sloped Facility
        [Serializable]
    public class SlopedFacility : Facility, INotifyPropertyChanged
    {
        #region Properties 
        [XmlIgnore] public Segment FinalSegment => Segments.Last();
        // -------------------------------------------------
        [XmlIgnore]public Segments segs;
        [XmlArrayItem("Segment")] 
        public Segments Segments
        {
            get => segs;
            set
            {
                if (value.Equals(segs)) return;
                segs = value;
                NotifyPropertyChanged(nameof(Segments));
            }
        }
        #endregion Properties

        #region factory/ctors
        private SlopedFacility() { Shape = Shape.Null; }
        [XmlIgnore] public static SlopedFacility Empty => new SlopedFacility
        {
            BlendedSoilDepth = 12m,
            Shape = Shape.Null,
            AboveGrade = AboveGradeProperties.Default,
            BelowGrade = BelowGradeProperties.Default,
            Segments = Segments.Empty
        };
        public static SlopedFacility Make(Facility fclty, Configuration config)
        {
            if (fclty is SlopedFacility slpd &&
                fclty.Configuration == config) 
                return slpd;
            // --------------------------------
            return new SlopedFacility
            {
                BlendedSoilDepth = fclty.BlendedSoilDepth,
                Configuration = config,
                Shape = Shape.Null,
                IsPublic = fclty.IsPublic,
                AboveGrade = AboveGradeProperties.Make(
                    fclty.AboveGrade, slpdFclty, config),
                BelowGrade = BelowGradeProperties.Make
                    (fclty.BelowGrade, slpdFclty, config),
                Segments = fclty is SlopedFacility slp? 
                    slp.Segments: Segments.Empty
            };
        }
        public static SlopedFacility Make(bool isPublic, 
            Configuration config, decimal blndSoilDepth, 
            AboveGradeProperties above, BelowGradeProperties below, 
            Segments segments)
        {
            return new SlopedFacility
            {
                BlendedSoilDepth = blndSoilDepth,
                Shape = Shape.Rectangular,
                Configuration = config,
                IsPublic = isPublic,
                AboveGrade = above?? AboveGradeProperties.Default,
                BelowGrade = below?? BelowGradeProperties.Default,
                Segments = segments?? Segments.Empty
            };
        }
        #endregion factory/ctors

        #region Virtual functions
        public override void UpdateSerializeProperties(Configuration config)
        {
            AboveGrade.SetSerializableProperties(slpdFclty, config);
            BelowGrade.SetSerializableProperties(slpdFclty, config);
        }
        #endregion Virtual functions

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(propertyName));
        }
        #endregion INotifyPropertyChanged implementation
    }
    #endregion Sloped acilities
    // ---------------------------

    #region Flat Facilities
    [Serializable]
    public class FlatPlanter: Facility, INotifyPropertyChanged
    {
        #region Fields/Properties   
        #endregion Fields/Properties        

        #region factory/ctors
        private FlatPlanter() {Shape = Shape.Null; }
        public static FlatPlanter Empty => new FlatPlanter
        {
            BlendedSoilDepth = 12m,
            Shape = Shape.Null,
            AboveGrade = AboveGradeProperties.Default,
            BelowGrade = BelowGradeProperties.Default
        };
        public static FlatPlanter Make(Facility currFlcty, Configuration config)
        {
            if (currFlcty is FlatPlanter flt &&
                currFlcty.Configuration == config)
                return flt;
            // -----------------------------
            return new FlatPlanter
            {
                BlendedSoilDepth = currFlcty.BlendedSoilDepth,
                Configuration = config,
                Shape = Shape.Null,
                IsPublic = currFlcty.IsPublic,
                AboveGrade = AboveGradeProperties.Make(
                    currFlcty.AboveGrade, fltPntr, config),
                BelowGrade = BelowGradeProperties.Make
                    (currFlcty.BelowGrade, fltPntr, config)
            };
        }
        public static FlatPlanter Make(bool isPublic,  
            decimal blndSoilDepth, Configuration config,
            AboveGradeProperties above, BelowGradeProperties below)
        { return new FlatPlanter
            {
                BlendedSoilDepth = blndSoilDepth, Shape = Shape.Null,
                IsPublic = isPublic, Configuration = config,
                AboveGrade = above?? AboveGradeProperties.Default, 
                BelowGrade = below?? BelowGradeProperties.Default}; }
        #endregion factory/ctors

        #region Virtual functions
        public override void UpdateSerializeProperties(Configuration config)
        {
            AboveGrade.SetSerializableProperties(fltPntr, config);
            BelowGrade.SetSerializableProperties(fltPntr, config);
        }
        #endregion Virtual functions

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(propertyName));
        }
        #endregion INotifyPropertyChanged implementation
    }
    // ---------------------------

    [Serializable]
    public class RectangularBasin : Facility, INotifyPropertyChanged
    {
        #region Fields/Properties   
        #endregion Fields/Properties        

        #region factory/ctors
        private RectangularBasin() {}
        public static RectangularBasin Empty => new RectangularBasin
        {
            Shape = Shape.Rectangular, BlendedSoilDepth = 12m, 
            AboveGrade = AboveGradeProperties.Default,
            BelowGrade = BelowGradeProperties.Default
        };
        public static RectangularBasin Make(Facility currFlcty, Configuration config)
        {
            if (currFlcty is RectangularBasin rect &&
                currFlcty.Configuration == config)
                return rect;
            // --------------------------------
            return new RectangularBasin
            {
                Shape = Shape.Rectangular,
                BlendedSoilDepth = currFlcty.BlendedSoilDepth,
                Configuration = config,
                IsPublic = currFlcty.IsPublic,
                AboveGrade = AboveGradeProperties.Make(
                    currFlcty.AboveGrade, rectBasin, config),
                BelowGrade = BelowGradeProperties.Make
                    (currFlcty.BelowGrade, rectBasin, config)
            };
        }
        public static RectangularBasin Make(bool isPublic, 
            decimal blndSoilDepth, Configuration config,
            AboveGradeProperties above, BelowGradeProperties below)
        {
            return new RectangularBasin
            {
                Shape = Shape.Rectangular,
                IsPublic = isPublic,
                BlendedSoilDepth = blndSoilDepth,
                Configuration = config,
                AboveGrade = above ?? AboveGradeProperties.Default,
                BelowGrade = below ?? BelowGradeProperties.Default
            };
        }
        #endregion factory/ctors

        #region Virtual functions
        public override void UpdateSerializeProperties(Configuration config)
        {
            //AboveGrade = AboveGradeProperties.Make(rectBasin, config);
            //BelowGrade = BelowGradeProperties.Make(rectBasin, config);
            AboveGrade.SetSerializableProperties(rectBasin, config);
            BelowGrade.SetSerializableProperties(rectBasin, config);
        }
        #endregion Virtual functions

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(propertyName));
        }
        #endregion INotifyPropertyChanged implementation
    }
    // ---------------------------

    [Serializable]
    public class AmoebaBasin : Facility, INotifyPropertyChanged
    {
        #region Fields/Properties   
        #endregion Fields/Properties        

        #region factory/ctors
        private AmoebaBasin() { }
        public static AmoebaBasin Empty => new AmoebaBasin
        {
            Shape = Shape.Amoeba, BlendedSoilDepth = 12m,
            AboveGrade = AboveGradeProperties.Default,
            BelowGrade = BelowGradeProperties.Default
        };
        public static AmoebaBasin Make(Facility currFlcty, Configuration config)
        {
            if (currFlcty is AmoebaBasin amb &&
                currFlcty.Configuration == config)
                return amb;
            // --------------------------------
            return new AmoebaBasin
            {
                Shape = Shape.Amoeba,
                BlendedSoilDepth = currFlcty.BlendedSoilDepth,
                Configuration = config,
                IsPublic = currFlcty.IsPublic,
                AboveGrade = AboveGradeProperties.Make(
                    currFlcty.AboveGrade, amoebaBasin, config),
                BelowGrade = BelowGradeProperties.Make(
                    currFlcty.BelowGrade, amoebaBasin, config)
            };
        }
        public static AmoebaBasin Make(bool isPublic, 
            Configuration config, decimal blndSoilDepth,
            AboveGradeProperties above, BelowGradeProperties below)
        {
            return new AmoebaBasin
            {
                Shape = Shape.Amoeba,
                IsPublic = isPublic,
                BlendedSoilDepth = blndSoilDepth,
                Configuration = config,
                AboveGrade = above ?? AboveGradeProperties.Default,
                BelowGrade = below ?? BelowGradeProperties.Default
            };
        }
        #endregion factory/ctors

        #region Virtual functions
        public override void UpdateSerializeProperties(Configuration config)
        {
            AboveGrade.SetSerializableProperties(amoebaBasin, config);
            BelowGrade.SetSerializableProperties(amoebaBasin, config);
        }
        #endregion Virtual functions

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(propertyName));
        }
        #endregion INotifyPropertyChanged implementation
    }
    // ---------------------------

    [Serializable]
    public class UserDefinedBasin : Facility, INotifyPropertyChanged
    {
        #region Fields/Properties   
        #endregion Fields/Properties        

        #region factory/ctors
        private UserDefinedBasin() {Shape = Shape.UserDefined; }
        public static UserDefinedBasin Empty => new UserDefinedBasin
        {
            Shape = Shape.UserDefined, BlendedSoilDepth = 12m,
            AboveGrade = AboveGradeProperties.Default,
            BelowGrade = BelowGradeProperties.Default
        };
        public static UserDefinedBasin Make(Facility currFlcty, Configuration config)
        {
            if (currFlcty is UserDefinedBasin usr &&
                currFlcty.Configuration == config)
                return usr;
            // --------------------------------
            return new UserDefinedBasin
            {
                Shape = Shape.UserDefined,
                BlendedSoilDepth = currFlcty.BlendedSoilDepth,
                Configuration = config,
                IsPublic = currFlcty.IsPublic,
                AboveGrade = AboveGradeProperties.Make(
                    currFlcty.AboveGrade, usrBasin, config),
                BelowGrade = BelowGradeProperties.Make(
                    currFlcty.BelowGrade, usrBasin, config)
            };
        }
        public static UserDefinedBasin Make(
            bool isPublic, Configuration config, decimal blndSoilDepth,
            AboveGradeProperties above, BelowGradeProperties below)
        {
            return new UserDefinedBasin
            { 
                Shape = Shape.UserDefined,
                IsPublic = isPublic,
                Configuration = config,
                BlendedSoilDepth = blndSoilDepth,
                AboveGrade = above ?? AboveGradeProperties.Default,
                BelowGrade = below ?? BelowGradeProperties.Default
            };
        }
        #endregion factory/ctors

        #region Virtual functions
        public override void UpdateSerializeProperties(Configuration config)
        {
            AboveGrade.SetSerializableProperties(usrBasin, config);
            BelowGrade.SetSerializableProperties(usrBasin, config);
        }
        #endregion Virtual functions

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(propertyName));
        }
        #endregion INotifyPropertyChanged implementation
    }
    #endregion Flat Facilities
}
