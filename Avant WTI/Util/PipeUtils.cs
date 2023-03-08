using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Avant.WTI.Util
{
    internal class PipeUtils
    {

        public static void SetSize(Pipe p, double size)
        {
            if (p.GetType() != typeof(Autodesk.Revit.DB.Plumbing.Pipe)) return;
            Parameter param = p.LookupParameter("Diameter");
            if (param == null) return;
            param.Set(size);
        }

        public static void GenerateFittigs(Document doc, List<Pipe> pipes)
        {
            foreach (Pipe pipe in pipes)
            {
                List<Connector> connectors = MEPUtils.GetConnectors(pipe);
                foreach (Connector c in connectors)
                {
                    if (c.IsConnected) continue;
                    bool connectionMade = false;

                    XYZ p = c.Origin;
                    foreach (Pipe pipe2 in pipes)
                    {
                        if (pipe == pipe2) continue;

                        Line l = (Line)((LocationCurve)pipe2.Location).Curve;

                        double dist = l.Distance(p);
                        if (dist < 0.00000001f)
                        {
                            List<Connector> otherConnectors = MEPUtils.GetConnectors(pipe2);
                            foreach (Connector other in otherConnectors)
                            {
                                if (other.IsConnected) continue;
                                if (other.Origin.IsAlmostEqualTo(p))
                                {
                                    double angle = MEPUtils.GetConnectorDirection(other).AngleTo(MEPUtils.GetConnectorDirection(c));
                                    if (angle < Math.PI / 3) continue;
                                    if (angle == Math.PI)
                                    {
                                        other.ConnectTo(c);
                                    }
                                    else
                                    {
                                        PlumbingUtils.ConnectPipePlaceholdersAtElbow(doc, c, other);
                                    }
                                }
                                connectionMade = c.IsConnected;
                                if (connectionMade) break;
                            }


                            // Try to create tee
                            if (!connectionMade)
                            {
                                connectionMade = PlumbingUtils.ConnectPipePlaceholdersAtTee(doc, pipe2.Id, pipe.Id);
                            }

                        }
                        if (connectionMade) break;
                    }
                    if (connectionMade) continue;
                }
            }
        }

        /// <summary>
        ///  Converts placeholder into pipes
        /// </summary>
        /// <param name="doc">Revit document</param>
        /// <param name="placeholders">Placeholders to convert</param>
        /// <returns></returns>
        public static List<ElementId> ConvertPlaceholders(Document doc, List<Pipe> placeholders)
        {
            List<ElementId> ids = new List<ElementId>();
            foreach (Pipe p in placeholders)
            {
                if (p.IsPlaceholder) ids.Add(p.Id);
            }
            if (ids.Count == 0) return new List<ElementId>();
            try
            {
                List<ElementId> newIds = (List<ElementId>)PlumbingUtils.ConvertPipePlaceholders(doc, ids);
                return newIds;
            }
            catch (Exception)
            {
                MessageBox.Show("Placeholder to pipe conversion failed.", "", MessageBoxButtons.OK);
            }
            return new List<ElementId>();
        }


        public static List<Pipe> CreatePipesFromGeometry(Document doc, PipeGeometryCollection pipeCollection)
        {
            List<Pipe> pipes = new List<Pipe>();
            foreach(Line l in pipeCollection.Geometry)
            {
                if (l == null) continue;

                XYZ begin = l.GetEndPoint(0);
                XYZ end = l.GetEndPoint(1);
                try
                {
                    Pipe pipe = Pipe.CreatePlaceholder(doc, pipeCollection.PipingSystemType.Id, pipeCollection.PipeType.Id, pipeCollection.Level.Id, begin, end);
                    if (pipe != null) pipes.Add(pipe);
                }
                catch (Exception) { }

            }

            return pipes;
        }


        public class PipeGeometryCollection
        {

            public readonly List<Line> Geometry = new List<Line>();
            public PipingSystemType PipingSystemType { get; }
            public PipeType PipeType { get; }
            public Level Level { get; }
            public double Size { get; }
            public bool PreviewOnly { get; set; } = false;


            public PipeGeometryCollection(PipingSystemType pipingSystemType, PipeType pipeType, Level level, double size)
            {
                PipingSystemType = pipingSystemType;
                PipeType = pipeType;
                Level = level;
                Size = size;
            }

            public void AddLine(Line l)
            {
                if (l == null) return;
                Geometry.Add(l);
            }

            public bool IsValidLength(double minimumPipeLengthFt)
            {
                bool valid = true;
                foreach (Line l in Geometry)
                {
                    if (l.Length < minimumPipeLengthFt)
                    {
                        valid = false;
                    }
                }
                return valid;
            }

        }

    }
}
