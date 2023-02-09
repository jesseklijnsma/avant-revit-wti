using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitTemplate
{
    internal class AreaUtils
    {
        public static CurveLoop getAreaCurveloop(Area area)
        {
            IList<IList<BoundarySegment>> bounds = area.GetBoundarySegments(new SpatialElementBoundaryOptions());
            List<Curve> curves = new List<Curve>();
            foreach(IList<BoundarySegment> boundlist in bounds)
            {
                foreach(BoundarySegment b in boundlist)
                {
                    curves.Add(b.GetCurve());
                }
            }
            try
            {
                return CurveLoop.Create(curves);

            }
            catch
            {
                Console.WriteLine("Failed to create curveloop from area");
                return null;
            }
        }


        public static System.Drawing.RectangleF getAreaRectangle(Area area)
        {
            CurveLoop cl = getAreaCurveloop(area);
            if (cl == null) return new System.Drawing.RectangleF(0,0,1,1);
            
            Plane areaPlane = cl.GetPlane();
            if (areaPlane == null || !cl.IsRectangular(areaPlane)) return new System.Drawing.RectangleF(0, 0, 1, 1);

            XYZ center = areaPlane.Origin;
            double w = cl.GetRectangularWidth(areaPlane);
            double h = cl.GetRectangularHeight(areaPlane);

            XYZ areavector = areaPlane.XVec.Multiply(w).Add(areaPlane.YVec.Multiply(h));
            float x1 = (float)center.Add(areavector.Multiply(-0.5)).X;
            float x2 = (float)center.Add(areavector.Multiply(0.5)).X;
            float y1 = (float)center.Add(areavector.Multiply(0.5)).Y;
            float y2 = (float)center.Add(areavector.Multiply(-0.5)).Y;

            RectangleF arearect = new RectangleF(
                Math.Min(x1, x2),
                Math.Min(y1, y2),
                Math.Abs(x1 - x2),
                Math.Abs(y1 - y2)
            );

            return arearect;
        }
    }
}
