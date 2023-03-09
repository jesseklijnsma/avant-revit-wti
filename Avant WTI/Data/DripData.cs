using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avant.WTI.Data
{
    public class DripData
    {


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


    }
}
