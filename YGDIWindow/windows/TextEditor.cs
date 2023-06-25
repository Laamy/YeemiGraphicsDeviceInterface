using System;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

using YGDIWindow_2D.YGDI2D;
using YGDIWindow_2D.YGDI2D.Events;

public class TextEditor
{
    private YGDIWindow window;
    private Font arial;
    private StringBuilder text;
    private static int scrollOffset;
    private static int cursorPosition = 0;

    public TextEditor()
    {
        // create new YGDI window
        window = new YGDIWindow();

        // setup YGDI transparency key cuz its a cool effect
        window.TransparencyKey = Color.Blue;

        // hook YGDI window events
        window.onUpdate += OnUpdate;
        window.KeyPress += OnKeyPress;
        window.MouseWheel += OnMouseWheel;

        // setup font & textbox
        arial = window.GetFont("Arial");
        SetText("abc\rabc\rabc");
        scrollOffset = 0;

        // start rendering
        window.StartRendering(30); // 24??
    }

    public void SetText(string text)
    {
        // create new string builder using text argument then update cursor position
        this.text = new StringBuilder(text);
        cursorPosition = text.Length;
    }

    private void OnKeyPress(object sender, KeyPressEventArgs e)
    {
        // check if backspace or not
        if (e.KeyChar == '\b' && text.Length > 0)
        {
            // remove last character & update cursor position
            // Note: I'll need to rewrite this once I make the cursor functional via arrow keys
            text.Length--;
            cursorPosition = Math.Max(0, cursorPosition - 1);
        }
        else
        {
            // insert character at cursor position
            text.Insert(cursorPosition, e.KeyChar);
            cursorPosition++;
        }
    }

    private void OnUpdate(object sender, YGDIUpdateEvent e)
    {
        // clear buffer
        e.Context.Clear(window.BackColor);

        // draw text
        e.Context.DrawString(arial, 32, Color.White, text.ToString(), new Point(10, scrollOffset));

        // draw cursor
        {
            // calculate cursor X & Y
            int cursorX = e.Context.MeasureText(arial, 32, text.ToString(0, cursorPosition)).Width + 10;
            int cursorY = 32 + scrollOffset + (int)(Regex.Matches(text.ToString(0, cursorPosition), "\r").Count * (32 + 18)) - 16;

            // draw cursor
            e.Context.FillRectangle(Color.White, new Point(cursorX, cursorY), new Size(6, 32));
        }

        // tell YGDI to draw everything
        e.Context.EndFrame();
    }

    private void OnMouseWheel(object sender, MouseEventArgs e)
    {
        // add & clamp scrollOffset with scrollwheel delta
        scrollOffset = Math.Min(0, scrollOffset + e.Delta);
    }
}