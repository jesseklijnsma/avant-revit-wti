using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI.Selection;
using Avant.WTI.Util;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Avant.WTI.Drip.Form
{
    /// <summary>
    ///  GUI for the Drip Irrigation generator
    /// </summary>
    public partial class WTIForm : System.Windows.Forms.Form
    {

        private const float CANVAS_ZOOM_SPEED = -0.5f;
        private const float MAX_ZOOM_SIZE_MM = 10.0f;
        private const MouseButtons PAN_BUTTON = MouseButtons.Middle;


        private Graphics g;

        private readonly DripData data;
        private readonly DripGenerator dripGenerator;

        private System.Drawing.RectangleF maxBounds;
        private System.Drawing.RectangleF bounds;
        private readonly Dictionary<Area, PolyLine> areaLineMap = new Dictionary<Area, PolyLine>();
        private readonly Dictionary<Pipe, Line> pipe_lineMap = new Dictionary<Pipe, Line>();

        public WTIForm(DripData data)
        {
            InitializeComponent();
            this.data = data;

            // Calculate bounds of the revit model based on grid lines and areas
            this.bounds = GeomUtils.CalculateBounds(data);
            this.maxBounds = this.bounds;

            // Convert areas into geometry for displaying
            foreach (Area area in data.areas)
            {
                PolyLine pl = AreaUtils.GetAreaPolyLine(area);
                if (pl == null) continue;
                areaLineMap.Add(area, pl);
            }

            // Initialize the drip generator
            dripGenerator = new DripGenerator(data);



            // Disable value changed event for inputs
            isLoading = true;

            // Load all input data into the form
            ReloadData();

            // Enable value changed event for inputs
            isLoading = false;


            // Enable double buffering for the canvas
            this.canvas.GetType().GetProperty(
                "DoubleBuffered",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic
            ).SetValue(this.canvas, true, null);

        }

        /// <summary>
        /// Reloads all data from the data model into the form control
        /// </summary>
        private void ReloadData()
        {
            // Bind pipetypes and their display values
            Dictionary<PipeType, string> pipetypes = data.pipetypes.ToDictionary(x => x, x => x.Name);
            Util.FormUtils.Combobox_bindItems(this.combo_pipetype, pipetypes);
            if (data.pipetype != null) this.combo_pipetype.SelectedValue = data.pipetype;
            else data.pipetype = (PipeType)this.combo_pipetype.SelectedValue;

            // Bind system types and their display values
            Dictionary<PipingSystemType, string> systemtypes = data.systemtypes.ToDictionary(x => x, x => x.Name);
            Util.FormUtils.Combobox_bindItems(this.combo_transportsystem, systemtypes);
            if (data.transportSystemType != null) this.combo_transportsystem.SelectedValue = data.transportSystemType;
            else data.transportSystemType = (PipingSystemType)this.combo_transportsystem.SelectedValue;

            Util.FormUtils.Combobox_bindItems(this.combo_distributionsystem, systemtypes);
            if (data.distributionSystemType != null) this.combo_distributionsystem.SelectedValue = data.distributionSystemType;
            else data.distributionSystemType = (PipingSystemType)this.combo_distributionsystem.SelectedValue;



            // Bind valve family symbols and their display values
            Dictionary<FamilySymbol, string> valvefamilies = data.valvefamilies.ToDictionary(x => x, x => x.Name);
            Util.FormUtils.Combobox_bindItems(this.combo_valvefamily, valvefamilies);
            if (data.valvefamily != null) this.combo_valvefamily.SelectedValue = data.valvefamily;
            else data.valvefamily = (FamilySymbol)this.combo_valvefamily.SelectedValue;

            // Load all primitive values
            num_valveheight.Value = data.valveheight;

            num_interdistance.Value = data.intermediateDistance;
            num_backwalldistance.Value = data.backwallDistance;

            num_valvecolumndistance.Value = data.valvecolumnDistance;
            num_pipecolumndistance.Value = data.pipecolumnDistance;

            num_transportheight.Value = data.transportlineheight;
            num_distributionheight.Value = data.distributionlineheight;

            button_convertplaceholders.Checked = data.convertPlaceholders;

            // Load sizes for the selected pipe type
            List<double> sizes = UpdateSizes();
            if (sizes.Contains(data.transport_diameter)) combo_transportdiameter.SelectedValue = data.transport_diameter;
            else
            {
                if (sizes.Count == 0) this.combo_transportdiameter.SelectedIndex = -1;
                else this.combo_transportdiameter.SelectedIndex = 0;
                data.transport_diameter = (double)this.combo_transportdiameter.SelectedValue;
            }

            if (sizes.Contains(data.distribution_diameter)) combo_distributiondiameter.SelectedValue = data.distribution_diameter;
            else
            {
                if (sizes.Count == 0) this.combo_distributiondiameter.SelectedIndex = -1;
                else this.combo_distributiondiameter.SelectedIndex = 0;
                data.distribution_diameter = (double)this.combo_distributiondiameter.SelectedValue;
            }
        }

        /// <summary>
        /// Generates and renders new preview lines
        /// </summary>
        public void ReloadPreview()
        {
            dripGenerator.GeneratePreviewGeometry();

            if(data.errorMessages.Count == 0)
            {
                label_previewwarning.Visible = false;
            }
            else
            {
                label_previewwarning.Visible = true;
            }
            // rerender
            canvas.Invalidate();
        }

        
        /// <summary>
        /// Loads the available pipe sizes of the selected pipetype into the size comboboxes
        /// </summary>
        private List<double> UpdateSizes()
        {
            // Remove old sizes
            this.combo_transportdiameter.DataSource = null;
            this.combo_distributiondiameter.DataSource = null;

            // Get pipe sizes
            if (data.pipetype == null) return new List<double>();
            if (!data.pipesizeMap.ContainsKey(data.pipetype)) return new List<double>();
            List<double> sizes = data.pipesizeMap[data.pipetype];

            // Binds the sizes and their display value to the comboboxes
            Dictionary<double, string> sizeItems = sizes.ToDictionary(x => x, x => x.ToString() + " mm");
            Util.FormUtils.Combobox_bindItems(this.combo_transportdiameter, sizeItems);
            Util.FormUtils.Combobox_bindItems(this.combo_distributiondiameter, sizeItems);

            return sizes;
        }

        /// <summary>
        /// Lets user select pipe placeholders in the Revit model
        /// </summary>
        private void SelectSourceLines()
        {
            List<Pipe> pipelines = new List<Pipe>();

            PipePlaceholderSelectionFilter selfilter = new PipePlaceholderSelectionFilter();

            try
            {
                IList<Reference> refs = data.uidoc.Selection.PickObjects(ObjectType.Element, selfilter);

                pipe_lineMap.Clear();
                foreach (Reference r in refs)
                {
                    Pipe p = (Pipe)data.doc.GetElement(r.ElementId);
                    pipelines.Add(p);

                    // Try to add the line of the pipe to the list of lines
                    // Check if the pipe is a curve and not a point (for whatever reason)
                    if (p.Location.GetType() == typeof(LocationCurve))
                    {
                        LocationCurve lc = (LocationCurve)p.Location;
                        // Check if the curve is a line
                        if (lc.Curve.GetType() == typeof(Line))
                        {
                            pipe_lineMap[p] = (Line)lc.Curve;
                        }
                    }
                }
                
                // Set the pipes
                data.pipelines = pipelines;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException) { }
            
            ReloadPreview();

            // Make sure the form is back in focus
            this.Activate();
        }

        /// <summary>
        /// Filter that returns only pipe placeholders
        /// </summary>
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
