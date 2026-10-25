using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace cwtsGame
{
    public partial class StageForm : Form
    {
        private Form previousForm;

        private MemoryStream backgroundStream;
        private Image backgroundImage;
        private bool backgroundAnimating;
        private Image parkPreview;
        private Image streetPreview;

        public StageForm(Form prev)
        {
            InitializeComponent();

            previousForm = prev;

            WindowState = FormWindowState.Maximized;
            DoubleBuffered = true;
            KeyPreview = true;

            SetupBackground();
            SetupCards();
            SetupBackButton();
            LayoutStage();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LayoutStage();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            LayoutStage();
            RefreshStageControls();

            // Under the debugger the form can be shown before the first paint
            // completes, so refresh once more after the message loop settles.
            try
            {
                BeginInvoke(new MethodInvoker(RefreshStageControls));
            }
            catch (InvalidOperationException)
            {
            }
        }

        private void RefreshStageControls()
        {
            if (IsDisposed)
            {
                return;
            }

            if (cardPark != null && !cardPark.IsDisposed)
            {
                cardPark.Refresh();
            }

            if (cardStreet != null && !cardStreet.IsDisposed)
            {
                cardStreet.Refresh();
            }

            if (pnlSign != null && !pnlSign.IsDisposed)
            {
                pnlSign.Refresh();
            }

            if (pnlFooter != null && !pnlFooter.IsDisposed)
            {
                pnlFooter.Refresh();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            LayoutStage();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                GoBack();
                return;
            }

            base.OnKeyDown(e);
        }

        // ─────────────────────────────
        // BACKGROUND
        // ─────────────────────────────
        private void SetupBackground()
        {
            string path = Path.Combine(Application.StartupPath, "imgBackGroundGIF2.gif");
            if (!File.Exists(path))
            {
                path = Path.Combine(Application.StartupPath, "imgBackgroundGIF2.gif");
            }

            if (!File.Exists(path))
            {
                BackColor = UiTheme.Sky;
                return;
            }

            try
            {
                // Keep the stream alive so the GIF keeps animating. The form paints
                // its own background image, which lets the transparent cards and
                // signs composite correctly over the artwork.
                backgroundStream = new MemoryStream(File.ReadAllBytes(path));
                backgroundImage = Image.FromStream(backgroundStream);
                BackgroundImage = backgroundImage;
                BackgroundImageLayout = ImageLayout.Stretch;
                StartBackgroundAnimation();
            }
            catch
            {
                BackColor = UiTheme.Sky;
            }
        }

        private void StartBackgroundAnimation()
        {
            if (backgroundImage == null || backgroundAnimating)
            {
                return;
            }

            ImageAnimator.Animate(backgroundImage, OnBackgroundFrameChanged);
            backgroundAnimating = true;
        }

        private void StopBackgroundAnimation()
        {
            if (backgroundImage == null || !backgroundAnimating)
            {
                return;
            }

            ImageAnimator.StopAnimate(backgroundImage, OnBackgroundFrameChanged);
            backgroundAnimating = false;
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (Visible)
            {
                StartBackgroundAnimation();
            }
            else
            {
                StopBackgroundAnimation();
            }
        }

        private void OnBackgroundFrameChanged(object sender, EventArgs e)
        {
            if (IsDisposed || Disposing || !IsHandleCreated)
            {
                return;
            }

            // ImageAnimator raises this on its own animation thread. WinForms
            // painting must happen on the UI thread, otherwise some controls
            // (notably the card lowest in the z-order) silently stop repainting.
            if (InvokeRequired)
            {
                try
                {
                    BeginInvoke(new EventHandler(OnBackgroundFrameChanged), sender, e);
                }
                catch (InvalidOperationException)
                {
                    // The form is closing; nothing left to repaint.
                }

                return;
            }

            Invalidate();
        }

        // ─────────────────────────────
        // STAGE CARDS
        // ─────────────────────────────
        private void SetupCards()
        {
            parkPreview = LoadStill("preview_park.png") ?? LoadStill("park2.png");
            streetPreview = LoadStill("preview_street.png") ?? LoadStill("mapstreet.png");

            cardPark.SetStage(
                1,
                "THE PARK",
                "Fountains, lawns and quiet paths — buried under litter.",
                "EASY",
                new[] { "5 LEVELS", "14 TO SORT" },
                UiTheme.Leaf,
                parkPreview);

            cardStreet.SetStage(
                2,
                "THE STREET",
                "Sidewalks, drains and storefronts on the busiest block.",
                "MEDIUM",
                new[] { "5 LEVELS", "14 TO SORT" },
                UiTheme.Coral,
                streetPreview);

            cardPark.PlayClicked += delegate { PlayStage(1); };
            cardStreet.PlayClicked += delegate { PlayStage(2); };
        }

        private static Image LoadStill(string fileName)
        {
            string path = Path.Combine(Application.StartupPath, fileName);
            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (Image source = Image.FromStream(stream))
                {
                    return new Bitmap(source);
                }
            }
            catch
            {
                return null;
            }
        }

        // ─────────────────────────────
        // BACK BUTTON
        // ─────────────────────────────
        private void SetupBackButton()
        {
            btnBack.Text = "←";
            btnBack.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            btnBack.BackColor = UiTheme.Cream;
            btnBack.ForeColor = UiTheme.Ink;
            btnBack.BorderColor = UiTheme.Ink;
            btnBack.BorderWidth = 2F;
            btnBack.HoverBackColor = UiTheme.Sunshine;
        }

        private void BtnBackClick(object sender, EventArgs e)
        {
            GoBack();
        }

        private void GoBack()
        {
            if (previousForm != null && !previousForm.IsDisposed)
            {
                previousForm.Show();
            }

            Close();
        }

        // ─────────────────────────────
        // RESPONSIVE LAYOUT
        // ─────────────────────────────
        private void LayoutStage()
        {
            if (cardPark == null || cardStreet == null || pnlSign == null || pnlFooter == null)
            {
                return;
            }

            int width = ClientSize.Width;
            int height = ClientSize.Height;
            if (width < 200 || height < 200)
            {
                return;
            }

            int signWidth = Math.Min(760, Math.Max(420, (int)(width * 0.56)));
            int signHeight = Math.Max(92, (int)(signWidth * 0.155));
            int signTop = Math.Max(16, (int)(height * 0.033));

            pnlSign.Size = new Size(signWidth, signHeight);
            pnlSign.Location = new Point((width - signWidth) / 2, signTop);

            int backSize = Math.Max(44, Math.Min(64, (int)(height * 0.060)));
            btnBack.Size = new Size(backSize, backSize);
            btnBack.Radius = backSize / 2;
            btnBack.Location = new Point(
                Math.Max(16, (int)(width * 0.018)),
                Math.Max(14, (int)(height * 0.030)));

            int footerHeight = Math.Max(34, (int)(height * 0.037));
            int footerBottom = Math.Max(18, (int)(height * 0.026));
            int footerTop = height - footerBottom - footerHeight;

            int cardsTop = pnlSign.Bottom + Math.Max(12, (int)(height * 0.018));
            int cardsBottom = footerTop - Math.Max(12, (int)(height * 0.016));
            int available = Math.Max(260, cardsBottom - cardsTop);

            int gap = Math.Max(24, (int)(width * 0.022));
            int cardHeight = Math.Min(560, Math.Max(320, available));
            int cardWidth = (int)(cardHeight * 0.75);

            int maxCardWidth = (width - gap * 3) / 2;
            if (maxCardWidth > 0 && cardWidth > maxCardWidth)
            {
                cardWidth = maxCardWidth;
                cardHeight = (int)(cardWidth / 0.75);
            }

            if (cardWidth < 260)
            {
                cardWidth = Math.Max(220, maxCardWidth);
                cardHeight = (int)(cardWidth / 0.75);
            }

            int totalWidth = cardWidth * 2 + gap;
            int left = Math.Max(gap, (width - totalWidth) / 2);
            int top = cardsTop + Math.Max(0, (available - cardHeight) / 2);

            cardPark.Size = new Size(cardWidth, cardHeight);
            cardPark.Location = new Point(left, top);

            cardStreet.Size = new Size(cardWidth, cardHeight);
            cardStreet.Location = new Point(left + cardWidth + gap, top);

            pnlFooter.Size = new Size(Math.Min(520, Math.Max(320, width - 80)), footerHeight);
            pnlFooter.Location = new Point((width - pnlFooter.Width) / 2, footerTop);

            cardPark.BringToFront();
            cardStreet.BringToFront();
            pnlSign.BringToFront();
            pnlFooter.BringToFront();
            btnBack.BringToFront();
        }

        // ─────────────────────────────
        // STAGE LOGIC
        // ─────────────────────────────
        private void PlayStage(int stage)
        {
            IntroForm intro = new IntroForm(stage, this);
            intro.Show();
            Hide();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            StopBackgroundAnimation();

            if (backgroundImage != null)
            {
                BackgroundImage = null;
                backgroundImage.Dispose();
                backgroundImage = null;
            }

            if (backgroundStream != null)
            {
                backgroundStream.Dispose();
                backgroundStream = null;
            }

            if (parkPreview != null)
            {
                parkPreview.Dispose();
                parkPreview = null;
            }

            if (streetPreview != null)
            {
                streetPreview.Dispose();
                streetPreview = null;
            }

            base.OnFormClosed(e);
        }
    }
}
