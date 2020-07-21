using System;
using System.Linq;
using System.Xml.Serialization;
using CoP.Enterprise;
using lib = CoP.Enterprise.Utilities;
using Newtonsoft.Json;

namespace BES.SWMM.PAC
{
    [Serializable]
    public class Segment
    {
        #region Dirty Binding & state Properties
        [XmlIgnore]
        public bool IsBinding { get; set; }
        [XmlIgnore]
        public bool IsDirty { get; set; }
        public void SetIsBinding(bool isBndng) { IsBinding = isBndng; }
        #endregion Dirty Binding & state Properties

        #region Segment Properties
        [XmlAttribute(AttributeName = "index", DataType = "int")]
        public int Index { get; set; }

        [XmlAttribute(AttributeName = "length", DataType = "decimal")]
        public decimal Length { get; set; }

        /// <summary>
        /// Width [Thickness] of checkdam separating Segments
        /// </summary>
        [XmlAttribute(AttributeName = "checkDamWidth", DataType = "decimal")]
        public decimal CheckDamWidth { get; set; }

        [XmlAttribute(AttributeName = "longitudinalSlope", DataType = "decimal")]
        public decimal LongitudinalSlope { get; set; }

        [XmlAttribute(AttributeName = "bottomWidth", DataType = "decimal")]
        public decimal BottomWidth { get; set; }
        [XmlIgnore] public decimal Area => BottomWidth * Length;
        [XmlIgnore] public decimal WettableArea => 
                BottomWidth * lib.Minimum(DownStreamDepthFt / LongitudinalSlope, 
                                            Length - CheckDamWidth / 2m);
        [XmlAttribute(AttributeName = "leftSlope", DataType = "decimal")]
        public decimal LeftSlope { get; set; }

        [XmlAttribute(AttributeName = "rightSlope", DataType = "decimal")]
        public decimal RightSlope { get; set; }
        /// <summary>
        /// Sum if both slopes for use in constraint validation
        /// </summary>
        [XmlIgnore] public decimal SumOfSlopes => LeftSlope + RightSlope;

        [XmlAttribute(AttributeName = "downstreamDepth", DataType = "decimal")]
        public decimal DownstreamDepth { get; set; }
        [XmlIgnore] public decimal DownStreamDepthFt => DownstreamDepth / 12m;
        [XmlAttribute(AttributeName = "landscapeWidth", DataType = "decimal")]
        public decimal LandscapeWidth { get; set; }

        #endregion Segment Properties

        #region factory/ctors

        public static Segment Make(
            decimal len, decimal chkDamLen,
            decimal longSlope, decimal bottomWidth,
            decimal leftSlope, decimal rightSlope,
            decimal dwnstrmDepth, decimal landWidth)
        { return Make(0, len, chkDamLen, longSlope,
                bottomWidth, leftSlope, rightSlope,
                dwnstrmDepth, landWidth);}

        public static Segment Make(int index,
            decimal len, decimal chkDamLen, 
            decimal longSlope, decimal bottomWidth, 
            decimal leftSlope, decimal rightSlope, 
            decimal dwnstrmDepth, decimal landWidth)
        {
            return new Segment
            {
                Index = index,
                Length = len,
                CheckDamWidth = chkDamLen,
                LongitudinalSlope = longSlope,
                BottomWidth = bottomWidth,
                LeftSlope = leftSlope,
                RightSlope = rightSlope,
                DownstreamDepth = dwnstrmDepth,
                LandscapeWidth = landWidth
            };
        }

        #endregion factory/ctors

        public override string ToString() 
        { 
            return $"Segment {Index}, Length: {Length} ft, Slope: {LongitudinalSlope}";
        }
    }

    [Serializable]
    public class Segments: SortableBindingList<Segment>
    {
        #region Dirty Binding & state Properties
        [XmlIgnore]
        public bool IsBinding { get; set; }
        [XmlIgnore]
        public bool IsDirty { get { return this.All(s => !s.IsDirty); } }
        public void SetIsBinding(bool isBndng)
        {
            IsBinding = isBndng;
            foreach (var s in this) s.SetIsBinding(isBndng);
        }
        #endregion Dirty Binding & state Properties

        public Segment LastSegment => Items.OrderBy(s => s.Index).Last();
        public decimal BottomArea => this.Sum(s => s.Area);
        public decimal WettableBottomArea => this.Sum(s => s.WettableArea);

        #region factory/ctors
        public void AddSegment(Segment seg)
        {
            seg.Index = Count+1;
            Add(seg);
        }
        public void AddSegment(decimal len,
            decimal chkDamLen, decimal longSlope,
            decimal bottomWidth, decimal leftSlope,
            decimal rightSlope, decimal dwnstrmDepth, 
            decimal landWidth)
        { AddSegment(Segment.Make(Count + 1, len,
                chkDamLen, longSlope,
                bottomWidth, leftSlope, rightSlope,
                dwnstrmDepth, landWidth));}

        public static Segments Empty => new Segments();
        #endregion factory/ctors

        public void AddRange(Segments segs) { foreach (var seg in segs) AddSegment(seg); }
    }
}