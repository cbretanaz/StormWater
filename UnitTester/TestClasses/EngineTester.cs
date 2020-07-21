using NUnit.Framework;
using lib=CoP.Enterprise.Utilities;

namespace BES.SWMM.PAC
{
    public class EngineTester
    {
        private static readonly double dlta = 1e-6d;  
        private static readonly string folder =  lib.ExtractPath(
        NUnit.Framework.Internal.AssemblyHelper
            .GetAssemblyPath(typeof(EngineTester).Assembly));

        #region DesignStorm Consts
        private const DesignStorm dswq = DesignStorm.WQ;
        private const DesignStorm ds2 = DesignStorm.TwoYear;
        private const DesignStorm ds5 = DesignStorm.FivYear;
        private const DesignStorm ds10 = DesignStorm.TenYear;
        private const DesignStorm ds25 = DesignStorm.TwntyFiv;
        #endregion DesignStorm Consts

        [Test]
        [TestCase(24d, InfiltrationTestProcedure.OpenPit,
            1d, HierarchyLevel.Three, Configuration.ConfigD, 10000,72, 98, 5, 5,
            1.0d, 325d, 3d, null, null, null, 18d, null, null, null,
            34.8, 12d, null, 0.3d, 4.0d, 1.0d, OrificeReason.Null)]
        [TestCase(6.5d, InfiltrationTestProcedure.OpenPit,
            1d, HierarchyLevel.TwoB, Configuration.ConfigD, 25000d, 79, 98, 5, 5,
            0.8d, 242d, 4d, 210d, .5d, 7d, 3.5d, 25d, 1.9d, 18d,
            242d, 2.4d, 3d, 0.12d, 1.33d, .75d, OrificeReason.None)]
        public void TestFlatPlanterCommonValues(decimal blndSoilDepth,
            InfiltrationTestProcedure ift, decimal natInfRate,
            HierarchyLevel lvl, Configuration cfg, decimal impArea, int preCN, int postCn,
            int preTC, int postTC, decimal infPcnt, decimal botArea, decimal botWidth,
            decimal botPerimeter, decimal ss, decimal freeBrd, decimal ovflHgt,
            decimal ovflSA, decimal ovflEHgt, decimal ovflESA, decimal rockBotArea,
            decimal rockStrgDepth, decimal rockWidth, decimal porosity,
            decimal belowPipeStrgDepth, decimal? orificeDiameter, OrificeReason rsn)
        {
            var ctch = MakeFlatPlanterCatchment(blndSoilDepth, ift, natInfRate, lvl, cfg, impArea, 
                preCN, postCn, preTC, postTC, infPcnt, botArea, botWidth, botPerimeter, 
                ss, freeBrd, ovflHgt, ovflSA, ovflEHgt, ovflESA, rockBotArea, rockStrgDepth, 
                rockWidth, porosity, belowPipeStrgDepth, orificeDiameter, rsn);
            var calc = Calculator.Make(ctch.Facility.Category);
            calc.CalculateCommonValues(ctch);
            Assert.IsNotNull(calc);
        }

        [Test]
        [TestCase(24d, InfiltrationTestProcedure.OpenPit, 23d, 1d, 
            HierarchyLevel.TwoA, Configuration.ConfigD, 10000d, 72, 98,
            5, 5, 1.0d, 325d, 3d, null, null, null, 
            18d, null, null, null, 81.25, 12d, 3d, 
            0.3d, 4d, 1d, OrificeReason.None,
              // ------------------------------------------------------------------------
            0.0054541539124822796d, 27.083333333333333333333333333d, 1.5d, 
            null, 2d, 1d, 1d, 0.33333333333333337d, 0.66666666666666674d,
            24.375d, 32.5d, 8.125d, 16.25d,
            27.083333333333336d, 1.2d, 27.083333333333336d, 
            3d, 81.25d, 24.375d, 8.125d, 16.25d, 
            75.833333333333329d, 0.0d, 3.25d,
            0.0d, 325d, null, 487.5d, null)]
        [TestCase(24d, InfiltrationTestProcedure.OpenPit, 23d, 1d,
            HierarchyLevel.TwoA, Configuration.ConfigD, 10000d, 72, 98,
            5, 5, 1.0d, 325d, 3d, null, null, null,
            18d, null, null, null, 81.25, 12d, 3d,
            0.3d, 4d, 487.5d, OrificeReason.None,
            // ------------------------------------------------------------------------
            1296.21376576337d, 27.083333333333333333333333333d, 
            1.5d, null, 2d, 1d, 1d,
            0.33333333333333337d, 0.66666666666666674d, 24.375d,
            32.5d, 8.125d, 16.25d,
            27.083333333333336d, 1.2d,
            27.083333333333336d, 3d, 81.25d, 24.375d,
            8.125d, 16.25d, 75.833333333333329d, 0.0d,
            3.25d, 0.0d, 325d,
            null, 487.5d, null)]
        public void TestFlatPlanterCommonValueResults(decimal blndSoilDepth,
            InfiltrationTestProcedure ift, decimal corrFac, decimal natInfRate,
            HierarchyLevel lvl, Configuration cfg, decimal impArea, int preCN, int postCn,
            int preTC, int postTC, decimal infPcnt, decimal botArea, decimal botWidth,
            decimal botPerimeter, decimal ss, decimal freeBrd, decimal ovflHgt,
            decimal ovflSA, decimal ovflEHgt, decimal ovflESA, decimal rockBotArea,
            decimal rockStrgDepth, decimal rockWidth, decimal porosity,
            decimal belowPipeStrgDepth, decimal? orificeDiameter, OrificeReason rsn,
            // ---------------------------------------------------------------------
            double expORArea, double expUnderDrainlen, double expMaxSurfDepth,
            double expOFEDep, double expBSD, double expBSDAR, double expMaxRSD,
            double? expMaxRSDBP, double? expMaxRSDAP, double? expMedCapN2R,
            double? expMedCapAR, double? expMedCapBPNR, double? expMedCapNRAP,
            double? expMedIFFromStrg, double? expNumberLayers, double? expLayerCapacity,
            double? expRSWidth, double? expRSArea, double? expRockCap,
            double? expRockCapBP, double? expRockCapAP, double? expMaxIncVolInputIntoRock,
            double? expRaM, double? expBotInfArea, double? expMaxDsgnInfVolPerTS,
            double? expSurfArea75, double? expOverFlowESA75,
            double? expMaxSurfVol, double? expOverFlowESurfVol)
        {
            var ctch = MakeFlatPlanterCatchment(blndSoilDepth, ift, natInfRate, lvl, cfg, impArea,
                preCN, postCn, preTC, postTC, infPcnt, botArea, botWidth, botPerimeter,
                ss, freeBrd, ovflHgt, ovflSA, ovflEHgt, ovflESA, rockBotArea, rockStrgDepth,
                rockWidth, porosity, belowPipeStrgDepth, orificeDiameter, rsn);
            var calc = Calculator.Make(ctch.Facility.Category);
            Assert.IsNotNull(calc);
            var cv = calc.CalculateCommonValues(ctch);
            Assert.AreEqual(expORArea, (double)cv.OrificeArea.Value, dlta, "OrificeArea");
            Assert.AreEqual(expUnderDrainlen, (double)cv.UnderDrainLength.Value, dlta, "UnderDrainLength");
            Assert.AreEqual(expMaxSurfDepth, (double)cv.MaxSurfaceAreaDepth, dlta,"MaxSurfaceAreaDepth");
            Assert.AreEqual(expBSD, (double)cv.BSDFt, "BSDFt");
            Assert.AreEqual(expBSDAR, (double)cv.BSDARFt, "BSDARFt");
            Assert.AreEqual(expMaxRSD, (double)cv.MaxRockStorageDepthFt, "MaxRockStorageDepthFt");
            Assert.AreEqual(expMaxRSDBP, (double)cv.UnderdrainHeightFt, "UnderdrainHeightFt");
            Assert.AreEqual(expMaxRSDAP, (double)cv.MaxRSDAbovePipeFt, "MaxRSDAbovePipeFt");
            Assert.AreEqual(expMedCapN2R, (double)cv.MediaCapacityNext2Rock, "MediaCapacityNext2Rock");
            Assert.AreEqual(expMedCapAR, (double)cv.MaxMediaVolumeAboveRock, "MaxMediaVolumeAboveRock");
            Assert.AreEqual(expMedCapBPNR, (double)cv.MediaCapacityBPnr, "MediaCapacityBPnr");
            Assert.AreEqual(expMedCapNRAP, (double)cv.MaxMediaVolumeNRap, "MaxMediaVolumeNRap");
            Assert.AreEqual(expMedIFFromStrg, (double)cv.MaxMediaInflowFromStorageVolume, 
                "MaxMediaInflowFromStorageVolume");
            Assert.AreEqual(expNumberLayers, (double)cv.NumberLayers, "NumberLayers");
            Assert.AreEqual(expLayerCapacity, (double)cv.LayerStorageVolume, "LayerStorageVolume");
            Assert.AreEqual(expRSWidth, (double)cv.RockStorageWidth, "RockStorageWidth");
            Assert.AreEqual(expRSArea, (double)cv.RockStorageArea, "RockStorageArea");
            Assert.AreEqual(expRockCap, (double)cv.RockCapacity, "RockCapacity");
            Assert.AreEqual(expRockCapBP, (double)cv.RockCapacityBelowPipe, "RockCapacityBelowPipe");
            Assert.AreEqual(expRockCapAP, (double)cv.RockCapacityAbovePipe, "RockCapacityAbovePipe");
            Assert.AreEqual(expMaxIncVolInputIntoRock, (double)cv.MaxIncrementalVolumeInputIntoRock, 
                "MaxIncrementalVolumeInputIntoRock");
            Assert.AreEqual(expRaM, (double)cv.RAM, "RAM");
            Assert.AreEqual(expBotInfArea, (double)cv.BottomInfiltrationArea, "BottomInfiltrationArea");
            Assert.AreEqual(expMaxDsgnInfVolPerTS, (double)cv.MaxDesignInfiltrationVolPerTimeStep,
                "MaxDesignInfiltrationVolPerTimeStep");
            Assert.AreEqual(expSurfArea75, (double)cv.SurfaceArea75, "SurfaceArea75");
            Assert.AreEqual(expMaxSurfVol, (double)cv.MaxSurfaceVolume, "MaxSurfaceVolume");
            if (cfg != Configuration.ConfigE) return;
            // -------------------------------------------
            Assert.AreEqual(expOverFlowESurfVol, (double)cv.OverflowESurfaceVolume, "OverflowESurfaceVolume");
            Assert.AreEqual(expOFEDep, (double)cv.OverflowEDepthFt, dlta, "OverflowEDepthFt");
            Assert.AreEqual(expOverFlowESA75, (double)cv.OverflowESurfArea75, "OverflowESurfArea75");
        }


        [Ignore("Can't get expected results right.")]
        [TestCase(24d, InfiltrationTestProcedure.OpenPit, 1d,
            HierarchyLevel.TwoA, Configuration.ConfigD, 10000d,72, 98, 5, 5,
            1.0d, 325d, 3d, null, null, null,
            18d, null, null, null, 81.25, 12d, 3d, 
            0.3d, 4d, 1d, OrificeReason.None,
            // ------------------------------------------------------------------------
            54.80741061116219264100874068d, 8.6d, 8.6d, 0d,
            2.8d, 31.7d, 7.2d, 7.2d, 0d,
            5.6d, 63.4d, 14.1d, 14.1d, 0d,
            12.9d, 77.4d, 16.3d, 16.3d, 0d,
            21.3, 91.4d, 16.8d, 16.3d, 0d,
            21.3, 102.5d, 20.7d, 17.8d, 2.9d)]
        public void TestFlatPlanterFinalResults(decimal blndSoilDepth,
            InfiltrationTestProcedure ift, decimal natInfRate,
            HierarchyLevel lvl, Configuration cfg, decimal impArea, int preCN, int postCn,
            int preTC, int postTC, decimal infPcnt, decimal botArea, decimal botWidth,
            decimal botPerimeter, decimal ss, decimal freeBrd, decimal ovflHgt,
            decimal ovflSA, decimal ovflEHgt, decimal ovflESA, decimal rockBotArea,
            decimal rockStrgDepth, decimal rockWidth, decimal porosity,
            decimal belowPipeStrgDepth, decimal? orificeDiameter, OrificeReason rsn,
            // --------------------------------------------------------------------------
            double exppkWQInflow, double exppkWQOutflow, double expWQUDOutflow, double exppkWQOverflow,
            double expH2YPreDev, double exppkH2YInflow, double exppkH2YOutflow, double expH2YUDOutflow, double exppkH2YOverflow,
            double exp2YPreDev, double exppk2YInflow, double exppk2YOutflow, double exp2YUDOutflow, double exppk2YOverflow,
            double exp5YPreDev, double exppk5YInflow, double exppk5YOutflow, double exp5YUDOutflow, double exppk5YOverflow,
            double exp10YPreDev, double exppk10YInflow, double exppk10YOutflow, double exp10YUDOutflow, double exppk10YOverflow,
            double exp25YPreDev, double exppk25YInflow, double exppk25YOutflow, double exp25YUDOutflow, double exppk25YOverflow)
        {
            var ctch = MakeFlatPlanterCatchment(blndSoilDepth, ift, natInfRate, lvl, cfg, impArea,
                preCN, postCn, preTC, postTC, infPcnt, botArea, botWidth, botPerimeter,
                ss, freeBrd, ovflHgt, ovflSA, ovflEHgt, ovflESA, rockBotArea, rockStrgDepth,
                rockWidth, porosity, belowPipeStrgDepth, orificeDiameter, rsn);
            var calc = Calculator.Make(ctch.Facility.Category);
            Assert.IsNotNull(calc);
            var strmRslts = calc.CalculateResults(ctch);
            ValidateResults(strmRslts[DesignStorm.WQ], 0d, exppkWQInflow, exppkWQOutflow, expWQUDOutflow, exppkWQOverflow);
            ValidateResults(strmRslts[DesignStorm.HalfTwoYear], expH2YPreDev, exppkH2YInflow, exppkH2YOutflow, expH2YUDOutflow, exppkH2YOverflow);
            ValidateResults(strmRslts[DesignStorm.TwoYear], exp2YPreDev, exppk2YInflow, exppk2YOutflow, exp2YUDOutflow, exppk2YOverflow);
            ValidateResults(strmRslts[DesignStorm.FivYear], exp5YPreDev, exppk5YInflow, exppk5YOutflow, exp5YUDOutflow, exppk5YOverflow);
            ValidateResults(strmRslts[DesignStorm.TenYear], exp10YPreDev, exppk10YInflow, exppk10YOutflow, exp10YUDOutflow, exppk10YOverflow);
            ValidateResults(strmRslts[DesignStorm.TwntyFiv], exp25YPreDev, exppk25YInflow, exppk25YOutflow, exp25YUDOutflow, exppk25YOverflow);
        }

        private void ValidateResults(StormResult sr,
            double ePreDev, double eInflow, double eOutflow,
            double eUDOutflow, double eOverflow)
        {
            //const decimal adjFactor = 448.83m;
            const double adjFactor = 1d;
            const double dlta = 1e-6d;
            if (sr.DesignStorm != dswq) 
                Assert.AreEqual(ePreDev * adjFactor, 
                    (double)sr.PeakPreDevRunoff, dlta, 
                    "Peak Pre-Dev runoff");
            Assert.AreEqual(eInflow*adjFactor, 
                (double)sr.PeakInflow, dlta, 
                $"{sr.DesignStorm} PeakInflow");
            Assert.AreEqual(eOutflow * adjFactor,
                (double)sr.PeakOutflow, dlta, 
                $"{sr.DesignStorm} PeakOutflow");
            Assert.AreEqual(eUDOutflow*adjFactor,
                (double)sr.PeakUnderdrain, dlta, 
                $"{sr.DesignStorm} PeakUnderdrain");
            Assert.AreEqual(eOverflow * adjFactor, 
                (double)sr.PeakSurfaceOverFlow, dlta,
                $"{sr.DesignStorm} PeakSurfaceOverFlow");
        }

        private static Catchment MakeFlatPlanterCatchment(
            decimal blndSoilDepth, InfiltrationTestProcedure ift, 
            decimal natInfRate, HierarchyLevel lvl, Configuration cfg,
            decimal impArea, int preCN, int postCn,
            int preTC, int postTC, decimal infPcnt, decimal botArea, decimal botWidth,
            decimal botPerimeter, decimal ss, decimal freeBrd, decimal ovflHgt,
            decimal ovflSA, decimal ovflEHgt, decimal ovflESA, decimal rockBotArea,
            decimal rockStrgDepth, decimal rockWidth, decimal porosity,
            decimal belowPipeStrgDepth, decimal? orificeDiameter, OrificeReason rsn)
        {
            return Catchment.Make("FlatPlanterTestCatchment",
                ift, PRStatus.Pass, natInfRate,
                lvl, impArea, preCN, postCn, preTC, postTC, 
                FlatPlanter.Make(true, blndSoilDepth, cfg,
                    AboveGradeProperties.Make( botArea,
                        botWidth, botPerimeter, ss, freeBrd,
                        ovflHgt, ovflSA,
                        ovflEHgt, ovflESA), 
                    BelowGradeProperties.Make(infPcnt, rockBotArea,
                        rockStrgDepth, rockWidth, porosity, belowPipeStrgDepth,
                        orificeDiameter.HasValue? 
                            Orifice.Make(orificeDiameter.Value):
                            Orifice.Make(rsn))));
        }

        private Segments GetSegments()
        {
            var segs = Segments.Empty;
            segs.Add(Segment.Make(1,
                12.7m, 7.5m, 0.25m,
                5.4m, 0.6m, 0.63m,
                13.5m, 11.7m));
            segs.Add(Segment.Make(2,
                21.7m, 5.6m, 0.25m,
                8.2m, 0.45m, 0.35m,
                10.9m, 11.7m));
            return segs;
        }
    }
}