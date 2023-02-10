using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avant.WTI.Util
{
    internal class AreaUtils
    {

        /// <summary>
        /// Gets a curveloop of a simple area
        /// </summary>
        /// <param name="area">Area</param>
        /// <returns>Curveloop or null</returns>
        public static CurveLoop GetAreaCurveloop(Area area)
        {
            // Get boundaries
            IList<IList<BoundarySegment>> bounds = area.GetBoundarySegments(new SpatialElementBoundaryOptions());

            // Convert boundaries to curves
            List<Curve> curves = new List<Curve>();
            foreach(IList<BoundarySegment> boundlist in bounds)
            {
                foreach(BoundarySegment b in boundlist)
                {
                    curves.Add(b.GetCurve());
                }
            }
            // Convert curves to curveloop
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

        /// <summary>
        /// Get a rectangle from the area
        /// </summary>
        /// <param name="area">Area</param>
        /// <returns>Rectangle representing the area or default rectangle</returns>
        public static System.Drawing.RectangleF GetAreaRectangle(Area area)
        {
            CurveLoop cl = GetAreaCurveloop(area);
            if (cl == null) return new System.Drawing.RectangleF(0,0,1,1);
            
            Plane areaPlane = cl.GetPlane();
            if (areaPlane == null || !cl.IsRectangular(areaPlane)) return new System.Drawing.RectangleF(0, 0, 1, 1);

            XYZ center = areaPlane.Origin;
            double w = cl.GetRectangularWidth(areaPlane);
            double h = cl.GetRectangularHeight(areaPlane);

            // Convert width and height in the areaPlane space to world space and represent as a vector
            XYZ areavector = areaPlane.XVec.Multiply(w).Add(areaPlane.YVec.Multiply(h));

            // Calculate all bounding points of the area
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
