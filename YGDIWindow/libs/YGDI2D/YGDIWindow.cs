using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

using YGDIWindow_2D.YGDI2D.Events;

namespace YGDIWindow_2D.YGDI2D
{
    public class YGDIWindow : Form
    {
        private bool canRender = false;
        private bool isRendering;
        private Thread renderThread;
        private int frameRate = 30;
        private int frameDelay;

        private Stopwatch stopwatch;

        private double deltaTime = 0;
        public double AssumeFramerate = 0;

        public Font GetFont(string fontName) => new Font(fontName, 16);

        public YGDIWindow(string title = "YGDI Window", int width = 640, int height = 360)
        {
            this.Text = title;
            this.ClientSize = new Size(width, height);

            this.BackColor = Color.FromArgb(0x12, 0x12, 0x12);

            this.Shown += YGDIWindow_Shown;

            // release events
            this.FormClosing += YGDIWindow_Closing;

            this.DoubleBuffered = true;

            stopwatch = new Stopwatch();
        }

        private void YGDIWindow_Closing(object sender, FormClosingEventArgs e)
        {
            this.StopRendering();
            stopwatch.Reset();

            //Application.Exit();
        }

        public EventHandler<YGDIUpdateEvent> onUpdate;

        private void YGDIWindow_Shown(object sender, EventArgs e)
        {
            isRendering = true;
            renderThread = new Thread(RenderLoop);
            renderThread.Start();
        }

        public void StartRendering(int targetFrameRate)
        {
            // avoid this mistake
            if (targetFrameRate >= 1000)
            {
                throw new Exception("If you want unlimited framerates then use -1");
            }

            frameRate = targetFrameRate;

            if (targetFrameRate != -1)
            {
                frameDelay = (int)Math.Round(1000.0 / frameRate);
            }
            else
            {
                frameDelay = -1;
            }

            Application.Run(this);
        }

        public void StopRendering()
        {
            isRendering = false;
            renderThread.Join();
        }

        private void RenderLoop()
        {
            stopwatch.Start();

            int frameCount = 0;
            double elapsedTime = 0.0;
            double fps = 0.0;

            int updateInterval = 250;

            Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;

            while (isRendering)
            {
                deltaTime = (double)stopwatch.ElapsedMilliseconds / 1000.0;
                elapsedTime += stopwatch.Elapsed.TotalMilliseconds;

                stopwatch.Restart();

                this.BeginInvoke(new Action(() =>{
                    Bitmap backBuffer = new Bitmap(this.Width, this.Height);
                    Graphics backBufferGraphics = Graphics.FromImage(backBuffer);

                    Graphics graphics = CreateGraphics();

                    YGDIContext context = new YGDIContext(backBufferGraphics);

                    if (onUpdate != null)
                    {
                        onUpdate.Invoke(this, new YGDIUpdateEvent(context, deltaTime));
                    }

                    context.Render();

                    graphics.DrawImage(backBuffer, 0, 0);
                }));

                frameCount++;

                if (elapsedTime >= updateInterval)
                {
                    fps = frameCount * (1000 / updateInterval);
                    frameCount = 0;
                    elapsedTime = 0.0;

                    AssumeFramerate = fps;
                }

                if (frameDelay == -1)
                {
                    Thread.Sleep(1);

                    continue;
                }

                int sleepTime = frameDelay - (int)Math.Round(stopwatch.Elapsed.TotalMilliseconds);
                if (sleepTime > 0)
                {
                    Console.WriteLine(sleepTime);
                    Thread.Sleep(sleepTime);
                }
            }

            stopwatch.Stop();
            stopwatch.Reset();
        }
    }
}
