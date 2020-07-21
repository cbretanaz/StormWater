using System;
using System.Collections.Generic;

namespace BES.SWMM.PAC
{
    public class MediaLayer
    {
        #region properties 
        /// <summary>
        /// Index        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// MiVinSCT: Incremental Media Volume entering into this Media Level 
        /// </summary>
        public decimal Inflow { get; set; }
        /// <summary>
        /// MiVinSCT: Incremental Media Volume entering into the Media Layer above this Level
        /// </summary>
        public decimal PriorTSInflow { get; set; }

        /// <summary>
        /// MiVbackSCT: Incremental Media Volume entering into Media Layer from Backup
        /// </summary>
        public decimal Backup { get; set; }

        /// <summary>
        /// MiVoutFinSCT: Incremental Media Volume from Layer Above Surface exiting Media Layer
        /// </summary>
        public decimal OutFlowWoBackup { get; set; }

        /// <summary>
        /// MiVoutFbackSCT: Incremental Media Volume from Backup exiting Media Layer
        /// </summary>
        public decimal BackUpOutflow { get; set; }

        /// <summary>
        /// McVavlFinSCT: Cumulative from Backup Media Volume Available
        /// </summary>
        public decimal CumBkUpVolumeAvail { get; set; }

        /// <summary>
        /// MiVbackSCT: Incremental Volume Entering Media Layer from backup
        /// </summary>
        public decimal InflowFromBackup { get; set; }

        /// <summary>
        /// MiVSCT: Incremental Media Volume
        /// </summary>
        public decimal IncVolume { get; set; }

        /// <summary>
        /// MiDsct: Incremental Media Depth (ft)
        /// </summary>
        public decimal IncremDepthFt { get; set; }

        /// <summary>
        /// MiVSCTfull: Is this layer fully saturated?
        /// </summary>
        public bool IsFull { get; set; }
        /// <summary>
        /// unknown: Are all layers below this one fully saturated?
        /// </summary>
        public bool IsFullBelow { get; set; }

        /// <summary>
        /// StormWater that would be in Rock Soil if it had unlimited Capacity
        ///  [RcV + McVnr + RiOver + MiVnrOVER]
        /// </summary>
        public decimal RockSoilStormWater { get; set; }
        #endregion properties

        #region ctor/factories
        /// <summary>
        /// Creates Empty Media Section object
        /// </summary>
        public static readonly MediaLayer Empty = new MediaLayer();

        /// <summary>
        /// Creates a media Section object.
        /// MediaLayer object represents a vertical section of growth Medium
        /// that rainfall can traverse in one 10 minute time step. 
        /// Properties include inflow and outflow volumes for that timestep.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="inflow">MiVinSCT: Incremental Media Volume entering into a section of Media</param>
        /// <param name="backflow">MiVbackSCT: Incremental Media Volume entering into a section of Media from Backup</param>
        /// <param name="surfOutflow">MiVoutFinSCT: Incremental Media Volume from Surface exiting section of Media</param>
        /// <param name="backupOutflow">MiVoutFbackSCT: Incremental Media Volume from Backup exiting a section of Media</param>
        /// <returns>new Media Section object</returns>
        public static MediaLayer Make(
            int index, decimal inflow,
            decimal backflow, decimal surfOutflow,
            decimal backupOutflow)
        {
            return new MediaLayer
            {
                Index = index,
                Inflow = inflow,
                Backup = backflow,
                OutFlowWoBackup = surfOutflow,
                BackUpOutflow = backupOutflow
            };
        }
        public static MediaLayer Make(int index) { return new MediaLayer {Index = index};}
        #endregion ctor/factories

        public override string ToString() { return $"Media Layer {Index} "; }
    }

    public class MediaLayers: SortedList<int, MediaLayer>
    {
        #region properties
        /// <summary>
        /// Number of Timesteps for stormwater to traverse Blended Soil Media
        /// </summary>
        public decimal TimeSteps { get; set; }
        /// <summary>
        /// Whether Stormwater will take more than one Timestep to traverse Blended Soil Media;
        /// (Whether there are is more than one Layer in Blended Soil Media)
        /// </summary>
        public bool OnlyOneLayer => TimeSteps <= 1M;
        /// <summary>
        /// Whether Stormwater will require only one or less than one Timestep to traverse Blended Soil Media;
        /// (Whether there is only one Layer in Blended Soil Media)
        /// </summary>
        public bool HasMultipleLayers => TimeSteps > 1M;
        /// <summary>
        /// Fractional measure of Timesteps for stormwater to traverse last Layer in Blended Soil Media
        /// </summary>
        public decimal lastLayerTS => TimeSteps - FullLayerCount;
        /// <summary>
        /// Count of number of Layers in Blended Soil Media  
        /// </summary>
        public int LayerCount => (int)Math.Ceiling(TimeSteps);
        /// <summary>
        /// Count of number of Full (600 sec) Layers in Blended Soil Media
        /// </summary>
        public int FullLayerCount => (int)Math.Floor(TimeSteps);
        /// <summary>
        /// Refernce to the last Layer in Blended Soil Media
        /// </summary>
        public MediaLayer BottomLayer => LayerCount > 0? this[LayerCount-1]: null;
        #endregion properties

        #region ctor/factories
        public static MediaLayers Empty => new MediaLayers();
        public static MediaLayers Make(decimal numLevels)
        {
            var ms = Empty;
            ms.TimeSteps = numLevels;
            for (var i = 0; i<numLevels; i++)
                ms.Add(i, MediaLayer.Make(i));
            return ms;
        }
        #endregion ctor/factories
    }
}