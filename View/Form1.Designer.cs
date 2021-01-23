namespace TankWars
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuGroupBox = new System.Windows.Forms.GroupBox();
            this.connectBtn = new System.Windows.Forms.Button();
            this.playerNameBox = new System.Windows.Forms.TextBox();
            this.nameLbl = new System.Windows.Forms.Label();
            this.ipAddrText = new System.Windows.Forms.TextBox();
            this.serverLbl = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuGroupBox.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuGroupBox
            // 
            this.menuGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.menuGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.menuGroupBox.Controls.Add(this.connectBtn);
            this.menuGroupBox.Controls.Add(this.playerNameBox);
            this.menuGroupBox.Controls.Add(this.nameLbl);
            this.menuGroupBox.Controls.Add(this.ipAddrText);
            this.menuGroupBox.Controls.Add(this.serverLbl);
            this.menuGroupBox.Controls.Add(this.menuStrip1);
            this.menuGroupBox.Location = new System.Drawing.Point(1, -1);
            this.menuGroupBox.Name = "menuGroupBox";
            this.menuGroupBox.Padding = new System.Windows.Forms.Padding(3, 1, 3, 3);
            this.menuGroupBox.Size = new System.Drawing.Size(800, 42);
            this.menuGroupBox.TabIndex = 0;
            this.menuGroupBox.TabStop = false;
            // 
            // connectBtn
            // 
            this.connectBtn.Location = new System.Drawing.Point(398, 15);
            this.connectBtn.Name = "connectBtn";
            this.connectBtn.Size = new System.Drawing.Size(75, 23);
            this.connectBtn.TabIndex = 4;
            this.connectBtn.Text = "Connect";
            this.connectBtn.UseVisualStyleBackColor = true;
            this.connectBtn.Click += new System.EventHandler(this.ConnectBtn_Click);
            // 
            // playerNameBox
            // 
            this.playerNameBox.Location = new System.Drawing.Point(292, 15);
            this.playerNameBox.Name = "playerNameBox";
            this.playerNameBox.Size = new System.Drawing.Size(100, 22);
            this.playerNameBox.TabIndex = 3;
            this.playerNameBox.Text = "player";
            // 
            // nameLbl
            // 
            this.nameLbl.AutoSize = true;
            this.nameLbl.Location = new System.Drawing.Point(237, 16);
            this.nameLbl.Name = "nameLbl";
            this.nameLbl.Size = new System.Drawing.Size(49, 17);
            this.nameLbl.TabIndex = 2;
            this.nameLbl.Text = "Name:";
            // 
            // ipAddrText
            // 
            this.ipAddrText.Location = new System.Drawing.Point(67, 15);
            this.ipAddrText.Name = "ipAddrText";
            this.ipAddrText.Size = new System.Drawing.Size(164, 22);
            this.ipAddrText.TabIndex = 1;
            this.ipAddrText.Text = "localhost";
            // 
            // serverLbl
            // 
            this.serverLbl.AutoSize = true;
            this.serverLbl.Location = new System.Drawing.Point(7, 16);
            this.serverLbl.Name = "serverLbl";
            this.serverLbl.Size = new System.Drawing.Size(54, 17);
            this.serverLbl.TabIndex = 0;
            this.serverLbl.Text = "Server:";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.menuStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.menuStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Visible;
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(729, 9);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(68, 28);
            this.menuStrip1.TabIndex = 5;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(55, 24);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(141, 26);
            this.toolStripMenuItem1.Text = "Control";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.ControlHelpBtn);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(141, 26);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.AboutToolStripMenuItem_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 900);
            this.Controls.Add(this.menuGroupBox);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Form1";
            this.menuGroupBox.ResumeLayout(false);
            this.menuGroupBox.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox menuGroupBox;
        private System.Windows.Forms.TextBox playerNameBox;
        private System.Windows.Forms.Label nameLbl;
        private System.Windows.Forms.TextBox ipAddrText;
        private System.Windows.Forms.Label serverLbl;
        private System.Windows.Forms.Button connectBtn;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
    }
}

