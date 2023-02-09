using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Avant.WTI.Util
{
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

        public List<Area> getAreas()
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
                    if (e.GetType() == typeof(Area))
                    {
                        areas.Add((Area)e);
                    }
                }
            }

            return areas;
        }

        public List<PipeType> getPipeTypes()
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

        public List<PipingSystemType> getPipingSystemTypes()
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

        public List<FamilySymbol> getValveFamilies()
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

        public List<Line> getGridLines()
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
                    if (e.GetType() == typeof(Grid))
                    {
                        Grid g = (Grid)e;
                        if (g.Curve.GetType() == typeof(Line))
                        {
                            gridlines.Add((Line)g.Curve);
                        }
                    }
                }
            }

            return gridlines;
        }

        public Level getGroundLevel()
        {
            if (doc == null) return null;

            List<Element> levels = (new FilteredElementCollector(doc))
            .OfCategory(BuiltInCategory.OST_Levels)
            .WhereElementIsNotElementType()
            .ToElements() as List<Element>;
            foreach (Element e in levels)
            {
                if (e.GetType() == typeof(Level))
                {
                    Level l = (Level)e;
                    if (l.Elevation == 0)
                    {
                        return l;
                    }
                }
            }
            return null;
        }


        public List<double> getPipeSizes(PipeType pipeType)
        {
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

        public List<XYZ> getColumnPoints()
        {
            List<XYZ> points = new List<XYZ>();
            foreach (Autodesk.Revit.DB.Document d in allDocuments)
            {
                List<Element> columns = (new FilteredElementCollector(d))
                    .OfCategory(BuiltInCategory.OST_StructuralColumns)
                    .WhereElementIsNotElementType()
                    .ToElements() as List<Element>;
                foreach (Element e in columns)
                {
                    //TODO dont use foundation depth
                    Parameter param = e.LookupParameter("Base Level");
                    if (param == null) continue;
                    if (param.AsValueString() == "Foundation Depth") continue;
                    if (e.Location.GetType() != typeof(LocationPoint)) continue;
                    LocationPoint lp = (LocationPoint)e.Location;
                    points.Add(lp.Point);

                }
            }

            return points;
        }
    }
}
