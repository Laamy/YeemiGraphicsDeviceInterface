using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace YGDIWindow_2D.YGDI2D
{
    public class YGDIContext
    {
        public Graphics GraphicsContext { get; private set; }
        public Size WindowSize = new Size(1920, 1080); // scaled resolution
        private Dictionary<Color, GraphicsPath> updatePaths;

        public YGDIContext(Graphics graphicsContext)
        {
            // check for valid graphics context
            if (graphicsContext == null)
                throw new Exception("YGDI Context tried to be initialized with an invalid GDI surface");

            // tell ygdi context what graphics context to use and create new drawing paths
            GraphicsContext = graphicsContext;
            updatePaths = new Dictionary<Color, GraphicsPath>();
        }

        public void Clear(Color backgroundColor)
        {
            // create new solid brush based on backgroundColor argument then draws a rectangle (no paths used so its a sperate call)
            using (var brush = new SolidBrush(backgroundColor))
            {
                // fill in the bitmap background with new information (used to clear last frame information, or use to)
                GraphicsContext.FillRectangle(brush, 0, 0, GraphicsContext.VisibleClipBounds.Width, GraphicsContext.VisibleClipBounds.Height);
            }
        }

        private float GetScaleFactor()
        {
            // get scaling factor based on width || height
            float scaleX = (float)GraphicsContext.VisibleClipBounds.Width / WindowSize.Width;
            float scaleY = (float)GraphicsContext.VisibleClipBounds.Height / WindowSize.Height;
            return Math.Min(scaleX, scaleY);
        }

        public Size MeasureText(Font font, int fontsize, string text)
        {
            // measure text using TextRenderer class
            return TextRenderer.MeasureText(text, new Font(font.FontFamily, fontsize));
        }

        public void DrawString(Font font, float fontSize, Color colour, string text, Point position)
        {
            // calculate scaling based on scaling factor
            float scaleFactor = GetScaleFactor();
            int scaledX = (int)(position.X * scaleFactor);
            int scaledY = (int)(position.Y * scaleFactor);

            // calculate font based on scaling factor
            float scaledSize = fontSize * scaleFactor;
            Font scaledFont = new Font(font.FontFamily, scaledSize, font.Style);

            // draw text using TextRenderer class
            TextRenderer.DrawText(GraphicsContext, text, scaledFont, new Point(scaledX, scaledY), colour);
        }

        public void FillRectangle(Color colour, Point position, Size size)
        {
            // calculate scaling based on scaling factor
            float scaleFactor = GetScaleFactor();
            int scaledX = (int)(position.X * scaleFactor);
            int scaledY = (int)(position.Y * scaleFactor);
            int scaledWidth = (int)(size.Width * scaleFactor);
            int scaledHeight = (int)(size.Height * scaleFactor);

            // create bounds & add bounds to new graphics path)
            RectangleF rectBounds = new RectangleF(scaledX, scaledY, scaledWidth, scaledHeight);
            GraphicsPath rectPath = new GraphicsPath();
            rectPath.AddRectangle(rectBounds);

            // add bounds for path
            UpdatePath(colour, rectPath);
        }

        public void UpdatePath(Color colour, GraphicsPath path)
        {
            // check if path exists
            if (updatePaths.ContainsKey(colour))
            {
                // add graphics path bounds to the rest of the graphics path for its colour
                updatePaths[colour].AddPath(path, false);
            }
            else
            {
                // clone graphics path and set it into the graphics path list
                updatePaths[colour] = path.Clone() as GraphicsPath;
            }
        }

        public void EndFrame()
        {
            // loop over all the graphics paths and render them in bulk
            foreach (var kvp in updatePaths)
            {
                // get key & value
                Color colour = kvp.Key;
                GraphicsPath path = kvp.Value;

                // create new solid brush based on key then fill all the paths in the graphics path using the new solid brush
                using (var brush = new SolidBrush(colour))
                {
                    GraphicsContext.FillPath(brush, path);
                }

                // reset path
                path.Reset();
            }

            // clear graphics path list for reuse
            updatePaths.Clear();
        }
    }
}
