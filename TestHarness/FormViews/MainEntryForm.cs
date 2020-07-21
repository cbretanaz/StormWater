using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml;
using System.Xml.Serialization;
using BES.SWMM.PAC.FormViews;
using CoP.Enterprise;
using lib = CoP.Enterprise.Utilities;
using msg = CoP.Enterprise.Message;
using crsr = System.Windows.Forms.Cursor;
using System.Web.Script.Serialization;
using Newtonsoft.Json;

namespace BES.SWMM.PAC
{
    public partial class SWWMForm : Form, 
        ISetFocus, ICanBeArranged, IValidateData
    {
        #region private fields
        private bool binding { get; set; }
        private bool loading;
        private bool hasResults;
        private bool ScenarioisValid = false;
        private NameValueCollection appStgs = ConfigurationManager.AppSettings;
        #endregion private fields

        #region consts
        #region General Statics & Consts
        private static readonly string sNL = Environment.NewLine;
        private static readonly StringComparison icic = StringComparison.InvariantCultureIgnoreCase;
        private readonly Dictionary<DataControl, Control> dataCtrls = new Dictionary<DataControl, Control>();
        protected static readonly PACConfig pacCfg = PACConfig.Make();
        protected static readonly EngineSettings engCfg = pacCfg.EngineSettings;
        protected static readonly int TS = engCfg.TimeStep;
        private static readonly decimal ORFCOEFF = engCfg.OrificeCoefficient;
        private const string msgTitl = "Bureau of Environmental Services PAC Sizing";
        #endregion General Statics & Consts

        #region Configuration consts
        private const Configuration cfgA = Configuration.ConfigA;
        private const Configuration cfgB = Configuration.ConfigB;
        private const Configuration cfgC = Configuration.ConfigC;
        private const Configuration cfgD = Configuration.ConfigD;
        private const Configuration cfgE = Configuration.ConfigE;
        private const Configuration cfgF = Configuration.ConfigF;
        private const Configuration hasUndrDrn =
            Configuration.ConfigC| Configuration.ConfigD|Configuration.ConfigF;
        #endregion Configuration consts

        #region FacilityCategory Configuration consts
        private const FacilityCategory catSlpd = FacilityCategory.SlopedFacility;
        private const FacilityCategory catFlat = FacilityCategory.FlatPlanter;
        private const FacilityCategory catRect = FacilityCategory.RectBasin;
        private const FacilityCategory catAmb = FacilityCategory.AmoebaBasin;
        private const FacilityCategory catUDB = FacilityCategory.UserDefinedBasin;
        #endregion FacilityCategory Configuration consts

        #region HierarchyLevel statics/Constants
        private const HierarchyLevel h1 = HierarchyLevel.One;
        private const HierarchyLevel h2A = HierarchyLevel.TwoA;
        private const HierarchyLevel h2B = HierarchyLevel.TwoB;
        private const HierarchyLevel h2C = HierarchyLevel.TwoC;
        private const HierarchyLevel h3 = HierarchyLevel.Three;
        // ---------------------------------------------------
        private bool isHL1 => catchmnt.HierarchyLevel == h1;
        private bool isHL2A => catchmnt.HierarchyLevel == h2A;
        private bool isHL2B => catchmnt.HierarchyLevel == h2B;
        private bool isHL2C => catchmnt.HierarchyLevel == h2C;
        private bool isHL3 => catchmnt.HierarchyLevel == h3;
        #endregion HierarchyLevel Constants

        #region Point/Size consts
        private const int ctrlTop0 = 90;
        private const int ctrlTop1 = 120;
        private const int ctrlLeft = 33;
        private const int ra1 = 22;
        private const int ra2 = 44;
        private const int ra3 = 66;
        private const int ra4 = 88;
        private const int ra5 = 110;
        private const int rb0 = 0;
        private const int rb1 = 22;
        private const int rb2 = 44;
        private const int rb3 = 66;
        private const int rb4 = 88;
        private const int rb5 = 110;
        private const int rr1 = 22;
        private const int rr2 = 44;
        private const int rr3 = 66;
        private const int rr4 = 88;
        private const int rr5 = 110;
        private const int cL = 14;
        private const int cR = 180;
        private const int rawLeft = 14;
        private const int orfLeft = 163;
        //--------------------------------------------------
        private readonly Point ptUR = new Point(cR, rb0);
        private readonly Point ptTL = new Point(cL, ra1);
        private readonly Point ptML = new Point(cL, ra2);
        private readonly Point ptBL = new Point(cL, ra3);
        private readonly Point ptr4L = new Point(cL, ra4);
        private readonly Point ptr5L = new Point(cL, ra5);
        private readonly Point ptTR = new Point(cR, rb1);
        private readonly Point ptMR = new Point(cR, ra2);
        private readonly Point ptBR = new Point(cR, ra3);
        private readonly Point ptr4R = new Point(cR, ra4);
        private readonly Point ptr5R = new Point(cR, ra5);
        private readonly Point raw1 = new Point(rawLeft, rr1);
        private readonly Point raw2 = new Point(rawLeft, rr2);
        private readonly Point raw3 = new Point(rawLeft, rr3);
        private readonly Point raw4 = new Point(rawLeft, rr4);
        private readonly Point raw5 = new Point(rawLeft, rr5);
        private readonly Point orf1 = new Point(orfLeft, rb1);
        private readonly Point orf2 = new Point(orfLeft, rb2);
        private readonly Point orf3 = new Point(orfLeft, rb3);
        private readonly Point topBasinPt = new Point(ctrlLeft, ctrlTop1);
        private readonly Point topOtherPt = new Point(ctrlLeft, ctrlTop0);
        //--------------------------------------------------------------------
        private static readonly Size ctchSiz = new Size(490, 430);
        private static readonly Size fcltyMin = new Size(510, 500);
        private static readonly Size fcltyMax = new Size(550, 830);
        private static readonly Size rsltSzMin = new Size(700, 400);
        private static readonly Size rsltSzMax = new Size(1800, 1600);
        #endregion Point/Size consts
        #endregion consts

        #region Interface Implementations
        public event EventHandler<ValidatorEventArgs> ValidateScenario;
        #region ICanBeArranged/ISetFocus Controller Implementation

        private Controller ViewCntlr;
        public event EventHandler<ArrangeControlsEventArgs> ArrangeControls;

        public Control FindControl(string controlName)
        { return Controls.Find(controlName, true)[0]; }
        // -------------------------------------
        public void SetFocus(string ctrlNm)
        { Controls.Find(ctrlNm, true).First().Focus(); }
        #endregion ICanBeArranged/ISetFocus Controller Implementation
        #endregion Interface Implementations

        #region private variables
        private Scenario scenario = Scenario.Empty;
        private Catchment catchmnt { get; set; }
        private Configuration config => ((CfgCboBoxItm) cboConfiguration.SelectedItem).ConfigValue;
        private FacilityCategory category =>
            rbSlopedFacility.Checked ? catSlpd :
            rbFlatPlanter.Checked ? catFlat :
            rbBasin.Checked && rbRectShape.Checked ? catRect :
            rbBasin.Checked && rbAmoebaShape.Checked ? catAmb :
            rbBasin.Checked && rbUserDefShape.Checked ? catUDB : 
                                FacilityCategory.Null;
        private List<Chart> Charts { get; set; }
        private ChartState ChartState = ChartState.All; 
        #endregion private variables

        #region ctor/Initialization
        public SWWMForm(){InitializeForm();}
        private void InitializeForm()
        {
            InitializeComponent();
            ViewCntlr = Controller.Make(this);
            InitializeDataControlArrays();
            loading = true;
            InitializeConfigDropDown(HierarchyLevel.One, Configuration.ConfigA);
            InitializeUpDowns();
            catchmnt = InitializeScenarioCatchment();
            InitializeFacility();
            InitializeSegmentGrid();
            InitializeResultsPanel();
            InitializeCharts();
            WireUpForm();
            ChangeTabPage(0);
        }
        private void InitializeConfigDropDown(HierarchyLevel level, Configuration cfg)
        {
            var isLevelOne = level == HierarchyLevel.One;
            cboConfiguration.Items.Clear();
            cboConfiguration.Items.Add(CfgCboBoxItm.Make(cfgA, "[A] Infiltration"));
            cboConfiguration.Items.Add(CfgCboBoxItm.Make(cfgB, "[B] Infiltration with Rock Storage[RS]"));
            if (!isLevelOne)
            {
                cboConfiguration.Items.Add(CfgCboBoxItm.Make(cfgC, "[C] Infiltration with RS & UnderDrain[Ud]"));
                cboConfiguration.Items.Add(CfgCboBoxItm.Make(cfgD, "[D] Lined Facility with RS and Ud"));
            }
            cboConfiguration.Items.Add(CfgCboBoxItm.Make(cfgE, "[E] Infiltration with RS Bypass"));
            if (!isLevelOne) cboConfiguration.Items.Add(CfgCboBoxItm.Make(cfgF, "[F] Infiltration with bypass to RS and Ud"));
            cboConfiguration.ValueMember = "ConfigValue";
            cboConfiguration.DisplayMember = "Description";
            foreach (var cfgItm in cboConfiguration.Items.Cast<CfgCboBoxItm>())
            {
                if (cfgItm.ConfigValue != cfg) continue;
                cboConfiguration.SelectedIndex = cboConfiguration.Items.IndexOf(cfgItm);
                cboConfiguration.SelectedItem = cfgItm;
                return;
            }
            cboConfiguration.SelectedIndex = 0;
        }
        private void InitializeUpDowns()
        {
            foreach(Control ctrl in Controls)
                if (ctrl is NumericUpDown nud) 
                    nud.Text = string.Empty;
        }
        private void InitializeDataControlArrays()
        {
            dataCtrls.Add(DataControl.ABW, nudAvgBottomWidth);
            dataCtrls.Add(DataControl.BA, nudBottomArea);
            dataCtrls.Add(DataControl.BSD, nudBlendedSoilDepth);
            dataCtrls.Add(DataControl.BP, nudBottomPerimeter);
            dataCtrls.Add(DataControl.C2S, cbCatchmentTooSmall);
            dataCtrls.Add(DataControl.CFG, cboConfiguration);
            dataCtrls.Add(DataControl.FD, nudFreeboard);
            dataCtrls.Add(DataControl.IFR, nudInfiltrationRate);
            dataCtrls.Add(DataControl.IMPA, nudImpArea);
            dataCtrls.Add(DataControl.IP, nudInfiltrationPercent);
            dataCtrls.Add(DataControl.OFH, nudOverflowHeight);
            dataCtrls.Add(DataControl.OFA, nudOverflowSurfaceArea);
            dataCtrls.Add(DataControl.ORD, nudOrificeDiameter);
            dataCtrls.Add(DataControl.ORR, cboOrificeReason);
            dataCtrls.Add(DataControl.PreCN, nudPreTC);
            dataCtrls.Add(DataControl.PstCN, nudPostCN);
            dataCtrls.Add(DataControl.PreTC, nudPreCN);
            dataCtrls.Add(DataControl.PstTC, nudPostTC);
            dataCtrls.Add(DataControl.OFEH, nudOverflowEHeight);
            dataCtrls.Add(DataControl.OFEA, nudOverflowESurfaceArea);
            dataCtrls.Add(DataControl.RBA, nudRockBottomArea);
            dataCtrls.Add(DataControl.RP, nudPorosity);
            dataCtrls.Add(DataControl.RSD, nudRockStorageDepth);
            dataCtrls.Add(DataControl.SS, nudSideSlope);
            dataCtrls.Add(DataControl.UDH, nudUnderdrainHeight);
        }
        private void InitializeResultsPanel()
        {
            for(var row = 1; row <= 8; row++)
            for (var col = 1; col <= 6; col++)
            {
                var dta =
                    row == 1 ? "Stat" :
                    row == 2 ? "PreDev" :
                    row == 3 ? "pkInflow" :
                    row == 4 ? "pkOutFlow" :
                    row == 5 ? "PkUndrDrn" :
                    row == 6 ? "PkSrfOvrFlo" :
                    row == 7 ? "PkHead" :
                    row == 8 ? "PkSrfHead" : null;
                var dsgnStrm =
                    col == 1 ? "WQ" :
                    col == 2 ? "2Y" :
                    col == 3 ? "H2Y" :
                    col == 4 ? "5Y" :
                    col == 5 ? "10Y" :
                    col == 6 ? "25Y" : null;
                var tb = new TextBox
                {
                    Name = $"tb{dsgnStrm}.{dta}",
                    Font = new Font("Ariel", 7f, FontStyle.Regular),
                    TextAlign = row == 1 ? HorizontalAlignment.Center : HorizontalAlignment.Right,
                    Padding = new Padding(0, 1, 3, 1)
                };
                tb.DoubleClick += ExpandChart;
                tblLoPan.Controls.Add(tb, col, row);
                tb.Dock = DockStyle.Fill;
            }
        }
        private Catchment InitializeScenarioCatchment()
        {
            tbScenarioName.Text = scenario.Name = "Enter Scenario Name";
            scenario.Created = DateTime.UtcNow;
            catchmnt = scenario.Catchment ?? Catchment.Empty;
            tbCatchmentName.Text = catchmnt.Name = "Enter catchment name";
            catchmnt.HierarchyLevel = HierarchyLevel.One;
            rbLevel1.Checked = true;
            nudImpArea.Value = catchmnt.ImperviousArea = 10000m;

            catchmnt.InfiltrationTestProcedure
                = InfiltrationTestProcedure.OpenPit;
            rbOpenPit.Checked = true;
            nudInfiltrationRate.Value = catchmnt.InfiltrationRate = 1m;
            catchmnt.Status = PRStatus.Fail;
            nudPreCN.Value = catchmnt.PreCurveNumber = 72;
            nudPostCN.Value = catchmnt.PostCurveNumber = 98;
            nudPreTC.Value = catchmnt.PreTOC = 5;
            nudPostTC.Value = catchmnt.PostTOC = 5;
            // PostTOC <= PreTOC is Normal
            nudPreTC.Minimum = 5m;
            nudPreTC.Maximum = 45m;
            nudPostTC.Minimum = 5m;
            nudPostTC.Maximum = 5m;
            // -----------------------
            cboConfiguration.SelectedIndex = 0;
            chkBoxBottomFacilityMeetsReqs.Checked = catchmnt.MeetsRequirements = false;
            // ------------------------------------------------------
            rbFlatPlanter.Checked = true;
            //fclty.BlendedSoilDepth = 12m;
            //fclty.BelowGrade.RockStorageDepth = 6m;
            //nudBlendedSoilDepth.Value = catchmnt.Facility.BlendedSoilDepth.Value;
            return catchmnt;
        }
        private void InitializeFacility()
        {
            ArrangeControls?.Invoke(this,
                ArrangeControlsEventArgs.Make(
                    catchmnt.Facility.Category, cfgA));
            ChangeCategory(Configuration.ConfigA);
            InitializeOrificeReason();
            if (binding) bsFacility.DataSource = catchmnt.Facility;
            if (binding) bsAboveGrade.DataSource = catchmnt.Facility.AboveGrade;
            if (binding) bsBelowGrade.DataSource = catchmnt.Facility.BelowGrade;
            if (binding) bsOrifice.DataSource = catchmnt.Facility.BelowGrade.Orifice;
        }
        private void InitializeSegmentGrid()
        {
            var hs = new DataGridViewCellStyle
            {
                Font = new Font("Arial", 7f),
                Alignment = DataGridViewContentAlignment.BottomCenter,
                WrapMode = DataGridViewTriState.True
            };
            dgvSegments.ColumnHeadersDefaultCellStyle = hs;
            dgvSegments.ColumnHeadersHeight = 85;
            dgvSegments.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            foreach (DataGridViewColumn c in dgvSegments.Columns)
            {
                c.SortMode = DataGridViewColumnSortMode.NotSortable;
                var isDelCol = c.Name == "dgvColDelete";
                var isSegNdxCol = c.Name == "dgvColSeg";
                c.Width = isSegNdxCol? 20: isDelCol ? 24: 50;
                c.DefaultCellStyle = new DataGridViewCellStyle
                {
                    Font = new Font("Consolas", 7.0f, FontStyle.Regular),
                    Padding = new Padding(0, 0, 0, 0),
                    Alignment = 
                        isDelCol? DataGridViewContentAlignment.MiddleCenter:
                                  DataGridViewContentAlignment.MiddleRight
                };
                switch (c.Name)
                {
                    case "dgvColLength":
                        c.HeaderText = "Segmnt Length [ft]";
                        break;
                    case "dgvColCheckDamWidth":
                        c.HeaderText = "Chkdam Width [ft]";
                        break;
                    case "dgvColSlope":
                        c.HeaderText = "Long Slope [h:1v]";
                        break;
                    case "dgvColBottomWidth":
                        c.HeaderText = "Bottom Width [ft]";
                        break;
                    case "dgvColLeftSlope":
                        c.HeaderText = "Left Slope [ft]";
                        break;
                    case "dgvColRightSlope":
                        c.HeaderText = "Right Slope [ft]";
                        break;
                    case "dgvColDownstreamDepth":
                        c.HeaderText = "Dwnstrm Depth [in]";
                        break;
                    case "dgvColLandscapeWidth":
                        c.HeaderText = "Lndscap Width [ft]";
                        break;
                    case "dgvColDelete":
                        c.HeaderText = "Del Seg";
                        break;
                }
            }

            if (binding) bsSegments.DataSource =
                catchmnt.Facility is SlopedFacility facility ? 
                    facility.Segments : null;
        }
        private void InitializeCharts()
        {
            chartWQ.Location = new Point(1,1);
            chartWQ.Tag = chartH2Y.Tag = 
            chart2Y.Tag =  chart5Y.Tag = 
            chart10Y.Tag = chart25Y.Tag = false;
            Charts = new List<Chart> 
                { chartWQ , chart2Y, chartH2Y, 
                  chart5Y, chart10Y, chart25Y };
    }
        private void InitializeOrificeReason()
        {
            cboOrificeReason.Items.Clear();
            var lst = Enum.GetValues(typeof(OrificeReason)).OfType<OrificeReason>().ToList();
            foreach (var or in lst.Where(r=>r!=OrificeReason.Null && r != OrificeReason.Serialize))
                if (or != OrificeReason.Null && or != OrificeReason.Serialize)
                    cboOrificeReason.Items.Add(or);
            cboOrificeReason.SelectedItem = OrificeReason.None;
        }

        #region Wiring
        private void WireUpForm()
        {
            ArrangeControls += ViewCntlr.ArrangeControls;
            //ValidateScenario += valdtr.ValidateScenarioData(ValidateScenario);
            tabCtrl.SelectedIndexChanged += SetChangeTabPage;
            tabCtrl.Selecting += delegate(object s, TabControlCancelEventArgs e)
            { if (e.TabPageIndex == 2 && !hasResults) e.Cancel = true; };

            WireUpCatchmentElements();
            WireUpFacilityElements();
            WireNUDs();
            WireUpCharts();
            WireSegmentGrid();
            btnLoad.Click += LoadScenario;
            btnSave.Click += SaveScenario;
            btnSaveJson.Click += SaveAsJson;
            btnValidateCalculate.Click += ValidateCalculate;
            // ------------------------------------------

            Load += delegate
            {
                var fclty = catchmnt.Facility;
                BindControls(catchmnt);
                SetAboveBelowGradeBindingSource(fclty);
                ArrangeControls?.Invoke(this,
                    ArrangeControlsEventArgs.Make(
                        fclty.Category, fclty.Configuration));
                loading = false;
            };
            Resize += delegate { ResizeCharts(); };
            lblUseSugDiam.Click += delegate
            {
                var agD = catchmnt.Facility.AboveGrade;
                var bgD = catchmnt.Facility.BelowGrade;
                var pdRunOff =
                    isHL3 ? StormEvent.Make(DesignStorm.TenYear, catchmnt.ImperviousArea,
                            catchmnt.PreCurveNumber, catchmnt.PreTOC)
                                .Max(s => s.RunOff) / 448.83m:
                    isHL2C ? StormEvent.Make(DesignStorm.TwoYear, catchmnt.ImperviousArea,
                            catchmnt.PreCurveNumber, catchmnt.PreTOC)
                                .Max(s => s.RunOff) / 448.83m:
                            StormEvent.Make(DesignStorm.TwoYear, catchmnt.ImperviousArea,
                            catchmnt.PreCurveNumber, catchmnt.PreTOC)
                                .Max(s => s.RunOff) / 897.66m;

                var RcDxFt = bgD.RockStorageDepth.GetValueOrDefault(0m) / 12m;
                var udHgtFt = bgD.UnderdrainHeight.GetValueOrDefault(0m) / 12m;
                var surfDepthFt = agD.OverflowHgtFt;
                var medDepthFt = catchmnt.Facility.BlendedSoilDepth.GetValueOrDefault(0m) / 12m;
                var x = (double)(RcDxFt - udHgtFt + surfDepthFt + medDepthFt * (isHL2B ? 0.8m: isHL2C? 0.5m:  0.2m));
                var newOrfDiam =
                     24m * (decimal)Math.Sqrt((double)pdRunOff / 
                        ((double)ORFCOEFF * Math.PI * Math.Sqrt(64.4d * x)));
                var nODRnd = Math.Floor((newOrfDiam + .02m) * 8m) / 8m;
                if (nODRnd > nudOrificeDiameter.Maximum)
                    msg.Info($"The suggested diameter for the orifice is {newOrfDiam: 0.000 in}, but this is{sNL}"+
                             $"greater than the Maximum allowable orifice diameter of {nudOrificeDiameter.Maximum: 0.000 in}.{sNL}" +
                             "This facility may not require an orifice.");
                else if (nODRnd < nudOrificeDiameter.Minimum)
                {
                    var pdEvnt = isHL3 ? "ten-year": isHL2C ? "two-year": "half-two-year";
                    msg.Info($"The suggested diameter for the orifice is {newOrfDiam: 0.000 in}, but this is{sNL}" +
                             $"smaller than the Minimum allowable orifice diameter of {nudOrificeDiameter.Minimum: 0.000 in}.{sNL}" +
                             $"The impervious area may be too small to reduce outflow to the pre-development {pdEvnt} storm event.");
                }
                else nudOrificeDiameter.Value = nODRnd;
            };
        }
        private void WireSegmentGrid()
        {
            dgvSegments.CellMouseClick += ClickSegmentGridCell;
            dgvSegments.RowsAdded += AddSegments;
            dgvSegments.RowsRemoved += RemoveSegments;
            dgvSegments.CellValidating += ValidatingSideSlope;
            dgvSegments.DataError += delegate {  };
        }
        private void ValidatingSideSlope(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (!(sender is DataGridView)||
                 !(catchmnt.Facility is SlopedFacility)) return;
            var curCell = dgvSegments.CurrentCell;
            var isLeftSlp = curCell.OwningColumn.Name.Equals("dgvColLeftSlope");
            var isRightSlp = curCell.OwningColumn.Name.Equals("dgvColRightSlope");
            if (!isLeftSlp && !isRightSlp) return;

            var newVal = decimal.Parse((string)curCell.EditedFormattedValue);
            if (newVal <= 0m || newVal >= 2m) return;

            msg.Error($"The sides of sloped facility segments may be vertical{sNL}" +
                      "(Slope = zero), but if sloped, the specified slope must be at least 2.00.");
            e.Cancel = true;
            curCell.Value = 2.0m;
        }
        private void WireNUDs()
        {
            // PreTOC >= Post is normal
            // PostTOC > PreTOC is error
            WireUpAboveGradeOptions();
            WireUpBelowGradeOptions();
            foreach (var nud in panCatchmentNumbers.Controls.OfType<NumericUpDown>())
            {
                nud.ValueChanged += DataChanged;
                nud.TextChanged += DataChanged;
            }
            nudBlendedSoilDepth.ValueChanged += DataChanged;
            nudInfiltrationRate.ValueChanged += DataChanged;
            nudImpArea.ValueChanged += DataChanged;
            nudOrificeDiameter.ValueChanged += DataChanged;

            nudBlendedSoilDepth.TextChanged += DataChanged;
            nudInfiltrationRate.TextChanged += DataChanged;
            nudImpArea.TextChanged += DataChanged;
            nudOrificeDiameter.TextChanged += DataChanged;
            // --------------------------------------------
            nudOrificeDiameter.Validated += delegate
            {
                catchmnt.Facility.BelowGrade.Orifice.Diameter = 
                    nudOrificeDiameter.Value;
            };
            nudPreTC.Validating += delegate
            { ResetMaximum(nudPostTC, nudPreTC.Value); };
            nudPostTC.Validating += delegate
            { ResetMinimum(nudPreTC, nudPostTC.Value); };
            nudBottomArea.Validating += delegate
            {
                var cfg = catchmnt.Facility.Configuration;
                if (Configuration.ConfigBCEF.HasFlag(cfg))
                    ResetMaximum(nudRockBottomArea, nudBottomArea.Value / 3m);

                ResetMinimum(nudOverflowSurfaceArea,
                    config != cfgE ?
                        nudBottomArea.Value :
                        lib.Minimum(nudBottomArea.Value,
                            nudOverflowESurfaceArea.Value));
                ResetMinimum(nudOverflowESurfaceArea, nudBottomArea.Value);
                if (cfg == cfgE)
                    ResetMaximum(nudOverflowSurfaceArea, 
                        nudBottomArea.Value);
            };

            nudBlendedSoilDepth.Validating += delegate (object sender, CancelEventArgs e)
            {
                var cfg = catchmnt.Facility.Configuration;
                try
                {
                    if (cfg != cfgA)
                        ResetMaximum(nudRockStorageDepth,
                            lib.Minimum(30m,
                                nudBlendedSoilDepth.Value - 6m));
                }
                catch (ParameterOutOfRangeException)
                {
                    e.Cancel = true;
                    msg.Warn(
                        $"Maximum Rock Storage Depth must be at least 6 inches less than the Blended Soil depth [{nudBlendedSoilDepth.Value: 0 in}].{sNL}" +
                        "Therefore Blended Soil depth may not be set to less than 6 inches.", msgTitl);
                }
            };
            nudRockStorageDepth.Validating += delegate (object sender, CancelEventArgs e)
            {
                var cfg = catchmnt.Facility.Configuration;
                if (!hasUndrDrn.HasFlag(cfg)) return;
                try
                {
                    if (Configuration.ConfigCDF.HasFlag(cfg))
                        ResetMaximum(nudUnderdrainHeight,
                            nudRockStorageDepth.Value - 6m);
                }
                catch (ParameterOutOfRangeException)
                {
                    e.Cancel = true; 
                    msg.Warn(
                    $"Maximum Underdrain Height cannot be higher than 6 inches below the Rock.{sNL}" +
                        "Therefore Rock Storage Depth may not be set lower than 6 inches.", msgTitl);
                }
            };
            nudOverflowESurfaceArea.Validating += delegate
            {
                var fclty = catchmnt.Facility;
                if (catchmnt.Facility.Configuration == cfgE && fclty is UserDefinedBasin)
                    ResetMinimum(nudOverflowSurfaceArea,
                        lib.Minimum(nudBottomArea.Value,
                            nudOverflowESurfaceArea.Value));
            };
            nudOverflowSurfaceArea.Validating += delegate
            {
                var fclty = catchmnt.Facility;
                if (catchmnt.Facility.Configuration != cfgE) return;
                if(fclty is UserDefinedBasin)
                    ResetMaximum(nudOverflowESurfaceArea, nudOverflowSurfaceArea.Value);
                ResetMinimum(nudBottomArea, nudOverflowESurfaceArea.Value);
            };
            nudOverflowHeight.Validating += delegate
            {
                if (catchmnt.Facility.Configuration == cfgE) 
                    ResetMaximum(nudOverflowEHeight, 
                    nudOverflowHeight.Value);
            };
        }
        private void WireUpCatchmentElements()
        {
            chkPublic.Click += delegate { ChangePublic(); };
            chkBoxBottomFacilityMeetsReqs.CheckedChanged += DataChanged;
            foreach (var ctrl in gbIFTs.Controls.OfType<RadioButton>())
                ctrl.CheckedChanged += DataChanged;
            foreach (var rb in gbLevels.Controls.OfType<RadioButton>())
            {
                rb.CheckedChanged += ChangeLevel;
                rb.CheckedChanged += DataChanged;
            }
        }
        private void WireUpFacilityElements()
        {
            WireUpCategoryOptions();
            WireUpShapeOptions();
            foreach (var rb in gbCategory.Controls.OfType<RadioButton>())
            {
                rb.CheckedChanged += ChangeCategory;
                rb.CheckedChanged += DataChanged;
            }
            btnSetStdValues.Click += SetStdRockStorageValues;
            chkHasOrifice.CheckedChanged += ChangeHasOrifice;
            cboOrificeReason.SelectedIndexChanged += ChangeOrificeReason;
            cboConfiguration.SelectedIndexChanged += ChangeConfiguration;
            chkHasOrifice.CheckedChanged += DataChanged;
            cboOrificeReason.SelectedIndexChanged += DataChanged;
        }
        private void WireUpCategoryOptions()
        {
            foreach (var rb in gbCategory.Controls.OfType<RadioButton>())
            {
                rb.CheckedChanged += ChangeCategory;
                rb.CheckedChanged += DataChanged;
            }
        }
        private void WireUpShapeOptions()
        {
            foreach (var rb in gbShape.Controls.OfType<RadioButton>())
            {
                rb.CheckedChanged += ChangeShape;
                rb.CheckedChanged += DataChanged;
            }
        }
        private void WireUpAboveGradeOptions()
        {
            foreach (var pan in panAboveGrade.Controls.OfType<Panel>())
            foreach (var nud in pan.Controls.OfType<NumericUpDown>())
            {
                nud.ValueChanged += DataChanged;
                nud.TextChanged += DataChanged;
            }
        }
        private void WireUpBelowGradeOptions()
        {
            foreach (var pan in panBelowGrade.Controls.OfType<Panel>())
            foreach (var nud in pan.Controls.OfType<NumericUpDown>())
            {
                nud.ValueChanged += DataChanged;
                nud.TextChanged += DataChanged;
            }
        }
        private void WireUpCharts()
        {
            foreach (var chrt in panCharts.Controls.OfType<Chart>())
                chrt.DoubleClick  += ExpandChart;
            tsmiWQ.MouseDown += DisplayFailures;
            tsmi2Y.MouseDown += DisplayFailures;
            tsmiH2Y.MouseDown += DisplayFailures;
            tsmi5Y.MouseDown += DisplayFailures;
            tsmi10Y.MouseDown += DisplayFailures;
            tsmi25Y.MouseDown += DisplayFailures;
        }
        #endregion Wiring

        #endregion ctor/Initialization

        #region events
        #region General FormEvents
        private void SetChangeTabPage(object sender, EventArgs e) 
        { ChangeTabPage(tabCtrl.SelectedIndex); }
        private void ChangeTabPage(int tabNdx)
        {
            switch (tabNdx)
            {
                case 0: // Catchment
                    //tabCtrl.Size = new Size(400, 300);
                    MinimumSize = ctchSiz;
                    Size = new Size(490, 430);
                    tabCtrl.Size = new Size(445, 295);
                    MaximumSize = ctchSiz;
                    FormBorderStyle = FormBorderStyle.Fixed3D;
                    break;

                case 1: // Facility
                    FormBorderStyle = FormBorderStyle.Sizable;
                    var isSloped = catchmnt.Facility.IsSloped;
                    if (!rbSlopedFacility.Checked && !rbFlatPlanter.Checked && !rbFlatPlanter.Checked)
                    {
                        rbSlopedFacility.Checked = catchmnt.Facility.IsSloped;
                        rbFlatPlanter.Checked = catchmnt.Facility.IsFlatPlanter;
                        rbBasin.Checked = catchmnt.Facility.IsBasin;
                    }
                    MinimumSize = fcltyMin;
                    Size = new Size(500, 470);
                    MaximumSize = fcltyMax;
                    var segVis = panSegments.Visible;
                    if (segVis)
                    {
                        panSegments.Top = panBelowGrade.Top + panBelowGrade.Height + 15;
                        panSegments.Height = tbPgFacility.Height - (panSegments.Top + 30);
                    }
                    var hgt = segVis ? panSegments.Top + panSegments.Height:
                                        panBelowGrade.Top + panBelowGrade.Height;
                    Size = isSloped? SlopedFacilityTabPageSize:
                            new Size( 445, hgt + 290);
                    break;

                case 2: // Results
                    MinimumSize = rsltSzMin;
                    MaximumSize = rsltSzMax;
                    Size = new Size(900, 700);
                    FormBorderStyle = FormBorderStyle.Sizable;
                    break;
            }
        }

        private void ClickSegmentGridCell(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (!(sender is DataGridView) ||
                e.Button != MouseButtons.Left ||
                dgvSegments.Columns[e.ColumnIndex].Name != "dgvColDelete" ||
                !(catchmnt.Facility is SlopedFacility)) return;
            var roNdx = e.RowIndex;
            var seg =  dgvSegments.Rows[roNdx].DataBoundItem as Segment;

            var segs = ((SlopedFacility) catchmnt.Facility).Segments;
            segs.Remove(seg);
        }
        private void AddSegments(object sender, DataGridViewRowsAddedEventArgs e) { ResetSegmentIndices(); }
        private void RemoveSegments(object sender, DataGridViewRowsRemovedEventArgs e) { ResetSegmentIndices(); }
        private void LoadScenario(object sender, EventArgs e) { LoadScenario(); }
        private void SaveScenario(object sender, EventArgs e) { SaveScenario(); }
        private void SaveAsJson(object sender, EventArgs e) { SaveResults(ExportFmt.Json); }
        private void ValidateCalculate(object sender, EventArgs e)
        {
            if (ScenarioisValid) Calculate();
            else ValidateScenarioData();
        }
        private void DataChanged(object sender, EventArgs e)
        {
            ResetValidateCalculateButton();
            CleanResults();
            BindAndDisplayCharts(HierarchyLevel.Null,   null);
            tabCtrl.TabPages[2].Text = "No Results";
            tabCtrl.TabPages[2].Enabled = hasResults = false;
        }
        #endregion General FormEvents

        #region Catchment/Facility Property Events
        private void ChangeLevel(object sender, EventArgs e)
        {
            if (!(sender is RadioButton)) return;
            var chngd2On = (RadioButton) sender;
            if (!chngd2On.Checked) return;
            var lvl =
                rbLevel1.Checked ? HierarchyLevel.One :
                rbLevel2a.Checked ? HierarchyLevel.TwoA :
                rbLevel2b.Checked ? HierarchyLevel.TwoB :
                rbLevel2c.Checked ? HierarchyLevel.TwoC :
                rbLevel3.Checked ? HierarchyLevel.Three :
                                   HierarchyLevel.Null;
            InitializeConfigDropDown(lvl, catchmnt.Facility.Configuration);
        }
        private void ChangeCategory(object sender, EventArgs e)
        {
            if (sender is RadioButton clkd && !clkd.Checked) return;
            ChangeCategory(config);
        }
        private void ChangeConfiguration(object sender, EventArgs e)
        {
            var fclty = catchmnt.Facility;
            var cfg = config;
            fclty.Configuration = cfg;
            fclty.UpdateSerializeProperties(cfg);
            if (Configuration.NeedsOrf.HasFlag(cfg) && fclty.BelowGrade.Orifice == null)
                fclty.BelowGrade.Orifice = Orifice.Default;
            if (!Configuration.NeedsOrf.HasFlag(cfg) && fclty.BelowGrade.Orifice != null)
                fclty.BelowGrade.Orifice = null;
            var nud = nudBlendedSoilDepth;
            if (cfg == cfgA)
            {
                nud.Minimum = 1;
                nud.Maximum = 12;
                if (!loading && nud.Value > 12) nud.Value = 12;
            }
            else
            {
                nud.Maximum = 36;
                nud.Minimum = 12;
                if (!loading && nud.Value < 12) nud.Value = 12;
            }
            ArrangeControls?.Invoke(this,
                ArrangeControlsEventArgs.Make(category, cfg));

            //ArrangeControls(category, cfg);
            nudRockStorageDepth.Minimum = cfg == cfgB ? 1 : 6;
            //nudRockStorageDepth.Maximum = lib.Minimum(30m, nudBlendedSoilDepth.Value - 6m);
            nudRockStorageDepth.Maximum = lib.Minimum(30m, 
                fclty.BlendedSoilDepth.GetValueOrDefault(24m) - 6m);
            var rsD = fclty.BelowGrade.RockStorageDepth.GetValueOrDefault(12m);
            if (loading) return;
            if (rsD < nudRockStorageDepth.Minimum)
                fclty.BelowGrade.RockStorageDepth = nudRockStorageDepth.Minimum;
            else if (rsD > nudRockStorageDepth.Maximum)
                fclty.BelowGrade.RockStorageDepth = nudRockStorageDepth.Maximum;
            else fclty.BelowGrade.RockStorageDepth = rsD;
        }
        private void ChangeShape(object sender, EventArgs e) { ChangeShape(sender); }
        private void SetStdRockStorageValues(object sender, EventArgs e)
        {
            var fclty = catchmnt.Facility;
            var isSloped = catchmnt.Facility.IsSloped;
            var slpd = isSloped? fclty as SlopedFacility: null;
            if (isSloped && slpd.Segments.Count == 0)
            {
                msg.Warn(
                    "Cannot calculate standard Rock Gallery Dimensions until Segment data has been entered.");
                return;
            }
            var cfg = fclty.Configuration;
            var isCfgA = cfg == cfgA;
            var agD = fclty.AboveGrade;
            var bgD = fclty.BelowGrade;
            var bW = agD.BottomWidth;
            var bA = agD.BottomArea.GetValueOrDefault(0m);
            var bLen = bW.HasValue && bW > 0m? bA / bW.Value: 0m;
            // -------------------------------------------------
            bgD.RockWidth = lib.Minimum(3m, 
                isSloped? slpd.Segments.LastSegment.BottomWidth: agD.BottomWidth.Value);  //nudRockWidth.Value = 3m;
            bgD.RockBottomArea =
                isSloped ? lib.Minimum(slpd.Segments.LastSegment.Area,
                        slpd.Segments.Sum(s => s.Area) / 4m) :
                cfg == cfgA || !bW.HasValue ?  bA / 4m:
                            nudRockWidth.Value * bLen / 4m;
        }
        
        #region Orifice Events
        private void ChangeHasOrifice(object sender, EventArgs e)
        {
            var orf = catchmnt.Facility.BelowGrade.Orifice;
            orf.HasOrifice = chkHasOrifice.Checked;
            if (chkHasOrifice.Checked)
            {
                cboOrificeReason.SelectedItem = OrificeReason.None;
                cboOrificeReason.Enabled = false;
                nudOrificeDiameter.Enabled = true;
            }
            else
            {
                nudOrificeDiameter.Text = string.Empty;
                orf.Diameter = null;
                nudOrificeDiameter.Enabled = false;
                cboOrificeReason.Enabled = true;
                cboOrificeReason.SelectedValue = OrificeReason.None;
            }
        }
        private void ChangeOrificeReason(object sender, EventArgs e)
        {
            var orf = catchmnt.Facility.BelowGrade.Orifice;
            var hasOrf = (OrificeReason)cboOrificeReason.SelectedItem == OrificeReason.None;
            nudOrificeDiameter.Enabled = chkHasOrifice.Checked = hasOrf;
            cboOrificeReason.Enabled = !hasOrf;

            orf.Reason = hasOrf ? OrificeReason.None :
                (OrificeReason)cboOrificeReason.SelectedItem;
        }
        #endregion Orifice Events

        #endregion Catchment/Facility Property Events

        #region ExpandCharts Events
        private void ExpandChart(object sender, EventArgs e)
        {
            if (sender is Chart chrt) ExpandChart(chrt, true);
            else if (sender is TextBox)
            {
                var cell = GetTableLayoutPanelRowCol(tblLoPan,
                    tblLoPan.PointToClient(crsr.Position));
                switch (cell.col)
                {
                    case 1: ExpandChart(chartWQ); break;
                    case 2: ExpandChart(chart2Y); break;
                    case 3: ExpandChart(chartH2Y); break;
                    case 4: ExpandChart(chart5Y); break;
                    case 5: ExpandChart(chart10Y); break;
                    case 6: ExpandChart(chart25Y); break;
                }
            }
        }
        private void ExpandChart(Control chrt, bool switchState = false)
        {
            if (ChartState == ChartState.All)
            {
                ChartState = ChartState.Single;
                chrt.Dock = DockStyle.Fill;
                foreach (var othr in Charts
                    .Where(othr => !othr.Equals(chrt)))
                    othr.Visible = false;
                lblExpandMsg.Text = $"Double click Chart{sNL}to Show all charts";
            }
            else if (switchState)
            {
                ChartState = ChartState.All;
                chrt.Dock = DockStyle.None;
                ResizeCharts();
                lblExpandMsg.Text = $"Double click Chart{sNL}to Expand Chart";
            }
            else
            {
                foreach (var othr in Charts.Where(o =>
                    o.Dock == DockStyle.Fill &&
                    !o.Equals(chrt)))
                {
                    othr.Dock = DockStyle.None;
                    othr.Visible = false;
                }
                chrt.Visible = true;
                chrt.Dock = DockStyle.Fill;
            }
        }
        #endregion ExpandCharts Events
        #endregion events

        #region BindControls
        private void BindControls(Scenario scen, bool suspend = true)
        {
            if (binding) return;
            try
            {
                if(suspend) bsScenario.SuspendBinding();
                binding = true;
                if (!bsScenario.DataSource.Equals(scen))
                    bsScenario.DataSource = scen;
                BindControls(scen.Catchment, suspend);
            }
            finally
            {
                bsScenario.ResumeBinding();
                binding = false;
            }
        }
        private void BindControls(Catchment ctch, bool suspend = true)
        {
            if (!bsCatchment.DataSource.Equals(ctch))
                ReBind(bsCatchment, ctch, suspend);
            BindControls(ctch.Facility, suspend);
        }
        private void BindControls(Facility fclty, bool suspend = true)
        {
            var cfg = fclty.Configuration;
            if (!bsFacility.DataSource.Equals(fclty))
                ReBind(bsFacility, fclty, suspend);

            if (!bsAboveGrade.DataSource.Equals(fclty.AboveGrade))
                ReBind(bsAboveGrade, fclty.AboveGrade, suspend);

            if (!bsBelowGrade.DataSource.Equals(fclty.BelowGrade))
                ReBind(bsBelowGrade, fclty.BelowGrade, suspend);

            if (Configuration.NeedsOrf.HasFlag(cfg) &&
                !bsOrifice.DataSource.Equals(fclty.BelowGrade.Orifice))
                ReBind(bsOrifice, fclty.BelowGrade.Orifice, suspend);

            if (fclty is SlopedFacility slpF &&
                (bsSegments.DataSource == null || !bsSegments.DataSource.Equals(slpF.Segments)))
                ReBind(bsSegments, slpF.Segments, suspend);
        }
        private void SetAboveBelowGradeBindingSource(Facility fclty)
        {
            if (binding) return;
            try
            {
                binding = true;
                bsAboveGrade.DataSource =
                    fclty is SlopedFacility ? AboveGradeProperties.Default :
                    fclty is FlatPlanter fltPlanterA ? fltPlanterA.AboveGrade :
                    fclty is RectangularBasin rBasinA ? rBasinA.AboveGrade :
                    fclty is AmoebaBasin aBasinA ? aBasinA?.AboveGrade :
                    fclty is UserDefinedBasin udBasinA ? udBasinA.AboveGrade : null;
                bsBelowGrade.DataSource =
                    fclty is SlopedFacility slpdB ? slpdB.BelowGrade :
                    fclty is FlatPlanter fltPlanterB ? fltPlanterB.BelowGrade :
                    fclty is RectangularBasin rBasinB ? rBasinB.BelowGrade :
                    fclty is AmoebaBasin aBasinB ? aBasinB?.BelowGrade :
                    fclty is UserDefinedBasin udBasinB ? udBasinB.BelowGrade : null;
            }
            finally
            {
                binding = false;
            }
        }
        private void ReBind(BindingSource bs, object dtaSrc, bool suspend = true)
        {
            try
            {
                if (suspend) bs.SuspendBinding();
                bs.DataSource = dtaSrc;
            }
            finally { bs.ResumeBinding(); }
        }
        #endregion BindControls

        #region File I/O

        private void LoadScenario()
        {
            var filIO = FileIO.Make(this);
            try
            {
                var scen = filIO.LoadScenario();
                IgnoreMinMaxValues();
                scenario = scen;
                catchmnt = scenario.Catchment;
                var fclty = catchmnt.Facility;
                ResumeLayout();
                Refresh();
                ReSetMinMaxValues(catchmnt);
                ArrangeControls?.Invoke(this,
                    ArrangeControlsEventArgs.Make(
                        fclty.Category, fclty.Configuration));
                DisplayValidCalcButton(ScenarioisValid = true);
                ShowValidFailMesage(ScenData.Valid);
                BindControls(scenario, true);
                PaintControls(catchmnt.Facility);
                if (fclty.IsSlopedFacility)
                    Size = SlopedFacilityTabPageSize;
                Refresh();
                scenario.ActualResults = null;
                hasResults = false;
                DisplayValidCalcButton(ScenarioisValid = false);
                tabCtrl.SelectedIndex = 1;
            }
            catch (ValidationException valX)
            {
                msg.Error(
                    $"Could not load Scenario {valX.Message}.",
                    msgTitl);
            }
        }

        private void SaveScenario()
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
                ser.Serialize(xwrtr, scenario);
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(sw.ToString());
                xmlDoc.Save(dlg.FileName);
            }
        }
        private void SaveResults(ExportFmt xFmt)
        {
            JsonSerializer serializer = new JsonSerializer();



            var isXml = xFmt == ExportFmt.Xml;
            var isJsn = xFmt == ExportFmt.Json;



            var dlg = new SaveFileDialog
            {
                Filter =
                    isXml ? "xml files (*.xml)|*.xml|All files (*.*)|*.*":
                    isJsn ? "Json files (*.json)|*.json|All files (*.*)|*.*" : 
                                            "All files(*.*) | *.*",
                FilterIndex = 2,
                RestoreDirectory = true
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;
            using (var sw = new StringWriter())
                if (isXml)
                {
                    using (var xwrtr = XmlWriter.Create(sw))
                    {
                        var xmlSer = new XmlSerializer(typeof(Scenario));
                        xmlSer.Serialize(xwrtr, scenario);
                        var xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(sw.ToString());
                        xmlDoc.Save(dlg.FileName);
                    }
                }
                else if (isJsn)
                {
                    var jsnSer = new JavaScriptSerializer();
                    var scenText = jsnSer.Serialize(scenario);
                    File.WriteAllText(dlg.FileName, scenText);
                }
        }
        #endregion File I/O

        #region Display/Paint Screens
        private void PaintControls(Facility fclty)
        {
            try
            {
                loading = true;
                PaintCatShapeConfiguration(fclty);
            }
            finally  { loading = false; }
        }
        private void PaintCatShapeConfiguration(Facility fclty)
        {
            rbSlopedFacility.Checked = fclty.IsSlopedFacility;
            rbFlatPlanter.Checked = fclty.IsFlatPlanter;
            rbBasin.Checked = fclty.IsBasin;
            // --------------------------------------
            rbRectShape.Checked = fclty.isRectShape;
            rbAmoebaShape.Checked = fclty.isAmoebaShape;
            rbUserDefShape.Checked = fclty.isUserDefShape;
            // --------------------------------------
            var cfg = fclty.Configuration;
            foreach (CfgCboBoxItm itm in cboConfiguration.Items)
                if (itm.ConfigValue.Equals(cfg))
                {
                    cboConfiguration.SelectedItem = itm;
                    break;
                }
        }

        private void ResetSegmentIndices()
        {
            if (!(catchmnt.Facility is SlopedFacility)) return;
            // --------------------------------------------------
            var segs = (catchmnt.Facility as SlopedFacility).Segments;
            var ndx = 1;
            foreach (var seg in segs)  seg.Index = ndx++;
        }
        private static void DisplayFailures(object sender, MouseEventArgs e)
        {
            var fs = (sender as ToolStripMenuItem)?.Tag as Failures;
            var fFrm = new FailuresForm(fs);
            fFrm.Show();
        }
        private void ResizeCharts()
        {
            if (ChartState == ChartState.Single) return;
            var bgDta = catchmnt.Facility.BelowGrade;
            var cfg = catchmnt.Facility.Configuration;
            var orf = catchmnt.Facility.BelowGrade.Orifice;
            var noOrfCtchTooSmall =
               Configuration.NeedsOrf.HasFlag(cfg) &&
              !orf.HasOrifice && orf.Reason == OrificeReason.CatchTooSmall ||
               bgDta.CatchmentTooSmall.GetValueOrDefault(false);
            // --------------------------------------
            foreach (var chrt in Charts)
                chrt.Visible = ShowChart(chrt, 
                    catchmnt.HierarchyLevel,
                    noOrfCtchTooSmall);
            // -----------------------------------
            var visChrts = Charts.Where(c => c.Visible).ToList();
            var cnt = visChrts.Count;
            if (cnt == 0) return;
            var rows = (cnt + 1) / 2;
            var chrtWid = 
                cnt == 1? panCharts.Width - 10: 
                          panCharts.Width / 2 - 10;
            var chrtHgt = panCharts.Height / rows - 10;
            var chrtSiz = new Size(chrtWid, chrtHgt);
            var locs = new Dictionary<int, Point>();
            for (var i = 0; i < cnt; i++)
            {
                var x = i%2==0? 1: panCharts.Width / 2 - 10;
                var y = i / 2 * chrtHgt + 1;
                locs.Add(i, new Point(x, y));
            }

            var n = 0;
            foreach (var chrt in visChrts)
            {
                chrt.Size = chrtSiz;
                chrt.Location = locs[n++];
            }
        }
        
        private Size SlopedFacilityTabPageSize => new Size(500, 
            panSegments.Top + panSegments.Height + 240); 
        #endregion Display/Paint Screens

        #region Calculate/Display Results
        private void Calculate()
        {
            if (!ScenarioisValid) return;
            // ---------------------------
            var needsOrf = catchmnt.Facility.NeedsOrifice;
            var orf = needsOrf ? catchmnt.Facility.BelowGrade.Orifice : null;
            var noOrfCtch2Small = 
               needsOrf && !orf.HasOrifice && 
              orf.Reason == OrificeReason.CatchTooSmall ||
              catchmnt.Facility.BelowGrade.xmlCatchmentTooSmall; 
            var calc = LoadCalulator(catchmnt.Facility.Category);
            try
            {
                var hL = catchmnt.HierarchyLevel;
                var reslts = scenario.ActualResults = calc.CalculateResults(catchmnt);
                DisplayStormResults(hL, reslts, noOrfCtch2Small);
                BindAndDisplayCharts(hL, reslts, noOrfCtch2Small);
                tabCtrl.TabPages[2].Enabled = hasResults = true;
                tabCtrl.TabPages[2].Text = "Test Results";
                tabCtrl.SelectedIndex = 2;
            }
            catch (PACSizingException ex)
            { msg.Error(ex.Message, "Bureau of Environmental Services"); }
        }
        private void DisplayStormResults(HierarchyLevel hl, 
           StormResults rslts, bool ctch2Small = false)
        { foreach (var sr in rslts) DisplayStormResult(hl, sr, ctch2Small); }
        private void CleanResults()
        {
            var srs = StormResults.EmptyPending;
            DisplayStormResults(HierarchyLevel.Null, srs);
        }

        private void DisplayStormResult(HierarchyLevel hl, 
            StormResult sr, bool ctch2Small = false)
        {
            const int stat = 1;
            const int pdevRO = 2;
            const int inFlo = 3;
            const int outFlo = 4;
            const int UndrDrn = 5;
            const int srfOvrFlo = 6;
            const int pkHd = 7;
            const int pkSrfHd = 8;
            var isH2YDS = sr.DesignStorm == DesignStorm.HalfTwoYear;

            var showResults =
                    isHL1 && DesignStorm.H1.HasFlag(sr.DesignStorm) ||
                    isHL2A && DesignStorm.H2A.HasFlag(sr.DesignStorm) ||
                    isHL3 && DesignStorm.H3.HasFlag(sr.DesignStorm) ||

                    ctch2Small && (isHL2B || isHL2C) && 
                        DesignStorm.C2S.HasFlag(sr.DesignStorm) ||

                    isHL2B && DesignStorm.H2B.HasFlag(sr.DesignStorm) ||
                    isHL2C && DesignStorm.H2C.HasFlag(sr.DesignStorm);

            var pend = sr.Status == PRStatus.Pending;
            var colNdx =
                sr.DesignStorm == DesignStorm.WQ ? 1 :
                sr.DesignStorm == DesignStorm.TwoYear ? 2 :
                sr.DesignStorm == DesignStorm.HalfTwoYear ? 3 :
                sr.DesignStorm == DesignStorm.FivYear ? 4 :
                sr.DesignStorm == DesignStorm.TenYear ? 5 :
                sr.DesignStorm == DesignStorm.TwntyFiv ? 6 : 0;
            var passed = sr.Status == PRStatus.Pass;
            var failed = sr.Status == PRStatus.Fail;
            var justRq = sr.Status == PRStatus.JustificationRequired;
            var statTB = tblLoPan.GetControlFromPosition(colNdx, stat);

            statTB.Text = showResults ? StatusString(sr.Status): "NA";
            statTB.ForeColor =
                !showResults? Color.Black:
                sr.Status.Equals(PRStatus.Pass) ? Color.DarkGreen :
                sr.Status.Equals(PRStatus.JustificationRequired) ? Color.LimeGreen :
                sr.Status.Equals(PRStatus.Fail) ? Color.Red : 
                                                     Color.SlateGray;

            var tsmi =
                !showResults || !failed ? null:
                sr.DesignStorm == DesignStorm.WQ ? tsmiWQ :
                sr.DesignStorm == DesignStorm.TwoYear ? tsmi2Y :
                sr.DesignStorm == DesignStorm.HalfTwoYear ? tsmiH2Y :
                sr.DesignStorm == DesignStorm.FivYear ? tsmi5Y :
                sr.DesignStorm == DesignStorm.TenYear ? tsmi10Y :
                sr.DesignStorm == DesignStorm.TwntyFiv ? tsmi25Y : null;

            statTB.ContextMenuStrip =
                !showResults || !failed ? new ContextMenuStrip() :
                sr.DesignStorm == DesignStorm.WQ ? ctxMnuStrpWQ :
                sr.DesignStorm == DesignStorm.TwoYear ? ctxMnuStrp2Y :
                sr.DesignStorm == DesignStorm.HalfTwoYear ? ctxMnuStrpH2Y :
                sr.DesignStorm == DesignStorm.FivYear ? ctxMnuStrp5Y :
                sr.DesignStorm == DesignStorm.TenYear ? ctxMnuStrp10Y :
                sr.DesignStorm == DesignStorm.TwntyFiv ? ctxMnuStrp25Y : 
                                                    new ContextMenuStrip();
            if (tsmi != null)
            {
                tsmi.Tag = !passed ? sr.Failures : null;
                tsmi.Enabled = showResults && failed;
                tsmi.Text = showResults && failed ? $"&Show {sr.DesignStorm.Name()} Failures" : null;
            }
            DisplayResultMetric(colNdx, pdevRO, sr.PeakPreDevRunoff / (isH2YDS ? 2m: 1m), showResults);
            DisplayResultMetric(colNdx, inFlo, sr.PeakInflow, showResults);
            DisplayResultMetric(colNdx, outFlo, sr.PeakOutflow, showResults);
            DisplayResultMetric(colNdx, UndrDrn, sr.PeakUnderdrain, showResults);
            DisplayResultMetric(colNdx, srfOvrFlo, sr.PeakSurfaceOverFlow, showResults);
            DisplayResultMetric(colNdx, pkHd, sr.PeakHead, showResults, true);
            DisplayResultMetric(colNdx, pkSrfHd, sr.PeakSurfaceHead, showResults, true);
        }
        private void DisplayResultMetric(int colNdx, int roNdx, decimal value, bool? show, bool isDepth = false)
        {
            var ctrl = (TextBox)tblLoPan.GetControlFromPosition(colNdx, roNdx);
            if (isDepth) ctrl.Text = show == null? string.Empty: 
                show.Value? value.ToString("0.00 'ft'"): "NA";
            else ctrl.Text = show == null ? string.Empty : 
                show.Value ? $"{value / TS: 0.0000 cfs} / " +
                             $"{value / TS * 448.83333m: 0.000 GPM}": "NA";

        }
        private void BindAndDisplayCharts(
            HierarchyLevel hL, StormResults rslts,
            bool disp25YrRslts = true, bool ctch2Small = false)
        {
            chart25Y.Series["PreDev"].LegendText = 
                hL == HierarchyLevel.Three?
                 "10Yr PreDev Runoff": "PreDev Runoff";
            BindChart(chartWQ, rslts?[DesignStorm.WQ].Timesteps, hL);
            BindChart(chart2Y, rslts?[DesignStorm.TwoYear].Timesteps, hL, ctch2Small);
            BindChart(chartH2Y, rslts?[DesignStorm.HalfTwoYear].Timesteps, hL, ctch2Small);
            BindChart(chart5Y, rslts?[DesignStorm.FivYear].Timesteps, hL, ctch2Small);
            BindChart(chart10Y, rslts?[DesignStorm.TenYear].Timesteps, hL, ctch2Small);
            BindChart(chart25Y, rslts?[DesignStorm.TwntyFiv].Timesteps, hL, ctch2Small);
        }
        private void BindChart(Chart chrt, IEnumerable ts, 
            HierarchyLevel hL, bool ctch2Small = false)
        {
            chrt.DataSource = ts;
            chrt.Visible = ShowChart(chrt, hL, ctch2Small);
        }
        private void ResetValidateCalculateButton()
        {
            DisplayValidCalcButton(ScenarioisValid = false);
            ShowValidFailMesage(ScenData.Pend);
        }
        private bool ShowChart(IDisposable chrt, HierarchyLevel hL, bool ctch2Small = false )
        {
            switch (hL)
            {
                case HierarchyLevel.One:
                    return ReferenceEquals(chrt, chartWQ) ||
                           ReferenceEquals(chrt, chart10Y);

                case HierarchyLevel.TwoA:
                    return ReferenceEquals(chrt, chartWQ);

                case HierarchyLevel.TwoB:
                    return ctch2Small? ReferenceEquals(chrt, chartWQ) || 
                                       ReferenceEquals(chrt, chart25Y):
                            !ReferenceEquals(chrt, chart2Y);

                case HierarchyLevel.TwoC:
                    return ctch2Small? ReferenceEquals(chrt, chartWQ) ||
                                       ReferenceEquals(chrt, chart25Y):
                            !ReferenceEquals(chrt, chartH2Y);

                case HierarchyLevel.Three:
                    return ReferenceEquals(chrt, chart25Y);
            }
            return false;
        }
        #endregion Calculate/Display Results

        #region Change Catchment/Facility properties
        private void ChangePublic()
        {
            var isPublic = chkPublic.Checked;
            var ovHgt = nudOverflowHeight.Value;
            var min = isPublic ? 1 : 6;
            var max = isPublic ? 9 : 18;
            if (ovHgt < min) ovHgt = min;
            if (ovHgt > max) ovHgt = max;
            nudOverflowHeight.Minimum = min;
            nudOverflowHeight.Maximum = max ;
        }
        private void ChangeCategory(Configuration config)
        {
            var currfclty = catchmnt.Facility;
            // -----------------------------------------------------
            gbShape.Visible = rbBasin.Checked;
            panConfig.Top = rbBasin.Checked ? 75 : 39;
            panFacilityTop.Height = rbBasin.Checked ? 105 : 73;
            if (rbSlopedFacility.Checked)
            {
                catchmnt.Facility = SlopedFacility.Make(currfclty, config);
                rbUserDefShape.Enabled = rbAmoebaShape.Enabled = false;
            }
            else if (rbFlatPlanter.Checked)
            {
                catchmnt.Facility = FlatPlanter.Make(currfclty, config);
                rbUserDefShape.Enabled = rbAmoebaShape.Enabled = false;
            }
            else if (rbBasin.Checked)
            {
                rbAmoebaShape.Enabled = rbUserDefShape.Enabled = rbRectShape.Enabled = true;
                rbRectShape.Checked = true;
                catchmnt.Facility = RectangularBasin.Make(currfclty, config);
            }

            var newFclty = catchmnt.Facility;
            if (newFclty == null) throw new CoPInvalidDataException(
                "Facility must not be null");
            ArrangeControls?.Invoke(this,
                ArrangeControlsEventArgs.Make(
                    newFclty.Category, newFclty.Configuration));
            ScenarioisValid = false;
            DisplayValidCalcButton(null);
            BindControls(newFclty, false);
        }
        private void ChangeShape(object obj)
        {
            if (!(obj is RadioButton)) throw new InvalidOperationException("Not a radio Button");
            var rb = (RadioButton) obj;
            if (!rb.Checked) return;
            // ----------------------------------------------
            var currfclty = catchmnt.Facility;
            var newFclty =
                rb == rbRectShape ? RectangularBasin.Make(currfclty, config) :
                rb == rbAmoebaShape ? AmoebaBasin.Make(currfclty, config) :
                rb == rbUserDefShape ? UserDefinedBasin.Make(currfclty, config) : 
                                                        (Facility)null;
            if(newFclty == null) throw new InvalidOperationException(
                "Unable to create Facility for Shape Change.");
            // ---------------------------------------------------------
            ArrangeControls?.Invoke(this,
                ArrangeControlsEventArgs.Make(
                    newFclty.Category, newFclty.Configuration));
            catchmnt.Facility = newFclty;
            ScenarioisValid = false;
            DisplayValidCalcButton(null);
            BindControls(catchmnt);
        }
        #endregion Change Catchment/Facility properties

        #region misc private helper functions

        internal void IgnoreMinMaxValues()
        {
            IgnoreContraints(nudBottomArea);
            IgnoreContraints(nudRockBottomArea);
            IgnoreContraints(nudOverflowSurfaceArea);
            IgnoreContraints(nudOverflowEHeight);
            IgnoreContraints(nudOverflowESurfaceArea);
            IgnoreContraints(nudRockStorageDepth);
            IgnoreContraints(nudUnderdrainHeight);
        }

        private void IgnoreContraints(NumericUpDown nud)
        {
            nud.Minimum = 0;
            nud.Maximum = decimal.MaxValue;
        }
        private void ReSetMinMaxValues(Catchment ctch)
        {
            var fclty = ctch.Facility;
            var cfg = fclty.Configuration;
            var isCfgE = cfg == Configuration.ConfigE;
            var agD = fclty.AboveGrade;
            var bgD = fclty.BelowGrade;
            var ba = agD.BottomArea.GetValueOrDefault(0m);
            var bsdFt = fclty.BlendedSoilDepth.GetValueOrDefault(0m);
            var rsdFt = bgD.RockStorageDepth.GetValueOrDefault(0m);
            var ofSA = agD.OverflowSurfaceArea.GetValueOrDefault(0m);
            var ofESA = agD.OverflowESurfaceArea.GetValueOrDefault(0m);
            var ofHgt = agD.OverflowHeight.GetValueOrDefault(0m);
            // ---------- nudBottomArea -----------------------------

            // PostTOC <= PreTOC or PreTOC > postTC is Valid
            // PostTC > PreTC , or PreTC <= PostTC  is Invalid
            ResetMinimum(nudPreTC, ctch.PostTOC);
            ResetMaximum(nudPostTC, ctch.PreTOC);

            ResetMinimum(nudBottomArea, ofESA);
            if(agD.BottomArea.GetValueOrDefault(0m) > ba)
                ba = agD.BottomArea.GetValueOrDefault(0m);

            // ---- nudRockBottomArea -------------------------------
            ResetMaximum(nudRockBottomArea,
                Configuration.ConfigBCEF.HasFlag(cfg) && !(fclty is SlopedFacility) ? ba / 3m: 99999);

 

            // ---- nudOverflowSurfaceArea --------------------------
            if (cfg == cfgE) ResetMaximum(nudOverflowSurfaceArea, ba);
            ResetMinimum(nudOverflowSurfaceArea,
                config != cfgE ? ba : lib.Minimum(ba, ofESA));
            if (agD.OverflowSurfaceArea.GetValueOrDefault(0m) > ofSA)
                ofSA = agD.BottomArea.GetValueOrDefault(0m);

            // --- nudOverflowEHeight ---------------------------
            if (isCfgE) ResetMaximum(nudOverflowEHeight, ofHgt);

            // --- nudOverflowESurfaceArea ---------------------
            if (isCfgE)
            {
                if (fclty is UserDefinedBasin)
                    ResetMaximum(nudOverflowESurfaceArea, ofSA);
                ResetMinimum(nudOverflowESurfaceArea, ba);
            }

            // --- nudRockStorageDepth ----------------------------
            if (cfg != cfgA)
                ResetMaximum(nudRockStorageDepth,
                    lib.Minimum(30m, bsdFt - 6m));

            // --- nudUnderdrainHeight ----------------------------
            if (Configuration.ConfigCDF.HasFlag(cfg))
                ResetMaximum(nudUnderdrainHeight, rsdFt - 6m);
        }
        private static void ResetMaximum(NumericUpDown nud, decimal newMax)
        {
            if(newMax < 0m) throw new ParameterOutOfRangeException(
                "Maximum value must be at least zero");
            if (newMax < nud.Minimum) nud.Minimum = newMax;
            nud.Value = lib.Minimum(newMax, nud.Minimum, nud.Value);
            nud.Maximum = newMax;
        }
        private void ResetMinimum(NumericUpDown nud, decimal newMin)
        {
            if (newMin > nud.Maximum) nud.Maximum = newMin;
            nud.Value = lib.Maximum(newMin, nud.Maximum, nud.Value);
            nud.Minimum = newMin;
        }
        private string StatusString(PRStatus stat)
        {
            return stat == PRStatus.Pass ? "PASS" :
                stat == PRStatus.JustificationRequired ? "JUST" :
                stat == PRStatus.Fail ? "FAIL" : "PEND";
        }
        private void DisplayValidCalcButton(bool? scenarioValid)
        {
            btnValidateCalculate.Text = 
                ScenarioisValid ? "Calculate" : "Validate";
            lblValid.Visible = scenarioValid ?? false;
            lblFail.Visible = !scenarioValid ?? false;
        }
        private void ShowValidFailMesage(ScenData valid)
        {
            lblValid.Visible = valid == ScenData.Valid;
            lblFail.Visible = valid == ScenData.Fail;
        }
        private void ValidateScenarioData()
        {
            ShowValidFailMesage(ScenarioisValid? ScenData.Valid: ScenData.Fail);
            try
            {
                LoadValidator().ValidateScenarioData(scenario);
                //ValidateScenario?.Invoke(this, ValidatorEventArgs.Make(scenario));
                DisplayValidCalcButton(ScenarioisValid = true);
            }
            catch (ValidationException valX)
            {
                if (valX.Control != DataControl.NA) 
                    dataCtrls[valX.Control].Focus();
                msg.Warn(valX.Message, msgTitl);
                DisplayValidCalcButton(ScenarioisValid = false);
            }
        }
        private (int row, int col) GetTableLayoutPanelRowCol(TableLayoutPanel tlp, Point point)
        {
            var cell = (row: 0, col: 0);
            if (point.X > tlp.Width || point.Y > tlp.Height) return cell;

            var w = tlp.Width;
            var h = tlp.Height;
            var widths = tlp.GetColumnWidths();

            int i;
            for (i = widths.Length - 1; i >= 0 && point.X < w; i--)
                w -= widths[i];
            cell.col = i + 1;

            var heights = tlp.GetRowHeights();
            for (i = heights.Length - 1; i >= 0 && point.Y < h; i--)
                h -= heights[i];

            cell.row = i + 1;

            return cell;
        }

        private ICalculateResults LoadCalulator(FacilityCategory cat)
        {
            var assmblyNm = appStgs["CalculatorAssembly"];
            var assmSpec = lib.ASSEMBLYFOLDER + assmblyNm;
            Assembly dA;
            try { dA = Assembly.LoadFrom(assmSpec); }
            catch (FileNotFoundException nfX)
            {
                throw new CoPInvalidOperationException(
                    $"PacSizing Engine Assembly [{assmSpec}] cannot be loaded.", nfX);
            }
            // -------------------------------------------------
            var calcClassNm =
                    cat == FacilityCategory.SlopedFacility ? appStgs["SlopedFacilityCalculator"] :
                    cat == FacilityCategory.RectBasin ? appStgs["RectBasinCalculator"] :
                    cat == FacilityCategory.FlatPlanter ? appStgs["FlatPlanterCalculator"] :
                    cat == FacilityCategory.AmoebaBasin ? appStgs["AmoebaCalculator"] :
                    cat == FacilityCategory.UserDefinedBasin ? appStgs["UserDefinedCalculator"] : null;
            var calcTyp = dA.DefinedTypes.First(t=>t.FullName== calcClassNm);
            if (!(Activator.CreateInstance(calcTyp) is ICalculateResults calc))
                throw new CoPInvalidOperationException(
                    $"Unable to instantiate {calcClassNm} from Reports Assembly {assmblyNm}");
            return calc;
        }
        private IValidateScenarioData LoadValidator()
        {
            var assmblyNm = appStgs["CalculatorAssembly"];
            var assmSpec = lib.ASSEMBLYFOLDER + assmblyNm;
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
                    $"Unable to instantiate {vadtrClassNm} from Reports Assembly {assmblyNm}");
            return valdtr;
        }
        #endregion misc private helper functions

    }
    internal enum ScenData { Pend = 0x00, Valid = 0x01, Fail = 0x02 }
    internal enum ExportFmt { Null = 0x00, Xml = 0x01, Json = 0x02 }
}