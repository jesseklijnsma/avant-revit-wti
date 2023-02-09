using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

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

        public bool isValidInput()
        {
            if (doc == null) return false;
            if (uidoc == null) return false;

            if (pipetypes.Count == 0) return false;
            if (pipetypes.Count != pipesizeMap.Keys.Count) return false;
            if (systemtypes.Count == 0) return false;
            if (valvefamilies.Count == 0) return false;

            if (areas.Count == 0) return false;
            if (columnpoints.Count == 0) return false;
            if (lines.Count == 0) return false;

            if (groundLevel == null) return false;
            return true;
        }

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


        // Misc.
        public List<Line> previewGeometry = new List<Line>();
        public List<XYZ> previewPoints = new List<XYZ>();
        public List<Line> debugLines = new List<Line>();

        /// <summary>
        /// 
        /// </summary>
        /// <returns>A boolean indicating whether all output data is valid</returns>
        public bool isValidOutput()
        {
            if (pipetype == null) return false;
            if (transportSystemType == null) return false;
            if (distributionSystemType == null) return false;
            if (valvefamily == null) return false;
            return true;
        }


        public DripData(Document document, UIDocument uidoc)
        {
            this.doc = document;
            this.uidoc = uidoc;
        }

    }
}
