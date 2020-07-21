using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BES.SWMM.PAC
{

    public class Controller: Constants
    {
        #region General Statics & Consts
        private readonly Dictionary<AGP, Control> aboveGrdCtrls = new Dictionary<AGP, Control>();
        private readonly Dictionary<BGP, Control> belowGrdCtrls = new Dictionary<BGP, Control>();
        #endregion General Statics & Consts

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

        #region private view properties
        private Panel MnPanel { get; set; }
        private Panel AGPanel { get; set; }
        private Panel SegPanel { get; set; }
        private Panel BGPanel { get; set; }
        private TabControl TabCntrl { get; set; }
        #endregion private view properties

        #region ctor/Factory
        private Controller(ICanBeArranged view) {InitializeControlArrays(view);}
        public static Controller Make(ICanBeArranged view) { return new Controller(view); }

        private void InitializeControlArrays(ICanBeArranged view)
        {
            MnPanel = (Panel) view.FindControl("panFacilityTop");
            SegPanel = (Panel) view.FindControl("panSegments");
            AGPanel = (Panel) view.FindControl("panAboveGrade");
            BGPanel = (Panel) view.FindControl("panBelowGrade");
            TabCntrl = (TabControl) view.FindControl("tabCtrl");
            // -------------------------------------------------------
            aboveGrdCtrls.Add(AGP.BA, view.FindControl("panBottomArea"));
            aboveGrdCtrls.Add(AGP.ABW, view.FindControl("panAvgBotmWidth"));
            aboveGrdCtrls.Add(AGP.OFH, view.FindControl("panOverflowHeight"));
            aboveGrdCtrls.Add(AGP.OFA, view.FindControl("panOverflowSurfaceArea"));
            aboveGrdCtrls.Add(AGP.OFEH, view.FindControl("panOverflowEHeight"));
            aboveGrdCtrls.Add(AGP.OFEA, view.FindControl("panOverflowESurfaceArea"));
            aboveGrdCtrls.Add(AGP.SS, view.FindControl("panSideSlope"));
            aboveGrdCtrls.Add(AGP.FD, view.FindControl("panFreeboard"));
            aboveGrdCtrls.Add(AGP.BP, view.FindControl("panBottomPerimeter"));
            // ---------------------------------------------
            belowGrdCtrls.Add(BGP.IP, view.FindControl("panInfiltrationPcnt"));
            belowGrdCtrls.Add(BGP.RSD, view.FindControl("panRockStorageDepth"));
            belowGrdCtrls.Add(BGP.RP, view.FindControl("panPorosity"));
            belowGrdCtrls.Add(BGP.UDH, view.FindControl("panUnderdrainHeight"));
            belowGrdCtrls.Add(BGP.ORF, view.FindControl("panOrifice"));
            belowGrdCtrls.Add(BGP.RAW, view.FindControl("panRockAreaWidth"));
            belowGrdCtrls.Add(BGP.C2S, view.FindControl("panCatchmentTooSmall"));
            // ----------------------------------------------
        }
        #endregion ctor/Factory

        public void ArrangeControls(object sender, ArrangeControlsEventArgs e)
        { ArrangeControls(e.Category, e.Config); }

        #region private work methods
        private void ArrangeControls(FacilityCategory cat, Configuration cfg)
        {
            switch (cfg)
            {
                #region Config A
                case cfgA:
                    switch (cat)
                    {
                        case slpdFclty:
                            DisplayAGControls(new AGCtrls { { AGP.OFH, ptUR } });
                            DisplayBGControls(new BGCtrls {
                                { BGP.IP, ptTL}, {BGP.C2S,  ptTR}});
                            break;
                        case fltPntr:
                            DisplayAGControls(
                                new AGCtrls {{AGP.BA, ptUR},
                                    {AGP.OFH, ptTL}, {AGP.ABW, ptTR}});
                            DisplayBGControls(new BGCtrls {
                                { BGP.IP, ptTL}, {BGP.C2S,  ptTR}});
                            break;
                        case rectBasin:
                            DisplayAGControls(
                                new AGCtrls {{AGP.BA, ptUR},
                                    {AGP.SS, ptTL}, {AGP.FD, ptTR},
                                    {AGP.OFH, ptML},{AGP.ABW, ptMR}}, true);
                            DisplayBGControls(new BGCtrls {
                                { BGP.IP, ptTL}, {BGP.C2S,  ptTR}});
                            break;
                        case amoebaBasin:
                            DisplayAGControls(
                                new AGCtrls {{AGP.BA, ptUR},
                                    {AGP.SS, ptTL}, {AGP.FD, ptTR},
                                    {AGP.OFH, ptML}, {AGP.BP, ptMR}}, true);
                            DisplayBGControls(new BGCtrls {
                                { BGP.IP, ptTL}, {BGP.C2S,  ptTR}});
                            break;
                        case usrBasin:
                            DisplayAGControls(
                                new AGCtrls {{AGP.BA, ptUR},
                                    {AGP.OFH, ptTL},{AGP.OFA, ptTR} }, true);
                            DisplayBGControls(new BGCtrls {
                                { BGP.IP, ptTL}, {BGP.C2S,  ptTR}});
                            break;
                    }
                    #endregion Config A
                    break;
                #region Config B
                case cfgB:
                    switch (cat)
                    {
                        case slpdFclty:
                            DisplayAGControls(AGCtrls.Empty);
                            DisplayBGControls(
                                new BGCtrls {
                                    {BGP.IP, ptTL}, {BGP.C2S,  ptTR},
                                    {BGP.RSD, ptML}, {BGP.RP, ptMR},
                                    {BGP.RAW, raw3}}, false);
                            break;
                        case fltPntr:
                            DisplayAGControls(
                                new AGCtrls {{AGP.BA, ptUR},
                                    {AGP.OFH, ptTL}, {AGP.ABW, ptTR}});
                            DisplayBGControls(
                                new BGCtrls {
                                    {BGP.IP, ptTL}, {BGP.C2S,  ptTR},
                                    {BGP.RSD, ptML}, {BGP.RP, ptMR},
                                    {BGP.RAW, raw3}}, false);
                            break;
                        case rectBasin:
                            DisplayAGControls(
                                new AGCtrls {{AGP.BA, ptUR},
                                    {AGP.ABW, ptTL}, {AGP.OFH, ptTR},
                                    {AGP.SS, ptML}, {AGP.FD, ptMR}}, true);
                            DisplayBGControls(
                                new BGCtrls{
                                    {BGP.IP, ptTL}, {BGP.C2S,  ptTR},
                                    {BGP.RSD, ptML}, {BGP.RP, ptMR},
                                    {BGP.RAW, raw3}}, false);
                            break;
                        case amoebaBasin:
                            DisplayAGControls(
                                new AGCtrls {{AGP.BA, ptUR},
                                    {AGP.BP, ptTL},{AGP.OFH, ptTR},
                                    {AGP.SS, ptML}, {AGP.FD, ptMR}}, true);
                            DisplayBGControls(
                                new BGCtrls {
                                    {BGP.IP, ptTL}, {BGP.C2S,  ptTR},
                                    {BGP.RSD, ptML}, {BGP.RP, ptMR},
                                    {BGP.RAW, raw3}}, false);
                            break;
                        case usrBasin:
                            DisplayAGControls(
                                new AGCtrls { {AGP.BA, ptUR},
                                    {AGP.OFH, ptTL},{AGP.OFA, ptTR} }, true);
                            DisplayBGControls(
                                new BGCtrls{
                                    {BGP.IP, ptTL}, {BGP.C2S,  ptTR},
                                    {BGP.RSD, ptML}, {BGP.RP, ptMR},
                                    {BGP.RAW, raw3}}, false);
                            break;
                    }
                    #endregion Config B
                    break;
                #region Config C
                case cfgC:
                    switch (cat)
                    {
                        case slpdFclty:
                            DisplayAGControls(AGCtrls.Empty);
                            DisplayBGControls(
                                new BGCtrls {
                                    {BGP.RSD, ptTL}, {BGP.IP, ptTR},
                                    {BGP.RP, ptML}, {BGP.UDH, ptMR},
                                    {BGP.ORF, ptBL}, {BGP.RAW, raw5}}, false);
                            break;
                        case fltPntr:
                            DisplayAGControls(
                                new AGCtrls {{AGP.BA, ptUR},
                                    {AGP.OFH, ptTL}, {AGP.ABW, ptTR}});
                            DisplayBGControls(
                                new BGCtrls {
                                    {BGP.RSD, ptTL}, {BGP.IP, ptTR},
                                    {BGP.RP, ptML}, {BGP.UDH, ptMR},
                                    {BGP.ORF, ptBL}, {BGP.RAW, raw5}});
                            break;
                        case rectBasin:
                            DisplayAGControls(
                                new AGCtrls {{AGP.BA, ptUR},
                                    {AGP.ABW, ptTL}, {AGP.OFH, ptTR},
                                    {AGP.SS, ptML}, {AGP.FD, ptMR}}, true);
                            DisplayBGControls(
                                new BGCtrls{
                                    {BGP.RSD, ptTL}, {BGP.IP, ptTR},
                                    {BGP.RP, ptML }, {BGP.UDH, ptMR},
                                    {BGP.ORF, ptBL}, {BGP.RAW, raw5}});
                            break;
                        case amoebaBasin:
                            DisplayAGControls(
                                new AGCtrls {{AGP.BA, ptUR},
                                    {AGP.BP, ptTL}, {AGP.OFH, ptTR},
                                    {AGP.SS, ptML}, {AGP.FD, ptMR}}, true);
                            DisplayBGControls(
                                new BGCtrls {
                                    {BGP.RSD, ptTL},{BGP.IP, ptTR},
                                    {BGP.RP, ptML}, {BGP.UDH, ptMR},
                                    {BGP.ORF, ptBL}, {BGP.RAW, raw5}});
                            break;
                        case usrBasin:
                            DisplayAGControls(
                                new AGCtrls { {AGP.BA, ptUR},
                                    {AGP.OFH, ptTL}, {AGP.OFA, ptTR} }, true);
                            DisplayBGControls(
                                new BGCtrls{
                                    {BGP.RSD, ptTL}, {BGP.IP, ptTR},
                                    {BGP.RP, ptML}, {BGP.UDH, ptMR},
                                    {BGP.ORF, ptBL},{BGP.RAW, raw5}});
                            break;
                    }
                    #endregion Config C
                    break;
                #region Config D
                case cfgD:
                    switch (cat)
                    {
                        case slpdFclty:
                            DisplayAGControls(AGCtrls.Empty);
                            DisplayBGControls(
                                new BGCtrls {  {BGP.RSD, ptUR},
                                    {BGP.RP, ptTL}, {BGP.UDH, ptTR},
                                    {BGP.ORF, ptML}, {BGP.RAW, raw4}}, false);
                            break;
                        case fltPntr:
                            DisplayAGControls(
                                new AGCtrls {{AGP.BA, ptUR},
                                    {AGP.OFH, ptTL}, {AGP.ABW, ptTR}});
                            DisplayBGControls(
                                new BGCtrls {{BGP.RSD, ptUR},
                                    {BGP.RP, ptTL}, {BGP.UDH, ptTR},
                                    {BGP.ORF, ptML}, {BGP.RAW, raw4}});
                            break;
                        case rectBasin:
                            DisplayAGControls(
                                new AGCtrls {
                                    {AGP.BA, ptTL}, {AGP.ABW, ptTR},
                                    {AGP.SS, ptML}, {AGP.FD, ptMR},
                                    {AGP.OFH, ptBL}}, true);
                            DisplayBGControls(
                                new BGCtrls{{BGP.RSD, ptUR},
                                    {BGP.RP, ptTL }, {BGP.UDH, ptTR},
                                    {BGP.ORF, ptML}, {BGP.RAW, raw4}}); break;
                        case amoebaBasin:
                            DisplayAGControls(
                                new AGCtrls {{AGP.BA, ptUR},
                                    {AGP.BP, ptTL}, {AGP.OFH, ptTR},
                                    {AGP.SS, ptML}, {AGP.FD, ptMR}}, true);
                            DisplayBGControls(
                                new BGCtrls {{BGP.RSD, ptUR},
                                    {BGP.RP, ptTL}, {BGP.UDH, ptTR},
                                    {BGP.ORF, ptML}, {BGP.RAW, raw4}});
                            break;
                        case usrBasin:
                            DisplayAGControls(
                                new AGCtrls {{AGP.BA, ptUR},
                                    {AGP.OFH, ptTL}, {AGP.OFA, ptTR}}, true);
                            DisplayBGControls(
                                new BGCtrls{{BGP.RSD, ptUR},
                                    {BGP.RP, ptTL}, {BGP.UDH, ptTR},
                                    {BGP.ORF, ptML}, {BGP.RAW, raw4}});
                            break;
                    }
                    #endregion Config D
                    break;
                #region Config E
                case cfgE:
                    switch (cat)
                    {
                        case slpdFclty:
                            DisplayAGControls(new AGCtrls { { AGP.OFEH, ptUR } });
                            DisplayBGControls(
                                new BGCtrls {
                                    {BGP.IP, ptTL}, {BGP.C2S,  ptTR},
                                    {BGP.RSD, ptML}, {BGP.RP, ptMR},
                                    {BGP.RAW, raw3}}, false);
                            break;
                        case fltPntr:
                            DisplayAGControls(
                                new AGCtrls {
                                    {AGP.BA, ptTL}, {AGP.ABW, ptTR},
                                    {AGP.OFH, ptML}, {AGP.OFEH, ptMR}});
                            DisplayBGControls(
                                new BGCtrls {
                                    {BGP.IP, ptTL}, {BGP.C2S,  ptTR},
                                    {BGP.RSD, ptML}, {BGP.RP, ptMR},
                                    {BGP.RAW, raw3}}, false);
                            break;
                        case rectBasin:
                            DisplayAGControls(
                                new AGCtrls {
                                    {AGP.BA, ptTL}, {AGP.ABW, ptTR},
                                    {AGP.SS, ptML}, {AGP.FD, ptMR},
                                    {AGP.OFH, ptBL}, {AGP.OFEH, ptBR}}, true);
                            DisplayBGControls(
                                new BGCtrls{
                                    {BGP.IP, ptTL}, {BGP.C2S,  ptTR},
                                    {BGP.RSD, ptML}, {BGP.RP, ptMR},
                                    {BGP.RAW, raw3}}, false);
                            break;
                        case amoebaBasin:
                            DisplayAGControls(
                                new AGCtrls {
                                    {AGP.BA, ptTL}, {AGP.BP, ptTR},
                                    {AGP.SS, ptML}, {AGP.FD, ptMR},
                                    {AGP.OFH, ptBL},{AGP.OFEH, ptBR}}, true);
                            DisplayBGControls(
                                new BGCtrls {
                                    {BGP.IP, ptTL}, {BGP.C2S,  ptTR},
                                    {BGP.RSD, ptML}, {BGP.RP, ptMR},
                                    {BGP.RAW, raw3}}, false);
                            break;
                        case usrBasin:
                            DisplayAGControls(
                                new AGCtrls {{AGP.BA, ptUR},
                                    {AGP.OFH, ptTL}, {AGP.OFA, ptTR},
                                    {AGP.OFEH, ptML}, {AGP.OFEA, ptMR}}, true);
                            DisplayBGControls(
                                new BGCtrls{
                                    {BGP.IP, ptTL}, {BGP.C2S,  ptTR},
                                    {BGP.RSD, ptML}, {BGP.RP, ptMR},
                                    {BGP.RAW, raw3}}, false);
                            break;
                    }
                    #endregion Config E
                    break;
                #region Config F
                case cfgF:
                    switch (cat)
                    {
                        case slpdFclty:
                            DisplayAGControls(AGCtrls.Empty);
                            DisplayBGControls(
                                new BGCtrls { {BGP.C2S,  ptUR},
                                    {BGP.RSD, ptTL}, {BGP.IP, ptTR},
                                    {BGP.RP, ptML}, {BGP.UDH, ptMR},
                                    {BGP.RAW, raw3}}, false);
                            break;
                        case fltPntr:
                            DisplayAGControls(
                                new AGCtrls {{AGP.BA, ptUR},
                                    {AGP.ABW, ptTL}, {AGP.OFH, ptTR}});
                            DisplayBGControls(
                                new BGCtrls { {BGP.C2S,  ptUR},
                                    {BGP.RSD, ptTL},{BGP.IP, ptTR},
                                    {BGP.RP, ptML}, {BGP.UDH, ptMR },
                                    {BGP.RAW, raw3}});
                            break;
                        case rectBasin:
                            DisplayAGControls(
                                new AGCtrls {{AGP.BA, ptUR},
                                    {AGP.ABW, ptTL}, {AGP.OFH, ptTR},
                                    {AGP.SS, ptML}, {AGP.FD, ptMR}}, true);
                            DisplayBGControls(
                                new BGCtrls{ {BGP.C2S,  ptUR},
                                    {BGP.RSD, ptTL}, {BGP.IP, ptTR},
                                    {BGP.RP, ptML }, {BGP.UDH, ptMR },
                                    {BGP.RAW, raw3}});
                            break;
                        case amoebaBasin:
                            DisplayAGControls(
                                new AGCtrls {{AGP.BA, ptUR},
                                    {AGP.SS, ptTL}, {AGP.FD, ptTR},
                                    {AGP.OFH, ptML}, {AGP.BP, ptMR}}, true);
                            DisplayBGControls(
                                new BGCtrls { {BGP.C2S,  ptUR},
                                    {BGP.RSD, ptTL}, {BGP.IP, ptTR},
                                    {BGP.RP, ptML}, {BGP.UDH, ptMR},
                                    {BGP.RAW, raw3}});
                            break;
                        case usrBasin:
                            DisplayAGControls(
                                new AGCtrls {{AGP.BA, ptUR},
                                    {AGP.OFH, ptTL}, {AGP.OFA, ptTR}}, true);
                            DisplayBGControls(
                                new BGCtrls{ {BGP.C2S,  ptUR},
                                    {BGP.RSD, ptTL}, {BGP.IP, ptTR},
                                    {BGP.RP, ptML}, {BGP.UDH, ptMR},
                                    {BGP.RAW, raw3}});
                            break;
                    }
                    #endregion Config F
                    break;
            }
            var isSloped = cat == FacilityCategory.SlopedFacility;

            SegPanel.Visible = isSloped;
            if (!isSloped) return;
            // -----------------------------
            SegPanel.Top = BGPanel.Visible ?
                BGPanel.Top + BGPanel.Height + 10 :
                AGPanel.Top + AGPanel.Height + 10;
            SegPanel.Height = TabCntrl.Height - SegPanel.Top - 60;
        }
        private void DisplayAGControls(AGCtrls agCtrls, bool isBasin = false)
        {
            var hidePt = new Point(1000, 500);
            if (!(AGPanel.Visible = (agCtrls.Count > 0))) return;
            // -----------------------------------------
            var last = aboveGrdCtrls[agCtrls.Last().Key];
            foreach (var agCK in aboveGrdCtrls.Keys)
            {
                var ctl = aboveGrdCtrls[agCK];
                var needsCtrl = agCtrls.ContainsKey(agCK);
                ctl.Visible = needsCtrl;
                ctl.Location = needsCtrl ? agCtrls[agCK] : hidePt;
            }

            AGPanel.Location = isBasin ? topBasinPt : topOtherPt;
            AGPanel.Height = last.Top + last.Height + 5;
        }
        private void DisplayBGControls(BGCtrls bgCtrls, bool hasAGCtrls = true)
        {
            var hidePt = new Point(1000, 500);
            if (!(BGPanel.Visible = bgCtrls.Count > 0)) return;
            // ------------------------------------------------------
            {
                var blast = belowGrdCtrls[bgCtrls.Last().Key];
                foreach (var bgCK in belowGrdCtrls.Keys)
                {
                    var ctl = belowGrdCtrls[bgCK];
                    var needsCtrl = bgCtrls.ContainsKey(bgCK);
                    ctl.Visible = needsCtrl;
                    ctl.Location = needsCtrl ? bgCtrls[bgCK] : hidePt;
                }
                BGPanel.Location = new Point(ctrlLeft, 10 +
                           (hasAGCtrls ? AGPanel.Top + AGPanel.Height :
                               MnPanel.Top + MnPanel.Height));
                BGPanel.Height = blast.Top + blast.Height + 5;
            }
        }
        #endregion private work methods
    }
}