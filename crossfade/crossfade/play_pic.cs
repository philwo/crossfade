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
using System.Text;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

namespace Crossfade
{   /**
    *  Handles the instructions with Pictures. Shows Pictures.
    */ 
	class play_pic : IPlayer
	{
		string filepath = null;
		float position = 0;
		//int ft = Crossfade.Properties.Settings.Default.Fadetime;
		int picshowtime = Crossfade.Properties.Settings.Default.PictureShowTime;

		public play_pic()
		{
		}
		/**
		 * Opens a media file and optionally updates some information in the media object, which was discovered after opening the file
		 * \param media The media object which should get opened
		 * */
		public void open(ref Media media)
		{
			media.Length = Crossfade.Properties.Settings.Default.PictureShowTime;
			setFilePathFromUri(media.uri);
			
		}
        /**
        *  Sets the media.filepath to the current uri
		 * \parm uri the Uri of the MediaObject
        */
		private void setFilePathFromUri(Uri uri)
		{
			if (uri.Scheme == Uri.UriSchemeFile &&
				(uri.LocalPath.ToLower().EndsWith(".jpg") ||
				uri.LocalPath.ToLower().EndsWith(".bmp") ||
				uri.LocalPath.ToLower().EndsWith(".png") ||
				uri.LocalPath.ToLower().EndsWith(".gif")))
			{
				this.filepath = (uri.Scheme == Uri.UriSchemeFile) ? (uri.LocalPath.ToString()) : (uri.ToString());
			}
			else
			{
				throw new ArgumentException("play_pic::open: Unsupported URI: " + uri);
			}

		}
		/**
		 * Stops the playback
		 * */
		public void play()
		{
			try
			{
				FileStream fs = new FileStream(filepath, FileMode.Open);
				if (fs != null)
				{
					position = 0;
					PictureBox picbox;
					Bitmap bmp = new Bitmap(fs);
					picbox = Program.gui.getDrawingCanvas();
					picbox.SizeMode = PictureBoxSizeMode.Zoom;
					picbox.Image = bmp;
					picbox.Refresh();
					fs.Close();
				}
			}
			catch (IOException ex)
			{
				MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
			}
		}
		/**
		  * Stops the playback
		  * */
        public void stop()
		{
			position = 0;
		}
		/**
		 * Pauses or unpauses the playback
		 * \param state true = pause, false = unpause
		 * */
		public void setPaused(bool state) { }

		/**
		 * Returns whether the playback has been paused
		 * */
		public bool getPaused() { return false; }

		/**
		 * Mutes or unmutes the sound
		 * \param state true = mute, false = unmute
		 * */
		public void setMuted(bool state) { }

		/**
		 * Returns whether the sound has been muted
		 * */
		public bool getMuted() { return false; }

		/**
		 * Seeks to the specified position
		 * \param position The position in milliseconds
		 * */
        public void setPosition(float pos) 
		{
			position = position + pos;
		}

		/**
		 * Returns the current playback position
		 * \return Playback position in milliseconds
		 * */
        public float getPosition() 
		{
			return position; 
		}

		/**
		 * Sets the current volume
		 * \param volume Volume between 0 and 100
		 * */
		public void setVolume(float volume) { }

		/**
		 * Returns the current volume
		 * \return Volume between 0 and 100
		 * */
        public float getVolume() { return 1.0f; }

		/**
		 * Should get called about every 250ms. Allows the plug-in to do several things, like crossfading or similar
		 * */
		public void tick()
		{
			position = Math.Min(getLength(), position + 250);

			if (position == getLength())
			{
				stop();
			}
		}

		/**
		  * Returns whether the plug-in is crossfading between two tracks at the moment
		  * */
		public bool getIsFading() { return false; }

		/**
		 * Sets the time two tracks should get crossfaded
		 * \param ft Fadetime in milliseconds
		 * */
        public void setFadetime(int ft) { }

		/**
		 * Returns the time a picture should get shown (because a picture has no defined length on its own)
		 * \return  The time the picture should be shown in milliseconds
		 * */
        public float getLength()
		{
			return picshowtime;
		}

		/**
		 * Sets the time a picture should get shown (because a picture has no defined length on its own)
		 * \param pic The time the picture should get shown in milliseconds
		 * */
        
        public void setPicshowtime(int picst)
		{
			picshowtime = picst;
		}
	}
}
