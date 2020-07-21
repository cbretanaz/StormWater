using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using CoP.Enterprise;

namespace BES.SWMM.PAC
{
    [Serializable]
    [XmlInclude(typeof(ExpectedResult))]
    public class Result
    {
        #region Result Properties
        [XmlAttribute(AttributeName = "status")]
        public PRStatus Status { get; set; }
        #endregion Result Properties

        #region Result Properties
        public static Result Empty => new Result();
        #endregion Result Properties
    }

    [Serializable]public class ExpectedResult: Result
    {
        #region ExpectedResult Properties
        public string Name { get; set; }
        [XmlArrayItem("StormResult")]
        public StormResults StormResults { get; set; }
        #endregion ExpectedResult Properties

        #region factory/ctors
        public static new ExpectedResult Empty => new ExpectedResult();
        public static ExpectedResult Make(string nam) =>
            new ExpectedResult { Name = nam ?? "Expected Test Result Data Here" };
        #endregion factory/ctors
    }

    [Serializable]
    public class StormResult
    {
        #region fields
        private decimal? totOvf;
        private decimal? pkRnOf;
        private decimal? pkDpth;
        #endregion fields

        #region Properties
        [XmlAttribute(AttributeName = "designStorm")]
        public DesignStorm DesignStorm { get; set; }
        
        [XmlAttribute(AttributeName = "status")]
        public PRStatus Status { get; set; }

        [XmlArrayItem("Timestep")]
        public Timesteps Timesteps { get; set; } // this is OVERiV [t] & QiV [t]

        /// <summary>
        /// RUNpkV:  Maximum(RUNiV)
        /// Maximum Pre-Development Runoff from design storm during any single timestep
        /// </summary>
        [XmlAttribute(AttributeName = "peakPreDevRunoff", DataType = "decimal")]
        public decimal PeakPreDevRunoff { get; set; }

        /// <summary>
        /// Maximum inflow from design storm during any single timestep
        /// </summary>
        [XmlAttribute(AttributeName = "peakInflow", DataType = "decimal")]
        public decimal PeakInflow { get; set; }

        [XmlIgnore] public decimal TotalCombinedOverflow => TotalOverflow + TotalOverflowE;
        /// <summary>
        /// QpkV:     Max(QiV)
        /// Maximum Outflow (RunOff Volume) during single timestep of storm
        /// </summary>
        [XmlAttribute(AttributeName = "peakOutflow", DataType = "decimal")]
        public decimal PeakOutflow { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute(AttributeName = "peakUnderdrain", DataType = "decimal")]
        public decimal PeakUnderdrain { get; set; }

        /// <summary>
        /// Maximum Overflow (through Overflow Pipe) during entire storm 
        /// </summary>
        [XmlAttribute(AttributeName = "peakSurfaceOverflow", DataType = "decimal")]
        public decimal PeakSurfaceOverFlow { get; set; }

        /// <summary>
        /// Maximum Overflow (through Overflow Pipe) during entire storm 
        /// </summary>
        [XmlAttribute(AttributeName = "peakOverflowE", DataType = "decimal")]
        public decimal PeakOverFlowE { get; set; }

        /// <summary>
        /// Maximum total Overflow (sum of SurfaceOverflow an Overflow E) during entire storm 
        /// </summary>
        [XmlAttribute(AttributeName = "peakTotalOverflow", DataType = "decimal")]
        public decimal PeakTotalOverflow { get; set; }

        /// <summary>
        /// OVERtV   SUM(OVERiV)
        /// Sum of Overflows (through Overflow Pipe) from entire storm (144 Timesteps)
        /// </summary>
        [XmlAttribute(AttributeName = "totalOverflow", DataType = "decimal")]
        public decimal TotalOverflow { get; set; }

        /// <summary>
        /// OVERtV   SUM(OVERiVfirst)
        /// Sum of Overflows (through second Overflow Pipe for Config E only) from entire storm (144 Timesteps)
        /// </summary>
        [XmlAttribute(AttributeName = "totalOverflowE", DataType = "decimal")]
        public decimal TotalOverflowE { get; set; }


        /// <summary>
        /// SpkD:   Maximum(ScD)
        /// Maximum Depth of storm water occurring on Surface during storm
        /// </summary>
        [XmlAttribute(AttributeName = "peakSurfaceHead", DataType = "decimal")]
        public decimal PeakSurfaceHead { get; set; }

        /// <summary>
        /// H:   Maximum(H)
        /// Maximum Depth of total Head
        /// </summary>
        [XmlAttribute(AttributeName = "peakHead", DataType = "decimal")]
        public decimal PeakHead { get; set; }

        /// <summary>
        /// PiV:   Sum(PiV) [Underdrain OutFlow]
        /// </summary>
        [XmlAttribute(AttributeName = "totalUnderdrainOutFlow", DataType = "decimal")]
        public decimal TotalUnderdrainOutflow { get; set; }

        /// <summary>
        /// GiV:   Sum(GiV) [Soil Outflow]
        /// </summary>
        [XmlAttribute(AttributeName = "totalSoilOutFlow", DataType = "decimal")]
        public decimal TotalSoilOutflow { get; set; }

        /// <summary>
        /// SiV:   Sum(SiV) [StormInflow]
        /// </summary>
        [XmlAttribute(AttributeName = "totalInFlow", DataType = "decimal")]
        public decimal TotalInflow { get; set; }


        [XmlArrayItem("Failure")]
        public Failures Failures { get; set; }
        #endregion Properties

        #region ctor/Factories
        public static StormResult Empty => new StormResult 
            { Timesteps = Timesteps.Empty, Failures = Failures.Make(DesignStorm.Null) };

        public static StormResult Make(
            DesignStorm dsgnStrm, PRStatus stat)
        {
            return new StormResult
            {
                DesignStorm = dsgnStrm, 
                Status = stat,
                Timesteps = Timesteps.Empty,
                Failures = Failures.Make(dsgnStrm)
            };
        }
        #endregion ctor/Factories
        public override string ToString()
        { return $"{DesignStorm} StormResult {Status}"; }
    }

    [Serializable]
    public class StormResults : SortableBindingList<StormResult>
    {
        public PRStatus Status { get; set;}
        public StormResult this[DesignStorm ds]
        { get { return this.FirstOrDefault(r => r.DesignStorm == ds); } }

        #region factory/ctors
        public static StormResults Empty => new StormResults {Status = PRStatus.Pending};

        public static StormResults EmptyPending
        {
            get
            {
                var srs = new StormResults {Status = PRStatus.Pending};
                srs.Add(StormResult.Make(DesignStorm.WQ, PRStatus.Pending));
                srs.Add(StormResult.Make(DesignStorm.HalfTwoYear, PRStatus.Pending));
                srs.Add(StormResult.Make(DesignStorm.TwoYear, PRStatus.Pending));
                srs.Add(StormResult.Make(DesignStorm.FivYear, PRStatus.Pending));
                srs.Add(StormResult.Make(DesignStorm.TenYear, PRStatus.Pending));
                srs.Add(StormResult.Make(DesignStorm.TwntyFiv, PRStatus.Pending));
                return srs;
            }
        }
        #endregion factory/ctors

        public void AddRange(IEnumerable<StormResult> rslts)
        { foreach(var sr in rslts) Add(sr); }
    }
}
