using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace cwtsGame
{
    /// <summary>
    /// A single, fully owner-drawn stage card: preview art, stage badge,
    /// difficulty, title, description, stat chips and a chunky START button.
    /// The whole card is clickable and keyboard accessible.
    /// </summary>
    public class StageCard : Control
    {
        private bool _hovered;
        private bool _pressed;

        public int StageNumber { get; private set; }
        public string StageTitle { get; private set; }
        public string Description { get; private set; }
        public string Difficulty { get; private set; }
        public string[] Chips { get; private set; }
        public Color AccentColor { get; private set; }
        public Image PreviewImage { get; private set; }

        public event EventHandler PlayClicked;

        public StageCard()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.Selectable, true);

            BackColor = UiTheme.Cream;
            Cursor = Cursors.Hand;
            TabStop = true;
            AccessibleRole = AccessibleRole.PushButton;
            DoubleBuffered = true;

            StageNumber = 1;
            StageTitle = "STAGE";
            Description = string.Empty;
            Difficulty = "EASY";
            Chips = new string[0];
            AccentColor = UiTheme.Leaf;
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

            float radius = Math.Max(14f, Height * 0.045f);
            using (GraphicsPath path = UiTheme.RoundedRect(new RectangleF(0f, 0f, Width, Height), radius))
            {
                Region oldRegion = Region;
                Region = new Region(path);
                if (oldRegion != null)
                {
                    oldRegion.Dispose();
                }
            }
        }

        public void SetStage(int number, string title, string description, string difficulty,
            string[] chips, Color accentColor, Image preview)
        {
            StageNumber = number;
            StageTitle = title;
            Description = description;
            Difficulty = difficulty;
            Chips = chips ?? new string[0];
            AccentColor = accentColor;
            PreviewImage = preview;
            AccessibleName = "Stage " + number + ": " + title;
            AccessibleDescription = description;
            Invalidate();
        }
        public void SetPreview(Image preview)
        {
            PreviewImage = preview;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            float radius = Math.Max(14f, Height * 0.045f);

            RectangleF card = new RectangleF(0f, 0f, Width, Height);
            if (card.Width < 40f || card.Height < 40f)
            {
                return;
            }

            UiTheme.FillRounded(g, card, radius, UiTheme.Cream);

            float inner = Math.Max(10f, card.Width * 0.042f);
            float previewHeight = card.Height * 0.42f;
            RectangleF preview = new RectangleF(
                card.X + inner,
                card.Y + inner,
                card.Width - inner * 2f,
                previewHeight);

            DrawPreview(g, preview, inner);

            // Badges sit over the corners of the preview art.
            float badgeFontSize = Math.Max(9f, Height * 0.019f);
            Font badgeFont = UiTheme.Pixel(badgeFontSize, g);
            string stageText = "STAGE " + StageNumber;
            float stageWidth = PillWidth(g, stageText, badgeFont);
            float badgeHeight = Math.Max(22f, Height * 0.041f);
            RectangleF stagePill = new RectangleF(preview.X + 10f, preview.Y + 10f, stageWidth, badgeHeight);
            DrawPill(g, stageText, badgeFont, stagePill, UiTheme.Sunshine, UiTheme.Ink);

            Font difficultyFont = UiTheme.Body(Math.Max(9f, Height * 0.020f), g, FontStyle.Bold);
            float difficultyWidth = PillWidth(g, Difficulty.ToUpperInvariant(), difficultyFont);
            RectangleF difficultyPill = new RectangleF(
                preview.Right - difficultyWidth - 10f,
                preview.Y + 10f,
                difficultyWidth,
                badgeHeight);
            DrawPill(g, Difficulty.ToUpperInvariant(), difficultyFont, difficultyPill, AccentColor, Color.White);

            // Title + little sunshine underline.
            Font titleFont = UiTheme.Pixel(Math.Max(17f, Height * 0.050f), g);
            float titleTop = preview.Bottom + inner * 0.55f;
            RectangleF titleRect = new RectangleF(
                card.X + inner,
                titleTop,
                card.Width - inner * 2f,
                titleFont.GetHeight(g) + 2f);

            using (StringFormat titleFormat = new StringFormat())
            {
                titleFormat.Alignment = StringAlignment.Near;
                titleFormat.LineAlignment = StringAlignment.Near;
                titleFormat.Trimming = StringTrimming.EllipsisCharacter;
                titleFormat.FormatFlags = StringFormatFlags.NoWrap;

                using (SolidBrush titleBrush = new SolidBrush(UiTheme.Ink))
                {
                    g.DrawString(StageTitle, titleFont, titleBrush, titleRect, titleFormat);
                }
            }

            RectangleF underline = new RectangleF(
                titleRect.X,
                titleRect.Bottom + 3f,
                Math.Max(34f, card.Width * 0.13f),
                Math.Max(3f, Height * 0.007f));
            UiTheme.FillRounded(g, underline, underline.Height / 2f, UiTheme.Sunshine);

            // Description (two to three short lines).
            Font descriptionFont = UiTheme.Body(Math.Max(10f, Height * 0.026f), g, FontStyle.Regular);
            float descriptionTop = underline.Bottom + Math.Max(8f, Height * 0.018f);
            float descriptionHeight = descriptionFont.GetHeight(g) * 2.6f;
            RectangleF descriptionRect = new RectangleF(
                card.X + inner,
                descriptionTop,
                card.Width - inner * 2f,
                descriptionHeight);

            using (StringFormat descriptionFormat = new StringFormat())
            {
                descriptionFormat.Alignment = StringAlignment.Near;
                descriptionFormat.LineAlignment = StringAlignment.Near;
                descriptionFormat.Trimming = StringTrimming.EllipsisWord;
                descriptionFormat.FormatFlags = StringFormatFlags.LineLimit;

                using (SolidBrush descriptionBrush = new SolidBrush(UiTheme.InkSoft))
                {
                    g.DrawString(Description, descriptionFont, descriptionBrush, descriptionRect, descriptionFormat);
                }
            }

            // Stat chips.
            RectangleF chipArea = new RectangleF(
                card.X + inner,
                descriptionRect.Bottom + Math.Max(6f, Height * 0.010f),
                card.Width - inner * 2f,
                Math.Max(22f, Height * 0.045f));
            DrawChips(g, chipArea);

            // Play button pinned to the bottom of the card.
            float playHeight = Math.Max(46f, Height * 0.108f);
            RectangleF play = new RectangleF(
                card.X + inner,
                card.Bottom - inner - playHeight,
                card.Width - inner * 2f,
                playHeight);

            // Small prompt above the button closes the gap nicely.
            Font promptFont = UiTheme.Pixel(Math.Max(8f, Height * 0.018f), g);
            RectangleF promptRect = new RectangleF(
                card.X + inner,
                play.Y - Math.Max(20f, Height * 0.040f),
                card.Width - inner * 2f,
                Math.Max(16f, Height * 0.030f));

            using (StringFormat promptFormat = new StringFormat())
            {
                promptFormat.Alignment = StringAlignment.Near;
                promptFormat.LineAlignment = StringAlignment.Center;

                using (SolidBrush promptBrush = new SolidBrush(UiTheme.Leaf))
                {
                    g.DrawString("READY, VOLUNTEER?", promptFont, promptBrush, promptRect, promptFormat);
                }
            }

            DrawStartButton(g, play, radius);

            if (_hovered)
            {
                UiTheme.DrawRounded(g, card, radius, UiTheme.Sunshine, 4f);
            }
            else
            {
                UiTheme.DrawRounded(g, card, radius, UiTheme.Ink, 4f);
            }

            if (Focused)
            {
                using (Pen focusPen = new Pen(Color.FromArgb(210, UiTheme.Sunshine), 2f))
                {
                    focusPen.DashStyle = DashStyle.Dot;
                    using (GraphicsPath focusPath = UiTheme.RoundedRect(
                        new RectangleF(card.X + 3f, card.Y + 3f, card.Width - 6f, card.Height - 6f),
                        Math.Max(4f, radius - 3f)))
                    {
                        g.DrawPath(focusPen, focusPath);
                    }
                }
            }
        }

        private void DrawPreview(Graphics g, RectangleF preview, float inner)
        {
            float previewRadius = Math.Max(10f, preview.Height * 0.075f);
            UiTheme.FillRounded(g, preview, previewRadius, Color.FromArgb(28, 76, 48));

            if (PreviewImage != null)
            {
                UiTheme.DrawImageCover(g, PreviewImage, preview, previewRadius);
            }
            else
            {
                using (LinearGradientBrush fallback = new LinearGradientBrush(
                    preview, AccentColor, UiTheme.Sky, LinearGradientMode.Vertical))
                {
                    using (GraphicsPath path = UiTheme.RoundedRect(preview, previewRadius))
                    {
                        g.FillPath(fallback, path);
                    }
                }

                Font numberFont = UiTheme.Pixel(Math.Max(24f, preview.Height * 0.24f), g);
                using (StringFormat numberFormat = new StringFormat())
                {
                    numberFormat.Alignment = StringAlignment.Center;
                    numberFormat.LineAlignment = StringAlignment.Center;
                    UiTheme.DrawTextWithShadow(g, StageNumber.ToString(), numberFont, UiTheme.Cream,
                        preview, numberFormat);
                }
            }

            // Soft dark fade at the bottom keeps the art from fighting the title.
            RectangleF fade = new RectangleF(
                preview.X,
                preview.Bottom - preview.Height * 0.22f,
                preview.Width,
                preview.Height * 0.22f);
            using (LinearGradientBrush fadeBrush = new LinearGradientBrush(
                fade, Color.FromArgb(0, 12, 32, 18), Color.FromArgb(70, 12, 32, 18), LinearGradientMode.Vertical))
            {
                GraphicsState state = g.Save();
                using (GraphicsPath clip = UiTheme.RoundedRect(preview, previewRadius))
                {
                    g.SetClip(clip);
                    g.FillRectangle(fadeBrush, fade);
                }
                g.Restore(state);
            }

            UiTheme.DrawRounded(g, preview, previewRadius, Color.FromArgb(210, 255, 247, 230), 2f);
        }

        private void DrawChips(Graphics g, RectangleF area)
        {
            if (Chips == null || Chips.Length == 0)
            {
                return;
            }

            Font chipFont = UiTheme.Body(Math.Max(8.5f, Height * 0.019f), g, FontStyle.Bold);
            float x = area.X;
            float gap = Math.Max(6f, area.Width * 0.018f);

            foreach (string chip in Chips)
            {
                if (string.IsNullOrEmpty(chip))
                {
                    continue;
                }

                string text = chip.ToUpperInvariant();
                SizeF textSize = g.MeasureString(text, chipFont);
                float width = textSize.Width + 30f;
                if (x + width > area.Right)
                {
                    break;
                }

                RectangleF rect = new RectangleF(x, area.Y, width, area.Height);
                UiTheme.FillRounded(g, rect, area.Height / 2f, UiTheme.CreamShade);
                UiTheme.DrawRounded(g, rect, area.Height / 2f, Color.FromArgb(226, 210, 178), 1f);

                using (SolidBrush dotBrush = new SolidBrush(AccentColor))
                {
                    float dot = Math.Max(4f, area.Height * 0.20f);
                    g.FillEllipse(dotBrush, rect.X + 9f, rect.Y + (rect.Height - dot) / 2f, dot, dot);
                }

                using (StringFormat chipFormat = new StringFormat())
                {
                    chipFormat.Alignment = StringAlignment.Near;
                    chipFormat.LineAlignment = StringAlignment.Center;

                    RectangleF textRect = new RectangleF(
                        rect.X + 17f,
                        rect.Y,
                        rect.Width - 21f,
                        rect.Height);

                    using (SolidBrush chipBrush = new SolidBrush(UiTheme.InkSoft))
                    {
                        g.DrawString(text, chipFont, chipBrush, textRect, chipFormat);
                    }
                }

                x += width + gap;
            }
        }

        private void DrawStartButton(Graphics g, RectangleF play, float cardRadius)
        {
            float buttonRadius = Math.Min(Math.Max(10f, play.Height * 0.30f), cardRadius);

            // Button shadow.
            UiTheme.FillRounded(g,
                new RectangleF(play.X, play.Y + 3f, play.Width, play.Height),
                buttonRadius,
                Color.FromArgb(55, 20, 60, 28));

            Color fill = _pressed ? UiTheme.Leaf : (_hovered ? UiTheme.LeafBright : UiTheme.Leaf);
            UiTheme.FillRounded(g, play, buttonRadius, fill);

            // Top highlight.
            RectangleF highlight = new RectangleF(play.X + 4f, play.Y + 3f, play.Width - 8f, Math.Max(3f, play.Height * 0.14f));
            UiTheme.FillRounded(g, highlight, highlight.Height / 2f, Color.FromArgb(70, 255, 255, 255));

            Font playFont = UiTheme.Pixel(Math.Max(12f, Height * 0.029f), g);
            string label = "START";
            SizeF labelSize = g.MeasureString(label, playFont);
            float triangleWidth = Math.Max(10f, play.Height * 0.20f);
            float gap = Math.Max(8f, play.Height * 0.14f);
            float groupWidth = triangleWidth + gap + labelSize.Width;
            float groupX = play.X + (play.Width - groupWidth) / 2f;
            float centerY = play.Y + play.Height / 2f;

            using (SolidBrush creamBrush = new SolidBrush(UiTheme.Cream))
            {
                PointF top = new PointF(groupX, centerY - triangleWidth * 0.55f);
                PointF bottom = new PointF(groupX, centerY + triangleWidth * 0.55f);
                PointF tip = new PointF(groupX + triangleWidth, centerY);
                g.FillPolygon(creamBrush, new[] { top, bottom, tip });
            }

            RectangleF labelRect = new RectangleF(
                groupX + triangleWidth + gap,
                play.Y,
                labelSize.Width + 6f,
                play.Height);

            using (StringFormat labelFormat = new StringFormat())
            {
                labelFormat.Alignment = StringAlignment.Near;
                labelFormat.LineAlignment = StringAlignment.Center;

                using (SolidBrush labelBrush = new SolidBrush(UiTheme.Cream))
                {
                    g.DrawString(label, playFont, labelBrush, labelRect, labelFormat);
                }
            }

            UiTheme.DrawRounded(g, play, buttonRadius, UiTheme.Ink, 3f);
        }

        private static float PillWidth(Graphics g, string text, Font font)
        {
            return g.MeasureString(text, font).Width + 20f;
        }

        private static void DrawPill(Graphics g, string text, Font font, RectangleF rect, Color fill, Color foreColor)
        {
            UiTheme.FillRounded(g, rect, rect.Height / 2f, fill);
            UiTheme.DrawRounded(g, rect, rect.Height / 2f, Color.FromArgb(80, 20, 42, 25), 1f);

            using (StringFormat format = new StringFormat())
            {
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;

                using (SolidBrush brush = new SolidBrush(foreColor))
                {
                    g.DrawString(text, font, brush, rect, format);
                }
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _hovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hovered = false;
            _pressed = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _pressed = true;
                Focus();
                Invalidate();
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            bool activate = _pressed && e.Button == MouseButtons.Left && ClientRectangle.Contains(e.Location);
            _pressed = false;
            Invalidate();

            if (activate)
            {
                ActivateCard();
            }

            base.OnMouseUp(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                ActivateCard();
                return;
            }

            base.OnKeyDown(e);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            Invalidate();
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            Invalidate();
            base.OnLostFocus(e);
        }

        private void ActivateCard()
        {
            EventHandler handler = PlayClicked;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}
