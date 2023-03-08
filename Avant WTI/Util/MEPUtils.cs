using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avant.WTI.Util
{
    internal class MEPUtils
    {

        /// <summary>
        ///  Gets all connectors of and element
        /// </summary>
        /// <param name="e">Element</param>
        /// <returns>All connectors or empty list</returns>
        public static List<Connector> GetConnectors(Element e)
        {
            if (e == null) return new List<Connector>();

            // Try to get connectormanager
            ConnectorManager connectorManager = null;
            if (typeof(FamilyInstance).IsAssignableFrom(e.GetType()))
            {
                FamilyInstance f = (FamilyInstance)e;
                MEPModel mep = f.MEPModel;
                connectorManager = mep?.ConnectorManager;
            }
            if (typeof(MEPCurve).IsAssignableFrom(e.GetType()))
            {
                MEPCurve curve = (MEPCurve)e;
                connectorManager = curve.ConnectorManager;
            }

            // Return empty list if element has no connectors/connectormanager
            if (connectorManager == null) return new List<Connector>();

            // Get connectors out of the connectormanager
            List<Connector> connectors = new List<Connector>();
            foreach (Connector c in connectorManager.Connectors)
            {
                connectors.Add(c);
            }
            return connectors;
        }

        /// <summary>
        ///  Connects a (placeholder) pipe to a connector
        /// </summary>
        /// <param name="p">Pipe</param>
        /// <param name="connector">Connector</param>
        /// <returns>True if succesful</returns>
        public static bool ConnectPipe(Pipe p, Connector connector)
        {
            // Get all connectors of the pipe
            List<Connector> connectors = GetConnectors(p);
            foreach (Connector c in connectors)
            {
                if (c.IsConnected) continue;
                // Check if connectors are in the same location
                if (c.Origin.IsAlmostEqualTo(connector.Origin))
                {
                    try
                    {
                        c.ConnectTo(connector);
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }
            return false;
        }


        /// <summary>
        /// Gets the vector pointing outward of a connector
        /// </summary>
        /// <param name="c">The connector</param>
        /// <returns></returns>
        public static XYZ GetConnectorDirection(Connector c)
        {
            return c.CoordinateSystem.BasisZ;
        }

    }
}
