using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.Exceptions;
using Autodesk.Revit.UI;
using Avant.WTI.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Avant.WTI.Drip
{
    internal class DripGenerator
    {

        private readonly DripData data;

        public DripGenerator(DripData data)
        {
            this.data = data;
        }

        /// <summary>
        ///  Converts placeholder into pipes
        /// </summary>
        /// <param name="placeholders">Placeholders to convert</param>
        /// <returns></returns>
        private List<ElementId> ConvertPlaceholders(List<Pipe> placeholders)
        {
            List<ElementId> ids = new List<ElementId>();
            foreach (Pipe p in placeholders)
            {
                if (p.IsPlaceholder) ids.Add(p.Id);
            }
            if (ids.Count == 0) return new List<ElementId>();
            try
            {
                List<ElementId> newIds = (List<ElementId>)PlumbingUtils.ConvertPipePlaceholders(data.doc, ids);
                return newIds;
            }
            catch (Exception)
            {
                MessageBox.Show("Placeholder to pipe conversion failed.", "", MessageBoxButtons.OK);
            }
            return new List<ElementId>();
        }

        /// <summary>
        /// Finds a point in the area closest to the source pipe and the center of the area
        /// </summary>
        /// <param name="columnpoints">All potential points</param>
        /// <param name="areacenter">Center of the area</param>
        /// <param name="rootVector">Vector in the direction from the source pipe to the center of the area</param>
        /// <param name="perpendicularVector">Vector perpendicular to the rootVector</param>
        /// <param name="sourceline">Source pipe line</param>
        /// <returns></returns>
        private XYZ FindValvePoint(List<XYZ> columnpoints, XYZ areacenter, XYZ rootVector, XYZ perpendicularVector, Line sourceline)
        {
            if (columnpoints.Count == 0) return XYZ.Zero;

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


        private Line FindBestCenterLine(XYZ source, Line preferredLine, List<XYZ> columns, double paddingmm)
        {
            double paddingft = paddingmm / 304.8;
            List<XYZ> closePoints = GeomUtils.GetClosestPoints(preferredLine, columns, 1 / 304.8);
            bool goodLine = !closePoints.Any(p => preferredLine.Distance(p) < paddingft || source.DistanceTo(p) < paddingft);

            if (goodLine) return preferredLine;

            XYZ linePoint = GeomUtils.GetClosestPoint(preferredLine, source);
            XYZ sourceToLine = (linePoint - source).Normalize();

            Line newLine = (Line)preferredLine.CreateTransformed(Transform.CreateTranslation(sourceToLine.Multiply(paddingft)));

            return FindBestCenterLine(source, newLine, columns, paddingmm);
        }

        public void GeneratePreviewGeometry()
        {
            // Preview doesn't need any input validation

            data.previewGeometry.Clear();
            data.debugLines.Clear();
            data.previewPoints.Clear();

            data.errorMessages.Clear();

            foreach (Area area in data.areas)
            {
                // Try to get corresponding type to area
                Pipe pipe = null;
                if (data.areapipemap.ContainsKey(area)) pipe = data.areapipemap[area];
                if (pipe == null) pipe = Utils.FindClosestPipe(data.pipelines, area);
                if (pipe == null) continue;

                GenerateAreaBranch(pipe, area, previewOnly: true);
            }
        }

        public bool GenerateDrip()
        {
            // Check if inputs are valid
            data.refreshErrorMessages(DripData.Data.OUTPUT);
            ErrorDialog errorDialog = new ErrorDialog(data.errorMessages);
            errorDialog.ShowErrors();

            if (errorDialog.maxSeverity == DripData.DripErrorMessage.Severity.FATAL) return false;

            data.errorMessages.Clear();

            // Create transaction
            Transaction t = new Transaction(data.doc);
            t.Start("Drip generation");

            // Set Revit model errors to non blocking dialogs
            FailureHandlingOptions fho = t.GetFailureHandlingOptions();
            fho.SetForcedModalHandling(false);
            t.SetFailureHandlingOptions(fho);

            try
            {
                List<Pipe> placeholders = new List<Pipe>();

                // List of unique source pipes
                HashSet<Pipe> sources = new HashSet<Pipe>();
                foreach (Area area in data.areas)
                {
                    // Try to get corresponding type to area
                    Pipe pipe = null;
                    if (data.areapipemap.ContainsKey(area)) pipe = data.areapipemap[area];
                    if (pipe == null) pipe = Utils.FindClosestPipe(data.pipelines, area);
                    if (pipe == null) continue;

                    // Add source pipe to the sources
                    sources.Add(pipe);

                    List<Pipe> pipes = GenerateAreaBranch(pipe, area, false);
                    if (pipes == null) continue;
                    placeholders.AddRange(pipes);
                }

                // Add the unique source pipes to all placeholders
                placeholders.AddRange(sources);


                if (data.convertPlaceholders)
                {
                    ConvertPlaceholders(placeholders);
                }

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

            if (errorDialog.maxSeverity == DripData.DripErrorMessage.Severity.FATAL) return false;

            return true;
        }

        /// <summary>
        ///     Generates a drip irrigation branch on a specific area
        /// </summary>
        /// <param name="source">Source pipe</param>
        /// <param name="area">Area</param>
        /// <param name="previewOnly">Whether the generated geometry should be placed into the model</param>
        /// <returns>List of pipe placeholders</returns>
        private List<Pipe> GenerateAreaBranch(Pipe source, Area area, bool previewOnly = false)
        {
            RectangleF arearect = AreaUtils.GetAreaBoundingRectangle(area);
            XYZ center = Utils.RectangleGetCenter(arearect);
            XYZ areavector = new XYZ(arearect.Width, arearect.Height, 0);

            // Get line of source pipe
            Line sourceline = ((LocationCurve)source.Location).Curve as Line;
            // Get closest point on source pipe line to area center
            XYZ connectionPoint = GeomUtils.GetClosestPoint(sourceline, center);

            // Gets the vector from the source point to the center
            XYZ branchinwardvector = center.Subtract(connectionPoint).Normalize();
            // Round the vector to create an axis unitvector
            branchinwardvector = VectorUtils.Vector_round(branchinwardvector);

            // Get all points inside of the area
            List<XYZ> areaColumnPoints = new List<XYZ>();
            foreach (XYZ p in data.columnpoints)
            {
                if (Utils.RectangleIntersect(arearect, p, tolerance: 1))
                {
                    areaColumnPoints.Add(p);
                }
            }

            // Get full size area vector
            XYZ rootVector = VectorUtils.Vector_mask(branchinwardvector, areavector);
            XYZ perpendicularVector = VectorUtils.Vector_mask(branchinwardvector.CrossProduct(XYZ.BasisZ), areavector);

            return GenerateBranch(source, arearect, rootVector, perpendicularVector, areaColumnPoints, previewOnly: previewOnly);
        }

        private List<Pipe> GenerateBranch(Pipe source, RectangleF areaRect, XYZ rootVector, XYZ perpendicularVector, List<XYZ> columnpoints, bool previewOnly = false)
        {
            Line sourcepipeline = ((LocationCurve)source.Location).Curve as Line;
            XYZ center = Utils.RectangleGetCenter(areaRect);

            // Find column to attach valve to
            XYZ valveColumnPoint = FindValvePoint(columnpoints, center, rootVector, perpendicularVector, sourcepipeline);
            if (valveColumnPoint == null)
            {
                Console.WriteLine("Error: No valve point found");
                return null;
            }

            previewOnly |= data.pipetype == null;
            previewOnly |= data.distributionSystemType == null;
            previewOnly |= data.transportSystemType == null;


            double minimumPipeLengthFt = 3 * Math.Max(this.data.transport_diameter, this.data.distribution_diameter) / 304.8;


            // Calculate the point to actually place the valve using an offset
            XYZ valvePoint = valveColumnPoint.Add(rootVector.Normalize().Multiply(data.valvecolumnDistance / 304.8));
            valvePoint = VectorUtils.Vector_setZ(valvePoint, data.valveheight / 304.8);

            data.previewPoints.Add(valvePoint);

            Line centerline = Line.CreateBound(center, VectorUtils.Vector_setZ(GeomUtils.GetClosestPoint(sourcepipeline, center), 0));

            Connector valve_in_c = null;
            Connector valve_out_c = null;
            XYZ valve_in_p = null;
            XYZ valve_out_p = null;


            if (!previewOnly && data.valvefamily != null)
            {
                // Place valve and find corresponding in and out points
                FamilyInstance valve = data.doc.Create.NewFamilyInstance(valvePoint, data.valvefamily, data.groundLevel, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                XYZ valvedir = ValveUtils.GetValveDirection(valve);
                Line up = Line.CreateUnbound(valvePoint, XYZ.BasisZ);
                valve.Location.Rotate(axis: up, valvedir.AngleOnPlaneTo(rootVector, XYZ.BasisZ));
                (valve_in_c, valve_out_c) = ValveUtils.GetValveConnectorPair(valve);
                valve_in_p = valve_in_c?.Origin;
                valve_out_p = valve_out_c?.Origin;


                // Check if the pipes will connect to the 'open' side of the connector
                if(valve_in_c.CoordinateSystem.BasisZ.DotProduct(new XYZ(0,0, data.transportlineheight / 304.8 - valve_in_p.Z)) < 0)
                {
                    valve_in_c = null;
                    data.errorMessages.Add(new DripData.DripErrorMessage("Pipe cannot be connected to In connector of the valve. Dummy connections will be created.", DripData.DripErrorMessage.Severity.WARNING));
                }
                if (valve_out_c.CoordinateSystem.BasisZ.DotProduct(new XYZ(0, 0, data.transportlineheight / 304.8 - valve_out_p.Z)) < 0)
                {
                    valve_out_c = null;
                    data.errorMessages.Add(new DripData.DripErrorMessage("Pipe cannot be connected to Out connector of the valve. Dummy connections will be created.", DripData.DripErrorMessage.Severity.WARNING));
                }
            }


            bool valveConnect = (valve_in_c != null && valve_out_c != null);

            if (previewOnly || !valveConnect)
            {
                // Create dummy in case of preview or valve creation failed
                valve_in_p = valvePoint.Add(rootVector.Normalize().Multiply(-200 / 304.8));
                valve_out_p = valvePoint.Add(rootVector.Normalize().Multiply(200 / 304.8));
            }

            // Resulting elements
            List<Pipe> pipes = new List<Pipe>();
            List<Pipe> transport_pipes = new List<Pipe>();
            List<Pipe> distribution_pipes = new List<Pipe>();

            List<Line> pipe_geometry = new List<Line>();

            // START CALCULATING ROUTING

            // ROUTE SOURCE TO VALVE
            // sourcepoint -> p1 -> valve_transport_corner_p -> p2 -> valve_in_p

            XYZ p2 = VectorUtils.Vector_setZ(valve_in_p, data.transportlineheight / 304.8);
            XYZ valve_transport_corner_p = p2.Add(perpendicularVector.Normalize().Multiply(data.pipecolumnDistance / 304.8));

            // Calculate ACTUAL source point
            XYZ sourcepoint = GeomUtils.GetClosestPoint(sourcepipeline, valve_transport_corner_p);
            XYZ p1 = VectorUtils.Vector_setZ(sourcepoint, data.transportlineheight / 304.8);


            // Checking sizes and distances
            double source_transport_heightdiff = Math.Abs(data.transportlineheight / 304.8 - sourcepoint.Z);
            if (source_transport_heightdiff < minimumPipeLengthFt && source_transport_heightdiff > 0.000000001f)
            {
                data.errorMessages.Add(new DripData.DripErrorMessage(string.Format("Height between source pipe and transport pipe is too small. Distance: {0}mm. No pipes will be generated.", source_transport_heightdiff * 304.8), DripData.DripErrorMessage.Severity.WARNING, unique: false));
                previewOnly = true;
            }
            double transport_distribution_heightdiff = Math.Abs(data.transportlineheight - data.distributionlineheight) / 304.8;
            if(transport_distribution_heightdiff < minimumPipeLengthFt)
            {
                data.errorMessages.Add(new DripData.DripErrorMessage(string.Format("Height between distribution pipe and transport pipe is too small. Distance needs to be at least {0}mm. No pipes will be generated.", minimumPipeLengthFt * 304.8), DripData.DripErrorMessage.Severity.WARNING, unique: false));
                previewOnly = true;
            }


            // ROUTE VALVE TO DISTRIBUTION TEE
            // offcenter:
            //  valve_out_p -> p3 -> valve_transport_corner_out_p -> p4 -> p5 -> tee
            // centered:
            //  valve_out_p -> p3 -> valve_transport_corner_out_p -> p5 -> tee
            bool offcenter = false;

            XYZ p3 = VectorUtils.Vector_setZ(valve_out_p, data.transportlineheight / 304.8);

            //GeomUtils.GetClosestPoints(centerline, columnpoints, 1/304.8);
            double minOffset = Math.Min(data.pipecolumnDistance, minimumPipeLengthFt);
            Line transportCenterLine = FindBestCenterLine(p3, centerline, columnpoints, minOffset);

            double distanceFromCenter = transportCenterLine.Distance(center);

            if (distanceFromCenter >= 0.000000001f)
            {
                offcenter = true;
                if(distanceFromCenter < minOffset)
                {
                    data.errorMessages.Add(new DripData.DripErrorMessage(string.Format("Transport line is too close to a column or other pipe. Distance: {0}mm. No pipes will be generated.", distanceFromCenter * 304.8), DripData.DripErrorMessage.Severity.WARNING));
                    previewOnly = true;
                }
            }

            XYZ valve_transport_corner_out_p = VectorUtils.Vector_setZ(GeomUtils.GetClosestPoint(transportCenterLine, p3), data.transportlineheight / 304.8);

            XYZ teeOffsetFromBackWall = rootVector.Normalize().Multiply(-data.backwallDistance / 304.8);
            XYZ tee = VectorUtils.Vector_setZ(center.Add(rootVector.Multiply(0.5).Add(teeOffsetFromBackWall)), data.distributionlineheight / 304.8);
            XYZ p5 = VectorUtils.Vector_setZ(tee, data.transportlineheight / 304.8);

            // Create extra elbow point in case its offcenter
            XYZ p4 = GeomUtils.GetClosestPoint(Line.CreateUnbound(valve_transport_corner_out_p, rootVector), p5);

            // CALCULATE TEE LINE
            XYZ sidePadding = perpendicularVector.Normalize().Multiply(-0.5 * data.intermediateDistance / 304.8);
            XYZ halfTee = perpendicularVector.Multiply(0.5).Add(sidePadding);

            XYZ tee_p1 = tee.Add(halfTee);
            XYZ tee_p2 = tee.Add(halfTee.Multiply(-1));



            // Source to valve
            pipe_geometry.Add(GeomUtils.CreateNamedLine(sourcepoint, p1, "Vertical from main line", data.errorMessages));

            pipe_geometry.Add(GeomUtils.CreateNamedLine(p1, valve_transport_corner_p, "From main line pointing towards valve", data.errorMessages));
            pipe_geometry.Add(GeomUtils.CreateNamedLine(valve_transport_corner_p, p2, "From an elbow to underneath the IN connector of the valve", data.errorMessages));
            pipe_geometry.Add(GeomUtils.CreateNamedLine(p2, valve_in_p, "Vertical to IN connector of the valve", data.errorMessages));

            // Valve to tee
            pipe_geometry.Add(GeomUtils.CreateNamedLine(valve_out_p, p3, "Vertical from OUT connector of the valve", data.errorMessages));
            pipe_geometry.Add(GeomUtils.CreateNamedLine(p3, valve_transport_corner_out_p, "From underneath the OUT connector to the start of the long transport line", data.errorMessages));
            if (offcenter)
            {
                pipe_geometry.Add(GeomUtils.CreateNamedLine(valve_transport_corner_out_p, p4, "Long transport line", data.errorMessages));
                pipe_geometry.Add(GeomUtils.CreateNamedLine(p4, p5, "From the end of the long transport line to underneath the tee", data.errorMessages));
            }
            else
            {
                pipe_geometry.Add(GeomUtils.CreateNamedLine(valve_transport_corner_out_p, p5, "Long transport line", data.errorMessages));
            }
            pipe_geometry.Add(GeomUtils.CreateNamedLine(p5, tee, "Vertical under the tee", data.errorMessages));

            // Tee
            pipe_geometry.Add(GeomUtils.CreateNamedLine(tee_p1, tee_p2, "Perpendicular branch line", data.errorMessages));


            // Check if there were any errors creating pipes
            if(pipe_geometry.RemoveAll(p => p == null) > 0)
            {
                previewOnly = true;
            }

            this.data.previewGeometry.AddRange(pipe_geometry);

            foreach(Line l in pipe_geometry)
            {
                if (l.Length < minimumPipeLengthFt)
                {
                    previewOnly = true;
                    this.data.errorMessages.Add(new DripData.DripErrorMessage("Generated pipe segment is too short! Branch will not be generated.", DripData.DripErrorMessage.Severity.WARNING));
                }
            }


            if (!previewOnly)
            {
                // Create all pipe placeholders

                // Route to valve
                Pipe l1 = Pipe.CreatePlaceholder(data.doc, data.transportSystemType.Id, data.pipetype.Id, data.groundLevel.Id, sourcepoint, p1);
                PlumbingUtils.ConnectPipePlaceholdersAtTee(data.doc, source.Id, l1.Id);
                Pipe l2 = Pipe.CreatePlaceholder(data.doc, data.transportSystemType.Id, data.pipetype.Id, data.groundLevel.Id, p1, valve_transport_corner_p);
                PlumbingUtils.ConnectPipePlaceholdersAtElbow(data.doc, l1.Id, l2.Id);
                Pipe l3 = Pipe.CreatePlaceholder(data.doc, data.transportSystemType.Id, data.pipetype.Id, data.groundLevel.Id, valve_transport_corner_p, p2);
                PlumbingUtils.ConnectPipePlaceholdersAtElbow(data.doc, l2.Id, l3.Id);
                Pipe l4 = Pipe.CreatePlaceholder(data.doc, data.transportSystemType.Id, data.pipetype.Id, data.groundLevel.Id, p2, valve_in_p);
                PlumbingUtils.ConnectPipePlaceholdersAtElbow(data.doc, l3.Id, l4.Id);

                ValveUtils.ConnectPipe(l4, valve_in_c);

                // Route from valve to tee
                Pipe l5 = Pipe.CreatePlaceholder(data.doc, data.transportSystemType.Id, data.pipetype.Id, data.groundLevel.Id, valve_out_p, p3);
                Pipe l6 = Pipe.CreatePlaceholder(data.doc, data.transportSystemType.Id, data.pipetype.Id, data.groundLevel.Id, p3, valve_transport_corner_out_p);
                PlumbingUtils.ConnectPipePlaceholdersAtElbow(data.doc, l5.Id, l6.Id);
                ValveUtils.ConnectPipe(l5, valve_out_c);

                Pipe l8;

                if (offcenter)
                {
                    Pipe l7 = Pipe.CreatePlaceholder(data.doc, data.transportSystemType.Id, data.pipetype.Id, data.groundLevel.Id, valve_transport_corner_out_p, p4);
                    PlumbingUtils.ConnectPipePlaceholdersAtElbow(data.doc, l6.Id, l7.Id);

                    l8 = Pipe.CreatePlaceholder(data.doc, data.transportSystemType.Id, data.pipetype.Id, data.groundLevel.Id, p4, p5);
                    PlumbingUtils.ConnectPipePlaceholdersAtElbow(data.doc, l7.Id, l8.Id);

                    pipes.Add(l7);
                    transport_pipes.Add(l7);
                }
                else
                {
                    l8 = Pipe.CreatePlaceholder(data.doc, data.transportSystemType.Id, data.pipetype.Id, data.groundLevel.Id, valve_transport_corner_out_p, p5);
                    PlumbingUtils.ConnectPipePlaceholdersAtElbow(data.doc, l6.Id, l8.Id);
                }

                Pipe l9 = Pipe.CreatePlaceholder(data.doc, data.transportSystemType.Id, data.pipetype.Id, data.groundLevel.Id, p5, tee);
                PlumbingUtils.ConnectPipePlaceholdersAtElbow(data.doc, l8.Id, l9.Id);


                // Distribution pipe
                Pipe teepipe = Pipe.CreatePlaceholder(data.doc, data.distributionSystemType.Id, data.pipetype.Id, data.groundLevel.Id, tee_p1, tee_p2);
                PlumbingUtils.ConnectPipePlaceholdersAtTee(data.doc, teepipe.Id, l9.Id);

                pipes.Add(l1);
                pipes.Add(l2);
                pipes.Add(l3);
                pipes.Add(l4);
                pipes.Add(l5);
                pipes.Add(l6);
                pipes.Add(l8);
                pipes.Add(l9);
                pipes.Add(teepipe);

                transport_pipes.Add(l1);
                transport_pipes.Add(l2);
                transport_pipes.Add(l3);
                transport_pipes.Add(l4);
                transport_pipes.Add(l5);
                transport_pipes.Add(l6);
                transport_pipes.Add(l8);
                transport_pipes.Add(l9);

                distribution_pipes.Add(teepipe);
            }

            // Set pipe sizes
            foreach (Pipe p in transport_pipes) Utils.SetSize(p, data.transport_diameter / 304.8);
            foreach (Pipe p in distribution_pipes) Utils.SetSize(p, data.distribution_diameter / 304.8);

            return pipes;
        }

    }
}
