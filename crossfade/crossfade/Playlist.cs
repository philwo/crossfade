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
using System.Diagnostics;

using Crossfade;

namespace Crossfade
{
	/**
	 * Playlist class which contains all parts for the playlist
	 */
	public sealed class Playlist
	{
		static readonly Playlist instance = new Playlist();

		public bool id3tag_edited = true;
		private int _cursor = 0, _count = 0;
		private bool _repeat = false, _shuffle = false;
		private List<Media> mediaList = new List<Media>();

		/**
		 * Explicit static constructor to tell C# compiler
		 * not to mark type as beforefieldinit
		 */
		static Playlist()
		{
		}

		Playlist()
		{
		}

		/** 
		*  To Get the actual instance of the playlist.
		 * \return the instance of the actual playlist class.
		*/
		public static Playlist Instance
		{
			get
			{
				return instance;
			}
		}

		/** 
		*  To initiate the Playlistclass in a specific order 
		*/
		public void bootstrap()
		{
		}

		/** 
		*  To cast a Playlist in an MediaArray
		 * \param Playlist A playlist which contains media
		 * \return A Array with Media-Objects
		*/
		public static explicit operator Media[](Playlist p)
		{
			return p.mediaList.ToArray();
		}

		/** 
		 * A Function to get the Media Object on a specific index in the playlist
		*  \param pos A Integer which indentificate a Media Object in the current Playlist
		 * \return A MediaObject 
		*/
		public Media this[int pos]
		{
			get
			{
				if (pos >= 0 &&	pos < mediaList.Count)
				{
					return mediaList[pos];
				}

				return null;
			}
			set
			{
				if (pos >= 0 &&
					pos < mediaList.Count)
				{
					mediaList[pos] = value;
				}
			}
		}

		/** 
		*  Can set or return the current Index of a Media
		 * \param value A Integer to set the change MediaObjectIndex
		 * \return Returns the Index of the current played MediaObject in Playlist
		*/
		public int Cursor
		{
			get
			{
				return _cursor;
			}

			set
			{
				if (value >= -1 &&
					value < mediaList.Count)
				{
					_cursor = value;
				}
			}
		}

		/** 
		*  Can set or get the actual state of Repeat
		 * \param value A boolean that set the current RepeatState
		 * \return Returns the actual RepeatState
		*/
		public bool Repeat
		{
			get
			{
				return _repeat;
			}
			set
			{
				_repeat = value;
			}
		}

		/** 
		*  Can set or get the actual state of Shuffle
		 * \param value A boolean that set the current ShuffleState
		 * \return Returns the actual ShuffleState
		*/
		public bool Shuffle
		{
			get
			{
				return _shuffle;
			}
			set
			{
				_shuffle = value;
			}
		}

		/** 
		*  Returns the next Index of the MediaFile that should played as next. It consider the actual state of Shuffle and Repeat
		 * \return Returns an integer that announces the index of the next MediaFile that should play.
		*/
		public int next()
		{
			if (Shuffle == true)
			{
				return new Random().Next(0, Count);
			}
			else
			{
				// Wenn wir auf dem letzten Lied sind ...
				if (Cursor + 1 == Count)
				{
					if (Repeat == true)
					{
						return 0;
					}
					else
					{
						return -1;
					}
				}
				else
				{
					return Cursor + 1;
				}
			}
		}

		/** 
		*  To get the previous Media-File that played. If Shuffle is true, it only return a new random Index.
		 * \return An integer that points to the Index of the previous played file.
		*/
		public int prev()
		{
			if (Shuffle == true)
			{
				return new Random().Next(0, Count);
			}
			else
			{
				// Wenn wir auf dem letzten Lied sind ...
				if (Cursor - 1 < 0)
				{
					if (Repeat == true)
					{
						return Count - 1;
					}
					else
					{
						return -1;
					}
				}
				else
				{
					return _cursor - 1;
				}
			}
		}

		/** 
		*  To switch the Repeat from on to false an he other way round
		 * \return A boolean with the actual RepeatState
		*/
		public bool toggleRepeat()
		{
			return (Repeat = !Repeat);
		}

		/** 
		*  To switch the Shuffle from on to false an he other way round
		 * \return A boolean with the actual ShuffleState
		*/
		public bool toggleShuffle()
		{
			return (Shuffle = !Shuffle);
		}

		/** 
		*  Removes an Item from the actual displayed playlist with the index
		 * \param index The internal index from the MediaObject in the playlist
		*/
		public void removeItem(int index)
		{
			if (index >= 0 && index < mediaList.Count)
				mediaList.RemoveAt(index);
		}

		/** 
		*  Clears the current playlist. 
		*/
		public void clearPlaylist()
		{
			mediaList.Clear();
		}

		/** 
		*  Add a Media to the current playlist.
		 * \param media A Mediaobject that sould be added.
		*/
		public void addMedia(Media media)
		{
			if (media == null) return;

			mediaList.Add(FileInfo.Instance.getInfo(media));
		}

		/** 
		*  Adds an Array of Medias to the current playlist
		 * \param media An array with one or more MediaObjects
		*/
		public void addMedia(Media[] media)
		{
			if (media == null) return;

			foreach (Media m in media)
				addMedia(m);
		}

		/** 
		*  Loads an File and can append to the current Playlist
		 * \param filename A Uri that references a m3u-Playlist
		 * \param append A Flag to check if the File, that should be added, should append to current playlist
		*/
		public void loadFromFile(Uri filename, bool append)
		{
			if (filename.Scheme != Uri.UriSchemeFile) throw new ArgumentException("We can only load playlists from the local disk at the moment!");
			if (!append) clearPlaylist();

			addMedia(parseM3U(filename));
		}

		/** 
		*  A Function which parses an m3u-file and return MediaObjects that can used in other Programmparts
		 * \param filename A Uri which references an m3u-Playlist
		 * \return Returns an Array of MediaObjects parsed from m3u-File
		*/
		public Media[] parseM3U(Uri filename)
		{
			StreamReader sr = new StreamReader(filename.LocalPath, Encoding.UTF8, true);
			String s = "";
			List<Media> newList = new List<Media>();

			String oldpath = System.IO.Directory.GetCurrentDirectory();
			System.IO.Directory.SetCurrentDirectory(System.IO.Path.GetDirectoryName(filename.LocalPath));

			while ((s = sr.ReadLine()) != null)
			{
				try
				{
					Uri tmp;

					try
					{
						// Erstmal versuchen, ob es schon ein gültiger URI ist
						tmp = new Uri(s);
					}
					catch (UriFormatException)
					{
						try
						{
							// Ist es nicht, dann ist es wohl einfach ein Dateiname ...
							tmp = new Uri(Path.GetFullPath(s));
						}
						catch (NotSupportedException)
						{
							// Winamp-M3U-Comment entdeckt. Böse.
							continue;
						}
						catch (UriFormatException)
						{
							// Immer noch nicht! Diese Datei überspringen.
							continue;
						}
						catch (ArgumentException)
						{
							// Irgendwas ganz komisches. Vermutlich ein leerer String.
							continue;
						}
					}
					try
					{
						if (tmp.Scheme == Uri.UriSchemeFile)
						{
							if (File.Exists(tmp.LocalPath))
							{
								newList.Add(new Media(tmp));
							}
						}
						else
						{
							newList.Add(new Media(tmp));
						}
					}
					catch (IOException ex)
					{
						throw ex;
					}
				}
				catch (FileNotFoundException)
				{
				}
			}

			return newList.ToArray();
		}

		/** 
		*  Saves the actual shown Playlist in an m3u-Playlist
		 * \param filename A string to the m3u-Playlist, that should contain the paths to the MediaObjects
		*/
		public void saveAsFile(string filename)
		{
			StreamWriter sw = new StreamWriter(filename, false, Encoding.UTF8);

			foreach (Media m in mediaList)
			{
				if (m.uri.Scheme == Uri.UriSchemeFile)
					sw.WriteLine(m.uri.LocalPath);
				else
					sw.WriteLine(m.uri);
			}

			sw.Close();
		}

		/** 
		*  Sort the actual PLaylist with usage from the comparer
		 * \param comparer The Comparer that should be used to compare two MediaObjects
		*/
		public void Sort(IComparer<Media> comparer)
		{
			mediaList.Sort(comparer);
		}

		/** 
		*  Set or get the maximum from the actual Playlist
		 * \return An integer that contains the index of the least element in playlist
		*/
		public int Count
		{
			get
			{
				_count = mediaList.Count;
				return _count;
			}
			set
			{
				//FIXME
				_count = value;
			}
		}
	}
}
