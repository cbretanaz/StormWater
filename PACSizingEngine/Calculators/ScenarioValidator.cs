using System;
using System.Linq;
using lib = CoP.Enterprise.Utilities;
using msg = CoP.Enterprise.Message;

namespace BES.SWMM.PAC
{
    public class Validator : Constants, IValidateScenarioData
    {
        public void ValidateScenario(object sender, ValidatorEventArgs e)
        { ValidateScenarioData(e.Scenario); }

        #region General Statics & Consts

        private static readonly string sNL = Environment.NewLine;
        private static readonly StringComparison icic = StringComparison.InvariantCultureIgnoreCase;

        #endregion General Statics & Consts

        #region ctor/Factories
        public static Validator Default => new Validator();

        #endregion ctor/Factories

        public void ValidateScenarioData(Scenario scen)
        {
            var ctch = scen.Catchment;
            var fclty = ctch.Facility;
            var cfg = fclty.Configuration;
            var agD = fclty.AboveGrade;
            var bgD = fclty.BelowGrade;
            var needsOrf = fclty.NeedsOrifice;
            var hasUnderDrn = cfgCDF.HasFlag(cfg);
            var orf = bgD.Orifice;
            // -------------------------------
            if (fclty is SlopedFacility slpd)
            {
                if (fclty.Shape != Shape.Null)
                    msg.Warn($"Sloped facilities must be rectangular.{sNL}" +
                             $"They may not have a defined shape.{sNL}" +
                             "Please edit the xml file to remove this attribute.");
                var segs = slpd.Segments;
                var totalLength = segs.Sum(s => s.Length);
                if (segs.Count < 1)
                    throw ValidationException.Make(
                        "Sloped facilities must have at least one segment.");

                if (segs.Sum(s => s.CheckDamWidth) > totalLength)
                    throw ValidationException.Make(
                        "For sloped facilities, the sum of all CheckDam widths " +
                        "must be less than the sum of all segment lengths.");

                if (segs.Any(s => s.LongitudinalSlope > 0.2m))
                    throw ValidationException.Make(
                        $"One or more segments has a longitudinal slope greater than 20 % (0.2).{sNL}" +
                        "Please modify the longitudinal slope before proceeding.");

                if (segs.Any(s => s.LandscapeWidth < slpd.AboveGrade.BottomWidth))
                    throw ValidationException.Make(
                        "For sloped facilities, the Landscape Width of every " +
                        "Segment must be at least as wide as the Facility Bottom width.");

                var segsNotEnoughSpace
                    = segs.Where(s => s.LandscapeWidth <
                                      s.BottomWidth * s.SumOfSlopes * s.DownStreamDepthFt);
                var spCount = segsNotEnoughSpace.Count();

                if (spCount > 0)
                    throw ValidationException.Make(
                        $"Not enough space for {spCount} segments in the facility. You will need {sNL}" +
                        $"to revise landscape width or downstream depth for {spCount} segments.");

                var segs2Stp = segs
                    .Where(s => s.LeftSlope < 2m && s.LeftSlope != 0m ||
                                s.RightSlope < 2m && s.RightSlope != 0m);
                var cntStp = segs2Stp.Count();
                if (cntStp > 0)
                    throw ValidationException.Make(
                        $"There {(cntStp > 1 ? "are" : "is")} {cntStp} segment{(cntStp > 1 ? "s" : "")} which " +
                        $"{(cntStp > 1 ? "have" : "has")} Sloped sides which exceed the maximum allowable slope.{sNL}" +
                        "Please revise the side slope values to be vertical (slope = zero), or at least 2.0.");
            }

            if (fclty is AmoebaBasin ambBas && ambBas.AboveGrade.BottomPerimeter <
                2m * (decimal) Math.Sqrt(Math.PI * (double) ambBas.AboveGrade.BottomArea.Value))
                throw ValidationException.Make(DataControl.BP,
                    "For Amoeba Basin facilities, the perimeter of the basin " +
                    "must be large enough to contain the specified bottom area.");

            if (!(fclty is FlatPlanter) && !(fclty is SlopedFacility))
            {
                var expShp =
                    fclty is AmoebaBasin ? Shape.Amoeba :
                    fclty is RectangularBasin ? Shape.Rectangular :
                    fclty is UserDefinedBasin ? Shape.UserDefined : Shape.Null;

                if (fclty.Shape != expShp)
                    throw ValidationException.Make(
                        $"{fclty.Category} facilities must have {expShp} Shape.{sNL}" +
                        $"They cannot be defined as {fclty.Shape}.{sNL}" +
                        "Please edit the xml file to remove this attribute and reload.");

                if (agD.SideSlope < 2m)
                    throw ValidationException.Make(DataControl.SS,
                        "Side-slope, (measured as run in feet / rise in feet), " +
                        "must be at least 2 ft per foot.");
            }

            if (needsOrf)
            {
                if (orf.HasOrifice && !orf.Diameter.HasValue)
                    throw ValidationException.Make(DataControl.ORD,
                        "If the facility has an Orifice, the diameter must be specified.");

                if (!orf.HasOrifice && !OrificeReason.Serialize.HasFlag(orf.Reason))
                    throw ValidationException.Make(DataControl.ORR,
                        "If the facility does not have an Orifice, you must specify why.");
            }

            if (cfg != cfgA) // hasRock
            {
                var thrshold =
                    fclty is FlatPlanter ? agD.BottomArea / 3m :
                        fclty is SlopedFacility slped ?
                            lib.Minimum(slped.FinalSegment.Area,
                                slped.Segments.Sum(s => s.Area) / 3m) :
                            (agD.BottomArea + 2m * agD.OverflowHeight * agD.SideSlope *
                                (decimal) Math.Sqrt(Math.PI * (double) agD.BottomArea)) / 3m;
                if (bgD.RockBottomArea > thrshold)
                    throw ValidationException.Make(DataControl.ABW,
                        $"The rock storage bottom area cannot be greater than the{sNL}" +
                        $"effective bottom area of the Blended Soil Media, ({thrshold: 0.0 sqft}).");

                if (!bgD.RockPorosity.HasValue)
                    throw ValidationException.Make(DataControl.RP,
                        $"Rock Porosity must be specified for Configurations B through F.");

                if (bgD.RockPorosity.Value < 0.01m ||
                    bgD.RockPorosity.Value > 1.0m)
                    throw ValidationException.Make(DataControl.RP,
                        "Rock Porosity must be between 0.01 and 1.0.");

                if (bgD.RockStorageDepth > fclty.BlendedSoilDepth - 6)
                    throw ValidationException.Make(DataControl.RSD,
                        "For Configurations B through F, Rock Storage Depth " +
                        "must no more than six inches less than the Blended Soil Depth.");

                if (cfg == cfgB && bgD.RockStorageDepth < 1m)
                    throw ValidationException.Make(DataControl.RSD,
                        "For Configuration B, Rock Storage Depth must be at least one inch.");

                if (bgD.RockBottomArea > agD.BottomArea / 3m)
                    throw ValidationException.Make(DataControl.RBA,
                        "Rock Bottom Area may not exceed one third of the Facility Bottom Area.");

                if (bgD.RockStorageDepth > 30m) 
                    throw ValidationException.Make(DataControl.RSD,
                        "Rock Storage Depth may not exceed 30 inches.");

                if (hasUnderDrn && bgD.UnderdrainHeight > bgD.RockStorageDepth - 6m)
                    throw ValidationException.Make(DataControl.UDH,
                        "The under drain height must be at least 6 inches below " +
                        "the rock, (rock storage depth).");

                //if (bgD.RockBottomArea > agD.BottomArea &&
                //    !msg.YN(
                //        $"Rock Bottom Area should generally not be greater than the overall facility bottom area.{sNL}" +
                //        "      .....  Are you sure you want to do this ?", msgTitl))
                //{
                //    frm.SetFocus("nudRockBottomArea");
                //    return false;
                //}
            }

            if (hasUnderDrn && bgD.UnderdrainHeight < 0m)
                throw ValidationException.Make(DataControl.UDH,
                    "The under drain height must be a positive number.");

            if (fclty.BlendedSoilDepth > (cfg == cfgA ? 12 : 30))
                throw ValidationException.Make(DataControl.BSD,
                    "For Configuration {cfg}, the Blended Soil Depth may not be greater than " +
                    $"{(cfg == cfgA ? 12 : 30): 0 inches}.");

            if (ctch.InfiltrationRate < 0m)
                throw ValidationException.Make(DataControl.IFR,
                    "Infiltration Rate must be positive number.");

            if (ctch.ImperviousArea <= 0m)
                throw ValidationException.Make(DataControl.IMPA,
                    "Impervious Area must be positive number.");

            if (ctch.PreCurveNumber < 65m ||
                ctch.PreCurveNumber > 81m)
                throw ValidationException.Make(DataControl.PreCN,
                    "Pre-Development Curve Number must be between 65 and 81.");

            if (ctch.PostCurveNumber != 98m &&
                ctch.PostCurveNumber != 99m)
                throw ValidationException.Make(DataControl.PstCN,
                    "Post-Development Curve Number must be either 98 or 99.");

            if (ctch.PreTOC < 5m)
                throw ValidationException.Make(DataControl.PreTC,
                    "Pre-Development Time of concentration must be at least 5 minutes.");

            if (ctch.PostTOC > ctch.PreTOC)
                throw ValidationException.Make(DataControl.PstTC,
                    "Post-Development Time of Concentration may not be larger as the Pre-Development TOC.");

            if (ctch.HierarchyLevel == HierarchyLevel.One &&
                Configuration.ConfigCDF.HasFlag(cfg))
                throw ValidationException.Make(DataControl.CFG,
                    "Cannot use configuration C, D or F if the Hierarchy Level is ONE");


            if (fclty is UserDefinedBasin)
            {
                if (agD.OverflowSurfaceArea < agD.BottomArea)
                    throw ValidationException.Make(DataControl.OFA,
                        "For User defined basins, the surface area at the overflow " +
                        "must be at least as large as the Bottom Area.");

                if (agD.OverflowSurfaceArea == agD.BottomArea)
                    throw ValidationException.Make(DataControl.OFA,
                        $"For User defined basins, The surface area at the overflow{sNL}" +
                        $"must be at least as large as the Bottom Area If it is the same, then the{sNL}" +
                        "facility category 'Flat Planter' should be selected rather than UserDefinedBasin.");

                if (!Configuration.ConfigAB.HasFlag(cfg) &&
                    bgD.RockStorageDepth < 6m)
                    throw ValidationException.Make(DataControl.RSD,
                        "For Configurations C through F, Rock Storage Depth must be at least six inches.");
            }
        }
    }
}