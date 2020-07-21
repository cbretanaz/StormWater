using System;
using System.Linq;
using System.Windows.Forms;

namespace CoP.Enterprise
{
    //public class RadioGroupBox : GroupBox
    //{
    //    public event EventHandler SelectedChanged = delegate { };

    //    private int selctd;

    //    public int Selected
    //    {
    //        get => selctd;
    //        set
    //        {
    //            var val = 0;
    //            var radioButton = 
    //                Controls.OfType<RadioButton>()
    //                        .FirstOrDefault(radio =>
    //                            radio.Tag != null &&
    //                            int.TryParse(radio.Tag.ToString(), out val) && 
    //                            val == value);

    //            if (radioButton == null) return;
    //            radioButton.Checked = true;
    //            selctd = val;
    //        }
    //    }

    //    protected override void OnControlAdded(ControlEventArgs e)
    //    {
    //        base.OnControlAdded(e);

    //        if (e.Control is RadioButton radioButton)
    //            radioButton.CheckedChanged += ChangeHierarchyLevel;
    //    }

    //    void ChangeHierarchyLevel(object sender, EventArgs e)
    //    {
    //        var radio = (RadioButton) sender;
    //        var hL = radio.Tag;
    //        if (!radio.Checked || radio.Tag == null || 
    //            !int.TryParse(radio.Tag.ToString(), out var val)) return;
    //        // ----------------------------
    //        selctd = val;
    //        SelectedChanged(this, new EventArgs());
    //    }
    //}

    public class EnumGroupBox<E>: GroupBox where E: Enum
    {
        public event EventHandler SelectedChanged = delegate { };

        private E selctd;

        public E Selected
        {
            get => selctd;
            set
            {
                var rb = Controls.OfType<RadioButton>()
                        .FirstOrDefault(r => r.Tag != null && 
                            ((E)r.Tag).Equals(value));
                if (rb == null) return;
                rb.Checked = true;
                selctd = (E)rb.Tag;
            }
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);

            if (e.Control is RadioButton rb) rb.CheckedChanged += ChangeEnum;
        }

        void ChangeEnum(object sender, EventArgs e)
        {
            var rb = (RadioButton)sender;
            if (rb.Tag == null) return;
            var val = (E)rb.Tag;
            if (!rb.Checked || val == null) return;
            // ----------------------------
            selctd = val;
            SelectedChanged(this, new EventArgs());
        }
    }
}