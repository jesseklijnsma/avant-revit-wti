using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Avant.WTI.Util
{
    public static class Utils
    {

        

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
            XYZ center = GeomUtils.RectangleGetCenter(arearect);

            // Orders the pipes by distance to the center of the area and gets the first one.
            Pipe closest = pipes.OrderBy(p =>
            {
                Line l = (Line)((LocationCurve)p.Location).Curve;
                return l.Distance(center);
            }).ToList().First();

            return closest;
        }

        /// <summary>
        /// Activates a FamilySymbol if necessary
        /// </summary>
        /// <param name="doc">Document</param>
        /// <param name="family">FamilySymbol</param>
        public static void EnsureFamilyActive(Document doc, FamilySymbol family)
        {
            if (!family.IsActive)
            {
                family.Activate();
                doc.Regenerate();
            }
        }

    }
}