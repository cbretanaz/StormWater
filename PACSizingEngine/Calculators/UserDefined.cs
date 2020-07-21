namespace BES.SWMM.PAC
{
    public class UserDefinedCalculator : Calculator
    {
        #region properties
        public UserDefinedBasin User { get; set; }
        #endregion properties

        #region ctor/Factories
        public UserDefinedCalculator() { }
        public static UserDefinedCalculator Default => new UserDefinedCalculator();
        #endregion ctor/Factories

        #region virtual Overrides
        public override void CalculateAreaAndVolume(Facility udf, CommonValues cm)
        {
            var IsCfgE = udf.Configuration == cfgE;
            var agDta = udf.AboveGrade;
            var ba = agDta.BottomArea.Value;
            var oSA = agDta.OverflowSurfaceArea.Value;
            var oHgt = agDta.OverflowHgtFt;
            // -------------------------------------
            // -------------------------------------
            cm.SurfaceArea75 = ba / 4m + 0.75m * oSA;
            cm.SurfaceArea100 = oSA;
            cm.MaxSurfaceVolume = 0.5m * oHgt * (ba + oSA);
            // ---------------------------------------------

            if (!IsCfgE) return;
            var oESA = agDta.OverflowESurfaceArea.Value;
            var oEHgt = agDta.OverflowEHgtFt;
            // ---------------------------------------------
            cm.OverflowESurfArea75 = 0.25m * ba + 0.75m * oESA;
            cm.OverflowESurfaceVolume = 0.5m * oEHgt * (ba + oESA);
        }
        protected override void ValidateAGInputData(
            AboveGradeProperties agDta, bool isCfgE = false)
        {
            if (!agDta.BottomArea.HasValue)
                throw new PACInputDataException(
                    "Bottom Area is required for calculations.",
                    "Bottom Area");
            // --------------------------------------
            if (!agDta.OverflowHeight.HasValue)
                throw new PACInputDataException(
                    "OverflowHeight, (height of first overflow pipe opening), " +
                    "is required for calculations. ",
                    "OverflowHeight");
            // -------------------------------------
            if (!agDta.OverflowSurfaceArea.HasValue)
                throw new PACInputDataException(
                    "Overflow Surface Area at Overflow pipe is required for calculations.",
                    "OverflowSurfaceArea");
            // -------------------------------------
            if (isCfgE) ValdiateConfigEAreaVolumeData(agDta, true);
        }
        protected override void ValidateBGInputData(BelowGradeProperties bgDta, Configuration cfg)
        {
            var IsCfgA = cfg == cfgA;
            var IsCfgD = cfg == cfgD;
            var IsCfgF = cfg == cfgF;
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
            if (Configuration.ConfigCD.HasFlag(cfg))
            {
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
            if (IsCfgF && !bgDta.UnderdrainHeight.HasValue)
                throw new PACInputDataException(
                    "Rock Storage Depth below the Underdrain pipe is required for calculations",
                    "UnderdrainHeight");
        }
        public override StormResults CalculateResults(Catchment ctch, bool getTimestepDetails = true)
        {
            InitializeEngine(ctch);
            var fclty = ctch.Facility;
            var aT = fclty.GetType();
            var et = typeof(UserDefinedBasin);
            if (!(fclty is UserDefinedBasin))
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