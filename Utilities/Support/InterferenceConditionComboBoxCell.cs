using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CoP.Controls
{

    public class DataGridViewFlaggedEnumColumn<E>: DataGridViewColumn where E:struct, IConvertible
    {
        public DataGridViewFlaggedEnumColumn()
            : base(new DataGridViewFlaggedEnumCell<E>())
        {
            if (!typeof(E).IsEnum) throw new ArgumentException(
                "E must be an enumerated type");
        }

        public override DataGridViewCell CellTemplate
        {
            get { return base.CellTemplate; }
            set
            {
                if(value != null &&
                    !value.GetType().IsAssignableFrom(
                    typeof(DataGridViewFlaggedEnumCell<E>)))
                    throw new InvalidCastException(
                        "Must be a DataGridViewMultiComboBoxCell");

                base.CellTemplate = value;
            }
        }
    }

    public class DataGridViewFlaggedEnumCell<E>: DataGridViewTextBoxCell where E:struct, IConvertible
    {
        public DataGridViewFlaggedEnumCell() : base()
        {
            if (!typeof(E).IsEnum) throw new ArgumentException(
                "E must be an enumerated type");
        }

        public override void InitializeEditingControl(int rowIndex, 
            object initialFormattedValue, 
            DataGridViewCellStyle dataGridViewCellStyle)
        {
            base.InitializeEditingControl(rowIndex, 
                initialFormattedValue, dataGridViewCellStyle);
            var ctrl = DataGridView.EditingControl as FlaggedEnumEditingControl<E>;
            ctrl.Value = Value == null ?  default(E) : (E)Value;
        }
 
        public override Type EditType { get { return typeof(FlaggedEnumEditingControl<E>); } }

        public override Type ValueType { get { return typeof (E); } }

        public override object DefaultNewRowValue { get { return default(E); } }
    }

    public class FlaggedEnumEditingControl<E> : CheckedListBox, IDataGridViewEditingControl where E : struct, IConvertible
    {
        DataGridView dgv;
        public List<string> DisplayValues { get; set; }
        private bool valueChanged = false;
        private readonly FlaggedEnumFormatProvder 
            fp = new FlaggedEnumFormatProvder();

        public FlaggedEnumEditingControl()
        {
            if (!typeof(E).IsEnum) throw new ArgumentException(
                "E must be an enumerated type");
            foreach (var mbr in DisplayValues) Items.Add(mbr);
        }

        public object EditingControlFormattedValue
        {
            get
            {
                E val = default(E);
                E tst = default(E);
                foreach (string itm in CheckedItems)
                    if (Enum.TryParse(itm, out tst)) 
                        val = (E)Enum.Parse(typeof(E), 
                            (val.ToInt32(fp) | tst.ToInt32(fp)).ToString("0"));
                return val; 
            } 
            set { if (value.GetType().IsInstanceOfType(typeof(E))) Value = (E)value; }
        }
        public object GetEditingControlFormattedValue(DataGridViewDataErrorContexts context)
        {  return EditingControlFormattedValue; }

        public void ApplyCellStyleToEditingControl(DataGridViewCellStyle dataGridViewCellStyle)
        {
            Font = dataGridViewCellStyle.Font;
            ForeColor = dataGridViewCellStyle.ForeColor;
            BackColor = dataGridViewCellStyle.BackColor;
        }

        public int EditingControlRowIndex { get; set; }

        public bool EditingControlWantsInputKey(Keys keyData, 
            bool dataGridViewWantsInputKey)
        {
            switch (keyData & Keys.KeyCode)
            {
                case Keys.Left:
                case Keys.Up:
                case Keys.Down:
                case Keys.Right:
                case Keys.Home:
                case Keys.End:
                case Keys.PageDown:
                case Keys.PageUp:
                    return true;
                default:
                    return !dataGridViewWantsInputKey;
            }
        }

        public E Value
        {
            get { return new E(); }
            set
            {
                foreach (var itm in Items)
                {
                    var ckd = (value.ToInt32(fp) & ((int)Enum.Parse(typeof(E), itm.ToString()))) > 0;
                    SetItemChecked(Items.IndexOf(itm), ckd);
                }
            }
        }

        public void PrepareEditingControlForEdit(bool selectAll) { /* Nothing */ }

        public bool RepositionEditingControlOnValueChange { get { return false; } }

        public DataGridView EditingControlDataGridView { get { return dgv; } set { dgv = value; } }

        public bool EditingControlValueChanged { get { return valueChanged; }set { valueChanged = value; } }

        public Cursor EditingPanelCursor { get { return base.Cursor; } }

        protected override void OnItemCheck(ItemCheckEventArgs e)
        {
            valueChanged = true;
            EditingControlDataGridView.NotifyCurrentCellDirty(true);
            base.OnItemCheck(e);
        }
    }

    internal class FlaggedEnumFormatProvder: IFormatProvider
    {
        public object GetFormat(Type formatType)
        {
            throw new NotImplementedException();
        }
    }
}