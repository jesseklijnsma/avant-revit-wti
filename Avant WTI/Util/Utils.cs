using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Avant.WTI.Drip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;

namespace Avant.WTI.Util
{
    public static class Utils
    {
        public static void LogThreadInfo(string name = "")
        {
            Thread th = Thread.CurrentThread;
            Debug.WriteLine($"Task Thread ID: {th.ManagedThreadId}, Thread Name: {th.Name}, Process Name: {name}");
        }

        public static void HandleError(Exception ex)
        {
            Debug.WriteLine(ex.Message);
            Debug.WriteLine(ex.Source);
            Debug.WriteLine(ex.StackTrace);
        }

        public static Line transformLine(Line line, Transform transform)
        {
            return Line.CreateBound(transformPoint(line.GetEndPoint(0), transform), transformPoint(line.GetEndPoint(1), transform));
        }

        public static XYZ transformPoint(XYZ p, Transform transform)
        {
            XYZ p2 = p.Add(transform.Origin);
            return new XYZ(p2.X * transform.BasisX.X, p2.Y * transform.BasisY.Y, p2.Z * transform.BasisZ.Z);
        }

        //public static XYZ pointToScreenPoint(XYZ p, Transform transform, System.Drawing.Size size)
        //{
        //    XYZ p2 = p.Add(transform.Origin);
        //    return new XYZ(p2.X * transform.BasisX.X, size.Height - p2.Y * transform.BasisY.Y, 0);
        //}

        public static XYZ pointToScreenPoint(XYZ p, RectangleF domain, System.Drawing.Size targetSize)
        {
            double x = targetSize.Width * ((p.X - domain.X) / domain.Width);
            double y = targetSize.Height - targetSize.Height * ((p.Y - domain.Y) / domain.Height);

            return new XYZ(x, y, 0);
        }

        public static Line lineToScreenLine(Line line, RectangleF domain, System.Drawing.Size size)
        {
            XYZ p1 = pointToScreenPoint(line.GetEndPoint(0), domain, size);
            XYZ p2 = pointToScreenPoint(line.GetEndPoint(1), domain, size);

            if (p1.IsAlmostEqualTo(p2)) return null;

            return Line.CreateBound(p1, p2);
        }

        public static List<Line> rectangleToLines(RectangleF r)
        {
            List<Line> lines = new List<Line>();
            lines.Add(Line.CreateBound(new XYZ(r.Left, r.Top, 0), new XYZ(r.Right, r.Top, 0)));
            lines.Add(Line.CreateBound(new XYZ(r.Right, r.Top, 0), new XYZ(r.Right, r.Bottom, 0)));
            lines.Add(Line.CreateBound(new XYZ(r.Right, r.Bottom, 0), new XYZ(r.Left, r.Bottom, 0)));
            lines.Add(Line.CreateBound(new XYZ(r.Left, r.Bottom, 0), new XYZ(r.Left, r.Top, 0)));
            return lines;
        }

        public static bool rectangleIntersect(RectangleF rect, XYZ p, int margin)
        {
            if (p.X < rect.Left - margin) return false;
            if (p.X > rect.Right + margin) return false;
            if (p.Y > rect.Bottom + margin) return false;
            if (p.Y < rect.Top - margin) return false;
            return true;
        }

        public static XYZ rectangleGetCenter(RectangleF r)
        {
            return new XYZ((r.Left + r.Right) / 2.0, (r.Top + r.Bottom) / 2.0, 0);
        }


        public static void setSize(Pipe p, double size)
        {
            if (p.GetType() != typeof(Element)) return;
            Parameter param = p.LookupParameter("Diameter");
            if (param == null) return;
            param.Set(size);
        }

        public static HashSet<Document> getAllDocuments(Document doc)
        {
            HashSet<Document> alldocs = new HashSet<Document>();
            alldocs.Add(doc);
            List<Element> links = (new FilteredElementCollector(doc))
                .OfCategory(BuiltInCategory.OST_RvtLinks)
                .WhereElementIsNotElementType()
                .ToElements() as List<Element>;
            foreach (Element el in links)
            {
                if (el.GetType() == typeof(RevitLinkInstance))
                {
                    RevitLinkInstance rl = (RevitLinkInstance)el;
                    alldocs.Add(rl.GetLinkDocument());
                }
            }
            return alldocs;
        }

        public static Pipe findClosestPipe(List<Pipe> pipes, Area a)
        {
            if (pipes.Count == 0) return null;

            RectangleF arearect = AreaUtils.getAreaRectangle(a);
            XYZ center = rectangleGetCenter(arearect);


            Pipe closest = pipes.OrderBy(p =>
            {
                Line l = (Line)((LocationCurve)p.Location).Curve;
                return l.Distance(center);
            }).ToList().First();

            return closest;
        }

        public static System.Drawing.RectangleF calculateBounds(DripData data)
        {
            List<XYZ> points = new List<XYZ>();
            foreach (Line l in data.lines)
            {
                points.Add(l.GetEndPoint(0));
                points.Add(l.GetEndPoint(1));
            }

            if (points.Count == 0) return new System.Drawing.RectangleF(0, 0, 1, 1);

            float left = (float)points.Min(p => p.X);
            float right = (float)points.Max(p => p.X);
            float top = (float)points.Min(p => p.Y);
            float bottom = (float)points.Max(p => p.Y);
            return new System.Drawing.RectangleF(left, top, right - left, bottom - top);
        }


    }
}