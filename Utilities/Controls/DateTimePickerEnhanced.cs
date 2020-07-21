using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Forms;
using wfMsg = System.Windows.Forms.Message;

namespace CoP.Enterprise.Controls
{
    public class Dtp: DateTimePicker
    {
        private const int WM_REFLECT = 0x2000;
        private const int WM_NOTIFY = 0x004E;
        private const uint DTN_DATETIMECHANGE = 0xFFFFFD09;

        [StructLayout(LayoutKind.Sequential)]
        private struct NMHDR
        {
            public IntPtr hwndFrom;
            public uint idFrom;
            public uint code;
        }

        public event EventHandler ValueChangedSpecial;
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        protected override void WndProc(ref wfMsg m)
        {
            base.WndProc(ref m);
            if (DesignMode || m.Msg != WM_REFLECT + WM_NOTIFY) return;
            var nm = (NMHDR) m.GetLParam(typeof (NMHDR));
            if (nm.code == DTN_DATETIMECHANGE)
                OnValueChangedSpecial();
        }

        protected virtual void OnValueChangedSpecial()
        {
            if (ValueChangedSpecial != null)
                ValueChangedSpecial(this, EventArgs.Empty);
        }
    }
}
