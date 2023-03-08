using Autodesk.Revit.DB;
using System;
using System.Drawing;

namespace Avant.WTI.Util
{
    public class RenderPoint
    {


        public XYZ Point { get; }
        public System.Drawing.Color Color { get; }
        public float Radius { get; }
        public RenderUnits Units { get; }

        public RenderPoint(XYZ point, System.Drawing.Color color, float radius, RenderUnits units)
        {
            this.Point = point;
            this.Color = color;
            this.Radius = radius;
            this.Units = units;
        }

        public RenderPoint(XYZ point) : this(point, System.Drawing.Color.White) { }
        public RenderPoint(XYZ point, System.Drawing.Color color) : this(point, color, 3.0f, RenderUnits.PX) { }

        /// <summary>
        /// Converts the radius from set units to millimetres
        /// </summary>
        /// <param name="domain">Domain of model</param>
        /// <param name="targetSize">Canvas size</param>
        /// <returns>Radius in millimetres</returns>
        public float GetPixelRadius(RectangleF domain, System.Drawing.Size targetSize)
        {
            if (Units == RenderUnits.PX) return Radius;

            float radiusw = Radius * targetSize.Width / domain.Width;
            float radiush = Radius * targetSize.Height / domain.Height;
            return Math.Min(radiusw, radiush) / 304.8f;
        }

        public enum RenderUnits
        {
            MM,
            PX
        }

    }
}
