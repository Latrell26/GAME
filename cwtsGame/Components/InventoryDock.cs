using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace cwtsGame
{
    /// <summary>
    /// Cream inventory dock for the items the volunteer is carrying. Item cards
    /// are drawn by hand so the dock stays crisp over the map art.
    /// </summary>
    public class InventoryDock : Control
    {
        private const int MaxVisibleSlots = 3;

        private readonly string[] itemNames = new string[MaxVisibleSlots];
        private readonly Image[] itemImages = new Image[MaxVisibleSlots];
        private int maxSlots = 1;
        private int carried;
        private string toolName = "HAND";
        private string signature = string.Empty;

        public InventoryDock()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            BackColor = UiTheme.Cream;
            DoubleBuffered = true;
            TabStop = false;
            AccessibleRole = AccessibleRole.Grouping;
            AccessibleName = "Carried items";
        }

        public void UpdateFrom(Player player)
        {
            int newMaxSlots = 1;
            int newCarried = 0;
            string newTool = "HAND";
            string[] newNames = new string[MaxVisibleSlots];
            Image[] newImages = new Image[MaxVisibleSlots];

            if (player != null)
            {
                newMaxSlots = Math.Max(1, Math.Min(MaxVisibleSlots, player.MaxSlots));
                newCarried = player.CountItems();
                newTool = string.IsNullOrEmpty(player.CurrentToolName)
                    ? "HAND"
                    : player.CurrentToolName.ToUpperInvariant();

                for (int i = 0; i < newMaxSlots; i++)
                {
                    if (i >= player.Inventory.Length || player.Inventory[i].IsEmpty)
                    {
                        continue;
                    }

                    TrashItem item = player.Inventory[i].Item;
                    if (item == null)
                    {
                        continue;
                    }

                    newNames[i] = item.Name ?? string.Empty;
                    newImages[i] = item.Picture == null ? null : item.Picture.Image;
                }
            }

            string newSignature = newMaxSlots + "|" + newCarried + "|" + newTool + "|" +
                string.Join("~", newNames);

            if (newSignature == signature)
            {
                return;
            }

            signature = newSignature;
            maxSlots = newMaxSlots;
            carried = newCarried;
            toolName = newTool;

            for (int i = 0; i < MaxVisibleSlots; i++)
            {
                itemNames[i] = newNames[i];
                itemImages[i] = newImages[i];
            }

            AccessibleDescription = "Carrying " + carried + " of " + maxSlots +
                " items. Tool: " + toolName;

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            if (Width < 80 || Height < 50)
            {
                return;
            }

            RectangleF card = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
            float radius = Math.Max(16f, Math.Min(24f, Height * 0.20f));

            UiTheme.FillRounded(g, card, radius, UiTheme.Cream);
            UiTheme.DrawRounded(g, card, radius, UiTheme.Ink, 3f);

            float headerY = 12f;
            Font headerFont = UiTheme.Pixel(9f, g);
            Font headerValueFont = UiTheme.Pixel(10f, g);

            using (SolidBrush softBrush = new SolidBrush(UiTheme.InkSoft))
            {
                g.DrawString("CARRY", headerFont, softBrush, 20f, headerY);
            }

            using (SolidBrush inkBrush = new SolidBrush(UiTheme.Ink))
            {
                g.DrawString(carried + "/" + maxSlots, headerValueFont, inkBrush, 72f, headerY - 1f);
            }

            string toolText = "TOOL  " + toolName;
            using (SolidBrush leafBrush = new SolidBrush(UiTheme.Leaf))
            using (StringFormat rightFormat = new StringFormat())
            {
                rightFormat.Alignment = StringAlignment.Far;
                rightFormat.LineAlignment = StringAlignment.Near;
                rightFormat.FormatFlags = StringFormatFlags.NoWrap;
                rightFormat.Trimming = StringTrimming.EllipsisCharacter;
                g.DrawString(toolText, headerFont, leafBrush,
                    new RectangleF(Width - 190f, headerY, 170f, 18f), rightFormat);
            }

            int slots = Math.Max(1, Math.Min(MaxVisibleSlots, maxSlots));
            float gap = 10f;
            float available = Width - 40f;
            float slotWidth = Math.Min(132f, (available - gap * (slots - 1)) / slots);
            float totalWidth = slotWidth * slots + gap * (slots - 1);
            float startX = (Width - totalWidth) / 2f;
            float slotY = 34f;
            float slotHeight = Math.Max(46f, Height - slotY - 30f);

            for (int i = 0; i < slots; i++)
            {
                RectangleF slot = new RectangleF(startX + i * (slotWidth + gap), slotY, slotWidth, slotHeight);
                DrawSlot(g, slot, i);
            }

            Font hintFont = UiTheme.Body(8.5f, g, FontStyle.Regular);
            RectangleF hintRect = new RectangleF(16f, Height - 23f, Width - 32f, 16f);
            using (SolidBrush hintBrush = new SolidBrush(UiTheme.InkSoft))
            using (StringFormat hintFormat = new StringFormat())
            {
                hintFormat.Alignment = StringAlignment.Center;
                hintFormat.LineAlignment = StringAlignment.Center;
                hintFormat.FormatFlags = StringFormatFlags.NoWrap;
                g.DrawString("WASD MOVE   \u2022   E INTERACT", hintFont, hintBrush, hintRect, hintFormat);
            }
        }

        private void DrawSlot(Graphics g, RectangleF slot, int index)
        {
            UiTheme.FillRounded(g, slot, 11f, UiTheme.CreamShade);
            UiTheme.DrawRounded(g, slot, 11f, Color.FromArgb(180, UiTheme.Ink), 2f);

            string name = index < itemNames.Length ? itemNames[index] : null;
            Image image = index < itemImages.Length ? itemImages[index] : null;

            if (string.IsNullOrEmpty(name))
            {
                Font emptyFont = UiTheme.Pixel(8.5f, g);
                using (SolidBrush emptyBrush = new SolidBrush(UiTheme.InkSoft))
                using (StringFormat emptyFormat = new StringFormat())
                {
                    emptyFormat.Alignment = StringAlignment.Center;
                    emptyFormat.LineAlignment = StringAlignment.Center;
                    g.DrawString("EMPTY", emptyFont, emptyBrush, slot, emptyFormat);
                }

                return;
            }

            float imageSide = Math.Min(36f, slot.Height - 14f);
            RectangleF imageRect = new RectangleF(
                slot.X + 8f,
                slot.Y + (slot.Height - imageSide) / 2f,
                imageSide,
                imageSide);

            if (image != null)
            {
                g.DrawImage(image, imageRect);
            }
            else
            {
                UiTheme.FillRounded(g, imageRect, 5f, UiTheme.Cream);
                UiTheme.DrawRounded(g, imageRect, 5f, UiTheme.InkSoft, 1f);
            }

            RectangleF nameRect = new RectangleF(
                imageRect.Right + 6f,
                slot.Y + 5f,
                Math.Max(10f, slot.Right - imageRect.Right - 12f),
                slot.Height - 10f);

            Font nameFont = UiTheme.Body(8.5f, g, FontStyle.Bold);
            using (SolidBrush nameBrush = new SolidBrush(UiTheme.Ink))
            using (StringFormat nameFormat = new StringFormat())
            {
                nameFormat.Alignment = StringAlignment.Near;
                nameFormat.LineAlignment = StringAlignment.Center;
                nameFormat.Trimming = StringTrimming.EllipsisWord;
                nameFormat.FormatFlags = StringFormatFlags.LineLimit;
                g.DrawString(name, nameFont, nameBrush, nameRect, nameFormat);
            }
        }
    }
}
