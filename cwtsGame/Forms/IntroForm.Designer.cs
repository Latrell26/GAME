namespace cwtsGame
{
    partial class IntroForm
    {
        private System.ComponentModel.IContainer components = null;

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
            this.SuspendLayout();
            //
            // IntroForm
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(82)))), ((int)(((byte)(168)))), ((int)(((byte)(224)))));
            this.ClientSize = new System.Drawing.Size(1280, 720);
            this.Name = "IntroForm";
            this.Text = "Trash Go - Mission Briefing";
            this.ResumeLayout(false);
        }
    }
}
