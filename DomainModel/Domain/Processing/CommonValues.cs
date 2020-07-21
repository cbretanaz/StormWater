using System;

namespace BES.SWMM.PAC
{
    public class CommonValues: Constants
    {
        #region properties
        /// <summary>
        /// Eq A: ORcA: Cross-sectional Area of Orifice pipe
        /// </summary>
        public decimal? OrificeArea { get; set; }
        /// <summary>
        /// Eq B:  PcL: UnderDrainLength - Length of Underdrain Pipe
        /// </summary>
        public decimal? UnderDrainLength { get; set; }
        /// <summary> 
        /// Eq C: ScDx: MaxSurfaceAreaDepth Greatest potential depth of surface storage
        /// </summary>
        public decimal? MaxSurfaceAreaDepth { get; set; }
        /// <summary>
        /// Eq D: ScDxFIRST: OverflowEDepthFt Depth of Surface Storage at first (lowest) overflow pipe
        ///  Can also be described as height of first overflow pipe above reference
        /// </summary>
        public decimal? OverflowEDepthFt { get; set; }
        /// <summary>
        ///   ScAx: BottomArea (sq feet) 
        /// </summary>
        public decimal BottomArea { get; set; }
        /// <summary>
        /// Eq E: McDx: Blended Soil Depth (feet) 
        /// </summary>
        public decimal BSDFt { get; set; }
        /// <summary>
        /// Eq F: McDxAR: BlendedSoil Storage Depth Above Rock in Feet 
        /// </summary>
        public decimal BSDARFt { get; set; }
        /// <summary>
        /// Eq G: RcDx: Rock Storage Depth (feet)
        /// </summary>
        public decimal? MaxRockStorageDepthFt { get; set; }
        public decimal MaxRSDFt => MaxRockStorageDepthFt ?? 0m;

        /// <summary>
        /// Eq H: RcDxBP: UnderdrainHeightFt
        /// </summary>
        public decimal? UnderdrainHeightFt { get; set; }
        /// <summary>
        /// Eq I: RcDxAP: MaxRSDAbovePipeFt
        /// </summary>
        public decimal? MaxRSDAbovePipeFt { get; set; }
        /// <summary>
        /// Eq J: McVxNR: MediaCapacityNext2Rock
        /// </summary>
        public decimal? MediaCapacityNext2Rock { get; set; }
        /// <summary>
        /// Eq K: McVxAR: MaxMediaVolumeAboveRock
        /// </summary>
        public decimal? MaxMediaVolumeAboveRock { get; set; }
        /// <summary>
        /// Eq L: McVxBPnr: MediaCapacityBPnr
        /// </summary>
        public decimal? MediaCapacityBPnr { get; set; }
        /// <summary>
        /// Eq M: McVxNRap: MaxMediaVolumeNRap
        /// </summary>
        public decimal? MaxMediaVolumeNRap { get; set; }
        /// <summary>
        /// Eq N: MiVxIN: MaxMediaInflowFromStorageVolume
        /// </summary>
        public decimal MaxMediaInflowFromStorageVolume { get; set; }
        /// <summary>
        /// Eq O: MsctN: NumberLayers [FLOATING POINT FRACTION]
        /// </summary>
        public decimal NumberLayers { get; set; }
        /// <summary>
        /// Eq P: MsctNru: LayerCount
        /// </summary>
        public int LayerCount => (int)Math.Ceiling(NumberLayers);
        /// <summary>
        /// Eq Q: MsctNrd: FullLayerCount
        /// </summary>
        public int FullLayerCount => (int)Math.Floor(NumberLayers);
        /// <summary>
        /// Eq R: MsctNruPER: BottomLayerSize
        /// </summary>
        public decimal BottomLayerSize => 
            NumberLayers == FullLayerCount? 1m:
             NumberLayers - FullLayerCount;

        /// <summary>
        /// Eq S/T: McVxSCT/McVxSCTf: MaxMediaVolumeLastLevel [Calculated Property]
        /// </summary>
        public decimal? LayerStorageVolume => MaxMediaVolumeAboveRock / NumberLayers;
        /// <summary>
        /// Eq U/V: McDxSCT/McDxSCTf: Maximum Media Storage depth for Layer
        /// </summary>
        public decimal? LayerStorageDepthFt => BSDARFt / NumberLayers;
        /// <summary>
        /// Eq W: RcWx: RockStorageWidth
        /// </summary>
        public decimal? RockStorageWidth { get; set; }
        /// <summary>
        /// Eq X: RcAx: RockStorageArea
        /// </summary>
        public decimal? RockStorageArea { get; set; }
        /// <summary>
        /// Eq Y RcVx: RockCapacity
        /// </summary>
        public decimal? RockCapacity { get; set; }
        /// <summary>
        /// Eq Z: RcVxBP: RockCapacityBelowPipe
        /// </summary>
        public decimal? RockCapacityBelowPipe { get; set; }
        /// <summary>
        /// Eq AA: RcVxAP: RockCapacityAbovePipe
        /// </summary>
        public decimal? RockCapacityAbovePipe { get; set; }
        /// <summary>
        /// Eq BB: RiVxIN: RockCapacityAbovePipe
        /// </summary>
        public decimal? MaxIncrementalVolumeInputIntoRock { get; set; }
        /// <summary>
        /// Eq CC: RaM: RockMediaInterfaceArea
        /// </summary>
        public decimal RAM{ get; set; }
        /// <summary>
        /// Eq DD: GcAx: BottomInfiltrationArea
        /// </summary>
        public decimal? BottomInfiltrationArea { get; set; }
        /// <summary>
        /// Eq EE: GiVx: MaxDesignInfiltrationVolPerTimeStep
        /// </summary>
        public decimal? MaxDesignInfiltrationVolPerTimeStep { get; set; }
        /// <summary>
        /// Eq FF: ScAx100PER: SurfaceArea100
        /// </summary>
        public decimal SurfaceArea100 { get; set; }
        /// <summary>
        /// Eq GG: ScAx75PER: SurfaceArea75
        /// </summary>
        public decimal SurfaceArea75 { get; set; }
        /// <summary>
        /// Eq HH: ScAx75PERfirst: OverflowESurfArea75
        /// </summary>
        public decimal? OverflowESurfArea75 { get; set; }
        /// <summary>
        /// Eq II: ScVx: MaxSurfaceVolume
        /// </summary>
        public decimal? MaxSurfaceVolume { get; set; }
        /// <summary>
        /// Eq JJ: ScVxFIRST: OverflowESurfaceVolume
        /// </summary>
        public decimal? OverflowESurfaceVolume { get; set; }
        #endregion properties

        #region ctor/Factories
        public static CommonValues Empty => new CommonValues();
        #endregion ctor/Factories
    }
}