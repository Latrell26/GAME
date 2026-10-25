using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace cwtsGame
{
    /// <summary>
    /// Small cream notification pill used for pickups, correct sorts and
    /// other one-line feedback.
    /// </summary>
    public class GameToast : Control
    {
        private string message = string.Empty;
        private Color accentColor = UiTheme.Leaf;

        public GameToast()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            BackColor = UiTheme.Cream;
            DoubleBuffered = true;
            TabStop = false;
            Visible = false;
            AccessibleRole = AccessibleRole.StaticText;
            AccessibleName = "Game feedback";
        }

        public void ShowMessage(string text, Color accent)
        {
            message = text ?? string.Empty;
            accentColor = accent;
            Visible = message.Length > 0;
            AccessibleDescription = message;
            Invalidate();
        }

        public void HideMessage()
        {
            message = string.Empty;
            Visible = false;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateRegion();
        }

        private void UpdateRegion()
        {
            if (Width <= 0 || Height <= 0)
            {
                return;
            }

            using (GraphicsPath path = UiTheme.RoundedRect(
                new RectangleF(0f, 0f, Width, Height), Height / 2f))
            {
                Region oldRegion = Region;
                Region = new Region(path);
                if (oldRegion != null)
                {
                    oldRegion.Dispose();
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            if (Width < 40 || Height < 20)
            {
                return;
            }

            RectangleF card = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
            UiTheme.FillRounded(g, card, Height / 2f, UiTheme.Cream);
            UiTheme.DrawRounded(g, card, Height / 2f, UiTheme.Ink, 2.5f);

            float barHeight = Math.Max(8f, Height - 16f);
            RectangleF accent = new RectangleF(11f, (Height - barHeight) / 2f, 6f, barHeight);
            UiTheme.FillRounded(g, accent, 3f, accentColor);

            Font font = UiTheme.Body(10.5f, g, FontStyle.Bold);
            RectangleF textRect = new RectangleF(28f, 0f, Width - 44f, Height);
            using (SolidBrush brush = new SolidBrush(UiTheme.Ink))
            using (StringFormat format = new StringFormat())
            {
                format.Alignment = StringAlignment.Near;
                format.LineAlignment = StringAlignment.Center;
                format.Trimming = StringTrimming.EllipsisCharacter;
                format.FormatFlags = StringFormatFlags.NoWrap;
                g.DrawString(message, font, brush, textRect, format);
            }
        }
    }
}
