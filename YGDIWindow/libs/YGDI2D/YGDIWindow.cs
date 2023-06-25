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
        private bool isRendering;
        private Thread renderThread;
        private int frameRate = 30;
        private int frameDelay;

        private const int UpdateInterval = 1000;

        private Stopwatch stopwatch;

        private double deltaTime = 0;

        public double AssumeFramerate = 0;

        public EventHandler<string> onLog;
        public EventHandler<object> onRenderStop;
        public EventHandler<YGDIUpdateEvent> onUpdate;

        public Font GetFont(string fontName) => new Font(fontName, 16);

        public YGDIWindow(string title = "YGDI Window", int width = 640, int height = 360)
        {
            // set window title & size based on arguments
            this.Text = title;
            this.ClientSize = new Size(width, height);

            // set default backcolour
            this.BackColor = Color.FromArgb(0x12, 0x12, 0x12);

            // window events
            this.Shown += YGDIWindow_Shown;
            this.FormClosing += YGDIWindow_Closing;

            // enable double buffering (not that this matters cuz we do our own double buffering)
            this.DoubleBuffered = true;

            // create new stopwatch
            stopwatch = new Stopwatch();
        }

        private void YGDIWindow_Closing(object sender, FormClosingEventArgs e)
        {
            // stop rendering & reset stopwatch when closing
            this.StopRendering();
            stopwatch.Reset();
        }

        private void YGDIWindow_Shown(object sender, EventArgs e)
        {
            // set rendering & start rendering thread when window shows
            isRendering = true;
            renderThread = new Thread(RenderLoop);
            renderThread.Start();
        }

        public void StartRendering(int targetFrameRate)
        {
            // avoid this mistake
            if (targetFrameRate >= 1000)
                throw new Exception("If you want unlimited framerates then use -1");

            // tell YGDI window what framerate to use
            frameRate = targetFrameRate;

            // frame delay calculation stuff for framerate
            if (targetFrameRate != -1)
                frameDelay = (int)Math.Round(1000.0 / frameRate);
            else frameDelay = -1;

            // display window
            Application.Run(this);
        }

        public void StopRendering()
        {
            // stop rendering stuff
            isRendering = false;
            renderThread.Join();
        }

        private void RenderLoop()
        {
            // start stopwatch
            stopwatch.Start();

            // setup global render variables
            int frameCount = 0;
            double elapsedTime = 0.0;
            double fps = 0.0;

            // set thread priority
            Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;

            // start render loop
            while (isRendering)
            {
                // store delta & elapsed time
                deltaTime = (double)stopwatch.ElapsedMilliseconds / 1000.0;
                elapsedTime += stopwatch.Elapsed.TotalMilliseconds;

                // restart stopwatch
                stopwatch.Restart();

                // start render event
                this.BeginInvoke(new Action(() =>{
                    try
                    {
                        // create backbuffer
                        Bitmap backBuffer = new Bitmap(this.Width, this.Height);
                        Graphics backBufferGraphics = Graphics.FromImage(backBuffer);

                        // create form graphics
                        Graphics graphics = CreateGraphics();

                        // create YGDI context
                        YGDIContext context = new YGDIContext(backBufferGraphics);

                        if (onUpdate != null)
                        {
                            // invoke YGDI update/render event
                            onUpdate.Invoke(this, new YGDIUpdateEvent(context, deltaTime));
                        }

                        // draw backbuffer onto form
                        graphics.DrawImage(backBuffer, 0, 0);

                        // release GDI resources
                        backBufferGraphics.Dispose();
                        backBuffer.Dispose();
                    }
                    catch (Exception ex)
                    {
                        // log YGDI error (if any)
                        if (onLog != null)
                            onLog.Invoke(this, $"YGDI failure: {ex.Message}");
                    }
                }));

                // increase framecount for later framerate calculation
                frameCount++;

                // update framerate 4 times a second
                if (elapsedTime >= UpdateInterval)
                {
                    fps = frameCount * (1000 / UpdateInterval);
                    frameCount = 0;
                    elapsedTime = 0.0;

                    AssumeFramerate = fps;
                }

                // unlimited framerate delay else window freezes
                if (frameDelay == -1)
                {
                    Thread.Sleep(1);

                    continue;
                }

                // framerate delay stuff based on deltatime (TMS)
                int sleepTime = frameDelay - (int)Math.Round(stopwatch.Elapsed.TotalMilliseconds);
                if (sleepTime > 0)
                {
                    Thread.Sleep(sleepTime);
                }
            }

            // stop & reset the stopwatch
            stopwatch.Stop();
            stopwatch.Reset();

            // invoke rendering stop event
            if (onRenderStop != null)
                onRenderStop.Invoke(this, stopwatch);
        }
    }
}
