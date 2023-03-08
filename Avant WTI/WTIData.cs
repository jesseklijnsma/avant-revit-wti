using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Avant.WTI.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Avant.WTI
{
    /// <summary>
    /// This class serves as all input data for the WTI Form. 
    /// And also as the output data
    /// </summary>
    public class WTIData
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

        public Dictionary<Area, XYZ> overrideValvePoints = new Dictionary<Area, XYZ>();

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
        public readonly List<RenderPoint> previewPoints = new List<RenderPoint>();
        public readonly List<Line> debugLines = new List<Line>();
        public readonly Dictionary<Area, RenderPoint> valvePoints = new Dictionary<Area, RenderPoint>();

        public double minimumPipeLengthFt = 0.5;

        // Error messages
        public List<DripErrorMessage> errorMessages = new List<DripErrorMessage>();

        public WTIData(Document document, UIDocument uidoc)
        {
            this.doc = document;
            this.uidoc = uidoc;
        }


        public void LoadPrevious()
        {
            pipetype = pipetypes.Find(p => p.Name.Equals(Properties.Settings.Default.PreviousPipeType));

            transportSystemType = systemtypes.Find(p => p.Name.Equals(Properties.Settings.Default.PreviousTransportSystem));
            distributionSystemType = systemtypes.Find(p => p.Name.Equals(Properties.Settings.Default.PreviousDistributionSystem));

            valvefamily = valvefamilies.Find(f => f.Name.Equals(Properties.Settings.Default.PreviousValveFamily));
            valveheight = (int)Properties.Settings.Default.PreviousValveHeight;

            intermediateDistance = (int)Properties.Settings.Default.PreviousIntermediateDistance;
            backwallDistance = (int)Properties.Settings.Default.PreviousBackwallDistance;

            valvecolumnDistance = (int)Properties.Settings.Default.PreviousValveColumnDistance;
            pipecolumnDistance = (int)Properties.Settings.Default.PreviousPipeColumnDistance;

            transportlineheight = (int)Properties.Settings.Default.PreviousTransportHeight;
            distributionlineheight = (int)Properties.Settings.Default.PreviousDistributionHeight;

            convertPlaceholders = Properties.Settings.Default.PreviousDoConvertPlaceholders;

            transport_diameter = Properties.Settings.Default.PreviousTransportDiameter;
            distribution_diameter = Properties.Settings.Default.PreviousDistributionDiameter;
        }

        public void refreshErrorMessages(Data d)
        {
            errorMessages.Clear();

            if (d == Data.INPUT)
            {
                if (uidoc == null)
                {
                    errorMessages.Add(new DripErrorMessage("Active UI document is null.", DripErrorMessage.Severity.FATAL));
                }
                if (doc == null)
                {
                    errorMessages.Add(new DripErrorMessage("Active document is null.", DripErrorMessage.Severity.FATAL));
                }
                if(pipetypes.Count == 0)
                {
                    errorMessages.Add(new DripErrorMessage("No pipetypes found in this document.", DripErrorMessage.Severity.WARNING));
                }
                if (pipetypes.Count != pipesizeMap.Count)
                {
                    errorMessages.Add(new DripErrorMessage("Not all pipes have a corresponding sizes.", DripErrorMessage.Severity.WARNING));
                    // TODO handle ui
                }
                foreach (KeyValuePair<PipeType, List<double>> kv in pipesizeMap){
                    PipeType pt = kv.Key;
                    List<double> sizes = kv.Value;
                    if (sizes.Count == 0) errorMessages.Add(new DripErrorMessage(string.Format("{0} does not have any corresponding sizes.", pt.Name), DripErrorMessage.Severity.WARNING));
                    // TODO handle ui
                }
                if (systemtypes.Count == 0)
                {
                    errorMessages.Add(new DripErrorMessage("No piping system types found in this document.", DripErrorMessage.Severity.WARNING));
                }
                if (valvefamilies.Count == 0)
                {
                    errorMessages.Add(new DripErrorMessage("No valve or pipe accessories found in this document.", DripErrorMessage.Severity.WARNING));
                }

                if (areas.Count == 0)
                {
                    errorMessages.Add(new DripErrorMessage("No areas found in this document.", DripErrorMessage.Severity.FATAL));
                }

                if(columnpoints.Count == 0)
                {
                    errorMessages.Add(new DripErrorMessage("No columns found in this document", DripErrorMessage.Severity.FATAL));
                }

                if(lines.Count == 0)
                {
                    errorMessages.Add(new DripErrorMessage("No grids found in this document", DripErrorMessage.Severity.WARNING));
                    // TODO handle ui
                    // TODO convert grid bounded to area bounded
                }

                if(groundLevel == null)
                {
                    errorMessages.Add(new DripErrorMessage("No levels found in this document.", DripErrorMessage.Severity.FATAL));
                }
            }
            else if (d == Data.OUTPUT)
            {
                if(pipetype == null)
                {
                    errorMessages.Add(new DripErrorMessage("No pipe type selected.", DripErrorMessage.Severity.FATAL));
                }
                if (transportSystemType == null)
                {
                    errorMessages.Add(new DripErrorMessage("No transport pipe system type selected.", DripErrorMessage.Severity.FATAL));
                }
                if (distributionSystemType == null)
                {
                    errorMessages.Add(new DripErrorMessage("No distribtion pipe system type selected.", DripErrorMessage.Severity.FATAL));
                }
                if (valvefamily == null)
                {
                    errorMessages.Add(new DripErrorMessage("No valve family selected. Creating dummy connections instead", DripErrorMessage.Severity.WARNING));
                }

                MEPSystemClassification globalClassification = transportSystemType.SystemClassification;
                if (globalClassification != distributionSystemType.SystemClassification)
                {
                    errorMessages.Add(new DripErrorMessage("System Classification cannot be different between system types.", DripErrorMessage.Severity.FATAL));
                }
                else
                {
                    foreach(Pipe pipe in pipelines)
                    {
                        PipingSystem ps = (PipingSystem)pipe.MEPSystem;
                        try
                        {
                            PipingSystemType pst = (PipingSystemType)doc.GetElement(ps.GetTypeId());
                            if (pst.SystemClassification != globalClassification)
                            {
                                string pipeClass = Enum.GetName(typeof(MEPSystemClassification), pst.SystemClassification);
                                string newClass = Enum.GetName(typeof(MEPSystemClassification), globalClassification);


                                errorMessages.Add(new DripErrorMessage(string.Format("Source pipe ({0}) is of system classification {1}, but the system classification of the set system types is {2}.", pipe.Id, pipeClass, newClass), DripErrorMessage.Severity.FATAL));
                            }
                        }
                        catch (Exception) { }
                    }
                }
            }

            errorMessages = errorMessages.OrderByDescending(m => m.severity).ToList();
        }

        public enum Data
        {
            INPUT,
            OUTPUT
        }

        public class DripErrorMessage
        {

            public string message { get; }
            public Severity severity { get; }
            public bool unique { get; }

            public DripErrorMessage(string message, Severity s, bool unique = true)
            {
                this.message = message;
                this.severity = s;
                this.unique = unique;
            }

            public override bool Equals(object obj)
            {
                if (obj == null || GetType() != obj.GetType()) return false;

                DripErrorMessage other = (DripErrorMessage)obj;
                if (unique) return base.Equals(obj);
                return message.Equals(other.message);
            }

            public override int GetHashCode()
            {
                int hashCode = 1300177356;
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(message);
                hashCode = hashCode * -1521134295 + severity.GetHashCode();
                hashCode = hashCode * -1521134295 + unique.GetHashCode();
                return hashCode;
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
