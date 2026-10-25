/*
 * Created by SharpDevelop.
 * User: cruzl
 * Date: 4/25/2026
 * Time: 5:10 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Windows.Forms;

namespace cwtsGame
{
	/// <summary>
	/// Description of StartForm.
	/// </summary>
	public partial class StartForm : Form
	{
		public StartForm()
		{
		    InitializeComponent();
		    StyleButtons();
		    
		    this.WindowState = FormWindowState.Maximized;
		
		    string gifPath = System.IO.Path.Combine(Application.StartupPath, "imgBackgroundGIF.gif");
		
		    PictureBox bg = new PictureBox();
		    bg.Dock = DockStyle.Fill;
		    bg.Image = Image.FromFile(gifPath);   
		    bg.SizeMode = PictureBoxSizeMode.StretchImage;
		
		    this.Controls.Add(bg);
		    bg.SendToBack();
		
		  
		}
		
		private void StyleButtons()
		{
		    StyleStageButton(BtnStart, "START");
		    StyleStageButton(BtnExit, "EXIT");
		}
		
		private static void StyleStageButton(RoundButton button, string text)
		{
		    button.Text = text;
		    button.BackColor = UiTheme.Cream;
		    button.ForeColor = UiTheme.Ink;
		    button.BorderColor = UiTheme.Ink;
		    button.BorderWidth = 3f;
		    button.HoverBackColor = UiTheme.Sunshine;
		    button.Radius = Math.Max(18, button.Height / 2);
		    button.PixelFontSize = 22f;
		    button.FlatStyle = FlatStyle.Flat;
		    button.FlatAppearance.BorderSize = 0;
		    button.UseVisualStyleBackColor = false;
		    button.Cursor = Cursors.Hand;
		}
		
		private void StartForm_Load(object sender, EventArgs e)
		{
		    int spacing = 20;
		
		    int totalWidth = BtnStart.Width + spacing + BtnExit.Width;
		
		    int startX = (this.ClientSize.Width - totalWidth) / 2;
		    int bottomY = this.ClientSize.Height - BtnStart.Height - 30;
		
		    BtnStart.Location = new System.Drawing.Point(startX, bottomY);
		    BtnExit.Location = new System.Drawing.Point(startX + BtnStart.Width + spacing, bottomY);
		}
		
		void BtnStartClick(object sender, EventArgs e)
		{
			StageForm StageForm = new StageForm(this);
            StageForm.Show();
            this.Hide();
            
           
		          
		}
		
		void BtnInstructionsClick(object sender, EventArgs e)
		{
			
            
            
		}
		
		void BtnExitClick(object sender, EventArgs e)
		{
			Application.Exit();
		}
		
	}
}
