using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using BES.SWMM.PAC.Annotations;
using Newtonsoft.Json;


namespace BES.SWMM.PAC
{
    [Serializable]
    public class AboveGradeProperties : Constants, INotifyPropertyChanged //, IXmlSerializable
    {
        #region Dirty Binding & state Properties

        [XmlIgnore] public bool IsBinding { get; set; }
        [XmlIgnore] public bool IsDirty { get; set; }

        #endregion Dirty Binding & state Properties

        #region ControlArray Determinents
        private static readonly Dictionary<Configuration, AGP> SlopedControls =
            new Dictionary<Configuration, AGP>
            {
                {cfgA, AGP.None}, {cfgB, AGP.None}, {cfgC, AGP.None},
                {cfgD, AGP.None}, {cfgE, AGP.OFEH}, {cfgF, AGP.None}
            };

        private static readonly Dictionary<Configuration, AGP> FlatPlanterControls =
            new Dictionary<Configuration, AGP>
            {
                {cfgA, AGP.BA | AGP.ABW | AGP.OFH},
                {cfgB, AGP.BA | AGP.ABW | AGP.OFH},
                {cfgC, AGP.BA | AGP.ABW | AGP.OFH},
                {cfgD, AGP.BA | AGP.ABW | AGP.OFH},
                {cfgE, AGP.BA | AGP.ABW | AGP.OFH | AGP.OFEH},
                {cfgF, AGP.BA | AGP.ABW | AGP.OFH}
            };

        private static readonly Dictionary<Configuration, AGP> RectBasinControls =
            new Dictionary<Configuration, AGP>
            {
                {cfgA, AGP.BA | AGP.ABW | AGP.SS | AGP.OFH | AGP.FD},
                {cfgB, AGP.BA | AGP.ABW | AGP.SS | AGP.OFH | AGP.FD},
                {cfgC, AGP.BA | AGP.ABW | AGP.SS | AGP.OFH | AGP.FD},
                {cfgD, AGP.BA | AGP.ABW | AGP.SS | AGP.OFH | AGP.FD},
                {cfgE, AGP.BA | AGP.ABW | AGP.SS | AGP.OFH | AGP.OFEH | AGP.FD},
                {cfgF, AGP.BA | AGP.ABW | AGP.SS | AGP.OFH | AGP.FD}
            };

        private static readonly Dictionary<Configuration, AGP> AmoebaBasinControls =
            new Dictionary<Configuration, AGP>
            {
                {cfgA, AGP.BA | AGP.BP | AGP.SS | AGP.OFH | AGP.FD},
                {cfgB, AGP.BA | AGP.BP | AGP.SS | AGP.OFH | AGP.FD},
                {cfgC, AGP.BA | AGP.BP | AGP.SS | AGP.OFH | AGP.FD},
                {cfgD, AGP.BA | AGP.BP | AGP.SS | AGP.OFH | AGP.FD},
                {cfgE, AGP.BA | AGP.BP | AGP.SS | AGP.OFH | AGP.OFEH | AGP.FD},
                {cfgF, AGP.BA | AGP.BP | AGP.SS | AGP.OFH | AGP.FD}
            };

        private static readonly Dictionary<Configuration, AGP> UserBasinControls =
            new Dictionary<Configuration, AGP>
            {
                {cfgA, AGP.BA | AGP.OFA | AGP.OFH},
                {cfgB, AGP.BA | AGP.OFA | AGP.OFH},
                {cfgC, AGP.BA | AGP.OFA | AGP.OFH},
                {cfgD, AGP.BA | AGP.OFA | AGP.OFH},
                {cfgE, AGP.BA | AGP.OFEA | AGP.OFA | AGP.OFH | AGP.OFEH},
                {cfgF, AGP.BA | AGP.OFA | AGP.OFH}
            };

        #endregion ControlArray Determinents

        #region Fields/Properties  

        [XmlIgnore] public AGP SerializeProperties;

        [XmlIgnore] private decimal? bA;

        [XmlAttribute(AttributeName = "bottomArea", DataType = "decimal")]
        public decimal xmlBottomArea
        {
            get => bA.Value;
            set => SetField(ref bA, value, nameof(BottomArea));
        }

        /// <summary>
        /// This is the area of the bottom of the surface storage
        /// (i.e., the top of the blended soil),
        /// in square feet, not counting any side-slopes.
        /// </summary>
        [XmlIgnore]
        public decimal? BottomArea
        {
            get => bA;
            set => SetField(ref bA, value);
        }

        public bool ShouldSerializexmlBottomArea()
        {
            return SerializeProperties.HasFlag(AGP.BA) && bA.HasValue;
        }
        // ---------------------------------------------------------------

        [XmlIgnore] private decimal? bW;

        [XmlAttribute(AttributeName = "bottomWidth", DataType = "decimal")]
        public decimal xmlBottomWidth
        {
            get => bW.Value;
            set => SetField(ref bW, value, nameof(BottomWidth));
        }

        /// <summary>
        /// This is the average width of the bottom of the surface storage, in feet, not 
        /// counting any side-slopes, as measured perpendicular to the longitudinal slope.
        /// </summary>
        [XmlIgnore]
        public decimal? BottomWidth
        {
            get => bW;
            set => SetField(ref bW, value);
        }

        public bool ShouldSerializexmlBottomWidth()
        {
            return SerializeProperties.HasFlag(AGP.ABW) && bW.HasValue;
        }
        // ---------------------------------------------------------------------

        [XmlIgnore] private decimal? bP;

        [XmlAttribute(AttributeName = "bottomPerimeter")]
        public decimal xmlBottomPerimeter
        {
            get => bP.Value;
            set => SetField(ref bP, value, nameof(BottomPerimeter));
        }

        /// <summary>
        /// This is the length, in feet, of the edge 
        /// along the flat bottom area, excluding the side slopes.
        /// </summary>
        [XmlIgnore]
        public decimal? BottomPerimeter
        {
            get => bP;
            set => SetField(ref bP, value);
        }

        public bool ShouldSerializexmlBottomPerimeter()
        {
            return SerializeProperties.HasFlag(AGP.BP) && bP.HasValue;
        }
        // ---------------------------------------------------------------------

        [XmlIgnore] private decimal? sS;

        [XmlAttribute(AttributeName = "sideSlope", DataType = "decimal")]
        public decimal xmlSideSlope
        {
            get => sS.Value;
            set => SetField(ref sS, value, nameof(SideSlope));
        }

        /// <summary>
        /// This is the horizontal distance, in feet, that the side-slope travels
        /// with each foot of vertical rise (i.e., the “x” in an x:1 slope).
        /// </summary>
        [XmlIgnore]
        public decimal? SideSlope
        {
            get => sS;
            set => SetField(ref sS, value);
        }

        public bool ShouldSerializexmlSideSlope()
        {
            return SerializeProperties.HasFlag(AGP.SS) && sS.HasValue;
        }
        // ---------------------------------------------------------------------

        [XmlIgnore] private decimal? oFHgt;

        [XmlAttribute(AttributeName = "overflowHeight", DataType = "decimal")]
        public decimal xmlOverflowHeight // 
        {
            get => oFHgt.Value;
            set => SetField(ref oFHgt, value, nameof(OverflowHeight));
        }

        /// <summary>
        /// This is the vertical distance, in inches,
        /// from the top of the blended soil to the top of the overflow pipe.  
        /// </summary>
        [XmlIgnore]
        public decimal? OverflowHeight 
        {
            get => oFHgt;
            set => SetField(ref oFHgt, value);
        }

        [XmlIgnore]
        public decimal OverflowHgtFt => OverflowHeight.GetValueOrDefault(0m) / 12m;
        public bool ShouldSerializexmlOverflowHeight()
        { return SerializeProperties.HasFlag(AGP.OFH) && oFHgt.HasValue; }
        // ---------------------------------------------------------------------

        [XmlIgnore] private decimal? ovrFSA;

        [XmlAttribute(AttributeName = "overflowSurfaceArea", DataType = "decimal")]
        public decimal xmlOverflowSurfaceArea
        {
            get => ovrFSA.Value;
            set => SetField(ref ovrFSA, value, nameof(OverflowSurfaceArea));
        }

        /// <summary>
        /// This is the area, in square feet, of the water’s surface
        /// when it is at the elevation of the highest (or only) overflow. 
        /// </summary>
        [XmlIgnore]
        public decimal? OverflowSurfaceArea
        {
            get => ovrFSA;
            set => SetField(ref ovrFSA, value);
        }

        public bool ShouldSerializexmlOverflowSurfaceArea()
        {
            return SerializeProperties.HasFlag(AGP.OFA) && ovrFSA.HasValue;
        }
        // ---------------------------------------------------------------------

        [XmlIgnore] private decimal? oFEH;
        [XmlAttribute(AttributeName = "overflowEHeight", DataType = "decimal")]
        public decimal xmlOverflowEHeight
        {
            get => oFEH.Value;
            set => SetField(ref oFEH, value, nameof(OverflowEHeight));
        }
        [XmlIgnore]
        public decimal OverflowEHgtFt => OverflowEHeight.GetValueOrDefault(0m) / 12m;

        /// <summary>
        /// This is the vertical distance, in inches, from the top of the 
        /// blended soil to the top of the separate piped overflow to the rock.
        /// </summary>
        [XmlIgnore]
        public decimal? OverflowEHeight
        {
            get => oFEH;
            set => SetField(ref oFEH, value);
        }

        public bool ShouldSerializexmlOverflowEHeight()
        {
            return SerializeProperties.HasFlag(AGP.OFEH) && oFEH.HasValue;
        }
        // ---------------------------------------------------------------------

        [XmlIgnore] private decimal? oFESA;
        [XmlAttribute(AttributeName = "overflowESurfaceArea", DataType = "decimal")]
        public decimal xmlOverflowESurfaceArea
        {
            get => oFESA.Value;
            set => SetField(ref oFESA, value, nameof(OverflowESurfaceArea));
        }
        /// <summary>
        /// This is the area, in square feet, of the water’s surface when the water
        /// is at the elevation of the top of the separated piped overflow to the rock. 
        /// </summary>
        [XmlIgnore]
        public decimal? OverflowESurfaceArea
        {
            get => oFESA;
            set => SetField(ref oFESA, value);
        }

        public bool ShouldSerializexmlOverflowESurfaceArea()
        {
            return SerializeProperties.HasFlag(AGP.OFEA) && oFESA.HasValue;
        }
        // ---------------------------------------------------------------------

        [XmlIgnore] private decimal? fD;
        [XmlAttribute(AttributeName = "freeboard", DataType = "decimal")]
        public decimal xmlFreeboardDepth
        {
            get => fD.Value;
            set => SetField(ref fD, value, nameof(FreeboardDepth));
        }

        [XmlIgnore]
        public decimal? FreeboardDepth
        {
            get => fD;
            set => SetField(ref fD, value);
        }

        public bool ShouldSerializexmlFreeboardDepth()
        { return SerializeProperties.HasFlag(AGP.FD) && fD.HasValue; }

        // ---------------------------------------------------------------------

        #endregion Fields/Properties        

        #region factory/ctors

        public static AboveGradeProperties Empty => new AboveGradeProperties();

        public static AboveGradeProperties Default => new AboveGradeProperties
        {
            SerializeProperties = AGP.All,
            BottomArea = 1000m,
            BottomPerimeter = 500m,
            BottomWidth = 10m,
            FreeboardDepth = 18m,
            SideSlope = 0.2m,
            OverflowHeight = 12m,
            OverflowSurfaceArea = 120.0m,
            OverflowEHeight = 9m,
            OverflowESurfaceArea = 100.0m
        };

        public static AboveGradeProperties Make(
            decimal? bottomArea,
            decimal? bottomWidth, decimal? bottomPerimeter,
            decimal? sideSlope, decimal? freeBoardDepth,
            decimal? ovrflwHeight, decimal? ovrflwSurfArea,
            decimal? ovrfloEHeight, decimal? ovrflwESurfArea,
            AGP serzProp = AGP.All)
        {
            return new AboveGradeProperties
            {
                SerializeProperties = serzProp,
                BottomArea = bottomArea,
                BottomWidth = bottomWidth,
                bP = bottomPerimeter,
                sS = sideSlope,
                fD = freeBoardDepth,
                OverflowHeight = ovrflwHeight,
                OverflowSurfaceArea = ovrflwSurfArea,
                OverflowEHeight = ovrfloEHeight,
                OverflowESurfaceArea = ovrflwESurfArea
            };
        }

        public static AboveGradeProperties Make(AboveGradeProperties agP, 
            FacilityCategory cat, Configuration cfg)
        {
            return new AboveGradeProperties
            {
                SerializeProperties = GetSerializableProperties(cat, cfg),
                BottomArea = agP.BottomArea,
                BottomWidth = agP.BottomWidth,
                bP = agP.BottomPerimeter,
                sS = agP.SideSlope,
                fD = agP.FreeboardDepth,
                OverflowHeight = agP.OverflowHeight,
                OverflowSurfaceArea = agP.OverflowSurfaceArea,
                OverflowEHeight = agP.OverflowEHeight,
                OverflowESurfaceArea = agP.OverflowESurfaceArea
            };
        }
        #endregion factory/ctors

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void SetField<T>(ref T field, T value,
            [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            NotifyPropertyChanged(propertyName);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void NotifyPropertyChanged(
            [CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(propertyName));
            PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs($"xml{propertyName}"));
        }

        public void SetIsBinding(bool isBndng)
        {
            IsBinding = isBndng;
        }

        #endregion INotifyPropertyChanged implementation

        public void SetSerializableProperties(FacilityCategory cat, Configuration cfg)
        {   SerializeProperties = GetSerializableProperties(cat, cfg);}

        public static AGP GetSerializableProperties(FacilityCategory cat, Configuration cfg)
        {
            return  cfg == Configuration.Null ? AGP.All :
                cat == slpdFclty ? SlopedControls[cfg] :
                cat == fltPntr ? FlatPlanterControls[cfg] :
                cat == rectBasin ? RectBasinControls[cfg] :
                cat == amoebaBasin ? AmoebaBasinControls[cfg] :
                cat == usrBasin ? UserBasinControls[cfg] : AGP.All;
        }
    }
}