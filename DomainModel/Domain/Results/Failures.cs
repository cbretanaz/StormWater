using System;
using CoP.Enterprise;

namespace BES.SWMM.PAC
{
    public class Failure
    {
        #region properties
        public int? Timestep { get; set; }
        public string TS => Timestep == null ? "Gen" : Timestep.Value.ToString("0");
        public FailType Type { get; set; }
        public string Message { get; set; }
        #endregion properties

        #region factory/ctors
        public static Failure Empty => new Failure();
        public static Failure Make(FailType type, string msg, int? ts)
        { return new Failure {Type= type, Message = msg, Timestep = ts }; }
        #endregion factory/ctors

    }

    public class Failures : SortableBindingList<Failure>
    {
        public DesignStorm DesignStorm { get; set; }
        #region factory/ctors

        public static Failures Make(DesignStorm ds)
        { return new Failures{DesignStorm = ds}; }
        public void AddFailure( FailType type, string msg, int? ts)
        { Add(Failure.Make(type, msg, ts));}
        #endregion factory/ctors

    }

}