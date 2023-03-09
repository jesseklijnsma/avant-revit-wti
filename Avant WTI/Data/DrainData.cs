using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avant.WTI.Data
{
    public class DrainData
    {

        public bool enabled = false;

        public XYZ collectorPoint = XYZ.Zero;
        public XYZ collectorDirection = XYZ.BasisY;

    }
}
