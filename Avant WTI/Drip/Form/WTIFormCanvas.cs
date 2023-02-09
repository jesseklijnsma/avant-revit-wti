using System.Drawing;
using System.Windows.Forms;
using System;
using Autodesk.Revit.DB;
using Avant.WTI.Util;
using System.Collections.Generic;

namespace Avant.WTI.Drip.Form
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
            //g.SetClip(new RectangleF(new PointF(), this.canvas.Size));

            // Fill with background color
            g.Clear(System.Drawing.Color.FromArgb(54, 54, 54));

            // Draw grid lines
            foreach (Line line in this.data.lines)
            {
                DrawLine(line, System.Drawing.Color.Black, false);
            }

            // Draw area lines
            foreach (List<Line> linelist in areaLineMap.Values)
            {
                foreach (Line l in linelist)
                {
                    DrawLine(l, System.Drawing.Color.Green, true);
                }
            }

            // Draw pipe lines
            foreach (Line line in pipe_lineMap.Values)
            {
                DrawLine(line, System.Drawing.Color.Yellow, false);
            }

            // Draw preview lines
            foreach (Line l in this.data.previewGeometry)
            {
                DrawLine(l, System.Drawing.Color.Aqua, false);
            }

            // Draw preview points
            foreach (XYZ p in this.data.previewPoints)
            {
                DrawPoint(p, System.Drawing.Color.White, 3);
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
            Line screenLine = Utils.lineToScreenLine(line, this.bounds, this.canvas.Size);
            if (screenLine == null) return;

            XYZ p1 = screenLine.GetEndPoint(0);
            XYZ p2 = screenLine.GetEndPoint(1);
            Pen pp = new Pen(c);
            if (dashed) pp.DashPattern = new float[] { 10, 10 };
            g.DrawLine(pp, (float)p1.X, (float)p1.Y, (float)p2.X, (float)p2.Y);
        }

        /// <summary>
        ///     Draws a point from the Revit model coordinate space onto the canvas as a dot
        /// </summary>
        /// <param name="point">Point to be drawn</param>
        /// <param name="c">Color</param>
        /// <param name="radius">Dot radius in pixels</param>
        private void DrawPoint(XYZ point, System.Drawing.Color c, float radius)
        {
            // Convert model coordinate space to canvas coordinates
            XYZ p = Utils.pointToScreenPoint(point, bounds, this.canvas.Size);
            SolidBrush brush = new SolidBrush(c);
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

        private void Canvas_mousedown(object sender, MouseEventArgs e)
        {
            if (e.Button == PAN_BUTTON)
            {
                canvas_panbuttondown = true;
                previous_mouse_location = e.Location;
            }
        }

        private void Canvas_mouseup(object sender, MouseEventArgs e)
        {
            if (e.Button == PAN_BUTTON)
            {
                canvas_panbuttondown = false;
            }
        }

        private void Canvas_mousemove(object sender, MouseEventArgs e)
        {
            if (canvas_panbuttondown)
            {
                // Handle pan

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
            // Rerender
            canvas.Invalidate();
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