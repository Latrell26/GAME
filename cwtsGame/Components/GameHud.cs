using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace cwtsGame
{
    /// <summary>
    /// Floating cream status strip used during play. It keeps the score,
    /// countdown, sorting goal and carried tool visible without the old
    /// black label blocks.
    /// </summary>
    public class GameHud : Control
    {
        private int score;
        private int level;
        private int timeRemaining;
        private int sorted;
        private int goal = 1;
        private string toolName = "HAND";
        private int carry;
        private int maxSlots = 1;
        private string signature = string.Empty;

        public GameHud()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            BackColor = UiTheme.Cream;
            DoubleBuffered = true;
            TabStop = false;
            AccessibleRole = AccessibleRole.StatusBar;
            AccessibleName = "Game status";
        }

        public void SetData(int scoreValue, int levelValue, int timeValue, int sortedValue,
            int goalValue, string currentTool, int carryValue, int maxSlotValue)
        {
            string newSignature = scoreValue + "|" + levelValue + "|" + timeValue + "|" +
                sortedValue + "|" + goalValue + "|" + (currentTool ?? string.Empty) + "|" +
                carryValue + "|" + maxSlotValue;

            if (newSignature == signature)
            {
                return;
            }

            signature = newSignature;
            score = Math.Max(0, scoreValue);
            level = Math.Max(1, levelValue);
            timeRemaining = Math.Max(0, timeValue);
            sorted = Math.Max(0, sortedValue);
            goal = Math.Max(1, goalValue);
            toolName = string.IsNullOrEmpty(currentTool) ? "HAND" : currentTool.ToUpperInvariant();
            carry = Math.Max(0, carryValue);
            maxSlots = Math.Max(1, maxSlotValue);

            AccessibleDescription = "Score " + score + ", level " + level + ", " +
                timeRemaining + " seconds left, " + sorted + " of " + goal + " sorted";

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            if (Width < 80 || Height < 40)
            {
                return;
            }

            RectangleF card = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
            float radius = Math.Max(16f, Math.Min(24f, Height * 0.28f));

            UiTheme.FillRounded(g, card, radius, UiTheme.Cream);
            UiTheme.DrawRounded(g, card, radius, UiTheme.Ink, 3f);

            float pad = Math.Max(16f, Width * 0.020f);
            float usable = Width - pad * 2f;
            float[] weights = { 0.22f, 0.22f, 0.34f, 0.22f };
            float x = pad;

            for (int i = 0; i < weights.Length; i++)
            {
                float sectionWidth = usable * weights[i];
                RectangleF section = new RectangleF(x, 0f, sectionWidth, Height);

                if (i == 0)
                {
                    DrawScore(g, section);
                }
                else if (i == 1)
                {
                    DrawTime(g, section);
                }
                else if (i == 2)
                {
                    DrawSorted(g, section);
                }
                else
                {
                    DrawTool(g, section);
                }

                if (i < weights.Length - 1)
                {
                    float dividerX = x + sectionWidth - 1f;
                    using (Pen divider = new Pen(Color.FromArgb(90, UiTheme.InkSoft), 1f))
                    {
                        divider.DashStyle = DashStyle.Dot;
                        g.DrawLine(divider, dividerX, 16f, dividerX, Height - 16f);
                    }
                }

                x += sectionWidth;
            }
        }

        private void DrawScore(Graphics g, RectangleF section)
        {
            float labelY = 14f;
            DrawLabel(g, "SCORE", section.X, labelY);
            DrawSparkle(g, section.Right - 20f, labelY + 5f, 10f);

            Font valueFont = UiTheme.Pixel(22f, g);
            RectangleF valueRect = new RectangleF(section.X, labelY + 17f, section.Width - 16f, 34f);
            using (SolidBrush brush = new SolidBrush(UiTheme.Ink))
            using (StringFormat format = new StringFormat())
            {
                format.Alignment = StringAlignment.Near;
                format.LineAlignment = StringAlignment.Center;
                format.Trimming = StringTrimming.EllipsisCharacter;
                format.FormatFlags = StringFormatFlags.NoWrap;
                g.DrawString(score.ToString(), valueFont, brush, valueRect, format);
            }
        }

        private void DrawTime(Graphics g, RectangleF section)
        {
            float labelY = 14f;
            DrawLabel(g, "TIME", section.X, labelY);
            DrawClock(g, section.Right - 21f, labelY + 2f, 13f);

            Font valueFont = UiTheme.Pixel(22f, g);
            Color valueColor = timeRemaining <= 10 ? UiTheme.Coral : UiTheme.Ink;
            string timeText = (timeRemaining / 60) + ":" + (timeRemaining % 60).ToString("00");

            RectangleF valueRect = new RectangleF(section.X, labelY + 17f, section.Width - 16f, 34f);
            using (SolidBrush brush = new SolidBrush(valueColor))
            using (StringFormat format = new StringFormat())
            {
                format.Alignment = StringAlignment.Near;
                format.LineAlignment = StringAlignment.Center;
                format.Trimming = StringTrimming.EllipsisCharacter;
                format.FormatFlags = StringFormatFlags.NoWrap;
                g.DrawString(timeText, valueFont, brush, valueRect, format);
            }
        }

        private void DrawSorted(Graphics g, RectangleF section)
        {
            float labelY = 14f;
            DrawLabel(g, "SORTED", section.X, labelY);

            Font valueFont = UiTheme.Pixel(22f, g);
            Color valueColor = sorted >= goal ? UiTheme.Leaf : UiTheme.Ink;
            string sortedText = sorted + "/" + goal;

            RectangleF valueRect = new RectangleF(section.X, labelY + 17f, section.Width - 16f, 30f);
            using (SolidBrush brush = new SolidBrush(valueColor))
            using (StringFormat format = new StringFormat())
            {
                format.Alignment = StringAlignment.Near;
                format.LineAlignment = StringAlignment.Center;
                format.Trimming = StringTrimming.EllipsisCharacter;
                format.FormatFlags = StringFormatFlags.NoWrap;
                g.DrawString(sortedText, valueFont, brush, valueRect, format);
            }

            RectangleF bar = new RectangleF(
                section.X,
                Height - 22f,
                Math.Max(30f, section.Width - 22f),
                7f);

            UiTheme.FillRounded(g, bar, bar.Height / 2f, UiTheme.CreamShade);
            UiTheme.DrawRounded(g, bar, bar.Height / 2f, Color.FromArgb(90, UiTheme.InkSoft), 1f);

            float progress = Math.Max(0f, Math.Min(1f, sorted / (float)goal));
            if (progress > 0.02f)
            {
                RectangleF fill = new RectangleF(bar.X, bar.Y, Math.Max(6f, bar.Width * progress), bar.Height);
                UiTheme.FillRounded(g, fill, fill.Height / 2f,
                    sorted >= goal ? UiTheme.Leaf : UiTheme.Sunshine);
            }
        }

        private void DrawTool(Graphics g, RectangleF section)
        {
            float labelY = 14f;
            DrawLabel(g, "TOOL", section.X, labelY);
            DrawBag(g, section.Right - 22f, labelY + 2f, 13f);

            Font valueFont = UiTheme.Pixel(18f, g);
            RectangleF valueRect = new RectangleF(section.X, labelY + 16f, section.Width - 16f, 26f);
            using (SolidBrush brush = new SolidBrush(UiTheme.Leaf))
            using (StringFormat format = new StringFormat())
            {
                format.Alignment = StringAlignment.Near;
                format.LineAlignment = StringAlignment.Center;
                format.Trimming = StringTrimming.EllipsisCharacter;
                format.FormatFlags = StringFormatFlags.NoWrap;
                g.DrawString(toolName, valueFont, brush, valueRect, format);
            }

            Font carryFont = UiTheme.Body(8.5f, g, FontStyle.Bold);
            RectangleF carryRect = new RectangleF(section.X, labelY + 40f, section.Width - 8f, 20f);
            using (SolidBrush brush = new SolidBrush(UiTheme.InkSoft))
            using (StringFormat format = new StringFormat())
            {
                format.Alignment = StringAlignment.Near;
                format.LineAlignment = StringAlignment.Center;
                format.Trimming = StringTrimming.EllipsisCharacter;
                format.FormatFlags = StringFormatFlags.NoWrap;
                g.DrawString("CARRY " + carry + "/" + maxSlots, carryFont, brush, carryRect, format);
            }
        }

        private static void DrawLabel(Graphics g, string text, float x, float y)
        {
            Font font = UiTheme.Pixel(9f, g);
            using (SolidBrush brush = new SolidBrush(UiTheme.InkSoft))
            {
                g.DrawString(text, font, brush, x, y);
            }
        }

        private static void DrawSparkle(Graphics g, float cx, float cy, float size)
        {
            float half = size / 2f;
            float thin = Math.Max(1.5f, size * 0.18f);
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

        private static void DrawClock(Graphics g, float cx, float cy, float size)
        {
            float r = size / 2f;
            using (SolidBrush face = new SolidBrush(UiTheme.CreamShade))
            using (Pen edge = new Pen(UiTheme.Ink, 1.6f))
            {
                g.FillEllipse(face, cx - r, cy - r, size, size);
                g.DrawEllipse(edge, cx - r, cy - r, size, size);
                g.DrawLine(edge, cx, cy, cx, cy - r * 0.58f);
                g.DrawLine(edge, cx, cy, cx + r * 0.48f, cy + r * 0.20f);
            }
        }

        private static void DrawBag(Graphics g, float x, float y, float size)
        {
            using (SolidBrush fill = new SolidBrush(UiTheme.Leaf))
            using (Pen edge = new Pen(UiTheme.Ink, 1.5f))
            {
                RectangleF body = new RectangleF(x, y + size * 0.30f, size, size * 0.70f);
                using (GraphicsPath path = UiTheme.RoundedRect(body, 3f))
                {
                    g.FillPath(fill, path);
                    g.DrawPath(edge, path);
                }

                using (GraphicsPath handle = new GraphicsPath())
                {
                    handle.AddArc(x + size * 0.25f, y, size * 0.50f, size * 0.55f, 180f, 180f);
                    g.DrawPath(edge, handle);
                }
            }
        }
    }
}
