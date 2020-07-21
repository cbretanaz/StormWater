using System.IO;
using System.Xml.Serialization;
using NUnit.Framework;
using lib = CoP.Enterprise.Utilities;

namespace BES.SWMM.PAC
{
    public class CommonValuesTester
    {
        private static readonly double dlta = 1e-6d;
        #region DesignStorm Consts
        private const DesignStorm dswq = DesignStorm.WQ;
        private const DesignStorm ds2 = DesignStorm.TwoYear;
        private const DesignStorm ds5 = DesignStorm.FivYear;
        private const DesignStorm ds10 = DesignStorm.TenYear;
        private const DesignStorm ds25 = DesignStorm.TwntyFiv;
        #endregion DesignStorm Consts
        [Test]
        [TestCase(true, Configuration.ConfigA, 23d,
            12d, 456d, 12.2d, 10d, 18d,
            9d, 81d, 0d)]
        [TestCase(true, Configuration.ConfigB, 23d,
            12d, 456d, 12.2d, 10d, 18d,
            9d, 81d, 0d)]
        [TestCase(true, Configuration.ConfigE, 23d,
            12d, 456d, 12.2d, 10d, 18d,
            9d, 81d, 0d)]
        [TestCase(true, Configuration.ConfigC, 23d,
            12d, 456d, 12.2d, 10d, 18d,
            9d, 81d, 41d)]
        [TestCase(true, Configuration.ConfigC, 23d,
            12d, 456d, 12.2d, 10d, 18d,
            9d, 81d, 41d)]
        public void CalculateEstIncUnderdrainOutflowTester(
            bool hasOrifice, Configuration cfg,
            decimal incMediaTotOut,   // MiVout[t]
            decimal pRockLvlBU,       // prev RiVoverandMiVnrOVER[t-1]
            decimal maxIncRockInput,  // RiVxIN
            decimal pOF,              // prevOverFlow
            decimal pOFE,             // prevOverFlowE[t-1]
            decimal estORVol,         // ORiVest[t]
            decimal pestUDOF,         // prevPiVest[t-1]
            decimal pCumRockAP,
            decimal expValue)
        {
            //var calc = Calculator.Make(Catchment.Empty);
            var undrDrnOF = Calculator.CalculateEstIncUnderdrainOutflow(hasOrifice, cfg,
                incMediaTotOut, pRockLvlBU, maxIncRockInput,
                pOF, pOFE, estORVol, pestUDOF, pCumRockAP);

            Assert.AreEqual(expValue, undrDrnOF);
        }

        [TestCase(true, Configuration.ConfigC, 23d,  
            12d,456d, 12.2d, 10d, 
            18d, 9d, 81d, 41d, 23d,
            104d)]
        public void CalculateIncrUnderdrainAndSubsurfaceOutFlowTester(
            bool hasOrifice, Configuration cfg,
            decimal incMedOut,            // MiVout
            decimal maxIncRockInput,      // RiVxIN
            decimal maxTSInfVol,          // GiVx
            decimal estUDOutflow,         // PiVest
            decimal estOROF,              // ORiVest
            // --------------------------------------
            decimal pOFlow,         // previous OVERiV
            decimal pOFlowE,        // previous OVERiVfirst
            decimal pRckLvlVol,     // previous RcVandMcVnr 
            decimal pUDOF,          // previous PiVest 
            decimal pRckVolAP,      // previous RcVap
            decimal expValue)      
        {
            var undrDrnOF 
                = Calculator.CalculateIncrementalFlowLeavingFacilitythroughUnderdrainAndSubsurface(
                    hasOrifice, cfg,  incMedOut, maxIncRockInput, maxTSInfVol,
                    estUDOutflow, estOROF, pOFlow, pOFlowE, pRckLvlVol, pUDOF, pRckVolAP);
            // -------------------------------------------------
            Assert.AreEqual(expValue, undrDrnOF, $"Underdrain was {undrDrnOF}, but should be {expValue}");
        }
    }
}