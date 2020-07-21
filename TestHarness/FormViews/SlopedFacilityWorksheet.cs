using System;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.IO;
using msg = CoP.Enterprise.Message;


namespace BES.SWMM.PAC
{
  public partial class SlopedFacilityWorksheet : Form
  {
    private SlopedFacility _facility;
    private Segments segmnts;

    public SlopedFacilityWorksheet(SlopedFacility facility)
    {
      InitializeComponent();

      this._facility = facility;

      slopedFacilityBindingSource.DataSource = this._facility;
      segmentsBindingSource.DataSource = this._facility.Segments;
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
      SaveFileDialog d = new SaveFileDialog();
      d.DefaultExt = "xml";
      d.Filter = "XML (*.xml)|*.xml";

      StreamWriter writer = null;

      if (d.ShowDialog() != DialogResult.OK)
        return;
      try
      {
        string s = d.FileName;
        XmlSerializer x = new XmlSerializer(typeof(Segments));
        writer = new StreamWriter(s);
        x.Serialize(writer, _facility.Segments);
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
      finally
      {
        if (writer != null)
          writer.Close();
      }

    }

    private void btnLoad_Click(object sender, EventArgs e)
    {
        var d = new OpenFileDialog 
            {DefaultExt = "xml", Filter = "XML (*.xml)|*.xml"};
        if (d.ShowDialog() != DialogResult.OK) return;

        try
        {
            var s = d.FileName;
            var x = new XmlSerializer(typeof(Segments));
            var reader = new StreamReader(s);

            if (_facility.Segments == null)
                _facility.Segments = Segments.Empty;

            _facility.Segments.Clear();
            slopedFacilityBindingSource.ResetBindings(false);
            segmentsBindingSource.ResetBindings(false);
            _facility.Segments.AddRange((Segments) x.Deserialize(reader));

            segmnts = _facility.Segments;
            segmentsBindingSource.DataSource = segmnts; // reset the pointer

            slopedFacilityBindingSource.ResetBindings(false); // re-read all items in list and refresh displayed values
            segmentsBindingSource.ResetBindings(false);
            reader.Close();
            this.Refresh();
        }
        catch (Exception ex) { msg.Error(ex.Message); }
    }

    public Segments Segmnts => segmnts;

    public SlopedFacility Facility
    {
      get { return _facility; }
      set 
      { 
          _facility = value;
          slopedFacilityBindingSource.DataSource = _facility; // reset binding source
          segmentsBindingSource.DataSource = _facility.Segments; // reset binding source
      }
    }

    //private void btnOk_Click(object sender, EventArgs e)
    //{
    //  this.DialogResult = DialogResult.OK;
    //  segmnts = Segments.Make(facility.Segments.AsReadOnly());
    //  this.Close();
    //}

    private void dataGridView1_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
    {
        reviseCalculatedFields();
    }

    private void dataGridView1_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
    {
        reviseCalculatedFields();
    }

    private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
        reviseCalculatedFields();
    }

    private void reviseCalculatedFields()
    {
        if (_facility != null)
        {
            //txtSurfaceVolume.Text = _facility.SurfaceCapacity.ToString();
            //txtInfiltrationArea.Text = _facility.InfiltrationPercentage.ToString();
            //txtRockStorageArea.Text = facility.RockStorageBottomAreaSqFt.ToString();
            //txtRockStorageVolume.Text = facility.RockStorageCapacityCuFt.ToString();
        }
        this.Refresh();
    }
  }
}
