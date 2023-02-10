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

        /// <summary>
        /// Set the Z coordinate of a point/vector
        /// </summary>
        /// <param name="p">Point</param>
        /// <param name="height">New Z</param>
        /// <returns>Transformed point</returns>
        public static XYZ Vector_setZ(XYZ p, double height)
        {
            return new XYZ(p.X, p.Y, height);
        }

        /// <summary>
        ///  Multiplies a and b term by term
        /// </summary>
        /// <param name="a">First vector</param>
        /// <param name="b">Second vector</param>
        /// <returns>Masked vector</returns>
        public static XYZ Vector_mask(XYZ a, XYZ b)
        {
            return new XYZ(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }

        /// <summary>
        /// Rounds all terms of a vector
        /// </summary>
        /// <param name="v">Vector</param>
        /// <returns>Rounded vector</returns>
        internal static XYZ Vector_round(XYZ v)
        {
            return new XYZ(Math.Round(v.X), Math.Round(v.Y), Math.Round(v.Z));
        }
    }
}
