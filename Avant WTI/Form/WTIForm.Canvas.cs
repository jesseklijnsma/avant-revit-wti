using System.Drawing;
using System.Windows.Forms;
using System;
using Autodesk.Revit.DB;
using Avant.WTI.Util;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB.Plumbing;

namespace Avant.WTI.Form
{
    partial class WTIForm
    {

        #region Rendering

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

            // Draw pipe lines
            foreach (Line line in pipe_lineMap.Values)
            {
                DrawLine(line, System.Drawing.Color.Yellow, false);
            }

            // Draw preview lines
            foreach (Line l in data.previewGeometry)
            {
                DrawLine(l, System.Drawing.Color.Aqua, false);
            }

            // Draw preview points
            foreach (RenderPoint p in data.previewPoints)
            {
                DrawPoint(p);
            }

            // Draw valve points
            foreach (RenderPoint p in data.valvePoints.Values)
            {
                DrawPoint(p);
            }

            // Render to canvas
            bg.Render();
            // Delete graphics buffer
            bg.Dispose();
        }


        /// <summary>
        ///     Draws a line from the Revit model coordinate space onto the canvas
        /// </summary>
        /// <param name="line">Model line to be drawn</param>
        /// <param name="c">Color</param>
        /// <param name="dashed">Sets line solid or dashed</param>
        private void DrawLine(Line line, System.Drawing.Color c, bool dashed = false)
        {
            // Convert model coordinate space to canvas coordinates
            Line screenLine = GeomUtils.LineToScreenLine(line, this.bounds, this.canvas.Size);
            if (screenLine == null) return;

            XYZ p1 = screenLine.GetEndPoint(0);
            XYZ p2 = screenLine.GetEndPoint(1);
            Pen pp = new Pen(c);
            if (dashed) pp.DashPattern = new float[] { 10, 10 };
            g.DrawLine(pp, (float)p1.X, (float)p1.Y, (float)p2.X, (float)p2.Y);
        }

        /// <summary>
        ///     Draws a polyline from the Revit model coordinate space onto the canvas
        /// </summary>
        /// <param name="polyLine">Model polyline to be drawn</param>
        /// <param name="c">Color</param>
        /// <param name="dashed">Sets line solid or dashed</param>
        private void DrawPolyLine(PolyLine polyLine, System.Drawing.Color c, bool dashed = false)
        {
            IList<XYZ> points = polyLine.GetCoordinates();

            // Map model points to canvas points
            points = points.Select(p => GeomUtils.PointToScreenPoint(p, bounds, this.canvas.Size)).ToList();
            if (points.Count < 2) return;

            Pen pp = new Pen(c);
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
        private void FillPolyLine(PolyLine polyLine, System.Drawing.Color c)
        {
            IList<XYZ> points = polyLine.GetCoordinates();

            // Map model points to canvas points
            points = points.Select(p => GeomUtils.PointToScreenPoint(p, bounds, this.canvas.Size)).ToList();
            if (points.Count < 2) return;

            Brush bb = new SolidBrush(c);
            PointF[] corners = points.Select(p => new PointF((float)p.X, (float)p.Y)).ToArray();
            g.FillPolygon(bb, corners);
        }

        /// <summary>
        ///     Draws a point from the Revit model coordinate space onto the canvas as a dot
        /// </summary>
        /// <param name="point">Point to be drawn</param>
        private void DrawPoint(RenderPoint point)
        {
            // Convert model coordinate space to canvas coordinates
            XYZ p = GeomUtils.PointToScreenPoint(point.Point, bounds, this.canvas.Size);
            float radius = point.GetPixelRadius(bounds, this.canvas.Size);
            SolidBrush brush = new SolidBrush(point.Color);
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

        private void Canvas_mousedown(object sender, MouseEventArgs e)
        {
            if (e.Button == PAN_BUTTON)
            {
                canvas_panbuttondown = true;
                previous_mouse_location = e.Location;
            }
            if(e.Button == MouseButtons.Left)
            {
                selectedArea = GetValvePointAreaUnderCursor(new XYZ(e.X, e.Y, 0));
            }
        }

        private void Canvas_mouseclick(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Right)
            {
                Pipe pipe = null;
                if(data.areapipemap.ContainsKey(areaUnderCursor)) pipe = data.areapipemap[areaUnderCursor];
                if(pipe == null)
                {
                    pipe = Utils.FindClosestPipe(data.pipelines, areaUnderCursor);
                }
                else
                {
                    pipe = null;
                }
                data.areapipemap[areaUnderCursor] = pipe;
                ReloadPreview();
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
            
            // Rerender
            canvas.Invalidate();
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
            XYZ mouseModel = GeomUtils.PointToModelPoint(mouse, bounds, this.canvas.Size);
            areaUnderCursor = AreaUtils.GetAreaAtPoint(data.areas, mouseModel);
        }

        private void HandleValveDrag(XYZ mouse)
        {
            XYZ mouseModel = GeomUtils.PointToModelPoint(mouse, bounds, this.canvas.Size);

            // Handle valve point moving
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
                bool reload = data.overrideValvePoints.ContainsKey(selectedArea) && data.overrideValvePoints[selectedArea] != valvePointSnap;

                data.overrideValvePoints[selectedArea] = valvePointSnap;

                if (reload) ReloadPreview();

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