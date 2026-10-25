using System;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

namespace cwtsGame
{
    public class TrashItem
    {
        public PictureBox Picture;
        public string Category;
        public string Name;

       	private double animOffset = 0;
		private double animDirection = 1;
		private double animationSpeed = 0.15;
        private int originalY;

        public TrashItem(string name, string category, int x, int y)
        {
            Name = name;
            Category = category;

            Picture = new PictureBox();
            Picture.Size = new Size(30,30);
            Picture.Location = new Point(x, y);
            Picture.SizeMode = PictureBoxSizeMode.StretchImage;
            Picture.BackColor = Color.Transparent;

            originalY = y;

            string imageFile = "";

            switch (name)
            {
                case "Banana Peel":
                    imageFile = "bananap.png";
                    break;

                case "Paper":
                    imageFile = "paper.png";
                    break;

                case "Plastic Bottle":
                    imageFile = "plasticbottle.png";
                    break;

                case "Tin Can":
                    imageFile = "tincan.png";
                    break;

                case "Styrofoam":
                    imageFile = "styro.png";
                    break;

                case "Battery":
                    imageFile = "batt.png";
                    break;

                case "Newspaper":
                    imageFile = "newsp.png";
                    break;

                case "Glass Bottle":
                    imageFile = "glassb.png";
                    break;

                case "Plastic Bag":
                    imageFile = "plasticb.png";
                    break;
            }

            if (File.Exists(imageFile))
            {
                Picture.Image = Image.FromFile(imageFile);
            }
            else
            {
                Picture.BackColor = Color.Yellow;
                Picture.BorderStyle = BorderStyle.FixedSingle;
            }
        }

        // Floating animation
        public void Animate()
		{
		    animOffset += animDirection * animationSpeed;
		
		    if (animOffset >= 3)
		        animDirection = -1;
		
		    if (animOffset <= -3)
		        animDirection = 1;
		
		    Picture.Top = originalY + (int)animOffset;
		}
    }
}