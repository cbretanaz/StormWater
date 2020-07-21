using System;
namespace BES.SWMM.PAC
{
    public class RectBasinCalculator : Calculator
    {
        #region properties
        public RectangularBasin Rect { get; set; }
        #endregion properties

        #region ctor/Factories
        public RectBasinCalculator() { }
        public static RectBasinCalculator Default => new RectBasinCalculator();
        #endregion ctor/Factories

        #region virtual Overrides 
        public override void CalculateAreaAndVolume(Facility rect, CommonValues cm)
        {
            var IsCfgE = rect.Configuration == cfgE;
            var agDta = rect.AboveGrade;
            var ba = agDta.BottomArea.GetValueOrDefault(0m);
            var ss = agDta.SideSlope.GetValueOrDefault(0m);
            var oHgt = agDta.OverflowHgtFt;
            var bw = agDta.BottomWidth.Value;
            // -------------------------------------

            cm.SurfaceArea100 = ba + 2m * (ba / bw + bw) * ss * oHgt +
                               (decimal)(Math.PI * Math.Pow((double)(ss * oHgt), 2d));
            // ----------------------------------------------------------------
            cm.SurfaceArea75 = ba + 1.5m * (ba / bw + bw) * ss * oHgt +
                               (decimal)(Math.PI * Math.Pow((double)(0.75m * ss * oHgt), 2d));
            // ----------------------------------------------------------------
            cm.MaxSurfaceVolume =
                !agDta.SideSlope.HasValue || !agDta.OverflowHeight.HasValue ? 0m :
                    ba * oHgt + (ba / bw + bw) * ss * (decimal)Math.Pow((double)oHgt, 2d) + 
                    (decimal)(Math.PI * Math.Pow((double)ss, 2d) * Math.Pow((double)oHgt, 3d) / 3d);
            // -----------------------------------------------------------------
            // -----------------------------------------------------------------
            if (!IsCfgE) return;
            var oEHgt = agDta.OverflowEHgtFt;

            cm.OverflowESurfaceVolume =
                !agDta.SideSlope.HasValue || !agDta.OverflowEHeight.HasValue ? 0m :
                    ba * oEHgt + (ba / bw + bw) * ss * (decimal)Math.Pow((double)oEHgt, 2d) +
                    (decimal)(Math.PI * Math.Pow((double)ss, 2d) * Math.Pow((double)oEHgt, 3d) / 3d);
            cm.OverflowESurfArea75 =
                    ba + 1.5m * ss * oEHgt * (ba / bw + bw) +
                    (decimal)(Math.PI * Math.Pow(0.75d * (double)(ss * oEHgt), 2d));
            // -------------------------------------------------------------------------------------
        }
        protected override void ValidateAGInputData(
            AboveGradeProperties agDta, bool isCfgE = false)
        {
            if (!agDta.BottomArea.HasValue)
                throw new PACInputDataException(
                    "Bottom Area is required for calculations",
                    "Bottom Area");
            if (!agDta.BottomWidth.HasValue)
                throw new PACInputDataException(
                    "BottomWidth is required for calculations",
                    "BottomWidth");
            // --------------------------------------
            if (!agDta.SideSlope.HasValue)
                throw new PACInputDataException(
                    "SideSlope is required for calculations",
                    "SideSlope");
            // -------------------------------------
            if (isCfgE && !agDta.OverflowHeight.HasValue)
                throw new PACInputDataException(
                    "OverflowHeight, (height of first overflow pipe opening), " +
                    "is required for calculations",
                    "OverflowHeight");
            // -------------------------------------
            if (!agDta.FreeboardDepth.HasValue)
                throw new PACInputDataException(
                    "Freeboard Depth is required for calculations",
                    "FreeboardDepth");
            // -------------------------------------
            if (isCfgE) ValdiateConfigEAreaVolumeData(agDta);
        }
        protected override void ValidateBGInputData(BelowGradeProperties bgDta, Configuration cfg)
        {
            var IsCfgA = cfg == cfgA;
            var IsCfgD = cfg == cfgD;
            if (!IsCfgD && !bgDta.InfiltrationPercentage.HasValue)
                throw new PACInputDataException(
                    "Infiltration Percentage is required for calculations",
                    "InfiltrationPercentage");
            // ----------------------------------------
            if (IsCfgA) return; // --------------------
            // ----------------------------------------

            if (!bgDta.RockStorageDepth.HasValue)
                throw new PACInputDataException(
                    "Rock Storage Depth is required for calculations",
                    "RockStorageDepth");
            // ----------------------------------------
            if (!bgDta.RockPorosity.HasValue)
                throw new PACInputDataException(
                    "Rock Porosity is required for calculations",
                    "RockPorosity");
            // ----------------------------------------

            if (Configuration.ConfigBCEF.HasFlag(cfg))
            {
                if (!bgDta.RockBottomArea.HasValue)
                    throw new PACInputDataException(
                        "Rock Bottom Area is required for calculations",
                        "RockBottomArea");
                // ----------------------------------------
                if (!bgDta.RockWidth.HasValue)
                    throw new PACInputDataException(
                        "Rock Width is required for calculations",
                        "RockWidth");
            }
            // ----------------------------------------
            // ----------------------------------------
            if (!Configuration.ConfigCD.HasFlag(cfg)) return;
            // ----------------------------------------------
            // ----------------------------------------------
            if (!bgDta.UnderdrainHeight.HasValue)
                throw new PACInputDataException(
                    "Rock Storage Depth Below the Underdrain pipe is required for calculations",
                    "UnderdrainHeight");
            if (bgDta.Orifice.HasOrifice && !bgDta.Orifice.Diameter.HasValue)
                throw new PACInputDataException(
                    $"This facility is Configuration {cfg}, and " +
                    "has an Orifice, but no Orifice diameter has been specified.",
                    "Orifice.Diameter");
            if (!bgDta.Orifice.HasOrifice && bgDta.Orifice.Reason == OrificeReason.Null)
                throw new PACInputDataException(
                    $"This facility is Configuration {cfg}, and has no Orifice. " +
                    "In this case the user must specify the reason that No orifice was included.",
                    "Orifice.Reason");
        }
        public override StormResults CalculateResults(Catchment ctch, bool getTimestepDetails = true)
        {
            InitializeEngine(ctch);
            var fclty = ctch.Facility;
            var aT = fclty.GetType();
            var et = typeof(RectangularBasin);
            if (!(fclty is RectangularBasin))
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