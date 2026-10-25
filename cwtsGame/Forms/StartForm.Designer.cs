namespace cwtsGame
{
    partial class StartForm
    {
        private System.ComponentModel.IContainer components = null;
        private cwtsGame.RoundButton BtnStart;
        private cwtsGame.RoundButton BtnExit;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.BtnStart = new cwtsGame.RoundButton();
            this.BtnExit = new cwtsGame.RoundButton();
            this.SuspendLayout();

            // 
            // BtnStart
            // 
            this.BtnStart.Cursor = System.Windows.Forms.Cursors.Hand;
            this.BtnStart.Name = "BtnStart";
            this.BtnStart.Radius = 40;
            this.BtnStart.Size = new System.Drawing.Size(300, 80);
            this.BtnStart.TabIndex = 0;
            this.BtnStart.Text = "START";
            this.BtnStart.UseVisualStyleBackColor = false;
            this.BtnStart.Click += new System.EventHandler(this.BtnStartClick);

            // 
            // BtnExit
            // 
            this.BtnExit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.BtnExit.Name = "BtnExit";
            this.BtnExit.Radius = 40;
            this.BtnExit.Size = new System.Drawing.Size(300, 80);
            this.BtnExit.TabIndex = 1;
            this.BtnExit.Text = "EXIT";
            this.BtnExit.UseVisualStyleBackColor = false;
            this.BtnExit.Click += new System.EventHandler(this.BtnExitClick);

            // 
            // StartForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1256, 541);
            this.Name = "StartForm";
            this.Text = "StartForm";

            this.Controls.Add(this.BtnStart);
            this.Controls.Add(this.BtnExit);

            this.Load += new System.EventHandler(this.StartForm_Load);

            this.ResumeLayout(false);
        }
    }
}