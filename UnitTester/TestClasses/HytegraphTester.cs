using System.Collections.Generic;
using NUnit.Framework;

namespace BES.SWMM.PAC
{
    [TestFixture]
    public class HyetgraphTester
    {
        private const decimal ep = 1e-5m;
        [Test, TestCaseSource(typeof(HydroGraphTestData), "IntervalPercentageTestCases")]
        public void TestIntervalPercentage(int ti, decimal expRslt)
        {
            var actRslt = Hyetograph.RainfallPercentage(ti);
            Assert.AreEqual(expRslt, actRslt);
        }
        [Test, TestCaseSource(typeof(HydroGraphTestData), "CumulativePercentageTestCases")]
        public void TestCumPercentage(int ti, double expRslt)
        {
            var actRslt = Hyetograph.CumulativePercentage(ti);
            Assert.AreEqual(expRslt, actRslt);
        }
        [Test, TestCaseSource(typeof(HydroGraphTestData), "RainfallTestCases")]
        public void TestCumRainfall(DesignStorm ds, int ti, double expRslt)
        {
            var rainFall = Hyetograph.CumulativeRainfall(ds, ti);
            Assert.AreEqual(expRslt, rainFall);
        }
    }

    public class HydroGraphTestData: Constants
    {
        public static IEnumerable<TestCaseData> IntervalPercentageTestCases
        {
            get
            {
                yield return new TestCaseData(5, 0.004m);
                yield return new TestCaseData(10, 0.004m);
                yield return new TestCaseData(12, 0.005m);
                yield return new TestCaseData(15, 0.005m);
                yield return new TestCaseData(16, 0.005m);
                yield return new TestCaseData(17, 0.006m);
                yield return new TestCaseData(21, 0.006m);
                yield return new TestCaseData(22, 0.006m);
                yield return new TestCaseData(27, 0.007m);
                yield return new TestCaseData(28, 0.007m);
                yield return new TestCaseData(29, 0.0082m);
                yield return new TestCaseData(34, 0.0082m);
                yield return new TestCaseData(35, 0.0095m);
                yield return new TestCaseData(40, 0.0095m);
                yield return new TestCaseData(41, 0.0134m);
                yield return new TestCaseData(43, 0.0134m);
                yield return new TestCaseData(45, 0.018m);
                yield return new TestCaseData(46, 0.034m);
                yield return new TestCaseData(47, 0.054m);
                yield return new TestCaseData(49, 0.018m);
                yield return new TestCaseData(50, 0.0134m);
                yield return new TestCaseData(52, 0.0134m);
                yield return new TestCaseData(53, 0.0088m);
                yield return new TestCaseData(64, 0.0088m);
                yield return new TestCaseData(65, 0.0072m);
                yield return new TestCaseData(76, 0.0072m);
                yield return new TestCaseData(77, 0.0057m);
                yield return new TestCaseData(88, 0.0057m);
                yield return new TestCaseData(89, 0.005m);
                yield return new TestCaseData(99, 0.005m);
                yield return new TestCaseData(100, 0.005m);
                yield return new TestCaseData(119, 0.004m);
                yield return new TestCaseData(143, 0.004m);
                yield return new TestCaseData(144, 0.004m);
                yield return new TestCaseData(145, 0.00m);
                yield return new TestCaseData(160, 0.00m);
                yield return new TestCaseData(190, 0.00m);
                yield return new TestCaseData(220, 0.00m);
                yield return new TestCaseData(250, 0.00m);
                yield return new TestCaseData(300, 0.00m);
                yield return new TestCaseData(350, 0.00m);
                yield return new TestCaseData(400, 0.00m);
            }
        }
        public static IEnumerable<TestCaseData> CumulativePercentageTestCases
        {
            get
            {
                yield return new TestCaseData(5, 0.02d);
                yield return new TestCaseData(10, 0.04d);
                yield return new TestCaseData(11, 0.045d);
                yield return new TestCaseData(12, 0.05d);
                yield return new TestCaseData(13, 0.055d);
                yield return new TestCaseData(15, 0.065d);
                yield return new TestCaseData(16, 0.07d);
                yield return new TestCaseData(17, 0.076d);
                yield return new TestCaseData(21, 0.1d);
                yield return new TestCaseData(22, 0.106d);
                yield return new TestCaseData(27, 0.141d);
                yield return new TestCaseData(28, 0.148d);
                yield return new TestCaseData(33, 0.189d);
                yield return new TestCaseData(34, 0.1972d);
                yield return new TestCaseData(39, 0.2447d);
                yield return new TestCaseData(40, 0.2542d);
                yield return new TestCaseData(42, 0.281d);
                yield return new TestCaseData(43, 0.2944d);
                yield return new TestCaseData(45, 0.3304d);
                yield return new TestCaseData(46, 0.3644d);
                yield return new TestCaseData(47, 0.4184d);
                yield return new TestCaseData(48, 0.4454d);
                yield return new TestCaseData(49, 0.4634d);
                yield return new TestCaseData(51, 0.4902d);
                yield return new TestCaseData(52, 0.5036d);
                yield return new TestCaseData(63, 0.6004d);
                yield return new TestCaseData(64, 0.6092d);
                yield return new TestCaseData(75, 0.6884d);
                yield return new TestCaseData(76, 0.6956d);
                yield return new TestCaseData(87, 0.7583d);
                yield return new TestCaseData(88, 0.764d);
                yield return new TestCaseData(99, 0.819d);
                yield return new TestCaseData(100, 0.824d);
                yield return new TestCaseData(119, 0.9d);
                yield return new TestCaseData(143, .996d);
                yield return new TestCaseData(144, 1.0d);
                yield return new TestCaseData(145, 1.0d);
                yield return new TestCaseData(150, 1.0d);
                yield return new TestCaseData(175, 1.0d);
                yield return new TestCaseData(200, 1.0d);
                yield return new TestCaseData(250, 1.0d);
                yield return new TestCaseData(300, 1.0d);
                yield return new TestCaseData(350, 1.0d);
                yield return new TestCaseData(400, 1.0d);
            }
        }
        public static IEnumerable<TestCaseData> RainfallTestCases
        {
            get
            {
                yield return new TestCaseData(dswq, 1, 0.00644);
                yield return new TestCaseData(ds2, 1, 0.0096d);
                yield return new TestCaseData(dsH2, 1, 0.0096d);
                yield return new TestCaseData(ds5, 1, 0.0116d);
                yield return new TestCaseData(ds10, 1, 0.0136d);
                yield return new TestCaseData(ds25, 1, 0.0152d);
                // -------------------------------------------
                yield return new TestCaseData(dswq, 10, 0.0644d);
                yield return new TestCaseData(ds2, 10, 0.096d);
                yield return new TestCaseData(dsH2, 10, 0.096d);
                yield return new TestCaseData(ds5, 10, 0.116d);
                yield return new TestCaseData(ds10, 10, 0.136d);
                yield return new TestCaseData(ds25, 10, 0.152d);
                // -------------------------------------------
                yield return new TestCaseData(dswq, 15, 0.10465d);
                yield return new TestCaseData(ds2, 15, 0.156d);
                yield return new TestCaseData(dsH2, 15, 0.156d);
                yield return new TestCaseData(ds5, 15, 0.1885d);
                yield return new TestCaseData(ds10, 15, 0.221d);
                yield return new TestCaseData(ds25, 15, 0.247d);
                // -------------------------------------------
                yield return new TestCaseData(dswq, 46, 0.586684d);
                yield return new TestCaseData(ds2, 46, 0.87456d);
                yield return new TestCaseData(dsH2, 46, 0.87456d);
                yield return new TestCaseData(ds5, 46, 1.05676d);
                yield return new TestCaseData(ds10, 46, 1.23896d);
                yield return new TestCaseData(ds25, 46, 1.38472d);
                // -------------------------------------------
                yield return new TestCaseData(dswq, 77, 1.129093d);
                yield return new TestCaseData(ds2, 77, 1.68312d);
                yield return new TestCaseData(ds5, 77, 2.03377d);
                yield return new TestCaseData(ds10, 77, 2.38442d);
                yield return new TestCaseData(ds25, 77, 2.66494d);
                // -------------------------------------------
                yield return new TestCaseData(dswq, 115, 1.42324d);
                yield return new TestCaseData(ds2, 115, 2.1216d);
                yield return new TestCaseData(ds5, 115, 2.5636d);
                yield return new TestCaseData(ds10, 115, 3.0056d);
                yield return new TestCaseData(ds25, 115, 3.3592d);
                // -------------------------------------------
                yield return new TestCaseData(dswq, 133, 1.53916d);
                yield return new TestCaseData(ds2, 133, 2.2944d);
                yield return new TestCaseData(ds5, 133, 2.7724d);
                yield return new TestCaseData(ds10, 133, 3.2504d);
                yield return new TestCaseData(ds25, 133, 3.6328d);
                // -------------------------------------------
                yield return new TestCaseData(dswq, 141, 1.59068d);
                yield return new TestCaseData(ds2, 141, 2.3712d);
                yield return new TestCaseData(ds5, 141, 2.8652d);
                yield return new TestCaseData(ds10, 141, 3.3592d);
                yield return new TestCaseData(ds25, 141, 3.7544d);
                // -------------------------------------------
                foreach (var ds in new Dictionary<DesignStorm, double>
                    { {dswq, 1.61d}, {ds2, 2.4d}, {ds5, 2.9d}, {ds10, 3.4d}, {ds25, 3.8d} })
                    for(var ts = 144; ts <= 400; ts+=5)
                        yield return new TestCaseData(ds.Key, ts, ds.Value);
            }
        }
    }
}