using Autodesk.Revit.DB;
using Autodesk.Revit.Exceptions;
using Avant.WTI.Drip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Avant.WTI.Util
{
    internal class GeomUtils
    {

        /// <summary>
        /// Gets the point on a line closest to the point
        /// </summary>
        /// <param name="l">Line</param>
        /// <param name="p">Point</param>
        /// <returns>Closest Point</returns>
        public static XYZ GetClosestPoint(Line l, XYZ p)
        {
            XYZ dir = l.Direction.Normalize();
            XYZ lhs = p.Subtract(l.Origin);
            double dotP = lhs.DotProduct(dir);
            return l.Origin.Add(dir.Multiply(dotP));
        }


        /// <summary>
        /// Get all points that are together the closest to a line
        /// </summary>
        /// <param name="l">Line</param>
        /// <param name="points">List of points</param>
        /// <param name="tolerance">Tolerance to group points by</param>
        /// <returns>List of points with an equal (within tolerance) distance to the line</returns>
        public static List<XYZ> GetClosestPoints(Line l, List<XYZ> points, double tolerance)
        {
            List<XYZ> result = new List<XYZ>();

            double min = int.MaxValue;
            foreach(XYZ p in points)
            {
                double dist = l.Distance(p);
                if(dist < min)
                {
                    min = dist;
                    result.Clear();
                }
                if (Math.Abs(dist - min) < tolerance)
                {
                    result.Add(p);
                }
            }

            return result;
        }

        /// <summary>
        ///  Gets the center of a boundingbox
        /// </summary>
        /// <param name="bb">Boundingbox</param>
        /// <returns>Center point</returns>
        public static XYZ boundingBoxGetCenter(BoundingBoxXYZ bb)
        {
            return (bb.Max + bb.Min) / 2;
        }


        /// <summary>
        ///  Tries to create a line and adds an error message to the list if necessary
        /// </summary>
        /// <param name="b">Begin point</param>
        /// <param name="e">End point</param>
        /// <param name="name">Description of line</param>
        /// <param name="errorMessages">List of error messages</param>
        /// <returns></returns>
        public static Line CreateNamedLine(XYZ b, XYZ e, string name, List<DripData.DripErrorMessage> errorMessages)
        {
            Line line = null;
            try
            {
                line = Line.CreateBound(b, e);
            }
            catch (ArgumentsInconsistentException)
            {
                errorMessages.Add(new DripData.DripErrorMessage(string.Format("Failed to create line '{0}', because it is too short.", name), DripData.DripErrorMessage.Severity.WARNING));
            }
            return line;
        }

    }
}
