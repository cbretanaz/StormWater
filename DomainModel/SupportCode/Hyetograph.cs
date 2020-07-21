using System;
using System.Collections.Generic;
using System.Linq;

namespace BES.SWMM.PAC
{
    public class Hyetograph: Constants
    {
        #region consts

        private static PACConfig pacCfg = PACConfig.Make();
        private static EngineSettings engStgs = pacCfg.EngineSettings;
        //private static int MAXTS = engStgs.MaxTimesteps;
        private static int MAXTS = 400;
        private static readonly string sNL = Environment.NewLine;
        public static IEnumerable<int> Intervals
        {get{for (var i = 1; i <= MAXTS; i++) yield return i;}}

        private static readonly Dictionary<int, decimal> consts = new Dictionary<int, decimal>();
        private static readonly Dictionary<DesignStorm, decimal> stormDepths 
            = new Dictionary<DesignStorm, decimal>();
        #endregion consts

        static Hyetograph()
        {
            consts.Add(0, 0.004m);
            consts.Add(11, 0.005m);
            consts.Add(17, 0.006m);
            consts.Add(23, 0.007m);
            consts.Add(29, 0.0082m);
            consts.Add(35, 0.0095m);
            consts.Add(41, 0.0134m);
            consts.Add(44, 0.018m);
            consts.Add(46, 0.034m);
            consts.Add(47, 0.054m);
            consts.Add(48, 0.027m);
            consts.Add(49, 0.018m);
            consts.Add(50, 0.0134m);
            consts.Add(53, 0.0088m);
            consts.Add(65, 0.0072m);
            consts.Add(77, 0.0057m);
            consts.Add(89, 0.005m);
            consts.Add(101, 0.004m);
            consts.Add(145, 0.00m);
            // -----------------------
            stormDepths.Add(dswq, 1.61m);
            stormDepths.Add(ds2, 2.4m);
            stormDepths.Add(dsH2, 2.4m);
            stormDepths.Add(ds5, 2.9m);
            stormDepths.Add(ds10, 3.4m);
            stormDepths.Add(ds25, 3.8m);
        }
        public static Hyetograph Initialize => new Hyetograph();
        public static decimal RainfallPercentage(int ti)
        {
            if (ti <= 0 || ti > MAXTS) throw
                new ParameterOutOfRangeException(ti,
                    $"Time Timestep of {ti} is invalid. {sNL}" +
                    $"Timestep must be between 1 and {MAXTS}.");
            return consts.Last(h => h.Key <= ti).Value;
        }
        public static decimal Rainfall(DesignStorm ds,int timItvl)
        { return RainfallPercentage(timItvl) * stormDepths[ds]; }
        public static decimal CumulativePercentage(int ti)
        {
            if (ti <= 0 || ti > MAXTS) throw
                new ParameterOutOfRangeException(ti,
                    $"Time Timestep of {ti} is invalid. {sNL}" +
                    $"Time Timestep must be between 1 and {MAXTS}.");
            var cumPcnt = 0.0m;
            for (var t = 1; t <= ti; t++)
                cumPcnt += RainfallPercentage(t);
            return cumPcnt;
        }
        public static decimal CumulativePercentage2(int ti)
        {
            if (ti <= 0 || ti > MAXTS) throw
                new ParameterOutOfRangeException(ti,
                    $"Time Timestep of {ti} is invalid. {sNL}" +
                    $"Timestep must be between 1 and {MAXTS}.");
            return Intervals.ToList().Sum(RainfallPercentage);
        }
        public static decimal CumulativeRainfall(DesignStorm ds, int timItvl)
        {
            return CumulativePercentage(timItvl) * stormDepths[ds];
        }
        public static decimal RunOff(int timItvl, DesignStorm ds)
        { return RainfallPercentage(timItvl) * stormDepths[ds]; }
    }
}