using System.Collections.Generic;
using System.Drawing;

namespace BES.SWMM.PAC
{
    public class AGCtrls : Dictionary<AGP, Point>
    {
        internal static AGCtrls Empty => new AGCtrls();
    }

    public class BGCtrls : Dictionary<BGP, Point>
    {
        internal static BGCtrls Empty => new BGCtrls();
    }

    public class CfgCboBoxItm
    {
        public Configuration ConfigValue { get; set; }
        public string Description { get; set; }

        public static CfgCboBoxItm Make(Configuration cfg, string descr)
        { return new CfgCboBoxItm { ConfigValue = cfg, Description = descr }; }
    }
    internal enum ChartState { All, Single }
}
