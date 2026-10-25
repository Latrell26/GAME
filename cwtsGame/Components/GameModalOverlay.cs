using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace cwtsGame
{
    /// <summary>
    /// Dim, click-blocking layer placed behind modal cards so the map stays
    /// visible but the dialog reads clearly.
    /// </summary>
    public class GameModalOverlay : Control
    {
        private readonly Color overlayColor = Color.FromArgb(150, 8, 22, 14);

        public GameModalOverlay()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.ResizeRedraw, true);

            BackColor = Color.Transparent;
            Dock = DockStyle.Fill;
            TabStop = false;
            AccessibleRole = AccessibleRole.Pane;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            using (SolidBrush brush = new SolidBrush(overlayColor))
            {
                e.Graphics.FillRectangle(brush, ClientRectangle);
            }
        }
    }
}
