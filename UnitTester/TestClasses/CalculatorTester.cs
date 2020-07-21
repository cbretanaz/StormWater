using System.IO;
using System.Xml.Serialization;
using NUnit.Framework;
using lib = CoP.Enterprise.Utilities;

namespace BES.SWMM.PAC
{
    public class CalculatorTester
    {
        private static readonly double dlta = 1e-6d;
        private static readonly string folder = lib.ExtractPath(
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
        [TestCase("Sloped.A")]
        [TestCase("Sloped.A2")]
        [TestCase("Sloped.B")]
        [TestCase("Sloped.C.Orf")]
        [TestCase("Sloped.D.Orf")]
        [TestCase("Sloped.E")]
        [TestCase("Sloped.F")]
        // -----------------------
        [TestCase("FlatPlanter.A")]
        [TestCase("FlatPlanter.B")]
        [TestCase("FlatPlanter.C.NoOrf")]
        [TestCase("FlatPlanter.D.NoOrf")]
        [TestCase("FlatPlanter.E")]
        [TestCase("FlatPlanter.F")]
        [TestCase("FlatPlanter.C.Orf")]
        [TestCase("FlatPlanter.D.Orf")]
        // ----------------------------
        [TestCase("RectBasin.A")]
        [TestCase("RectBasin.B")]
        [TestCase("RectBasin.C.1kNoOrf")]
        [TestCase("RectBasin.D.NoOrf")]
        [TestCase("RectBasin.D.Orf")]
        [TestCase("RectBasin.E")]
        [TestCase("RectBasin.F")]
        [TestCase("RectBasin.C.Orf.1k")]
        [TestCase("RectBasin.C.Orf.500")]
        // -----------------------------
        [TestCase("Amoeba.A")]
        [TestCase("Amoeba.B")]
        [TestCase("Amoeba.C")]
        [TestCase("Amoeba.D")]
        [TestCase("Amoeba.E")]
        [TestCase("Amoeba.F")]
        // ----------------------
        [TestCase("UserBasin.A")]
        [TestCase("UserBasin.B")]
        [TestCase("UserBasin.C.NoOrf")]
        [TestCase("UserBasin.D.NoOrf")]
        [TestCase("UserBasin.E")]
        [TestCase("UserBasin.E2")]
        [TestCase("UserBasin.F")]
        [TestCase("UserBasin.C.Orf")]
        [TestCase("UserBasin.D.Orf")]
        // --------------------------
        public void TestCalculator(string filName)
        {
            var filSpec = $@"{folder}\TestCases\{filName}.xml";
            var serializer = new XmlSerializer(typeof(Scenario));
            using (var filStrm = new FileStream(filSpec, FileMode.Open))
            {
                var scenario = (Scenario)serializer.Deserialize(filStrm);
                var ctch = scenario.Catchment;
                // -------------------------------------------
                var reslts = 
                    Calculator.Make(ctch.Facility.Category)
                        .CalculateResults(ctch,false);
                var expRslts = scenario.ExpectedResult.StormResults;
                foreach (var actRslt in reslts)
                    CheckStormResultData(
                        expRslts[actRslt.DesignStorm],
                        actRslt);
            }
        }

        #region Helper methods
        private static void CheckStormResultData(StormResult exp, StormResult act)
        {
            Assert.AreEqual(exp.PeakHead, act.PeakHead,
                $"Peak Head [{act.PeakHead: 0.0 ft}], different " +
                $"from expected value [{exp.PeakHead: 0.0 ft}].");
            Assert.AreEqual(exp.PeakInflow, act.PeakInflow,
                $"Peak Inflow [{act.PeakInflow: 0.0 cfs}], different " +
                $"from expected value [{exp.PeakInflow: 0.0 cfs}].");
            Assert.AreEqual(exp.PeakOutflow, act.PeakOutflow,
                $"Peak Outflow [{act.PeakOutflow: 0.0 cfs}], different " +
                $"from expected value [{exp.PeakOutflow: 0.0 cfs}].");
            Assert.AreEqual(exp.PeakTotalOverflow, act.PeakTotalOverflow,
                $"Peak TotalOverflow [{act.PeakTotalOverflow: 0.0 cfs}], different " +
                $"from expected value [{exp.PeakTotalOverflow: 0.0 cfs}].");
            Assert.AreEqual(exp.PeakUnderdrain, act.PeakUnderdrain,
                $"Peak Underdrain [{act.PeakUnderdrain: 0.0 cfs}], different " +
                $"from expected value [{exp.PeakUnderdrain: 0.0 cfs}].");
            Assert.AreEqual(exp.TotalCombinedOverflow, act.TotalCombinedOverflow,
                $"Total Combined Overflow [{act.TotalCombinedOverflow: 0.0 cfs}], different " +
                $"from expected value [{exp.TotalCombinedOverflow: 0.0 cfs}].");
            Assert.AreEqual(exp.TotalInflow, act.TotalInflow,
                $"Total Inflow [{act.TotalInflow: 0.0 cfs}], different " +
                $"from expected value [{exp.TotalInflow: 0.0 cfs}].");
            Assert.AreEqual(exp.TotalOverflow, act.TotalOverflow,
                $"Total Overflow [{act.TotalOverflow: 0.0 cfs}], different " +
                $"from expected value [{exp.TotalOverflow: 0.0 cfs}].");
            Assert.AreEqual(exp.TotalSoilOutflow, act.TotalSoilOutflow,
                $"Total Soil Outflow [{act.TotalSoilOutflow: 0.0 cfs}], different " +
                $"from expected value [{exp.TotalSoilOutflow: 0.0 cfs}].");
            Assert.AreEqual(exp.TotalUnderdrainOutflow, act.TotalUnderdrainOutflow,
                $"Total Underdrain Outflow [{act.TotalUnderdrainOutflow: 0.0 cfs}], different " +
                $"from expected value [{exp.TotalUnderdrainOutflow: 0.0 cfs}].");
        }
        #endregion Helper methods
    }
}
