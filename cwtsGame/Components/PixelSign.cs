using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace cwtsGame
{
    /// <summary>
    /// Owner-drawn wooden sign used for the StageForm heading, and a compact
    /// cream banner variant used for the "more stages" note.
    /// </summary>
    public class PixelSign : Control
    {
        private string _title = string.Empty;
        private string _subtitle = string.Empty;
        private bool _footerStyle;

        public string Title
        {
            get { return _title; }
            set { _title = value ?? string.Empty; Invalidate(); }
        }

        public string Subtitle
        {
            get { return _subtitle; }
            set { _subtitle = value ?? string.Empty; Invalidate(); }
        }

        public bool FooterStyle
        {
            get { return _footerStyle; }
            set
            {
                if (_footerStyle == value)
                {
                    return;
                }

                _footerStyle = value;
                BackColor = value ? UiTheme.Cream : UiTheme.Wood;
                UpdateRegion();
                Invalidate();
            }
        }

        public PixelSign()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            BackColor = UiTheme.Wood;
            DoubleBuffered = true;
            TabStop = false;
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

            RectangleF board;
            float radius;
            if (_footerStyle)
            {
                radius = Math.Max(10f, Height * 0.36f);
                board = new RectangleF(5f, 3f, Width - 15f, Height - 11f);
            }
            else
            {
                radius = Math.Max(14f, Height * 0.16f);
                board = new RectangleF(8f, 5f, Width - 22f, Height - 18f);
            }

            if (board.Width < 4f || board.Height < 4f)
            {
                return;
            }

            using (GraphicsPath path = UiTheme.RoundedRect(board, radius))
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

            if (_footerStyle)
            {
                DrawFooter(g);
            }
            else
            {
                DrawHeader(g);
            }
        }

        private void DrawHeader(Graphics g)
        {
            float radius = Math.Max(14f, Height * 0.16f);
            RectangleF board = new RectangleF(8f, 5f, Width - 22f, Height - 18f);

            using (LinearGradientBrush wood = new LinearGradientBrush(
                board, UiTheme.WoodLight, UiTheme.WoodDark, LinearGradientMode.Vertical))
            using (GraphicsPath path = UiTheme.RoundedRect(board, radius))
            {
                g.FillPath(wood, path);
            }

            UiTheme.DrawRounded(g, board, radius, Color.FromArgb(63, 40, 23), 4f);

            // Inner bevel.
            RectangleF bevel = new RectangleF(board.X + 6f, board.Y + 6f, board.Width - 12f, board.Height - 12f);
            UiTheme.DrawRounded(g, bevel, Math.Max(8f, radius - 5f), Color.FromArgb(90, 255, 226, 170), 1.5f);

            // Little brass nails.
            using (SolidBrush nail = new SolidBrush(Color.FromArgb(255, 214, 138)))
            using (Pen nailEdge = new Pen(Color.FromArgb(120, 60, 36, 18), 1f))
            {
                float nailSize = Math.Max(5f, Height * 0.052f);
                float nailY = board.Y + nailSize * 0.8f;
                g.FillEllipse(nail, board.X + nailSize * 1.4f, nailY, nailSize, nailSize);
                g.DrawEllipse(nailEdge, board.X + nailSize * 1.4f, nailY, nailSize, nailSize);
                g.FillEllipse(nail, board.Right - nailSize * 2.4f, nailY, nailSize, nailSize);
                g.DrawEllipse(nailEdge, board.Right - nailSize * 2.4f, nailY, nailSize, nailSize);
            }

            // Title.
            bool hasSubtitle = !string.IsNullOrEmpty(_subtitle);
            float titleHeight = hasSubtitle ? board.Height * 0.52f : board.Height * 0.72f;
            float titleTop = board.Y + (hasSubtitle ? board.Height * 0.14f : board.Height * 0.14f);
            Font titleFont = UiTheme.Pixel(Math.Max(18f, Height * 0.255f), g);

            using (StringFormat titleFormat = new StringFormat())
            {
                titleFormat.Alignment = StringAlignment.Center;
                titleFormat.LineAlignment = StringAlignment.Center;
                titleFormat.Trimming = StringTrimming.EllipsisCharacter;
                titleFormat.FormatFlags = StringFormatFlags.NoWrap;

                RectangleF titleRect = new RectangleF(
                    board.X + 34f,
                    titleTop,
                    board.Width - 68f,
                    titleHeight);

                UiTheme.DrawTextWithShadow(g, _title, titleFont, UiTheme.Cream, titleRect, titleFormat);
            }

            // Sparkles on either side of the title.
            DrawSparkle(g, board.X + 26f, board.Y + board.Height * 0.34f, Math.Max(5f, Height * 0.055f));
            DrawSparkle(g, board.Right - 26f, board.Y + board.Height * 0.34f, Math.Max(5f, Height * 0.055f));

            if (hasSubtitle)
            {
                Font subtitleFont = UiTheme.Body(Math.Max(9f, Height * 0.115f), g, FontStyle.Regular);
                RectangleF subtitleRect = new RectangleF(
                    board.X + 34f,
                    board.Y + board.Height * 0.63f,
                    board.Width - 68f,
                    board.Height * 0.25f);

                using (StringFormat subtitleFormat = new StringFormat())
                {
                    subtitleFormat.Alignment = StringAlignment.Center;
                    subtitleFormat.LineAlignment = StringAlignment.Center;
                    subtitleFormat.Trimming = StringTrimming.EllipsisCharacter;
                    subtitleFormat.FormatFlags = StringFormatFlags.NoWrap;

                    using (SolidBrush subtitleBrush = new SolidBrush(Color.FromArgb(255, 231, 176)))
                    {
                        g.DrawString(_subtitle, subtitleFont, subtitleBrush, subtitleRect, subtitleFormat);
                    }
                }
            }
        }

        private void DrawFooter(Graphics g)
        {
            float radius = Math.Max(10f, Height * 0.36f);
            RectangleF board = new RectangleF(5f, 3f, Width - 15f, Height - 11f);

            UiTheme.FillRounded(g, board, radius, UiTheme.Cream);
            UiTheme.DrawRounded(g, board, radius, UiTheme.Ink, 2.5f);

            Font font = UiTheme.Pixel(Math.Max(8f, Height * 0.30f), g);
            RectangleF textRect = new RectangleF(
                board.X + 26f,
                board.Y,
                board.Width - 52f,
                board.Height);

            using (StringFormat format = new StringFormat())
            {
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;
                format.Trimming = StringTrimming.EllipsisCharacter;
                format.FormatFlags = StringFormatFlags.NoWrap;

                using (SolidBrush brush = new SolidBrush(UiTheme.Ink))
                {
                    g.DrawString(_title, font, brush, textRect, format);
                }
            }

            DrawSparkle(g, board.X + 14f, board.Y + board.Height / 2f, Math.Max(3.5f, Height * 0.10f));
            DrawSparkle(g, board.Right - 14f, board.Y + board.Height / 2f, Math.Max(3.5f, Height * 0.10f));
        }

        private static void DrawSparkle(Graphics g, float cx, float cy, float size)
        {
            float half = size / 2f;
            float thin = Math.Max(1.5f, size * 0.20f);
            PointF[] points =
            {
                new PointF(cx, cy - half),
                new PointF(cx + thin, cy - thin),
                new PointF(cx + half, cy),
                new PointF(cx + thin, cy + thin),
                new PointF(cx, cy + half),
                new PointF(cx - thin, cy + thin),
                new PointF(cx - half, cy),
                new PointF(cx - thin, cy - thin)
            };

            using (SolidBrush brush = new SolidBrush(UiTheme.Sunshine))
            {
                g.FillPolygon(brush, points);
            }
        }
    }
}
