using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Microsoft.Scripting.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avant.WTI.Util
{
    internal class ValveUtils
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
            if(typeof(FamilyInstance).IsAssignableFrom(e.GetType()))
            {
                FamilyInstance f = (FamilyInstance)e;
                MEPModel mep = f.MEPModel;
                connectorManager = mep?.ConnectorManager;
            }
            if(typeof(MEPCurve).IsAssignableFrom(e.GetType()))
            {
                MEPCurve curve = (MEPCurve)e;
                connectorManager = curve.ConnectorManager;
            }

            // Return empty list if element has no connectors/connectormanager
            if (connectorManager == null) return new List<Connector>();

            // Get connectors out of the connectormanager
            List<Connector> connectors = new List<Connector>();
            foreach(Connector c in connectorManager.Connectors)
            {
                connectors.Add(c);
            }
            return connectors;
        }

        /// <summary>
        /// Get In and Out connector of a familyinstance
        /// </summary>
        /// <param name="valve">Valve (any FamilyInstance)</param>
        /// <returns>In and Out connector tuple</returns>
        public static (Connector c_in, Connector c_out) GetValveConnectorPair(FamilyInstance valve)
        {
            List<Connector> connectors = GetConnectors(valve);
            // Check if there are enough connectors
            if (connectors.Count < 2) return (null,null);

            // Try to find In and Out connectors
            Connector c_in = null;
            Connector c_out = null;
            foreach(Connector c in connectors)
            {
                if (c.Direction == FlowDirectionType.In) c_in = c;
                else if (c.Direction == FlowDirectionType.Out) c_out = c;
            }

            return (c_in, c_out);
        }

        /// <summary>
        ///  Gets the vector from the In connector to the Out connector
        /// </summary>
        /// <param name="valve">Valve (any FamilyInstance)</param>
        /// <returns>Vector from in to out connector</returns>
        public static XYZ GetValveDirection(FamilyInstance valve)
        {
            (Connector c_in, Connector c_out) = GetValveConnectorPair(valve);
            // Check if the connectors are both valid
            if (c_in == null || c_out == null) return XYZ.Zero;
            XYZ dir = c_out.Origin.Subtract(c_in.Origin);
            return dir;
        }

        /// <summary>
        ///  Connects a (placeholder) pipe to a connector
        /// </summary>
        /// <param name="p">Pipe</param>
        /// <param name="connector">Connector</param>
        /// <returns>True if succesful</returns>
        internal static bool ConnectPipe(Pipe p, Connector connector)
        {
            // Get all connectors of the pipe
            List<Connector> connectors = GetConnectors(p);
            foreach(Connector c in connectors)
            {
                if (c.IsConnected) continue;
                // Check if connectors are in the same location
                if (c.Origin.IsAlmostEqualTo(connector.Origin))
                {
                    try
                    {
                        c.ConnectTo(connector);
                        return true;
                    }catch(Exception)
                    {
                        return false;
                    }
                }
            }
            return false;
        }
    }
}
