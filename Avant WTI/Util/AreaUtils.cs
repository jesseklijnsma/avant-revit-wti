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
        public static PolyLine GetAreaPolyLine(Area area)
        {
            // Get boundaries
            IList<IList<BoundarySegment>> bounds = area.GetBoundarySegments(new SpatialElementBoundaryOptions());

            if (bounds.Count == 0) throw new Exception(string.Format("Area {0} doesn't have any regions.", area.Name));
            if (bounds.Count > 1) throw new Exception(string.Format("Area {0} has multiple regions.", area.Name));

            // Get all corner points of boundary
            List<XYZ> points = new List<XYZ>();
            IList<BoundarySegment> boundList = bounds[0];
            foreach (BoundarySegment b in boundList)
            {
                points.Add(b.GetCurve().GetEndPoint(0));
            }

            // Add last point
            if (boundList.Count > 0) points.Add(boundList[boundList.Count - 1].GetCurve().GetEndPoint(1));

            // Check if enough points
            if (points.Count < 2) throw new Exception(string.Format("Area {0} doesn't have enough corner points", area.Name));

            return PolyLine.Create(points);
        }

        /// <summary>
        /// Get the axis aligned bounding rectangle of an area
        /// </summary>
        /// <param name="area">Area</param>
        /// <returns>Rectangle</returns>
        public static System.Drawing.RectangleF GetAreaBoundingRectangle(Area area)
        {
            PolyLine pl = GetAreaPolyLine(area);
            IList<XYZ> points = pl.GetCoordinates();

            double minX = points.Min(p => p.X);
            double maxX = points.Max(p => p.X);
            double minY = points.Min(p => p.Y);
            double maxY = points.Max(p => p.Y);

            RectangleF arearect = new RectangleF(
                (float)minX,
                (float)minY,
                (float)(maxX - minX),
                (float)(maxY - minY)
            );

            return arearect;
        }
    }
}
