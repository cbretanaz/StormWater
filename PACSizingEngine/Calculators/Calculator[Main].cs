
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CoP.Enterprise;
using lib = CoP.Enterprise.Utilities;

namespace BES.SWMM.PAC
{
    public abstract class Calculator : Constants, ICalculateResults
    {
        #region Consts
        #region Configuration statics
        protected static readonly PACConfig pacCfg = PACConfig.Make();
        protected static readonly EngineSettings engCfg = pacCfg.EngineSettings;
        protected static readonly List<decimal> priorHeads = new List<decimal>();
        protected static readonly int MAXRAINFALLTS = engCfg.MaxRainfallTimesteps;
        protected static readonly int MAXTS = engCfg.MaxTimesteps;
        protected static readonly int HEADLOOKBACKTS = 5;
        #endregion Configuration statics

        #region other Global Constants
        protected static readonly decimal MPO = engCfg.mediaPorosity;
        protected static readonly int TS = engCfg.TimeStep;
        protected static readonly decimal MINF = engCfg.InfilitrationRate;
        protected static readonly decimal ORCOEFF = engCfg.OrificeCoefficient;
        protected static readonly string sNL = Environment.NewLine;
        protected double epsilonPcnt = 0.1;
        protected Dictionary<DesignStorm, StormEvent> PreDevStormEvents { get; set; }
        protected StormEvent pdWQ { get; set; }
        protected StormEvent pd2 { get; set; }
        protected StormEvent pd5 { get; set; }
        protected StormEvent pd10 { get; set; }
        protected StormEvent pd25 { get; set; }

        #endregion other Global Constants
        #endregion Consts

        #region private variables
        protected List<StormEvent> StormEvents;
        #endregion private variables

        #region ctor/Factories/Initialization
        public static Calculator Make(FacilityCategory cat)
        {
            switch (cat)
            {
                case slpdFclty: return SlopedFacilityCalculator.Default;
                case fltPntr: return FlatPlanterCalculator.Default;
                case rectBasin: return RectBasinCalculator.Default;
                case amoebaBasin: return AmoebaCalculator.Default;
                case usrBasin: return UserDefinedCalculator.Default;
                default: throw new WrongFacilityException(
                "Invalid Facility Category for calculator calculation engine.");
            }
        }

        protected void InitializeEngine(Catchment ctch)
        {
            InitializeStormEvents(ctch);
            InitializePreDevStormEvents(
                ctch.ImperviousArea, 
                ctch.PreCurveNumber, 
                ctch.PreTOC);
        }

        private void InitializeStormEvents(Catchment ctch)
        {
            StormEvents = new List<StormEvent>
            {
                StormEvent.Make(dswq, ctch),
                StormEvent.Make(dsH2, ctch),
                StormEvent.Make(ds2, ctch),
                StormEvent.Make(ds5, ctch),
                StormEvent.Make(ds10, ctch),
                StormEvent.Make(ds25, ctch),
            };
        }
        private void InitializePreDevStormEvents( decimal impArea, decimal cn, decimal toc)
        {
            PreDevStormEvents = new Dictionary<DesignStorm, StormEvent>
            {
                { dswq, StormEvent.Make(dswq, impArea, cn, toc)},
                { ds2, StormEvent.Make(ds2, impArea, cn, toc)},
                { ds5, StormEvent.Make(ds5, impArea, cn, toc)},
                { ds10, StormEvent.Make(ds10, impArea, cn, toc)},
                { ds25, StormEvent.Make(ds25, impArea, cn, toc)}
            };
             pdWQ = PreDevStormEvents[dswq];
             pd2 = PreDevStormEvents[ds2];
             pd5 = PreDevStormEvents[ds5];
             pd10 = PreDevStormEvents[ds10];
             pd25 = PreDevStormEvents[ds25];
        }
        #endregion ctor/Factories

        #region Common Calculator Methods
        public abstract StormResults CalculateResults(Catchment ctch, bool getTimestepDetails = true);
        public CommonValues CalculateCommonValues(Catchment ctch)
        {
            var cm = CommonValues.Empty;
            var fclty = ctch.Facility;
            var cfg = fclty.Configuration;
            var IsCfgA = cfg == cfgA;
            var IsCfgE = cfg == cfgE;

            var isSloped = fclty is SlopedFacility;
            var segmnts = isSloped ? ((SlopedFacility) fclty).Segments : null;
            // -----------------------------------------
            var orf = fclty.BelowGrade.Orifice;
            var agDta = fclty.AboveGrade;
            if (agDta == null)
                throw new CoPMissingDataException(
                    "Above Grade data elements are necessary for calculation.");
            var bgDta = fclty.BelowGrade;
            if (bgDta == null)
                throw new CoPMissingDataException(
                    "Below Grade data elements are necessary for calculation.");
            cm.BottomArea = isSloped ? segmnts.WettableBottomArea : 
                                        agDta.BottomArea ?? 0m;// ScAx [McAx]
                
            var RBA = bgDta.RockBottomArea ?? 0m;
            var BW = agDta.BottomWidth ?? 0m;

            // Eq A: ORcA. Orifice Area (sq ft)
            cm.OrificeArea = orf?.Diameter == null ?
                (decimal?) null :
                (decimal) (Math.PI * Math.Pow((double) (orf.Diameter.Value / 24m), 2));

            CalculateAreaAndVolume(ctch.Facility, cm);
            var areaDif = cm.SurfaceArea100 - RBA;
            var RSDFt = (bgDta.RockStorageDepth ?? 0m) / 12m;
            // ------------------------------------------------------
            // Eq B: PcL. Underdrain Length (ft)
            cm.UnderDrainLength =
                fclty is SlopedFacility slpdF ?
                    lib.Minimum(slpdF.FinalSegment.Length,
                        slpdF.Segments.Sum(s => s.Length) / 4m):
                fclty is AmoebaBasin || fclty is UserDefinedBasin? 
                    (decimal)Math.Sqrt((double)cm.BottomArea) / 4m :
                    cm.BottomArea / (4m * BW);

            // Eq C: ScDx: Depth of media at overflow { Height of Overflow} in FEET
            cm.MaxSurfaceAreaDepth = agDta.OverflowHgtFt;

            // Eq D: ScDxFIRST: Depth of media at First (E) overflow { Height of Overflow E}
            if (IsCfgE) cm.OverflowEDepthFt = (agDta.OverflowEHeight ?? 0m) / 12m;

            // EQ E: McDx: Blended Soil Depth in Feet 
            cm.BSDFt = (fclty.BlendedSoilDepth ?? 0m) / 12m;

            // EQ F: McDxAR: BlendedSoil Storage Depth Above Rock in Feet
            cm.BSDARFt = IsCfgA ? cm.BSDFt:
                        (bgDta.RockBottomArea ?? 0m) < cm.BottomArea ? 
                            cm.BSDFt - RSDFt : cm.BSDFt;

            // EQ G: RcDx: Rock Storage Depth (feet)
            cm.MaxRockStorageDepthFt = RSDFt;

            // Eq H: RcDxBP: 
            cm.UnderdrainHeightFt = (bgDta.UnderdrainHeight ?? 0m) / 12m;

            // Eq I: RcDxAP: 
            cm.MaxRSDAbovePipeFt = cm.MaxRockStorageDepthFt - cm.UnderdrainHeightFt;

            // Eq J: McVxNR: 
            cm.MediaCapacityNext2Rock = cm.MaxRockStorageDepthFt * areaDif * MPO;

            // Eq K: McVxAR: 
            //cm.MaxMediaVolumeAboveRock = cm.BSDARFt * cm.BottomArea * MPO;
            cm.MaxMediaVolumeAboveRock = cm.BSDARFt * cm.SurfaceArea75 * MPO;

            // Eq L: McVxBPnr
            cm.MediaCapacityBPnr = cm.UnderdrainHeightFt * areaDif * MPO;

            // Eq M: McVxNRap: 
            cm.MaxMediaVolumeNRap = cm.MaxRSDAbovePipeFt * areaDif * MPO; // todo: fix this -

            // Eq N: MiVxIN: 
            cm.MaxMediaInflowFromStorageVolume = cm.SurfaceArea75 * MINF * TS / 43200m; //todo: Only for Config E

            // Eq O: MsctN: NumberLayers
            cm.NumberLayers = 43200m * cm.BSDARFt * MPO / (TS * MINF);

            /* *** Eq P, Q, R are calculated properties: ******************************* 
                Eq P: MsctNru: LayerCount: - Integral Count of levels: Calculated Property
                Eq Q: MsctNrd:  FullLayerCount: Integral Count of full 600 sec levels: Calculated Property
                Eq R: MsctNruPER: BottomLayerSize (as Percentage of a full 600 sec Level) Calculated Property
            *** ************************************************************************************/

            // Eq S/T: McVxSCT/McVxSCTf: MaxMediaVolumePerLevel - Calculated Properties
            //         Maximum Media Storage volume for section[Level],
            //         McVxSCTf: MaxMediaVolumeLastLevel Calculated Property      
            // cm.LayerStorageVolume = MaxMediaVolumeAboveRock / NumberLayers;

            // Eq U/V: McDxSCT/McDxSCTf: Maximum Media Storage depth for Layer, - Calculated Properties
            //      V: McDxSCTf: BottomLayerStorageDepth: Maximum Media Storage depth
            //                   for last Layer (Calculated Property)
            //cm.LayerStorageDepthFt = BSDARFt / NumberLayers;

            // Eq W: RcWx
            cm.RockStorageWidth = fclty.IsCnfgA ? 0m :
                bgDta.RockWidth ??
                (fclty.IsSloped && ((SlopedFacility) fclty).FinalSegment.BottomWidth < 3m ?
                    ((SlopedFacility) fclty).FinalSegment.BottomWidth :
                    !fclty.IsSloped && BW < 3m ? BW : 3m);

            // Eq X: RcAx
            cm.RockStorageArea = fclty.IsCnfgA ? 0m :
                bgDta.RockBottomArea ??
                (fclty.IsSloped && ((SlopedFacility) fclty).FinalSegment.BottomWidth < 3m ?
                    ((SlopedFacility) fclty).FinalSegment.BottomWidth :
                    !fclty.IsSloped && BW < 3m ? BW :3m) * cm.UnderDrainLength;

            #region Eq Y/Z/AA: RcVx/RcVxAP/RcVxBP

            if (!IsCfgA)
            {
                var rcFac = cm.RockStorageArea * bgDta.RockPorosity.Value;
                // Eq Y: RcVx
                cm.RockCapacity = cm.MaxRockStorageDepthFt * rcFac;
                // Eq Z: RcVxBP
                cm.RockCapacityBelowPipe = cm.UnderdrainHeightFt * rcFac;
                // Eq AA: RcVxAP
                cm.RockCapacityAbovePipe = cm.MaxRSDAbovePipeFt * rcFac;
            }
            #endregion Eq Y/Z/AA: RcVx/RcVxAP/RcVxBP

            // Eq AB: RiVxIN
            cm.MaxIncrementalVolumeInputIntoRock =
                cm.MaxMediaInflowFromStorageVolume +
                cm.MediaCapacityNext2Rock + cm.RockCapacity;

            // EQ AC: RaM:  RockMediaInterfaceArea = 

            switch (fclty)
            {
                case SlopedFacility slpd:
                    var bw = slpd.FinalSegment.BottomWidth;
                    cm.RAM = CalculateRaM(
                        slpd.FinalSegment.DownStreamDepthFt,
                        slpd.FinalSegment.Length,
                        bw > 4m ? RAMSide.Both : 
                        bw > 3m? RAMSide.One: RAMSide.Neither);
                    break;

                case AmoebaBasin _:
                case UserDefinedBasin _:
                {
                    var effWid = (decimal) Math.Sqrt((double) cm.BottomArea);
                    cm.RAM = CalculateRaM(
                        cm.BSDFt, effWid / 4m,
                        effWid > 4m ? RAMSide.Both :
                        effWid > 3m ? RAMSide.One : RAMSide.Neither);
                    break;
                }

                default:
                    var btmWid = agDta.BottomWidth;
                    cm.RAM = CalculateRaM(
                        cm.BSDFt, cm.BottomArea / agDta.BottomWidth.Value,
                        btmWid > 4m ? RAMSide.Both :
                        btmWid > 3m ? RAMSide.One : RAMSide.Neither);
                    break;
            }


            // Eq AD: GcAx
            cm.BottomInfiltrationArea = bgDta.InfiltrationPct * cm.SurfaceArea100;

            // Eq AE: GiVx
            cm.MaxDesignInfiltrationVolPerTimeStep =
                ctch.Facility.IsCnfgD ?
                    0m :
                    cm.BottomInfiltrationArea * ctch.InfiltrationRate * TS /
                    (ctch.IsDoubleRing ? 43200 : 86400);
            return cm;

        }
        #endregion Common Calculator Methods
        
        #region abstract/virtual methods
        #region ValidateInput Data Methods
        protected void ValidateInputData(Catchment ctch)
        {
            var fclty = ctch.Facility;
            var IsCfgE = fclty.Configuration == cfgE;
            var agDta = fclty.AboveGrade;
            var bgDta = fclty.BelowGrade;
            ValidateFacilityInputData(fclty);
            ValidateAGInputData(agDta, IsCfgE);
            ValidateBGInputData(bgDta, fclty.Configuration);
            if (fclty is SlopedFacility slpd)
                (this as SlopedFacilityCalculator)?
                    .ValidateSegmentData(slpd.Segments);
        }
        protected void ValidateFacilityInputData(Facility fclty)
        {
            if (!fclty.BlendedSoilDepth.HasValue)
                throw new PACInputDataException(
                    "Blended Soil Depth is required for calculations",
                    "BlendedSoilDepth");
            // --------------------------------------
            if (fclty.Category == FacilityCategory.Null)
                throw new PACInputDataException(
                    "Facility Category must be specified for Calculations.",
                    "FacilityCategory");
            // --------------------------------------
            if (fclty is UserDefinedBasin && fclty.Shape == Shape.Null)
                throw new PACInputDataException(
                    "Facility Shape must be specified for User Defined Basin Calculations.",
                    "Shape");
            // --------------------------------------
            if (fclty.Configuration == Configuration.Null)
                throw new PACInputDataException(
                    "Facility Configuration must be specified for Calculations.",
                    "Configuration");
        }
        protected abstract void ValidateAGInputData(AboveGradeProperties agDta, bool isCfgE = false);
        protected abstract void ValidateBGInputData(BelowGradeProperties bgDta, Configuration cfg);
        #endregion ValidateInput Data Methods

        public StormResults IterateTimeSteps(Catchment ctch, CommonValues cm, bool getTimestepDetails = true)
        {
            var fclty = ctch.Facility;
            var orifice = fclty.BelowGrade.Orifice;

            var cfg = fclty.Configuration;
            var IsCfgA = cfg == cfgA;
            var IsCfgE = cfg == cfgE;
            var IsCfgF = cfg == cfgF;
            var agDta = fclty.AboveGrade;
            var reslts = StormResults.Empty;

            foreach (var strmEvt in StormEvents)
            {
                var dsgnStrm = strmEvt.DesignStorm;
                var pdStrmEvnt = 
                    dsgnStrm == DesignStorm.HalfTwoYear? pd2:  
                        PreDevStormEvents[dsgnStrm];
                var lyrs = MediaLayers.Make(cm.NumberLayers);
                var timStps = new Timesteps();
                reslts.Add(StormResult.Make(dsgnStrm, PRStatus.Pending));
                var reslt = reslts[dsgnStrm];
                reslt.PeakPreDevRunoff = pdStrmEvnt.Max(s=>s.RunOff);
                var prevCumSurfStorage = 0m;         // prior ScV
                var prevOverFlow = 0m;               // OVERiV
                var prevOverFlowE = 0m;              // OVERiVfirst
                var prevHead = 0m;                   //
                var prevRockLayerVolume = 0m;        //
                var prevLevelWithRockVol = 0m;       // RcVandMcVnr
                var prevBackUpFmRock = 0m;           // RiVoverandMiVnrOVER
                var prevCumRockVolAbovePipe = 0m;    // RcVap: Cumulative Rock Volume above Pipe
                var prevVolumeLevelWithRockAP = 0m;  // RcVapandMcVapNR: Rock Volume AbovePipe and MediaVol Above Pipe Next to Rock
                var prevEstUnderDrainOutflow = 0m;   // PiVest: 
                var prevRockLevelBackup = 0m;        // MiVbackRandMiVbackMnr
                var prevRockBackup2Surf = 0m;        // SiVbackRandSiVbackMnr
                var prevcumSrfcandOverFlowVol = 0m;  // ScVandOVERiVandOVERiVfirst
                // -------------------------------
                var prevTSLyrBUs = PrevTSLvlBackups.Initial(cm.LayerCount);
                // --------------------------------------------------------
                var isSloped = fclty is SlopedFacility slpd;
                for (var ts = 1; ts <= MAXTS; ts++)
                {
                    // Eq 1: SiVin: Incremental Volume Increase in this timestep
                    var stormInflow = ts > MAXRAINFALLTS ? 0m : strmEvt[ts].RunOff;
                    
                    // Equation 2: MiVin
                    var inflowFmSurface =
                        lib.Minimum(stormInflow + prevCumSurfStorage, 
                                            cm.MaxMediaInflowFromStorageVolume);
                    Debug.Assert(inflowFmSurface >= 0m, 
                        $"Timestep {ts}: inflowFromSurfaceStorage must be zero or positive.");

                    var onlyOneLayer = lyrs.OnlyOneLayer;
                    // Iterate through Layers in Blended Soil Media
                    var botmLyr = lyrs.BottomLayer;
                    var topLyr = lyrs[0];
                    foreach (var i in lyrs.Keys) // i:  0 to LayerCount -1
                    {
                        var lyr = lyrs[i];
                        var isToplayer = lyr.Equals(topLyr);
                        var isLastLayer = lyr.Equals(botmLyr); // this is last [Bottom] Layer
                        var prevLyr = i > 0? lyrs[i - 1]: null;

                        // Eq 3: MiVinSCT[y,t] (Inflow From Surface Storage) y=lyrKy, t=timestep
                        var botLyrSiz = cm.BottomLayerSize;
                        lyr.Inflow = (isToplayer ? inflowFmSurface : 
                            prevLyr.PriorTSInflow + prevTSLyrBUs[i - 1])  
                               * (isLastLayer ? botLyrSiz : 1m);
                        Debug.Assert(lyr.Inflow >= 0m, 
                            $"Inflow into Media Layer {i} must be zero or positive.");

                        //Eq 4: McVavlFinSCT [y, t]:  Volume of Available Space for backup
                        // in this layer of Blended Soil at the end of this time step 
                        lyr.CumBkUpVolumeAvail =
                            lib.Maximum(0m, cm.LayerStorageVolume.GetValueOrDefault(0m) * 
                                 (isLastLayer ? cm.BottomLayerSize : 1m) - lyr.Inflow);
                        //lyr.PriorTSInflow = lyr.Inflow;
                    }

                    var botmLyrCumBUAvl = lyrs.BottomLayer.CumBkUpVolumeAvail;
                    
                    // Eq 5: MiVout: Volume of Water Exiting Blended Soil During Time Step 
                    var fac = 1 - cm.BottomLayerSize;
                    var incMediaTotalOutflow =
                        botmLyr.PriorTSInflow + prevTSLyrBUs[cm.LayerCount - 1] +
                        fac * (onlyOneLayer ? inflowFmSurface :
                              lyrs[cm.LayerCount - 2].PriorTSInflow + prevTSLyrBUs[cm.LayerCount - 2]);

                    // Eq 6: McVFin - Cumulative from all Layers
                    var cumMediaVolumeWithoutCurrBackup = lyrs.Values.Sum(l => l.Inflow);

                    // Eq 7: McVavlFin: Cumulative Volume available for Backup: Volume of Available Space 
                    //                  for backup in Blended Soil at the end of the time step
                    var cumVolumeAvailable = lib.Maximum(0m, 
                        cm.MaxMediaVolumeAboveRock.Value - cumMediaVolumeWithoutCurrBackup);

                    // Eq 8: ORiVest: EstimatedOrificeOutflow
                    var estOrificeOutflow = 
                         ORCOEFF * cm.OrificeArea.GetValueOrDefault(0m) * TS *
                                 (decimal) Math.Sqrt(64.4d * (double) prevHead);

                    // Eq 9: GiVest: Estimated Max Incremental Infiltration to underlying soil
                    var giVx  = cm.MaxDesignInfiltrationVolPerTimeStep.GetValueOrDefault(0m); // GiVx
                    var estIncInfiltoUnderSoil
                        = IsCfgF ? lib.Minimum(giVx,
                                incMediaTotalOutflow + prevLevelWithRockVol + prevOverFlow) :
                          IsCfgE ? lib.Minimum(giVx,
                            incMediaTotalOutflow + prevLevelWithRockVol + prevOverFlowE) :
                          IsCfgA ? lib.Minimum(giVx, incMediaTotalOutflow) :
                          lib.Minimum(giVx,
                               incMediaTotalOutflow + (IsCfgA? 0m: prevLevelWithRockVol));

                    // Eq 10: PiVest: Estimated Incremental underdrain outflow
                    var estIncUnderdrainOutflow =
                        CalculateEstIncUnderdrainOutflow(
                            fclty.NeedsOrifice && orifice.HasOrifice,
                            fclty.Configuration, incMediaTotalOutflow, // MiVout [t]
                            cm.MaxIncrementalVolumeInputIntoRock.GetValueOrDefault(0m), // RiVxIN
                            estOrificeOutflow,         // ORiVest [t]
                            // ---------------------------------------------
                            prevBackUpFmRock,      // previous RiVoverandMiVnrOVER
                            prevOverFlow,               // previous OVERiV
                            prevVolumeLevelWithRockAP, // previous RcVapandMcVapNR
                            prevEstUnderDrainOutflow, // previous PiVest [t-1]
                            prevCumRockVolAbovePipe);        // previous RcVap [t-1]

                    // Eq 11: GiVandPiV: Incremental Infiltration (Outflows) 
                    //         to underlying Soil and Underdrain Outflows
                    var incOutFlow =
                        CalculateIncrementalFlowLeavingFacilitythroughUnderdrainAndSubsurface(
                            fclty.NeedsOrifice && orifice.HasOrifice, 
                            fclty.Configuration, incMediaTotalOutflow, // MiVout [t]
                            cm.MaxIncrementalVolumeInputIntoRock.GetValueOrDefault(0m), //RiVxIN
                            cm.MaxDesignInfiltrationVolPerTimeStep.Value, // GiVx
                            estIncUnderdrainOutflow, // PiVest
                            estOrificeOutflow,  // ORiVest
                            // ---------------------------------------------------
                            prevOverFlow, // previous OVERiV
                            prevOverFlowE, // previous OVERiVfirst
                            prevLevelWithRockVol, // previous RcVandMcVnr
                            prevEstUnderDrainOutflow, // previous PiVest
                            prevCumRockVolAbovePipe); // previous RcVap

                    // Eq 12: GiV: Incremental Infiltration (Outflows) to underlying Soil
                    var eSI2US = estIncInfiltoUnderSoil; // GiVest
                    var tmpA = cm.RockCapacityBelowPipe.GetValueOrDefault(0m) + 
                               cm.MediaCapacityBPnr.GetValueOrDefault(0m);
                    var tmpB = eSI2US + estIncUnderdrainOutflow; // GiVest + PiVest
                    var incSoilOutflow =
                        Configuration.ConfigABEF.HasFlag(cfg) ? estIncInfiltoUnderSoil :
                        tmpB > incOutFlow && eSI2US > tmpA ?
                        (eSI2US - tmpA) * (incOutFlow - tmpA) / (tmpB - tmpA) + tmpA : eSI2US;
                    
                    // Eq 13: PiV: Incremental Underdrain Outflow (cubic feet)
                    var IncrementalUnderdrainOutflow =
                        CalculateIncUnderdrainOutflow(cfg,
                            estIncInfiltoUnderSoil,         // GiVest
                            estIncUnderdrainOutflow,       // PiVest
                            incOutFlow,                         // GiVandPiV
                            cm.MediaCapacityBPnr.GetValueOrDefault(0m),    // McVxBPnr
                            cm.RockCapacityBelowPipe.GetValueOrDefault(0m)); // RcVxBP 

                    // Eq 14: PiVinph: rate of incremental underdrain outflow (in/hr)
                    var IncrUnderdrainOutflowRate = 
                        43200m * IncrementalUnderdrainOutflow / 
                             (cm.SurfaceArea75 * TS);

                    // Eq 15: RcVandMcVnrandRiVoverandMiVnrOVER (RockLayerVolume)
                    //    CumRockStrgVol + Cum RockStrgVolN2R + IncBackup AR + IncMedOverflowN2R
                    //     TotalOutflow2Drain: 
                    var RockLayerVolume = 
                        CalculateRockLayerVolume(cfg,
                            incMediaTotalOutflow,  // MiVout [t]
                            incSoilOutflow,       // GiV
                            IncrementalUnderdrainOutflow, // PiV
                            // -------------------------------------
                            prevRockLayerVolume,     // previous RcVandMcVnrandRiVoverandMiVnrOVER
                            prevRockLevelBackup,    // previous MiVbackRandMiVbackMnr
                            prevRockBackup2Surf,         // previous SiVbackRandSiVbackMnr
                            prevOverFlow,            // previous OVERiV
                            prevOverFlowE);         // previous OVERiVfirst

                    // Eq 16: RiVoverandMiVnrOVER: Backup From rock and Backup from media N2R
                    var incRockLevelBackUp
                        = lib.Maximum(0m, RockLayerVolume -
                             (IsCfgA ? 0m : cm.RockCapacity.Value + cm.MediaCapacityNext2Rock.Value));
                    
                    // Eq 17: RcVandMcVnr: Cumulative Rock Volume and Cumulative Media Vl N2R
                    var cumVolRockLevelVolume = lib.Maximum(0m,
                        IsCfgA ? 0m : RockLayerVolume - incRockLevelBackUp);

                    // Eq 18: RcVapandMcVapNR: Cumulative Rock Volume AbovePipe And
                    //                         Cumulative Media Volume N2R AbovePipe
                    var cumRockLevelAPVolume = IsCfgA ? 0m :
                        lib.Maximum(0m, cumVolRockLevelVolume -
                                cm.RockCapacityBelowPipe.Value - cm.MediaCapacityBPnr.Value);

                    // Eq 19: RcVap: Cumulative Rock Storage Volume AbovePipe
                    var cumRockVolAbovePipe = IsCfgA ? 0m :
                        cumRockLevelAPVolume * cm.RockCapacityAbovePipe.Value /
                            (cm.RockCapacity.Value + cm.MediaCapacityNext2Rock.Value - 
                              (cm.RockCapacityBelowPipe.Value + cm.MediaCapacityBPnr.Value));

                    // Eq 20: SiVbackRandSiVbackMnr: Incremental Backup From Rock and MediaN2R to Surface
                    var incRockLevelBackup2Surf
                        = IsCfgF ? lib.Maximum(prevBackUpFmRock - 
                                        (cm.MaxIncrementalVolumeInputIntoRock.Value + prevOverFlow), 0m) :
                                   lib.Maximum(incRockLevelBackUp - cumVolumeAvailable, 0m);

                    // Eq 21: RcVavlandMcVnrAVL: Available Rock Storage and Avail MediaN2R Storage
                     var rockLevelStorageAvail = IsCfgA ? 0m :
                        lib.Maximum(cm.RockCapacity.Value + 
                                cm.MediaCapacityNext2Rock.Value - 
                                cumVolRockLevelVolume, 0m);

                    // Eq 22: MiVbackRandMiVbackMnr 
                    var rockLevelBackup2BlndSoil = 
                        lib.Maximum(0m, 
                              IsCfgF ? lib.Maximum(incRockLevelBackUp - incRockLevelBackup2Surf -
                                      (cm.MaxIncrementalVolumeInputIntoRock.Value + prevOverFlow),0m) :
                                  lib.Maximum(incRockLevelBackUp - incRockLevelBackup2Surf));

                    // Eq 23:  MediaLayers iteration # 2
                    // This iteration for two purposes.
                    // 1. to calculate and store, for each layer that is fully saturated, 
                    //     the amount of storm water in that layer (in linear feet)
                    //     in order to later determine the Head, and
                    // 2. Populate, for each layer the value prevTSLayerBackups[lyr.Index]
                    //    for use in earlier layers iteration for the next following timestep.
                    var remBackup = rockLevelBackup2BlndSoil;
                    foreach (var lyr in lyrs.OrderByDescending(l => l.Key)
                                            .Select(l=>l.Value))
                    {

                        prevTSLyrBUs[lyr.Index] = lyr.InflowFromBackup = lyr.Backup
                            = lib.Minimum(remBackup, lyr.CumBkUpVolumeAvail);
                        //prevTSLyrBUs[lyr.Index] = lyr.Backup;

                        lyr.IsFull = remBackup >= lyr.CumBkUpVolumeAvail;
                        remBackup -= lyr.Backup;


                        // Eq 23 b: MiVsct: IncMediaVolume
                        lyr.IncVolume = lyr.Backup + lyr.Inflow;
                        
                        // Eq 23 c: MiDsct MsctNru: Incremental Media Depth in Layer
                        var lyrRatio = lyr.IncVolume / cm.LayerStorageVolume.Value;
                        lyr.IncremDepthFt = cm.LayerStorageDepthFt.Value * lyrRatio;

                        lyr.IsFullBelow = true;
                        if (!lyr.IsFull) break;
                    }

                    // Eq 24: ScVandOVERiVandOVERiVfirst: Cum Surface Volume and Incremental Overflow Volume 
                    var cumSrfcandOverFlowVol = lib.Maximum(0m, 
                            prevcumSrfcandOverFlowVol - (prevOverFlow + prevOverFlowE) + stormInflow +
                            incRockLevelBackup2Surf - inflowFmSurface);

                    // Eq 25: OVERiVfirst:                    
                    var incOverflowE = lib.Maximum(0m,
                         !IsCfgE || cumSrfcandOverFlowVol < cm.OverflowESurfaceVolume ? 0m:
                            lib.Minimum(rockLevelStorageAvail,
                                    cumSrfcandOverFlowVol - cm.OverflowESurfaceVolume.Value));

                    // Eq 26: OVERiV:
                    var incOverflow = lib.Maximum(0m,
                        cumSrfcandOverFlowVol - cm.MaxSurfaceVolume.Value -
                                    (IsCfgE ? incOverflowE:0m));

                    // Eq 27: ScV: Cumulative Surface Storage Volume:
                    var cumSurfStrgVol = lib.Maximum(cumSrfcandOverFlowVol - (incOverflow + incOverflowE), 0m);

                    // Eq 28: McD: Cumulative Media Depth:
                    var cumMedDepth = lyrs.OrderByDescending(l => l.Key)
                        .Where(l => l.Value.IsFullBelow)
                        .Sum(l => l.Value.IncremDepthFt);

                    // Eq 29: ScD: Cumulative SurfaceDepth [Surface Head]
                    var surfaceHead = cumSurfStrgVol / 
                              (!isSloped? cm.BottomArea:
                                  (fclty as SlopedFacility).Segments
                                  .Sum(s=>s.Area));

                    // Eq 30: RcDap: Cumulative Rock Depth Above Pipe [ RockHead]
                    var rockHead  = IsCfgA ? 0m :
                              cm.MaxRSDAbovePipeFt.Value * cumRockLevelAPVolume / 
                       (cm.RockCapacityAbovePipe.Value + cm.MaxMediaVolumeNRap.Value);

                    // Eq 31: HL: Head Loss: 
                    var headLoss = cumMedDepth * IncrUnderdrainOutflowRate / MINF;

                    // Eq 32: Mh: Media Head: Computed earlier ( last Line in Layers loop & below)
                    var medHead = lib.Maximum(cumMedDepth - headLoss, 0m);

                    // Eq 33: H: Head:
                    // MaxRSDAbovePipeFt (ft)
                    // rockHead ()
                    var head = 
                        rockHead < cm.MaxRSDAbovePipeFt.Value ? rockHead :
                                     cumMedDepth < cm.BSDARFt ? rockHead + medHead :
                                                                rockHead + medHead + surfaceHead;

                    // Eq 34: QiV: Total Outflows!
                    var outFlow = IsCfgF ?
                        IncrementalUnderdrainOutflow :              // PiV
                        IncrementalUnderdrainOutflow + incOverflow; // PiV + OVERiV

                    // Next two are persisted values for final Pass/Fail decision
                    var grphSE 
                        = ctch.HierarchyLevel == HierarchyLevel.Three && 
                          dsgnStrm == ds25? PreDevStormEvents[ds10]:  pdStrmEvnt;
                    timStps.AddTimestep(ts,
                        strmEvt[ts] == null? 0:
                            grphSE[ts].RunOff * (dsgnStrm == dsH2? 0.5m: 1m),
                        stormInflow, outFlow, incSoilOutflow,
                        IncrementalUnderdrainOutflow,
                        incOverflow, incOverflowE,
                         surfaceHead, head);

                    #region set previous variables for next timestep run //-----------------------
                    foreach (var lyr in lyrs.Values) lyr.PriorTSInflow = lyr.Inflow;
                    prevCumSurfStorage = cumSurfStrgVol;           // prior ScV 
                    prevHead = head;                         // prior H 
                    prevLevelWithRockVol = cumVolRockLevelVolume;  // prior RcVandMcVnr
                    prevRockLayerVolume = RockLayerVolume;         // prior RcVandMcVnrandRiVoverandMiVnrOVER
                    prevBackUpFmRock = incRockLevelBackUp;         // prior RiVoverandMiVnrOVER
                    prevVolumeLevelWithRockAP = cumRockLevelAPVolume;  // RcVapandMcVapNR
                    prevOverFlow = incOverflow;                    // OVERiV
                    prevOverFlowE = incOverflowE;                  // OVERiVfirst
                    prevCumRockVolAbovePipe = cumRockVolAbovePipe;  //
                    prevEstUnderDrainOutflow = estIncUnderdrainOutflow; //
                    prevRockLevelBackup = rockLevelBackup2BlndSoil;          // MiVbackRandMiVbackMnr
                    prevRockBackup2Surf = incRockLevelBackup2Surf; // SiVbackRandSiVbackMnr
                    prevcumSrfcandOverFlowVol = cumSrfcandOverFlowVol; //ScVandOVERiVandOVERiVfirst
                    #endregion set previous variables
                }

                reslt.PeakInflow = timStps.Max(ts => ts.Inflow);
                reslt.PeakOutflow = timStps.Max(ts => ts.Outflow);
                reslt.PeakUnderdrain = timStps.Max(ts => ts.UnderdrainOutflow);
                reslt.PeakSurfaceOverFlow = timStps.Max(ts => ts.SurfaceOverflow);
                reslt.PeakOverFlowE = timStps.Max(ts => ts.OverflowE);
                reslt.PeakTotalOverflow = timStps.Max(ts => ts.SurfaceOverflow + ts.OverflowE);
                reslt.PeakSurfaceHead = timStps.Max(ts => ts.SurfaceHead);
                reslt.PeakHead = timStps.Max(ts => ts.Head);
                // ---------------------------------------------------------------
                reslt.TotalOverflow = timStps.Sum(ts => ts.SurfaceOverflow);
                reslt.TotalOverflowE = timStps.Sum(ts => ts.OverflowE);
                reslt.TotalSoilOutflow = timStps.Sum(ts => ts.IncrementalSoilOutflow);
                reslt.TotalUnderdrainOutflow = timStps.Sum(ts => ts.UnderdrainOutflow);
                reslt.TotalInflow = timStps.Sum(ts => ts.Inflow);
                if (getTimestepDetails) reslt.Timesteps = timStps;
            }
            return reslts;
        }

        #region algorithm Step helper methods Eqs 15 - 18
        // EQ CC: RaM: 
        public decimal CalculateRaM(decimal depth, decimal length, RAMSide ram)
        {
            return
                ram == RAMSide.Both ? 2m * depth * length :
                ram == RAMSide.One ? depth * length : 0m;
        }

        // Eq 10: PiVest: Estimated Incremental underdrain outflow 
        public static decimal CalculateEstIncUnderdrainOutflow(
            bool hasOrf, Configuration cfg,
            decimal incMediaTotOut,   // MiVout[t]
            decimal maxIncRockInput,  // RiVxIN
            decimal estOROF,          // ORiVest[t]
            // ------------------------------------------------------------------------
            decimal pRLBU,            // previous RiVoverandMiVnrOVER
            decimal pOFlow,           // previous OVERiV
            decimal pAPRockLvlVol,    // previous RcVapandMcVapNR 
            decimal pestUDOF,         // previous PiVest
            decimal pCumRockAP)       // previous RcVap
        {
            var isCfgF = cfg == cfgF;
            var maxTmp = lib.Maximum(estOROF, pestUDOF);
            return 
                cfgABE.HasFlag(cfg) ? 0m :
                isCfgF ? lib.Minimum(pRLBU, maxIncRockInput + pOFlow) :
                !hasOrf? lib.Minimum(incMediaTotOut + pAPRockLvlVol,
                                       maxIncRockInput + pCumRockAP) :
                         lib.Minimum(maxTmp,
                                       incMediaTotOut + pAPRockLvlVol,
                                       maxIncRockInput + pCumRockAP);
        }

        // Eq 11: GiVandPiV - IncrementalOutflowThrough Underdrain/SubSurface
        public static decimal CalculateIncrementalFlowLeavingFacilitythroughUnderdrainAndSubsurface(
            bool hasOrf, Configuration cfg,  
            decimal incMedOut,            // MiVout
            decimal maxIncRockInput,      // RiVxIN
            decimal maxTSInfVol,          // GiVx
            decimal estUDOutflow,         // PiVest
            decimal estOROF,              // ORiVest
            // --------------------------------------
            decimal pOFlow,         // previous OVERiV
            decimal pOFlowE,        // previous OVERiVfirst
            decimal pRckLvlVol,     // previous RcVandMcVnr 
            decimal pUDOF,          // previous PiVest 
            decimal pRckVolAP)      // previous RcVap 
        {
            var isCfgA = cfg == cfgA;
            var isCfgE = cfg == cfgE;
            var isCfgF = cfg == cfgF;
            var tmpA = incMedOut + (isCfgA? 0m: pRckLvlVol);
            return cfgAB.HasFlag(cfg) ? lib.Minimum(tmpA, maxTSInfVol) :
                isCfgE ? lib.Minimum(tmpA + pOFlowE, maxTSInfVol ) :
                isCfgF ? lib.Minimum(tmpA + pOFlow, maxTSInfVol + estUDOutflow) : 
                !hasOrf? lib.Minimum(tmpA, 
                             maxTSInfVol + maxIncRockInput + pRckVolAP) :
                         lib.Minimum(tmpA,
                             maxTSInfVol + maxIncRockInput + pRckVolAP,
                             maxTSInfVol + lib.Maximum(estOROF, pUDOF));
        }

        // Eq 13: PiV: IncrementalUnderdrainOutflow
        public static decimal CalculateIncUnderdrainOutflow(
            Configuration cfg, 
            decimal eIUS,       // GiVest:     estimated Incremental Infiltration 2 UnderSoil
            decimal eIUDOF,     // PiVest:     estimated Incremental underdrain outflow
            decimal incOutFlow, // GiVandPiV:  incremental Outflow
            decimal vBPNR,      // McVxBPnr    Media Capacity Below Pipe Next2Rock
            decimal rcBP)       // RcVxBP:     Rock Capacity Below Pipe
        {
            var isCfgF = cfg == cfgF;
            var val1 = eIUS + eIUDOF;
            var val2 = rcBP + vBPNR;
            var predA = val1 > incOutFlow;
            var predB = eIUS > val2; 
            var val3 = lib.Maximum(incOutFlow - eIUS, 0m);
            // --------------------------
            return lib.Maximum(0m, 
                cfgABE.HasFlag(cfg) ? 0m :
                             isCfgF ? eIUDOF :
                     predA && predB ? eIUDOF * (incOutFlow - val2) / 
                                           (val1 - val2) :
                              predA ? val3 : eIUDOF);                
        }

        //  Eq 15: RcVandMcVnrandRiVoverandMiVnrOVER (RockLayerVolume)
        //     TotalOutflow2Drain
        public static decimal CalculateRockLayerVolume(
            Configuration cfg,
            decimal incMTOF,   // MiVout
            decimal incSOFlw,  // GiV
            decimal incUDOF,   // PiV
            // ---------------------------------
            decimal pRLV,     // previous RcVandMcVnrandRiVoverandMiVnrOVER
            decimal pBuFR,    // previous MiVbackRandMiVbackMnr
            decimal pRBU2Surf,// previous SiVbackRandSiVbackMnr
            decimal pIOF,     // previous OVERiV
            decimal pIOFE)    // previous OVERiVfirst
        {
            var isCfgE = cfg == cfgE;
            var isCfgF = cfg == cfgF;
            return lib.Maximum(0m,
                incMTOF - (incSOFlw + incUDOF) + 
                    pRLV - (pBuFR + pRBU2Surf) +
                   (isCfgE ? pIOFE : isCfgF ? pIOF : 0m));
        }
        #endregion algorithm Step helper methods

        public abstract void CalculateAreaAndVolume(Facility fclty, CommonValues cm);
        public StormResults DeterminePassFail(Catchment ctch, StormResults srs)
        {
            //var ctch = Catchment;
            var flcty = ctch.Facility;
            var cfg = flcty.Configuration;
            var needsOrf = flcty.NeedsOrifice;
            var orf = needsOrf ? flcty.BelowGrade.Orifice: null;
            var hL = ctch.HierarchyLevel;
            var selCtchmnt2Small = flcty.BelowGrade.CatchmentTooSmall.GetValueOrDefault(false);
            var NoOrificeBecausCtchTooSmall = needsOrf && 
                !orf.HasOrifice && orf.Reason == OrificeReason.CatchTooSmall || 
                selCtchmnt2Small;

            var iCfgE = cfg == cfgE;
            // ---------------------------------------
            var impArea = ctch.ImperviousArea;
            var pCN = ctch.PreCurveNumber;
            var pTC = ctch.PreTOC;
            // ------------------------------------------------------
            var pdWQ = StormEvent.Make(dswq, impArea, pCN, pTC);
            var pd2 = StormEvent.Make(ds2, impArea, pCN, pTC);
            var pdH2 = StormEvent.Make(dsH2, impArea, pCN, pTC);
            var pd5 = StormEvent.Make(ds5, impArea, pCN, pTC);
            var pd10 = StormEvent.Make(ds10, impArea, pCN, pTC);
            var pd25 = StormEvent.Make(ds25, impArea, pCN, pTC);

            var srWQ = srs[dswq];
            var sr2  = srs[ds2];
            var srH2 = srs[dsH2];
            var sr5  = srs[ds5];
            var sr10 = srs[ds10];
            var sr25 = srs[ds25];

            if (HierarchyLevel.ReqWQ.HasFlag(hL) && srWQ.TotalOverflow > 0m &&
                srWQ.TotalSoilOutflow + srWQ.TotalUnderdrainOutflow < 0.9m * srWQ.TotalInflow)
                AddFailure(srWQ, FailType.WQOverflow,
                    $"Water Quality is required, and only [{srWQ.TotalSoilOutflow + srWQ.TotalUnderdrainOutflow: 0.00 cubic ft}] " +
                    $" has exited the facility in the first  72 hours.{sNL}" +
                    $"A minimum of 90% of the original inflow [{srWQ.TotalInflow: 0.00 cubic ft} ], " +
                    " must exit within this time period to satisfy City Water Quality requirements.");

            switch (hL)
            {
                case H1:  
                    #region H1 
                    if (iCfgE && sr10.TotalOverflow > 0m)
                        AddFailure(sr10, FailType.TotOverflow,
                            $"Configuration E, {hL} Facility and ten-year storm " +
                            $"had {sr10.TotalOverflow: 0.0 ft} greater than zero");
                    if (cfg != cfgE && sr10.PeakOutflow > 0m)
                        AddFailure(sr10, FailType.PeakOutflow,
                            $"Configuration {cfg}, H1 Facility and ten-year storm had " +
                            $"{sr10.PeakOutflow: 0.00 cubic ft} peak outflow greater than zero.");

                    #endregion H1
                    break;

                case H2B:
                case H2C:
                    #region H2B/H2C
                    var thrshld2bc =
                        ctch.HierarchyLevel == H2C ? 699 :
                        flcty.IsPublic ? 2399M : 2369M;
                    var thisSr2 = hL == H2B ? srH2 : sr2;
                    var pkOFThshld =  hL == H2B ? srH2.PeakOutflow: sr2.PeakOutflow;
                    var pkOfPreDev = (hL == H2B ? 0.5m : 1m) * pd2.Max(s => s.RunOff);
                    if (pkOFThshld > pkOfPreDev)
                        AddFailure(thisSr2, FailType.XcdsPreDev,
                            $"Hierarchy {hL} Facility and {(hL == H2B? "one half of ": string.Empty)}" +
                            $"peak outflow, [{thisSr2.PeakOutflow: 0.00 cubic ft}], " +
                            $"exceeded  {(hL == H2B ? "one half the " : string.Empty)} peak Pre-Development Runoff " +
                            $"[{thisSr2.PeakPreDevRunoff: 0.00 cubic ft}].");

                    if (sr5.PeakOutflow > pd5.Max(s => s.RunOff))
                        AddFailure(sr5, FailType.XcdsPreDev, $"Hierarchy {hL} Facility and had " + 
                            $"{sr5.TotalOverflow: 0.00 'cubic feet'} total overflow.");

                    if (sr10.PeakOutflow > pd10.Max(s => s.RunOff))
                        AddFailure(sr10, FailType.XcdsPreDev,
                            $"Hierarchy {hL} Facility and had " +
                            $"{sr10.TotalOverflow: 0.00 cubic ft} total overflow.");

                    if (hL == H2B && !flcty.IsPublic &&
                        sr25.PeakOutflow > pd25.Max(s => s.RunOff))
                        AddFailure(sr25, FailType.XcdsPreDev,
                            $"Hierarchy H2B Private Facility and had " +
                            $"{sr25.TotalOverflow: 0.00 cubic ft} total overflow.");

                    if (NoOrificeBecausCtchTooSmall)
                    {
                        if (sr25.TotalOverflow > 0m)
                            AddFailure(sr25, FailType.CatchTooSmallOverflow,
                                $"Hierarchy {hL} Facility without orifice and had " +
                                $"{sr25.TotalOverflow: 0.00 cubic ft} total overflow.");
                        if (ctch.ImperviousArea > thrshld2bc)
                            AddFailure(sr25, FailType.ImperArea2Large,
                                $"Hierarchy {hL} Facilities with Impervious area greater than {thrshld2bc:0 sq ft}{sNL}" +
                                    "require that justification be provided in the storm report.", JUST);
                        if(srWQ.PeakSurfaceOverFlow > 0m)
                            AddFailure(srWQ, FailType.PeakSurfaceOverflow,
                                $"Hierarchy {hL} Facilities specified as Catchment too small{sNL}" +
                                    "must not discharge any Overflow from the surface.");
                        if (sr25.PeakSurfaceOverFlow > 0m)
                            AddFailure(sr25, FailType.PeakSurfaceOverflow,
                                $"Hierarchy {hL} Facilities specified as Catchment too small{sNL}" +
                                "must not discharge any Overflow from the surface.");
                    }

                    if (sr25.TotalOverflow > 0m && ctch.ImperviousArea > thrshld2bc)
                        AddFailure(sr25, FailType.ImperArea2Large,
                            $"Any Facility with Impervious area greater than {thrshld2bc:0 sq ft}{sNL}" +
                                "require that justification be provided in the storm report.", JUST);
                    #endregion H2B/H2C
                    break;

                case H3:
                    #region H3
                    var thrshldH3 = ctch.PostCurveNumber == 72 ? 1119m : 499m;
                    if (NoOrificeBecausCtchTooSmall)
                    {
                        if (sr25.TotalOverflow > 0m)
                            AddFailure(sr25, FailType.CatchTooSmallOverflow,
                                $"Hierarchy {hL} Facility without orifice and had " +
                                $"{sr25.TotalOverflow: 0.00 'cubic ft'} total overflow.");
                        else if (ctch.ImperviousArea > thrshldH3)
                            AddFailure(sr25, FailType.ImperArea2Large,
                                $"Hierarchy H3 Facilities using Post-Development Curve Number {ctch.PostCurveNumber}, " +
                                   $"with Impervious area less than {thrshldH3:0 sq ft}{sNL}" +
                                   "require that justification be provided in the storm report.", JUST);
                    }

                    if (NoOrificeBecausCtchTooSmall)
                    {
                        if (sr25.TotalOverflow > 0m && ctch.ImperviousArea > thrshldH3)
                            AddFailure(sr25, FailType.ImperArea2Large,
                                $"Hierarchy H3 Facility without orifice with " +
                                $"{ctch.ImperviousArea: 0 sq ft} impervious area.", JUST);
                        if (sr25.PeakSurfaceOverFlow > 0m)
                            AddFailure(sr25, FailType.PeakSurfaceOverflow,
                                $"Hierarchy {hL} Facilities specified as Catchment too small{sNL}" +
                                "must not discharge any Overflow from the surface.");
                    }

                    var Y10PreDevPkRO = pd10.Max(s => s.RunOff);
                    if (sr25.PeakOutflow > Y10PreDevPkRO && !NoOrificeBecausCtchTooSmall)
                        AddFailure(sr25, FailType.XcdsPreDev,
                            $"Hierarchy H3 Facility and had " +
                            $"{sr25.PeakOutflow: 0.00 cubic ft} peak outflow, {sNL}" +
                            "which exceeds the 10-year Pre-Development Peak Runoff " +
                            $"[{Y10PreDevPkRO: 0.00 cubic ft} ].");
                    #endregion H3
                    break;
            }

            if (hL != HierarchyLevel.Three)
            {
                if (srWQ.TotalOverflow > 0m)
                    AddFailure(srWQ, FailType.WQOverflow,
                        $"Configuration {cfg}, {hL} Facility and had " +
                        $"{srWQ.TotalOverflow: 0.00 'cubic feet'} total overflow.");
                if (iCfgE && srWQ.TotalOverflowE > 0m)
                    AddFailure(srWQ, FailType.WQOverflow,
                        $"Configuration E, {hL}  Facility and had " +
                        $"{srWQ.TotalOverflowE: 0.00 'cubic feet'} total overflow from second overflow pipe.");
            }

            foreach (var sr in StormEvents
                .Select(sE => srs[sE.DesignStorm]))
                if(sr.Status == PEND) sr.Status = PASS;

            return srs;
        }

        protected void ValdiateConfigEAreaVolumeData(AboveGradeProperties agp, bool needsESurfaceArea = false)
        {
            if (!agp.OverflowEHeight.HasValue)
                throw new PACInputDataException(
                    "OverflowE Height (height of first overflow pipe opening), " +
                    "is required for calculations for configuration E Facilities.",
                    "OverflowEHeight");
            if (needsESurfaceArea && !agp.OverflowESurfaceArea.HasValue)
                throw new PACInputDataException(
                    "Overflow E Surface Area is required for " +
                    "calculations for Configuration E Facilities.",
                    "OverflowESurfaceArea");
        }
        #endregion abstract methods

        #region helper methods
        public void AddFailure( StormResult sr, FailType typ, string msg,
            PRStatus stat = FAIL, int? ts = null)
        {
            sr.Status = stat;
            sr.Failures.AddFailure(typ, msg, ts);
        }
        #endregion helper methods
    }

    internal class PrevTSLvlBackups : Dictionary<int, decimal>
    {
        #region factory
        internal static PrevTSLvlBackups Initial(int cnt)
        {
            var ptsBUs = new PrevTSLvlBackups();
            for(var i = 0; i < cnt; i++) ptsBUs.Add(i, 0m);
            return ptsBUs;
        } 
        #endregion factory
    }
}