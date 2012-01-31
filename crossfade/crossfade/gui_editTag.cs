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
using System.IO;
using System.Diagnostics;
using Musicbrainz;
using System.Net;

namespace Crossfade
{
	public partial class gui_editTag : ComponentFactory.Krypton.Toolkit.KryptonForm
	{
		private static Media media;

		public gui_editTag()
		{
			InitializeComponent();
			openFileDialog2.FileName = null;
		}

		public void readMedium(Media tmp)
		{
			try
			{
				media = tmp;
				pictureBox1.Image = null;
				textBox1.Text = media.TrackNr.ToString();
				textBox2.Text = media.Title;
				textBox3.Text = media.Album;
				textBox4.Text = media.Artist;
				textBox5.Text = media.Year.ToString();
				textBox6.Text = media.Genre;
				textBox7.Text = media.Comment;
				textBox8.Text = media.Composer;
				textBox9.Text = media.Copyright;
				if (media.Cover != null)
					pictureBox1.Image = Image.FromStream(media.Cover);
			}
			catch (IOException ex)
			{
				throw ex;
			}
		}

		private void save_button_Click(object sender, EventArgs e)
		{
			Stream fs;

			//Media media = Playlist.Instance[Playlist.Instance.Cursor];
			media.TrackNr = UInt32.Parse(textBox1.Text);
			media.Title = textBox2.Text;
			media.Album = textBox3.Text;
			media.Artist = textBox4.Text;
			media.Year = UInt32.Parse(textBox5.Text);
			media.Genre = textBox6.Text;
			media.Comment = textBox7.Text;
			media.Composer = textBox8.Text;
			media.Copyright = textBox9.Text;

			if (pictureBox1.Image != null)
			{
				if (openFileDialog2.FileName != "")
				{
					fs = new FileStream(openFileDialog2.FileName, FileMode.Open);
					media.Cover = fs;
				}
				else
				{
					fs = new MemoryStream();
					pictureBox1.Image.Save(fs, System.Drawing.Imaging.ImageFormat.Jpeg);
					media.Cover = fs;
				}
			}
			
			media.commitToTag();


			// Flag wird gesetzt das File geändert wurde, somit ein playlist refresh ausgelöst
			Playlist.Instance.id3tag_edited = true;
			Close();
		}

		private void openpic_button_Click(object sender, EventArgs e)
		{
			Stream myStream = null;

			openFileDialog2.Filter = "Bilddateien (*.jpg; *.png)|*.jpg;*.png";
			if (openFileDialog2.ShowDialog() == DialogResult.OK)
			{
				try
				{
					if ((myStream = openFileDialog2.OpenFile()) != null)
					{

						Bitmap bmp = new Bitmap(myStream);
						pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
						pictureBox1.Image = bmp;
						pictureBox1.Refresh();
						myStream.Close();
					}
				}
				catch (IOException ex)
				{
					MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message,"IOException",MessageBoxButtons.OK, MessageBoxIcon.Stop);
				}
			}
		}

		private Bitmap ResizeBitmap(Bitmap b, int nWidth, int nHeight)
		{
			Bitmap result = new Bitmap(nWidth, nHeight);
			using (Graphics g = Graphics.FromImage((Image)result))
				g.DrawImage(b, 0, 0, nWidth, nHeight);
			return result;
		}

		new public void Show()
		{
			((Form)this).Show();
			WindowState = FormWindowState.Normal;
			BringToFront();
		}

		private void gui_editTag_Load(object sender, EventArgs e)
		{
		}

		private void cancel_button_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void SuggestButton_Click(object sender, EventArgs e)
		{
			progressBar1.Visible = true;
			progressBar1.Value = 0;
			progressBar1.Value = (progressBar1.Value+1)%100;
			string interpret = textBox4.Text;
			string titel = textBox2.Text;
			string album = textBox3.Text;
            progressBar1.Value = (progressBar1.Value + 1) % 100;
			musicbrainz musicbrain = new musicbrainz();
            progressBar1.Value = (progressBar1.Value + 1) % 100;
			musicbrainz.InterpretInfo[] Interpreten = new musicbrainz.InterpretInfo[25];
			musicbrainz.CDInfo[] CDInfos = new musicbrainz.CDInfo[25];
            progressBar1.Value = (progressBar1.Value + 1) % 100;
			try
			{
				Interpreten = musicbrain.get_Interpret(interpret);
			}
			catch (WebException)
			{
				MessageBox.Show("Remoteserver access failed!\nPlease check the Interpret and the network connection.", "Suggest-Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
			}
			System.Threading.Thread.Sleep(1000);
            progressBar1.Value = (progressBar1.Value + 1) % 100;
			if (Interpreten[0].score >= 99)
			{
				bool album_query = false;
				if (album == null || album == "")
				{
                    progressBar1.Value = (progressBar1.Value + 1) % 100;
					try
					{
						CDInfos = musicbrain.get_CDs(interpret, titel);
					}
					catch(WebException){
						MessageBox.Show("Remoteserver access failed!\nPlease check the Interpret, Titel and the network connection.", "Suggest-Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
					}
                    progressBar1.Value = (progressBar1.Value + 1) % 100;
				}
				else
				{
                    progressBar1.Value = (progressBar1.Value + 1) % 100;
					try
					{
					CDInfos = musicbrain.get_CDs_from_Album(interpret, album);
				}
				catch (WebException)
				{
					MessageBox.Show("Remoteserver access failed!\nPlease check the Interpret, Album and the network connection.", "Suggest-Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
				}
					album_query = true;
                    progressBar1.Value = (progressBar1.Value + 1) % 100;
				}

				for (int n = 0; n <= 24; n++)
				{
					progressBar1.Value++;
					if (CDInfos[n].cdASIN != null)
					{
                        progressBar1.Value = (progressBar1.Value + 1) % 100;
						show_CDImage(CDInfos[n].coverURL);
						if (!album_query)
							textBox3.Text = CDInfos[n].cdName;
						break;
					}
				}
			}
            progressBar1.Visible = false;
            
		}
		public void show_CDImage(string img_url)
		{
			string strImageUrl = img_url;
			try
			{
				WebRequest wrq = WebRequest.Create(strImageUrl);
				WebResponse wrp = wrq.GetResponse();

				Bitmap bmp = new Bitmap(wrp.GetResponseStream());
				pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
				pictureBox1.Image = bmp;
				pictureBox1.Refresh();
				pictureBox1.Update();
			}
			catch
			{
				pictureBox1.Image = null;
				pictureBox1.Update();
			}
		}		
	}
}
