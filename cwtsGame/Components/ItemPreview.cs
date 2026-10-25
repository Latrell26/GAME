using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace cwtsGame
{
    /// <summary>
    /// Rounded item portrait used by the sorting dialog.
    /// </summary>
    public class ItemPreview : Control
    {
        private Image itemImage;
        private string itemName = string.Empty;

        public ItemPreview()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            BackColor = UiTheme.CreamShade;
            DoubleBuffered = true;
            TabStop = false;
            AccessibleRole = AccessibleRole.Graphic;
        }

        public void SetItem(Image image, string name)
        {
            itemImage = image;
            itemName = name ?? string.Empty;
            AccessibleDescription = itemName;
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

            using (GraphicsPath path = UiTheme.RoundedRect(
                new RectangleF(0f, 0f, Width, Height), 16f))
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
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            if (Width < 20 || Height < 20)
            {
                return;
            }

            RectangleF card = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
            UiTheme.FillRounded(g, card, 16f, UiTheme.CreamShade);
            UiTheme.DrawRounded(g, card, 16f, UiTheme.Ink, 2.5f);

            float labelHeight = 34f;
            RectangleF imageArea = new RectangleF(
                12f,
                12f,
                Width - 24f,
                Math.Max(20f, Height - labelHeight - 20f));

            if (itemImage != null)
            {
                float scale = Math.Min(
                    (imageArea.Width - 12f) / itemImage.Width,
                    (imageArea.Height - 12f) / itemImage.Height);
                scale = Math.Min(scale, 3.2f);

                float width = Math.Max(1f, itemImage.Width * scale);
                float height = Math.Max(1f, itemImage.Height * scale);
                RectangleF imageRect = new RectangleF(
                    imageArea.X + (imageArea.Width - width) / 2f,
                    imageArea.Y + (imageArea.Height - height) / 2f,
                    width,
                    height);

                g.DrawImage(itemImage, imageRect);
            }
            else
            {
                Font placeholderFont = UiTheme.Pixel(36f, g);
                using (SolidBrush placeholderBrush = new SolidBrush(UiTheme.InkSoft))
                using (StringFormat placeholderFormat = new StringFormat())
                {
                    placeholderFormat.Alignment = StringAlignment.Center;
                    placeholderFormat.LineAlignment = StringAlignment.Center;
                    g.DrawString("?", placeholderFont, placeholderBrush, imageArea, placeholderFormat);
                }
            }

            Font nameFont = UiTheme.Pixel(10f, g);
            RectangleF nameRect = new RectangleF(10f, Height - labelHeight - 2f, Width - 20f, labelHeight);
            using (SolidBrush nameBrush = new SolidBrush(UiTheme.Ink))
            using (StringFormat nameFormat = new StringFormat())
            {
                nameFormat.Alignment = StringAlignment.Center;
                nameFormat.LineAlignment = StringAlignment.Center;
                nameFormat.Trimming = StringTrimming.EllipsisCharacter;
                nameFormat.FormatFlags = StringFormatFlags.NoWrap;
                g.DrawString(itemName.ToUpperInvariant(), nameFont, nameBrush, nameRect, nameFormat);
            }
        }
    }
}
