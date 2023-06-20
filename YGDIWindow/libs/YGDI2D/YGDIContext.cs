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
            GraphicsContext = graphicsContext;
            updatePaths = new Dictionary<Color, GraphicsPath>();
        }

        public void Clear(Color backgroundColor)
        {
            using (var brush = new SolidBrush(backgroundColor))
            {
                GraphicsContext.FillRectangle(brush, 0, 0, GraphicsContext.VisibleClipBounds.Width, GraphicsContext.VisibleClipBounds.Height);
            }
        }

        private float GetScaleFactor()
        {
            float scaleX = (float)GraphicsContext.VisibleClipBounds.Width / WindowSize.Width;
            float scaleY = (float)GraphicsContext.VisibleClipBounds.Height / WindowSize.Height;
            return Math.Min(scaleX, scaleY);
        }

        public Size MeasureText(Font font, string text) => TextRenderer.MeasureText(text, font);

        public void DrawString(Font font, float fontSize, Color colour, string text, Point position)
        {
            float scaleFactor = GetScaleFactor();
            int scaledX = (int)(position.X * scaleFactor);
            int scaledY = (int)(position.Y * scaleFactor);

            float scaledSize = fontSize * scaleFactor;
            Font scaledFont = new Font(font.FontFamily, scaledSize, font.Style);

            TextRenderer.DrawText(GraphicsContext, text, scaledFont, new Point(scaledX, scaledY), colour);
        }

        public void FillRectangle(Color colour, Point position, Size size)
        {
            float scaleFactor = GetScaleFactor();
            int scaledX = (int)(position.X * scaleFactor);
            int scaledY = (int)(position.Y * scaleFactor);
            int scaledWidth = (int)(size.Width * scaleFactor);
            int scaledHeight = (int)(size.Height * scaleFactor);

            RectangleF rectBounds = new RectangleF(scaledX, scaledY, scaledWidth, scaledHeight);
            GraphicsPath rectPath = new GraphicsPath();
            rectPath.AddRectangle(rectBounds);

            UpdatePath(colour, rectPath);
        }

        public void UpdatePath(Color colour, GraphicsPath path)
        {
            if (updatePaths.ContainsKey(colour))
            {
                updatePaths[colour].AddPath(path, false);
            }
            else
            {
                updatePaths[colour] = path.Clone() as GraphicsPath;
            }
        }

        public void Render()
        {
            foreach (var kvp in updatePaths)
            {
                Color colour = kvp.Key;
                GraphicsPath path = kvp.Value;

                using (var brush = new SolidBrush(colour))
                {
                    GraphicsContext.FillPath(brush, path);
                }

                path.Reset();
            }

            updatePaths.Clear();
        }
    }
}
