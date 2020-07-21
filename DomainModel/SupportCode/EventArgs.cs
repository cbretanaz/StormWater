
using System;

namespace BES.SWMM.PAC
{
    public class ArrangeControlsEventArgs : EventArgs
    {
        public FacilityCategory Category { set; get; }
        public Configuration Config { set; get; }

        public static ArrangeControlsEventArgs Make(FacilityCategory cat, Configuration cfg)
        {
            return new ArrangeControlsEventArgs {Category = cat, Config = cfg};
        }
    }
    public class ValidatorEventArgs : EventArgs
    {
        public Scenario Scenario { set; get; }
        public static ValidatorEventArgs Make(Scenario scen)
        { return new ValidatorEventArgs { Scenario = scen }; }
    }
}
