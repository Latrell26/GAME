using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace cwtsGame
{
    /// <summary>
    /// Rounded pixel-font tag used for NPC names and trash labels. It replaces
    /// the old black rectangular labels with cream badges.
    /// </summary>
    public class PixelTag : Label
    {
        private float pixelFontSize = 9f;
        private Color fillColor = UiTheme.Cream;
        private Color borderColor = UiTheme.Ink;
        private Color accentColor = Color.Empty;
        private bool showAccent;
        private bool highlighted;

        public PixelTag()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = UiTheme.Cream;
            ForeColor = UiTheme.Ink;
            AutoSize = true;
            TabStop = false;
            Padding = new Padding(7, 3, 7, 3);
            UseMnemonic = false;
        }

        public float PixelFontSize
        {
            get { return pixelFontSize; }
            set
            {
                pixelFontSize = Math.Max(6f, value);
                PerformLayout();
                Invalidate();
            }
        }

        public Color FillColor
        {
            get { return fillColor; }
            set
            {
                fillColor = value;
                BackColor = value;
                Invalidate();
            }
        }

        public Color BorderColor
        {
            get { return borderColor; }
            set { borderColor = value; Invalidate(); }
        }

        public Color AccentColor
        {
            get { return accentColor; }
            set { accentColor = value; Invalidate(); }
        }

        public bool ShowAccent
        {
            get { return showAccent; }
            set
            {
                if (showAccent == value)
                {
                    return;
                }

                showAccent = value;
                Padding = value ? new Padding(12, 3, 7, 3) : new Padding(7, 3, 7, 3);
                PerformLayout();
                Invalidate();
            }
        }

        public bool Highlighted
        {
            get { return highlighted; }
            set
            {
                if (highlighted == value)
                {
                    return;
                }

                highlighted = value;
                Invalidate();
            }
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            using (Bitmap measureBitmap = new Bitmap(1, 1))
            using (Graphics measure = Graphics.FromImage(measureBitmap))
            {
                Font font = UiTheme.Pixel(pixelFontSize, measure);
                string text = string.IsNullOrEmpty(Text) ? " " : Text;
                SizeF size = measure.MeasureString(text, font);

                return new Size(
                    (int)Math.Ceiling(size.Width) + Padding.Horizontal + 4,
                    (int)Math.Ceiling(size.Height) + Padding.Vertical + 2);
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);

            if (AutoSize)
            {
                Size = GetPreferredSize(Size.Empty);
            }

            Invalidate();
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

            float radius = Math.Max(5f, Math.Min(9f, Height * 0.32f));
            using (GraphicsPath path = UiTheme.RoundedRect(
                new RectangleF(0f, 0f, Width, Height), radius))
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

            if (Width < 4 || Height < 4)
            {
                return;
            }

            RectangleF tag = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
            float radius = Math.Max(5f, Math.Min(9f, Height * 0.32f));
            Color fill = highlighted ? UiTheme.Sunshine : fillColor;

            UiTheme.FillRounded(g, tag, radius, fill);
            UiTheme.DrawRounded(g, tag, radius, borderColor, 2f);

            if (showAccent && !accentColor.IsEmpty)
            {
                float barHeight = Math.Max(4f, Height - 10f);
                RectangleF bar = new RectangleF(4f, (Height - barHeight) / 2f, 3f, barHeight);
                UiTheme.FillRounded(g, bar, 1.5f, accentColor);
            }

            if (string.IsNullOrEmpty(Text))
            {
                return;
            }

            Font font = UiTheme.Pixel(pixelFontSize, g);
            RectangleF textRect = new RectangleF(
                Padding.Left,
                0f,
                Math.Max(1f, Width - Padding.Horizontal),
                Height);

            using (SolidBrush brush = new SolidBrush(ForeColor))
            using (StringFormat format = new StringFormat())
            {
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;
                format.Trimming = StringTrimming.EllipsisWord;
                format.FormatFlags = StringFormatFlags.LineLimit;
                g.DrawString(Text, font, brush, textRect, format);
            }
        }
    }
}
