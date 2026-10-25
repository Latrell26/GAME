using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace cwtsGame
{
    public class NPC
    {
        public PictureBox Picture;
        public PixelTag NameLabel;
        public string Type;

        public NPC(string type, string displayName, int x, int y, string imageFile, Color textColor)
        {
            Type = type;

            Picture = new PictureBox();
            Picture.Size = new Size(80, 80);
            Picture.Location = new Point(x, y);
            Picture.SizeMode = PictureBoxSizeMode.StretchImage;
            Picture.BackColor = Color.Transparent;

            // LOAD IMAGE OR GIF
            if (File.Exists(imageFile))
            {
                Picture.Image = Image.FromFile(imageFile);
            }

            NameLabel = new PixelTag();
            NameLabel.Text = displayName;
            NameLabel.PixelFontSize = 8.5f;
            NameLabel.FillColor = UiTheme.Cream;
            NameLabel.ForeColor = UiTheme.Ink;
            NameLabel.BorderColor = UiTheme.Ink;
            NameLabel.AccentColor = textColor;
            NameLabel.ShowAccent = true;
            NameLabel.Location = new Point(
                x + (Picture.Width - NameLabel.Width) / 2,
                y + 82);
        }
    }
}