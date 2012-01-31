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
using System.Runtime.Remoting;
using System.Reflection;
using System.Security.Policy;
using System.Security;
using System.Threading;
using System.Diagnostics;

using Crossfade;

namespace Crossfade
{
	public sealed class Player
	{
		static readonly Player instance = new Player();

		private bool _crossfading = false, _muted = false, _playing = false;
		private IPlayer _player = null, _playerFMOD = null, _playerVideo = null, _playerPic = null;
		private int _fadetime = 0, _picshowtime = 0;
		public bool mediaChangeIntended = false;
		float currpos = 0;
		float globalvolume = 1.0f;

		// Explicit static constructor to tell C# compiler
		// not to mark type as beforefieldinit
		static Player()
		{
		}

		/** 
		*  returns the actual Instance of the PlayerClass
		 * \return Returns the Instance of the Player
		*/
		public static Player Instance
		{
			get { return instance; }
		}

		/** 
		*  To initiate the Playlistclass in a specific order 
		*/
		public void bootstrap()
		{
		}

		/** 
		*  Can set or get the actual state of Crossfade
		 * \param value A boolean that set the current CrossfadeState
		 * \return Returns the actual CossfadeState
		*/
		public bool Crossfading
		{
			get { return _crossfading; }
			set { _crossfading = value; }
		}

		/** 
		*  Can set or get the actual state of Paused
		 * \param value A boolean that set the current PausedState
		 * \return Returns the actual PausedState
		*/
		public bool Paused
		{
			get { if (_player != null) return _player.getPaused(); else return false; }
			set { _playing = !value; if (_player != null) _player.setPaused(value); }
		}

		/** 
		*  Can set or get the actual state of Muted
		 * \param value A boolean that set the current MutedState
		 * \return Returns the actual MutedState
		*/
		public bool Muted
		{
			get { if (_player != null) return _player.getMuted(); else return false; }
			set { _muted = value; if (_player != null) _player.setMuted(value); }
		}

		/** 
		*  Sets or Get the actual Postion of the Media
		 * \param value Sets the actual position of the Media
		 * \return the actual position from the Media
		*/
		public float Position
		{
			get { if (_player != null) return (float)_player.getPosition(); else return 0; }
			set { if (_player != null) _player.setPosition(value); }
		}

		/** 
		*  Changes the current Volume
		 * \param value a float between 0 and 1
		 * \return Returns the current volume
		*/
		public float Volume
		{
			get { if (_player != null) return (float)_player.getVolume(); else return 1; }
			set
			{
				if (_player != null)
				{
					_player.setVolume(value);
					globalvolume = value;
		}
			}
		}

		/** 
		*  Set or get the Current Fadetime
		 * \param value sets the curent Fadetime in milliseconds
		 * \return Returns the Current Fadetime
		*/
		public int Fadetime
		{
			get { return _fadetime; }
			set { _fadetime = value; }
		}

		/** 
		*  Returns the current State of FadingState
		 * \return Returns the actual Statr of FadingState
		*/
		public bool isFading
		{
			get { if (_player != null) return _player.getIsFading(); else return false; }
		}

		/** 
		*  Returns the current State of PlayingState
		 * \return Returns the actual Playing State
		*/
		public bool isPlaying
		{
			get { return _playing; }
		}

		/** 
		*  FIXME 
		*/
		Player()
		{
			System.Diagnostics.Debug.Print("Thread.GetDomain()");
			AppDomain ad = Thread.GetDomain();

			System.Diagnostics.Debug.Print("Assembly.ReflectionOnlyLoadFrom(" + System.IO.Directory.GetCurrentDirectory() + @"\play_fmod.dll" + ")");
			Assembly a = Assembly.ReflectionOnlyLoadFrom(System.IO.Directory.GetCurrentDirectory() + @"\play_fmod.dll");

			System.Diagnostics.Debug.Print("ad.Load("+a.FullName+")");
			a = ad.Load(a.FullName);

			foreach (Type t in a.GetTypes())
			{
				List<Type> interfaces = new List<Type>(t.GetInterfaces());
				if (interfaces.Contains(typeof(IPlayer)))
				{
					System.Diagnostics.Debug.Print("Found FMOD plug-in: " + t.FullName);
					_playerFMOD = (IPlayer)ad.CreateInstanceAndUnwrap(a.FullName, t.FullName);
				}
			}

			// Bekannter Bug bei DirectX: Laden eines Videos verursacht LoaderLock Exception.
			// Bitte unter Debug-->Exceptions-->ManagedDebuggingAsssistants den Haken bei "LoaderLock" entfernen.
			System.Diagnostics.Debug.Print("new play_video()");
			_playerVideo = new play_video();

			System.Diagnostics.Debug.Print("new play_pic()");
			_playerPic = new play_pic();

			System.Diagnostics.Debug.Print("_playerFMOD.setFadeTime()");
			int ft = Crossfade.Properties.Settings.Default.Fadetime;
			_playerFMOD.setFadetime(ft);
		}

		/** 
		*  Opens a Media File
		 * \param 
		*/
		public void open(ref Media file)
		{
			if (file == null) return;

			IPlayer oldPlayer = _player;

			if (file.uri.Scheme == Uri.UriSchemeFile)
			{
				switch (file.Type)
				{
					case Media.MediaType.Audio:
						_player = _playerFMOD;
						Program.gui.setViewmode(Viewmode.Playlist);
						break;
					case Media.MediaType.Picture:
						_player = _playerPic;
						Program.gui.setViewmode(Viewmode.Picture);
						break;
					case Media.MediaType.Video:
						_player = _playerVideo;
						Program.gui.setViewmode(Viewmode.Video);
						break;
					case Media.MediaType.Unknown:
						_player = null;
						break;
				}
			}
			else
			{
				_player = _playerFMOD;
			}

			if (oldPlayer != _player && oldPlayer != null)
				oldPlayer.stop();

			try
			{
			if (_player != null)
				try
				{
					_player.open(ref file);
				}
				catch (InvalidOperationException ie)
				{
					throw ie;
				}
		}
			catch (System.Net.WebException w)
			{
				throw w;
			}
		}

		/** 
		*  FIXME 
		*/
		public void play()
		{
			if (_player != null)
			{
				_playing = true;
				_player.play();
				_player.setVolume(globalvolume);
				Program.gui.onMediaChanged();
			}
		}

		/** 
		*  FIXME 
		*/
		public void stop()
		{
			if (_player != null)
			{
				_playing = false;
				_player.stop();
			}
		}

		/** 
		*  FIXME 
		*/
		public bool togglePause()
		{
			if (_player != null)
			{
				_playing = !_playing;
				return (Paused = !Paused);
			}

			return false;
		}

		/** 
		*  FIXME 
		*/
		public bool toggleMute()
		{
			return (Muted = !Muted);
		}

		/** 
		*  FIXME 
		*/
		public bool toggleCrossfade()
		{
			return (Crossfading = !Crossfading);
		}

		/** 
		*  FIXME 
		*/
		public void tick()
		{
			if (_player == null) return;

			if (!isPlaying)
			{
				currpos = 0;
				return;
			}

			_player.tick();

			float tmppos = Player.Instance.Position;

			/*Medium zu Ende*/
			if (tmppos < currpos)
			{
				if (!mediaChangeIntended && !isFading && Playlist.Instance.next() != -1)
				{
					Playlist.Instance.Cursor = Playlist.Instance.next();

					Media tmp = Playlist.Instance[Playlist.Instance.Cursor];
					if (tmp != null)
					{
						try
						{
							this.open(ref tmp);
							this.play();
						}
						catch (System.Net.WebException w)
						{
							throw w;
						}
						catch (InvalidOperationException ie)
						{
							throw ie;
						}
					}
				}

				mediaChangeIntended = false;
			}

			currpos = tmppos;
		}

		/** 
		*  FIXME 
		*/
		public int Picshowtime
		{
			get { return _picshowtime; }
			set { _picshowtime = value; }
		}
	}
}
