using Autodesk.Revit.DB;
using Autodesk.Revit.Exceptions;
using Avant.WTI.Generators;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

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
        /// Gets the vector from the point to the line, orthogonal to the line
        /// </summary>
        /// <param name="l">Line</param>
        /// <param name="p">Point</param>
        /// <returns>Vector</returns>
        public static XYZ GetVectorFromPointToLine(Line l, XYZ p)
        {
            XYZ closestPoint = GetClosestPoint(l, p);
            return closestPoint - p;
        }


        /// <summary>
        /// Get all points that are together the closest to a line
        /// </summary>
        /// <param name="l">Line</param>
        /// <param name="points">List of points</param>
        /// <param name="toleranceft">Tolerance to group points by</param>
        /// <returns>List of points with an equal (within tolerance) distance to the line</returns>
        public static List<XYZ> GetClosestPoints(Line l, List<XYZ> points, double toleranceft)
        {
            List<XYZ> result = new List<XYZ>();

            double min = int.MaxValue;
            foreach(XYZ p in points)
            {
                double dist = l.Distance(p);
                if(dist < min - toleranceft)
                {
                    min = dist;
                    result.Clear();
                }
                if (Math.Abs(dist - min) < toleranceft)
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
        public static XYZ BoundingBoxGetCenter(BoundingBoxXYZ bb)
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
        public static Line CreateNamedLine(XYZ b, XYZ e, string name, List<WTIData.DripErrorMessage> errorMessages)
        {
            Line line = null;
            try
            {
                line = Line.CreateBound(b, e);
            }
            catch (ArgumentsInconsistentException)
            {
                errorMessages.Add(new WTIData.DripErrorMessage(string.Format("Failed to create line '{0}', because it is too short.", name), WTIData.DripErrorMessage.Severity.WARNING));
            }
            return line;
        }


        /// <summary>
        ///  Checks if a point is inside of a rectangle
        /// </summary>
        /// <param name="rect">Rectangle</param>
        /// <param name="p">Point</param>
        /// <param name="tolerance">Tolerance</param>
        /// <returns>True if point is inside of a rectangle</returns>
        public static bool RectangleIntersect(RectangleF rect, XYZ p, float tolerance)
        {
            if (p.X < rect.Left - tolerance) return false;
            if (p.X > rect.Right + tolerance) return false;
            if (p.Y > rect.Bottom + tolerance) return false;
            if (p.Y < rect.Top - tolerance) return false;
            return true;
        }

        /// <summary>
        /// Calculates the center of a rectangle
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public static XYZ RectangleGetCenter(RectangleF r)
        {
            return new XYZ((r.Left + r.Right) / 2.0, (r.Top + r.Bottom) / 2.0, 0);
        }

        /// <summary>
        /// Transforms a point in Revit model space to canvas space
        /// </summary>
        /// <param name="p">Point to transform</param>
        /// <param name="domain">Full model space bounds to map to canvas coordinates</param>
        /// <param name="targetSize">Canvas size</param>
        /// <returns>Transformed point</returns>
        public static XYZ PointToScreenPoint(XYZ p, RectangleF domain, System.Drawing.Size targetSize)
        {
            double x = targetSize.Width * ((p.X - domain.X) / domain.Width);
            double y = targetSize.Height - targetSize.Height * ((p.Y - domain.Y) / domain.Height);

            return new XYZ(x, y, 0);
        }


        /// <summary>
        /// Transforms a point in canvas space to Revit model space
        /// </summary>
        /// <param name="p">Point to transform</param>
        /// <param name="domain">Full model space bounds to map to canvas coordinates</param>
        /// <param name="targetSize">Canvas size</param>
        /// <returns>Transformed point</returns>
        public static XYZ PointToModelPoint(XYZ p, RectangleF domain, System.Drawing.Size targetSize)
        {
            double x = p.X * domain.Width / targetSize.Width + domain.X;
            double y = (targetSize.Height - p.Y) * domain.Height / targetSize.Height + domain.Y;

            return new XYZ(x, y, 0);
        }

        /// <summary>
        /// Transform a line in Revit model space to canvas space
        /// </summary>
        /// <param name="line">Line to transform</param>
        /// <param name="domain">Full model space bounds to map to canvas coordinates</param>
        /// <param name="size">Canvas size</param>
        /// <returns>Transformed Line</returns>
        public static Line LineToScreenLine(Line line, RectangleF domain, System.Drawing.Size size)
        {
            XYZ p1 = PointToScreenPoint(line.GetEndPoint(0), domain, size);
            XYZ p2 = PointToScreenPoint(line.GetEndPoint(1), domain, size);

            // Can't create a line if the points are not far enough apart
            if (p1.DistanceTo(p2) < 0.8) return null;

            return Line.CreateBound(p1, p2);
        }

        /// <summary>
        /// Converts RectangleF to Revit lines
        /// </summary>
        /// <param name="r">Rectangle</param>
        /// <returns></returns>
        public static List<Line> RectangleToLines(RectangleF r)
        {
            List<Line> lines = new List<Line>();
            lines.Add(Line.CreateBound(new XYZ(r.Left, r.Top, 0), new XYZ(r.Right, r.Top, 0)));
            lines.Add(Line.CreateBound(new XYZ(r.Right, r.Top, 0), new XYZ(r.Right, r.Bottom, 0)));
            lines.Add(Line.CreateBound(new XYZ(r.Right, r.Bottom, 0), new XYZ(r.Left, r.Bottom, 0)));
            lines.Add(Line.CreateBound(new XYZ(r.Left, r.Bottom, 0), new XYZ(r.Left, r.Top, 0)));
            return lines;
        }

        /// <summary>
        /// Calculates the bounds of all grid lines and areas
        /// </summary>
        /// <param name="data">DripData</param>
        /// <returns>Bounding Rectangle</returns>
        public static System.Drawing.RectangleF CalculateBounds(WTIData data)
        {
            // Convert lines to their end points
            List<XYZ> points = new List<XYZ>();
            foreach (Line l in data.lines)
            {
                points.Add(l.GetEndPoint(0));
                points.Add(l.GetEndPoint(1));
            }

            foreach (Area area in data.areas)
            {
                RectangleF bounds = AreaUtils.GetAreaBoundingRectangle(area);
                points.Add(new XYZ(bounds.Left, bounds.Top, 0));
                points.Add(new XYZ(bounds.Right, bounds.Top, 0));
                points.Add(new XYZ(bounds.Right, bounds.Bottom, 0));
                points.Add(new XYZ(bounds.Left, bounds.Bottom, 0));
            }


            // Check if we even have points, otherwise return default rectangle
            if (points.Count == 0) return new System.Drawing.RectangleF(0, 0, 1, 1);

            float left = (float)points.Min(p => p.X);
            float right = (float)points.Max(p => p.X);
            float top = (float)points.Min(p => p.Y);
            float bottom = (float)points.Max(p => p.Y);

            return new System.Drawing.RectangleF(left, top, right - left, bottom - top);
        }

    }
}
