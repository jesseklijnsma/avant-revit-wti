using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RevitTemplate
{
    /// <summary>
    /// 
    /// </summary>
    public partial class WTIForm : System.Windows.Forms.Form
    {

        private const bool DEBUG = true;

        private DripData data;
        private System.Drawing.RectangleF maxBounds;
        private System.Drawing.RectangleF bounds;
        private Graphics g;

        private Dictionary<Area, List<Line>> areaLineMap = new Dictionary<Area, List<Line>>();
        private Dictionary<Pipe, Line> pipe_lineMap = new Dictionary<Pipe, Line>();


        private DripGenerator dripGenerator;


        /// <summary>
        /// 
        /// </summary>
        public WTIForm(DripData data)
        {
            InitializeComponent();
            if (!data.isValidInput()) throw new ArgumentException();
            this.data = data;
            this.bounds = Util.calculateBounds(data);
            this.maxBounds = this.bounds;

            foreach (Area area in this.data.areas)
            {
                RectangleF arearect = AreaUtils.getAreaRectangle(area);
                areaLineMap.Add(area, Util.rectangleToLines(arearect));
            }

            reloadData();


            dripGenerator = new DripGenerator(this.data);

            // Enable double buffering for the canvas
            this.canvas.GetType().GetProperty(
                "DoubleBuffered",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic
            ).SetValue(this.canvas, true, null);

        }


        private void reloadData()
        {
            Dictionary<PipeType, string> pipetypes = this.data.pipetypes.ToDictionary(x => x, x => x.Name);
            combobox_bindItems(this.combo_pipetype, pipetypes);
            Dictionary<PipingSystemType, string> systemtypes = this.data.systemtypes.ToDictionary(x => x, x => x.Name);
            combobox_bindItems(this.combo_transportsystem, systemtypes);
            combobox_bindItems(this.combo_distributionsystem, systemtypes);

            Dictionary<FamilySymbol, string> valvefamilies = this.data.valvefamilies.ToDictionary(x => x, x => x.Name);
            combobox_bindItems(this.combo_valvefamily, valvefamilies);


            num_interdistance.Value = this.data.intermediateDistance;
            num_backwalldistance.Value = this.data.backwallDistance;

            num_valvecolumndistance.Value = this.data.valvecolumnDistance;
            num_pipecolumndistance.Value = this.data.pipecolumnDistance;

            num_transportheight.Value = this.data.transportlineheight;
            num_distributionheight.Value = this.data.distributionlineheight;

            button_convertplaceholders.Checked = this.data.convertPlaceholders;

            updateSizes();
        }

        private class DropdownItem<T>
        {
            public string Name { get; }
            public T Value { get; }

            public DropdownItem(string Name, T Value)
            {
                this.Name = Name;
                this.Value = Value;
            }
        }

        private void combobox_bindItems<T>(ComboBox b, Dictionary<T, string> items)
        {
            List<DropdownItem<T>> comboBoxItems = new List<DropdownItem<T>>();
            foreach (KeyValuePair<T, string> kv in items)
            {
                comboBoxItems.Add(new DropdownItem<T>(kv.Value, kv.Key));
            }
            b.DisplayMember = "Name";
            b.ValueMember = "Value";
            b.DataSource = comboBoxItems;
        }

        private void WTI_Load(object sender, EventArgs e)
        {

        }

        public void reloadPreview()
        {
            dripGenerator.generatePreviewGeometry();
            this.canvas.Invalidate();
        }

        private void canvas_paint(object sender, PaintEventArgs e)
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
                drawLine(line, System.Drawing.Color.Black, false);
            }


            foreach (List<Line> linelist in areaLineMap.Values)
            {
                foreach (Line l in linelist)
                {
                    drawLine(l, System.Drawing.Color.Green, true);
                }
            }

            foreach (Line line in pipe_lineMap.Values)
            {
                drawLine(line, System.Drawing.Color.Yellow, false);
            }

            if (this.data.previewGeometry != null)
            {
                foreach (Line l in this.data.previewGeometry)
                {
                    drawLine(l, System.Drawing.Color.Aqua, false);
                }
            }

            if (DEBUG)
            {
                foreach (XYZ p in this.data.debugPoints)
                {
                    drawPoint(p, System.Drawing.Color.White, 3);
                }
            }

            bg.Render();
            bg.Dispose();
        }

        private void drawLine(Line line, System.Drawing.Color c, bool dashed)
        {
            Line screenLine = Util.lineToScreenLine(line, this.bounds, this.canvas.Size);
            if (screenLine == null) return;

            XYZ p1 = screenLine.GetEndPoint(0);
            XYZ p2 = screenLine.GetEndPoint(1);
            Pen pp = new Pen(c);
            if (dashed) pp.DashPattern = new float[] { 10, 10 };
            g.DrawLine(pp, (float)p1.X, (float)p1.Y, (float)p2.X, (float)p2.Y);
        }

        private void drawPoint(XYZ point, System.Drawing.Color c, float radius)
        {
            XYZ p = Util.pointToScreenPoint(point, bounds, this.canvas.Size);
            SolidBrush brush = new SolidBrush(c);
            g.FillEllipse(brush, new RectangleF((float)(p.X - radius), (float)(p.Y - radius), radius * 2, radius * 2));
        }



        private void canvas_mousewheel(object sender, MouseEventArgs e)
        {
            int n = e.Delta / 120;
            float rscale = (float)Math.Pow(2, -0.5 * n);

            float dx = e.X * this.bounds.Width * (1 - rscale) / this.canvas.Width;
            float dy = (this.canvas.Height - e.Y) * this.bounds.Height * (1 - rscale) / (this.canvas.Height);

            this.bounds.Width *= rscale;
            this.bounds.Height *= rscale;

            this.bounds.X += dx;
            this.bounds.Y += dy;

            canvas_checkBounds();

            this.canvas.Invalidate();
        }

        private void canvas_checkBounds()
        {
            if (this.bounds.Width > this.maxBounds.Width) this.bounds.Width = this.maxBounds.Width;
            if (this.bounds.Height > this.maxBounds.Height) this.bounds.Height = this.maxBounds.Height;

            if (this.bounds.Left < this.maxBounds.Left) this.bounds.X = this.maxBounds.X;
            if (this.bounds.Top < this.maxBounds.Top) this.bounds.Y = this.maxBounds.Y;
            if (this.bounds.Right > this.maxBounds.Right) this.bounds.X = this.maxBounds.Right - this.bounds.Width;
            if (this.bounds.Bottom > this.maxBounds.Bottom) this.bounds.Y = this.maxBounds.Bottom - this.bounds.Height;
        }

        private bool canvas_buttondown = false;
        private System.Drawing.Point canvas_mouse_location = new System.Drawing.Point();

        private void canvas_mousedown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                canvas_buttondown = true;
                canvas_mouse_location = e.Location;
            }
        }

        private void canvas_mouseup(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                canvas_buttondown = false;
            }
        }

        private void canvas_mousemove(object sender, MouseEventArgs e)
        {
            if (canvas_buttondown)
            {
                float dx = canvas_mouse_location.X - e.Location.X;
                float dy = e.Location.Y - canvas_mouse_location.Y;

                dx *= this.bounds.Width / this.canvas.Width;
                dy *= this.bounds.Height / this.canvas.Height;

                this.bounds.Location += new SizeF(dx, dy);

                canvas_checkBounds();

                canvas_mouse_location = e.Location;
            }
            canvas.Invalidate();
        }
        private void canvas_resize(object sender, EventArgs e)
        {
            canvas.Invalidate();
        }

        private void WTIForm_Activated(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
        }




        private void selectSourceLines(object sender, EventArgs e)
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

            reloadPreview();
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

        private void updateSizes()
        {
            this.combo_transportdiameter.DataSource = null;
            this.combo_distributiondiameter.DataSource = null;

            if (this.data.pipetype == null) return;
            if (!this.data.pipesizeMap.ContainsKey(this.data.pipetype)) return;
            List<double> sizes = this.data.pipesizeMap[this.data.pipetype];

            Dictionary<double, string> sizeItems = sizes.ToDictionary(x => x, x => x.ToString() + " mm");
            combobox_bindItems(this.combo_transportdiameter, sizeItems);
            combobox_bindItems(this.combo_distributiondiameter, sizeItems);

        }



        // Value Event Listeners

        private void combo_pipetype_SelectedIndexChanged(object sender, EventArgs e)
        {
            PipeType pt = (PipeType)combo_pipetype.SelectedValue;

            if (this.data.pipetype == pt) return;
            this.data.pipetype = pt;
            updateSizes();
        }

        private void combo_transportsystem_SelectedIndexChanged(object sender, EventArgs e)
        {
            PipingSystemType pst = (PipingSystemType)combo_transportsystem.SelectedValue;
            this.data.transportSystemType = pst;
        }

        private void combo_distributionsystem_SelectedIndexChanged(object sender, EventArgs e)
        {
            PipingSystemType pst = (PipingSystemType)combo_distributionsystem.SelectedValue;
            this.data.distributionSystemType = pst;
        }

        private void combo_valvefamily_SelectedIndexChanged(object sender, EventArgs e)
        {
            FamilySymbol valve = (FamilySymbol)combo_valvefamily.SelectedValue;
            this.data.valvefamily = valve;
        }

        private void num_interdistance_ValueChanged(object sender, EventArgs e)
        {
            this.data.intermediateDistance = (int)num_interdistance.Value;
            reloadPreview();
        }

        private void num_backwalldistance_ValueChanged(object sender, EventArgs e)
        {
            this.data.backwallDistance = (int)num_backwalldistance.Value;
            reloadPreview();
        }

        private void num_valvecolumndistance_ValueChanged(object sender, EventArgs e)
        {
            this.data.valvecolumnDistance = (int)num_valvecolumndistance.Value;
            reloadPreview();
        }

        private void num_pipecolumndistance_ValueChanged(object sender, EventArgs e)
        {
            this.data.pipecolumnDistance = (int)num_pipecolumndistance.Value;
            reloadPreview();
        }

        private void num_transportheight_ValueChanged(object sender, EventArgs e)
        {
            this.data.transportlineheight = (int)num_transportheight.Value;
        }

        private void combo_transportdiameter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.combo_transportdiameter.SelectedValue == null) return;
            double size = (double)combo_transportdiameter.SelectedValue;
            this.data.transport_diameter = size;
        }

        private void num_distributionheight_ValueChanged(object sender, EventArgs e)
        {
            this.data.distributionlineheight = (int)num_distributionheight.Value;
        }

        private void combo_distributiondiameter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.combo_distributiondiameter.SelectedValue == null) return;
            double size = (double)combo_distributiondiameter.SelectedValue;
            this.data.distribution_diameter = size;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            reloadPreview();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            dripGenerator.generateDrip();
            this.Close();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            this.data.convertPlaceholders = button_convertplaceholders.Checked;

        }
    }


}
