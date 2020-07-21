using System;
using System.Drawing;
using System.Windows.Forms;

namespace CoP.Enterprise.Controls
{
    public class TransparentControl : Control
    {
        public bool drag = false;
        public bool enab = false;
        private int opacity = 100;

        private int alpha;

        public TransparentControl()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.Opaque, true);
            BackColor = Color.Transparent;
        }

        public int Opacity
        {
            get { return opacity > 100? 100: opacity < 1? 1: opacity; }
            set
            {
                opacity = value;
                Parent?.Invalidate(Bounds, true);
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle = cp.ExStyle | 0x20;
                return cp;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            var bounds = new Rectangle(0, 0, Width - 1, Height - 1);

            var frmColor = Parent.BackColor;
            var bckColor = default(Brush);

            alpha = (opacity*255)/100;

            if (drag)
            {
                var dragBckColor = default(Color);

                if (BackColor != Color.Transparent)
                {
                    var Rb = BackColor.R*alpha/255 + frmColor.R*(255 - alpha)/255;
                    var Gb = BackColor.G*alpha/255 + frmColor.G*(255 - alpha)/255;
                    var Bb = BackColor.B*alpha/255 + frmColor.B*(255 - alpha)/255;
                    dragBckColor = Color.FromArgb(Rb, Gb, Bb);
                }
                else
                    dragBckColor = frmColor;

                alpha = 255;
                bckColor = new SolidBrush(Color.FromArgb(alpha, dragBckColor));
            }
            else
                bckColor = new SolidBrush(Color.FromArgb(alpha, BackColor));

            if (BackColor != Color.Transparent | drag)
                g.FillRectangle(bckColor, bounds);

            bckColor.Dispose();
            g.Dispose();
            base.OnPaint(e);
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            Parent?.Invalidate(Bounds, true);
            base.OnBackColorChanged(e);
        }

        protected override void OnParentBackColorChanged(EventArgs e)
        {
            Invalidate();
            base.OnParentBackColorChanged(e);
        }
    }
    public class TransparentTextBox : TextBox
    {
        public bool drag = false;
        public bool enab = false;
        private int opacity = 100;

        private int alpha;

        public TransparentTextBox()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.Opaque, false);
            BackColor = Color.Transparent;
        }

        public int Opacity
        {
            get { return opacity > 100 ? 100 : opacity < 1 ? 1 : opacity; }
            set
            {
                opacity = value;
                Parent?.Invalidate(Bounds, true);
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle = cp.ExStyle | 0x20;
                return cp;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            var bounds = new Rectangle(0, 0, Width - 1, Height - 1);

            var frmColor = Parent.BackColor;
            var bckColor = default(Brush);

            alpha = (opacity * 255) / 100;

            if (drag)
            {
                var dragBckColor = default(Color);

                if (BackColor != Color.Transparent)
                {
                    var Rb = BackColor.R * alpha / 255 + frmColor.R * (255 - alpha) / 255;
                    var Gb = BackColor.G * alpha / 255 + frmColor.G * (255 - alpha) / 255;
                    var Bb = BackColor.B * alpha / 255 + frmColor.B * (255 - alpha) / 255;
                    dragBckColor = Color.FromArgb(Rb, Gb, Bb);
                }
                else
                    dragBckColor = frmColor;

                alpha = 255;
                bckColor = new SolidBrush(Color.FromArgb(alpha, dragBckColor));
            }
            else
                bckColor = new SolidBrush(Color.FromArgb(alpha, BackColor));

            if (BackColor != Color.Transparent | drag)
                g.FillRectangle(bckColor, bounds);

            bckColor.Dispose();
            g.Dispose();
            base.OnPaint(e);
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            Parent?.Invalidate(Bounds, true);
            base.OnBackColorChanged(e);
        }

        protected override void OnParentBackColorChanged(EventArgs e)
        {
            Invalidate();
            base.OnParentBackColorChanged(e);
        }
    }

}