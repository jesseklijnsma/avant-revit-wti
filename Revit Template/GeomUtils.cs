using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace RevitTemplate
{
    internal class GeomUtils
    {

        public static XYZ getClosestPoint(Line l, XYZ p)
        {
            XYZ dir = l.Direction.Normalize();
            XYZ lhs = p.Subtract(l.Origin);
            double dotP = lhs.DotProduct(dir);
            return l.Origin.Add(dir.Multiply(dotP));
        }

        public static List<XYZ> getClosestPoints(Line l, List<XYZ> points, double margin)
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
                if (Math.Abs(dist - min) < margin)
                {
                    result.Add(p);
                }
            }

            return result;
        }

    }
}
