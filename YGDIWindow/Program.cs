using System.Drawing;

using YGDIWindow_2D.YGDI2D;
using YGDIWindow_2D.YGDI2D.Events;

namespace YGDIWindow_2D
{
    internal class Program
    {
        private static YGDIWindow window;

        static void Main(string[] args)
        {
            window = new YGDIWindow();

            window.TransparencyKey = window.BackColor;

            window.onUpdate += OnUpdate;

            arial = window.GetFont("Arial");

            window.StartRendering(-1); // 24??
        }

        private static Font arial;

        private static void OnUpdate(object sender, YGDIUpdateEvent e)
        {
            e.Context.Clear(window.TransparencyKey);
            
            for (int x = 16; x < 32; x++)
            {
                for (int y =  16; y < 32; y++)
                {
                    e.Context.FillRectangle(Color.Blue, new Point(x * 27, y * 27), new Size(25, 25));
                }
            }

            e.Context.DrawString(arial, 48, Color.Red, "FPS: " + window.AssumeFramerate, new Point(25, 25));
        }
    }
}
