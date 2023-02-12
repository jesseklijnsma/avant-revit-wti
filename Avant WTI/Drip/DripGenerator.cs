using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
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
                List<ElementId> newIds = (List<ElementId>)PlumbingUtils.ConvertPipePlaceholders(this.data.doc, ids);
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

            // Calculate point closest point on the source line to the center
            XYZ sourcepoint = VectorUtils.Vector_setZ(GeomUtils.GetClosestPoint(sourceline, areacenter), 0);

            // Get points in front of source point
            List<XYZ> pointsInFront = new List<XYZ>();
            foreach (XYZ p in columnpoints)
            {
                // Check if the vector from the source point to the current point
                // has an angle smaller than 90 degrees to the rootVector
                if (p.Subtract(sourcepoint).DotProduct(rootVector) > 0.0)
                {
                    pointsInFront.Add(VectorUtils.Vector_setZ(p, 0));
                }
            }

            // Create line from source to center
            Line centerLine = Line.CreateBound(VectorUtils.Vector_setZ(areacenter, 0), sourcepoint);

            // Find the row of points that are together the clostest to the center line
            List<XYZ> centerLineClosePoints = GeomUtils.GetClosestPoints(centerLine, pointsInFront, 1);
            // Find the row of points that are together the clostest to the source line
            List<XYZ> closestPoints = GeomUtils.GetClosestPoints(sourceline, centerLineClosePoints, 1);

            // Sort points by distance to origin
            List<XYZ> sortedClosestPoints = closestPoints.OrderBy(p => VectorUtils.Vector_mask(p, perpendicularVector).GetLength()).ToList();

            if (sortedClosestPoints.Count == 0) return null;
            return sortedClosestPoints[0];
        }

        public void GeneratePreviewGeometry()
        {
            // Preview doesn't need any input validation

            this.data.previewGeometry.Clear();
            this.data.debugLines.Clear();
            this.data.previewPoints.Clear();
            foreach (Area area in this.data.areas)
            {
                // Try to get corresponding type to area
                Pipe pipe = null;
                if (this.data.areapipemap.ContainsKey(area)) pipe = this.data.areapipemap[area];
                if (pipe == null) pipe = Utils.FindClosestPipe(this.data.pipelines, area);
                if (pipe == null) continue;

                GenerateAreaBranch(pipe, area, previewOnly: true);
            }
        }

        public void GenerateDrip()
        {
            // Check if inputs are valid
            List<DripData.DripDataErrorMessage> msgs = data.getErrorMessages(DripData.Data.OUTPUT);
            DripData.DripDataErrorMessage.Severity maxSeverity = Utils.DisplayErrors(msgs);

            if (maxSeverity == DripData.DripDataErrorMessage.Severity.FATAL) return;


            // Create transaction
            Transaction t = new Transaction(this.data.doc);
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
                foreach (Area area in this.data.areas)
                {
                    // Try to get corresponding type to area
                    Pipe pipe = null;
                    if (this.data.areapipemap.ContainsKey(area)) pipe = this.data.areapipemap[area];
                    if (pipe == null) pipe = Utils.FindClosestPipe(this.data.pipelines, area);
                    if (pipe == null) continue;

                    // Add source pipe to the sources
                    sources.Add(pipe);

                    List<Pipe> pipes = GenerateAreaBranch(pipe, area, false);
                    if (pipes == null) continue;
                    placeholders.AddRange(pipes);
                }

                // Add the unique source pipes to all placeholders
                placeholders.AddRange(sources);


                if (this.data.convertPlaceholders)
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
            foreach (XYZ p in this.data.columnpoints)
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

            previewOnly |= this.data.pipetype == null;
            previewOnly |= this.data.distributionSystemType == null;
            previewOnly |= this.data.transportSystemType == null;



            // Calculate the point to actually place the valve using an offset
            XYZ valvePoint = valveColumnPoint.Add(rootVector.Normalize().Multiply(this.data.valvecolumnDistance / 304.8));
            valvePoint = VectorUtils.Vector_setZ(valvePoint, this.data.valveheight);

            this.data.previewPoints.Add(valvePoint);

            Line centerline = Line.CreateBound(center, VectorUtils.Vector_setZ(GeomUtils.GetClosestPoint(sourcepipeline, center), 0));

            Connector valve_in_c = null;
            Connector valve_out_c = null;
            XYZ valve_in_p = null;
            XYZ valve_out_p = null;


            if (!previewOnly && this.data.valvefamily != null)
            {
                // Place valve and find corresponding in and out points
                FamilyInstance valve = this.data.doc.Create.NewFamilyInstance(valvePoint, this.data.valvefamily, this.data.groundLevel, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                XYZ valvedir = ValveUtils.GetValveDirection(valve);
                Line up = Line.CreateUnbound(valvePoint, XYZ.BasisZ);
                valve.Location.Rotate(axis: up, valvedir.AngleOnPlaneTo(rootVector, XYZ.BasisZ));
                (valve_in_c, valve_out_c) = ValveUtils.GetValveConnectorPair(valve);
                valve_in_p = valve_in_c?.Origin;
                valve_out_p = valve_out_c?.Origin;
            }

            bool valveConnect = valve_in_c != null && valve_out_c != null;

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

            XYZ p2 = VectorUtils.Vector_setZ(valve_in_p, this.data.transportlineheight / 304.8);
            XYZ valve_transport_corner_p = p2.Add(perpendicularVector.Normalize().Multiply(this.data.pipecolumnDistance / 304.8));

            // Calculate ACTUAL source point
            XYZ sourcepoint = GeomUtils.GetClosestPoint(sourcepipeline, valve_transport_corner_p);
            XYZ p1 = VectorUtils.Vector_setZ(sourcepoint, this.data.transportlineheight / 304.8);



            // ROUTE VALVE TO DISTRIBUTION TEE
            // offcenter:
            //  valve_out_p -> p3 -> valve_transport_corner_out_p -> p4 -> p5 -> tee
            // centered:
            //  valve_out_p -> p3 -> valve_transport_corner_out_p -> p5 -> tee
            bool offcenter = false;

            XYZ p3 = VectorUtils.Vector_setZ(valve_out_p, this.data.transportlineheight / 304.8);
            XYZ valve_transport_corner_out_p = VectorUtils.Vector_setZ(GeomUtils.GetClosestPoint(centerline, p3), this.data.transportlineheight / 304.8);
            if (centerline.Distance(VectorUtils.Vector_setZ(p3, 0)) <= this.data.pipecolumnDistance / 304.8)
            {
                valve_transport_corner_out_p = p3.Add(perpendicularVector.Normalize().Multiply(this.data.pipecolumnDistance / 304.8));
                offcenter = true;
            }

            XYZ teeOffsetFromBackWall = rootVector.Normalize().Multiply(-this.data.backwallDistance / 304.8);
            XYZ tee = VectorUtils.Vector_setZ(center.Add(rootVector.Multiply(0.5).Add(teeOffsetFromBackWall)), this.data.distributionlineheight / 304.8);
            XYZ p5 = VectorUtils.Vector_setZ(tee, this.data.transportlineheight / 304.8);

            // Create extra elbow point in case its offcenter
            XYZ p4 = GeomUtils.GetClosestPoint(Line.CreateUnbound(valve_transport_corner_out_p, rootVector), p5);

            // CALCULATE TEE LINE
            XYZ sidePadding = perpendicularVector.Normalize().Multiply(-0.5 * this.data.intermediateDistance / 304.8);
            XYZ halfTee = perpendicularVector.Multiply(0.5).Add(sidePadding);

            XYZ tee_p1 = tee.Add(halfTee);
            XYZ tee_p2 = tee.Add(halfTee.Multiply(-1));

            if (!previewOnly)
            {
                // Create all pipe placeholders

                // Route to valve
                Pipe l1 = Pipe.CreatePlaceholder(this.data.doc, this.data.transportSystemType.Id, this.data.pipetype.Id, this.data.groundLevel.Id, sourcepoint, p1);
                PlumbingUtils.ConnectPipePlaceholdersAtTee(this.data.doc, source.Id, l1.Id);
                Pipe l2 = Pipe.CreatePlaceholder(this.data.doc, this.data.transportSystemType.Id, this.data.pipetype.Id, this.data.groundLevel.Id, p1, valve_transport_corner_p);
                PlumbingUtils.ConnectPipePlaceholdersAtElbow(this.data.doc, l1.Id, l2.Id);
                Pipe l3 = Pipe.CreatePlaceholder(this.data.doc, this.data.transportSystemType.Id, this.data.pipetype.Id, this.data.groundLevel.Id, valve_transport_corner_p, p2);
                PlumbingUtils.ConnectPipePlaceholdersAtElbow(this.data.doc, l2.Id, l3.Id);
                Pipe l4 = Pipe.CreatePlaceholder(this.data.doc, this.data.transportSystemType.Id, this.data.pipetype.Id, this.data.groundLevel.Id, p2, valve_in_p);
                PlumbingUtils.ConnectPipePlaceholdersAtElbow(this.data.doc, l3.Id, l4.Id);

                ValveUtils.ConnectPipe(l4, valve_in_c);

                // Route from valve to tee
                Pipe l5 = Pipe.CreatePlaceholder(this.data.doc, this.data.transportSystemType.Id, this.data.pipetype.Id, this.data.groundLevel.Id, valve_out_p, p3);
                Pipe l6 = Pipe.CreatePlaceholder(this.data.doc, this.data.transportSystemType.Id, this.data.pipetype.Id, this.data.groundLevel.Id, p3, valve_transport_corner_out_p);
                PlumbingUtils.ConnectPipePlaceholdersAtElbow(this.data.doc, l5.Id, l6.Id);
                ValveUtils.ConnectPipe(l5, valve_out_c);

                Pipe l8;

                if (offcenter)
                {
                    Pipe l7 = Pipe.CreatePlaceholder(this.data.doc, this.data.transportSystemType.Id, this.data.pipetype.Id, this.data.groundLevel.Id, valve_transport_corner_out_p, p4);
                    PlumbingUtils.ConnectPipePlaceholdersAtElbow(this.data.doc, l6.Id, l7.Id);

                    l8 = Pipe.CreatePlaceholder(this.data.doc, this.data.transportSystemType.Id, this.data.pipetype.Id, this.data.groundLevel.Id, p4, p5);
                    PlumbingUtils.ConnectPipePlaceholdersAtElbow(this.data.doc, l7.Id, l8.Id);

                    pipes.Add(l7);
                    transport_pipes.Add(l7);
                }
                else
                {
                    l8 = Pipe.CreatePlaceholder(this.data.doc, this.data.transportSystemType.Id, this.data.pipetype.Id, this.data.groundLevel.Id, valve_transport_corner_out_p, p5);
                    PlumbingUtils.ConnectPipePlaceholdersAtElbow(this.data.doc, l6.Id, l8.Id);
                }

                Pipe l9 = Pipe.CreatePlaceholder(this.data.doc, this.data.transportSystemType.Id, this.data.pipetype.Id, this.data.groundLevel.Id, p5, tee);
                PlumbingUtils.ConnectPipePlaceholdersAtElbow(this.data.doc, l8.Id, l9.Id);


                // Distribution pipe
                Pipe teepipe = Pipe.CreatePlaceholder(this.data.doc, this.data.distributionSystemType.Id, this.data.pipetype.Id, this.data.groundLevel.Id, tee_p1, tee_p2);
                PlumbingUtils.ConnectPipePlaceholdersAtTee(this.data.doc, teepipe.Id, l9.Id);

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

            // Source to valve
            pipe_geometry.Add(Line.CreateBound(sourcepoint, p1));
            pipe_geometry.Add(Line.CreateBound(p1, valve_transport_corner_p));
            pipe_geometry.Add(Line.CreateBound(valve_transport_corner_p, p2));
            pipe_geometry.Add(Line.CreateBound(p2, valve_in_p));

            // Valve to tee
            pipe_geometry.Add(Line.CreateBound(valve_out_p, p3));
            pipe_geometry.Add(Line.CreateBound(p3, valve_transport_corner_out_p));
            if (offcenter)
            {
                pipe_geometry.Add(Line.CreateBound(valve_transport_corner_out_p, p4));
                pipe_geometry.Add(Line.CreateBound(p4, p5));
            }
            else
            {
                pipe_geometry.Add(Line.CreateBound(valve_transport_corner_out_p, p5));
            }
            pipe_geometry.Add(Line.CreateBound(p5, tee));

            // Tee
            pipe_geometry.Add(Line.CreateBound(tee_p1, tee_p2));


            this.data.previewGeometry.AddRange(pipe_geometry);

            // Set pipe sizes
            foreach (Pipe p in transport_pipes) Utils.SetSize(p, this.data.transport_diameter / 304.8);
            foreach (Pipe p in distribution_pipes) Utils.SetSize(p, this.data.distribution_diameter / 304.8);

            return pipes;
        }

    }
}
