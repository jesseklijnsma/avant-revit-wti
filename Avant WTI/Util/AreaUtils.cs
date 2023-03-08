using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Avant.WTI.Drip.DripData;

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

            if (bounds.Count == 0) return null;

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

            // Check if polyline exists, otherwise return default rectangle
            if (pl == null) return new RectangleF(0,0,1,1);
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

        public static (XYZ rootVector, XYZ perpendicularVector) GetAreaVectors(Area area, XYZ preferredRoot)
        {
            RectangleF arearect = AreaUtils.GetAreaBoundingRectangle(area);
            //XYZ center = GeomUtils.RectangleGetCenter(arearect);
            XYZ areavector = new XYZ(arearect.Width, arearect.Height, 0);

            //// Get line of source pipe
            //Line sourceline = ((LocationCurve)source.Location).Curve as Line;
            //// Get closest point on source pipe line to area center
            //XYZ connectionPoint = GeomUtils.GetClosestPoint(sourceline, center);

            // Gets the vector from the source point to the center
            //XYZ branchinwardvector = VectorUtils.Vector_setZ(center.Subtract(connectionPoint),0).Normalize();
            //// Round the vector to create an axis unitvector
            //branchinwardvector = VectorUtils.Vector_round(branchinwardvector);

            preferredRoot = VectorUtils.Vector_setZ(preferredRoot, 0);
            preferredRoot = preferredRoot.Normalize();
            preferredRoot = VectorUtils.Vector_round(preferredRoot);

            XYZ rootVector = VectorUtils.Vector_mask(preferredRoot, areavector);
            XYZ perpendicularVector = VectorUtils.Vector_mask(preferredRoot.CrossProduct(XYZ.BasisZ), areavector);

            return (rootVector, perpendicularVector);
        }

        public static List<XYZ> GetPointsInArea(Area area, List<XYZ> points)
        {
            RectangleF arearect = AreaUtils.GetAreaBoundingRectangle(area);
            // Get all points inside of the area
            List<XYZ> insidePoints = new List<XYZ>();
            foreach (XYZ p in points)
            {
                if (GeomUtils.RectangleIntersect(arearect, p, tolerance: 1))
                {
                    insidePoints.Add(p);
                }
            }
            return insidePoints;
        }



    }
}
