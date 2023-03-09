using System.Drawing;
using System.Windows.Forms;
using System;
using Autodesk.Revit.DB;
using Avant.WTI.Util;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB.Plumbing;
using Color = System.Drawing.Color;
using System.Windows;

namespace Avant.WTI.Form
{
    partial class WTIForm
    {

    #region Rendering


    byte dripAlpha = 0x7F;
    byte drainAlpha = 0x7F;

            
    private void Canvas_paint(object sender, PaintEventArgs e)
        {
            Graphics gr = e.Graphics;

            // Create graphics buffer
            System.Drawing.Rectangle rect = this.canvas.ClientRectangle;
            BufferedGraphicsContext bgm = BufferedGraphicsManager.Current;
            BufferedGraphics bg = bgm.Allocate(gr, rect);
            g = bg.Graphics;

            // Fill with background color
            g.Clear(System.Drawing.Color.FromArgb(54, 54, 54));

            if(areaUnderCursor != null)
            {
                if (areaLineMap.ContainsKey(areaUnderCursor))
                {
                    PolyLine line = areaLineMap[areaUnderCursor];
                    FillPolyLine(line, System.Drawing.Color.FromArgb(56, 56, 56));
                }
            }

            // Draw grid lines
            foreach (Line line in data.lines)
            {
                DrawLine(line, System.Drawing.Color.Black, false);
            }

            // Draw area lines
            foreach(PolyLine polyLine in areaLineMap.Values)
            {
                DrawPolyLine(polyLine, System.Drawing.Color.Green, true);
            }

            // Draw preview points
            foreach (RenderPoint p in data.previewPoints)
            {
                DrawPoint(p);
            }


            

            #region DRIP
            // Draw pipe lines
            foreach (Line line in pipe_lineMap.Values)
            {
                DrawLine(line, System.Drawing.Color.Yellow, false, alpha: dripAlpha);
            }

            // Draw preview lines
            foreach (Line l in data.previewGeometry)
            {
                DrawLine(l, System.Drawing.Color.Aqua, false, alpha: dripAlpha);
            }

            // Draw valve points
            foreach (RenderPoint p in data.valvePoints.Values)
            {
                DrawPoint(p, alpha: dripAlpha);
            }

            #endregion

            #region DRAIN

            if (data.drain.enabled)
            {
                RenderCollector();
            }

            #endregion

            // Render to canvas
            bg.Render();
            // Delete graphics buffer
            bg.Dispose();
        }


        private XYZ DrainHandle = null;

        private void RenderCollector()
        {
            float hw = 2000.0f;
            float hb = 200.0f;

            hw /= 304.8f;
            hb /= 304.8f;

            XYZ p = data.drain.collectorPoint;
            XYZ v = data.drain.collectorDirection.Normalize();

            XYZ pv = v.CrossProduct(XYZ.BasisZ).Normalize();

            XYZ p1 = p + pv * hw;
            XYZ p2 = p - pv * hw;

            //DrawPoint(new RenderPoint(p1, System.Drawing.Color.Yellow, hb, RenderPoint.RenderUnits.MM), alpha: drainAlpha);
            //DrawPoint(new RenderPoint(p2, System.Drawing.Color.Yellow, hb, RenderPoint.RenderUnits.MM), alpha: drainAlpha);

            XYZ p1t = p1 + v * hb;
            XYZ p1b = p1 - v * hb;
            XYZ p2t = p2 + v * hb;
            XYZ p2b = p2 - v * hb;


            IList<XYZ> points = new List<XYZ> { p1t, p2t, p2b, p1b };

            PolyLine pl = PolyLine.Create(points);
            FillPolyLine(pl, System.Drawing.Color.Yellow, alpha: drainAlpha);

            // Arrow
            XYZ arrowpoint = p + v * hw;
            XYZ arrowsource = p - v * hw;
            XYZ arrow1 = arrowpoint - v * hw * 0.4 - pv * hw * 0.3;
            XYZ arrow2 = arrowpoint - v * hw * 0.4 + pv * hw * 0.3;

            //IList<XYZ> arrowpoints = new List<XYZ> { p1t, p2t, p2b, p1b };
            //PolyLine arrowpl = PolyLine.Create(points);
            DrawLine(Line.CreateBound(arrowsource, arrowpoint), System.Drawing.Color.Yellow, alpha: drainAlpha);
            DrawLine(Line.CreateBound(arrowpoint, arrow1), System.Drawing.Color.Yellow, alpha: drainAlpha);
            DrawLine(Line.CreateBound(arrowpoint, arrow2), System.Drawing.Color.Yellow, alpha: drainAlpha);

            DrawPoint(new RenderPoint(arrowpoint, Color.White, 5, RenderPoint.RenderUnits.PX), alpha: (byte)(drainAlpha / 2));

            DrainHandle = arrowpoint;
        }



        /// <summary>
        ///     Draws a line from the Revit model coordinate space onto the canvas
        /// </summary>
        /// <param name="line">Model line to be drawn</param>
        /// <param name="c">Color</param>
        /// <param name="dashed">Sets line solid or dashed</param>
        /// <param name="alpha">Transparency</param>
        private void DrawLine(Line line, System.Drawing.Color c, bool dashed = false, byte alpha = 255)
        {
            // Convert model coordinate space to canvas coordinates
            Line screenLine = GeomUtils.LineToScreenLine(line, this.bounds, this.canvas.Size);
            if (screenLine == null) return;

            XYZ p1 = screenLine.GetEndPoint(0);
            XYZ p2 = screenLine.GetEndPoint(1);
            Color color = Color.FromArgb(alpha, c);
            Pen pp = new Pen(color);
            if (dashed) pp.DashPattern = new float[] { 10, 10 };
            g.DrawLine(pp, (float)p1.X, (float)p1.Y, (float)p2.X, (float)p2.Y);
        }

        /// <summary>
        ///     Draws a polyline from the Revit model coordinate space onto the canvas
        /// </summary>
        /// <param name="polyLine">Model polyline to be drawn</param>
        /// <param name="c">Color</param>
        /// <param name="dashed">Sets line solid or dashed</param>
        /// <param name="alpha">Transparency</param>
        private void DrawPolyLine(PolyLine polyLine, System.Drawing.Color c, bool dashed = false, byte alpha = 255)
        {
            IList<XYZ> points = polyLine.GetCoordinates();

            // Map model points to canvas points
            points = points.Select(p => GeomUtils.PointToScreenPoint(p, bounds, this.canvas.Size)).ToList();
            if (points.Count < 2) return;

            Color color = Color.FromArgb(alpha, c);

            Pen pp = new Pen(color);
            if (dashed) pp.DashPattern = new float[] { 10, 10 };

            for (int i = 0; i < points.Count - 1; i++)
            {
                XYZ p1 = points[i];
                XYZ p2 = points[i + 1];
                g.DrawLine(pp, (float)p1.X, (float)p1.Y, (float)p2.X, (float)p2.Y);
            }
        }

        /// <summary>
        ///     Draws a polyline from the Revit model coordinate space onto the canvas
        /// </summary>
        /// <param name="polyLine">Model polyline to be drawn</param>
        /// <param name="c">Color</param>
        /// <param name="alpha">Transparency</param>
        private void FillPolyLine(PolyLine polyLine, System.Drawing.Color c, byte alpha = 255)
        {
            IList<XYZ> points = polyLine.GetCoordinates();

            // Map model points to canvas points
            points = points.Select(p => GeomUtils.PointToScreenPoint(p, bounds, this.canvas.Size)).ToList();
            if (points.Count < 2) return;

            Color color = Color.FromArgb(alpha, c);

            Brush bb = new SolidBrush(color);
            PointF[] corners = points.Select(p => new PointF((float)p.X, (float)p.Y)).ToArray();
            g.FillPolygon(bb, corners);
        }

        /// <summary>
        ///     Draws a point from the Revit model coordinate space onto the canvas as a dot
        /// </summary>
        /// <param name="point">Point to be drawn</param>
        /// <param name="alpha">Transparency</param>
        private void DrawPoint(RenderPoint point, byte alpha = 255)
        {
            // Convert model coordinate space to canvas coordinates
            XYZ p = GeomUtils.PointToScreenPoint(point.Point, bounds, this.canvas.Size);
            float radius = point.GetPixelRadius(bounds, this.canvas.Size);

            Color color = Color.FromArgb(alpha, point.Color);

            SolidBrush brush = new SolidBrush(color);
            g.FillEllipse(brush, new RectangleF((float)(p.X - radius), (float)(p.Y - radius), radius * 2, radius * 2));
        }

        #endregion


        #region Zoom and Pan handling

        private void Canvas_mousewheel(object sender, MouseEventArgs e)
        {
            // Convert mouse wheel delta to detents
            int n = e.Delta / 120;

            // Calculate scale (how much the canvas should zoom in)
            float rscale = (float)Math.Pow(2, CANVAS_ZOOM_SPEED * n);

            // Calculate new position for the bounds to keep the mouse on the same model coordinate (magic)
            float dx = e.X * this.bounds.Width * (1 - rscale) / this.canvas.Width;
            float dy = (this.canvas.Height - e.Y) * this.bounds.Height * (1 - rscale) / (this.canvas.Height);

            // Check if not zoomed it too far
            if (this.bounds.Width * rscale < MAX_ZOOM_SIZE_MM / 304.8 || this.bounds.Height * rscale < MAX_ZOOM_SIZE_MM / 304.8) return;


            // Scale the bounds
            // (Negative rscale = zoom in, Positive rscale = zoom out)
            this.bounds.Width *= rscale;
            this.bounds.Height *= rscale;

            this.bounds.X += dx;
            this.bounds.Y += dy;

            Canvas_checkBounds();

            // Rerender
            this.canvas.Invalidate();
        }


        private bool canvas_panbuttondown = false;
        private System.Drawing.Point previous_mouse_location = new System.Drawing.Point();

        private Area selectedArea = null;
        private bool HandleActive = false;

        private void Canvas_mousedown(object sender, MouseEventArgs e)
        {
            XYZ mouse = new XYZ(e.X, e.Y, 0);
            if (e.Button == PAN_BUTTON)
            {
                canvas_panbuttondown = true;
                previous_mouse_location = e.Location;
            }
            if(ActiveTab == Tab.DRIP)
            {
                if(e.Button == MouseButtons.Left)
                {
                    selectedArea = GetValvePointAreaUnderCursor(mouse);
                }
            }
            if(ActiveTab == Tab.DRAIN)
            {
                if(e.Button == MouseButtons.Left)
                {
                    HandleActive = CursorAboveHandle(mouse);
                }
            }


        }

        private void Canvas_mouseclick(object sender, MouseEventArgs e)
        {
            if (ActiveTab == Tab.DRIP)
            {
                if (e.Button == MouseButtons.Right)
                {
                    Pipe pipe = null;
                    if (data.drip.areapipemap.ContainsKey(areaUnderCursor)) pipe = data.drip.areapipemap[areaUnderCursor];
                    if (pipe == null)
                    {
                        pipe = Utils.FindClosestPipe(data.drip.pipelines, areaUnderCursor);
                    }
                    else
                    {
                        pipe = null;
                    }
                    data.drip.areapipemap[areaUnderCursor] = pipe;
                    ReloadPreview();
                }
            }
        }

        private void Canvas_mouseup(object sender, MouseEventArgs e)
        {
            if (e.Button == PAN_BUTTON)
            {
                canvas_panbuttondown = false;
            }
            if (e.Button == MouseButtons.Left)
            {
                selectedArea = null;
                HandleActive = false;
            }
        }

        private void Canvas_mouseexit(object sender, EventArgs e)
        {
            XYZ mouse = new XYZ(previous_mouse_location.X, previous_mouse_location.Y, 0);

            HandleAreaActions(mouse);
        }

        private void Canvas_mousemove(object sender, MouseEventArgs e)
        {
            XYZ mouse = new XYZ(e.X, e.Y, 0);
            //XYZ mouseModel = GeomUtils.PointToModelPoint(mouse, bounds, this.canvas.Size);


            HandleAreaActions(mouse);
            HandleValveDrag(mouse);
            HandlePan(e);
            HandleDrainCollector(mouse);
            
            // Rerender
            canvas.Invalidate();
        }

        private List<XYZ> AvailableCollectorDirections = new List<XYZ> { XYZ.BasisX, -XYZ.BasisX, XYZ.BasisY, -XYZ.BasisY };

        private void HandleDrainCollector(XYZ mouseScreen)
        {
            if(HandleActive)
            {
                Cursor.Current = Cursors.SizeAll;

                XYZ collectorScreen = GeomUtils.PointToScreenPoint(data.drain.collectorPoint, bounds, this.canvas.Size);


                XYZ dir = VectorUtils.Vector_setZ(mouseScreen - collectorScreen, 0).Normalize();
                XYZ newDir = AvailableCollectorDirections.OrderBy(v => v.AngleTo(dir)).First();

                data.drain.collectorDirection = new XYZ(newDir.X, -newDir.Y, 0);
            }
            else
            {
                if (CursorAboveHandle(mouseScreen))
                {
                    Cursor.Current = Cursors.SizeAll;
                }
                else
                {
                    Cursor.Current = Cursors.Default;
                }
            }
        }

        private bool CursorAboveHandle(XYZ mouse)
        {
            if (DrainHandle == null) return false;

            XYZ handle = GeomUtils.PointToScreenPoint(DrainHandle, bounds, canvas.Size);

            double grabDist = 10.0;

            if (handle.DistanceTo(mouse) < grabDist) return true;
            return false;
        }

        private void HandlePan(MouseEventArgs e)
        {
            // Handle pan
            if (canvas_panbuttondown)
            {

                // Calculate mouse dx, dy
                float dx = previous_mouse_location.X - e.Location.X;
                float dy = e.Location.Y - previous_mouse_location.Y;

                // Scale dx,dy from canvas to model
                dx *= this.bounds.Width / this.canvas.Width;
                dy *= this.bounds.Height / this.canvas.Height;

                this.bounds.Location += new SizeF(dx, dy);

                Canvas_checkBounds();

                previous_mouse_location = e.Location;
            }
        }

        private Area areaUnderCursor = null;

        private void HandleAreaActions(XYZ mouse)
        {
            if (ActiveTab == Tab.DRIP)
            {
                XYZ mouseModel = GeomUtils.PointToModelPoint(mouse, bounds, this.canvas.Size);
                areaUnderCursor = AreaUtils.GetAreaAtPoint(data.areas, mouseModel);
            }
            else
            {
                areaUnderCursor = null;
            }
        }

        private void HandleValveDrag(XYZ mouse)
        {
            XYZ mouseModel = GeomUtils.PointToModelPoint(mouse, bounds, this.canvas.Size);

            if (ActiveTab == Tab.DRIP)
            {
                if (selectedArea == null)
                {
                    Area area = GetValvePointAreaUnderCursor(mouse);
                    if (area == null)
                    {
                        Cursor.Current = Cursors.Default;
                    }
                    else
                    {
                        Cursor.Current = Cursors.Hand;
                    }
                }
                else
                {
                    // Dragging
                    Cursor.Current = Cursors.Hand;

                    XYZ valvePointSnap = this.data.columnpoints.OrderBy(p => p.DistanceTo(mouseModel)).FirstOrDefault();
                    bool reload = data.drip.overrideValvePoints.ContainsKey(selectedArea) && data.drip.overrideValvePoints[selectedArea] != valvePointSnap;

                    data.drip.overrideValvePoints[selectedArea] = valvePointSnap;

                    if (reload) ReloadPreview();
                }
            }
            else
            {
                Cursor.Current = Cursors.Default;
            }

        }

        private Area GetValvePointAreaUnderCursor(XYZ mousePoint)
        {
            foreach(KeyValuePair<Area,RenderPoint> kv in data.valvePoints)
            {
                RenderPoint point = kv.Value;
                Area area = kv.Key;

                XYZ screenPoint = GeomUtils.PointToScreenPoint(point.Point, bounds, this.canvas.Size);

                double selectionMarginPx = Math.Max(2.0, point.GetPixelRadius(bounds, this.canvas.Size) * 2);

                if (screenPoint.DistanceTo(mousePoint) < selectionMarginPx)
                {
                    return area;
                }
            }
            return null;
        }

        /// <summary>
        /// Makes sure the bounds are within the maximum bounds of the model
        /// if not, it will rescale/reposition it to fit inside the maximum bounds
        /// </summary>
        private void Canvas_checkBounds()
        {
            if (this.bounds.Width > this.maxBounds.Width) this.bounds.Width = this.maxBounds.Width;
            if (this.bounds.Height > this.maxBounds.Height) this.bounds.Height = this.maxBounds.Height;

            if (this.bounds.Left < this.maxBounds.Left) this.bounds.X = this.maxBounds.X;
            if (this.bounds.Top < this.maxBounds.Top) this.bounds.Y = this.maxBounds.Y;
            if (this.bounds.Right > this.maxBounds.Right) this.bounds.X = this.maxBounds.Right - this.bounds.Width;
            if (this.bounds.Bottom > this.maxBounds.Bottom) this.bounds.Y = this.maxBounds.Bottom - this.bounds.Height;
        }

        #endregion

        private void Canvas_resize(object sender, EventArgs e)
        {
            // Rerender canvas when resizes
            canvas.Invalidate();
        }

        private void WTIForm_Activated(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
        }

    }
}