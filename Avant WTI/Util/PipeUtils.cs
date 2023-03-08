using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Avant.WTI.Drip.DripData;
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

    }
}
