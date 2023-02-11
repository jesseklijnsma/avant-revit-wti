using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Avant.WTI.Util
{

    /// <summary>
    /// Functions as an interface for the revit model
    /// </summary>
    internal class WTIElementCollector
    {

        private HashSet<Document> allDocuments;
        private Document doc;


        public WTIElementCollector(Document mainDocument, HashSet<Document> allDocuments)
        {
            if (allDocuments == null) throw new ArgumentNullException();
            this.allDocuments = allDocuments;
            this.doc = mainDocument;
        }


        /// <summary>
        /// Gets all areas in all documents
        /// </summary>
        /// <returns>All Areas</returns>
        public List<Area> GetAreas()
        {
            List<Area> areas = new List<Area>();

            foreach (Document d in allDocuments)
            {
                List<Element> elements = (new FilteredElementCollector(d))
                .OfCategory(BuiltInCategory.OST_Areas)
                .WhereElementIsNotElementType()
                .ToElements() as List<Element>;
                foreach (Element e in elements)
                {
                    areas.Add((Area)e);
                }
            }

            return areas;
        }


        /// <summary>
        /// Gets all available pipe types in the current document
        /// </summary>
        /// <returns>All pipetypes</returns>
        public List<PipeType> GetPipeTypes()
        {
            List<PipeType> pipetypes = new List<PipeType>();

            List<Element> elements = (new FilteredElementCollector(doc))
            .OfCategory(BuiltInCategory.OST_PipeCurves)
            .WhereElementIsElementType()
            .ToElements() as List<Element>;
            foreach (Element e in elements)
            {
                PipeType pt = (PipeType)e;
                pipetypes.Add(pt);
            }

            // Sort the pipe types by likelyhood of being used
            pipetypes = pipetypes.OrderBy(pst => {
                int n = pst.Name.IndexOf("PN", StringComparison.OrdinalIgnoreCase);
                return n == -1 ? int.MaxValue : n;
            }).ToList();

            pipetypes = pipetypes.OrderBy(pst => {
                int n = pst.Name.IndexOf("PVC", StringComparison.OrdinalIgnoreCase);
                return n == -1 ? int.MaxValue : n;
            }).ToList();

            return pipetypes;
        }

        /// <summary>
        /// Get all piping system types in the current document
        /// </summary>
        /// <returns>All piping system types</returns>
        public List<PipingSystemType> GetPipingSystemTypes()
        {
            List<PipingSystemType> pipingSystemTypes = new List<PipingSystemType>();

            List<Element> elements = (new FilteredElementCollector(doc))
            .OfCategory(BuiltInCategory.OST_PipingSystem)
            .WhereElementIsElementType()
            .ToElements() as List<Element>;
            foreach (Element e in elements)
            {
                PipingSystemType pst = (PipingSystemType)e;
                pipingSystemTypes.Add(pst);
            }

            // Order by the location of "Drip" in the name
            pipingSystemTypes = pipingSystemTypes.OrderBy(pst => {
                int n = pst.Name.IndexOf("Drip", StringComparison.OrdinalIgnoreCase);
                return n == -1 ? int.MaxValue : n;
            }).ToList();

            return pipingSystemTypes;
        }

        /// <summary>
        /// Get all pipe accessory families in the current document
        /// </summary>
        /// <returns>List of pipe accessory families</returns>
        public List<FamilySymbol> GetValveFamilies()
        {
            List<FamilySymbol> families = new List<FamilySymbol>();

            List<Element> elements = (new FilteredElementCollector(doc))
            .OfCategory(BuiltInCategory.OST_PipeAccessory)
            .WhereElementIsElementType()
            .ToElements() as List<Element>;
            foreach (Element e in elements)
            {
                FamilySymbol f = (FamilySymbol)e;
                families.Add(f);
            }

            // Order by the location of "VS" in the name
            families = families.OrderBy(pst => {
                int n = pst.Name.IndexOf("VS", StringComparison.OrdinalIgnoreCase);
                return n == -1 ? int.MaxValue : n;
            }).ToList();

            return families;
        }


        /// <summary>
        /// Get all grid lines in all documents
        /// </summary>
        /// <returns>List of lines</returns>
        public List<Line> GetGridLines()
        {
            List<Line> gridlines = new List<Line>();

            foreach (Document d in allDocuments)
            {
                List<Element> grids = (new FilteredElementCollector(d))
                .OfCategory(BuiltInCategory.OST_Grids)
                .WhereElementIsNotElementType()
                .ToElements() as List<Element>;
                foreach (Element e in grids)
                {
                    Grid g = (Grid)e;
                    if (g.Curve.GetType() == typeof(Line))
                    {
                        gridlines.Add((Line)g.Curve);
                    }
                }
            }

            return gridlines;
        }


        /// <summary>
        /// Gets the level closest to elevation 0
        /// </summary>
        /// <returns>Ground level</returns>
        public Level GetGroundLevel()
        {
            List<Element> levels = (new FilteredElementCollector(doc))
            .OfCategory(BuiltInCategory.OST_Levels)
            .WhereElementIsNotElementType()
            .ToElements() as List<Element>;

            if (levels.Count == 0) return null;

            // Order levels by absolute distance to 0
            levels = levels.OrderBy(l => Math.Abs(((Level)l).Elevation)).ToList();

            return (Level)levels[0];
        }

        /// <summary>
        /// Gets all sizes corresponding to a pipe type
        /// </summary>
        /// <param name="pipeType">Pipe Type</param>
        /// <returns></returns>
        public List<double> GetPipeSizes(PipeType pipeType)
        {
            if (pipeType == null) throw new ArgumentNullException("pipeType");

            List<double> sizes = new List<double>();

            RoutingPreferenceManager man = pipeType.RoutingPreferenceManager;
            int n = man.GetNumberOfRules(RoutingPreferenceRuleGroupType.Segments);
            for (int i = 0; i < n; i++)
            {
                RoutingPreferenceRule rule = man.GetRule(RoutingPreferenceRuleGroupType.Segments, i);
                if (rule.MEPPartId == null) continue;
                Segment segment = (Segment)pipeType.Document.GetElement(rule.MEPPartId);
                if (segment == null) continue;
                foreach (MEPSize size in segment.GetSizes())
                {
                    sizes.Add(size.NominalDiameter * 304.8);
                }
            }

            return sizes;
        }

        /// <summary>
        /// Gets all points of columns
        /// </summary>
        /// <returns>List of points</returns>
        public List<XYZ> GetColumnPoints()
        {
            List<XYZ> points = new List<XYZ>();
            foreach (Document d in allDocuments)
            {
                List<Element> columns = (new FilteredElementCollector(d))
                    .OfCategory(BuiltInCategory.OST_StructuralColumns)
                    .WhereElementIsNotElementType()
                    .ToElements() as List<Element>;

                // Group element by the Z coordinate of the center of their boundingbox
                var groupedByZ = from c in columns group c by GeomUtils.boundingBoxGetCenter(c.get_BoundingBox(null)).Z into g select new { height = g.Key, columns = g.ToList()};
                if (groupedByZ.Count() == 0) continue;

                // Get group of elements with the most elements and with the lowest height
                List<Element> mostOccurringHeight = groupedByZ.OrderBy(a => a.height).OrderByDescending(a => a.columns.Count).First().columns;                    
                foreach (Element e in mostOccurringHeight)
                {
                    if (e.Location.GetType() != typeof(LocationPoint)) continue;

                    LocationPoint lp = (LocationPoint)e.Location;
                    points.Add(VectorUtils.Vector_setZ(lp.Point, 0));
                }
            }

            return points;
        }
    }
}
