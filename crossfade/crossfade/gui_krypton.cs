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
using System.Resources;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Reflection;
using System.Globalization;
using System.Threading;
using System.Diagnostics;

using ZeroconfService;


namespace Crossfade
{
	public partial class gui_krypton : ComponentFactory.Krypton.Toolkit.KryptonForm, IGUI
	{
		private bool volumeScrolling = false;
		private TreeNode tnMediaLibrary = null, tnPlaylists = null, tnSmartlists = null, tnPeer2Peer = null, tnAudioCDs = null;
		private List<Media> mediaListBackup = null;
		private Query StandardQuery;

		private delegate void DelegateAddMedia(String tmp);           // type
		private DelegateAddMedia m_DelegateAddMedia;                // instance

		public Media RowGetterDelegate(int i)
		{
			if (i < Playlist.Instance.Count)
				return Playlist.Instance[i];
			else
				return null;
		}

		public void SortDelegate(BrightIdeasSoftware.OLVColumn column, SortOrder order)
		{
			List<Media> mediaList = new List<Media>();

			string orderStr;

			switch (order)
			{
				case SortOrder.Ascending:
				case SortOrder.None:
					orderStr = "ASC";
					break;
				case SortOrder.Descending:
				default:
					orderStr = "DESC";
					break;
			}


			if (mediaListBackup == null)
			{
				ObjectComparer<Media> comparer;
				if (column.AspectName == "TrackNr")
				{
					comparer = new ObjectComparer<Media>("Artist ASC, Album ASC, TrackNr ASC", true);
				}
				else
				{
					comparer = new ObjectComparer<Media>(column.AspectName + " " + orderStr, true);
				}

				Playlist.Instance.Sort(comparer);
			}
			else
			{
				switch (order)
				{
					case SortOrder.Ascending:
					case SortOrder.None:
						StandardQuery.sort.order = Query.SortOrder.Ascending;
						break;
					case SortOrder.Descending:
					default:
						StandardQuery.sort.order = Query.SortOrder.Descending;
						break;
				}

				if (column.AspectName == "TrackNr")
					StandardQuery.sort.by = Query.SortBy.Smart;
				else if (column.AspectName == "Artist")
					StandardQuery.sort.by = Query.SortBy.Artist;
				else if (column.AspectName == "Title")
					StandardQuery.sort.by = Query.SortBy.Title;
				else if (column.AspectName == "Album")
					StandardQuery.sort.by = Query.SortBy.Album;

				queryPlaylist();
			}

			lvPlaylist.BuildList();
		}

		public gui_krypton()
		{
			InitializeComponent();
			this.DoubleBuffered = true;
			this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
			this.UpdateStyles();

		}

		new public void Show()
		{
			((Form)this).Show();
			WindowState = FormWindowState.Normal;
			BringToFront();
		}

		private void gui_krypton_Load(object sender, EventArgs e)
		{
			gui_SplashScreen.SetStatus("Populating playlist ...");
			lvPlaylist.RowGetter = RowGetterDelegate;
			lvPlaylist.CustomSorter = SortDelegate;

			// Install aspect getters to optimize performance
			columnTrackNr.AspectGetter = delegate(object x)
			{
				if (x == null) return 0;
				return ((Media)x).TrackNr;
			};
			columnArtist.AspectGetter = delegate(object x)
			{
				if (x == null) return "";
				return ((Media)x).Artist;
			};
			columnTitle.AspectGetter = delegate(object x)
			{
				if (x == null) return "";
				return ((Media)x).Title;
			};
			columnAlbum.AspectGetter = delegate(object x)
			{
				if (x == null) return "";
				return ((Media)x).Album;
			};
			columnRating.AspectGetter = delegate(object x)
			{
				if (x == null) return 0;
				return ((Media)x).Rating;
			};
			columnLength.AspectGetter = delegate(object x)
			{
				if (x == null) return -1;
				return ((Media)x).Length;
			};
			columnTrackNr.AspectToStringConverter = delegate(object aspect)
			{
				int i = Convert.ToInt32(aspect);
				if (i == 0) return "";
				else return i.ToString();
			};
			columnLength.AspectToStringConverter = delegate(object aspect)
			{
				string lenStr;
				int length = ((int)aspect);

				if (length == -1)
				{
					lenStr = "";
				}
				else
				{
					lenStr = String.Format("{0:00}:{1:00}", length / 1000 / 60, (length / 1000) % 60);
				}

				return lenStr;
			};
			columnRating.Renderer = new BrightIdeasSoftware.MultiImageRenderer(Properties.Resources.star, 5, 0, 5);

			columnTrackNr.ImageGetter = delegate(object rowObject)
			{
				if (rowObject == null) return 4;

				Media m = (Media)rowObject;
				Media currentMedia = ((Media)Playlist.Instance[Playlist.Instance.Cursor]);

				if (currentMedia == null) return 4;

				if (m.uri == currentMedia.uri)
					return 1;
				else
					return 4;
			};

			StandardQuery = new Query(new List<Query.Match>());
			StandardQuery.sort.by = Query.SortBy.Smart;
			StandardQuery.sort.order = Query.SortOrder.Ascending;

			Media[] results = MediaDB.Instance.mediaList.ToArray();
			Playlist.Instance.clearPlaylist();
			Playlist.Instance.addMedia(results);
			updatePlaylist();

			setViewmode(Viewmode.Playlist);

			/* Populate Media Library TreeView */
			tnMediaLibrary = new TreeNode("Media Library", 0, 0);
			tnMediaLibrary.Tag = "media-lib";

			tnPlaylists = new TreeNode("My Playlists", 1, 1);
			tnSmartlists = new TreeNode("My Smartlists", 2, 2);
			tnPeer2Peer = new TreeNode("My Network", 4, 4);
			tnAudioCDs = new TreeNode("Audio CD", 3, 3);

			refreshPlayAndSmartlists();

			this.tvMediaLibrary.Nodes.AddRange(new System.Windows.Forms.TreeNode[] { tnMediaLibrary, tnPlaylists, tnSmartlists, tnPeer2Peer, tnAudioCDs });
			m_DelegateAddMedia = new DelegateAddMedia(this.addMediaToPlaylist);

			timer1.Enabled = true;
			gui_SplashScreen.CloseForm();
			onMediaChanged();
		}

		public void refreshPlayAndSmartlists()
		{
			tnPlaylists.Nodes.Clear();
			foreach (PlaylistFile p in MediaDB.Instance.playlists)
			{
				TreeNode tmp = new TreeNode(p.name, 1, 1);
				tmp.Tag = p.filename;
				tnPlaylists.Nodes.Add(tmp);
			}

			tnSmartlists.Nodes.Clear();
			foreach (Query q in MediaDB.Instance.smartlists.Items)
			{
				TreeNode tmp = new TreeNode(q.name, 2, 2);
				tmp.Tag = q;
				tnSmartlists.Nodes.Add(tmp);
			}

			tnAudioCDs.Nodes.Clear();
			DriveInfo[] drives = DriveInfo.GetDrives();
			foreach (DriveInfo drive in drives)
			{
				if (drive.DriveType == DriveType.CDRom)
				{
					TreeNode tmp = new TreeNode("Audio CD (" + drive.Name + ")", 3, 3);
					tmp.Tag = drive.Name;
					tnAudioCDs.Nodes.Add(tmp);
				}
			}
		}

		public PictureBox getDrawingCanvas()
		{
			return drawingCanvas;
		}

		public PictureBox getVideoCanvas()
		{
			return videoCanvas;
		}

		public void setViewmode(Viewmode vm)
		{
			switch (vm)
			{
				case Viewmode.Playlist:
					lvPlaylist.Show();
					videoCanvas.Hide();
					drawingCanvas.Hide();
					backButton.Hide();
					break;
				case Viewmode.Picture:
					drawingCanvas.Show();
					videoCanvas.Hide();
					lvPlaylist.Hide();
					backButton.Show();
					break;
				case Viewmode.Video:
					videoCanvas.Show();
					drawingCanvas.Hide();
					lvPlaylist.Hide();
					backButton.Show();
					break;
			}
		}

		private void btnPlay_Click(object sender, EventArgs e)
		{
			if (Player.Instance.isPlaying) Player.Instance.mediaChangeIntended = true;
			Media tmp = Playlist.Instance[Playlist.Instance.Cursor];

			try
			{
				Player.Instance.stop();
				Player.Instance.open(ref tmp);
				Player.Instance.play();
				updatePlaylist();
			}
			catch (WebException)
			{
				MessageBox.Show("The server has locked the file. Please try again later.", "WebException", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			catch (InvalidOperationException)
			{
				btnNext_Click(sender, e);
			}
		}

		private void btnStop_Click(object sender, EventArgs e)
		{
			tbPosition.Value = 0;
			Player.Instance.stop();
		}

		private void btnPause_Click(object sender, EventArgs e)
		{
			Player.Instance.togglePause();
		}

		private void btnPrev_Click(object sender, EventArgs e)
		{
			Playlist.Instance.Cursor = Playlist.Instance.prev();
			btnPlay_Click(sender, e);
		}

		private void btnNext_Click(object sender, EventArgs e)
		{
			Playlist.Instance.Cursor = Playlist.Instance.next();
			btnPlay_Click(sender, e);
		}

		private void btnOpen_Click(object sender, EventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.CheckFileExists = true;
			ofd.CheckPathExists = true;
			ofd.DereferenceLinks = true;
			ofd.RestoreDirectory = true;
			ofd.Filter = "Musikdateien (*.mp3, *.ogg, *.flac, *.wma)|*.mp3;*.ogg;*.flac; *.wma|Alle Dateien (*.*)|*.*";

			if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				Media media = new Media(new Uri(System.IO.Path.GetFullPath(ofd.FileName)));
				if (mediaListBackup != null)
				{
					mediaListBackup.Add(FileInfo.Instance.getInfo(media));
				}
				else
				{
					Playlist.Instance.addMedia(media);
				}
				this.updatePlaylist();
			}
		}

		private void tsmiRepeat_Click(object sender, EventArgs e)
		{
			Playlist.Instance.Repeat = tsmiRepeat.Checked;
		}

		private void tsmiShuffle_Click(object sender, EventArgs e)
		{
			Playlist.Instance.Shuffle = tsmiShuffle.Checked;
		}

		private void tsmiCrossfade_Click(object sender, EventArgs e)
		{
			Player.Instance.Crossfading = tsmiCrossfade.Checked;
		}

		private void tsmiLoadPlaylist_Click(object sender, EventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.InitialDirectory = Program.MusicPath + @"\Playlists";
			ofd.CheckFileExists = true;
			ofd.CheckPathExists = true;
			ofd.DereferenceLinks = true;
			ofd.RestoreDirectory = true;
			ofd.Filter = "Playlist (*.m3u)|*.m3u|Alle Dateien (*.*)|*.*";

			if (!Directory.Exists(ofd.InitialDirectory)) Directory.CreateDirectory(ofd.InitialDirectory);

			if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				Playlist.Instance.loadFromFile(new Uri(ofd.FileName), false);
				mediaListBackup = null;
				tbxSearch.Text = "";
				this.updatePlaylist();
			}
		}

		private void savePlaylistToFileToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SaveFileDialog sfd = new SaveFileDialog();
			sfd.InitialDirectory = Program.MusicPath + @"\Playlists";
			sfd.DereferenceLinks = true;
			sfd.RestoreDirectory = true;
			sfd.Filter = "Playlist (*.m3u)|*.m3u|Alle Dateien (*.*)|*.*";
			sfd.OverwritePrompt = true;
			sfd.AddExtension = true;
			sfd.DefaultExt = "m3u";

			if (!Directory.Exists(sfd.InitialDirectory)) Directory.CreateDirectory(sfd.InitialDirectory);

			if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				Playlist.Instance.saveAsFile(sfd.FileName);
			}
		}

		private void updatePlaylist()
		{
			try
			{
				lvPlaylist.VirtualListSize = Playlist.Instance.Count;
			}
			catch (ArgumentOutOfRangeException)
			{
				if (lvPlaylist.VirtualListSize > 0) lvPlaylist.Items[0].EnsureVisible();
				lvPlaylist.VirtualListSize = Playlist.Instance.Count;
			}

			if (Playlist.Instance.Cursor >= 0 && Playlist.Instance.Cursor < lvPlaylist.VirtualListSize)
				lvPlaylist.Items[Playlist.Instance.Cursor].EnsureVisible();

			lvPlaylist.Refresh();
			queryPlaylist();
		}


		private void lvPlaylist_DoubleClick(object sender, EventArgs e)
		{
			if (lvPlaylist.SelectedIndices.Count == 1)
			{
				Playlist.Instance.Cursor = lvPlaylist.SelectedIndices[0];
				btnPlay_Click(sender, e);
			}
		}

		private void tbPosition_MouseDown(object sender, MouseEventArgs e)
		{
			Player.Instance.Paused = true;
		}

		private void tbPosition_MouseUp(object sender, MouseEventArgs e)
		{
			Player.Instance.Position = (int)tbPosition.Value * 1000;
			Player.Instance.Paused = false;
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			Media media = Playlist.Instance[Playlist.Instance.Cursor];

			try
			{
				Player.Instance.tick();
			}
			catch (InvalidOperationException)
			{
				Playlist.Instance.Cursor = Playlist.Instance.next();
				btnPlay_Click(sender, e);
			}

			//Check wenn id3-tag geändert wurde, wird ein playlist refresh ausgelöst
			if (Playlist.Instance.id3tag_edited == true)
			{
				updatePlaylist();
				Playlist.Instance.id3tag_edited = false;
			}

			if (media != null)
			{
				/*foreach (ListViewItem item in lvPlaylist.Items)
					if (item.ImageIndex == 1 && Playlist.Instance.Cursor != item.Index) item.ImageIndex = -1;

				lvPlaylist.Items[Playlist.Instance.Cursor].ImageIndex = 1;*/

				if (media.Album == null && media.Artist == null && media.Title == null)
				{
					lblTitle.Text = media.uri.ToString();
					lblTitle2.Text = "";
				}
				else
				{
					lblTitle.Text = (string)((media.Title != null && media.Title != "") ? media.Title : "(Unknown)");
					lblTitle2.Text = "by "
						+ (string)((media.Artist != null && media.Artist != "") ? media.Artist : "(Unknown)")
						+ " from "
						+ (string)((media.Album != null && media.Album != "") ? media.Album : "(Unknown)");
				}

				if (media.Length != -1)
				{
					double position = (double)Player.Instance.Position / 1000;
					string posStr = String.Format("{0:00}:{1:00}", (int)Math.Floor(position / 60), ((int)position) % 60);
					string lenStr = String.Format("{0:00}:{1:00}", media.Length / 1000 / 60, (media.Length / 1000) % 60);

					lblPosition.Text = posStr + " of " + lenStr;

					if (!Player.Instance.Paused)
					{
						tbPosition.Minimum = 0;
						tbPosition.Maximum = (int)media.Length / 1000;
						if (position != 0)
							tbPosition.Value = (int)position;
						tbPosition.Enabled = true;
					}
				}
				else
				{
					lblPosition.Text = "Streaming ...";
					tbPosition.Minimum = 0;
					tbPosition.Maximum = 1;
					tbPosition.Value = 0;
					tbPosition.Enabled = false;
				}
			}

			// Lautstärkenänderung
			if (!volumeScrolling)
				tbVolume.Value = (int)(Player.Instance.Volume * 100);
		}
		public void onMediaChanged()
		{
			pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
			Media m = Playlist.Instance[Playlist.Instance.Cursor];

			if (m != null && m.Cover != null)
			{
				Bitmap bmp = new Bitmap(m.Cover);
				pictureBox1.Image = bmp;
				pictureBox1.Update();
				pictureBox1.Refresh();
				pbMiniCover.Image = bmp;
				pbMiniCover.Update();
				pbMiniCover.Refresh();
			}
			else
			{
				pictureBox1.Image = null;
				pictureBox1.Update();
				pictureBox1.Refresh();
				pbMiniCover.Image = Properties.Resources.stdalbum;
				pbMiniCover.Update();
				pbMiniCover.Refresh();
			}
		}

		private void gui_krypton_FormClosed(object sender, FormClosedEventArgs e)
		{
			Application.Exit();
		}

		private void tvMediaLibrary_DoubleClick(object sender, EventArgs e)
		{
			TreeNode node = tvMediaLibrary.SelectedNode;

			if (node != null && node.Tag != null)
			{
				if (node == tnMediaLibrary)
				{
					MediaDB.Instance.rebuild();
					MediaDB.Instance.commit();
					Playlist.Instance.clearPlaylist();
					Playlist.Instance.addMedia(MediaDB.Instance.mediaList.ToArray());
					mediaListBackup = null;
					tbxSearch.Text = "";
					updatePlaylist();
				}
				if (node.Parent == tnPlaylists)
				{
					try
					{
						Playlist.Instance.loadFromFile((Uri)node.Tag, false);
						mediaListBackup = null;
						tbxSearch.Text = "";
					}
					catch (IOException)
					{
						MessageBox.Show("One or more files from the Playlist are already in use", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
					}

					updatePlaylist();
				}
				else if (node.Parent == tnSmartlists)
				{
					Query q = (Query)node.Tag;

					queryEngine qe = new queryEngine();
					qe.Data = MediaDB.Instance.mediaList.ToArray();
					Media[] result = qe.query(q);

					Playlist.Instance.clearPlaylist();
					Playlist.Instance.addMedia(result);

					lvPlaylist.SelectedIndices.Clear();
					if (lvPlaylist.VirtualListSize > 0) lvPlaylist.Items[0].EnsureVisible();
					lvPlaylist.VirtualListSize = Playlist.Instance.Count;
					lvPlaylist.Refresh();

				}
				else if (node.Parent == tnAudioCDs)
				{
					string drive = (string)node.Tag;
					List<Media> tracks = new List<Media>();

					foreach (string file in Directory.GetFiles(drive, "*.cda", SearchOption.TopDirectoryOnly))
					{
						tracks.Add(new Media(new Uri(file)));
					}

					Playlist.Instance.clearPlaylist();
					Playlist.Instance.addMedia(tracks.ToArray());

					lvPlaylist.SelectedIndices.Clear();
					if (lvPlaylist.VirtualListSize > 0) lvPlaylist.Items[0].EnsureVisible();
					lvPlaylist.VirtualListSize = Playlist.Instance.Count;
					lvPlaylist.Refresh();
				}
				else if (node.Parent == tnPeer2Peer)
				{
					P2PDB.Instance.Resolve((NetService)node.Tag);
				}
			}
		}

		private void toolStripSlider1_MouseDown(object sender, MouseEventArgs e)
		{
			volumeScrolling = true;
		}

		private void toolStripSlider1_MouseUp(object sender, MouseEventArgs e)
		{
			volumeScrolling = false;
		}

		private void toolStripSlider1_Scroll(object sender, ScrollEventArgs e)
		{
			Player.Instance.Volume = (float)tbVolume.Value / 100;
		}

		private void toolStripSplitButton1_Click(object sender, EventArgs e)
		{
			// bugged - abfragen ob nur der button geklickt wird
			if (!toolStripSplitButton1.DropDownButtonPressed)
			{
				Player.Instance.toggleMute();
			}
		}

		private void tsmi_Preferences_Click(object sender, EventArgs e)
		{
			gui_tsmi_preferences prefref = new gui_tsmi_preferences();
			prefref.Show();
		}

		private void gui_krypton_DragDrop(object sender, DragEventArgs e)
		{
			Array a = (Array)e.Data.GetData(DataFormats.FileDrop);

			if (a != null)
			{
				for (int i = 0; i <= a.Length - 1; i++)
				{
					string s = a.GetValue(i).ToString();
					this.BeginInvoke(m_DelegateAddMedia, new Object[] { s });

					// Falls der Explorer über unserer Form liegt ...
					this.Activate();
				}
			}
		}

		private void gui_krypton_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
				e.Effect = DragDropEffects.Copy;
			else
				e.Effect = DragDropEffects.None;
		}

		private void addMediaToPlaylist(String s)
		{
			string ext = Path.GetExtension(s);

			if (ext.ToLower().EndsWith(".m3u"))
			{
				Media[] arr = Playlist.Instance.parseM3U(new Uri(s));

				if (mediaListBackup != null)
				{
					foreach (Media m in arr)
					{
						mediaListBackup.Add(FileInfo.Instance.getInfo(m));
					}
				}
				else
				{
					foreach (Media m in arr)
					{
						Playlist.Instance.addMedia(FileInfo.Instance.getInfo(m));
					}
				}
			}
			else
			{
				Media media = new Media(new Uri(s));

				if (mediaListBackup != null)
				{
					mediaListBackup.Add(FileInfo.Instance.getInfo(media));
				}
				else
				{
					Playlist.Instance.addMedia(media);
				}
			}

			this.updatePlaylist();
		}

		private void lvPlaylist_MouseUp(object sender, MouseEventArgs e)
		{
			ListViewItem clickedItem = lvPlaylist.GetItemAt(5, e.Y);
			if (clickedItem != null)
			{
				clickedItem.Selected = true;
				clickedItem.Focused = true;
			}
		}

		private void toolStripMenuItem1_Click(object sender, EventArgs e)
		{
			gui_editTag guiEdit = new gui_editTag();
			Media tmp = Playlist.Instance[lvPlaylist.SelectedIndices[0]];
			if (tmp.Type == Media.MediaType.Audio && tmp.uri.Scheme == Uri.UriSchemeFile)
			{
				try
				{
					guiEdit.readMedium(tmp);
					guiEdit.Show();
				}
				catch (IOException)
				{
					MessageBox.Show("File already in use", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
				}
			}
			else
				MessageBox.Show("Editing is not possible via Network!\nYou can't edit/rate videos and pictures.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}

		private void starsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Media tmp = Playlist.Instance[lvPlaylist.SelectedIndices[0]];
			if (tmp.Type == Media.MediaType.Audio && tmp.uri.Scheme == Uri.UriSchemeFile)
			{
				int tmp_rating = tmp.Rating;
				try
				{
					Playlist.Instance.id3tag_edited = true;
					tmp.Rating = Convert.ToInt32((string)((ToolStripMenuItem)sender).Tag);
					tmp.commitToTag();
					lvPlaylist.Refresh();
				}
				catch (IOException)
				{
					tmp.Rating = tmp_rating;
					lvPlaylist.Refresh();
					MessageBox.Show("File already in use", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
				}
			}
			else
				if (tmp.uri.Scheme == Uri.UriSchemeHttp)
					MessageBox.Show("Editing is not possible via Network", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				else if (tmp.Type != Media.MediaType.Audio)
					MessageBox.Show("Editing is only possible on audio files", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				else
					MessageBox.Show("Editing is only possible.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}


		private void tbxSearch_TextChanged(object sender, EventArgs e)
		{
			queryPlaylist();
		}

		private void queryPlaylist()
		{
			if (tbxSearch.Text != "")
			{
				if (mediaListBackup == null)
				{
					mediaListBackup = new List<Media>();
					mediaListBackup.AddRange((Media[])Playlist.Instance);
				}

				Query.Match match = new Query.Match();
				match.lhs = Query.MatchLHS.All;
				match.op = Query.MatchOp.Contains;
				match.rhs = tbxSearch.Text;

				List<Query.Match> matches = new List<Query.Match>();
				matches.Add(match);

				Query q = new Query(matches);
				q.conj = Query.Conjunction.And;
				q.sort.by = this.StandardQuery.sort.by;
				q.sort.order = this.StandardQuery.sort.order;
				q.numberOfResults = -1;

				queryEngine qe = new queryEngine();
				qe.Data = mediaListBackup.ToArray();
				Media[] result = qe.query(q);

				Playlist.Instance.clearPlaylist();
				Playlist.Instance.addMedia(result);

				lvPlaylist.SelectedIndices.Clear();
				if (lvPlaylist.VirtualListSize > 0) lvPlaylist.Items[0].EnsureVisible();
				lvPlaylist.VirtualListSize = Playlist.Instance.Count;
				lvPlaylist.Refresh();
			}
			else
			{
				if (mediaListBackup != null)
				{
					StandardQuery.sort.by = Query.SortBy.Smart;
					StandardQuery.sort.order = Query.SortOrder.Ascending;

					Playlist.Instance.clearPlaylist();
					Playlist.Instance.addMedia(mediaListBackup.ToArray());
					mediaListBackup = null;
					updatePlaylist();
				}
			}
		}


		private void pictureBox1_Click(object sender, EventArgs e)
		{
			Media tmp = Playlist.Instance[Playlist.Instance.Cursor];

			if (tmp.Cover != null && tmp != null)
			{
				SaveFileDialog sfd = new SaveFileDialog();
				sfd.InitialDirectory = Program.MusicPath;
				sfd.DereferenceLinks = true;
				sfd.RestoreDirectory = true;
				sfd.Filter = "Bilder (*.jpg)|*.jpg|Alle Dateien (*.*)|*.*";
				sfd.OverwritePrompt = true;
				sfd.AddExtension = true;
				sfd.DefaultExt = "jpg";

				Media m = Playlist.Instance[Playlist.Instance.Cursor];
				string filename = ((m.Artist != "") ? m.Artist : "(Unknown)")
									+ " - " +
									((m.Title != "") ? m.Title : "(Unknown)");

				if (!Directory.Exists(sfd.InitialDirectory)) Directory.CreateDirectory(sfd.InitialDirectory);
				sfd.FileName = filename;
				if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					Stream ToStream = File.Create(sfd.FileName);

					Stream FromStream = ((Media)Playlist.Instance[Playlist.Instance.Cursor]).Cover;
					BinaryReader br = new BinaryReader(FromStream);
					BinaryWriter bw = new BinaryWriter(ToStream);
					bw.Write(br.ReadBytes((int)FromStream.Length));
					bw.Flush();
					bw.Close();
					br.Close();
				}
			}
		}

		private void backButton_Click(object sender, EventArgs e)
		{
			backButton.Hide();
			this.setViewmode(Viewmode.Playlist);
			Player.Instance.stop();
		}

		List<NetService> servicesList = new List<NetService>();
		public void p2p_DidRemoveService(NetServiceBrowser browser, NetService service, bool moreComing)
		{
			foreach (TreeNode item in tnPeer2Peer.Nodes)
			{
				if (item.Text == service.Name)
					tnPeer2Peer.Nodes.Remove(item);
			}
		}

		List<NetService> waitingAdd = new List<NetService>();
		public void p2p_DidFindService(NetServiceBrowser browser, NetService service, bool moreComing)
		{
			if (service.Name != System.Environment.MachineName)
			{
				for (int i = tnPeer2Peer.Nodes.Count - 1; i >= 0; i--)
				{
					TreeNode node = tnPeer2Peer.Nodes[i];

					if (node.Text == service.Name)
						node.Remove();
				}

				TreeNode newNode = new TreeNode(service.Name, 5, 5);
				newNode.Tag = service;
				tnPeer2Peer.Nodes.Add(newNode);
			}
		}

		public void p2p_DidResolveService(NetService service)
		{
			foreach (IPEndPoint addr in service.Addresses)
			{
				if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
				{
					Playlist.Instance.clearPlaylist();
					Playlist.Instance.addMedia(P2PDB.Instance.query(addr.Address, null));
					mediaListBackup = null;
					tbxSearch.Text = "";
					updatePlaylist();
				}
			}
		}

		private void gui_krypton_FormClosing(object sender, FormClosingEventArgs e)
		{
			P2PDB.Instance.stop();
		}

		private void createNewEmptyPlaylistToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Player.Instance.stop();
			Playlist.Instance.clearPlaylist();
			mediaListBackup = null;
			tbxSearch.Text = "";
			updatePlaylist();
		}

		private void deleteSelectedItemFromPlaylistToolStripMenuItem_Click(object sender, EventArgs e)
		{
			List<int> list = new List<int>();

			if (mediaListBackup != null)
			{
				if (MessageBox.Show("You can't delete an item from playlist while search is active.\nDo you want to cancel the search and switch to the complete view?", "Search active", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					Playlist.Instance.clearPlaylist();
					Playlist.Instance.addMedia(mediaListBackup.ToArray());
					mediaListBackup = null;
					tbxSearch.Text = "";
					updatePlaylist();
				}
				else
				{
					return;
				}
			}

			foreach (int i in lvPlaylist.SelectedIndices)
			{
				list.Add(i);
			}

			list.Sort();
			list.Reverse();

			foreach (int i in list)
			{
				Playlist.Instance.removeItem(i);
			}

			lvPlaylist.SelectedIndices.Clear();
			updatePlaylist();
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void activePeertoPeerNetworkToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			switch (activePeertoPeerNetworkToolStripMenuItem.Checked)
			{
				case true:
					P2PDB.Instance.start();
					Program.serv.Start(Program.MusicPath);
					break;
				case false:
					Program.serv.Stop();
					P2PDB.Instance.stop();
					tnPeer2Peer.Nodes.Clear();
					break;
			}
		}

		private void tsmiDeleteLibObj_Click(object sender, EventArgs e)
		{
			TreeNode selnode = tvMediaLibrary.SelectedNode;

			if (selnode != null)
			{
				if (selnode.Parent == tnPlaylists)
				{
					Uri uri = (Uri)selnode.Tag;
					File.Delete(uri.LocalPath);
					tnPlaylists.Nodes.Remove(selnode);
				}
				else if (selnode.Parent == tnSmartlists)
				{
					Query q = (Query)selnode.Tag;
					MediaDB.Instance.smartlists.removeSmartlist(q);
					tnSmartlists.Nodes.Remove(selnode);
					MediaDB.Instance.commit();
				}
			}
		}

		private void tvMediaLibrary_MouseDown(object sender, MouseEventArgs e)
		{
			tvMediaLibrary.SelectedNode = tvMediaLibrary.GetNodeAt(e.Location);
		}

		private void createNewSmartlistToolStripMenuItem_Click(object sender, EventArgs e)
		{
			gui_Smartlist guismartlist = new gui_Smartlist();
			guismartlist.Show();
		}

		private void editToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			TreeNode selnode = tvMediaLibrary.SelectedNode;

			if (selnode != null)
			{
				if (selnode.Parent == tnPlaylists)
				{
				}
				else if (selnode.Parent == tnSmartlists)
				{
					Query q = (Query)selnode.Tag;
					gui_Smartlist guismartlist = new gui_Smartlist();
					guismartlist.Show();
					guismartlist.loadSmartlist(q);
				}
			}
		}

		private void contextMenuStrip2_Opening(object sender, CancelEventArgs e)
		{
			TreeNode node = tvMediaLibrary.SelectedNode;

			if (node != null && node.Tag != null)
			{
				if (node.Parent == tnPlaylists)
				{
					editToolStripMenuItem1.Enabled = false;
					tsmiDeleteLibObj.Enabled = true;
				}
				else if (node.Parent == tnSmartlists)
				{
					editToolStripMenuItem1.Enabled = true;
					tsmiDeleteLibObj.Enabled = true;
				}
				else
				{
					editToolStripMenuItem1.Enabled = false;
					tsmiDeleteLibObj.Enabled = false;
				}
			}
		}

		private void aboutCrossfadeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			gui_About about = new gui_About();
			about.Show();
		}

		private void introductionToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				// launch default browser
				System.Diagnostics.Process.Start("http://www.philwo.de/crossfade.pdf");
			}
			catch (Exception)
			{
				MessageBox.Show("Could not open your default browser. Please go to http://www.philwo.de/crossfade.pdf yourself. :-)", "Default browser not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}
	}
}
