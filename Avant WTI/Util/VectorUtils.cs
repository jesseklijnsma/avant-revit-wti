using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avant.WTI.Util
{
    internal class VectorUtils
    {

        public static XYZ vector_setZ(XYZ p, double height)
        {
            return new XYZ(p.X, p.Y, height);
        }

        public static XYZ vector_mask(XYZ a, XYZ b)
        {
            return new XYZ(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }

        internal static XYZ vector_round(XYZ v)
        {
            return new XYZ(Math.Round(v.X), Math.Round(v.Y), Math.Round(v.Z));
        }
    }
}
