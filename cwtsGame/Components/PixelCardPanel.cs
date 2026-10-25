using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace cwtsGame
{
    /// <summary>
    /// Opaque cream dialog card with a pixel title, subtitle and colored
    /// accent. Content controls are added as children below the header.
    /// </summary>
    public class PixelCardPanel : Panel
    {
        private string title = string.Empty;
        private string subtitle = string.Empty;
        private Color accentColor = UiTheme.Leaf;
        private int headerHeight = 100;
        private int cornerRadius = 26;

        public PixelCardPanel()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            BackColor = UiTheme.Cream;
            DoubleBuffered = true;
        }

        public string Title
        {
            get { return title; }
            set { title = value ?? string.Empty; Invalidate(); }
        }

        public string Subtitle
        {
            get { return subtitle; }
            set { subtitle = value ?? string.Empty; Invalidate(); }
        }

        public Color AccentColor
        {
            get { return accentColor; }
            set { accentColor = value; Invalidate(); }
        }

        public int HeaderHeight
        {
            get { return headerHeight; }
            set
            {
                headerHeight = Math.Max(0, value);
                Invalidate();
            }
        }

        public int CornerRadius
        {
            get { return cornerRadius; }
            set
            {
                cornerRadius = Math.Max(8, value);
                UpdateRegion();
                Invalidate();
            }
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
                new RectangleF(0f, 0f, Width, Height), cornerRadius))
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

            if (Width < 40 || Height < 40)
            {
                return;
            }

            RectangleF card = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
            UiTheme.FillRounded(g, card, cornerRadius, UiTheme.Cream);
            UiTheme.DrawRounded(g, card, cornerRadius, UiTheme.Ink, 3f);

            if (headerHeight > 0 && !string.IsNullOrEmpty(title))
            {
                float barHeight = Math.Min(48f, headerHeight * 0.48f);
                RectangleF accent = new RectangleF(20f, 22f, 6f, barHeight);
                UiTheme.FillRounded(g, accent, 3f, accentColor);

                Font titleFont = UiTheme.Pixel(23f, g);
                RectangleF titleRect = new RectangleF(
                    38f,
                    16f,
                    Width - 98f,
                    Math.Max(30f, titleFont.GetHeight(g) + 4f));

                using (SolidBrush titleBrush = new SolidBrush(UiTheme.Ink))
                using (StringFormat titleFormat = new StringFormat())
                {
                    titleFormat.Alignment = StringAlignment.Near;
                    titleFormat.LineAlignment = StringAlignment.Center;
                    titleFormat.Trimming = StringTrimming.EllipsisCharacter;
                    titleFormat.FormatFlags = StringFormatFlags.NoWrap;
                    g.DrawString(title.ToUpperInvariant(), titleFont, titleBrush, titleRect, titleFormat);
                }

                if (!string.IsNullOrEmpty(subtitle))
                {
                    Font subtitleFont = UiTheme.Body(9.5f, g, FontStyle.Bold);
                    RectangleF subtitleRect = new RectangleF(
                        40f,
                        52f,
                        Width - 80f,
                        22f);

                    using (SolidBrush subtitleBrush = new SolidBrush(UiTheme.InkSoft))
                    using (StringFormat subtitleFormat = new StringFormat())
                    {
                        subtitleFormat.Alignment = StringAlignment.Near;
                        subtitleFormat.LineAlignment = StringAlignment.Center;
                        subtitleFormat.Trimming = StringTrimming.EllipsisCharacter;
                        subtitleFormat.FormatFlags = StringFormatFlags.NoWrap;
                        g.DrawString(subtitle.ToUpperInvariant(), subtitleFont, subtitleBrush,
                            subtitleRect, subtitleFormat);
                    }
                }

                RectangleF underline = new RectangleF(40f, 78f, 52f, 4f);
                UiTheme.FillRounded(g, underline, 2f, UiTheme.Sunshine);
            }

            if (headerHeight > 0)
            {
                using (Pen separator = new Pen(Color.FromArgb(80, UiTheme.InkSoft), 1f))
                {
                    separator.DashStyle = DashStyle.Dot;
                    g.DrawLine(separator, 22f, headerHeight, Width - 22f, headerHeight);
                }
            }
        }
    }
}
