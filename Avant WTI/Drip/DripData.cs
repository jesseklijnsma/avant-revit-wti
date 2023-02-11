using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Avant.WTI.Drip
{
    /// <summary>
    /// This class serves as all input data for the WTI Form. 
    /// And also as the output data
    /// </summary>
    public class DripData
    {

        // Input data
        //  All these fields need to be valid in order to show the window
        public Document doc;
        public UIDocument uidoc;

        public List<PipeType> pipetypes = new List<PipeType>();
        public Dictionary<PipeType, List<double>> pipesizeMap = new Dictionary<PipeType, List<double>>();
        public List<PipingSystemType> systemtypes = new List<PipingSystemType>();
        public List<FamilySymbol> valvefamilies = new List<FamilySymbol>();

        public List<Area> areas = new List<Area>();
        public List<XYZ> columnpoints = new List<XYZ>();
        public List<Line> lines = new List<Line>();

        public Level groundLevel;

        // Output data
        //  All these outputs need to be valid in order to run the drip generator
        public List<Pipe> pipelines = new List<Pipe>();
        public Dictionary<Area, Pipe> areapipemap = new Dictionary<Area, Pipe>();
        public PipeType pipetype = null;

        public PipingSystemType transportSystemType = null;
        public double transport_diameter = 110;
        public PipingSystemType distributionSystemType = null;
        public double distribution_diameter = 75;

        public FamilySymbol valvefamily = null;

        public int valveheight = 0;

        public int transportlineheight = -400;
        public int distributionlineheight = 0;

        public int intermediateDistance = 1000;
        public int backwallDistance = 1000;
        public int valvecolumnDistance = 500;
        public int pipecolumnDistance = 500;

        public bool convertPlaceholders = true;


        // Misc
        public readonly List<Line> previewGeometry = new List<Line>();
        public readonly List<XYZ> previewPoints = new List<XYZ>();
        public readonly List<Line> debugLines = new List<Line>();

        public DripData(Document document, UIDocument uidoc)
        {
            this.doc = document;
            this.uidoc = uidoc;
        }


        public void LoadPrevious()
        {
            if(this.pipetypes.Count > 0) this.pipetype = this.pipetypes.Find(p => p.Name.Equals(Properties.Settings.Default.PreviousPipeType));

            if (this.systemtypes.Count > 0)
            {
                this.transportSystemType = this.systemtypes.Find(p => p.Name.Equals(Properties.Settings.Default.PreviousTransportSystem));
                this.distributionSystemType = this.systemtypes.Find(p => p.Name.Equals(Properties.Settings.Default.PreviousDistributionSystem));
            }

            if(this.valvefamilies.Count > 0) this.valvefamily = this.valvefamilies.Find(f => f.Name.Equals(Properties.Settings.Default.PreviousValveFamily));

            this.intermediateDistance = (int)Properties.Settings.Default.PreviousIntermediateDistance;
            this.backwallDistance = (int)Properties.Settings.Default.PreviousBackwallDistance;

            this.valvecolumnDistance = (int)Properties.Settings.Default.PreviousValveColumnDistance;
            this.pipecolumnDistance = (int)Properties.Settings.Default.PreviousPipeColumnDistance;

            this.transportlineheight = (int)Properties.Settings.Default.PreviousTransportHeight;
            this.distributionlineheight = (int)Properties.Settings.Default.PreviousDistributionHeight;

            this.convertPlaceholders = Properties.Settings.Default.PreviousDoConvertPlaceholders;

            this.transport_diameter = Properties.Settings.Default.PreviousTransportDiameter;
            this.distribution_diameter = Properties.Settings.Default.PreviousDistributionDiameter;
        }

        public List<DripDataErrorMessage> getErrorMessages(Data d)
        {
            List<DripDataErrorMessage> messages = new List<DripDataErrorMessage>();

            if (d == Data.INPUT)
            {
                if (this.uidoc == null)
                {
                    messages.Add(new DripDataErrorMessage("Active UI document is null.", DripDataErrorMessage.Severity.FATAL));
                }
                if (this.doc == null)
                {
                    messages.Add(new DripDataErrorMessage("Active document is null.", DripDataErrorMessage.Severity.FATAL));
                }
                if(this.pipetypes.Count == 0)
                {
                    messages.Add(new DripDataErrorMessage("No pipetypes found in this document.", DripDataErrorMessage.Severity.WARNING));
                    // TODO handle ui
                }
                if (this.pipetypes.Count != this.pipesizeMap.Count)
                {
                    messages.Add(new DripDataErrorMessage("Not all pipes have a corresponding sizes.", DripDataErrorMessage.Severity.WARNING));
                    // TODO handle ui
                }
                foreach (KeyValuePair<PipeType, List<double>> kv in this.pipesizeMap){
                    PipeType pt = kv.Key;
                    List<double> sizes = kv.Value;
                    if (sizes.Count == 0) messages.Add(new DripDataErrorMessage(string.Format("{0} does not have any corresponding sizes.", pt.Name), DripDataErrorMessage.Severity.WARNING));
                    // TODO handle ui
                }
                if (systemtypes.Count == 0)
                {
                    messages.Add(new DripDataErrorMessage("No piping system types found in this document.", DripDataErrorMessage.Severity.WARNING));
                    // TODO handle ui
                }
                if (valvefamilies.Count == 0)
                {
                    messages.Add(new DripDataErrorMessage("No valve or pipe accessories found in this document.", DripDataErrorMessage.Severity.WARNING));
                    // TODO handle ui
                }

                if (areas.Count == 0)
                {
                    messages.Add(new DripDataErrorMessage("No areas found in this document.", DripDataErrorMessage.Severity.FATAL));
                }

                if(columnpoints.Count == 0)
                {
                    messages.Add(new DripDataErrorMessage("No columns found in this document", DripDataErrorMessage.Severity.FATAL));
                }

                if(lines.Count == 0)
                {
                    messages.Add(new DripDataErrorMessage("No grids found in this document", DripDataErrorMessage.Severity.WARNING));
                    // TODO handle ui
                    // TODO convert grid bounded to area bounded
                }

                if(groundLevel == null)
                {
                    messages.Add(new DripDataErrorMessage("No levels found in this document.", DripDataErrorMessage.Severity.FATAL));
                }
            }
            else if (d == Data.OUTPUT)
            {
                if(pipetype == null)
                {
                    messages.Add(new DripDataErrorMessage("No pipe type selected.", DripDataErrorMessage.Severity.FATAL));
                }
                if (transportSystemType == null)
                {
                    messages.Add(new DripDataErrorMessage("No transport pipe system type selected.", DripDataErrorMessage.Severity.FATAL));
                }
                if (distributionSystemType == null)
                {
                    messages.Add(new DripDataErrorMessage("No distribtion pipe system type selected.", DripDataErrorMessage.Severity.FATAL));
                }
                if (valvefamily == null)
                {
                    messages.Add(new DripDataErrorMessage("No valve family selected.\nCreating dummy connections instead", DripDataErrorMessage.Severity.WARNING));
                }
            }

            messages = messages.OrderByDescending(m => m.severity).ToList();

            return messages;
        }

        public enum Data
        {
            INPUT,
            OUTPUT
        }

        public class DripDataErrorMessage
        {

            public string message { get; }
            public Severity severity { get; }

            public DripDataErrorMessage(string message, Severity s)
            {
                this.message = message;
                this.severity = s;
            }

            public enum Severity
            {
                FATAL = 2,
                WARNING = 1,
                NONE = 0
            }

        }

    }
}
