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
using System.Windows.Forms;
using Microsoft.DirectX.AudioVideoPlayback;

namespace Crossfade
{   /**
    *  Handles the instructions with videos. Plays videos.
    */
	class play_video : IPlayer
	{
		string filepath = null;
		Video DXvideo = null;
		float position = 0;
		int volbeforemute = -10000;

		private enum State
		{
			Playing,
			Paused,
			Stopped
		}

		private State _state;

		// Bekannter Bug bei DirectX: Laden eines Videos verursacht LoaderLock Exception.
		// Bitte unter Debug-->Exceptions-->ManagedDebuggingAsssistants den Haken bei "LoaderLock" entfernen.

		/**
		 * Opens a media file and optionally updates some information in the media object, which was discovered after opening the file
		 * \param media The media object which should get opened
		 * */   
        
        public void open(ref Media media)
		{
			setFilePathFromUri(media.uri);
			if (filepath != null)
			{
				try
				{
					PictureBox picbox;
					DXvideo = new Video(filepath, false);
					media.Length = (int)getLength();
					picbox = Program.gui.getVideoCanvas();
					DXvideo.Owner = picbox;
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}
		/**
        *  Sets the media.filepath to the current uri
		 * \parm uri the Uri of the MediaObject
        */
		private void setFilePathFromUri(Uri uri)
		{
			if (uri.Scheme == Uri.UriSchemeFile &&
				(uri.LocalPath.ToLower().EndsWith(".avi") ||
				uri.LocalPath.ToLower().EndsWith(".wmv") ||
				uri.LocalPath.ToLower().EndsWith(".mpg")))
			{
				this.filepath = (uri.Scheme == Uri.UriSchemeFile) ? (uri.LocalPath.ToString()) : (uri.ToString());
			}
			else
			{
				throw new ArgumentException("play_pic::open: Unsupported URI: " + uri);
			}
		}


		/**
		 * Start playing the previously opened media object
		 * */
		public void play() //startet gestopptes video
		{
			if (DXvideo != null)
			{
				DXvideo.Play();
				_state = State.Playing;
				position = 0;
			}
		}
		/**
		 * Stops the playback
		 * */
		public void stop()  //stoppt das Video
		{
			if (DXvideo != null)
			{
				DXvideo.Stop();
				_state = State.Stopped;
				position = 0;
			}
		}

		/**
		 * Pauses or unpauses the playback
		 * \param state true = pause, false = unpause
		 * */
        public void setPaused(bool state)
		{
			if (DXvideo != null)
			{
				if (state == true&&_state==State.Playing)
				{
					DXvideo.Pause();
				}
				if (state == false && _state == State.Playing)
				{
					DXvideo.Play();
				}
			}
		}

		/**
		  * Returns whether the playback has been paused
		  * */
		public bool getPaused()
		{
			if (DXvideo != null)
				return DXvideo.Paused;
			else
				throw new ArgumentNullException("DivX Error: No Video loaded");
		}

		/**
		 * Seeks to the specified position
		 * \param position The position in milliseconds
		 * */
        public void setPosition(float position)
		{
			if (DXvideo != null)
			{
				DXvideo.CurrentPosition = (double)position / 1000;
				this.position = position;

			}
		}

		/**
		 * Returns the current playback position
		 * \return Playback position in milliseconds
		 * */
        public float getPosition()
		{
			if (DXvideo != null)
			{
				return (float)DXvideo.CurrentPosition * 1000;
			}
			else
				throw new ArgumentNullException("DivX Error: Could not read Position");
		}

		/**
		 * Sets the current volume
		 * \param volume Volume between 0 and 100
		 * */
		public void setVolume(float vol) // 0=volle Lautstärke -10000=stumm
		{
			if (DXvideo != null)
			{
				vol = (vol - 1) * 10000f;
				DXvideo.Audio.Volume = (int)vol;
			}
		}

		/**
		 * Returns the current volume
		 * \return Volume between 0 and 100
		 * */
		public float getVolume()
		{
			if (DXvideo != null)
			{
				return ((DXvideo.Audio.Volume / 10000) + 1f);
			}
			else
				throw new ArgumentNullException("DivX Error: Could not read Volume");
		}

		
        /**
        *  Function to get the Length of the current played MediaObject
		 * \return A uint with the length of the current MediaObject in Milliseconds
        */
        public uint getLength()
		{
			if (DXvideo != null)
			{
				return (uint)DXvideo.Duration * 1000;
			}
			else
				throw new ArgumentNullException("DirectX Error: Could not read length");
		}
		/**
		 * Mutes or unmutes the sound
		 * \param state true = mute, false = unmute
		 * */
        public void setMuted(bool state)
		{
			if (DXvideo != null)
			{
				if (state == true)
				{
					this.volbeforemute = DXvideo.Audio.Volume;
					DXvideo.Audio.Volume = -10000;
				}
				if (state == false)
				{
					DXvideo.Audio.Volume = this.volbeforemute;
				}
			}
		}

		/**
		 * Returns whether the sound has been muted
		 * */
		public bool getMuted()
		{
			if (DXvideo != null)
			{
				if (DXvideo.Audio.Volume == -10000)
					return true;
				else
					return false;
			}
			else
				throw new ArgumentNullException("DivX Error: No Video loaded");
		}
        
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
		public bool getIsFading()
		{
			if (DXvideo != null)
				return false;
			else
				throw new ArgumentNullException("DivX Error: No Video loaded");
		}

		/**
		 * Sets the time two tracks should get crossfaded
		 * \param ft Fadetime in milliseconds
		 * */
		public void setFadetime(int fadetime)
		{
		}

		/**
		 * Sets the time a picture should get shown (because a picture has no defined length on its own)
		 * \param pic The time the picture should get shown in milliseconds
		 * */
        public void setPicshowtime(int picst)
		{
		}
	}
}
