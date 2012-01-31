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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace Crossfade
{
	public partial class gui_tsmi_preferences : ComponentFactory.Krypton.Toolkit.KryptonForm
	{
		public gui_tsmi_preferences()
		{
			InitializeComponent();
			this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
			this.UpdateStyles();

			string ft = (Crossfade.Properties.Settings.Default.Fadetime/1000).ToString();
			comboBox1.SelectedItem = ft;
			string mf = Crossfade.Properties.Settings.Default.MusicFolder;
			kryptonLabel4.Text = Crossfade.Properties.Settings.Default.MusicFolder;
			string pt = (Crossfade.Properties.Settings.Default.PictureShowTime/1000).ToString();
			comboBox2.SelectedItem = pt;

			kryptonGroup1.Dock = DockStyle.Fill;
			kryptonGroup2.Dock = DockStyle.Fill;

			listBox1.SelectedIndex = 0;
			kryptonGroup1.Show();

			this.ClientSize = new Size(480, 160);
		}

		private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			switch (listBox1.SelectedIndex)
			{
				case 0:
					kryptonGroup1.Show();
					kryptonGroup2.Hide();
					break;
				case 1:
					kryptonGroup1.Hide();
					kryptonGroup2.Show();
					break;
			}
		}

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			int tmp;
			switch (comboBox1.SelectedIndex)
			{
				case 0:
					int.TryParse(comboBox1.SelectedItem.ToString(), out tmp);
					Crossfade.Properties.Settings.Default.Fadetime = tmp * 1000;
					Crossfade.Properties.Settings.Default.Save();
					Player.Instance.Fadetime = tmp;
					break;
				case 1:
					int.TryParse(comboBox1.SelectedItem.ToString(), out tmp);
					Crossfade.Properties.Settings.Default.Fadetime = tmp * 1000;
					Crossfade.Properties.Settings.Default.Save();
					Player.Instance.Fadetime = tmp;
					break;
				case 2:
					int.TryParse(comboBox1.SelectedItem.ToString(), out tmp);
					Crossfade.Properties.Settings.Default.Fadetime = tmp * 1000;
					Crossfade.Properties.Settings.Default.Save();
					Player.Instance.Fadetime = tmp;
					break;
				case 3:
					int.TryParse(comboBox1.SelectedItem.ToString(), out tmp);
					Crossfade.Properties.Settings.Default.Fadetime = tmp * 1000;
					Crossfade.Properties.Settings.Default.Save();
					Player.Instance.Fadetime = tmp;
					break;
			}
		}

		private void kryptonButton1_Click(object sender, EventArgs e)
		{
			folderBrowserDialog1.ShowDialog();
			if (folderBrowserDialog1.SelectedPath != null)
			{
				Crossfade.Properties.Settings.Default.MusicFolder = folderBrowserDialog1.SelectedPath;
				Crossfade.Properties.Settings.Default.Save();
				kryptonLabel4.Text = folderBrowserDialog1.SelectedPath;
			}
		}

		private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
		{
			int tmp;
			switch (comboBox1.SelectedIndex)
			{
				case 0:
					int.TryParse(comboBox2.SelectedItem.ToString(), out tmp);
					Crossfade.Properties.Settings.Default.PictureShowTime = tmp * 1000;
					Crossfade.Properties.Settings.Default.Save();
					Player.Instance.Picshowtime = tmp;
					break;
				case 1:
					int.TryParse(comboBox2.SelectedItem.ToString(), out tmp);
					Crossfade.Properties.Settings.Default.PictureShowTime = tmp * 1000;
					Crossfade.Properties.Settings.Default.Save();
					Player.Instance.Picshowtime = tmp;
					break;
				case 2:
					int.TryParse(comboBox2.SelectedItem.ToString(), out tmp);
					Crossfade.Properties.Settings.Default.PictureShowTime = tmp * 1000;
					Crossfade.Properties.Settings.Default.Save();
					Player.Instance.Picshowtime = tmp;
					break;
				case 3:
					int.TryParse(comboBox2.SelectedItem.ToString(), out tmp);
					Crossfade.Properties.Settings.Default.PictureShowTime = tmp * 1000;
					Crossfade.Properties.Settings.Default.Save();
					Player.Instance.Picshowtime = tmp;
					break;
			}
		}
	}
}
