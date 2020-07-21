using System;
using System.Windows.Forms;
using BES.SWMM.PAC;

namespace BES.SWMM.PAC
{
    public interface ISetFocus  { void SetFocus(string ctrl); }
    public interface ICanBeArranged
    {
        event EventHandler<ArrangeControlsEventArgs> ArrangeControls;
        Control FindControl(string controlName);
    }

    public interface IValidateData
    { event EventHandler<ValidatorEventArgs> ValidateScenario; }
}
