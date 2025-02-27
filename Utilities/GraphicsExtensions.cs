using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace iTextDesignerWithGUI.Utilities
{
    /// <summary>
    /// Extension methods for the Graphics class
    /// </summary>
    public static class GraphicsExtensions
    {
        /// <summary>
        /// Draws a rounded rectangle using the specified pen and corner radius
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="pen">The pen to use for drawing</param>
        /// <param name="bounds">The bounding rectangle</param>
        /// <param name="cornerRadius">The radius of the corners</param>
        public static void DrawRoundedRectangle(this Graphics g, Pen pen, Rectangle bounds, int cornerRadius)
        {
            if (g == null) throw new ArgumentNullException(nameof(g));
            if (pen == null) throw new ArgumentNullException(nameof(pen));
            
            using (var path = RoundedRect(bounds, cornerRadius))
            {
                g.DrawPath(pen, path);
            }
        }
        
        /// <summary>
        /// Creates a rounded rectangle GraphicsPath
        /// </summary>
        /// <param name="bounds">The bounding rectangle</param>
        /// <param name="radius">The radius of the corners</param>
        /// <returns>A GraphicsPath with a rounded rectangle</returns>
        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();
            
            // Top left arc
            path.AddArc(arc, 180, 90);
            
            // Top right arc
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);
            
            // Bottom right arc
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            
            // Bottom left arc
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);
            
            path.CloseFigure();
            return path;
        }
    }
}
