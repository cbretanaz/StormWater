using lib = CoP.Enterprise.Utilities;
namespace BES.SWMM.PAC
{
    public class SlopedFacilityCalculator : Calculator
    {
        #region properties
        public SlopedFacility sloped { get; set; }
        #endregion properties

        #region ctor/Factories
        public SlopedFacilityCalculator() {}
        public static SlopedFacilityCalculator Default => new SlopedFacilityCalculator();
        #endregion ctor/Factories

        #region virtual Overrides

        #region virtual Validation method Overrides
        protected override void ValidateAGInputData(AboveGradeProperties agDta, bool isCfgE = false)
        {
            if (isCfgE) ValdiateConfigEAreaVolumeData(agDta);
        }
        protected override void ValidateBGInputData(BelowGradeProperties bgDta, Configuration cfg)
        {
            var IsCfgD = cfg == cfgD;
            if (!IsCfgD && !bgDta.InfiltrationPercentage.HasValue)
                throw new PACInputDataException(
                    "Infiltration Percentage is required for calculations",
                    "InfiltrationPercentage");
        }
        public void ValidateSegmentData(Segments segs)
        {
        }
        #endregion virtual Validation method Overrides

        public override void CalculateAreaAndVolume(Facility slpd, CommonValues cm)
        {
            var cfg = slpd.Configuration;
            var IsCfgE = cfg == cfgE;
            var segs = ((SlopedFacility) slpd).Segments;
            var surfArea75 = 0m;
            var totalVol = 0m;
            var surfArea100 = 0m;
            var agDta = slpd.AboveGrade;
            #region LastSegment Calculations

            var lastSeg = segs.LastSegment;
            var oEHgt = agDta.OverflowEHeight.GetValueOrDefault(0m);

            // ScLFIRST
            var ofELen = lib.Minimum(oEHgt / lastSeg.LongitudinalSlope,
                                            lastSeg.Length - lastSeg.CheckDamWidth / 2m);
            #endregion LastSegment Calculations

            foreach (var s in segs)
            {
                var slp = s.LongitudinalSlope;
                var chkDamLen = s.CheckDamWidth;
                var bw = s.BottomWidth; // ScWsf
                var sumSlopes = s.RightSlope + s.LeftSlope;
                // ---------------------------------
                // 1: ScDdwnSF
                var dwnDep = s.DownStreamDepthFt;

                // 2: ScL75per
                var segLen75 = slp == 0m? s.Length - chkDamLen/2m: 
                    lib.Minimum(0.75m * dwnDep/slp, s.Length - chkDamLen/2m);

                // 3: ScDup75SF
                var upDep75 = 
                    lib.Maximum( 0.75m * dwnDep - segLen75 * slp, 0m);


                // 4: ScL100per
                var segLen100 = slp == 0m?  s.Length - chkDamLen / 2m:
                        lib.Minimum(dwnDep / slp, s.Length - chkDamLen / 2m);

                // 5: ScDup100SF
                var upDep100 = lib.Maximum(dwnDep - segLen100 * slp, 0m);

                // 6: ScAx75PER
                var area75 =
                    segLen75 * (bw + sumSlopes * (0.75m*dwnDep + upDep75) / 2m);

                // 7: ScAx100PER
                var area100 = segLen100 * (bw + sumSlopes * (dwnDep + upDep100) / 2m);

                // 8: ScVx
                //var volume = 0.5m *
                //     (0.5m * len100 * upDep100 * (bw + bw + upDep100 * sumSlopes) +
                //     dwnDep * (bw + bw + dwnDep * sumSlopes));
                var volume = segLen100 * 
                     (upDep100 * (bw + upDep100 * sumSlopes/2m) + 
                        dwnDep * (bw + dwnDep * sumSlopes/2m)) / 2m;
                // ------------------------------------------------------------------
                
                if (s.LandscapeWidth < bw + s.DownStreamDepthFt * sumSlopes)
                {
                    var mssg = "Not enough space for the facility.  Need to revise.";
                    throw new PACSizingException(mssg);
                }

                surfArea75 += area75;
                totalVol += volume;
                surfArea100 += area100;

                if (!IsCfgE) continue;
                // 9: ScDupFIRST For Config E only
                var ofEDepth = 
                    lib.Maximum(oEHgt - ofELen * s.LongitudinalSlope, 0m);
            }

            cm.SurfaceArea75 = surfArea75;
            cm.MaxSurfaceVolume = totalVol;
            cm.SurfaceArea100 = surfArea100;
            // -----------------------------
            if (!IsCfgE) return;

            // Are these necessary ??? 
            cm.OverflowESurfArea75 = 0m; 
            cm.OverflowESurfaceVolume = 0m;
        }
        public override StormResults CalculateResults(Catchment ctch, bool getTimestepDetails = true)
        {
            InitializeEngine(ctch);
            var fclty = ctch.Facility;
            var aT = fclty.GetType();
            var et = typeof(SlopedFacility);
            if (!(fclty is SlopedFacility))
                throw new WrongFacilityException(aT, et,
                    $"Wrong Type Facility passed: Expected " +
                    $"{et.FullName}, but {aT.FullName} was passed.",
                    null);
            ValidateInputData(ctch);
            var cm = CalculateCommonValues(ctch);
            var rslts = IterateTimeSteps(ctch, cm, getTimestepDetails);
            return DeterminePassFail(ctch, rslts);
        }
        #endregion virtual Overrides
    }
}