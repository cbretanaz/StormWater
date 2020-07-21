
using System.Windows.Forms;

namespace BES.SWMM.PAC.FormViews
{
    public partial class FailuresForm : Form
    {
        public FailuresForm(Failures fails)
        {
            InitializeComponent();
            DisplayFailures(fails);
        }

        private void DisplayFailures(Failures fails)
        {
            bsFails.DataSource = fails;
            Text = $"Design Storm {fails.DesignStorm} Failures";
        }
    }
}
