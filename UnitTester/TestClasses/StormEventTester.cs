using System.Collections.Generic;
using NUnit.Framework;

namespace BES.SWMM.PAC
{
    [TestFixture]
    public class StormEventTester
    {
        #region DesignStorm Consts
        private const DesignStorm dswq = DesignStorm.WQ;
        private const DesignStorm ds2 = DesignStorm.TwoYear;
        private const DesignStorm ds5 = DesignStorm.FivYear;
        private const DesignStorm ds10 = DesignStorm.TenYear;
        private const DesignStorm ds25 = DesignStorm.TwntyFiv;
        #endregion DesignStorm Consts
        // -------------------------------------------------------------------

        [Test, TestCaseSource( typeof(StormEventTestData), 
             "CumulativeRunoffTestCases")]
        public void CumRunoffTester(DesignStorm ds,
            decimal impArea, decimal cn, decimal toc,
            decimal expCumRunOff)
        {
            var sevt = StormEvent.Make(ds, impArea, cn, toc);
            Assert.AreEqual(expCumRunOff, sevt.CumulativeRunOff,
                 "Total Volume in Cubic Ft");
        }

        // -----------------------------------------------------------------
        // -----------------------------------------------------------------
        // -----------------------------------------------------------------

        [Test, TestCaseSource(typeof(StormEventTestData), 
             "TotalVolumeTestCases")]
        public void TotalVolumeTester(DesignStorm ds,
            decimal impArea, decimal cn, decimal toc, decimal expcumRO)
        {
            var sevt = StormEvent.Make(ds, impArea, cn, toc);
            Assert.AreEqual(expcumRO, 2 * sevt.TotalCubicFeetVolume, "Cumulative Volume");
            // -------------------------------------------------------------
        }

        [Test]
        public void CommonValuesTester()
        {
            //var ctch = Catchment.Make("TestCatchMent");
            //var cV = CommonValues.Fill();
        }

    }

    public class StormEventTestData
    {
        #region DesignStorm Consts
        private const DesignStorm dswq = DesignStorm.WQ;
        private const DesignStorm ds2 = DesignStorm.TwoYear;
        private const DesignStorm ds5 = DesignStorm.FivYear;
        private const DesignStorm ds10 = DesignStorm.TenYear;
        private const DesignStorm ds25 = DesignStorm.TwntyFiv;
        #endregion DesignStorm Consts

        public static IEnumerable<TestCaseData> TotalVolumeTestCases
        {
            get
            {
                yield return new TestCaseData(dswq, 5000m, 79m, 5m, 259.33865737336057567442438304m);
                yield return new TestCaseData(dswq, 5000m, 98m, 11m, 1157.1578303320397437372923617m);
                yield return new TestCaseData(ds2, 5000m, 79m, 5m, 642.63868830175887863534675532m);
                yield return new TestCaseData(ds2, 5000m, 98m, 11m, 1809.4588153732815153927813154m);
                yield return new TestCaseData(ds5, 5000m, 79m, 5m, 929.9066438429678082766725422m);
                yield return new TestCaseData(ds5, 5000m, 98m, 11m, 2223.9153489510387519431490114m);
                yield return new TestCaseData(ds10, 5000m, 79m, 5m, 1240.5884751947825622232401889m);
                yield return new TestCaseData(ds10, 5000m, 98m, 11m, 2638.9921375193857388316151212m);
                yield return new TestCaseData(ds25, 5000m, 79m, 5m, 1502.0096716574987540936921546m);
                yield return new TestCaseData(ds25, 5000m, 98m, 11m, 2971.3423989575231719876416056m);
            }
        }
        public static IEnumerable<TestCaseData> CumulativeRunoffTestCases 
        {
            get
            {
                yield return new TestCaseData(dswq, 5000m, 79m, 5m, 0.3112063888480326908093092584m);
                yield return new TestCaseData(dswq, 5000m, 98m, 11m, 1.3885893963984476924847508346m);
                yield return new TestCaseData(ds2, 5000m, 79m, 5m, 0.7711664259621106543624161073m);
                yield return new TestCaseData(ds2, 5000m, 98m, 11m, 2.1713505784479378184713375799m);
                yield return new TestCaseData(ds5, 5000m, 79m, 5m, 1.115887972611561369932007051m);
                yield return new TestCaseData(ds5, 5000m, 98m, 11m, 2.6686984187412465023317788144m);
                yield return new TestCaseData(ds10, 5000m, 79m, 5m, 1.4887061702337390746678882271m);
                yield return new TestCaseData(ds10, 5000m, 98m, 11m, 3.1667905650232628865979381446m);
                yield return new TestCaseData(ds25, 5000m, 79m, 5m, 1.8024116059889985049124305851m);
                yield return new TestCaseData(ds25, 5000m, 98m, 11m, 3.5656108787490278063851699282m);

            }
        }
    }
}