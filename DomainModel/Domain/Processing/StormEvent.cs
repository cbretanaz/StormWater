using System;
using System.Collections.Generic;
using System.Linq;

namespace BES.SWMM.PAC
{
    public abstract class StormEvent : List<StormEventInterval>
    {
        #region static constants
        protected static readonly PACConfig pacCfg = PACConfig.Make();
        protected static readonly EngineSettings engStgs = pacCfg.EngineSettings;
        protected static readonly int TS = engStgs.TimeStep;
        protected static readonly int TSMIN = engStgs.TimeStepMinutes;
        protected static readonly int MAXTS = 400;
        #region DesignStorm Consts
        protected const DesignStorm dswq = DesignStorm.WQ;
        protected const DesignStorm ds2 = DesignStorm.TwoYear;
        protected const DesignStorm dsH2 = DesignStorm.HalfTwoYear;
        protected const DesignStorm ds5 = DesignStorm.FivYear;
        protected const DesignStorm ds10 = DesignStorm.TenYear;
        protected const DesignStorm ds25 = DesignStorm.TwntyFiv;
        #endregion DesignStorm Consts
        #endregion static constants

        #region Properties
        public abstract DesignStorm DesignStorm { get; }
        public abstract bool isHalfTwoyear { get; }
        public decimal totalCubicFt = 0.0m;
        public decimal TotalCubicFeetVolume
        {
            get => totalCubicFt;
            set => totalCubicFt = value;
        }
        public decimal cumRunOff = 0.0m;
        public decimal CumulativeRunOff
        {
            get => cumRunOff;
            set => cumRunOff = value;
        }
        public bool Contains(int ti)
        { return this.Any(itv => itv.Timestep == ti); }

        public new StormEventInterval this[int ti]
        { get { return this.FirstOrDefault(i => i.Timestep == ti); } }
        #endregion Properties

        public void FillStormEventRunOffSchedule(
            decimal impArea, decimal cn, decimal toc )
        {
            var potMaxRetention = 1000.0m / cn - 10.0m; // The rainfall threshold without the 20% factor
            var initAbstraction = 0.2m * potMaxRetention;
            var lastCumRO = 0.0m; 
            var lastRtdRO = 0.0m;
            var lastInstRO = 0.0m; 
            var totalVolume = 0.0m;
            // -------------------------------------------------------------
            for (var ti = 1; ti <= MAXTS; ti++) //todo: make the 170 value a configuration setting
            {
                if(!Contains(ti)) Add(StormEventInterval.Make(ti));
                var rainfall = Hyetograph.CumulativeRainfall(DesignStorm, ti);
                var sei = this[ti]; // This is the current StormEventInterval object
                var skipCalc = rainfall <= initAbstraction; // boolean to specify whether to perform calculation
                
                // RUNcD
                //var cumRO = skipCalc ? 0.0m: Runoff( potMaxRetention, rainfall);
                var cumRO = skipCalc ? 0.0m : 
                    (decimal)Math.Pow((double)(rainfall - 0.2m * potMaxRetention), 2d) /
                                                 (rainfall + 0.8m * potMaxRetention);
                // RUNiQins
                //var instRO = InstRunOffRate(impArea, cumRO, lastCumRO); //Should be zero if cumRO is zero
                var instRO =  impArea * (cumRO - lastCumRO) / (12m * TS);

                // RUNiQrte
                //var rtdRO  = RoutedRunOffRate(toc, instRO, lastIncRO, lastRtdRO); //Should be zero if cumRO is zero
                // --------------------------------------------------------
                var rtdRO = lastRtdRO +
                            TSMIN * (instRO + lastInstRO - 2m * lastRtdRO) /
                                      (TSMIN + 2m * toc);

                sei.RunOff = TS * rtdRO; // this value is persisted 
                if (isHalfTwoyear) sei.RunOff /= 2m; //Eliminate this for HalfTwoYearEvent-TwoYearEvent-HalfTwoYearEvent for PREDEV ONLY...
                lastCumRO = cumRO;  // set variables 
                lastRtdRO = rtdRO;  // -------------
                lastInstRO = instRO; // for next iteration
                totalVolume += sei.RunOff;
            }
            TotalCubicFeetVolume = totalVolume;
            CumulativeRunOff = lastCumRO;
        }

        #region ctor/Factories
        public static StormEvent Make(DesignStorm dsgn, decimal impArea, decimal cn, decimal toc) =>
            dsgn == dswq ? WaterQualityEvent.Make(impArea, cn, toc) :
            dsgn == ds2 ? TwoYearEvent.Make(impArea, cn, toc) :
            dsgn == ds5 ? FiveYearEvent.Make( impArea, cn, toc) :
            dsgn == ds10 ? TenYearEvent.Make( impArea, cn, toc) :
            dsgn == ds25 ? TwentyFiveYearEvent.Make(impArea, cn, toc) :
            (StormEvent)null;

        public static StormEvent Make(DesignStorm dsgn, Catchment ctch) =>
            dsgn == dswq ? WaterQualityEvent.Make( ctch.ImperviousArea, ctch.PostCurveNumber, ctch.PostTOC):
            dsgn == ds2 ? TwoYearEvent.Make(ctch.ImperviousArea, ctch.PostCurveNumber, ctch.PostTOC):
            dsgn == dsH2 ? HalfTwoYearEvent.Make(ctch.ImperviousArea, ctch.PostCurveNumber, ctch.PostTOC):
            dsgn == ds5 ? FiveYearEvent.Make(ctch.ImperviousArea, ctch.PostCurveNumber, ctch.PostTOC): 
            dsgn == ds10 ? TenYearEvent.Make(ctch.ImperviousArea, ctch.PostCurveNumber, ctch.PostTOC):
            dsgn == ds25 ? TwentyFiveYearEvent.Make(ctch.ImperviousArea, ctch.PostCurveNumber, ctch.PostTOC):
            (StormEvent)null;
        #endregion ctor/Factories

        #region private Helper functions
        #region Calculation functions
        //protected static decimal Runoff(decimal potMaxRetention, decimal rainfall)
        //{
        //    return (decimal)(Math.Pow((double)(rainfall - 0.2m * potMaxRetention), 2d)) / 
        //                             (rainfall + 0.8m * potMaxRetention);
        //}
        //protected static decimal InstRunOffRate(
        //    decimal impArea, decimal cumRO, decimal? lastCumRO)
        //{ return impArea * (cumRO - (lastCumRO ?? 0m))/7200m; }
        //protected static decimal RoutedRunOffRate(decimal toc, decimal iROR,
        //    decimal lastIROR, decimal lastRteROR)
        //{
        //    return lastRteROR + 
        //            TS * (iROR + lastIROR - 2m * lastRteROR) /
        //                        (TS + 2m*toc);
        //}
        #endregion Calculation functions
        #endregion private Helper functions

        public override string ToString(){ return $"{DesignStorm} Event";}
    }

    #region StormEvent subClasses
    public class WaterQualityEvent : StormEvent
    {
        public override DesignStorm DesignStorm => dswq;
        public override bool isHalfTwoyear => false;
        #region ctor/Factories
        public static WaterQualityEvent Empty => new WaterQualityEvent();

        public static WaterQualityEvent Make(
            decimal impArea, decimal cn, decimal toc)
        {
            var evt = Empty;
            evt.FillStormEventRunOffSchedule(impArea, cn, toc);
            return evt;
        }
        #endregion ctor/Factories
    }
    public class TwoYearEvent : StormEvent
    {
        public override DesignStorm DesignStorm => ds2;
        public override bool isHalfTwoyear => false;

        #region ctor/Factories
        public static TwoYearEvent Empty => new TwoYearEvent();
        public static TwoYearEvent Make(
            decimal impArea, decimal cn, decimal toc)
        {
            var evt = Empty;
            evt.FillStormEventRunOffSchedule(impArea, cn, toc);
            return evt;
        }
        #endregion ctor/Factories
    }
    public class HalfTwoYearEvent : TwoYearEvent
    {
        public override DesignStorm DesignStorm => dsH2;
        public override bool isHalfTwoyear => true;

        #region ctor/Factories
        public static new HalfTwoYearEvent Empty => new HalfTwoYearEvent();
        public new static HalfTwoYearEvent Make(
            decimal impArea, decimal cn, decimal toc)
        {
            var evt = Empty;
            evt.FillStormEventRunOffSchedule(impArea, cn, toc);
            return evt;
        }
        #endregion ctor/Factories
    }

    public class FiveYearEvent : StormEvent
    {
        public override DesignStorm DesignStorm => ds5;
        public override bool isHalfTwoyear => false;

        #region ctor/Factories
        public static FiveYearEvent Empty => new FiveYearEvent();
        public static FiveYearEvent Make(
            decimal impArea, decimal cn, decimal toc)
        {
            var evt = Empty;
            evt.FillStormEventRunOffSchedule(impArea, cn, toc);
            return evt;
        }
        #endregion ctor/Factories
    }
    public class TenYearEvent : StormEvent
    {
        public override DesignStorm DesignStorm => ds10;
        public override bool isHalfTwoyear => false;

        #region ctor/Factories
        public static TenYearEvent Empty => new TenYearEvent();
        public static TenYearEvent Make(
            decimal impArea, decimal cn, decimal toc)
        {
            var evt = Empty;
            evt.FillStormEventRunOffSchedule(impArea, cn, toc);
            return evt;
        }
        #endregion ctor/Factories
    }
    public class TwentyFiveYearEvent : StormEvent
    {
        public override DesignStorm DesignStorm => ds25;
        public override bool isHalfTwoyear => false;

        #region ctor/Factories
        public static TwentyFiveYearEvent Empty => new TwentyFiveYearEvent();
        public static TwentyFiveYearEvent Make(
            decimal impArea, decimal cn, decimal toc)
        {
            var evt = Empty;
            evt.FillStormEventRunOffSchedule(impArea, cn, toc);
            return evt;
        }
        #endregion ctor/Factories
    }
    #endregion StormEvent subClasses
}