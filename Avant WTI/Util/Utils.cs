using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Avant.WTI.Drip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using static Avant.WTI.Drip.DripData;
using System.Windows.Forms;

namespace Avant.WTI.Util
{
    public static class Utils
    {

        public static void HandleError(Exception ex)
        {
            Debug.WriteLine(ex.Message);
            Debug.WriteLine(ex.Source);
            Debug.WriteLine(ex.StackTrace);
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
            if (p1.IsAlmostEqualTo(p2)) return null;

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


        public static void SetSize(Pipe p, double size)
        {
            if (p.GetType() != typeof(Element)) return;
            Parameter param = p.LookupParameter("Diameter");
            if (param == null) return;
            param.Set(size);
        }

        /// <summary>
        /// Gets all (linked) documents in the document
        /// </summary>
        /// <param name="doc">Current Document</param>
        /// <returns></returns>
        public static HashSet<Document> GetAllDocuments(Document doc)
        {
            HashSet<Document> alldocs = new HashSet<Document>();
            alldocs.Add(doc);
            List<Element> links = (new FilteredElementCollector(doc))
                .OfCategory(BuiltInCategory.OST_RvtLinks)
                .WhereElementIsNotElementType()
                .ToElements() as List<Element>;
            foreach (Element el in links)
            {
                RevitLinkInstance rl = (RevitLinkInstance)el;
                alldocs.Add(rl.GetLinkDocument());
            }
            return alldocs;
        }

        /// <summary>
        /// Picks the closest pipe to an area
        /// </summary>
        /// <param name="pipes"></param>
        /// <param name="a"></param>
        /// <returns>Closest pipe to an area</returns>
        public static Pipe FindClosestPipe(List<Pipe> pipes, Area a)
        {
            if (pipes.Count == 0) return null;

            RectangleF arearect = AreaUtils.GetAreaBoundingRectangle(a);
            XYZ center = RectangleGetCenter(arearect);

            // Orders the pipes by distance to the center of the area and gets the first one.
            Pipe closest = pipes.OrderBy(p =>
            {
                Line l = (Line)((LocationCurve)p.Location).Curve;
                return l.Distance(center);
            }).ToList().First();

            return closest;
        }

        /// <summary>
        /// Calculates the bounds of all grid lines and areas
        /// </summary>
        /// <param name="data">DripData</param>
        /// <returns>Bounding Rectangle</returns>
        public static System.Drawing.RectangleF CalculateBounds(DripData data)
        {
            // Convert lines to their end points
            List<XYZ> points = new List<XYZ>();
            foreach (Line l in data.lines)
            {
                points.Add(l.GetEndPoint(0));
                points.Add(l.GetEndPoint(1));
            }

            foreach(Area area in data.areas)
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


        /// <summary>
        /// Displays all errors as a dialog and returns the maximum error severity
        /// </summary>
        /// <param name="msgs"></param>
        /// <returns></returns>
        public static DripData.DripDataErrorMessage.Severity DisplayErrors(List<DripData.DripDataErrorMessage> msgs)
        {
            if (msgs == null) return DripData.DripDataErrorMessage.Severity.NONE;

            DripData.DripDataErrorMessage.Severity maxSeverity = DripData.DripDataErrorMessage.Severity.NONE;
            for (int i = 0; i < msgs.Count; i++)
            {
                DripData.DripDataErrorMessage msg = msgs[i];

                if (msg.severity > maxSeverity) maxSeverity = msg.severity;

                string caption;
                MessageBoxIcon icon;
                switch (msg.severity)
                {
                    case DripData.DripDataErrorMessage.Severity.FATAL:
                        icon = MessageBoxIcon.Error;
                        caption = "An error occurred!";
                        break;
                    case DripData.DripDataErrorMessage.Severity.WARNING:
                        icon = MessageBoxIcon.Warning;
                        caption = "Warning!";
                        break;
                    default:
                        icon = MessageBoxIcon.Information;
                        caption = "AvantWTI";
                        break;

                }

                if (msgs.Count > 1)
                {
                    string captionSuffix = string.Format(" ({0} of {1})", i + 1, msgs.Count);
                    caption += captionSuffix;
                }

                MessageBox.Show(msg.message, caption, MessageBoxButtons.OK, icon);

            }

            return maxSeverity;
        }

    }
}