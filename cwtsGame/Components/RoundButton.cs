using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace cwtsGame
{
    public class RoundButton : Button
    {
        private int radius = 20;
        private Color borderColor = Color.White;
        private float borderWidth = 1.5f;
        private Color hoverBackColor = Color.Empty;
        private bool hovering;
        private float pixelFontSize;

        public RoundButton()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.SupportsTransparentBackColor, true);

            DoubleBuffered = true;
        }

        public int Radius
        {
            get { return radius; }
            set { radius = value; Invalidate(); }
        }

        public Color BorderColor
        {
            get { return borderColor; }
            set { borderColor = value; Invalidate(); }
        }

        public float BorderWidth
        {
            get { return borderWidth; }
            set { borderWidth = value; Invalidate(); }
        }

        public Color HoverBackColor
        {
            get { return hoverBackColor; }
            set { hoverBackColor = value; Invalidate(); }
        }

        /// <summary>
        /// Pixel size for the bundled Silkscreen font. Zero keeps the normal Font property.
        /// </summary>
        public float PixelFontSize
        {
            get { return pixelFontSize; }
            set
            {
                pixelFontSize = Math.Max(0f, value);
                Invalidate();
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            hovering = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            hovering = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            Invalidate();
            base.OnEnabledChanged(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            int r = Math.Max(1, Math.Min(radius, Math.Min(Width, Height) / 2));
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(0, 0, r, r, 180, 90);
                path.AddArc(Width - r, 0, r, r, 270, 90);
                path.AddArc(Width - r, Height - r, r, r, 0, 90);
                path.AddArc(0, Height - r, r, r, 90, 90);
                path.CloseFigure();

                Color fill = Enabled ? BackColor : Color.FromArgb(150, BackColor);
                if (hovering && Enabled && !hoverBackColor.IsEmpty)
                {
                    fill = hoverBackColor;
                }

                using (SolidBrush brush = new SolidBrush(fill))
                {
                    g.FillPath(brush, path);
                }

                if (borderWidth > 0f)
                {
                    using (Pen pen = new Pen(borderColor, borderWidth))
                    {
                        pen.Alignment = PenAlignment.Inset;
                        g.DrawPath(pen, path);
                    }
                }

                if (Image != null)
                {
                    int imageSide = Math.Max(8, Math.Min(Width, Height) - 20);
                    Rectangle imageRect = new Rectangle(
                        (Width - imageSide) / 2,
                        (Height - imageSide) / 2,
                        imageSide,
                        imageSide);

                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(Image, imageRect);
                }

                if (!string.IsNullOrEmpty(Text))
                {
                    Font textFont = pixelFontSize > 0f ? UiTheme.Pixel(pixelFontSize, g) : Font;
                    using (StringFormat sf = new StringFormat())
                    {
                        sf.Alignment = StringAlignment.Center;
                        sf.LineAlignment = StringAlignment.Center;

                        using (SolidBrush brush = new SolidBrush(ForeColor))
                        {
                            g.DrawString(Text, textFont, brush, new RectangleF(0, 0, Width, Height), sf);
                        }
                    }
                }

                Region oldRegion = Region;
                Region = new Region(path);
                if (oldRegion != null)
                {
                    oldRegion.Dispose();
                }
            }
        }
    }
}
