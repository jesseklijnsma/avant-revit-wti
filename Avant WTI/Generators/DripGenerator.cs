using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Avant.WTI.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Avant.WTI.Data;
using static Avant.WTI.Util.PipeUtils;

namespace Avant.WTI.Generators
{
    internal class DripGenerator : Generator
    {
        public DripGenerator(WTIData data) : base(data)
        {
        }

        public override void GeneratePreview()
        {
            // Preview doesn't need any input validation
            foreach (Area area in data.areas)
            {
                // Try to get corresponding type to area
                if (!data.drip.areapipemap.ContainsKey(area)) continue;
                Pipe pipe = data.drip.areapipemap[area];
                if (pipe == null) continue;

                GenerateBranch(pipe, area, previewOnly: true);
            }
        }

        public override bool GenerateModel()
        {
            // Check if inputs are valid
            data.refreshErrorMessages(WTIData.Data.OUTPUT);
            ErrorDialog errorDialog = new ErrorDialog(data.errorMessages);
            errorDialog.ShowErrors();

            if (errorDialog.maxSeverity == WTIData.DripErrorMessage.Severity.FATAL) return false;

            data.errorMessages.Clear();

            // Create transaction
            Transaction t = new Transaction(data.doc);
            t.Start("Drip generation");

            // Set Revit model errors to non blocking dialogs
            FailureHandlingOptions fho = t.GetFailureHandlingOptions();
            fho.SetForcedModalHandling(false);
            fho.SetFailuresPreprocessor(new DripWarningSwallower());
            t.SetFailureHandlingOptions(fho);

            try
            {
                List<Pipe> placeholders = new List<Pipe>();

                // List of unique source pipes
                HashSet<Pipe> sources = new HashSet<Pipe>();
                foreach (Area area in data.areas)
                {
                    // Try to get corresponding type to area
                    if (!data.drip.areapipemap.ContainsKey(area)) continue;
                    Pipe pipe = data.drip.areapipemap[area];
                    if (pipe == null) continue;

                    // Add source pipe to the sources
                    sources.Add(pipe);

                    List<Pipe> pipes = GenerateBranch(pipe, area, false);
                    if (pipes == null) continue;
                    placeholders.AddRange(pipes);
                }

                // Add the unique source pipes to all placeholders
                placeholders.AddRange(sources);

                if (data.convertPlaceholders)
                {
                    PipeUtils.ConvertPlaceholders(data.doc, placeholders);
                }

                CapDistributionLine();

                // We managed to make it to the end without any exceptions
                t.Commit();
            }
            catch (Exception e)
            {
                string message = "An exception has occured";
                string caption = e.Message + "\n" + e.StackTrace;

                MessageBoxButtons buttons = MessageBoxButtons.OK;
                MessageBox.Show(message, caption, buttons, MessageBoxIcon.Warning);
                t.RollBack();
#if DEBUG
                throw;
#endif
            }

            errorDialog = new ErrorDialog(data.errorMessages);
            errorDialog.ShowErrors();

            if (errorDialog.maxSeverity == WTIData.DripErrorMessage.Severity.FATAL) return false;

            return true;
        }

        /// <summary>
        /// Finds a point in the area closest to the source pipe and the center of the area
        /// </summary>
        /// <param name="columnpoints">All potential points</param>
        /// <param name="area">Area</param>
        /// <param name="rootVector">Vector in the direction from the source pipe to the center of the area</param>
        /// <param name="perpendicularVector">Vector perpendicular to the rootVector</param>
        /// <param name="sourceline">Source pipe line</param>
        /// <returns></returns>
        private XYZ FindValvePoint(List<XYZ> columnpoints, Area area, XYZ rootVector, XYZ perpendicularVector, Line sourceline)
        {
            if (data.drip.overrideValvePoints.ContainsKey(area))
            {
                XYZ valvePoint = data.drip.overrideValvePoints[area];
                if (valvePoint != null) return valvePoint;
            }

            if (columnpoints.Count == 0) return XYZ.Zero;

            XYZ areacenter = GeomUtils.RectangleGetCenter(AreaUtils.GetAreaBoundingRectangle(area));

            // Create line from source to center
            Line centerLine = Line.CreateUnbound(VectorUtils.Vector_setZ(areacenter, 0), rootVector);

            // Pull all points to height 0
            List<XYZ> flatColumnPoints = columnpoints.Select(p => VectorUtils.Vector_setZ(p, 0)).ToList();

            // Find intersection point of the center line and the source line
            IList<ClosestPointsPairBetweenTwoCurves> intersectionPoints = new List<ClosestPointsPairBetweenTwoCurves>();
            centerLine.ComputeClosestPoints(sourceline, false, true, false, out intersectionPoints);
            XYZ intersectionPoint = intersectionPoints.FirstOrDefault().XYZPointOnFirstCurve;
            if (intersectionPoint == null) return XYZ.Zero;

            // Group all points by distance to the intersection with a margin of 10mm
            var groupedByDist = from p in columnpoints group p by Math.Round(p.DistanceTo(intersectionPoint) * 304.8 / 10) * 10 / 304.8 into g select new { distance = g.Key, points = g.ToList() };
            if (groupedByDist.Count() == 0) return XYZ.Zero;

            // Get group of points closest to intersection point
            List<XYZ> surroundingPoints = groupedByDist.OrderBy(a => a.distance).First().points;

            List<XYZ> orderedPoints = surroundingPoints
                .OrderBy(p => VectorUtils.Vector_mask(p, perpendicularVector).GetLength())
                .OrderByDescending(p => VectorUtils.Vector_mask(p, rootVector).GetLength())
                .ToList();

            // Get best point
            return orderedPoints.FirstOrDefault();
        }

        /// <summary>
        ///  Puts a cap on all open ends of pipes that belong to the distribution system type
        /// </summary>
        private void CapDistributionLine()
        {
            if (data.drip.distributionSystemType == null) return;
            List<Pipe> distribution_pipes = new FilteredElementCollector(data.doc)
                   .OfCategory(BuiltInCategory.OST_PipeCurves)
                   .WhereElementIsNotElementType()
                   .Where(el => ((Pipe)el).MEPSystem?.GetTypeId() == data.drip.distributionSystemType.Id)
                   .Select(el => (Pipe)el)
                   .ToList();

            foreach (Pipe p in distribution_pipes)
            {
                if (PlumbingUtils.HasOpenConnector(data.doc, p.Id))
                {
                    PlumbingUtils.PlaceCapOnOpenEnds(data.doc, p.Id, p.GetTypeId());
                }
            }
        }


        /// <summary>
        ///  Recursively finds a line that is not too close to a column
        /// </summary>
        /// <param name="source">Source point</param>
        /// <param name="preferredLine">Preferred Line</param>
        /// <param name="columns">List of column points</param>
        /// <param name="paddingft">Mimimum distance between a column and the line</param>
        /// <returns>A line</returns>
        private Line FindBestCenterLine(XYZ source, Line preferredLine, List<XYZ> columns, double paddingft)
        {
            List<XYZ> closePoints = GeomUtils.GetClosestPoints(preferredLine, columns, 1 / 304.8);
            bool goodLine = !closePoints.Any(p => preferredLine.Distance(p) < paddingft || source.DistanceTo(p) < paddingft);

            if (goodLine) return preferredLine;

            XYZ linePoint = GeomUtils.GetClosestPoint(preferredLine, source);
            XYZ sourceToLine;
            if (linePoint.IsAlmostEqualTo(source, 0.001))
            {
                sourceToLine = preferredLine.Direction.CrossProduct(XYZ.BasisZ).Normalize();
            }
            else
            {
                sourceToLine = VectorUtils.Vector_setZ((linePoint - source), 0).Normalize();
            }

            Line newLine = (Line)preferredLine.CreateTransformed(Transform.CreateTranslation(sourceToLine.Multiply(paddingft + 0.5/304.8)));

            return FindBestCenterLine(source, newLine, columns, paddingft);
        }

        /// <summary>
        ///  Tries to create a valve pointing in a direction
        /// </summary>
        /// <param name="point">Location</param>
        /// <param name="direction">Direction from IN to OUT connector</param>
        /// <returns>Valve FamilyInstance</returns>
        private FamilyInstance PlaceValve(XYZ point, XYZ direction)
        {
            Utils.EnsureFamilyActive(data.doc, data.drip.valvefamily);
            // Place valve and find corresponding in and out points
            FamilyInstance valve = data.doc.Create.NewFamilyInstance(point, data.drip.valvefamily, data.groundLevel, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
            XYZ valvedir = ValveUtils.GetValveDirection(valve);
            Line up = Line.CreateUnbound(point, XYZ.BasisZ);
            valve.Location.Rotate(axis: up, valvedir.AngleOnPlaneTo(direction, XYZ.BasisZ));
            return valve;
        }

        
        /// <summary>
        /// Generates the Drip installation in a certain branch
        /// </summary>
        /// <param name="source">Source pipe placeholder</param>
        /// <param name="area">Area</param>
        /// <param name="previewOnly">Whether to alter the Revit document or not</param>
        /// <returns>List of generated pipes</returns>
        private List<Pipe> GenerateBranch(Pipe source, Area area, bool previewOnly = false)
        {
            // Resulting elements
            List<Pipe> pipes = new List<Pipe>();

            List<XYZ> columnpoints = AreaUtils.GetPointsInArea(area, data.columnpoints);

            Line sourcepipeline = ((LocationCurve)source.Location).Curve as Line;
            RectangleF areaRect = AreaUtils.GetAreaBoundingRectangle(area);
            XYZ center = GeomUtils.RectangleGetCenter(areaRect);

            XYZ branchInwardVector = -GeomUtils.GetVectorFromPointToLine(sourcepipeline, center);
            (XYZ rootVector, XYZ perpendicularVector) = AreaUtils.GetAreaVectors(area, branchInwardVector);

            // Find column to attach valve to
            XYZ valveColumnPoint = FindValvePoint(columnpoints, area, rootVector, perpendicularVector, sourcepipeline);
            if (valveColumnPoint == null)
            {
                data.errorMessages.Add(new WTIData.DripErrorMessage("No valve column found", WTIData.DripErrorMessage.Severity.FATAL));
                return pipes;
            }

            previewOnly |= data.drip.pipetype == null;
            previewOnly |= data.drip.distributionSystemType == null;
            previewOnly |= data.drip.transportSystemType == null;

            data.minimumPipeLengthFt = 3 * Math.Max(data.drip.transport_diameter, data.drip.distribution_diameter) / 304.8;

            // Calculate the point to actually place the valve using an offset
            XYZ valvePoint = valveColumnPoint.Add(rootVector.Normalize().Multiply(-data.drip.valvecolumnDistance / 304.8));
            valvePoint = VectorUtils.Vector_setZ(valvePoint, data.drip.valveheight / 304.8);

            //data.previewPoints.Add(new RenderPoint(valvePoint, System.Drawing.Color.White, 200, RenderPoint.RenderUnits.MM));
            data.valvePoints[area] = new RenderPoint(valvePoint, System.Drawing.Color.White, 200, RenderPoint.RenderUnits.MM);

            Line centerline = Line.CreateBound(center, VectorUtils.Vector_setZ(GeomUtils.GetClosestPoint(sourcepipeline, center), 0));


            Connector valve_in_c = null;
            Connector valve_out_c = null;
            if (!previewOnly && data.drip.valvefamily != null)
            {
                FamilyInstance valve = PlaceValve(valvePoint, -rootVector);
                (valve_in_c, valve_out_c) = ValveUtils.GetValveConnectorPair(valve);
            }

            // Get locations of connectors
            // Or create dummy in case of preview or valve creation failed
            XYZ valve_in_p = valve_in_c?.Origin ?? valvePoint.Add(rootVector.Normalize().Multiply(200 / 304.8));
            XYZ valve_out_p = valve_out_c?.Origin ?? valvePoint.Add(rootVector.Normalize().Multiply(-200 / 304.8));

            bool valveConnect = valve_in_c != null && valve_out_c != null;
            if (valveConnect)
            {
                // Check if the pipes will connect to the 'open' side of the connector
                if (MEPUtils.GetConnectorDirection(valve_in_c).DotProduct(new XYZ(0, 0, data.drip.transportlineheight / 304.8 - valve_in_p.Z)) < 0)
                {
                    valve_in_c = null;
                    data.errorMessages.Add(new WTIData.DripErrorMessage("Pipe cannot be connected to In connector of the valve. Dummy connections will be created.", WTIData.DripErrorMessage.Severity.WARNING));
                }
                if (MEPUtils.GetConnectorDirection(valve_out_c).DotProduct(new XYZ(0, 0, data.drip.transportlineheight / 304.8 - valve_out_p.Z)) < 0)
                {
                    valve_out_c = null;
                    data.errorMessages.Add(new WTIData.DripErrorMessage("Pipe cannot be connected to Out connector of the valve. Dummy connections will be created.", WTIData.DripErrorMessage.Severity.WARNING));
                }
            }

            // Create dummy in case of preview or valve creation failed

            double minOffset = Math.Max(data.drip.pipecolumnDistance / 304.8, data.minimumPipeLengthFt);
            Line transportCenterLine = FindBestCenterLine(valve_out_p, centerline, columnpoints, minOffset);

            // Check if transport line is correctly generated
            double distanceFromCenter = transportCenterLine.Distance(center);
            if (distanceFromCenter >= 0.000000001f && distanceFromCenter < minOffset)
            {
                data.errorMessages.Add(new WTIData.DripErrorMessage(string.Format("Transport line is too close to a column or other pipe or is too short. Distance: {0}mm. No pipes will be generated.", distanceFromCenter * 304.8), WTIData.DripErrorMessage.Severity.WARNING));
                previewOnly = true;
            }

            // Generate all pipe lines
            List<PipeUtils.PipeGeometryCollection> pipe_geometry = GeneratePipeGeometry(transportCenterLine, sourcepipeline, center, rootVector, perpendicularVector, valve_in_p, valve_out_p);
            foreach (PipeUtils.PipeGeometryCollection pipecollection in pipe_geometry)
            {
                // Add geometry to GUI preview
                data.previewGeometry.AddRange(pipecollection.Geometry);

                // Dump into model
                if (!previewOnly)
                {
                    if (pipecollection.IsValidLength(data.minimumPipeLengthFt))
                    {
                        // Create pipes
                        List<Pipe> newPipes = PipeUtils.CreatePipesFromGeometry(data.doc, pipecollection);
                        // Set sizes
                        foreach (Pipe p in newPipes) PipeUtils.SetSize(p, pipecollection.Size / 304.8);
                        pipes.AddRange(newPipes);
                    }
                    else
                    {
                        data.errorMessages.Add(new WTIData.DripErrorMessage("Generated pipe segment is too short! Branch will not be generated.", WTIData.DripErrorMessage.Severity.WARNING, false));
                    }
                }
            }

            if (!previewOnly)
            {
                if (valveConnect)
                {
                    if (valve_in_c != null) MEPUtils.ConnectPipe(pipes, valve_in_c);
                    if (valve_out_c != null) MEPUtils.ConnectPipe(pipes, valve_out_c);
                }
                PipeUtils.GenerateFittigs(data.doc, pipes);
            }

            return pipes;
        }

        /// <summary>
        ///  Generates all necessary pipelines without placing pipes in the Revit document
        /// </summary>
        /// <param name="transportCenterLine">Location of the long transport line through the area</param>
        /// <param name="sourceLine">Source pipe line</param>
        /// <param name="center">Center of the area</param>
        /// <param name="rootVector">Vector pointing into the area</param>
        /// <param name="perpendicularVector">Vector perpendicular to the root vector</param>
        /// <param name="valve_in_p">Location of the valve IN connector</param>
        /// <param name="valve_out_p">Location of the valve OUT connector</param>
        /// <returns>List of geometry collections</returns>
        public List<PipeUtils.PipeGeometryCollection> GeneratePipeGeometry(Line transportCenterLine, Line sourceLine, XYZ center, XYZ rootVector, XYZ perpendicularVector, XYZ valve_in_p, XYZ valve_out_p)
        {
            PipeGeometryCollection transport_geometry = new PipeGeometryCollection(data.drip.transportSystemType, data.drip.pipetype, data.groundLevel, data.drip.transport_diameter);
            PipeGeometryCollection distribution_geometry = new PipeGeometryCollection(data.drip.distributionSystemType, data.drip.pipetype, data.groundLevel, data.drip.distribution_diameter);

            // START CALCULATING ROUTING

            // Calculate from valve to long transport line
            XYZ p3 = VectorUtils.Vector_setZ(valve_out_p, data.drip.transportlineheight / 304.8);
            XYZ valve_closest_point = GeomUtils.GetClosestPoint(transportCenterLine, p3);
            XYZ dir_valve_to_centerline = VectorUtils.Vector_setZ((valve_closest_point - p3), 0).Normalize();

            // ROUTE SOURCE TO VALVE
            // sourcepoint -> p1 -> valve_transport_corner_p -> p2 -> valve_in_p
            XYZ p2 = VectorUtils.Vector_setZ(valve_in_p, data.drip.transportlineheight / 304.8);
            XYZ valve_transport_corner_p = p2.Add(dir_valve_to_centerline.Multiply(-data.drip.pipecolumnDistance / 304.8));

            // Calculate ACTUAL source point
            XYZ sourcepoint = GeomUtils.GetClosestPoint(sourceLine, valve_transport_corner_p);
            XYZ p1 = VectorUtils.Vector_setZ(sourcepoint, data.drip.transportlineheight / 304.8);


            // Checking sizes and distances
            double source_transport_heightdiff = Math.Abs(data.drip.transportlineheight / 304.8 - sourcepoint.Z);
            if (source_transport_heightdiff < data.minimumPipeLengthFt && source_transport_heightdiff > 0.000000001f)
            {
                data.errorMessages.Add(new WTIData.DripErrorMessage(string.Format("Height between source pipe and transport pipe is too small. Distance: {0}mm. No pipes will be generated.", source_transport_heightdiff * 304.8), WTIData.DripErrorMessage.Severity.WARNING, unique: false));
                transport_geometry.PreviewOnly = true;
            }
            double transport_distribution_heightdiff = Math.Abs(data.drip.transportlineheight - data.drip.distributionlineheight) / 304.8;
            if (transport_distribution_heightdiff < data.minimumPipeLengthFt)
            {
                data.errorMessages.Add(new WTIData.DripErrorMessage(string.Format("Height between distribution pipe and transport pipe is too small. Distance needs to be at least {0}mm. No pipes will be generated.", data.minimumPipeLengthFt * 304.8), WTIData.DripErrorMessage.Severity.WARNING, unique: false));
                transport_geometry.PreviewOnly = true;
                distribution_geometry.PreviewOnly = true;
            }


            // ROUTE VALVE TO DISTRIBUTION TEE
            // offcenter:
            //  valve_out_p -> p3 -> valve_transport_corner_out_p -> p4 -> p5 -> tee
            // centered:
            //  valve_out_p -> p3 -> valve_transport_corner_out_p -> p5 -> tee
            double distanceFromCenter = transportCenterLine.Distance(center);
            bool offcenter = distanceFromCenter >= 0.000000001f;

            XYZ valve_transport_corner_out_p = VectorUtils.Vector_setZ(GeomUtils.GetClosestPoint(transportCenterLine, p3), data.drip.transportlineheight / 304.8);

            XYZ teeOffsetFromBackWall = rootVector.Normalize().Multiply(-data.drip.backwallDistance / 304.8);
            XYZ tee = VectorUtils.Vector_setZ(center.Add(rootVector.Multiply(0.5).Add(teeOffsetFromBackWall)), data.drip.distributionlineheight / 304.8);
            XYZ p5 = VectorUtils.Vector_setZ(tee, data.drip.transportlineheight / 304.8);

            // Create extra elbow point in case its offcenter
            XYZ p4 = GeomUtils.GetClosestPoint(Line.CreateUnbound(valve_transport_corner_out_p, rootVector), p5);

            // CALCULATE TEE LINE
            XYZ sidePadding = perpendicularVector.Normalize().Multiply(-0.5 * data.drip.intermediateDistance / 304.8);
            XYZ halfTee = perpendicularVector.Multiply(0.5).Add(sidePadding);

            XYZ tee_p1 = tee.Add(halfTee);
            XYZ tee_p2 = tee.Add(halfTee.Multiply(-1));

            // Source to valve
            transport_geometry.AddLine(GeomUtils.CreateNamedLine(sourcepoint, p1, "Vertical from main line", data.errorMessages));

            transport_geometry.AddLine(GeomUtils.CreateNamedLine(p1, valve_transport_corner_p, "From main line pointing towards valve", data.errorMessages));
            transport_geometry.AddLine(GeomUtils.CreateNamedLine(valve_transport_corner_p, p2, "From an elbow to underneath the IN connector of the valve", data.errorMessages));
            transport_geometry.AddLine(GeomUtils.CreateNamedLine(p2, valve_in_p, "Vertical to IN connector of the valve", data.errorMessages));

            // Valve to tee
            transport_geometry.AddLine(GeomUtils.CreateNamedLine(valve_out_p, p3, "Vertical from OUT connector of the valve", data.errorMessages));
            transport_geometry.AddLine(GeomUtils.CreateNamedLine(p3, valve_transport_corner_out_p, "From underneath the OUT connector to the start of the long transport line", data.errorMessages));
            if (offcenter)
            {
                transport_geometry.AddLine(GeomUtils.CreateNamedLine(valve_transport_corner_out_p, p4, "Long transport line", data.errorMessages));
                transport_geometry.AddLine(GeomUtils.CreateNamedLine(p4, p5, "From the end of the long transport line to underneath the tee", data.errorMessages));
            }
            else
            {
                transport_geometry.AddLine(GeomUtils.CreateNamedLine(valve_transport_corner_out_p, p5, "Long transport line", data.errorMessages));
            }
            transport_geometry.AddLine(GeomUtils.CreateNamedLine(p5, tee, "Vertical under the tee", data.errorMessages));

            // Tee
            distribution_geometry.AddLine(GeomUtils.CreateNamedLine(tee_p1, tee_p2, "Perpendicular branch line", data.errorMessages));

            List<PipeGeometryCollection> geometry = new List<PipeGeometryCollection> { transport_geometry, distribution_geometry };

            return geometry;
        }


        public class DripWarningSwallower : IFailuresPreprocessor
        {
            public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
            {
                IList<FailureMessageAccessor> failList = failuresAccessor.GetFailureMessages(); ;
                // Inside event handler, get all warnings
                foreach (FailureMessageAccessor failure in failList)
                {
                    // check FailureDefinitionIds against ones that you want to dismiss, 
                    FailureDefinitionId failID = failure.GetFailureDefinitionId();
                    // prevent Revit from showing Unenclosed room warnings
                    if (failID == BuiltInFailures.AutoRouteFailures.ElementHasOpenConnection)
                    {
                        failuresAccessor.DeleteWarning(failure);
                    }
                    else if (failID == BuiltInFailures.AutoRouteFailures.ElementHasFlowCalculation)
                    {
                        failuresAccessor.DeleteWarning(failure);
                    }
                }

                return FailureProcessingResult.Continue;
            }
        }


    }
}
