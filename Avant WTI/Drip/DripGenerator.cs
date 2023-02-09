﻿using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
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


        private XYZ FindValvePoint(List<XYZ> columnpoints, XYZ areacenter, XYZ rootVector, XYZ perpendicularVector, Line sourceline)
        {
            if (columnpoints.Count == 0) return XYZ.Zero;

            XYZ sourcepoint = VectorUtils.vector_setZ(GeomUtils.getClosestPoint(sourceline, areacenter), 0);

            // Get points in front of source point
            List<XYZ> pointsInFront = new List<XYZ>();
            foreach (XYZ p in columnpoints)
            {
                if (p.Subtract(sourcepoint).DotProduct(rootVector) > 0.0)
                {
                    pointsInFront.Add(VectorUtils.vector_setZ(p, 0));
                }
            }

            Line centerLine = Line.CreateBound(VectorUtils.vector_setZ(areacenter, 0), sourcepoint);

            List<XYZ> centerLineClosePoints = GeomUtils.getClosestPoints(centerLine, pointsInFront, 1);
            List<XYZ> closestPoints = GeomUtils.getClosestPoints(sourceline, centerLineClosePoints, 1);

            // Sort by distance to origin
            List<XYZ> sortedClosestPoints = closestPoints.OrderBy(p => VectorUtils.vector_mask(p, perpendicularVector).GetLength()).ToList();

            if (sortedClosestPoints.Count == 0) return null;
            return sortedClosestPoints[0];
        }

        public void GeneratePreviewGeometry()
        {
            this.data.previewGeometry.Clear();
            this.data.debugLines.Clear();
            this.data.previewPoints.Clear();
            foreach (Area area in this.data.areas)
            {
                Pipe pipe = null;
                if (this.data.areapipemap.ContainsKey(area)) pipe = this.data.areapipemap[area];
                if (pipe == null) pipe = Utils.findClosestPipe(this.data.pipelines, area);
                if (pipe == null) continue;

                GenerateAreaBranch(pipe, area, this.data.columnpoints, preview: true);
            }
        }

        public void GenerateDrip()
        {

            if (!this.data.isValidOutput())
            {
                string message = "The inputs are not valid!";
                string caption = "";
                MessageBox.Show(message, caption, MessageBoxButtons.OK);
                return;
            }

            Transaction t = new Transaction(this.data.doc);
            t.Start("Drip generation");

            FailureHandlingOptions fho = t.GetFailureHandlingOptions();
            fho.SetForcedModalHandling(false);
            t.SetFailureHandlingOptions(fho);

            try
            {
                List<Pipe> placeholders = new List<Pipe>();
                HashSet<Pipe> sources = new HashSet<Pipe>();
                foreach (Area area in this.data.areas)
                {
                    Pipe pipe = null;
                    if (this.data.areapipemap.ContainsKey(area)) pipe = this.data.areapipemap[area];
                    if (pipe == null) pipe = Utils.findClosestPipe(this.data.pipelines, area);
                    if (pipe == null) continue;

                    sources.Add(pipe);

                    List<Pipe> pipes = GenerateAreaBranch(pipe, area, this.data.columnpoints, false);
                    if (pipes == null) continue;
                    placeholders.AddRange(pipes);
                }

                placeholders.AddRange(sources);


                if (this.data.convertPlaceholders)
                {
                    List<ElementId> newpipes = ConvertPlaceholders(placeholders);
                }

                t.Commit();
            }
            catch (Exception e)
            {
                string message = "An exception has occured";
                string caption = e.Message + "\n" + e.StackTrace;

                MessageBoxButtons buttons = MessageBoxButtons.OK;
                MessageBox.Show(message, caption, buttons, MessageBoxIcon.Warning);
                t.RollBack();
                throw;
            }
        }

        private List<Pipe> GenerateAreaBranch(Pipe source, Area area, List<XYZ> columnpoints, bool preview = false)
        {
            RectangleF arearect = AreaUtils.getAreaRectangle(area);
            XYZ center = Utils.rectangleGetCenter(arearect);
            XYZ areavector = new XYZ(arearect.Width, arearect.Height, 0);

            Line sourceline = ((LocationCurve)source.Location).Curve as Line;
            XYZ connectionPoint = GeomUtils.getClosestPoint(sourceline, center);

            XYZ branchinwardvector = center.Subtract(connectionPoint).Normalize();
            branchinwardvector = VectorUtils.vector_round(branchinwardvector);


            List<XYZ> areaColumnPoints = new List<XYZ>();
            foreach (XYZ p in columnpoints)
            {
                if (Utils.rectangleIntersect(arearect, p, 1))
                {
                    areaColumnPoints.Add(p);
                }
            }


            XYZ rootVector = VectorUtils.vector_mask(branchinwardvector, areavector);
            XYZ perpendicularVector = VectorUtils.vector_mask(branchinwardvector.CrossProduct(XYZ.BasisZ), areavector);

            return GenerateBranch(source, arearect, rootVector, perpendicularVector, areaColumnPoints, preview);
        }

        private List<Pipe> GenerateBranch(Pipe source, RectangleF areaRect, XYZ rootVector, XYZ perpendicularVector, List<XYZ> columnpoints, bool previewOnly = false)
        {
            Line sourcepipeline = ((LocationCurve)source.Location).Curve as Line;
            XYZ center = Utils.rectangleGetCenter(areaRect);


            XYZ valveColumnPoint = FindValvePoint(columnpoints, center, rootVector, perpendicularVector, sourcepipeline);
            if (valveColumnPoint == null)
            {
                Console.WriteLine("Error: No valve point found");
                return null;
            }


            XYZ valvePoint = valveColumnPoint.Add(rootVector.Normalize().Multiply(this.data.valvecolumnDistance / 304.8));
            valvePoint = VectorUtils.vector_setZ(valvePoint, this.data.valveheight);

            this.data.previewPoints.Add(valvePoint);

            Line centerline = Line.CreateBound(center, VectorUtils.vector_setZ(GeomUtils.getClosestPoint(sourcepipeline, center), 0));

            Connector valve_in_c = null;
            Connector valve_out_c = null;
            XYZ valve_in_p = null;
            XYZ valve_out_p = null;


            if (!previewOnly)
            {
                // Place valve and find corresponding in and out points
                FamilyInstance valve = this.data.doc.Create.NewFamilyInstance(valvePoint, this.data.valvefamily, this.data.groundLevel, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                XYZ valvedir = ValveUtils.getValveDirection(valve);
                Line up = Line.CreateUnbound(valvePoint, XYZ.BasisZ);
                valve.Location.Rotate(up, valvedir.AngleOnPlaneTo(rootVector, XYZ.BasisZ));
                (valve_in_c, valve_out_c) = ValveUtils.getValveConnectorPair(valve);
                valve_in_p = valve_in_c?.Origin;
                valve_out_p = valve_out_c?.Origin;
            }

            bool valveConnect = valve_in_c != null && valve_out_c != null;

            if (!previewOnly && !valveConnect)
            {

            }

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

            XYZ p2 = VectorUtils.vector_setZ(valve_in_p, this.data.transportlineheight / 304.8);
            XYZ valve_transport_corner_p = p2.Add(perpendicularVector.Normalize().Multiply(this.data.pipecolumnDistance / 304.8));

            XYZ sourcepoint = GeomUtils.getClosestPoint(sourcepipeline, valve_transport_corner_p);
            XYZ p1 = VectorUtils.vector_setZ(sourcepoint, this.data.transportlineheight / 304.8);



            // ROUTE VALVE TO DISTRIBUTION TEE
            // offcenter:
            //  valve_out_p -> p3 -> valve_transport_corner_out_p -> p4 -> p5 -> tee
            // centered:
            //  valve_out_p -> p3 -> valve_transport_corner_out_p -> p5 -> tee
            bool offcenter = false;

            XYZ p3 = VectorUtils.vector_setZ(valve_out_p, this.data.transportlineheight / 304.8);
            XYZ valve_transport_corner_out_p = VectorUtils.vector_setZ(GeomUtils.getClosestPoint(centerline, p3), this.data.transportlineheight / 304.8);
            if (centerline.Distance(VectorUtils.vector_setZ(p3, 0)) <= this.data.pipecolumnDistance / 304.8)
            {
                valve_transport_corner_out_p = p3.Add(perpendicularVector.Normalize().Multiply(this.data.pipecolumnDistance / 304.8));
                offcenter = true;
            }

            XYZ teeOffsetFromBackWall = rootVector.Normalize().Multiply(-this.data.backwallDistance / 304.8);
            XYZ tee = VectorUtils.vector_setZ(center.Add(rootVector.Multiply(0.5).Add(teeOffsetFromBackWall)), this.data.distributionlineheight / 304.8);
            XYZ p5 = VectorUtils.vector_setZ(tee, this.data.transportlineheight / 304.8);

            // Create extra elbow point in case its offcenter
            XYZ p4 = GeomUtils.getClosestPoint(Line.CreateUnbound(valve_transport_corner_out_p, rootVector), p5);

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

                ValveUtils.connectPipe(l4, valve_in_c);

                // Route from valve to tee
                Pipe l5 = Pipe.CreatePlaceholder(this.data.doc, this.data.transportSystemType.Id, this.data.pipetype.Id, this.data.groundLevel.Id, valve_out_p, p3);
                Pipe l6 = Pipe.CreatePlaceholder(this.data.doc, this.data.transportSystemType.Id, this.data.pipetype.Id, this.data.groundLevel.Id, p3, valve_transport_corner_out_p);
                PlumbingUtils.ConnectPipePlaceholdersAtElbow(this.data.doc, l5.Id, l6.Id);
                ValveUtils.connectPipe(l5, valve_out_c);

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
            foreach (Pipe p in transport_pipes) Utils.setSize(p, this.data.transport_diameter / 304.8);
            foreach (Pipe p in distribution_pipes) Utils.setSize(p, this.data.distribution_diameter / 304.8);

            return pipes;
        }

    }
}
