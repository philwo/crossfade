/* Crossfade - Your media player.
 * 
 * Copyright (C) 2007 Philipp Wollermann, Benjamin Lieberwirth,
 * Simon Franz, Christoph Griesser, Sebastian Sebald, Dominik Erb
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * */

namespace Crossfade
{
    partial class gui_tsmi_preferences
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(gui_tsmi_preferences));
			this.kryptonHeaderGroup1 = new ComponentFactory.Krypton.Toolkit.KryptonHeaderGroup();
			this.listBox1 = new System.Windows.Forms.ListBox();
			this.comboBox1 = new System.Windows.Forms.ComboBox();
			this.kryptonLabel1 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
			this.kryptonSplitContainer1 = new ComponentFactory.Krypton.Toolkit.KryptonSplitContainer();
			this.kryptonGroup2 = new ComponentFactory.Krypton.Toolkit.KryptonGroup();
			this.comboBox2 = new System.Windows.Forms.ComboBox();
			this.kryptonLabel5 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
			this.kryptonGroup1 = new ComponentFactory.Krypton.Toolkit.KryptonGroup();
			this.kryptonLabel4 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
			this.kryptonButton1 = new ComponentFactory.Krypton.Toolkit.KryptonButton();
			this.kryptonLabel2 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
			this.kryptonLabel3 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
			this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
			((System.ComponentModel.ISupportInitialize)(this.kryptonHeaderGroup1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.kryptonHeaderGroup1.Panel)).BeginInit();
			this.kryptonHeaderGroup1.Panel.SuspendLayout();
			this.kryptonHeaderGroup1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.kryptonLabel1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.kryptonSplitContainer1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.kryptonSplitContainer1.Panel1)).BeginInit();
			this.kryptonSplitContainer1.Panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.kryptonSplitContainer1.Panel2)).BeginInit();
			this.kryptonSplitContainer1.Panel2.SuspendLayout();
			this.kryptonSplitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.kryptonGroup2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.kryptonGroup2.Panel)).BeginInit();
			this.kryptonGroup2.Panel.SuspendLayout();
			this.kryptonGroup2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.kryptonLabel5)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.kryptonGroup1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.kryptonGroup1.Panel)).BeginInit();
			this.kryptonGroup1.Panel.SuspendLayout();
			this.kryptonGroup1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.kryptonLabel4)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.kryptonButton1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.kryptonLabel2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.kryptonLabel3)).BeginInit();
			this.SuspendLayout();
			// 
			// kryptonHeaderGroup1
			// 
			this.kryptonHeaderGroup1.DirtyPaletteCounter = 67;
			this.kryptonHeaderGroup1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.kryptonHeaderGroup1.Location = new System.Drawing.Point(0, 0);
			this.kryptonHeaderGroup1.Name = "kryptonHeaderGroup1";
			// 
			// kryptonHeaderGroup1.Panel
			// 
			this.kryptonHeaderGroup1.Panel.Controls.Add(this.listBox1);
			this.kryptonHeaderGroup1.Size = new System.Drawing.Size(369, 369);
			this.kryptonHeaderGroup1.TabIndex = 0;
			this.kryptonHeaderGroup1.Text = "Preferences";
			this.kryptonHeaderGroup1.ValuesPrimary.Description = "";
			this.kryptonHeaderGroup1.ValuesPrimary.Heading = "Preferences";
			this.kryptonHeaderGroup1.ValuesPrimary.Image = ((System.Drawing.Image)(resources.GetObject("kryptonHeaderGroup1.ValuesPrimary.Image")));
			this.kryptonHeaderGroup1.ValuesSecondary.Description = "";
			this.kryptonHeaderGroup1.ValuesSecondary.Heading = "General,  Player...";
			this.kryptonHeaderGroup1.ValuesSecondary.Image = null;
			// 
			// listBox1
			// 
			this.listBox1.BackColor = System.Drawing.Color.Black;
			this.listBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.listBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.listBox1.ForeColor = System.Drawing.Color.White;
			this.listBox1.FormattingEnabled = true;
			this.listBox1.ItemHeight = 15;
			this.listBox1.Items.AddRange(new object[] {
            "General",
            "Player"});
			this.listBox1.Location = new System.Drawing.Point(0, 0);
			this.listBox1.Name = "listBox1";
			this.listBox1.Size = new System.Drawing.Size(367, 315);
			this.listBox1.TabIndex = 0;
			this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
			// 
			// comboBox1
			// 
			this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBox1.FormattingEnabled = true;
			this.comboBox1.Items.AddRange(new object[] {
            "2",
            "5",
            "7",
            "10"});
			this.comboBox1.Location = new System.Drawing.Point(3, 33);
			this.comboBox1.Name = "comboBox1";
			this.comboBox1.Size = new System.Drawing.Size(75, 21);
			this.comboBox1.TabIndex = 5;
			this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
			// 
			// kryptonLabel1
			// 
			this.kryptonLabel1.DirtyPaletteCounter = 34;
			this.kryptonLabel1.Location = new System.Drawing.Point(3, 8);
			this.kryptonLabel1.Name = "kryptonLabel1";
			this.kryptonLabel1.Size = new System.Drawing.Size(75, 19);
			this.kryptonLabel1.TabIndex = 4;
			this.kryptonLabel1.Text = "Set Fadetime";
			this.kryptonLabel1.Values.ExtraText = "";
			this.kryptonLabel1.Values.Image = null;
			this.kryptonLabel1.Values.Text = "Set Fadetime";
			// 
			// kryptonSplitContainer1
			// 
			this.kryptonSplitContainer1.Cursor = System.Windows.Forms.Cursors.Default;
			this.kryptonSplitContainer1.DirtyPaletteCounter = 68;
			this.kryptonSplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.kryptonSplitContainer1.Location = new System.Drawing.Point(0, 0);
			this.kryptonSplitContainer1.Name = "kryptonSplitContainer1";
			// 
			// kryptonSplitContainer1.Panel1
			// 
			this.kryptonSplitContainer1.Panel1.Controls.Add(this.kryptonHeaderGroup1);
			this.kryptonSplitContainer1.Panel1MinSize = 150;
			// 
			// kryptonSplitContainer1.Panel2
			// 
			this.kryptonSplitContainer1.Panel2.Controls.Add(this.kryptonGroup2);
			this.kryptonSplitContainer1.Panel2.Controls.Add(this.kryptonGroup1);
			this.kryptonSplitContainer1.Size = new System.Drawing.Size(957, 369);
			this.kryptonSplitContainer1.SplitterDistance = 369;
			this.kryptonSplitContainer1.TabIndex = 0;
			// 
			// kryptonGroup2
			// 
			this.kryptonGroup2.DirtyPaletteCounter = 55;
			this.kryptonGroup2.Location = new System.Drawing.Point(188, 3);
			this.kryptonGroup2.Name = "kryptonGroup2";
			// 
			// kryptonGroup2.Panel
			// 
			this.kryptonGroup2.Panel.Controls.Add(this.comboBox2);
			this.kryptonGroup2.Panel.Controls.Add(this.kryptonLabel5);
			this.kryptonGroup2.Panel.Controls.Add(this.comboBox1);
			this.kryptonGroup2.Panel.Controls.Add(this.kryptonLabel1);
			this.kryptonGroup2.Size = new System.Drawing.Size(246, 156);
			this.kryptonGroup2.TabIndex = 1;
			// 
			// comboBox2
			// 
			this.comboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBox2.FormattingEnabled = true;
			this.comboBox2.Items.AddRange(new object[] {
            "5",
            "10",
            "15",
            "20"});
			this.comboBox2.Location = new System.Drawing.Point(93, 32);
			this.comboBox2.Name = "comboBox2";
			this.comboBox2.Size = new System.Drawing.Size(130, 21);
			this.comboBox2.TabIndex = 7;
			this.comboBox2.SelectedIndexChanged += new System.EventHandler(this.comboBox2_SelectedIndexChanged);
			// 
			// kryptonLabel5
			// 
			this.kryptonLabel5.DirtyPaletteCounter = 35;
			this.kryptonLabel5.Location = new System.Drawing.Point(93, 8);
			this.kryptonLabel5.Name = "kryptonLabel5";
			this.kryptonLabel5.Size = new System.Drawing.Size(121, 19);
			this.kryptonLabel5.TabIndex = 6;
			this.kryptonLabel5.Text = "Set Picture Show Time";
			this.kryptonLabel5.Values.ExtraText = "";
			this.kryptonLabel5.Values.Image = null;
			this.kryptonLabel5.Values.Text = "Set Picture Show Time";
			// 
			// kryptonGroup1
			// 
			this.kryptonGroup1.DirtyPaletteCounter = 55;
			this.kryptonGroup1.Location = new System.Drawing.Point(3, 3);
			this.kryptonGroup1.Name = "kryptonGroup1";
			// 
			// kryptonGroup1.Panel
			// 
			this.kryptonGroup1.Panel.Controls.Add(this.kryptonLabel4);
			this.kryptonGroup1.Panel.Controls.Add(this.kryptonButton1);
			this.kryptonGroup1.Panel.Controls.Add(this.kryptonLabel2);
			this.kryptonGroup1.Size = new System.Drawing.Size(179, 156);
			this.kryptonGroup1.TabIndex = 0;
			// 
			// kryptonLabel4
			// 
			this.kryptonLabel4.DirtyPaletteCounter = 54;
			this.kryptonLabel4.Location = new System.Drawing.Point(3, 32);
			this.kryptonLabel4.Name = "kryptonLabel4";
			this.kryptonLabel4.Size = new System.Drawing.Size(81, 19);
			this.kryptonLabel4.TabIndex = 7;
			this.kryptonLabel4.Text = "kryptonLabel4";
			this.kryptonLabel4.Values.ExtraText = "";
			this.kryptonLabel4.Values.Image = null;
			this.kryptonLabel4.Values.Text = "kryptonLabel4";
			// 
			// kryptonButton1
			// 
			this.kryptonButton1.DirtyPaletteCounter = 14;
			this.kryptonButton1.Location = new System.Drawing.Point(3, 56);
			this.kryptonButton1.Name = "kryptonButton1";
			this.kryptonButton1.PaletteMode = ComponentFactory.Krypton.Toolkit.PaletteMode.Office2007Black;
			this.kryptonButton1.Size = new System.Drawing.Size(93, 23);
			this.kryptonButton1.TabIndex = 6;
			this.kryptonButton1.Text = "Change Folder";
			this.kryptonButton1.Values.ExtraText = "";
			this.kryptonButton1.Values.Image = null;
			this.kryptonButton1.Values.ImageStates.ImageCheckedNormal = null;
			this.kryptonButton1.Values.ImageStates.ImageCheckedPressed = null;
			this.kryptonButton1.Values.ImageStates.ImageCheckedTracking = null;
			this.kryptonButton1.Values.Text = "Change Folder";
			this.kryptonButton1.Click += new System.EventHandler(this.kryptonButton1_Click);
			// 
			// kryptonLabel2
			// 
			this.kryptonLabel2.DirtyPaletteCounter = 36;
			this.kryptonLabel2.Location = new System.Drawing.Point(3, 8);
			this.kryptonLabel2.Name = "kryptonLabel2";
			this.kryptonLabel2.Size = new System.Drawing.Size(93, 19);
			this.kryptonLabel2.TabIndex = 5;
			this.kryptonLabel2.Text = "Set Music Folder";
			this.kryptonLabel2.Values.ExtraText = "";
			this.kryptonLabel2.Values.Image = null;
			this.kryptonLabel2.Values.Text = "Set Music Folder";
			// 
			// kryptonLabel3
			// 
			this.kryptonLabel3.DirtyPaletteCounter = 41;
			this.kryptonLabel3.Location = new System.Drawing.Point(3, 8);
			this.kryptonLabel3.Name = "kryptonLabel3";
			this.kryptonLabel3.PaletteMode = ComponentFactory.Krypton.Toolkit.PaletteMode.Office2007Blue;
			this.kryptonLabel3.Size = new System.Drawing.Size(138, 19);
			this.kryptonLabel3.TabIndex = 5;
			this.kryptonLabel3.Text = "Set Path For Music Folder";
			this.kryptonLabel3.Values.ExtraText = "";
			this.kryptonLabel3.Values.Image = null;
			this.kryptonLabel3.Values.Text = "Set Path For Music Folder";
			// 
			// gui_tsmi_preferences
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.WindowText;
			this.ClientSize = new System.Drawing.Size(957, 369);
			this.Controls.Add(this.kryptonSplitContainer1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "gui_tsmi_preferences";
			this.Text = "Preferences";
			((System.ComponentModel.ISupportInitialize)(this.kryptonHeaderGroup1.Panel)).EndInit();
			this.kryptonHeaderGroup1.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.kryptonHeaderGroup1)).EndInit();
			this.kryptonHeaderGroup1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.kryptonLabel1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.kryptonSplitContainer1.Panel1)).EndInit();
			this.kryptonSplitContainer1.Panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.kryptonSplitContainer1.Panel2)).EndInit();
			this.kryptonSplitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.kryptonSplitContainer1)).EndInit();
			this.kryptonSplitContainer1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.kryptonGroup2.Panel)).EndInit();
			this.kryptonGroup2.Panel.ResumeLayout(false);
			this.kryptonGroup2.Panel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.kryptonGroup2)).EndInit();
			this.kryptonGroup2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.kryptonLabel5)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.kryptonGroup1.Panel)).EndInit();
			this.kryptonGroup1.Panel.ResumeLayout(false);
			this.kryptonGroup1.Panel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.kryptonGroup1)).EndInit();
			this.kryptonGroup1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.kryptonLabel4)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.kryptonButton1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.kryptonLabel2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.kryptonLabel3)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

		private ComponentFactory.Krypton.Toolkit.KryptonHeaderGroup kryptonHeaderGroup1;
		private System.Windows.Forms.ComboBox comboBox1;
		private ComponentFactory.Krypton.Toolkit.KryptonLabel kryptonLabel1;
		private System.Windows.Forms.ListBox listBox1;
		private ComponentFactory.Krypton.Toolkit.KryptonSplitContainer kryptonSplitContainer1;
		private ComponentFactory.Krypton.Toolkit.KryptonGroup kryptonGroup2;
		private ComponentFactory.Krypton.Toolkit.KryptonGroup kryptonGroup1;
		private ComponentFactory.Krypton.Toolkit.KryptonLabel kryptonLabel2;
		private ComponentFactory.Krypton.Toolkit.KryptonLabel kryptonLabel3;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
		private ComponentFactory.Krypton.Toolkit.KryptonLabel kryptonLabel4;
		private ComponentFactory.Krypton.Toolkit.KryptonButton kryptonButton1;
		private System.Windows.Forms.ComboBox comboBox2;
		private ComponentFactory.Krypton.Toolkit.KryptonLabel kryptonLabel5;


	}
}