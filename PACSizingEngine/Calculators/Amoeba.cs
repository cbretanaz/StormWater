namespace BES.SWMM.PAC
{
    public class AmoebaCalculator : Calculator
    {
        #region properties
        public AmoebaBasin Amoeba { get; set; }
        #endregion properties

        #region ctor/Factories
        public AmoebaCalculator() { }
        public static AmoebaCalculator Default => new AmoebaCalculator();
        #endregion ctor/Factories

        #region virtual Overrides
        public override void CalculateAreaAndVolume(Facility amoeba, CommonValues cm)
        {
            var IsCfgE = amoeba.Configuration == cfgE;
            var agDta = amoeba.AboveGrade;
            var ba = agDta.BottomArea.GetValueOrDefault(0m);
            var bp = agDta.BottomPerimeter.GetValueOrDefault(0m);
            var ss = agDta.SideSlope.GetValueOrDefault(0m);
            var oHgt = agDta.OverflowHgtFt;
            // --------------------------------------------------
            // --------------------------------------------------
            var refFac = bp * ss * oHgt;
            cm.SurfaceArea100 = ba + refFac;
            cm.SurfaceArea75 = ba + 0.75m * refFac;
            cm.MaxSurfaceVolume = (refFac / 2m + ba) * oHgt;
            if (!IsCfgE) return;
            // -------------------------------------
            var oEHgt = agDta.OverflowEHgtFt;
            var refFacE = bp * ss * oEHgt;
            // -------------------------------------
            cm.OverflowESurfArea75 = ba + 0.75m * refFacE;
            cm.OverflowESurfaceVolume = refFacE / 2m + ba * oEHgt;
        }
        protected override void ValidateAGInputData(
            AboveGradeProperties agDta, bool isCfgE = false)
        {
            if (!agDta.BottomArea.HasValue)
                throw new PACInputDataException(
                    "Bottom Area is required for calculations",
                    "Bottom Area");
            // --------------------------------------
            if ( !agDta.BottomPerimeter.HasValue)
                throw new PACInputDataException(
                    "BottomPerimeter is required for calculations",
                    "BottomPerimeter");
            // -------------------------------------
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
            var IsCfgA = cfg == Configuration.ConfigA;
            var IsCfgD = cfg == Configuration.ConfigD;
            var IsCfgF = cfg == Configuration.ConfigF;
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
                        "The height of the Underdrain pipe must be specified.",
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
            var et = typeof(AmoebaBasin);
            if (!(fclty is AmoebaBasin))
                throw new WrongFacilityException(aT, et,
                    $"Wrong Type Facility passed: Expected " +
                    $"{et.FullName}, but {aT.FullName} was passed.",
                    null);
            ValidateInputData(ctch);
            var cmVals = CalculateCommonValues(ctch);
            var rslts = IterateTimeSteps(ctch, cmVals, getTimestepDetails);
            return DeterminePassFail(ctch, rslts);
        }

        #endregion virtual Overrides
    }
}