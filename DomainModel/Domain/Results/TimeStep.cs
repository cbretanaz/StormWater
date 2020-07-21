using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using CoP.Enterprise;

namespace BES.SWMM.PAC
{
    [Serializable]
    public class Timestep
    {
        #region properties
        [XmlAttribute(AttributeName = "timeStep", DataType = "int")]
        public int Index { get; set; }
        // ----------------------------------------------------------

        [XmlAttribute(AttributeName = "preDevRunOff", DataType = "decimal")]
        public decimal PreDevRunOff { get; set; }
        // ----------------------------------------------------------

        [XmlAttribute(AttributeName = "inflow", DataType = "decimal")]
        public decimal Inflow { get; set; }
        // ----------------------------------------------------------

        [XmlAttribute(AttributeName = "outflow", DataType = "decimal")]
        public decimal Outflow { get; set; }
        // ----------------------------------------------------------

        [XmlAttribute(AttributeName = "incSoilOutflow", DataType = "decimal")] 
        public decimal IncrementalSoilOutflow { get; set; }
        // ----------------------------------------------------------
        [XmlAttribute(AttributeName = "underdrainOutflow", DataType = "decimal")]
        public decimal UnderdrainOutflow { get; set; }
        // ----------------------------------------------------------

        [XmlAttribute(AttributeName = "surfaceOverflow", DataType = "decimal")]
        public decimal SurfaceOverflow { get; set; }
        // ----------------------------------------------------------

        [XmlAttribute(AttributeName = "overflowE", DataType = "decimal")]
        public decimal OverflowE { get; set; }
        // ----------------------------------------------------------

        [XmlAttribute(AttributeName = "head", DataType = "decimal")]
        public decimal Head { get; set; }
        // ----------------------------------------------------------

        [XmlAttribute(AttributeName = "surfaceHead", DataType = "decimal")]
        public decimal SurfaceHead { get; set; }
        #endregion properties

        #region factory/ctors
        public static Timestep Empty => new Timestep();
        public static Timestep Make(int timeStep, decimal preDevRunoff,
            decimal inflow, decimal outflow, decimal incSoilOutflow, decimal undrDrnOutflow, 
            decimal surfOverflow, decimal overflowE, decimal surfHead, decimal head) => 
            new Timestep{ Index = timeStep, PreDevRunOff = preDevRunoff, 
                Inflow = inflow, Outflow = outflow, 
                UnderdrainOutflow = undrDrnOutflow,
                SurfaceOverflow = surfOverflow, OverflowE= overflowE, 
                SurfaceHead = surfHead, Head = head
            };
        #endregion factory/ctors

        public override string ToString()
        {
            return $"Timestep {Index}: {SurfaceOverflow: 0.0 cubic ft} Surface overflow, " +
                $"{Outflow: 0.0 cubic ft} outflow, {Head: 0.0 cubic ft} Head";
        }
    }

    public class Timesteps: SortableBindingList<Timestep>
    {
        #region factory/ctors
        public static Timesteps Empty => new Timesteps();
        public void AddtoEnd(Timestep ts)
        { Add(Timestep.Make(Count, ts.PreDevRunOff, 
            ts.Inflow, ts.Outflow, ts.IncrementalSoilOutflow,
            ts.UnderdrainOutflow,
            ts.SurfaceOverflow, ts.OverflowE, 
            ts.SurfaceHead, ts.Head)); }
        public void AddTimestep(int timestep, 
            decimal preDevRunoff, decimal inflow,
            decimal outflow, decimal incSoilOutflow, decimal unrDrnOutFlow,
            decimal surfOverflow, decimal overflowE,
            decimal surfHead, decimal head)
        { Add(Timestep.Make(timestep, preDevRunoff, 
            inflow, outflow, incSoilOutflow, unrDrnOutFlow, 
            surfOverflow, overflowE, surfHead, head )); }
        public void AddtoEnd(decimal preDevRunoff, decimal inflow, 
            decimal outflow, decimal incSoilOutflow, decimal undrDranOutflow, 
            decimal surfOverflow, decimal overflowE, 
            decimal surfHead, decimal head)
        { Add(Timestep.Make(Count, preDevRunoff, inflow,
            outflow, incSoilOutflow, undrDranOutflow, surfOverflow,  
            overflowE, surfHead, head)); }
        public void AddTimesteps(IEnumerable<Timestep> tss)
        {
            foreach(var ts in tss)
                Add(Timestep.Make(Count, ts.PreDevRunOff,
                    ts.Inflow, ts.Outflow, ts.IncrementalSoilOutflow, 
                    ts.UnderdrainOutflow,
                    ts.SurfaceOverflow, ts.OverflowE, 
                    ts.SurfaceHead, ts.Head));
        }
        #endregion factory/ctors
    }
}