using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using NUnit.Framework;
using sd = CoP.Enterprise.SerializableDictionary<int, decimal>;

namespace BES.SWMM.PAC
{
    public class XmlSerializationFacilitytester
    {
        [Test]
        public void TestSlopedFacilitySerialization()
        {
            using (var sw = new StringWriter())
                using (var xwrtr = XmlWriter.Create(sw))
            {
                var ser = new XmlSerializer(typeof(Catchment));
                ser.Serialize(xwrtr, MakeCatchment());
                var s = sw.ToString();
                Assert.IsNotNull(s);
            }
        }


        [Test]
        public void TestFlatPlanterSerialization()
        {
            var c = Catchment.Make("TestFlatPlanter",
                InfiltrationTestProcedure.OpenPit,
                PRStatus.Fail,3.56m, 
                HierarchyLevel.DischargeToRiverSlough,
                23456, 45, 56, 6, 8,
                FlatPlanter.Make(true, 4.89m, Configuration.ConfigB,
                    AboveGradeProperties.Make(234m, 8.9m,
                        70m, 0.23m, 6.7m, 5.34m,
                        4.5m, 9.0m, 7.6m),
                    BelowGradeProperties.Make(0.56m,87.5m,
                        5.2m, 8.7m, 6m, .87m,
                        Orifice.Make(1.6m))));
            using (var sw = new StringWriter())
            using (var xwrtr = XmlWriter.Create(sw))
            {
                var ser = new XmlSerializer(typeof(Catchment));
                ser.Serialize(xwrtr, c);
                var s = sw.ToString();
                Assert.IsNotNull(s);
            }
        }

        [Test]
        public void TestFlatPlanterDeSerialization()
        {
            var s = MakeCatchmentString();
            var ser = new XmlSerializer(typeof(Catchment));

            var sRdr = new StringReader(s);
            var ctchment = (Catchment)ser.Deserialize(sRdr);
            sRdr.Close();

            Assert.IsNotNull(ctchment);
        }

        [Test]
        public void TestRectangularBasinSerialization()
        {
            var c = Catchment.Make("TestRectBasin",
                InfiltrationTestProcedure.OpenPit,
                PRStatus.Fail, 3.56m, 
                HierarchyLevel.DischargeToRiverSlough,
                23456, 45, 56, 6, 8,
                RectangularBasin.Make(true, 6.2m, 
                    Configuration.ConfigA,
                    AboveGradeProperties.Make( 234m, 8.9m,
                        70m, 0.23m, 6.7m, 5.34m,
                        4.5m, 9.0m, 7.6m),
                    BelowGradeProperties.Make(0.89m,87.5m,
                        6m, 5.4m, .6m, 0.45m,
                        Orifice.Make(1.6m))));
            using (var sw = new StringWriter())
            using (var xwrtr = XmlWriter.Create(sw))
            {
                var ser = new XmlSerializer(typeof(Catchment));
                ser.Serialize(xwrtr, c);
                var s = sw.ToString();
                Assert.IsNotNull(s);
            }
        }
        [Test]
        public void TestAmoebaBasinSerialization()
        {
            var c = Catchment.Make("TestAmoeba",
                InfiltrationTestProcedure.OpenPit,
                PRStatus.Fail, 3.56m, 
                HierarchyLevel.DischargeToRiverSlough,
                23456, 45, 56, 6, 8,
                AmoebaBasin.Make(true, Configuration.ConfigA, 6.2m,
                    AboveGradeProperties.Make(234m, 8.9m,
                        70m, 0.23m, 6.7m, 5.34m,
                        4.5m, 9.0m, 7.6m),
                    BelowGradeProperties.Make(0.45m, 87.5m,
                        5.2m, 4m, .46m, 0.77m,
                        Orifice.Make(1.6m))));
            using (var sw = new StringWriter())
            using (var xwrtr = XmlWriter.Create(sw))
            {
                var ser = new XmlSerializer(typeof(Catchment));
                ser.Serialize(xwrtr, c);
                var s = sw.ToString();
                Assert.IsNotNull(s);
            }
        }
        [Test]
        public void TestUserDefinedBasinSerialization()
        {
            var c = Catchment.Make("TestUserDefined",
                InfiltrationTestProcedure.OpenPit,
                PRStatus.Fail,3.56m, 
                HierarchyLevel.DischargeToRiverSlough,
                23456, 45, 56, 6, 8,
                UserDefinedBasin.Make(true, Configuration.ConfigA, 6.2m,
                    AboveGradeProperties.Make(234m, 8.9m,
                        70m, 0.23m, 6.7m, 5.34m,
                        4.5m, 9.0m, 7.6m),
                    BelowGradeProperties.Make(0.76m,87.5m,
                        5.2m, 8.2m, 6m, 0.62m, 
                        Orifice.Make(1.6m))));
            using (var sw = new StringWriter())
            using (var xwrtr = XmlWriter.Create(sw))
            {
                var ser = new XmlSerializer(typeof(Catchment));
                ser.Serialize(xwrtr, c);
                var s = sw.ToString();
                Assert.IsNotNull(s);
            }
        }

        #region CreateCatchments
        private Catchment MakeCatchment()
        {
            var segs = Segments.Empty;
            segs.Add(Segment.Make(1,
                12.7m, 7.5m, 0.25m,
                5.4m, 0.6m, 0.63m,
                13.5m, 11.7m));
            segs.Add(Segment.Make(2, 
                42.1m, 5.6m, 0.25m,
                8.2m, 0.45m, 0.35m,
                10.9m, 11.7m));
            return Catchment.Make("TestSlopedFacility",
                InfiltrationTestProcedure.OpenPit,
                PRStatus.Fail, 3.56m,
                HierarchyLevel.DischargeToRiverSlough,
                23456, 45, 56, 6, 8,
                SlopedFacility.Make(true,
                    Configuration.ConfigA, 5.2m,
                    AboveGradeProperties.Make(222m, 5.3m,
                        187m, 0.9m, 7.2m, 4.5m,
                        287m, 5.4m, 87m),
                    BelowGradeProperties.Make(56, 87.5m,
                        11.2m, 6.9m, 5.5m, 0.23m,
                        Orifice.Make(1.6m)),
                    segs));
        }
        private string MakeCatchmentString()
        {
            var segs = Segments.Empty;
            segs.AddSegment(Segment.Make(
                12.7m, 7.5m, 0.25m,
                5.4m, 0.6m, 0.63m,
                13.5m, 11.7m));
            segs.AddSegment(Segment.Make(
                42.1m, 5.6m, 0.25m,
                8.2m, 0.45m, 0.35m,
                10.9m, 11.7m));

            var c = Catchment.Make("TestSlopedFacility",
                InfiltrationTestProcedure.OpenPit,
                PRStatus.Fail, 3.56m,
                HierarchyLevel.DischargeToRiverSlough,
                23456, 45, 56, 6, 8,
                SlopedFacility.Make(true,
                    Configuration.ConfigA, 5.2m,
                    AboveGradeProperties.Make( 222m, 5.3m,
                        187m, 0.9m, 7.2m, 4.5m,
                        287m, 5.4m, 87m),
                    BelowGradeProperties.Make(56,87.5m,
                        11.2m, 6.9m, 5.5m, 0.23m,
                        Orifice.Make(1.6m)),
                    segs));
            using (var sw = new StringWriter())
            using (var xwrtr = XmlWriter.Create(sw))
            {
                var ser = new XmlSerializer(typeof(Catchment));
                ser.Serialize(xwrtr, c);
                return sw.ToString();
            }
        }
        #endregion CreateCatchments
    }
    public class XmlSerializationScenariotester
    {
        [Test]
        public void TestSlopedFacilitySerialization()
        {
            var segs = Segments.Empty;
            segs.AddSegment(Segment.Make(
                12.7m, 7.5m, 0.25m,
                5.4m, 0.6m, 0.63m,
                13.5m, 11.7m));
            segs.AddSegment(Segment.Make(
                21.7m, 5.6m, 0.25m,
                8.2m, 0.45m, 0.35m,
                10.9m, 11.7m));

            var c = Catchment.Make("TestSlopedFacility",
                InfiltrationTestProcedure.OpenPit,
                PRStatus.Fail, 3.56m,
                HierarchyLevel.DischargeToRiverSlough,
                23456, 45, 56, 6, 8,
                SlopedFacility.Make(true,
                    Configuration.ConfigA, 5.2m,
                    AboveGradeProperties.Make( 222m, 5.3m,
                        187m, 0.9m, 7.2m, 4.5m,
                        287m, 5.4m, 87m),
                    BelowGradeProperties.Make(56,87.5m,
                        223.4m, 11.2m,
                        6.9m, 6m,
                        Orifice.Make(1.6m)),
                    segs));
            var rslt = ExpectedResult.Make("sloped");
            rslt.StormResults = StormResults.Empty;
            rslt.StormResults.AddRange(new[]
            {
                StormResult.Make(DesignStorm.WQ, PRStatus.Pass),
                StormResult.Make(DesignStorm.TwoYear,  PRStatus.Pass),
                //StormResult.Make(DesignStorm.HalfTwoYear,  PRStatus.Pass),
                StormResult.Make(DesignStorm.FivYear,  PRStatus.Pass),
                StormResult.Make(DesignStorm.TenYear,  PRStatus.Pass),
                StormResult.Make(DesignStorm.TwntyFiv,  PRStatus.Pass)
            });
            foreach (var se in rslt.StormResults)
            {
                var rnd = new Random();
                se.Timesteps = Timesteps.Empty;
                for (var ts = 1; ts <= 144; ts++)
                    se.Timesteps.AddTimestep(ts,
                        2m * (decimal)rnd.NextDouble(),
                        2m * (decimal)rnd.NextDouble(),
                        2m * (decimal)rnd.NextDouble(),
                        10.4m * (decimal)rnd.NextDouble(),
                        10.4m * (decimal)rnd.NextDouble(),
                        3.6m * (decimal)rnd.NextDouble(),
                        12.6m * (decimal)rnd.NextDouble(),
                        12.6m * (decimal)rnd.NextDouble(),
                        23.1m * (decimal)rnd.NextDouble());
            }
            var scenario = Scenario.Make("sloped", c, DateTime.UtcNow, rslt);
        
        using (var sw = new StringWriter())
            using (var xwrtr = XmlWriter.Create(sw))
            {
                var ser = new XmlSerializer(typeof(Scenario));
                ser.Serialize(xwrtr, scenario);
                var s = sw.ToString();
                Assert.IsNotNull(s);
            }
        }
        [Test]
        public void TestFlatPlanterSerialization()
        {
            var c = Catchment.Make("TestFlatPlanter",
                InfiltrationTestProcedure.OpenPit,
                PRStatus.Fail, 3.56m,
                HierarchyLevel.DischargeToRiverSlough,
                23456, 45, 56, 6, 8,
                FlatPlanter.Make(true, 4.89m, Configuration.ConfigB, 
                    AboveGradeProperties.Make( 234m, 8.9m,
                        70m, 0.23m, 6.7m, 5.34m,
                        4.5m, 9.0m, 7.6m),
                    BelowGradeProperties.Make(0.76m,87.5m,
                        5.2m, 223.4m, 6m, 0.83m, 
                        Orifice.Make(1.6m))));
            var scenario = Scenario.Make("sloped", c, DateTime.UtcNow, ExpectedResult.Empty);
            using (var sw = new StringWriter())
            using (var xwrtr = XmlWriter.Create(sw))
            {
                var ser = new XmlSerializer(typeof(Scenario));
                ser.Serialize(xwrtr, scenario);
                var s = sw.ToString();
                Assert.IsNotNull(s);
            }
        }
        [Test]
        public void TestRectangularBasinSerialization()
        {
            var c = Catchment.Make("TestRectBasin",
                InfiltrationTestProcedure.OpenPit,
                PRStatus.Fail, 3.56m,
                HierarchyLevel.DischargeToRiverSlough,
                23456, 45, 56, 6, 8,
                RectangularBasin.Make(true, 6.2m, 
                    Configuration.ConfigA,
                    AboveGradeProperties.Make(234m, 8.9m,
                        70m, 0.23m, 6.7m, 5.34m,
                        4.5m, 9.0m, 7.6m),
                    BelowGradeProperties.Make(0.76m, 87.5m,
                        5.2m, 223.4m, 6m, 0.56m,
                        Orifice.Make(1.6m))));
            var scenario = Scenario.Make("sloped", c, DateTime.UtcNow, ExpectedResult.Empty);
            using (var sw = new StringWriter())
            using (var xwrtr = XmlWriter.Create(sw))
            {
                var ser = new XmlSerializer(typeof(Scenario));
                ser.Serialize(xwrtr, scenario);
                var s = sw.ToString();
                Assert.IsNotNull(s);
            }
        }
        [Test]
        public void TestAmoebaBasinSerialization()
        {
            var c = Catchment.Make("TestAmoeba",
                InfiltrationTestProcedure.OpenPit,
                PRStatus.Fail, 3.56m,
                HierarchyLevel.DischargeToRiverSlough,
                23456, 45, 56, 6, 8,
                AmoebaBasin.Make(true, Configuration.ConfigA, 6.2m,
                    AboveGradeProperties.Make( 234m, 8.9m,
                        70m, 0.23m, 6.7m, 5.34m,
                        4.5m, 9.0m, 7.6m),
                    BelowGradeProperties.Make(0.76m,87.5m,
                        5.2m, 223.4m, 6m, 0.56m,
                        Orifice.Make(1.6m))));
            var scenario = Scenario.Make("sloped", c, DateTime.UtcNow, ExpectedResult.Empty);
            using (var sw = new StringWriter())
            using (var xwrtr = XmlWriter.Create(sw))
            {
                var ser = new XmlSerializer(typeof(Scenario));
                ser.Serialize(xwrtr, scenario);
                var s = sw.ToString();
                Assert.IsNotNull(s);
            }
        }
        [Test]
        public void TestUserDefinedBasinSerialization()
        {
            var c = Catchment.Make("TestUserDefined",
                InfiltrationTestProcedure.OpenPit,
                PRStatus.Fail, 3.56m,
                HierarchyLevel.DischargeToRiverSlough,
                23456, 45, 56, 6, 8,
                UserDefinedBasin.Make(true, Configuration.ConfigA, 6.2m,
                    AboveGradeProperties.Make(234m, 8.9m,
                        70m, 0.23m, 6.7m, 5.34m,
                        4.5m, 9.0m, 7.6m),
                    BelowGradeProperties.Make(0.76m,87.5m,
                        5.2m, 223.4m, 6m, 0.56m,
                        Orifice.Make(1.6m))));
            var scenario = Scenario.Make("sloped", c, DateTime.UtcNow, ExpectedResult.Empty);
            using (var sw = new StringWriter())
            using (var xwrtr = XmlWriter.Create(sw))
            {
                var ser = new XmlSerializer(typeof(Scenario));
                ser.Serialize(xwrtr, scenario);
                var s = sw.ToString();
                Assert.IsNotNull(s);
            }
        }
    }
}
