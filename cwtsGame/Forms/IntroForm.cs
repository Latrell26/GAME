using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Media;
using System.Windows.Forms;

namespace cwtsGame
{
    public partial class IntroForm : Form
    {
        private readonly Form previousForm;
        private readonly int currentStage;

        private string stageTitle = "";
        private string stageDifficulty = "";
        private string[] stageChips = new string[0];
        private Color stageAccent = UiTheme.Leaf;
        private string fullText = "";
        private Image stagePreview;

        private MemoryStream backgroundStream;
        private Image backgroundImage;
        private bool backgroundAnimating;

        private PixelSign pnlSign;
        private PixelSign pnlFooter;
        private Panel pnlCard;
        private Label lblText;
        private RoundButton BtnStart;
        private RoundButton btnSkip;
        private RoundButton btnBack;

        private Timer typingTimer;
        private SoundPlayer typingSound;
        private bool typingSoundPlaying;
        private int textIndex;
        private bool typingStarted;

        private RectangleF previewBounds;
        private RectangleF snapshotBounds;
        private RectangleF sectionBounds;
        private RectangleF promptBounds;
        private bool showPreview;
        private bool showSnapshot;

        private const string StageSubtitleFormat = "STAGE {0} \u2022 MISSION BRIEFING";

        public IntroForm(int stage, Form prev)
        {
            InitializeComponent();

            currentStage = stage;
            previousForm = prev;

            WindowState = FormWindowState.Maximized;
            DoubleBuffered = true;
            KeyPreview = true;
            BackColor = UiTheme.Sky;

            LoadStage();
            SetupBackground();
            SetupTypingSound();
            SetupInterface();
            LayoutIntro();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LayoutIntro();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            LayoutIntro();
            RefreshIntroControls();

            if (!typingStarted)
            {
                typingStarted = true;
                StartTypingAnimation();
            }

            // Under the debugger the first paint can happen before the form is
            // fully laid out. Refresh once after the message loop settles.
            try
            {
                BeginInvoke(new MethodInvoker(RefreshIntroControls));
            }
            catch (InvalidOperationException)
            {
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            LayoutIntro();
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

        // ─────────────────────────────
        // STAGE CONTENT
        // ─────────────────────────────
        private void LoadStage()
        {
            if (currentStage == 2)
            {
                stageTitle = "THE STREET";
                stageDifficulty = "MEDIUM";
                stageChips = new[] { "5 LEVELS", "14 TO SORT" };
                stageAccent = UiTheme.Coral;
                fullText =
                    "The street was once the heartbeat of the neighborhood. Vendors set up their stalls early in the morning, " +
                    "neighbors greeted each other while sweeping their storefronts, and children played hopscotch on the pavement.\n\n" +
                    "But over the years, the street has deteriorated. Litter accumulated along sidewalks. Discarded plastic bags " +
                    "tangled around trees, cigarette butts scattered across the pavement.\n\n" +
                    "The local community leaders decided it was time to act.\n\n" +
                    "As a CWTS volunteer, you've been called to help revitalize this important community space. Clean, organize " +
                    "waste disposal, and restore the street.";
                stagePreview = LoadStill("preview_street.png") ?? LoadStill("mapstreet.png");
            }
            else
            {
                stageTitle = "THE PARK";
                stageDifficulty = "EASY";
                stageChips = new[] { "5 LEVELS", "14 TO SORT" };
                stageAccent = UiTheme.Leaf;
                fullText =
                    "Families would gather here every weekend. Children played under the shade of tall trees, and students from " +
                    "nearby schools often held small CWTS clean-up drives to keep the area beautiful. It was a place full of life, " +
                    "laughter, and fresh air.\n\n" +
                    "But over time, things changed. People became busy. Visitors stopped caring. Trash bins were ignored even when " +
                    "they were right beside benches.\n\n" +
                    "Plastic bottles, food wrappers, and scattered waste slowly began to take over the grass and pathways. The park " +
                    "that once symbolized unity and care became quiet\u2026 and dirty.\n\n" +
                    "Now, the local community has decided to bring it back to life\u2014but they need help.\n\n" +
                    "As a CWTS volunteer, you are assigned to restore the park. Clean the waste, bring back its beauty, and remind " +
                    "everyone that even small actions\u2014like picking up trash\u2014can revive a place that once meant so much.";
                stagePreview = LoadStill("preview_park.png") ?? LoadStill("park2.png");
            }

            fullText = (fullText ?? "").Trim();
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

        private void OnBackgroundFrameChanged(object sender, EventArgs e)
        {
            if (IsDisposed || Disposing || !IsHandleCreated)
            {
                return;
            }

            if (InvokeRequired)
            {
                try
                {
                    BeginInvoke(new EventHandler(OnBackgroundFrameChanged), sender, e);
                }
                catch (InvalidOperationException)
                {
                }

                return;
            }

            Invalidate();
        }

        // ─────────────────────────────
        // INTERFACE
        // ─────────────────────────────
        private void SetupInterface()
        {
            SuspendLayout();

            pnlCard = new BufferedPanel();
            pnlCard.BackColor = UiTheme.Cream;
            pnlCard.TabStop = false;
            pnlCard.AccessibleRole = AccessibleRole.Pane;
            pnlCard.AccessibleName = "Mission briefing";
            pnlCard.Paint += PnlCardPaint;
            pnlCard.Resize += PnlCardResize;
            Controls.Add(pnlCard);

            pnlSign = new PixelSign();
            pnlSign.Title = stageTitle;
            pnlSign.Subtitle = string.Format(StageSubtitleFormat, currentStage);
            pnlSign.AccessibleName = stageTitle + " mission briefing";
            Controls.Add(pnlSign);

            pnlFooter = new PixelSign();
            pnlFooter.FooterStyle = true;
            pnlFooter.Title = "CLEAN - SORT - RESTORE";
            Controls.Add(pnlFooter);

            lblText = new Label();
            lblText.AutoSize = false;
            lblText.AutoEllipsis = false;
            lblText.BackColor = UiTheme.Cream;
            lblText.ForeColor = UiTheme.InkSoft;
            lblText.TextAlign = ContentAlignment.TopLeft;
            lblText.UseMnemonic = false;
            lblText.Text = "";
            lblText.AccessibleName = "Briefing text";
            pnlCard.Controls.Add(lblText);

            BtnStart = new RoundButton();
            StylePrimaryButton(BtnStart);
            BtnStart.Click += BtnStartClick;
            pnlCard.Controls.Add(BtnStart);

            btnSkip = new RoundButton();
            StyleSecondaryButton(btnSkip);
            btnSkip.Click += BtnSkipClick;
            pnlCard.Controls.Add(btnSkip);

            btnBack = new RoundButton();
            StyleBackButton(btnBack);
            btnBack.Click += BtnBackClick;
            Controls.Add(btnBack);

            BtnStart.TabIndex = 2;
            btnSkip.TabIndex = 1;
            btnBack.TabIndex = 0;

            ResumeLayout(false);
        }

        private void StylePrimaryButton(RoundButton button)
        {
            button.Text = "LET'S GO";
            button.BackColor = UiTheme.Leaf;
            button.ForeColor = UiTheme.Cream;
            button.BorderColor = UiTheme.Ink;
            button.BorderWidth = 3f;
            button.HoverBackColor = UiTheme.LeafBright;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.UseVisualStyleBackColor = false;
            button.Cursor = Cursors.Hand;
            button.TabStop = true;
            button.AccessibleRole = AccessibleRole.PushButton;
            button.AccessibleName = "Let's go to the game";
        }

        private void StyleSecondaryButton(RoundButton button)
        {
            button.Text = "SKIP";
            button.BackColor = UiTheme.Cream;
            button.ForeColor = UiTheme.Ink;
            button.BorderColor = UiTheme.Ink;
            button.BorderWidth = 2.5f;
            button.HoverBackColor = UiTheme.Sunshine;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.UseVisualStyleBackColor = false;
            button.Cursor = Cursors.Hand;
            button.TabStop = true;
            button.AccessibleRole = AccessibleRole.PushButton;
            button.AccessibleName = "Skip the briefing typing";
        }

        private void StyleBackButton(RoundButton button)
        {
            button.Text = "\u2190";
            button.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            button.BackColor = UiTheme.Cream;
            button.ForeColor = UiTheme.Ink;
            button.BorderColor = UiTheme.Ink;
            button.BorderWidth = 2f;
            button.HoverBackColor = UiTheme.Sunshine;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.UseVisualStyleBackColor = false;
            button.Cursor = Cursors.Hand;
            button.TabStop = true;
            button.AccessibleRole = AccessibleRole.PushButton;
            button.AccessibleName = "Back to stages";
        }

        private void RefreshIntroControls()
        {
            if (IsDisposed)
            {
                return;
            }

            if (pnlCard != null && !pnlCard.IsDisposed)
            {
                pnlCard.Invalidate();
                pnlCard.Update();
            }

            if (lblText != null && !lblText.IsDisposed)
            {
                lblText.Refresh();
            }

            if (BtnStart != null && !BtnStart.IsDisposed)
            {
                BtnStart.Refresh();
            }

            if (btnSkip != null && !btnSkip.IsDisposed)
            {
                btnSkip.Refresh();
            }

            if (pnlSign != null && !pnlSign.IsDisposed)
            {
                pnlSign.Refresh();
            }

            if (pnlFooter != null && !pnlFooter.IsDisposed)
            {
                pnlFooter.Refresh();
            }

            if (btnBack != null && !btnBack.IsDisposed)
            {
                btnBack.Refresh();
            }
        }

        // ─────────────────────────────
        // TYPEWRITER
        // ─────────────────────────────
        private void SetupTypingSound()
        {
            string path = Path.Combine(Application.StartupPath, "typing.wav");
            if (!File.Exists(path))
            {
                return;
            }

            try
            {
                typingSound = new SoundPlayer(path);
                typingSound.Load();
            }
            catch
            {
                if (typingSound != null)
                {
                    typingSound.Dispose();
                    typingSound = null;
                }
            }
        }

        private void StartTypingSound()
        {
            if (typingSound == null || typingSoundPlaying)
            {
                return;
            }

            try
            {
                typingSound.PlayLooping();
                typingSoundPlaying = true;
            }
            catch
            {
                typingSoundPlaying = false;
            }
        }

        private void StopTypingSound()
        {
            if (typingSound == null || !typingSoundPlaying)
            {
                return;
            }

            try
            {
                typingSound.Stop();
            }
            catch
            {
            }

            typingSoundPlaying = false;
        }

        private void StartTypingAnimation()
        {
            StopTypingTimer();

            lblText.Text = "";
            textIndex = 0;
            btnSkip.Visible = true;

            typingTimer = new Timer();
            typingTimer.Interval = 8;
            typingTimer.Tick += TypingEffect;
            typingTimer.Start();
            StartTypingSound();
        }

        private void TypingEffect(object sender, EventArgs e)
        {
            if (textIndex >= fullText.Length)
            {
                StopTypingTimer();
                btnSkip.Visible = false;
                return;
            }

            int revealCount = Math.Min(2, fullText.Length - textIndex);
            lblText.Text += fullText.Substring(textIndex, revealCount);
            textIndex += revealCount;
        }

        private void StopTypingTimer()
        {
            StopTypingSound();

            if (typingTimer != null)
            {
                typingTimer.Stop();
                typingTimer.Dispose();
                typingTimer = null;
            }
        }

        private void BtnSkipClick(object sender, EventArgs e)
        {
            StopTypingTimer();
            lblText.Text = fullText;
            btnSkip.Visible = false;
            BtnStart.Focus();
        }

        // ─────────────────────────────
        // NAVIGATION
        // ─────────────────────────────
        private void BtnStartClick(object sender, EventArgs e)
        {
            StopTypingTimer();

            GameForm gameForm = new GameForm(currentStage);
            gameForm.Show();
            Hide();
        }

        private void BtnBackClick(object sender, EventArgs e)
        {
            GoBack();
        }

        private void GoBack()
        {
            StopTypingTimer();

            if (previousForm != null && !previousForm.IsDisposed)
            {
                previousForm.Show();
            }

            Close();
        }

        // ─────────────────────────────
        // RESPONSIVE LAYOUT
        // ─────────────────────────────
        private void LayoutIntro()
        {
            if (pnlCard == null || pnlSign == null || pnlFooter == null || btnBack == null)
            {
                return;
            }

            int width = ClientSize.Width;
            int height = ClientSize.Height;
            if (width < 300 || height < 300)
            {
                return;
            }

            int signWidth = Math.Min(760, Math.Max(420, (int)(width * 0.56f)));
            int signHeight = Math.Max(92, (int)(signWidth * 0.155f));
            int signTop = Math.Max(16, (int)(height * 0.033f));

            pnlSign.Size = new Size(signWidth, signHeight);
            pnlSign.Location = new Point((width - signWidth) / 2, signTop);

            int backSize = Math.Max(44, Math.Min(64, (int)(height * 0.060f)));
            btnBack.Size = new Size(backSize, backSize);
            btnBack.Radius = backSize / 2;
            btnBack.Location = new Point(
                Math.Max(16, (int)(width * 0.018f)),
                Math.Max(14, (int)(height * 0.030f)));

            int footerHeight = Math.Max(34, (int)(height * 0.037f));
            int footerBottom = Math.Max(18, (int)(height * 0.026f));
            int footerTop = height - footerBottom - footerHeight;

            pnlFooter.Size = new Size(Math.Min(520, Math.Max(320, width - 80)), footerHeight);
            pnlFooter.Location = new Point((width - pnlFooter.Width) / 2, footerTop);

            int cardTopBase = pnlSign.Bottom + Math.Max(14, (int)(height * 0.018f));
            int cardBottom = footerTop - Math.Max(14, (int)(height * 0.016f));
            int available = Math.Max(280, cardBottom - cardTopBase);

            int cardWidth = Math.Min(1280, Math.Max(860, width - Math.Max(80, width / 12)));
            cardWidth = Math.Min(cardWidth, width - 32);
            if (cardWidth < 420)
            {
                cardWidth = Math.Max(320, width - 32);
            }

            int cardHeight = Math.Max(280, Math.Min(780, available));
            int cardTop = cardTopBase + Math.Max(0, (available - cardHeight) / 2);

            pnlCard.Size = new Size(cardWidth, cardHeight);
            pnlCard.Location = new Point((width - cardWidth) / 2, cardTop);

            UpdateCardRegion();
            LayoutCardContents();

            pnlCard.BringToFront();
            pnlSign.BringToFront();
            pnlFooter.BringToFront();
            btnBack.BringToFront();
            pnlCard.Invalidate();
        }

        private void LayoutCardContents()
        {
            if (pnlCard == null || lblText == null || BtnStart == null || btnSkip == null)
            {
                return;
            }

            int width = pnlCard.Width;
            int height = pnlCard.Height;
            if (width < 120 || height < 100)
            {
                return;
            }

            int pad = Math.Max(18, Math.Min(34, (int)(width * 0.024f)));
            int leftWidth = Math.Max(260, Math.Min(460, (int)(width * 0.36f)));
            int gap = Math.Max(22, Math.Min(36, (int)(width * 0.028f)));
            int contentWidth = width - pad * 2;
            int rightWidth = contentWidth - leftWidth - gap;

            if (rightWidth < 360)
            {
                int deficit = 360 - rightWidth;
                leftWidth = Math.Max(200, leftWidth - deficit);
                rightWidth = contentWidth - leftWidth - gap;
            }

            showPreview = leftWidth >= 220 && rightWidth >= 280;
            if (!showPreview)
            {
                leftWidth = 0;
                gap = 0;
                rightWidth = contentWidth;
                previewBounds = RectangleF.Empty;
                snapshotBounds = RectangleF.Empty;
                showSnapshot = false;
            }

            float contentHeight = height - pad * 2f;
            if (showPreview)
            {
                float previewHeight = Math.Min(leftWidth * 0.78f, contentHeight * 0.58f);
                previewBounds = new RectangleF(pad, pad, leftWidth, previewHeight);

                float snapshotTop = previewBounds.Bottom + 18f;
                snapshotBounds = new RectangleF(
                    pad,
                    snapshotTop,
                    leftWidth,
                    pnlCard.Height - pad - snapshotTop);
                showSnapshot = snapshotBounds.Height > 80f;
            }

            float rightX = showPreview ? pad + leftWidth + gap : pad;
            float rightY = pad;
            float rightW = pnlCard.Width - pad - rightX;
            if (rightW < 100f)
            {
                return;
            }

            RectangleF right = new RectangleF(rightX, rightY, rightW, contentHeight);

            float sectionHeight = Math.Max(20f, height * 0.034f);
            sectionBounds = new RectangleF(right.X, right.Y, right.Width, sectionHeight);

            float primaryHeight = Math.Max(48f, Math.Min(62f, height * 0.090f));
            float primaryWidth = Math.Max(190f, Math.Min(250f, primaryHeight * 4.2f));
            float skipHeight = Math.Max(38f, primaryHeight * 0.78f);
            float skipWidth = Math.Max(100f, Math.Min(150f, skipHeight * 2.7f));
            float buttonGap = 12f;

            if (primaryWidth + buttonGap + skipWidth > right.Width)
            {
                skipWidth = Math.Max(0f, right.Width - primaryWidth - buttonGap);
                if (skipWidth < 70f)
                {
                    primaryWidth = Math.Max(120f, right.Width * 0.52f);
                    skipWidth = Math.Max(0f, right.Width - primaryWidth - buttonGap);
                }
            }

            float buttonY = right.Bottom - primaryHeight;
            float promptHeight = Math.Max(18f, height * 0.026f);
            float promptTop = buttonY - promptHeight - 12f;
            promptBounds = new RectangleF(right.X, promptTop, right.Width, promptHeight);

            float bodyTop = sectionBounds.Bottom + 10f;
            float bodyBottom = promptTop - 10f;
            float bodyHeight = Math.Max(60f, bodyBottom - bodyTop);
            RectangleF body = new RectangleF(right.X, bodyTop, right.Width, bodyHeight);

            lblText.Bounds = Rectangle.Round(body);

            BtnStart.Bounds = Rectangle.Round(new RectangleF(
                right.Right - primaryWidth,
                buttonY,
                primaryWidth,
                primaryHeight));

            btnSkip.Bounds = Rectangle.Round(new RectangleF(
                right.Right - primaryWidth - buttonGap - skipWidth,
                buttonY + (primaryHeight - skipHeight),
                Math.Max(1f, skipWidth),
                skipHeight));

            BtnStart.PixelFontSize = Math.Max(12f, BtnStart.Height * 0.32f);
            btnSkip.PixelFontSize = Math.Max(9f, btnSkip.Height * 0.28f);

            BtnStart.Radius = Math.Max(8, Math.Min(BtnStart.Height / 2, BtnStart.Width / 2));
            btnSkip.Radius = Math.Max(8, Math.Min(btnSkip.Height / 2, btnSkip.Width / 2));

            float maxBodyPixels = Math.Max(14f, Math.Min(18f, height * 0.032f));
            SetBodyFont(body.Width, body.Height, maxBodyPixels);

            pnlCard.Invalidate();
        }

        private void SetBodyFont(float width, float height, float maxPixels)
        {
            if (lblText == null || width < 20f || height < 20f)
            {
                return;
            }

            Font chosen = null;
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {
                for (float pixels = maxPixels; pixels >= 10.5f; pixels -= 0.5f)
                {
                    Font candidate = UiTheme.Body(pixels, g, FontStyle.Regular);
                    Size measured = TextRenderer.MeasureText(
                        fullText,
                        candidate,
                        new Size((int)Math.Max(40f, width - 2f), 100000),
                        TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);

                    if (measured.Height <= height - 6f)
                    {
                        chosen = candidate;
                        break;
                    }
                }

                if (chosen == null)
                {
                    chosen = UiTheme.Body(10.5f, g, FontStyle.Regular);
                }
            }

            if (!ReferenceEquals(lblText.Font, chosen))
            {
                lblText.Font = chosen;
            }
        }

        private void PnlCardResize(object sender, EventArgs e)
        {
            UpdateCardRegion();
            LayoutCardContents();

            if (pnlCard != null && !pnlCard.IsDisposed)
            {
                pnlCard.Invalidate();
            }
        }

        private float CardRadius()
        {
            if (pnlCard == null)
            {
                return 24f;
            }

            return Math.Max(18f, Math.Min(40f, pnlCard.Height * 0.042f));
        }

        private void UpdateCardRegion()
        {
            if (pnlCard == null || pnlCard.Width < 2 || pnlCard.Height < 2)
            {
                return;
            }

            using (GraphicsPath path = UiTheme.RoundedRect(
                new RectangleF(0, 0, pnlCard.Width, pnlCard.Height), CardRadius()))
            {
                Region oldRegion = pnlCard.Region;
                pnlCard.Region = new Region(path);
                if (oldRegion != null)
                {
                    oldRegion.Dispose();
                }
            }
        }

        // ─────────────────────────────
        // CARD PAINTING
        // ─────────────────────────────
        private void PnlCardPaint(object sender, PaintEventArgs e)
        {
            if (pnlCard == null || pnlCard.Width < 40 || pnlCard.Height < 40)
            {
                return;
            }

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            using (SolidBrush cream = new SolidBrush(UiTheme.Cream))
            {
                g.FillRectangle(cream, new RectangleF(0, 0, pnlCard.Width, pnlCard.Height));
            }

            UiTheme.DrawRounded(
                g,
                new RectangleF(0, 0, pnlCard.Width, pnlCard.Height),
                CardRadius(),
                UiTheme.Ink,
                4f);

            if (showPreview)
            {
                DrawPreview(g);
                if (showSnapshot)
                {
                    DrawSnapshot(g);
                }
            }

            DrawSection(g);
            DrawPrompt(g);
        }

        private void DrawPreview(Graphics g)
        {
            if (previewBounds.Width <= 4f || previewBounds.Height <= 4f)
            {
                return;
            }

            float previewRadius = Math.Max(12f, previewBounds.Height * 0.065f);

            if (stagePreview != null)
            {
                UiTheme.DrawImageCover(g, stagePreview, previewBounds, previewRadius);
            }
            else
            {
                using (GraphicsPath path = UiTheme.RoundedRect(previewBounds, previewRadius))
                using (LinearGradientBrush fallback = new LinearGradientBrush(
                    previewBounds,
                    stageAccent,
                    UiTheme.Sky,
                    LinearGradientMode.Vertical))
                {
                    g.FillPath(fallback, path);
                }

                Font numberFont = UiTheme.Pixel(Math.Max(36f, previewBounds.Height * 0.30f), g);
                using (StringFormat format = new StringFormat())
                {
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Center;

                    using (SolidBrush brush = new SolidBrush(Color.FromArgb(135, UiTheme.Cream)))
                    {
                        g.DrawString(
                            currentStage.ToString("00"),
                            numberFont,
                            brush,
                            previewBounds,
                            format);
                    }
                }
            }

            UiTheme.DrawRounded(g, previewBounds, previewRadius, UiTheme.Ink, 2f);

            Font pillFont = UiTheme.Pixel(Math.Max(10f, pnlCard.Height * 0.014f), g);
            float pillHeight = Math.Max(24f, pnlCard.Height * 0.034f);
            float inset = Math.Max(10f, previewBounds.Width * 0.030f);

            string stageLabel = "STAGE " + currentStage;
            float stagePillWidth = PillWidth(g, stageLabel, pillFont);
            RectangleF stagePill = new RectangleF(
                previewBounds.X + inset,
                previewBounds.Y + inset,
                stagePillWidth,
                pillHeight);
            DrawPill(g, stageLabel, pillFont, stagePill, UiTheme.Sunshine, UiTheme.Ink);

            string difficulty = stageDifficulty.ToUpperInvariant();
            float difficultyWidth = PillWidth(g, difficulty, pillFont);
            RectangleF difficultyPill = new RectangleF(
                previewBounds.Right - difficultyWidth - inset,
                previewBounds.Y + inset,
                difficultyWidth,
                pillHeight);
            DrawPill(g, difficulty, pillFont, difficultyPill, stageAccent, Color.White);
        }

        private void DrawSnapshot(Graphics g)
        {
            if (snapshotBounds.Width <= 4f || snapshotBounds.Height <= 4f)
            {
                return;
            }

            Font labelFont = UiTheme.Pixel(Math.Max(10f, Math.Min(12f, pnlCard.Height * 0.016f)), g);
            using (SolidBrush labelBrush = new SolidBrush(stageAccent))
            {
                g.DrawString(
                    "MISSION SNAPSHOT",
                    labelFont,
                    labelBrush,
                    new PointF(snapshotBounds.X, snapshotBounds.Y));
            }

            Font chipFont = UiTheme.Body(Math.Max(10f, Math.Min(13f, pnlCard.Height * 0.020f)), g, FontStyle.Bold);
            float chipHeight = Math.Max(24f, pnlCard.Height * 0.032f);
            float chipTop = snapshotBounds.Y + Math.Max(26f, pnlCard.Height * 0.036f);
            float x = snapshotBounds.X;

            foreach (string chip in stageChips)
            {
                float chipWidth = g.MeasureString(chip, chipFont).Width + 38f;
                if (x + chipWidth > snapshotBounds.Right)
                {
                    break;
                }

                RectangleF chipRect = new RectangleF(x, chipTop, chipWidth, chipHeight);
                UiTheme.FillRounded(g, chipRect, chipRect.Height / 2f, UiTheme.CreamShade);
                UiTheme.DrawRounded(g, chipRect, chipRect.Height / 2f, Color.FromArgb(226, 210, 178), 1f);

                float dot = Math.Max(4f, chipRect.Height * 0.20f);
                using (SolidBrush dotBrush = new SolidBrush(stageAccent))
                {
                    g.FillEllipse(
                        dotBrush,
                        chipRect.X + 10f,
                        chipRect.Y + (chipRect.Height - dot) / 2f,
                        dot,
                        dot);
                }

                using (StringFormat chipFormat = new StringFormat())
                {
                    chipFormat.Alignment = StringAlignment.Near;
                    chipFormat.LineAlignment = StringAlignment.Center;
                    chipFormat.FormatFlags = StringFormatFlags.NoWrap;

                    using (SolidBrush chipBrush = new SolidBrush(UiTheme.Ink))
                    {
                        g.DrawString(
                            chip,
                            chipFont,
                            chipBrush,
                            new RectangleF(
                                chipRect.X + 20f,
                                chipRect.Y,
                                chipRect.Width - 24f,
                                chipRect.Height),
                            chipFormat);
                    }
                }

                x += chipWidth + 10f;
            }

            if (snapshotBounds.Height > 160f)
            {
                Font watermarkFont = UiTheme.Pixel(Math.Max(42f, snapshotBounds.Height * 0.40f), g);
                string watermark = currentStage.ToString("00");
                SizeF watermarkSize = g.MeasureString(watermark, watermarkFont);

                using (SolidBrush watermarkBrush = new SolidBrush(Color.FromArgb(241, 225, 194)))
                {
                    g.DrawString(
                        watermark,
                        watermarkFont,
                        watermarkBrush,
                        new PointF(
                            snapshotBounds.X,
                            snapshotBounds.Bottom - watermarkSize.Height + 4f));
                }
            }

            if (snapshotBounds.Height > 180f)
            {
                Font quoteFont = UiTheme.Body(
                    Math.Max(10f, pnlCard.Height * 0.019f),
                    g,
                    FontStyle.Italic);

                string quote = currentStage == 1
                    ? "Bring the green back."
                    : "Take back the block.";

                using (SolidBrush quoteBrush = new SolidBrush(UiTheme.InkSoft))
                {
                    g.DrawString(
                        quote,
                        quoteFont,
                        quoteBrush,
                        new RectangleF(
                            snapshotBounds.X,
                            chipTop + chipHeight + 16f,
                            snapshotBounds.Width,
                            48f));
                }
            }
        }

        private void DrawSection(Graphics g)
        {
            if (sectionBounds.Width <= 4f)
            {
                return;
            }

            Font sectionFont = UiTheme.Pixel(Math.Max(11f, Math.Min(14f, pnlCard.Height * 0.020f)), g);
            using (SolidBrush brush = new SolidBrush(stageAccent))
            {
                g.DrawString(
                    "YOUR ASSIGNMENT",
                    sectionFont,
                    brush,
                    new PointF(sectionBounds.X, sectionBounds.Y));
            }

            RectangleF underline = new RectangleF(
                sectionBounds.X,
                sectionBounds.Bottom - 2f,
                Math.Max(42f, sectionBounds.Width * 0.07f),
                3f);
            UiTheme.FillRounded(g, underline, 1.5f, UiTheme.Sunshine);
        }

        private void DrawPrompt(Graphics g)
        {
            if (promptBounds.Width <= 4f)
            {
                return;
            }

            RectangleF line = new RectangleF(
                promptBounds.X,
                promptBounds.Y - 6f,
                promptBounds.Width,
                2f);
            UiTheme.FillRounded(g, line, 1f, UiTheme.CreamShade);

            Font promptFont = UiTheme.Pixel(Math.Max(10f, Math.Min(12f, pnlCard.Height * 0.017f)), g);
            using (StringFormat format = new StringFormat())
            {
                format.Alignment = StringAlignment.Near;
                format.LineAlignment = StringAlignment.Center;
                format.FormatFlags = StringFormatFlags.NoWrap;

                using (SolidBrush brush = new SolidBrush(UiTheme.Leaf))
                {
                    g.DrawString("READY, VOLUNTEER?", promptFont, brush, promptBounds, format);
                }
            }
        }

        private static float PillWidth(Graphics g, string text, Font font)
        {
            return g.MeasureString(text, font).Width + 22f;
        }

        private static void DrawPill(Graphics g, string text, Font font, RectangleF rect,
            Color fill, Color textColor)
        {
            UiTheme.FillRounded(g, rect, rect.Height / 2f, fill);
            UiTheme.DrawRounded(g, rect, rect.Height / 2f, Color.FromArgb(80, 20, 42, 25), 1f);

            using (StringFormat format = new StringFormat())
            {
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;
                format.FormatFlags = StringFormatFlags.NoWrap;

                using (SolidBrush brush = new SolidBrush(textColor))
                {
                    g.DrawString(text, font, brush, rect, format);
                }
            }
        }

        // ─────────────────────────────
        // CLEANUP
        // ─────────────────────────────
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            StopTypingTimer();
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

            if (stagePreview != null)
            {
                stagePreview.Dispose();
                stagePreview = null;
            }

            if (typingSound != null)
            {
                typingSound.Dispose();
                typingSound = null;
            }

            base.OnFormClosed(e);
        }

        /// <summary>
        /// A double-buffered card surface. The briefing card is fully opaque,
        /// which keeps it readable over the animated GIF behind the form.
        /// </summary>
        private sealed class BufferedPanel : Panel
        {
            public BufferedPanel()
            {
                DoubleBuffered = true;
                SetStyle(ControlStyles.ResizeRedraw, true);
            }
        }
    }
}
