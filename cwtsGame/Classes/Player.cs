/*
 * Created by SharpDevelop.
 * User: cruzl
 * Date: 4/25/2026
 * Time: 4:26 PM
 *
 * FIXES:
 *  1. Transparency  - TransparentPictureBox simplified (no WS_EX_TRANSPARENT /
 *     manual parent painting). It just draws the image over the form background.
 *  2. White sprite background - LoadFrames() now converts the sheet to ARGB and
 *     flood-fills the white background of each frame to transparent, so a
 *     character.png without real alpha still works.
 *  3. Diagonal speed normalization - Move(dx, dy) normalizes the vector.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace cwtsGame
{
    // ---------------------------------------------------------------
    // PictureBox that lets the form's background show through
    // ---------------------------------------------------------------
    public class TransparentPictureBox : PictureBox
    {
        public TransparentPictureBox()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;

            if (Image != null)
                e.Graphics.DrawImage(Image, 0, 0, Width, Height);
        }
    }

    public class Player
    {
        public TransparentPictureBox Picture;
        public int Speed;
        public int MaxSlots;
        public InventorySlot[] Inventory;
        public int ToolLevel;

        Bitmap spriteSheet;

        const int DISPLAY_W = 42;  // 14 x 3
        const int DISPLAY_H = 60;  // 20 x 3

        private string[] toolNames = { "Hand", "Bag", "Cart" };
        private int[] toolSlots = { 1, 2, 3 };

        private int frame = 0;
        private int lastDir = 0;

        private Bitmap[,] frames; // [col, dir]

        // { srcX, srcY, srcW, srcH }
        private static readonly int[,,] frameCoords = new int[3, 4, 4]
        {
            // col 0
            {
                { 5,  4,  14, 18 }, // Down
                { 5,  28, 14, 20 }, // Up
                { 5,  52, 14, 18 }, // Right
                { 5,  76, 14, 19 }, // Left
            },
            // col 1
            {
                { 30, 4,  13, 18 }, // Down
                { 30, 28, 13, 20 }, // Up
                { 30, 52, 13, 18 }, // Right
                { 30, 76, 13, 19 }, // Left
            },
            // col 2
            {
                { 54, 4,  13, 18 }, // Down
                { 54, 28, 13, 20 }, // Up
                { 54, 52, 13, 18 }, // Right
                { 54, 76, 13, 19 }, // Left
            },
        };

        public string CurrentToolName
        {
            get { return toolNames[ToolLevel]; }
        }

        public Player(int startX, int startY)
        {
            ToolLevel = 0;
            MaxSlots = 1;
            Speed = 5;

            Inventory = new InventorySlot[5];
            for (int i = 0; i < 5; i++)
                Inventory[i] = new InventorySlot();

            Picture = new TransparentPictureBox();
            Picture.Size = new Size(DISPLAY_W, DISPLAY_H);
            Picture.MinimumSize = new Size(DISPLAY_W, DISPLAY_H);
            Picture.MaximumSize = new Size(DISPLAY_W, DISPLAY_H);
            Picture.Location = new Point(startX, startY);

            spriteSheet = new Bitmap(Application.StartupPath + "\\character.png");
            LoadFrames();
            Picture.Image = frames[0, 0];
        }

        // ================= BACKGROUND REMOVAL =================

        // Make sure the sheet has a real alpha channel so transparency can be stored.
        private static Bitmap ToArgb(Bitmap src)
        {
            Bitmap dst = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(dst))
            {
                g.DrawImage(src, new Rectangle(0, 0, src.Width, src.Height));
            }
            return dst;
        }

        // Already transparent, or (near) white.
        // Lower 235 if it eats into the sprite's light colors.
        private static bool IsBackground(Color c)
        {
            return c.A < 10 || (c.R > 235 && c.G > 235 && c.B > 235);
        }

        // Flood-fill inward from the edges of ONE frame, so only the outer
        // background is removed. White inside the sprite (eyes, shirt) stays.
        private static void ClearBackground(Bitmap bmp, Rectangle area)
        {
            area = Rectangle.Intersect(area, new Rectangle(0, 0, bmp.Width, bmp.Height));
            if (area.Width <= 0 || area.Height <= 0) return;

            bool[,] seen = new bool[bmp.Width, bmp.Height];
            Stack<Point> todo = new Stack<Point>();

            for (int x = area.Left; x < area.Right; x++)
            {
                todo.Push(new Point(x, area.Top));
                todo.Push(new Point(x, area.Bottom - 1));
            }
            for (int y = area.Top; y < area.Bottom; y++)
            {
                todo.Push(new Point(area.Left, y));
                todo.Push(new Point(area.Right - 1, y));
            }

            while (todo.Count > 0)
            {
                Point p = todo.Pop();
                if (!area.Contains(p) || seen[p.X, p.Y]) continue;
                seen[p.X, p.Y] = true;

                if (!IsBackground(bmp.GetPixel(p.X, p.Y))) continue;

                bmp.SetPixel(p.X, p.Y, Color.Transparent);
                todo.Push(new Point(p.X + 1, p.Y));
                todo.Push(new Point(p.X - 1, p.Y));
                todo.Push(new Point(p.X, p.Y + 1));
                todo.Push(new Point(p.X, p.Y - 1));
            }
        }

        // ================= FRAMES / ANIMATION =================

        private void LoadFrames()
        {
            // Convert once so transparency can actually be stored
            Bitmap argb = ToArgb(spriteSheet);
            spriteSheet.Dispose();
            spriteSheet = argb;

            frames = new Bitmap[3, 4];

            for (int dir = 0; dir < 4; dir++)
            {
                for (int col = 0; col < 3; col++)
                {
                    int srcX = frameCoords[col, dir, 0];
                    int srcY = frameCoords[col, dir, 1];
                    int srcW = frameCoords[col, dir, 2];
                    int srcH = frameCoords[col, dir, 3];

                    // Remove the white around this frame before cropping/scaling it
                    ClearBackground(spriteSheet, new Rectangle(srcX, srcY, srcW, srcH));

                    Bitmap bmp = new Bitmap(DISPLAY_W, DISPLAY_H, PixelFormat.Format32bppArgb);
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.Clear(Color.Transparent);
                        g.InterpolationMode = InterpolationMode.NearestNeighbor;
                        g.PixelOffsetMode = PixelOffsetMode.Half;
                        g.DrawImage(
                            spriteSheet,
                            new Rectangle(0, 0, DISPLAY_W, DISPLAY_H),
                            new Rectangle(srcX, srcY, srcW, srcH),
                            GraphicsUnit.Pixel
                        );
                    }
                    frames[col, dir] = bmp;
                }
            }
        }

        private void Animate(int dir)
        {
            if (lastDir != dir) frame = 0;
            lastDir = dir;
            frame++;
            if (frame > 2) frame = 0;
            Picture.Image = frames[frame, dir];
        }

        public void Move(float dx, float dy)
        {
            if (dx == 0 && dy == 0) return;

            // Normalize so diagonal length == 1 (not sqrt(2))
            float len = (float)Math.Sqrt(dx * dx + dy * dy);
            dx /= len;
            dy /= len;

            Picture.Left += (int)Math.Round(dx * Speed);
            Picture.Top += (int)Math.Round(dy * Speed);

            // Facing direction for animation: prefer horizontal when moving diagonally
            if (Math.Abs(dx) >= Math.Abs(dy))
                Animate(dx > 0 ? 2 : 3); // Right : Left
            else
                Animate(dy > 0 ? 0 : 1); // Down  : Up
        }

        // Keep the old cardinal helpers in case anything else calls them
        public void MoveDown() { Picture.Top += Speed; Animate(0); }
        public void MoveUp() { Picture.Top -= Speed; Animate(1); }
        public void MoveRight() { Picture.Left += Speed; Animate(2); }
        public void MoveLeft() { Picture.Left -= Speed; Animate(3); }

        // ================= INVENTORY =================

        public bool HasEmptySlot()
        {
            for (int i = 0; i < MaxSlots; i++)
                if (Inventory[i].IsEmpty) return true;
            return false;
        }

        public bool AddItem(TrashItem item)
        {
            for (int i = 0; i < MaxSlots; i++)
            {
                if (Inventory[i].IsEmpty)
                {
                    Inventory[i].Item = item;
                    return true;
                }
            }
            return false;
        }

        public int CountItems()
        {
            int count = 0;
            for (int i = 0; i < MaxSlots; i++)
                if (!Inventory[i].IsEmpty) count++;
            return count;
        }

        public void ClearInventory()
        {
            for (int i = 0; i < 5; i++)
                Inventory[i].Item = null;
        }

        public bool UpgradeTool()
        {
            if (ToolLevel >= 2) return false;
            ToolLevel++;
            MaxSlots = toolSlots[ToolLevel];
            return true;
        }

        public int NextUpgradeCost()
        {
            int[] costs = { 20, 50, 100 };
            return costs[ToolLevel];
        }
    }
}