using System;
using System.Drawing;
using System.Windows.Forms;

namespace CoP.Enterprise.Controls
{
    public class DateTimeListBox : ListBox
    {
        public DateTimeListBox(){Initialize();}

        private void Initialize()
        {  DrawMode = DrawMode.OwnerDrawFixed; }
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index >= 0 && e.Index < Items.Count)
            {
                var selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
                var back = selected ? SystemColors.Highlight : BackColor;
                var fore = selected ? SystemColors.HighlightText : ForeColor;
                var txt =((DateTime)Items[e.Index]).ToString(FormatString);
                TextRenderer.DrawText(e.Graphics, txt, Font, e.Bounds, fore, back, 
                    TextFormatFlags.Right | TextFormatFlags.SingleLine);
            }
            e.DrawFocusRectangle();
        }
    }
}
