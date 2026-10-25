namespace cwtsGame
{
    partial class StageForm
    {
        private System.ComponentModel.IContainer components = null;

        private cwtsGame.PixelSign pnlSign;
        private cwtsGame.StageCard cardPark;
        private cwtsGame.StageCard cardStreet;
        private cwtsGame.PixelSign pnlFooter;
        private cwtsGame.RoundButton btnBack;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlSign = new cwtsGame.PixelSign();
            this.cardPark = new cwtsGame.StageCard();
            this.cardStreet = new cwtsGame.StageCard();
            this.pnlFooter = new cwtsGame.PixelSign();
            this.btnBack = new cwtsGame.RoundButton();
            this.SuspendLayout();
            // 
            // pnlSign
            // 
            this.pnlSign.FooterStyle = false;
            this.pnlSign.Location = new System.Drawing.Point(580, 32);
            this.pnlSign.Name = "pnlSign";
            this.pnlSign.Size = new System.Drawing.Size(760, 118);
            this.pnlSign.Subtitle = "Where will you volunteer today?";
            this.pnlSign.TabIndex = 1;
            this.pnlSign.TabStop = false;
            this.pnlSign.Title = "CHOOSE A LOCATION";
            // 
            // cardPark
            // 
            this.cardPark.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cardPark.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cardPark.Location = new System.Drawing.Point(500, 300);
            this.cardPark.Name = "cardPark";
            this.cardPark.Size = new System.Drawing.Size(405, 540);
            this.cardPark.TabIndex = 2;
            // 
            // cardStreet
            // 
            this.cardStreet.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cardStreet.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cardStreet.Location = new System.Drawing.Point(943, 300);
            this.cardStreet.Name = "cardStreet";
            this.cardStreet.Size = new System.Drawing.Size(405, 540);
            this.cardStreet.TabIndex = 3;
            // 
            // pnlFooter
            // 
            this.pnlFooter.FooterStyle = true;
            this.pnlFooter.Location = new System.Drawing.Point(725, 940);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Size = new System.Drawing.Size(470, 42);
            this.pnlFooter.Subtitle = "";
            this.pnlFooter.TabIndex = 4;
            this.pnlFooter.TabStop = false;
            this.pnlFooter.Title = "MORE CLEANUP SITES COMING SOON";
            // 
            // btnBack
            // 
            this.btnBack.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(247)))), ((int)(((byte)(230)))));
            this.btnBack.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(74)))), ((int)(((byte)(52)))));
            this.btnBack.BorderWidth = 2F;
            this.btnBack.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBack.FlatAppearance.BorderSize = 0;
            this.btnBack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBack.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBack.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(74)))), ((int)(((byte)(52)))));
            this.btnBack.HoverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(208)))), ((int)(((byte)(70)))));
            this.btnBack.Location = new System.Drawing.Point(30, 34);
            this.btnBack.Name = "btnBack";
            this.btnBack.Radius = 28;
            this.btnBack.Size = new System.Drawing.Size(56, 56);
            this.btnBack.TabIndex = 0;
            this.btnBack.Text = "←";
            this.btnBack.UseVisualStyleBackColor = false;
            this.btnBack.Click += new System.EventHandler(this.BtnBackClick);
            // 
            // StageForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(82)))), ((int)(((byte)(168)))), ((int)(((byte)(224)))));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1920, 1055);
            this.Controls.Add(this.btnBack);
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.pnlSign);
            this.Controls.Add(this.cardStreet);
            this.Controls.Add(this.cardPark);
            this.DoubleBuffered = true;
            this.KeyPreview = true;
            this.Name = "StageForm";
            this.Text = "Trash Go — Choose a Location";
            this.ResumeLayout(false);

        }
    }
}
