using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Microsoft.Scripting.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitTemplate
{
    internal class ValveUtils
    {

        public static List<Connector> getConnectors(Element e)
        {
            if (e == null) return new List<Connector>();
            ConnectorManager connectorManager = null;
            if(e.GetType() == typeof(FamilyInstance))
            {
                FamilyInstance f = (FamilyInstance)e;
                MEPModel mep = f.MEPModel;
                if(mep != null)
                {
                    connectorManager = mep.ConnectorManager;
                }
            }
            if(e.GetType() == typeof(MEPCurve))
            {
                MEPCurve curve = (MEPCurve)e;
                connectorManager = curve.ConnectorManager;
            }
            if (connectorManager == null) return new List<Connector>();

            List<Connector> connectors = new List<Connector>();
            foreach(Connector c in connectorManager.Connectors)
            {
                connectors.Add(c);
            }
            return connectors;
        }

        public static (Connector c_in, Connector c_out) getValveConnectorPair(FamilyInstance valve)
        {
            List<Connector> connectors = getConnectors(valve);
            if (connectors.Count < 2) return (null,null);
            Connector c_in = null;
            Connector c_out = null;
            foreach(Connector c in connectors)
            {
                if (c.Direction == FlowDirectionType.In) c_in = c;
                else if (c.Direction == FlowDirectionType.Out) c_out = c;
            }
            return (c_in, c_out);
        }

        public static XYZ getValveDirection(FamilyInstance valve)
        {
            (Connector c_in, Connector c_out) = getValveConnectorPair(valve);
            if (c_in == null || c_out == null) return XYZ.Zero;
            XYZ dir = c_out.Origin.Subtract(c_in.Origin);
            return dir;
        }

        internal static bool connectPipe(Pipe p, Connector connector)
        {
            List<Connector> connectors = getConnectors(p);
            foreach(Connector c in connectors)
            {
                if (c.IsConnected) continue;
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
