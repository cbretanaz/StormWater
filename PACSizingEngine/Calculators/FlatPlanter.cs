namespace BES.SWMM.PAC
{
    public class FlatPlanterCalculator: Calculator
    {
        #region properties
        public FlatPlanter Flat { get; set; }
        #endregion properties

        #region ctor/Factories
        public FlatPlanterCalculator() { }
        public static FlatPlanterCalculator Default => new FlatPlanterCalculator();
        #endregion ctor/Factories

        #region virtual Overrides

        public override void CalculateAreaAndVolume(Facility flat, CommonValues cm)
        {
            var IsCfgE = flat.Configuration == cfgE;
            var agDta = flat.AboveGrade;
            var ba = agDta.BottomArea.Value;
            // ScAx100PER / ScAx75PER
            cm.SurfaceArea75 = cm.SurfaceArea100 = ba;

            // ScVx
            if (!agDta.OverflowHeight.HasValue)
                throw new PACInputDataException(
                    "OverflowHeight (height of overflow pipe opening), is required for calculations",
                    "OverflowHeight");
            cm.MaxSurfaceVolume = agDta.OverflowHgtFt * ba;
            
            if (!IsCfgE) return;
            // ScVxfirst/ ScAx75PERfirst [For Configuration E Scenarios]
            cm.OverflowESurfArea75 = ba;
            cm.OverflowESurfaceVolume = agDta.OverflowEHgtFt;
        }

        #region virtual Validation method Overrides
        protected override void ValidateAGInputData(AboveGradeProperties agDta, bool isCfgE = false)
        {
            if (!agDta.BottomArea.HasValue)
                throw new PACInputDataException(
                    "Bottom Area is required for calculations",
                    "BottomArea");
            // --------------------------------------
            if (!agDta.BottomWidth.HasValue)
                throw new PACInputDataException(
                    "Bottom Width is required for calculations",
                    "BottomWidth");
            // -------------------------------------
            if (isCfgE && !agDta.OverflowHeight.HasValue)
                throw new PACInputDataException(
                    "OverflowHeight, (height of first overflow pipe opening), " +
                    "is required for calculations",
                    "OverflowHeight");
            if(isCfgE) ValdiateConfigEAreaVolumeData(agDta);
        }
        protected override void ValidateBGInputData(BelowGradeProperties bgDta, Configuration cfg)
        {
            var IsCfgA = cfg == cfgA;
            var IsCfgD = cfg == cfgD;
            if (!IsCfgD && !bgDta.InfiltrationPercentage.HasValue)
                throw new PACInputDataException(
                    "Infiltration Percentage is required for calculations",
                    "InfiltrationPercentage");
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
            if (!Configuration.ConfigCD.HasFlag(cfg)) return;
            // -----------------------------------------------
            // -----------------------------------------------
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
        #endregion virtual Validation method Overrides

        public override StormResults CalculateResults(Catchment ctch, bool getTimestepDetails = true)
        {
            InitializeEngine(ctch);
            var fclty = ctch.Facility;
            var aT = fclty.GetType();
            var et = typeof(FlatPlanter);
            if (!(fclty is FlatPlanter))
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