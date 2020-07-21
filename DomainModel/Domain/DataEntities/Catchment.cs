using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using BES.SWMM.PAC.Annotations;
using CoP.Enterprise;
using System.ComponentModel.DataAnnotations;
using itp = BES.SWMM.PAC.InfiltrationTestProcedure;
using Newtonsoft.Json;

namespace BES.SWMM.PAC
{
    [Serializable, XmlRoot("Catchment")]
    public class Catchment : INotifyPropertyChanged, IDataErrorInfo
    {
        protected const StringComparison icic 
            = StringComparison.InvariantCultureIgnoreCase;
        #region Dirty Binding & state Properties
        [XmlIgnore]
        public bool IsBinding { get; set; }
        [XmlIgnore]
        public bool IsDirty { get; set; }
        [XmlIgnore]
        public bool IsAnyDirty => IsDirty || Facility.IsAnyDirty;
        public void SetIsBinding(bool isBndng)
        {
            IsBinding = isBndng;
            Facility?.SetIsBinding(isBndng);
        }
        #endregion Dirty Binding & state Properties

        #region fields
        private string nam;
        //private decimal area;
        //private decimal corFac;
        private PRStatus stat;
        private itp infProc;
        private decimal infRat;
        private HierarchyLevel lvl;
        private decimal impArea;
        private Facility fclty;
        #endregion fields

        #region Properties
        [Required, StringLength(35)]
        [XmlAttribute(AttributeName = "name", DataType = "string")]
        public string Name
        {
            get => nam;
            set => SetField(ref nam, value);
        }

        [XmlAttribute(AttributeName = "status")]
        [Required]
        public PRStatus Status
        {
            get => stat;
            set => SetField(ref stat, value);
        }
        public bool ShouldSerializeStatus() { return Status != PRStatus.Null; }

        #region TestProcs

        [XmlAttribute(AttributeName = "testProc")]
        public string xmltestProc
        {
            get =>
                infProc == itp.OpenPit ? "OpenPit" :
                infProc == itp.Encased ? "Encased" :
                infProc == itp.DoubleRing ? "DblRing" :
                infProc == itp.NA ? "NA" : "";
            set
            {
                infProc =
                    value.Equals("OpenPit", icic) ? itp.OpenPit :
                    value.Equals("Encased", icic) ? itp.Encased :
                    value.Equals("DblRing", icic) ? itp.DoubleRing : itp.NA;
                NotifyPropertyChanged(nameof(InfiltrationTestProcedure));
            }
        }
        [XmlIgnore]
        [Required]
        public itp InfiltrationTestProcedure
        {
            get => infProc;
            set => SetField(ref infProc, value);
        }
        public bool ShouldSerializeInfiltrationTestProcedure() { return InfiltrationTestProcedure != itp.Null; }

        [XmlIgnore]
        public bool IsOpenPit
        {
            get => InfiltrationTestProcedure == itp.OpenPit;
            set { if (value) InfiltrationTestProcedure = itp.OpenPit; }
        }

        [XmlIgnore]
        public bool IsEncased
        {
            get => InfiltrationTestProcedure == itp.Encased;
            set { if (value) InfiltrationTestProcedure = itp.Encased; }
        }
        [XmlIgnore]
        public bool IsDoubleRing
        {
            get => InfiltrationTestProcedure == itp.DoubleRing;
            set { if (value) InfiltrationTestProcedure = itp.DoubleRing; }
        }
        [XmlIgnore]
        public bool IsNA
        {
            get => InfiltrationTestProcedure == itp.NA;
            set { if (value) InfiltrationTestProcedure = itp.NA; }
        }
        #endregion TestProcs

        //public bool ShouldSerializeCorrectionFactor() { return true; }

        [Required, Range(0, 1.0)]
        [XmlAttribute(AttributeName = "infiltRate", DataType = "decimal")]
        public decimal InfiltrationRate
        {
            get => infRat;
            set => SetField(ref infRat, value);
        }
        public bool ShouldSerializeInfiltrationRate() { return true; }

        #region HierarchyLevels
        /// <summary>
        /// This is the set of requirements the facility must be designed to meet.
        /// It is based upon the facility’s discharge point.
        /// The manual describes how to determine which hierarchy level is
        /// acceptable for a given project.
        /// The City has the authority to require a specific hierarchy level be met.
        /// </summary>
        [Required]
        [XmlAttribute(AttributeName = "level", DataType = "string")]
        public string xmlLevel
        {
            get
            {
                switch (HierarchyLevel)
                {
                    case HierarchyLevel.One: return "L1";
                    case HierarchyLevel.TwoA: return "L2A";
                    case HierarchyLevel.TwoB: return "L2B";
                    case HierarchyLevel.TwoC: return "L2C";
                    case HierarchyLevel.Three: return "L3";
                    default: return null;
                }
            }
            set
            {
                switch (value)
                {
                    case "L1":
                        HierarchyLevel = HierarchyLevel.One;
                        break;
                    case "L2A":
                        HierarchyLevel = HierarchyLevel.TwoA;
                        break;
                    case "L2B":
                        HierarchyLevel = HierarchyLevel.TwoB;
                        break;
                    case "L2C":
                        HierarchyLevel = HierarchyLevel.TwoC;
                        break;
                    case "L3":
                        HierarchyLevel = HierarchyLevel.Three;
                        break;
                    default:
                        HierarchyLevel = HierarchyLevel.Null;
                        break;
                }
                NotifyPropertyChanged(nameof(HierarchyLevel));
            }
        }
        public bool ShouldSerializexmlLevel() { return HierarchyLevel.Serialize.HasFlag(HierarchyLevel); }
        [XmlIgnore]public HierarchyLevel HierarchyLevel
        {
            get => lvl;
            set
            {
                if (value == lvl) return;
                lvl = value;
                NotifyPropertyChanged(nameof(HierarchyLevel));
            }
        }
        /// <summary>
        /// This level should be selected unless site constraints prevent
        /// infiltration or the facility is a water quality-only facility
        /// that discharges to an underground injection control.  
        /// </summary>
        [XmlIgnore] public bool IsLevel1
        {
            get => HierarchyLevel == HierarchyLevel.One;
            set { if(value) HierarchyLevel = HierarchyLevel.One; }
        }
        /// <summary>
        /// This level should be selected for a facility that discharges 1) to a UIC or
        /// 2) to the Columbia Slough, Willamette River, or Columbia River,
        /// either directly or via an adequately sized storm-only system.
        /// </summary>
        [XmlIgnore]public bool IsLevel2a
        {
            get => HierarchyLevel == HierarchyLevel.TwoA;
            set { if (value) HierarchyLevel = HierarchyLevel.TwoA; }
        }

        /// <summary>
        /// This level should be selected for a facility that discharges,
        /// either directly or via a storm-only system, to a body of water
        /// other than the Columbia Slough, Willamette River, or Columbia River.
        /// </summary>
        [XmlIgnore]public bool IsLevel2b
        {
            get => HierarchyLevel == HierarchyLevel.TwoB;
            set { if (value) HierarchyLevel = HierarchyLevel.TwoB; }
        }
        /// <summary>
        /// This level should be selected for facilities that discharge through
        /// a storm-only system that outfalls directly to the Columbia Slough,
        /// Willamette River, or Columbia River if pipe capacity is a concern.
        /// </summary>
        [XmlIgnore]public bool IsLevel2c
        {
            get => HierarchyLevel == HierarchyLevel.TwoC;
            set { if (value) HierarchyLevel = HierarchyLevel.TwoC; }
        }

        /// <summary>
        /// This level should be selected for facilities that discharge to the combined system.
        /// </summary>
        [XmlIgnore]
        public bool IsLevel3
        {
            get => HierarchyLevel == HierarchyLevel.Three;
            set { if (value) HierarchyLevel = HierarchyLevel.Three; }
        }
        //[XmlIgnore]public bool IsLevel3 => HierarchyLevel == HierarchyLevel.Three;
        #endregion HierarchyLevels

        /// <summary>
        /// This is the area of impervious surfaces,
        /// as measured in square feet from a plane surface,
        /// that will drain to the stormwater facility.
        /// </summary>
        [Required, Range(100, 100000)]
        [XmlAttribute(AttributeName = "impervArea", DataType = "decimal")]
        public decimal ImperviousArea
        {
            get => impArea;
            set => SetField(ref impArea, value);
        }
        public bool ShouldSerializeImperviousArea() { return true; }

        [XmlIgnore] private int preCrv;
        /// <summary>
        /// This is a rough estimate of the impervious area’s capacity to retain rainfall.
        /// The PreCurveNumber is the capacity to retain rainfall before it became impervious,
        /// in its natural, undeveloped state (i.e., pre-development).
        /// </summary>
        [Required, Range(1, 99)]
        [XmlAttribute(AttributeName = "preCN", DataType = "int")]
        public int PreCurveNumber
        {
           get => preCrv;
           set => SetField(ref preCrv, value);
       }
        public bool ShouldSerializePreCurveNumber() { return true; }

        [XmlIgnore] private int pstCrv;
        /// <summary>
        /// This is a rough estimate of the impervious area’s capacity to retain rainfall.
        /// The PostCurveNumber is the capacity to retain rainfall after development
        /// (i.e., post-development). 
        /// </summary>
        [Required, Range(1, 99)]
        [XmlAttribute(AttributeName = "postCN", DataType = "int")]
        public int PostCurveNumber
        {
           get => pstCrv;
           set => SetField(ref pstCrv, value);
       }
        public bool ShouldSerializePostCurveNumber() { return true; }

        [XmlIgnore] private int preTc;
        /// <summary>
        /// This is the longest time it is calculated to take, in minutes, for stormwater runoff
        /// to travel through the catchment area, or 5 minutes, whichever is longer. 
        /// PreTOC is the time it is calculated to take in its natural, undeveloped state. 
        /// </summary>
        [Required, Range(5,12)]
        [XmlAttribute(AttributeName = "preTC", DataType = "int")]
        public int PreTOC
        {
            get => preTc;
            set => SetField(ref preTc, value);
        }
        public bool ShouldSerializePreTOC() { return true; }

        [XmlIgnore] private int pstTc;
        /// <summary>
        /// This is the longest time it is calculated to take, in minutes, for stormwater runoff
        /// to travel through the catchment area, or 5 minutes, whichever is longer. 
        /// PostTOC is the time it is calculated to take to reach the facility after development. 
        /// </summary>
        [Required, Range(5, 12)]
        [XmlAttribute(AttributeName = "postTC", DataType = "int")]
        public int PostTOC
        {
            get => pstTc;
            set => SetField(ref pstTc, value);
        }
        public bool ShouldSerializePostTOC() { return true; }

        /// <summary>
        /// Needs Water Quality Test filter:
        /// Set True for Hierarchy 2a, 2b, & 2c;
        /// Set False for Hierarchy 3
        /// Determined by user data input for Hierarchy 1
        /// </summary>
        [XmlIgnore]private bool ndsWQ;
        [XmlAttribute(AttributeName = "needsWaterQuality", DataType = "boolean")]
        public bool NeedsWaterQuality
        {
            get => IsLevel1 && ndsWQ || IsLevel2a || IsLevel2b || IsLevel2c;
            set => SetField(ref ndsWQ, value, nameof(NeedsWaterQuality));
        }
        public bool ShouldSerializeNeedsWaterQuality() { return true; }


        [XmlIgnore] private bool req;
        [XmlAttribute(AttributeName = "meetsRequirements", DataType = "boolean")]
        public bool MeetsRequirements
        {
            get => req;
            set => SetField(ref req, value);
        }
        public bool ShouldSerializeMeetsRequirements() { return true; }

        public Facility Facility
        {
            get => fclty;
            set => SetField(ref fclty, value);
        }
        #endregion Properties

        #region factory/ctors
        public static Catchment Empty => new Catchment
            { Facility = Facility.Make(FacilityCategory.FlatPlanter) };
        public static Catchment Make(string name,
            itp testProc, PRStatus prStat, 
            decimal native, HierarchyLevel level,
            decimal impArea, int preCN, int postCN,
            int preTC, int postTC, Facility facility)
        {
            return new Catchment
            {
                Name = name, Status = prStat,
                InfiltrationTestProcedure = testProc,
                InfiltrationRate = native,
                HierarchyLevel = level, ImperviousArea = impArea,
                PreCurveNumber = preCN, PostCurveNumber = postCN,
                PreTOC = preTC, PostTOC = postTC,
                Facility = facility?? Facility.Make(FacilityCategory.SlopedFacility),
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
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, 
                new PropertyChangedEventArgs(propertyName));
        }
        #endregion INotifyPropertyChanged implementation

        public string this[string property]
        {
            get
            {
                var propertyDescriptor = TypeDescriptor.GetProperties(this)[property];
                if (propertyDescriptor == null) return string.Empty;

                var results = new List<ValidationResult>();
                var result = Validator.TryValidateProperty(
                    propertyDescriptor.GetValue(this),
                    new ValidationContext(this, null, null)
                        { MemberName = property },
                    results);
                return !result ? results.First().ErrorMessage : string.Empty;
            }
        }

        public string Error
        {
            get
            {
                var results = new List<ValidationResult>();
                var result = Validator.TryValidateObject(this,
                    new ValidationContext(this, 
                        null, null),
                    results, true);
                return !result ? string.Join("\n", 
                    results.Select(x => x.ErrorMessage)) : null;
            }
        }
    }

    public class Catchments : SortableBindingList<Catchment>
    {
        #region Dirty Binding & state Properties
        public bool IsBinding { get; set; }
        public bool IsDirty { get { return this.All(s => !s.IsDirty); } }
        public void SetIsBinding(bool isBndng)
        {
            IsBinding = isBndng;
            foreach(var c in this) c.SetIsBinding(isBndng);
        }
        #endregion Dirty Binding & state Properties


        public void AddCatchment(Catchment ctch) { Add(ctch); }

        public void AddCatchment(string name,
            decimal area, PRStatus prStat, 
            InfiltrationTestProcedure testProc, 
            decimal native, HierarchyLevel level, decimal impArea, 
            byte preCN, byte postCN, byte preTC, byte postTC, 
            Facility facility)
        {
            Add(Catchment.Make(name, 
                testProc, prStat, 
                native, level, impArea,
                preCN, postCN, preTC, postTC, 
                facility));
        }
        public void AddCatchment(string name,
            decimal area, PRStatus prStat,
            InfiltrationTestProcedure testProc, 
            decimal native, HierarchyLevel level, decimal impArea,
            byte preCN, byte postCN, byte preTC, byte postTC,
            FacilityCategory facTyp = FacilityCategory.Null)
        {
            var fclty =
                facTyp == FacilityCategory.SlopedFacility ? SlopedFacility.Empty :
                facTyp == FacilityCategory.FlatPlanter ? FlatPlanter.Empty :
                facTyp == FacilityCategory.RectBasin ? RectangularBasin.Empty :
                facTyp == FacilityCategory.AmoebaBasin ? AmoebaBasin.Empty:
                facTyp == FacilityCategory.UserDefinedBasin ? UserDefinedBasin.Empty :
                         (Facility)null;

            Add(Catchment.Make(name, 
                testProc, prStat, 
                native, level, impArea,
                preCN, postCN, preTC, postTC,
                fclty));
        }
        #region factory/ctors
        public static Catchments Empty => new Catchments();
        #endregion factory/ctors

        public void AddRange(Catchments ctchs) { foreach (var ctch in ctchs) Add(ctch); }
    }
}
