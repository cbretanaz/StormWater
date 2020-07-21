using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using CoP.Enterprise;
using Message = CoP.Enterprise.Message;

namespace BES.SWMM.PAC
{
    public class FileIO: IManageDataFiles
    {
        private NameValueCollection appStgs = ConfigurationManager.AppSettings;
        public event EventHandler<ValidatorEventArgs> ValidateScenario;
        private SWWMForm vw;
        public static FileIO Make(SWWMForm view)  { return new FileIO { vw= view};}
        public Scenario LoadScenario()
        {
            var oDlg = new OpenFileDialog
            {
                Title = "Select A File",
                Filter = "Xml Files (*.xml)|*.xml" + "|" +
                         "All Files (*.*)|*.*",
                CheckPathExists = true,
                CheckFileExists = true,
                DefaultExt = "xml"
            };

            if (oDlg.ShowDialog() != DialogResult.OK) return null;
            // ---------------------------------------------------
            var filSpec = oDlg.FileName;
            var serializer = new XmlSerializer(typeof(Scenario));
            using (var filStrm = new FileStream(filSpec, FileMode.Open))
            {
                var scen = (Scenario)serializer.Deserialize(filStrm);
                LoadValidator().ValidateScenarioData(scen);
                //ValidateScenario?.Invoke(this, ValidatorEventArgs.Make(scen));
                var catchmnt = scen.Catchment;
                var fclty = catchmnt.Facility;
                fclty.AboveGrade.SetSerializableProperties(
                    fclty.Category, fclty.Configuration);
                fclty.BelowGrade.SetSerializableProperties(
                    fclty.Category, fclty.Configuration);
                return scen;
            }
        }
        public void SaveScenario(Scenario scen)
        {
            var dlg = new SaveFileDialog
            {
                Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*",
                FilterIndex = 2,
                RestoreDirectory = true
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;
            using (var sw = new StringWriter())
            using (var xwrtr = XmlWriter.Create(sw))
            {
                var ser = new XmlSerializer(typeof(Scenario));
                ser.Serialize(xwrtr, scen);
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(sw.ToString());
                xmlDoc.Save(dlg.FileName);
            }
        }
        private void IgnoreMinMaxValues()
        {
            IgnoreContraints("nudBottomArea");
            IgnoreContraints("nudRockBottomArea");
            IgnoreContraints("nudOverflowSurfaceArea");
            IgnoreContraints("nudOverflowEHeight");
            IgnoreContraints("nudOverflowESurfaceArea");
            IgnoreContraints("nudRockStorageDepth");
            IgnoreContraints("nudUnderdrainHeight");
        }
        private void IgnoreContraints(string nudNm)
        {
            var nud = (NumericUpDown)(vw as ICanBeArranged).FindControl(nudNm);
            nud.Minimum = 0;
            nud.Maximum = decimal.MaxValue;
        }
        private IValidateScenarioData LoadValidator()
        {
            var assmblyNm = appStgs["CalculatorAssembly"];
            var assmSpec = Utilities.ASSEMBLYFOLDER + assmblyNm;
            Assembly dA;
            try { dA = Assembly.LoadFrom(assmSpec); }
            catch (FileNotFoundException nfX)
            {
                throw new CoPInvalidOperationException(
                    $"PacSizing Engine Assembly [{assmSpec}] cannot be loaded.", nfX);
            }
            // -------------------------------------------------
            var vadtrClassNm = appStgs["ValidatorClass"];
            if (!(dA.CreateInstance(vadtrClassNm) is IValidateScenarioData valdtr))
                throw new CoPInvalidOperationException(
                    $"Unable to instantiate {vadtrClassNm} from PAC Sizing Engine Assembly {assmblyNm}");
            return valdtr;
        }

    }

}
