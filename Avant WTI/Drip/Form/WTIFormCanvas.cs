using System.Drawing;
using System.Windows.Forms;
using System;
using Autodesk.Revit.DB;
using Avant.WTI.Util;

namespace Avant.WTI.Drip.Form
{
    partial class WTIForm
    {

        private void DrawLine(Line line, System.Drawing.Color c, bool dashed)
        {
            Line screenLine = Utils.lineToScreenLine(line, this.bounds, this.canvas.Size);
            if (screenLine == null) return;

            XYZ p1 = screenLine.GetEndPoint(0);
            XYZ p2 = screenLine.GetEndPoint(1);
            Pen pp = new Pen(c);
            if (dashed) pp.DashPattern = new float[] { 10, 10 };
            g.DrawLine(pp, (float)p1.X, (float)p1.Y, (float)p2.X, (float)p2.Y);
        }

        private void DrawPoint(XYZ point, System.Drawing.Color c, float radius)
        {
            XYZ p = Utils.pointToScreenPoint(point, bounds, this.canvas.Size);
            SolidBrush brush = new SolidBrush(c);
            g.FillEllipse(brush, new RectangleF((float)(p.X - radius), (float)(p.Y - radius), radius * 2, radius * 2));
        }



        private void Canvas_mousewheel(object sender, MouseEventArgs e)
        {
            int n = e.Delta / 120;
            float rscale = (float)Math.Pow(2, -0.5 * n);

            float dx = e.X * this.bounds.Width * (1 - rscale) / this.canvas.Width;
            float dy = (this.canvas.Height - e.Y) * this.bounds.Height * (1 - rscale) / (this.canvas.Height);

            this.bounds.Width *= rscale;
            this.bounds.Height *= rscale;

            this.bounds.X += dx;
            this.bounds.Y += dy;

            Canvas_checkBounds();

            this.canvas.Invalidate();
        }

        private void Canvas_checkBounds()
        {
            if (this.bounds.Width > this.maxBounds.Width) this.bounds.Width = this.maxBounds.Width;
            if (this.bounds.Height > this.maxBounds.Height) this.bounds.Height = this.maxBounds.Height;

            if (this.bounds.Left < this.maxBounds.Left) this.bounds.X = this.maxBounds.X;
            if (this.bounds.Top < this.maxBounds.Top) this.bounds.Y = this.maxBounds.Y;
            if (this.bounds.Right > this.maxBounds.Right) this.bounds.X = this.maxBounds.Right - this.bounds.Width;
            if (this.bounds.Bottom > this.maxBounds.Bottom) this.bounds.Y = this.maxBounds.Bottom - this.bounds.Height;
        }

        private bool canvas_buttondown = false;
        private System.Drawing.Point canvas_mouse_location = new System.Drawing.Point();

        private void Canvas_mousedown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                canvas_buttondown = true;
                canvas_mouse_location = e.Location;
            }
        }

        private void Canvas_mouseup(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                canvas_buttondown = false;
            }
        }

        private void Canvas_mousemove(object sender, MouseEventArgs e)
        {
            if (canvas_buttondown)
            {
                float dx = canvas_mouse_location.X - e.Location.X;
                float dy = e.Location.Y - canvas_mouse_location.Y;

                dx *= this.bounds.Width / this.canvas.Width;
                dy *= this.bounds.Height / this.canvas.Height;

                this.bounds.Location += new SizeF(dx, dy);

                Canvas_checkBounds();

                canvas_mouse_location = e.Location;
            }
            canvas.Invalidate();
        }
        private void Canvas_resize(object sender, EventArgs e)
        {
            canvas.Invalidate();
        }

        private void WTIForm_Activated(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
        }

    }
}