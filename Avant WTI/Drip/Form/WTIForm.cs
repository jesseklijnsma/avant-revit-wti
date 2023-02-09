using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI.Selection;
using Avant.WTI.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Avant.WTI.Drip.Form
{
    /// <summary>
    /// 
    /// </summary>
    public partial class WTIForm : System.Windows.Forms.Form
    {

        private readonly DripData data;
        private System.Drawing.RectangleF maxBounds;
        private System.Drawing.RectangleF bounds;
        private Graphics g;

        private readonly Dictionary<Area, List<Line>> areaLineMap = new Dictionary<Area, List<Line>>();
        private readonly Dictionary<Pipe, Line> pipe_lineMap = new Dictionary<Pipe, Line>();


        private readonly DripGenerator dripGenerator;


        /// <summary>
        /// 
        /// </summary>
        public WTIForm(DripData data)
        {
            InitializeComponent();
            if (!data.isValidInput()) throw new ArgumentException();
            this.data = data;
            this.bounds = Utils.calculateBounds(data);
            this.maxBounds = this.bounds;

            foreach (Area area in this.data.areas)
            {
                RectangleF arearect = AreaUtils.getAreaRectangle(area);
                areaLineMap.Add(area, Utils.rectangleToLines(arearect));
            }

            ReloadData();

            dripGenerator = new DripGenerator(this.data);

            // Enable double buffering for the canvas
            this.canvas.GetType().GetProperty(
                "DoubleBuffered",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic
            ).SetValue(this.canvas, true, null);

        }

        private void ReloadData()
        {
            Dictionary<PipeType, string> pipetypes = this.data.pipetypes.ToDictionary(x => x, x => x.Name);
            Util.FormUtils.Combobox_bindItems(this.combo_pipetype, pipetypes);
            Dictionary<PipingSystemType, string> systemtypes = this.data.systemtypes.ToDictionary(x => x, x => x.Name);
            Util.FormUtils.Combobox_bindItems(this.combo_transportsystem, systemtypes);
            Util.FormUtils.Combobox_bindItems(this.combo_distributionsystem, systemtypes);

            Dictionary<FamilySymbol, string> valvefamilies = this.data.valvefamilies.ToDictionary(x => x, x => x.Name);
            Util.FormUtils.Combobox_bindItems(this.combo_valvefamily, valvefamilies);


            num_interdistance.Value = this.data.intermediateDistance;
            num_backwalldistance.Value = this.data.backwallDistance;

            num_valvecolumndistance.Value = this.data.valvecolumnDistance;
            num_pipecolumndistance.Value = this.data.pipecolumnDistance;

            num_transportheight.Value = this.data.transportlineheight;
            num_distributionheight.Value = this.data.distributionlineheight;

            button_convertplaceholders.Checked = this.data.convertPlaceholders;

            UpdateSizes();
        }

        public void ReloadPreview()
        {
            dripGenerator.GeneratePreviewGeometry();
            this.canvas.Invalidate();
        }

        private void Canvas_paint(object sender, PaintEventArgs e)
        {
            Graphics gr = e.Graphics;
            System.Drawing.Rectangle rect = this.canvas.ClientRectangle;
            BufferedGraphicsContext bgm = BufferedGraphicsManager.Current;
            BufferedGraphics bg = bgm.Allocate(gr, rect);
            g = bg.Graphics;
            g.SetClip(new RectangleF(new PointF(), this.canvas.Size));

            g.Clear(System.Drawing.Color.FromArgb(54, 54, 54));

            foreach (Line line in this.data.lines)
            {
                DrawLine(line, System.Drawing.Color.Black, false);
            }


            foreach (List<Line> linelist in areaLineMap.Values)
            {
                foreach (Line l in linelist)
                {
                    DrawLine(l, System.Drawing.Color.Green, true);
                }
            }

            foreach (Line line in pipe_lineMap.Values)
            {
                DrawLine(line, System.Drawing.Color.Yellow, false);
            }

            if (this.data.previewGeometry != null)
            {
                foreach (Line l in this.data.previewGeometry)
                {
                    DrawLine(l, System.Drawing.Color.Aqua, false);
                }
            }

            foreach (XYZ p in this.data.previewPoints)
            {
                DrawPoint(p, System.Drawing.Color.White, 3);
            }

            bg.Render();
            bg.Dispose();
        }

        private void UpdateSizes()
        {
            this.combo_transportdiameter.DataSource = null;
            this.combo_distributiondiameter.DataSource = null;

            if (this.data.pipetype == null) return;
            if (!this.data.pipesizeMap.ContainsKey(this.data.pipetype)) return;
            List<double> sizes = this.data.pipesizeMap[this.data.pipetype];

            Dictionary<double, string> sizeItems = sizes.ToDictionary(x => x, x => x.ToString() + " mm");
            Util.FormUtils.Combobox_bindItems(this.combo_transportdiameter, sizeItems);
            Util.FormUtils.Combobox_bindItems(this.combo_distributiondiameter, sizeItems);

        }

        private void SelectSourceLines()
        {

            List<Pipe> pipelines = new List<Pipe>();

            PipePlaceholderSelectionFilter selfilter = new PipePlaceholderSelectionFilter();
            pipe_lineMap.Clear();

            try
            {
                IList<Reference> refs = this.data.uidoc.Selection.PickObjects(ObjectType.Element, selfilter);
                foreach (Reference r in refs)
                {
                    Pipe p = (Pipe)this.data.doc.GetElement(r.ElementId);
                    pipelines.Add(p);
                    if (p.Location.GetType() == typeof(LocationCurve))
                    {
                        LocationCurve lc = (LocationCurve)p.Location;
                        if (lc.Curve.GetType() == typeof(Line))
                        {
                            pipe_lineMap[p] = (Line)lc.Curve;
                        }
                    }
                }
                this.data.pipelines = pipelines;
            }
            catch (Exception) { }

            ReloadPreview();
            this.Activate();
        }

        private class PipePlaceholderSelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                if (elem.Category?.BuiltInCategory == BuiltInCategory.OST_PlaceHolderPipes) return true;
                return false;
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                return false;
            }
        }

    }

}
