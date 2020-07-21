

using System.Windows.Forms;

namespace BES.SWMM.PAC
{
    public interface IControlView
    {
        TabControl TabControl { get; }
        void BindControls(Scenario scenario);
        void PaintControls(Facility fclty);
        void UpdateScenario(Scenario scenario);
    }
}
