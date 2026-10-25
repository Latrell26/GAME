using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace cwtsGame
{
    /// <summary>
    /// Shared palette, rounded-shape helpers and font loading for the pixel-art UI.
    /// The Silkscreen font is bundled with the game and loaded privately, so no
    /// system-wide font installation is required.
    /// </summary>
    internal static class UiTheme
    {
        public static readonly Color Cream = Color.FromArgb(255, 247, 230);
        public static readonly Color CreamShade = Color.FromArgb(247, 232, 201);
        public static readonly Color Ink = Color.FromArgb(35, 74, 52);
        public static readonly Color InkSoft = Color.FromArgb(91, 112, 92);
        public static readonly Color Leaf = Color.FromArgb(63, 163, 77);
        public static readonly Color LeafBright = Color.FromArgb(88, 196, 90);
        public static readonly Color Sunshine = Color.FromArgb(255, 208, 70);
        public static readonly Color Sky = Color.FromArgb(82, 168, 224);
        public static readonly Color Wood = Color.FromArgb(122, 78, 45);
        public static readonly Color WoodDark = Color.FromArgb(78, 49, 29);
        public static readonly Color WoodLight = Color.FromArgb(158, 108, 66);
        public static readonly Color Coral = Color.FromArgb(239, 106, 91);

        private static readonly Dictionary<string, Font> FontCache = new Dictionary<string, Font>();
        private static readonly PrivateFontCollection PixelFonts = new PrivateFontCollection();
        private static bool _pixelLoaded;

        static UiTheme()
        {
            LoadPixelFont();
        }

        private static void LoadPixelFont()
        {
            try
            {
                string folder = Path.Combine(Application.StartupPath, "Assets", "Fonts");
                string[] candidates = { "Silkscreen-Regular.ttf", "Silkscreen-Bold.ttf" };

                foreach (string name in candidates)
                {
                    string path = Path.Combine(folder, name);
                    if (File.Exists(path))
                    {
                        PixelFonts.AddFontFile(path);
                    }
                }

                if (PixelFonts.Families.Length > 0)
                {
                    _pixelLoaded = true;
                }
            }
            catch
            {
                _pixelLoaded = false;
            }
        }

        public static Font Pixel(float pixelSize, Graphics graphics)
        {
            float points = PixelsToPoints(pixelSize, graphics);
            if (_pixelLoaded)
            {
                return GetPixelFont(points);
            }

            return GetFont("Segoe UI", points, FontStyle.Bold);
        }

        private static Font GetPixelFont(float points)
        {
            string key = "pixel|" + points.ToString("0.##", CultureInfo.InvariantCulture);
            Font font;
            if (!FontCache.TryGetValue(key, out font))
            {
                font = new Font(PixelFonts.Families[0], points, FontStyle.Regular, GraphicsUnit.Point);
                FontCache[key] = font;
            }

            return font;
        }

        public static Font Body(float pixelSize, Graphics graphics, FontStyle style)
        {
            return GetFont("Segoe UI", PixelsToPoints(pixelSize, graphics), style);
        }

        private static float PixelsToPoints(float pixels, Graphics graphics)
        {
            float dpi = graphics == null ? 96f : graphics.DpiY;
            return pixels * 72f / dpi;
        }

        private static Font GetFont(string family, float points, FontStyle style)
        {
            string key = family + "|" + points.ToString("0.##", CultureInfo.InvariantCulture) + "|" + (int)style;
            Font font;
            if (!FontCache.TryGetValue(key, out font))
            {
                font = new Font(family, points, style, GraphicsUnit.Point);
                FontCache[key] = font;
            }

            return font;
        }

        public static GraphicsPath RoundedRect(RectangleF rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            radius = Math.Max(0f, Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2f));

            if (radius <= 0.1f)
            {
                path.AddRectangle(rect);
                return path;
            }

            float diameter = radius * 2f;
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180f, 90f);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270f, 90f);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0f, 90f);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90f, 90f);
            path.CloseFigure();
            return path;
        }

        public static void FillRounded(Graphics g, RectangleF rect, float radius, Color color)
        {
            using (GraphicsPath path = RoundedRect(rect, radius))
            using (SolidBrush brush = new SolidBrush(color))
            {
                g.FillPath(brush, path);
            }
        }

        public static void DrawRounded(Graphics g, RectangleF rect, float radius, Color color, float width)
        {
            using (GraphicsPath path = RoundedRect(rect, radius))
            using (Pen pen = new Pen(color, width))
            {
                pen.Alignment = PenAlignment.Inset;
                g.DrawPath(pen, path);
            }
        }

        public static void DrawImageCover(Graphics g, Image image, RectangleF bounds, float radius)
        {
            if (image == null)
            {
                return;
            }

            GraphicsState state = g.Save();
            try
            {
                using (GraphicsPath clip = RoundedRect(bounds, radius))
                {
                    g.SetClip(clip);
                    float scale = Math.Max(bounds.Width / image.Width, bounds.Height / image.Height);
                    float width = image.Width * scale;
                    float height = image.Height * scale;
                    float x = bounds.X + (bounds.Width - width) / 2f;
                    float y = bounds.Y + (bounds.Height - height) / 2f;
                    g.DrawImage(image, x, y, width, height);
                }
            }
            finally
            {
                g.Restore(state);
            }
        }

        public static void DrawTextWithShadow(Graphics g, string text, Font font, Color color,
            RectangleF rect, StringFormat format)
        {
            using (SolidBrush shadow = new SolidBrush(Color.FromArgb(120, 20, 32, 22)))
            {
                RectangleF shadowRect = new RectangleF(rect.X + 2f, rect.Y + 2f, rect.Width, rect.Height);
                g.DrawString(text, font, shadow, shadowRect, format);
            }

            using (SolidBrush brush = new SolidBrush(color))
            {
                g.DrawString(text, font, brush, rect, format);
            }
        }
    }
}
