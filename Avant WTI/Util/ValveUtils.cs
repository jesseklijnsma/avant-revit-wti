using Autodesk.Revit.DB;
using System.Collections.Generic;
using static Avant.WTI.WTIData;

namespace Avant.WTI.Util
{
    internal class ValveUtils
    {


        

        /// <summary>
        /// Get In and Out connector of a familyinstance
        /// </summary>
        /// <param name="valve">Valve (any FamilyInstance)</param>
        /// <returns>In and Out connector tuple</returns>
        public static (Connector c_in, Connector c_out) GetValveConnectorPair(FamilyInstance valve)
        {
            List<Connector> connectors = MEPUtils.GetConnectors(valve);
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


    }
}
