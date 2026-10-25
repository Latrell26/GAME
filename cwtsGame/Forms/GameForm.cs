using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Media; 

namespace cwtsGame
{
    public partial class GameForm : Form
    {
    	//get from classes
        private Timer gameTimer;
        private Timer hideTimer;
        private Timer levelTimer;
        private Player player;
        private List<TrashItem> trashList;
        private List<Label> trashLabels;
        private List<NPC> npcs;
        private GameManager manager;
        
        private PixelSign stageSign;
        private GameHud hud;
        private InventoryDock inventoryDock;
        private GameToast toast;

        private PixelCardPanel sortPanel;
        private ItemPreview sortPreview;
        private Label lblSortItem;
        private Label lblSortQuestion;
        private RoundButton btnBio;
        private RoundButton btnNonBio;
        private RoundButton btnRecycle;
        private RoundButton btnCancelSort;

        private PixelCardPanel shopPanel;
        private Label lblShopInfo;
        private Label lblShopItem;
        private Label lblShopDesc;
        private RoundButton btnBuySpeed;
        private RoundButton btnCloseShop;

        private RoundButton btnSettings;
        private Image settingsIcon;
        private PixelCardPanel settingsPanel;
        private RoundButton btnCloseSettings;
        private RoundButton btnMusicToggle;
        private RoundButton btnExitGame;
        private RoundButton btnResumeSettings;

        private PixelCardPanel upgradePanel;
        private Label lblUpgradeInfo;
        private RoundButton btnUpgrade;
        private RoundButton btnCloseUpgrade;

        private GameModalOverlay activeModalOverlay;

        private bool keyLeft, keyRight, keyUp, keyDown;
        private bool interactHeld;
        private bool isSorting;
        private bool isShopOpen;
        private bool isUpgradeOpen;
        private Random rnd;
        
        private int currentStage;
		private string currentMap;

		// LEVEL PROGRESSION VARIABLES
		private int levelTimeRemaining;
		private int levelSortGoal;      // Goal is now based on CORRECTLY SORTED trash
		private int trashSortedCorrectly; // Counts correct sorts (not pickups)
		private bool levelComplete;
		private int currentGameLevel;
		
		private Image mapBackground;
		
		private SoundPlayer backgroundMusic;
		private bool musicOn = true;
		
		

		private string[] trashNames = { 
		    "Banana Peel", 
		    "Paper",       
		    "Plastic Bottle", 
		    "Tin Can",     
		    "Styrofoam",   
		    "Battery",
		    "Newspaper",
		    "Glass Bottle",
		    "Plastic Bag"
		};
		
		private string[] trashCategories = { 
		    "bio",      // Banana Peel
		    "recycle",  // Paper
		    "recycle",  // Plastic Bottle
		    "recycle",  // Tin Can
		    "nonbio",   // Styrofoam
		    "nonbio",   // Battery
		    "recycle",  // Newspaper
		    "recycle",  // Glass Bottle
		    "nonbio"    // Plastic Bag
		};

       	public GameForm(int stage)
		{
		    InitializeComponent();

		    this.WindowState = FormWindowState.Maximized;
		    this.KeyPreview = true;
		    this.DoubleBuffered = true;
		    this.AutoScaleMode = AutoScaleMode.None;
		    this.BackColor = UiTheme.Cream;
		
		    currentStage = stage;
		
		    rnd = new Random();
		    manager = new GameManager();
		    trashList = new List<TrashItem>();
		    trashLabels = new List<Label>();
		    npcs = new List<NPC>();
		
		    LoadStageData(currentStage); 
		    
		    try
		    {
		        backgroundMusic = new SoundPlayer(AssetPath("backGroundMusic.wav"));
		        backgroundMusic.PlayLooping();  
		    }
		    catch (Exception ex)
		    {
		        MessageBox.Show("Music file not found: " + ex.Message);
		    }
		
		    SetupHUD();
		    SetupNPCs();
		    SetupPlayer();
		    SetupSortPanel();
		    SetupShopPanel();
		    SetupUpgradePanel();
		    SetupTimer();
		
		    this.Load += new EventHandler(GameForm_Load);
		}
       	
       	protected override void OnFormClosing(FormClosingEventArgs e)
		{
		    if (backgroundMusic != null)
		    {
		        backgroundMusic.Stop();
		        backgroundMusic.Dispose();
		    }

		    if (settingsIcon != null)
		    {
		        settingsIcon.Dispose();
		        settingsIcon = null;
		    }

		    base.OnFormClosing(e);
		}

        // Always resolve game assets next to the executable, not the current
        // working directory (which changes when launched from a shortcut/IDE).
        private static string AssetPath(string fileName)
        {
            return System.IO.Path.Combine(Application.StartupPath, fileName);
        }

        void GameForm_Load(object sender, EventArgs e)
        {
        	ApplyMapBackground(currentStage);
        	LayoutGameUI();
		    SpawnTrash();
		    InitializeLevel(1);
		    UpdateHUD();
		    BringGameUiToFront();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (IsHandleCreated)
            {
                LayoutGameUI();
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            LayoutGameUI();
            BringGameUiToFront();
        }

        private void LayoutGameUI()
        {
            if (ClientSize.Width <= 0 || ClientSize.Height <= 0)
            {
                return;
            }

            int margin = Math.Max(16, Math.Min(28, ClientSize.Width / 60));
            int top = margin;
            int signWidth = Math.Max(250, Math.Min(320, ClientSize.Width / 5));
            int signHeight = 88;

            if (stageSign != null)
            {
                stageSign.Bounds = new Rectangle(margin, top, signWidth, signHeight);
            }

            int gearSize = ClientSize.Width < 1000 ? 44 : 50;
            if (btnSettings != null)
            {
                btnSettings.Size = new Size(gearSize, gearSize);
                btnSettings.Location = new Point(
                    ClientSize.Width - margin - gearSize,
                    top + (signHeight - gearSize) / 2);
            }

            if (hud != null)
            {
                int hudX = margin + signWidth + 14;
                int hudRight = ClientSize.Width - margin - gearSize - 14;
                int hudWidth = Math.Max(300, hudRight - hudX);
                hud.Bounds = new Rectangle(hudX, top, hudWidth, signHeight);
            }

            if (inventoryDock != null)
            {
                int dockWidth = Math.Min(460, Math.Max(340, ClientSize.Width / 3));
                int dockHeight = 124;
                inventoryDock.Bounds = new Rectangle(
                    (ClientSize.Width - dockWidth) / 2,
                    ClientSize.Height - margin - dockHeight,
                    dockWidth,
                    dockHeight);
            }

            if (toast != null)
            {
                int toastWidth = Math.Min(620, ClientSize.Width - 80);
                toast.Bounds = new Rectangle(
                    (ClientSize.Width - toastWidth) / 2,
                    top + signHeight + 12,
                    toastWidth,
                    46);
            }

            CenterPanelIfVisible(settingsPanel);
            CenterPanelIfVisible(sortPanel);
            CenterPanelIfVisible(shopPanel);
            CenterPanelIfVisible(upgradePanel);
        }

        private void CenterPanelIfVisible(Control panel)
        {
            if (panel != null && panel.Visible)
            {
                panel.Location = new Point(
                    (ClientSize.Width - panel.Width) / 2,
                    (ClientSize.Height - panel.Height) / 2);
            }
        }

        private void BringGameUiToFront()
        {
            if (stageSign != null) stageSign.BringToFront();
            if (hud != null) hud.BringToFront();
            if (inventoryDock != null) inventoryDock.BringToFront();
            if (toast != null) toast.BringToFront();
            if (btnSettings != null) btnSettings.BringToFront();
        }
        
        private void LoadStageData(int stage)
		{
		    switch (stage)
		    {
		        case 1:
		            currentMap = "Park";
		            this.Text = "EcoSort — Stage 1: Park";
		            break;
		        case 2:
		            currentMap = "Street";
		            this.Text = "EcoSort — Stage 2: Street";
		            break;
		        default:
		            currentMap = "Park";
		            this.Text = "EcoSort — Stage 1: Park";
		            break;
		    }
		}
        
        // Timer
        private void SetupTimer()
        {
            gameTimer = new Timer();
            gameTimer.Interval = 20;
            gameTimer.Tick += new EventHandler(Timer1Tick);
            gameTimer.Start();

            levelTimer = new Timer();
            levelTimer.Interval = 1000;
            levelTimer.Tick += new EventHandler(LevelTimerTick);
            levelTimer.Start();
            SetupSettingsButton();
        }

        void Timer1Tick(object sender, EventArgs e)
        {
            MovePlayer();
            CheckCollectTrash();
            CheckNPCHighlight();
            UpdateHUD();
            foreach (TrashItem t in trashList)
			{
			    t.Animate();
			}
        }

        void LevelTimerTick(object sender, EventArgs e)
        {
            if (levelComplete) return;
            
            levelTimeRemaining--;
            UpdateHUD();
            
            if (levelTimeRemaining <= 0)
            {
                EndLevel(false);
            }
        }

        private void InitializeLevel(int level)
        {
            currentGameLevel = level;
            manager.Level = level;       // keep scoring and the HUD in sync
            trashSortedCorrectly = 0; // Reset correct sort count per level
            levelComplete = false;
            
           
            switch (level)
			{
			    case 1:
			        levelTimeRemaining = 80;   
			        levelSortGoal = 14;
			        break;
			
			    case 2:
			        levelTimeRemaining = 65;  
			        levelSortGoal = 10;
			        break;
			
			    case 3:
			        levelTimeRemaining = 50;   
			        levelSortGoal = 10;
			        break;
			
			    case 4:
			        levelTimeRemaining = 40;   
			        levelSortGoal = 10;
			        break;
			
			    case 5:
			        levelTimeRemaining = 35;   
			        levelSortGoal = 10;
			        break;
			}
            
            UpdateStageSign();
            UpdateHUD();
            ShowFeedback("Level " + level + " - Correctly sort " + levelSortGoal + " trash in 3 minutes!", UiTheme.Sunshine);
        }

        private void UpdateStageSign()
        {
            if (stageSign == null)
            {
                return;
            }

            string mapName = string.IsNullOrEmpty(currentMap)
                ? "PARK"
                : currentMap.ToUpperInvariant();

            stageSign.Title = "THE " + mapName;
            stageSign.Subtitle = "LEVEL " + currentGameLevel + "  \u2022  GOAL " + levelSortGoal;
        }

        private void EndLevel(bool success)
        {
            levelComplete = true;
            gameTimer.Stop();
            levelTimer.Stop();
            
            sortPanel.Visible = false;
    		isSorting = false;
            
            if (success && currentGameLevel >= 3)
            {
                OpenWinForm();
            }
            else if (success && currentGameLevel < 3)
            {
                ShowLevelCompletePopup(currentGameLevel);
            }
            else
            {
                OpenGameOverForm();
            }
        }

        // -------------------------------------------------------
        // LEVEL COMPLETE POPUP (no reset, just advance to next level)f
        // -------------------------------------------------------
        private void ShowLevelCompletePopup(int completedLevel)
        {
            gameTimer.Stop();
            levelTimer.Stop();

            PixelCardPanel card = CreateModalCard(
                "LEVEL " + completedLevel + " COMPLETE!",
                "NICE WORK, VOLUNTEER",
                UiTheme.Leaf,
                new Size(560, 400));

            Label lblStats = CreateCardLabel(
                "CORRECTLY SORTED   " + trashSortedCorrectly + " / " + levelSortGoal
                + "\nSCORE   " + manager.Score + " PTS",
                UiTheme.Body(12f, null, FontStyle.Bold),
                UiTheme.InkSoft,
                ContentAlignment.MiddleCenter);
            lblStats.Location = new Point(30, 128);
            lblStats.Size = new Size(500, 74);

            PixelTag lblNext = new PixelTag();
            lblNext.Text = "LEVEL " + (completedLevel + 1) + " IS READY";
            lblNext.PixelFontSize = 10f;
            lblNext.FillColor = UiTheme.Sunshine;
            lblNext.ForeColor = UiTheme.Ink;
            lblNext.BorderColor = UiTheme.Ink;
            lblNext.Location = new Point((card.Width - lblNext.Width) / 2, 220);
            lblNext.AccessibleName = "Next level ready";

            RoundButton btnContinue = CreatePixelButton("CONTINUE", UiTheme.Leaf,
                UiTheme.Cream, UiTheme.LeafBright, 15f);
            btnContinue.Size = new Size(280, 58);
            btnContinue.Location = new Point((card.Width - btnContinue.Width) / 2, 300);
            btnContinue.AccessibleName = "Continue to next level";
            btnContinue.Click += delegate
            {
                CloseActiveModal();
                InitializeLevel(completedLevel + 1);
                SpawnTrash();
                UpdateHUD();
                gameTimer.Start();
                levelTimer.Start();
            };

            card.Controls.Add(lblStats);
            card.Controls.Add(lblNext);
            card.Controls.Add(btnContinue);
        }
        private void OpenWinForm()
        {
            if (backgroundMusic != null)
            {
                backgroundMusic.Stop();
            }

            try
            {
                SoundPlayer winSound = new SoundPlayer(AssetPath("youwin.wav"));
                winSound.Play();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Win sound not found: " + ex.Message);
            }

            PixelCardPanel card = CreateModalCard(
                "ALL CLEAN!",
                "YOU RESTORED THE PARK AND THE STREET",
                UiTheme.Sunshine,
                new Size(600, 440));

            Label lblWin = CreateCardLabel(
                "CONGRATULATIONS!\n\nYou completed all 3 levels.\n\nFINAL SCORE   " + manager.Score + " PTS",
                UiTheme.Body(14f, null, FontStyle.Bold),
                UiTheme.InkSoft,
                ContentAlignment.MiddleCenter);
            lblWin.Location = new Point(40, 126);
            lblWin.Size = new Size(520, 120);

            PixelTag lblHero = new PixelTag();
            lblHero.Text = "ECO HERO";
            lblHero.PixelFontSize = 11f;
            lblHero.FillColor = UiTheme.Sunshine;
            lblHero.ForeColor = UiTheme.Ink;
            lblHero.BorderColor = UiTheme.Ink;
            lblHero.Location = new Point((card.Width - lblHero.Width) / 2, 252);
            lblHero.AccessibleName = "Eco hero";

            RoundButton btnClose = CreatePixelButton("GO BACK", UiTheme.Leaf,
                UiTheme.Cream, UiTheme.LeafBright, 15f);
            btnClose.Size = new Size(260, 58);
            btnClose.Location = new Point((card.Width - btnClose.Width) / 2, 330);
            btnClose.AccessibleName = "Go back to stage select";
            btnClose.Click += delegate
            {
                StartForm startForm = new StartForm();
                startForm.Show();
                this.Close();
            };

            card.Controls.Add(lblWin);
            card.Controls.Add(lblHero);
            card.Controls.Add(btnClose);
        }

        private void OpenGameOverForm()
        {
            PixelCardPanel card = CreateModalCard(
                "TIME'S UP!",
                "THE LITTER GOT AWAY THIS TIME",
                UiTheme.Coral,
                new Size(560, 400));

            Label lblGameOver = CreateCardLabel(
                "YOU SORTED   " + trashSortedCorrectly + " / " + levelSortGoal + " CORRECTLY\n\nFINAL SCORE   " + manager.Score + " PTS",
                UiTheme.Body(12f, null, FontStyle.Bold),
                UiTheme.InkSoft,
                ContentAlignment.MiddleCenter);
            lblGameOver.Location = new Point(30, 126);
            lblGameOver.Size = new Size(500, 90);

            RoundButton btnRetry = CreatePixelButton("RETRY LEVEL", UiTheme.Leaf,
                UiTheme.Cream, UiTheme.LeafBright, 13f);
            btnRetry.Size = new Size(240, 56);
            btnRetry.Location = new Point(30, 300);
            btnRetry.AccessibleName = "Retry level";
            btnRetry.Click += delegate
            {
                CloseActiveModal();
                InitializeLevel(currentGameLevel);
                SpawnTrash();
                UpdateHUD();
                gameTimer.Start();
                levelTimer.Start();
            };

            RoundButton btnQuit = CreatePixelButton("QUIT GAME", UiTheme.Coral,
                UiTheme.Cream, Color.FromArgb(255, 132, 116), 13f);
            btnQuit.Size = new Size(240, 56);
            btnQuit.Location = new Point(290, 300);
            btnQuit.AccessibleName = "Quit game";
            btnQuit.Click += delegate
            {
                this.Close();
            };

            card.Controls.Add(lblGameOver);
            card.Controls.Add(btnRetry);
            card.Controls.Add(btnQuit);
        }

        private PixelCardPanel CreateModalCard(string title, string subtitle, Color accent, Size size)
        {
            CloseActiveModal();

            GameModalOverlay overlay = new GameModalOverlay();
            overlay.Dock = DockStyle.Fill;
            Controls.Add(overlay);

            PixelCardPanel card = new PixelCardPanel();
            card.Size = size;
            card.Title = title;
            card.Subtitle = subtitle;
            card.AccentColor = accent;
            card.Location = new Point(
                Math.Max(0, (ClientSize.Width - size.Width) / 2),
                Math.Max(0, (ClientSize.Height - size.Height) / 2));

            overlay.Controls.Add(card);
            overlay.Resize += delegate
            {
                card.Location = new Point(
                    Math.Max(0, (overlay.ClientSize.Width - card.Width) / 2),
                    Math.Max(0, (overlay.ClientSize.Height - card.Height) / 2));
            };

            activeModalOverlay = overlay;
            overlay.BringToFront();
            card.BringToFront();
            return card;
        }

        private void CloseActiveModal()
        {
            if (activeModalOverlay == null)
            {
                return;
            }

            Controls.Remove(activeModalOverlay);
            activeModalOverlay.Dispose();
            activeModalOverlay = null;
        }

        private static Label CreateCardLabel(string text, Font font, Color color, ContentAlignment alignment)
        {
            Label label = new Label();
            label.Text = text;
            label.Font = font;
            label.ForeColor = color;
            label.BackColor = UiTheme.Cream;
            label.AutoSize = false;
            label.TextAlign = alignment;
            label.UseMnemonic = false;
            return label;
        }

        private static RoundButton CreatePixelButton(string text, Color fill, Color foreground,
            Color hover, float pixelFontSize)
        {
            RoundButton button = new RoundButton();
            button.Text = text;
            button.BackColor = fill;
            button.ForeColor = foreground;
            button.HoverBackColor = hover;
            button.BorderColor = UiTheme.Ink;
            button.BorderWidth = 2.5f;
            button.Radius = 20;
            button.PixelFontSize = pixelFontSize;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.UseVisualStyleBackColor = false;
            button.Cursor = Cursors.Hand;
            button.TabStop = true;
            return button;
        }

        private static RoundButton CreateCloseButton()
        {
            RoundButton button = CreatePixelButton("X", UiTheme.Cream, UiTheme.Ink,
                UiTheme.Sunshine, 12f);
            button.Size = new Size(40, 40);
            button.Radius = 20;
            button.BorderWidth = 2f;
            button.AccessibleName = "Close";
            return button;
        }
        private void ApplyMapBackground(int stage)
		{
	    try
	    {
	        if (stage == 1) 
	        {
	            string mapPath = AssetPath("park2.png");
	            
	            if (System.IO.File.Exists(mapPath))
	            {
	                mapBackground = Image.FromFile(mapPath);
	                this.BackgroundImage = mapBackground;
	             this.BackgroundImageLayout = ImageLayout.Stretch;   
	            }
	            else
	            {
	                MessageBox.Show("park.png not found in bin/Debug folder!");
	                this.BackColor = Color.DarkGreen;
	            }
	        }
	        else if (stage == 2) 
	        {
	            string mapPath = AssetPath("mapstreet.png");
	            
	            if (System.IO.File.Exists(mapPath))
	            {
	                mapBackground = Image.FromFile(mapPath);
	                this.BackgroundImage = mapBackground;
	             this.BackgroundImageLayout = ImageLayout.Stretch;   
	            }
	            else
	            {
	                MessageBox.Show("park.png not found in bin/Debug folder!");
	                this.BackColor = Color.DarkGreen;
	            }
	        }
	    }
	    catch (Exception ex)
	    {
	        MessageBox.Show("Error loading map: " + ex.Message);
	        this.BackColor = Color.DarkGreen;
	    }
}
        private void SetupSettingsButton()
		{
		    btnSettings = new RoundButton();
		    btnSettings.Size = new Size(50, 50);
		    btnSettings.Text = string.Empty;
		    btnSettings.AccessibleName = "Settings";
		    btnSettings.AccessibleRole = AccessibleRole.PushButton;
		    btnSettings.BackColor = UiTheme.Cream;
		    btnSettings.ForeColor = UiTheme.Ink;
		    btnSettings.BorderColor = UiTheme.Ink;
		    btnSettings.BorderWidth = 3f;
		    btnSettings.HoverBackColor = UiTheme.Sunshine;
		    btnSettings.Radius = 25;
		    btnSettings.FlatStyle = FlatStyle.Flat;
		    btnSettings.FlatAppearance.BorderSize = 0;
		    btnSettings.UseVisualStyleBackColor = false;
		    btnSettings.Cursor = Cursors.Hand;
		    btnSettings.TabStop = true;

		    string iconPath = AssetPath("setting.png");
		    if (System.IO.File.Exists(iconPath))
		    {
		        try
		        {
		            settingsIcon = Image.FromFile(iconPath);
		            btnSettings.Image = settingsIcon;
		        }
		        catch
		        {
		            settingsIcon = null;
		        }
		    }

		    if (btnSettings.Image == null)
		    {
		        btnSettings.Text = "SET";
		        btnSettings.PixelFontSize = 9f;
		    }

		    btnSettings.Click += new EventHandler(BtnSettingsClick);
		    this.Controls.Add(btnSettings);

		    settingsPanel = new PixelCardPanel();
		    settingsPanel.Size = new Size(520, 430);
		    settingsPanel.Title = "PAUSED";
		    settingsPanel.Subtitle = "SETTINGS  \u2022  TAKE A BREATH";
		    settingsPanel.AccentColor = UiTheme.Leaf;
		    settingsPanel.Visible = false;
		    settingsPanel.AccessibleName = "Paused settings";

		    btnCloseSettings = CreateCloseButton();
		    btnCloseSettings.Location = new Point(456, 18);
		    btnCloseSettings.Click += delegate { HideSettings(); };

		    btnMusicToggle = CreatePixelButton("MUSIC  ON", UiTheme.Leaf, UiTheme.Cream,
		        UiTheme.LeafBright, 14f);
		    btnMusicToggle.Location = new Point(40, 128);
		    btnMusicToggle.Size = new Size(440, 60);
		    btnMusicToggle.AccessibleName = "Toggle music";
		    btnMusicToggle.Click += delegate { ToggleMusic(); };

		    btnExitGame = CreatePixelButton("EXIT GAME", UiTheme.Coral, UiTheme.Cream,
		        Color.FromArgb(255, 132, 116), 14f);
		    btnExitGame.Location = new Point(40, 204);
		    btnExitGame.Size = new Size(440, 60);
		    btnExitGame.AccessibleName = "Exit game";
		    btnExitGame.Click += new EventHandler(BtnExitGameClick);

		    btnResumeSettings = CreatePixelButton("RESUME", UiTheme.Cream, UiTheme.Ink,
		        UiTheme.Sunshine, 14f);
		    btnResumeSettings.Location = new Point(40, 310);
		    btnResumeSettings.Size = new Size(440, 60);
		    btnResumeSettings.AccessibleName = "Resume game";
		    btnResumeSettings.Click += delegate { HideSettings(); };

		    settingsPanel.Controls.Add(btnCloseSettings);
		    settingsPanel.Controls.Add(btnMusicToggle);
		    settingsPanel.Controls.Add(btnExitGame);
		    settingsPanel.Controls.Add(btnResumeSettings);

		    this.Controls.Add(settingsPanel);
		    UpdateMusicButton();
		}

        private void ToggleMusic()
        {
            musicOn = !musicOn;
            UpdateMusicButton();

            if (backgroundMusic == null)
            {
                return;
            }

            try
            {
                if (musicOn)
                {
                    backgroundMusic.PlayLooping();
                }
                else
                {
                    backgroundMusic.Stop();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Music error: " + ex.Message);
            }
        }

        private void UpdateMusicButton()
        {
            if (btnMusicToggle == null)
            {
                return;
            }

            if (musicOn)
            {
                btnMusicToggle.Text = "MUSIC  ON";
                btnMusicToggle.BackColor = UiTheme.Leaf;
                btnMusicToggle.ForeColor = UiTheme.Cream;
                btnMusicToggle.HoverBackColor = UiTheme.LeafBright;
            }
            else
            {
                btnMusicToggle.Text = "MUSIC  OFF";
                btnMusicToggle.BackColor = UiTheme.Cream;
                btnMusicToggle.ForeColor = UiTheme.Ink;
                btnMusicToggle.HoverBackColor = UiTheme.Sunshine;
            }

            btnMusicToggle.Invalidate();
        }

        private void HideSettings()
        {
            if (settingsPanel == null)
            {
                return;
            }

            settingsPanel.Visible = false;
            ResumeGame();
        }

        void BtnSettingsClick(object sender, EventArgs e)
		{
		    if (settingsPanel == null)
		    {
		        return;
		    }

		    if (settingsPanel.Visible)
		    {
		        HideSettings();
		        return;
		    }

		    if (isSorting || isShopOpen || isUpgradeOpen || levelComplete)
		    {
		        return;
		    }

		    gameTimer.Stop();
		    levelTimer.Stop();

		    settingsPanel.Location = new Point(
			    (this.ClientSize.Width - settingsPanel.Width) / 2,
			    (this.ClientSize.Height - settingsPanel.Height) / 2);
		    settingsPanel.Visible = true;
		    settingsPanel.BringToFront();
		    btnSettings.BringToFront();
		}
		
		void BtnExitGameClick(object sender, EventArgs e)
		{
		    gameTimer.Stop();
		    levelTimer.Stop();
		    this.Close();
		}
		// HUD
        private void SetupHUD()
        {
            stageSign = new PixelSign();
            stageSign.Title = "THE PARK";
            stageSign.Subtitle = "LEVEL 1  \u2022  GOAL 14";
            stageSign.AccessibleName = "Current stage";
            Controls.Add(stageSign);

            hud = new GameHud();
            Controls.Add(hud);

            inventoryDock = new InventoryDock();
            Controls.Add(inventoryDock);

            toast = new GameToast();
            toast.Visible = false;
            Controls.Add(toast);
        }

        private void UpdateHUD()
        {
            if (hud != null && manager != null && player != null)
            {
                hud.SetData(
                    manager.Score,
                    manager.Level,
                    levelTimeRemaining,
                    trashSortedCorrectly,
                    levelSortGoal,
                    player.CurrentToolName,
                    player.CountItems(),
                    player.MaxSlots);
            }

            if (inventoryDock != null && player != null)
            {
                inventoryDock.UpdateFrom(player);
            }
        }
        
        // NPCs
        private void SetupNPCs()
        {
            NPC sorter = new NPC(
		    "sorter",
		    "SORTING\nPRESS E",
		    320,
        	500,
		    "sortergif.gif",
		    UiTheme.Leaf
		);
            NPC upgrader = new NPC(
		    "upgrade",
		    "UPGRADE\nPRESS E",
		    720,
		    500,
		    "upgradegif.gif",
		    UiTheme.Sunshine
		);
			NPC shop = new NPC(
		    "shop",
		    "SHOP\nPRESS E",
		    1120,
        	500,
		    "shopgif.gif",
		    UiTheme.Sky
		);

            npcs.Add(sorter);
            npcs.Add(upgrader);
            npcs.Add(shop);

            foreach (NPC n in npcs)
            {
                this.Controls.Add(n.Picture);
                this.Controls.Add(n.NameLabel);
            }
        }
        
        private void OpenNPCNearPlayer()
        {
            if (levelComplete) return;
            if (settingsPanel != null && settingsPanel.Visible) return;
            if (isSorting || isShopOpen || isUpgradeOpen) return;

            foreach (NPC n in npcs)
            {
                if (player.Picture.Bounds.IntersectsWith(n.Picture.Bounds))
                {
                    if (n.Type == "sorter") OpenSortPanel();
                    if (n.Type == "upgrade") OpenUpgradePanel();
                    if (n.Type == "shop") OpenShopPanel();
                    return;
                }
            }
        }
        
        // Collect Trash — NO scoring here, just pickup
        private void CheckCollectTrash()
		{
		    if (!player.HasEmptySlot()) return;
		
		    TrashItem collected = null;
		
		    // Find item to collect WITHOUT modifying the list
		    foreach (TrashItem t in trashList)
		    {
		        if (t.Picture.Visible &&
		            player.Picture.Bounds.IntersectsWith(t.Picture.Bounds))
		        {
		            if (player.AddItem(t))
		            {
		                collected = t;
		                break;
		            }
		        }
		    }
		
		    // Now safely modify outside the foreach
		    if (collected != null)
		    {
		        collected.Picture.Visible = false;
		
		        foreach (Label lbl in trashLabels)
		        {
		            if (lbl.Tag == collected)
		            {
		                lbl.Visible = false;
		                break;
		            }
		        }
		
		        ShowFeedback("Picked: " + collected.Name + " — Go sort it!", UiTheme.Sky);
		
		        AddTrash(1); // Safe to call now — no longer inside foreach
		    }
		}

        // Player
        private void SetupPlayer()
        {
            player = new Player(500, 400);
            this.Controls.Add(player.Picture);
            player.Picture.BringToFront();
        }
        
        // Spawn Trash 
       private void SpawnTrash()
		{
		    foreach (Label lbl in trashLabels)
		        this.Controls.Remove(lbl);  // removes from form
		    trashLabels.Clear();
		
		    foreach (TrashItem t in trashList)
		        this.Controls.Remove(t.Picture);
		    trashList.Clear();
		
		    AddTrash(8);
		}
		        
        private void CheckAndRefillTrash()
		{
		    int visibleCount = 0;
		    foreach (TrashItem t in trashList)
		        if (t.Picture.Visible) visibleCount++;
		
		    if (visibleCount < 8)
		    {
		        int needed = 8 - visibleCount;
		        AddTrash(needed);
		    }
		}
        
		private void AddTrash(int count)
		{
		    int safeMaxX = this.ClientSize.Width  > 200 ? this.ClientSize.Width  - 120 : 700;
		    int safeMaxY = this.ClientSize.Height > 300 ? this.ClientSize.Height - 220 : 450;
		    int safeMinX = 60;
		    int safeMinY = 130;
		
		    if (safeMaxX <= safeMinX) safeMaxX = safeMinX + 200;
		    if (safeMaxY <= safeMinY) safeMaxY = safeMinY + 200;
		
		    for (int i = 0; i < count; i++)
		    {
		        int idx = rnd.Next(trashNames.Length);
		        int x   = rnd.Next(safeMinX, safeMaxX);
		        int y   = rnd.Next(safeMinY, safeMaxY);
				
		        TrashItem t = new TrashItem(trashNames[idx], trashCategories[idx], x, y); 

		        PixelTag lbl = new PixelTag();
		        lbl.Text = t.Name;
		        lbl.PixelFontSize = 8.5f;
		        lbl.FillColor = UiTheme.Cream;
		        lbl.ForeColor = UiTheme.Ink;
		        lbl.BorderColor = UiTheme.Ink;

		        int labelX = Math.Max(8, Math.Min(x, ClientSize.Width - lbl.Width - 12));
		        int labelY = Math.Max(8, Math.Min(y + 35, ClientSize.Height - lbl.Height - 8));
		        lbl.Location = new Point(labelX, labelY);
		        lbl.Tag = t;
		
		        this.Controls.Add(lbl);
		        this.Controls.Add(t.Picture);
		        trashLabels.Add(lbl);
		        trashList.Add(t);
		
		        lbl.BringToFront();
		        t.Picture.BringToFront();
		    }
		
		    player.Picture.BringToFront();
		    BringGameUiToFront();
		}
		
        // NPC Highlight
        private void CheckNPCHighlight()
        {
            foreach (NPC n in npcs)
            {
                bool near = player.Picture.Bounds.IntersectsWith(n.Picture.Bounds);
                n.NameLabel.Highlighted = near;
            }
        }

        private TrashItem GetNextItemToSort()
        {
            for (int i = 0; i < player.MaxSlots; i++)
                if (!player.Inventory[i].IsEmpty) return player.Inventory[i].Item;
            return null;
        }

        private void RemoveNextItemFromInventory()
        {
            for (int i = 0; i < player.MaxSlots; i++)
            {
                if (!player.Inventory[i].IsEmpty)
                {
                    player.Inventory[i].Item = null;
                    return;
                }
            }
        }

        // -------------------------------------------------------
        // SORT PROCESSING — scoring AND level progression happen here
        // -------------------------------------------------------
        private void ProcessSort(string chosenBin)
        {
            TrashItem item = GetNextItemToSort();
            if (item == null) return;

           if (item.Category.Trim().ToLower() == chosenBin.Trim().ToLower())
            {
                // Score added ONLY on correct sort
                manager.CorrectSort();
                trashSortedCorrectly++; // Increment level progress

                int pointsEarned = 10 * manager.Level;
                ShowFeedback("Correct! +" + pointsEarned + " pts  [" + trashSortedCorrectly + "/" + levelSortGoal + "]", UiTheme.LeafBright);

                // Check if level goal is reached
                if (trashSortedCorrectly >= levelSortGoal && !levelComplete)
                {
                    RemoveNextItemFromInventory();
                    UpdateHUD();
                    EndLevel(true);
                    return;
                }
            }
            else
            {
                // Wrong sort — no points, no progress. Keep the sort panel open
                // and the game paused; the previous code restarted both timers
                // here, letting the countdown run behind the modal panel.
                manager.WrongSort();
                ShowFeedback("Wrong! " + item.Name + " belongs in: " + item.Category, UiTheme.Coral);
            }

            RemoveNextItemFromInventory();

            TrashItem next = GetNextItemToSort();
            if (next != null)
            {
                ShowSortItem(next);
            }
           else
			{
			    sortPanel.Visible = false;
			    isSorting = false;
			    ResumeGame();
			}

            UpdateHUD();
        }

        void BtnBioClick(object sender, EventArgs e) { ProcessSort("bio");    }
        void BtnNonBioClick(object sender, EventArgs e) { ProcessSort("nonbio"); }
        void BtnRecycleClick(object sender, EventArgs e) { ProcessSort("recycle");}

        // Pause/resume helpers: never restart timers once the level is over.
        private void ResumeGame()
        {
            if (levelComplete) return;
            gameTimer.Start();
            levelTimer.Start();
        }

        void BtnCancelSortClick(object sender, EventArgs e)
        {
            sortPanel.Visible = false;
            isSorting = false;
            ResumeGame();
            ShowFeedback("Sorting cancelled", UiTheme.Sunshine);
        }



        void BtnUpgradeClick(object sender, EventArgs e)
        {
            int cost = player.NextUpgradeCost();
            if (manager.Score >= cost)
            {
                manager.SpendScore(cost);
                player.UpgradeTool();
                ShowFeedback("Upgraded to " + player.CurrentToolName + "!", UiTheme.Sunshine);
                OpenUpgradePanel();
            }
            else
            {
                ShowFeedback("Not enough points!", UiTheme.Coral);
            }
        }

        void BtnCloseUpgradeClick(object sender, EventArgs e)
        {
            upgradePanel.Visible = false;
            isUpgradeOpen = false;
            ResumeGame();
        }
        
        // SORT PANEL
        private void SetupSortPanel()
        {
            sortPanel = new PixelCardPanel();
            sortPanel.Size = new Size(620, 452);
            sortPanel.Title = "SORTING STATION";
            sortPanel.Subtitle = "MATCH THE ITEM TO THE RIGHT BIN";
            sortPanel.AccentColor = UiTheme.Leaf;
            sortPanel.Visible = false;
            sortPanel.AccessibleName = "Sorting station";

            sortPreview = new ItemPreview();
            sortPreview.Size = new Size(180, 170);
            sortPreview.Location = new Point(30, 124);
            sortPreview.AccessibleName = "Item preview";

            lblSortItem = new Label();
            lblSortItem.Text = "ITEM";
            lblSortItem.Font = UiTheme.Pixel(17f, null);
            lblSortItem.ForeColor = UiTheme.Ink;
            lblSortItem.BackColor = UiTheme.Cream;
            lblSortItem.AutoSize = false;
            lblSortItem.Location = new Point(238, 128);
            lblSortItem.Size = new Size(350, 42);
            lblSortItem.TextAlign = ContentAlignment.MiddleLeft;
            lblSortItem.UseMnemonic = false;

            lblSortQuestion = new Label();
            lblSortQuestion.Text = "Which bin does it belong to?";
            lblSortQuestion.Font = UiTheme.Body(11.5f, null, FontStyle.Regular);
            lblSortQuestion.ForeColor = UiTheme.InkSoft;
            lblSortQuestion.BackColor = UiTheme.Cream;
            lblSortQuestion.AutoSize = false;
            lblSortQuestion.Location = new Point(238, 178);
            lblSortQuestion.Size = new Size(350, 54);
            lblSortQuestion.TextAlign = ContentAlignment.TopLeft;
            lblSortQuestion.UseMnemonic = false;

            btnBio = CreatePixelButton("BIO", UiTheme.Leaf, UiTheme.Cream,
                UiTheme.LeafBright, 18f);
            btnBio.Location = new Point(30, 312);
            btnBio.Size = new Size(180, 58);
            btnBio.AccessibleName = "Biodegradable bin";
            btnBio.Click += new EventHandler(BtnBioClick);

            btnNonBio = CreatePixelButton("NON-BIO", UiTheme.Coral, UiTheme.Cream,
                Color.FromArgb(255, 132, 116), 16f);
            btnNonBio.Location = new Point(220, 312);
            btnNonBio.Size = new Size(180, 58);
            btnNonBio.AccessibleName = "Non-biodegradable bin";
            btnNonBio.Click += new EventHandler(BtnNonBioClick);

            btnRecycle = CreatePixelButton("RECYCLE", UiTheme.Sky, UiTheme.Cream,
                Color.FromArgb(114, 193, 238), 16f);
            btnRecycle.Location = new Point(410, 312);
            btnRecycle.Size = new Size(180, 58);
            btnRecycle.AccessibleName = "Recyclable bin";
            btnRecycle.Click += new EventHandler(BtnRecycleClick);

            btnCancelSort = CreatePixelButton("CANCEL", UiTheme.Cream, UiTheme.Ink,
                UiTheme.Sunshine, 13f);
            btnCancelSort.Location = new Point(30, 384);
            btnCancelSort.Size = new Size(560, 46);
            btnCancelSort.AccessibleName = "Cancel sorting";
            btnCancelSort.Click += new EventHandler(BtnCancelSortClick);

            sortPanel.Controls.Add(sortPreview);
            sortPanel.Controls.Add(lblSortItem);
            sortPanel.Controls.Add(lblSortQuestion);
            sortPanel.Controls.Add(btnBio);
            sortPanel.Controls.Add(btnNonBio);
            sortPanel.Controls.Add(btnRecycle);
            sortPanel.Controls.Add(btnCancelSort);

            this.Controls.Add(sortPanel);
        }

        private void ShowSortItem(TrashItem item)
        {
            if (item == null)
            {
                sortPreview.SetItem(null, string.Empty);
                lblSortItem.Text = string.Empty;
                lblSortQuestion.Text = "Which bin does it belong to?";
                return;
            }

            Image itemImage = item.Picture == null ? null : item.Picture.Image;
            sortPreview.SetItem(itemImage, item.Name);
            lblSortItem.Text = (item.Name ?? string.Empty).ToUpperInvariant();
            lblSortQuestion.Text = "Which bin does it belong to?";
        }

        private void OpenSortPanel()
		{
		    if (player.CountItems() == 0)
		    {
		        ShowFeedback("Inventory empty! Collect trash first.", UiTheme.Sunshine);
		        return;
		    }
		    
		    TrashItem next = GetNextItemToSort();
		    if (next != null)
		    {
		        ShowSortItem(next);
		        isSorting = true;
		        
		        sortPanel.Location = new Point(
		            (this.ClientSize.Width - sortPanel.Width) / 2,
		            (this.ClientSize.Height - sortPanel.Height) / 2);
		        
		        sortPanel.Visible = true;
		        sortPanel.BringToFront();
		        
		        gameTimer.Stop();
		        levelTimer.Stop();
		    }
		}
        
        // UPGRADE PANEL
        private void SetupUpgradePanel()
        {
            upgradePanel = new PixelCardPanel();
            upgradePanel.Size = new Size(560, 360);
            upgradePanel.Title = "UPGRADE STATION";
            upgradePanel.Subtitle = "CARRY MORE  \u2022  CLEAN FASTER";
            upgradePanel.AccentColor = UiTheme.Sunshine;
            upgradePanel.Visible = false;
            upgradePanel.AccessibleName = "Upgrade station";

            lblUpgradeInfo = new Label();
            lblUpgradeInfo.Text = string.Empty;
            lblUpgradeInfo.Font = UiTheme.Body(11.5f, null, FontStyle.Regular);
            lblUpgradeInfo.ForeColor = UiTheme.InkSoft;
            lblUpgradeInfo.BackColor = UiTheme.Cream;
            lblUpgradeInfo.AutoSize = false;
            lblUpgradeInfo.Location = new Point(30, 122);
            lblUpgradeInfo.Size = new Size(500, 116);
            lblUpgradeInfo.TextAlign = ContentAlignment.TopLeft;
            lblUpgradeInfo.UseMnemonic = false;

            btnUpgrade = CreatePixelButton("UPGRADE TOOL", UiTheme.Leaf, UiTheme.Cream,
                UiTheme.LeafBright, 15f);
            btnUpgrade.Location = new Point(30, 260);
            btnUpgrade.Size = new Size(500, 60);
            btnUpgrade.AccessibleName = "Upgrade tool";
            btnUpgrade.Click += new EventHandler(BtnUpgradeClick);

            btnCloseUpgrade = CreateCloseButton();
            btnCloseUpgrade.Location = new Point(496, 18);
            btnCloseUpgrade.Click += new EventHandler(BtnCloseUpgradeClick);

            upgradePanel.Controls.Add(lblUpgradeInfo);
            upgradePanel.Controls.Add(btnUpgrade);
            upgradePanel.Controls.Add(btnCloseUpgrade);

            this.Controls.Add(upgradePanel);
        }
        
       	private void OpenUpgradePanel()
		{
		    gameTimer.Stop();
		    levelTimer.Stop();
		
		    isUpgradeOpen = true;
		
		    if (player.ToolLevel >= 2)
		    {
		        lblUpgradeInfo.Text =
		            "CURRENT TOOL    " + player.CurrentToolName.ToUpperInvariant()
		            + "  (" + player.MaxSlots + " SLOTS)\n"
		            + "ALL SLOTS UNLOCKED\n"
		            + "YOUR SCORE      " + manager.Score + " PTS";
		        btnUpgrade.Text = "MAX LEVEL";
		        btnUpgrade.Enabled = false;
		        btnUpgrade.BackColor = UiTheme.CreamShade;
		        btnUpgrade.ForeColor = UiTheme.Ink;
		        btnUpgrade.BorderColor = UiTheme.InkSoft;
		        btnUpgrade.HoverBackColor = UiTheme.CreamShade;
		    }
		    else
		    {
		        int cost = player.NextUpgradeCost();
		
		        lblUpgradeInfo.Text =
		              "CURRENT TOOL    " + player.CurrentToolName.ToUpperInvariant()
		            + "  (" + player.MaxSlots + " SLOT" + (player.MaxSlots == 1 ? "" : "S") + ")\n"
		            + "NEXT UPGRADE    " + cost + " PTS\n"
		            + "YOUR SCORE      " + manager.Score + " PTS\n"
		            + (manager.Score >= cost ? "READY TO UPGRADE!" : "NEED MORE POINTS.");

		        if (manager.Score >= cost)
		        {
		            btnUpgrade.Text = "UPGRADE  \u2022  " + cost + " PTS";
		            btnUpgrade.Enabled = true;
		            btnUpgrade.BackColor = UiTheme.Leaf;
		            btnUpgrade.ForeColor = UiTheme.Cream;
		            btnUpgrade.BorderColor = UiTheme.Ink;
		            btnUpgrade.HoverBackColor = UiTheme.LeafBright;
		        }
		        else
		        {
		            btnUpgrade.Text = "NEED " + (cost - manager.Score) + " MORE PTS";
		            btnUpgrade.Enabled = false;
		            btnUpgrade.BackColor = UiTheme.CreamShade;
		            btnUpgrade.ForeColor = UiTheme.Ink;
		            btnUpgrade.BorderColor = UiTheme.InkSoft;
		            btnUpgrade.HoverBackColor = UiTheme.CreamShade;
		        }
		    }
		
		    upgradePanel.Location = new Point(
		        (this.ClientSize.Width - upgradePanel.Width) / 2,
		        (this.ClientSize.Height - upgradePanel.Height) / 2);
		
		    upgradePanel.Visible = true;
		    upgradePanel.BringToFront();
		}

        // SHOP PANEL
        private void SetupShopPanel()
        {
            shopPanel = new PixelCardPanel();
            shopPanel.Size = new Size(560, 360);
            shopPanel.Title = "SUPPLY SHOP";
            shopPanel.Subtitle = "SPEND POINTS  \u2022  CLEAN SMARTER";
            shopPanel.AccentColor = UiTheme.Sky;
            shopPanel.Visible = false;
            shopPanel.AccessibleName = "Supply shop";

            lblShopInfo = new Label();
            lblShopInfo.Text = "YOUR SCORE  0 PTS";
            lblShopInfo.Font = UiTheme.Pixel(13f, null);
            lblShopInfo.ForeColor = UiTheme.Ink;
            lblShopInfo.BackColor = UiTheme.Cream;
            lblShopInfo.AutoSize = false;
            lblShopInfo.Location = new Point(30, 120);
            lblShopInfo.Size = new Size(500, 30);
            lblShopInfo.TextAlign = ContentAlignment.MiddleLeft;
            lblShopInfo.UseMnemonic = false;

            lblShopItem = new Label();
            lblShopItem.Text = "SPEED BOOST";
            lblShopItem.Font = UiTheme.Pixel(17f, null);
            lblShopItem.ForeColor = UiTheme.Leaf;
            lblShopItem.BackColor = UiTheme.Cream;
            lblShopItem.AutoSize = false;
            lblShopItem.Location = new Point(30, 166);
            lblShopItem.Size = new Size(500, 34);
            lblShopItem.TextAlign = ContentAlignment.MiddleLeft;
            lblShopItem.UseMnemonic = false;

            lblShopDesc = new Label();
            lblShopDesc.Text = "+2 MOVEMENT SPEED  \u2022  COST 30 PTS";
            lblShopDesc.Font = UiTheme.Body(10.5f, null, FontStyle.Regular);
            lblShopDesc.ForeColor = UiTheme.InkSoft;
            lblShopDesc.BackColor = UiTheme.Cream;
            lblShopDesc.AutoSize = false;
            lblShopDesc.Location = new Point(30, 202);
            lblShopDesc.Size = new Size(500, 26);
            lblShopDesc.TextAlign = ContentAlignment.MiddleLeft;
            lblShopDesc.UseMnemonic = false;

            btnBuySpeed = CreatePixelButton("BUY SPEED BOOST  \u2022  30 PTS",
                UiTheme.Leaf, UiTheme.Cream, UiTheme.LeafBright, 13f);
            btnBuySpeed.Location = new Point(30, 258);
            btnBuySpeed.Size = new Size(500, 58);
            btnBuySpeed.AccessibleName = "Buy speed boost";
            btnBuySpeed.Click += new EventHandler(BtnBuySpeedClick);

            btnCloseShop = CreateCloseButton();
            btnCloseShop.Location = new Point(496, 18);
            btnCloseShop.Click += new EventHandler(BtnCloseShopClick);

            shopPanel.Controls.Add(lblShopInfo);
            shopPanel.Controls.Add(lblShopItem);
            shopPanel.Controls.Add(lblShopDesc);
            shopPanel.Controls.Add(btnBuySpeed);
            shopPanel.Controls.Add(btnCloseShop);

            this.Controls.Add(shopPanel);
        }

        private void OpenShopPanel()
        {
        	gameTimer.Stop();
		    levelTimer.Stop();
        	
            isShopOpen = true;
            lblShopInfo.Text = "YOUR SCORE  " + manager.Score + " PTS";
            shopPanel.Location = new Point(
                (this.ClientSize.Width  - shopPanel.Width)  / 2,
                (this.ClientSize.Height - shopPanel.Height) / 2);
            shopPanel.Visible = true;
            shopPanel.BringToFront();
        }
		
        void BtnCloseShopClick(object sender, EventArgs e)
        {
            shopPanel.Visible = false;
            isShopOpen = false;
            ResumeGame();
        }

        void BtnBuySpeedClick(object sender, EventArgs e)
        {
            if (manager.Score >= 30)
            {
                manager.SpendScore(30);
                player.Speed += 2;
                lblShopInfo.Text = "YOUR SCORE  " + manager.Score + " PTS";
                UpdateHUD();
                ShowFeedback("Speed Boosted!", UiTheme.Sky);
            }
            else
            {
                ShowFeedback("Not enough points!", UiTheme.Coral);
            }
        }

        // Feedback
        private void ShowFeedback(string msg, Color color)
        {
            if (toast != null)
            {
                toast.ShowMessage(msg, color);
                toast.BringToFront();
            }

            if (hideTimer != null)
            {
                hideTimer.Stop();
                hideTimer.Dispose();
            }
            hideTimer = new Timer();
            hideTimer.Interval = 1800;
            hideTimer.Tick += new EventHandler(HideFeedbackTick);
            hideTimer.Start();
        }

        void HideFeedbackTick(object sender, EventArgs e)
        {
            if (toast != null)
            {
                toast.HideMessage();
            }

            hideTimer.Stop();
        }
        
        // Movement
		private void MovePlayer()
		{
		    float dx = 0, dy = 0;
		    if (keyLeft)  dx -= 1;
		    if (keyRight) dx += 1;
		    if (keyUp)    dy -= 1;
		    if (keyDown)  dy += 1;
		
		    if (dx == 0 && dy == 0) return;
		
		    // Clamp to bounds BEFORE moving
		    float len = (float)Math.Sqrt(dx * dx + dy * dy);
		    float nx = dx / len;
		    float ny = dy / len;
		
		    int newX = player.Picture.Left  + (int)Math.Round(nx * player.Speed);
		    int newY = player.Picture.Top   + (int)Math.Round(ny * player.Speed);
		
		    // Horizontal bounds
		    if (newX < 0) newX = 0;
		    if (newX + player.Picture.Width > this.ClientSize.Width)
		        newX = this.ClientSize.Width - player.Picture.Width;
		
		    // Vertical bounds (keep below HUD)
		    if (newY < 20) newY = 20;
		    if (newY + player.Picture.Height > this.ClientSize.Height - 10)
		        newY = this.ClientSize.Height - 10 - player.Picture.Height;
		
		    player.Move(dx, dy); // animates and moves with normalised vector
		    player.Picture.Left = newX;
		    player.Picture.Top  = newY;
		}

        // Keyboard
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Left  || e.KeyCode == Keys.A) keyLeft  = true;
            if (e.KeyCode == Keys.Right || e.KeyCode == Keys.D) keyRight = true;
            if (e.KeyCode == Keys.Up    || e.KeyCode == Keys.W) keyUp    = true;
            if (e.KeyCode == Keys.Down  || e.KeyCode == Keys.S) keyDown  = true;
            if (e.KeyCode == Keys.E && !interactHeld)
            {
                interactHeld = true;
                OpenNPCNearPlayer();
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (e.KeyCode == Keys.Left  || e.KeyCode == Keys.A) keyLeft  = false;
            if (e.KeyCode == Keys.Right || e.KeyCode == Keys.D) keyRight = false;
            if (e.KeyCode == Keys.Up    || e.KeyCode == Keys.W) keyUp    = false;
            if (e.KeyCode == Keys.Down  || e.KeyCode == Keys.S) keyDown  = false;
            if (e.KeyCode == Keys.E) interactHeld = false;
        }

        // If focus leaves the window while a key is held, KeyUp never arrives
        // and the player would keep walking (or E would stay "held").
        protected override void OnDeactivate(EventArgs e)
        {
            keyLeft = keyRight = keyUp = keyDown = false;
            interactHeld = false;
            base.OnDeactivate(e);
        }
    }
}