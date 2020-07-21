using System;
using System.Windows.Forms;

namespace CoP.Enterprise.Controls
{
    public class NumberTextBox : TextBox
    {
        private static readonly StringComparison icic 
            = StringComparison.InvariantCultureIgnoreCase;

        #region properties
        public bool IsCurrency { get; set; }
        public decimal MinValue { get; set; }
        public Decimal Value
        {
            get { return decimal.Parse(CleanText); }
            set { Text = value.ToString(EffFormat); }
        }
        public bool AllowNegative { get; set; }
        public decimal MaxValue { get; set; }
        public byte DecimalPrecision
        {
            get { return decimals; }
            set { decimals = value; }
        }
        #endregion properties

        private string CleanText
        {
            get
            {
                return Text.Replace("$", string.Empty);
            }
        }
       
        
        public NumberTextBox()
        {
            DecimalPrecision = 0;
            TextAlign = HorizontalAlignment.Right;
            AllowNegative = true;
        }


        private byte decimals;

        private decimal lastValue { get; set; }
        public string Format { get; set; }
        

        public string EffFormat
        {
            get { return 
                !string.IsNullOrWhiteSpace(Format)? Format:
                (DecimalPrecision == 0 ? "0" :
                    IsCurrency? "$#,##0.00":
                    "0.".PadRight(DecimalPrecision + 2, '0')); }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            var clnTxt = CleanText;
            if (e.KeyChar == '-' && (!AllowNegative || SelectionStart != 0))
                e.Handled = true;
            lastValue = clnTxt == "-" || clnTxt == "." ||
                        string.IsNullOrWhiteSpace(clnTxt) ? 0 :
                             decimal.Parse(clnTxt);
            // Ignore all non-control and non-numeric key presses
            var hasDecimal = CleanText.Contains(".");
            if (!char.IsControl(e.KeyChar) && 
                !char.IsDigit(e.KeyChar) &&

                (e.KeyChar != '-' || !AllowNegative) &&
                (e.KeyChar != '.' || hasDecimal))
              e.Handled = true;

            //  to Call the implementation in the base TextBox class,
            // which raises the KeyPress event.
            base.OnKeyPress(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            var clnTxt = CleanText;
            var val = clnTxt == "-" ||string.IsNullOrWhiteSpace(clnTxt)?
                      0 : decimal.Parse(clnTxt);
            if (val > MaxValue || val < MinValue) Text = lastValue.ToString(EffFormat);
            base.OnKeyUp(e);
        }

        protected override void OnValidated(EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Text)) return;
            Text = Value.ToString(EffFormat);
            base.OnValidated(e);
        }
    }
}