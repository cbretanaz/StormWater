using System;
using System.Windows.Forms;
using msg = CoP.Enterprise.Message;
namespace CoP.Enterprise.Controls
{
    public class NumberBox : TextBox
    {
        private static readonly StringComparison  icic
            = StringComparison.InvariantCultureIgnoreCase;
        private bool SettingText { get; set; }

        #region properties

        private decimal? val;
        public decimal? Value
        {
            get { return val; }
            set
            {
                val = value;
                Text = val.HasValue ? val.Value.ToString(EffFormat): string.Empty;
            }
        }

        public decimal DecimalValue
        {
            get { return Value.GetValueOrDefault(0m); }
            set { Value = value; }
        }

        public int IntegerValue
        {
            get
            {
                if (Value.HasValue && (Value > Int32.MaxValue || Value < Int32.MinValue))
                    throw new ArgumentOutOfRangeException(
                        "The value in numberBox out or range for an Int32.");
                return (int)Math.Round(Value.GetValueOrDefault(0m), 0, MidpointRounding.ToEven);
            }
            set { Value = value; }
        }

        public short Int16Value
        {
            get
            {
                if (Value.HasValue)
                {
                    if (Value > short.MaxValue)
                        return short.MaxValue;
                    if (Value < short.MinValue)
                        return short.MinValue;
                    return (short)Value;
                }

                return (short)Math.Round(Value.GetValueOrDefault(0m), 0, MidpointRounding.ToEven);
            }
            set { Value = value; }
        }
        public bool IsCurrency { get; set; }
        public bool IsPercent { get; set; }
        public string NullValueText { get; set; }
        public string ValidationMessageNonNumeric { get; set; }
        public decimal MinValue { get; set; }
        public string ValidationMessageLow { get; set; }
        public bool AllowNegative { get; set; }
        public decimal MaxValue { get; set; }
        public string ValidationMessageHigh { get; set; }
        public byte DecimalPrecision
        {
            get { return decimals; }
            set { decimals = value; }
        }
        #endregion properties

        private string CleanText => Text.Replace("$", string.Empty);

        public NumberBox()
        {
            DecimalPrecision = 0;
            TextAlign = HorizontalAlignment.Right;
            AllowNegative = true;
        }


        private byte decimals;

        private decimal lastValue { get; set; }
        public string Format { get; set; }
        

        public string EffFormat => 
            !string.IsNullOrWhiteSpace(Format)? Format:
            (DecimalPrecision == 0 ? "0" :
                IsCurrency? "$#,##0.00":
                    "0.".PadRight(DecimalPrecision + 2, '0'));

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
                if (e.KeyChar == '-' && (!AllowNegative || SelectionStart != 0))
                e.Handled = true;


            var hasDecimal = Text.Contains(".");
            if (!char.IsControl(e.KeyChar) && 
                !char.IsDigit(e.KeyChar) &&
                (e.KeyChar != '-' || !AllowNegative) &&
                (e.KeyChar != '.' || hasDecimal))
              e.Handled = true;

            base.OnKeyPress(e);
        }
        protected override void OnTextChanged(EventArgs e)
        {
            if (SettingText) return;
            if (string.IsNullOrWhiteSpace(Text))
            {   
                val = null;
                Text = NullValueText;
            }
            else
            {
                var emp = string.Empty;
                decimal nbr;
                var pTxt = Text.Replace("$", emp).Replace(",", emp);
                if (decimal.TryParse(pTxt, out nbr))
                {
                    if (IsPercent) nbr /= 100;
                    if (nbr >= MinValue && nbr <= MaxValue)
                        val = nbr;
                }
                else if (IsPercent && pTxt.EndsWith("%", icic) &&
                    decimal.TryParse(pTxt.Replace("%", emp), out nbr))
                {
                    nbr /= 100;
                    if (nbr >= MinValue && nbr <= MaxValue)
                        val = nbr;
                }
            }
            base.OnTextChanged(e);
        }
        protected override void OnValidated(EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Text)) return;
            var emp = String.Empty;
            decimal nbr;
            var pTxt = Text.Replace("$", emp).Replace(",", emp);

            if (decimal.TryParse(pTxt, out nbr))
            {
                if (IsPercent) nbr /= 100;
                if (nbr >= MinValue && nbr <= MaxValue)
                    val = nbr;
                else if (nbr < MinValue && !string.IsNullOrWhiteSpace(ValidationMessageLow))
                    msg.Warn(FormatValidationMessage(ValidationMessageLow, pTxt, MinValue));
                else if (nbr > MaxValue && !string.IsNullOrWhiteSpace(ValidationMessageHigh))
                    msg.Warn(FormatValidationMessage(ValidationMessageHigh, pTxt, MaxValue));
            }
            else if (!string.IsNullOrWhiteSpace(ValidationMessageNonNumeric))
                msg.Warn(ValidationMessageNonNumeric);
            
            Text = val.HasValue ? val.Value.ToString(EffFormat) : string.Empty;
            base.OnValidated(e);
        }

        private string FormatValidationMessage(string msg, string inVal, decimal? limitVal)
        {
            decimal decVal;
            if (!decimal.TryParse(inVal, out decVal))
                return msg.Replace("[V]", "{0}");
            // ------------------------------------
            msg = msg.Replace("[V]", inVal);
            return limitVal.HasValue?
                msg.Replace("[L]", limitVal.Value.ToString(EffFormat)) : msg;
        }
    }
}