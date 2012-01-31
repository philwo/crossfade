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

namespace Crossfade
{
	/**
	 * Handles ID3v2 reading / writing using TagLib-Sharp
	 * */
	public class LibraryTagLib
	{
		TagLib.File media;

		LibraryTagLib()
		{
		}

		/** 
		 * Opens a TagLib handle for the specified file
		 * */
		public LibraryTagLib(Uri file)
		{
			if (file.Scheme != Uri.UriSchemeFile)
				throw new ArgumentException("Unsupported Uri Schema!");

			try
			{
				media = TagLib.File.Create(file.LocalPath);
			}
			catch (IOException ex)
			{
				throw ex;
			}
		}

		public string Title
		{
			get
			{
				return media.Tag.Title;
			}
			set
			{
				media.Tag.Title = value;
			}
		}

		public string Artist
		{
			get
			{
				if (media.Tag.Artists.Length > 0)
					return media.Tag.Artists[0];

				return null;
			}
			set
			{
				media.Tag.Artists = value.Split(new char[] { ';' });
			}
		}

		public string Album
		{
			get
			{
				return media.Tag.Album;
			}
			set
			{
				media.Tag.Album = value;
			}
		}

		public uint Tracknr
		{
			get
			{
				return media.Tag.Track;
			}
			set
			{
				media.Tag.Track = value;
			}
		}

		public uint Year
		{
			get
			{
				return media.Tag.Year;
			}
			set
			{
				media.Tag.Year = value;
			}
		}

		public string Genre
		{
			get
			{
				return media.Tag.FirstGenre;
			}
			set
			{
				if (value != null)
					media.Tag.Genres = value.Split(new char[] { ';' });
			}
		}

		public string Comment
		{
			get
			{
				return media.Tag.Comment;
			}
			set
			{
				media.Tag.Comment = value;
			}
		}

		public string Composer
		{
			get
			{
				return media.Tag.FirstComposer;
			}
			set
			{
				if (value != null)
					media.Tag.Composers = value.Split(new char[] { ';' });
			}
		}

		public string Copyright
		{
			get
			{
				return media.Tag.Copyright;
			}
			set
			{
				media.Tag.Copyright = value;
			}
		}

		public Stream Cover
		{
			get
			{
				foreach (TagLib.IPicture picture in media.Tag.Pictures)
				{
					if (picture.Type == TagLib.PictureType.FrontCover)
					{
						MemoryStream stream = new MemoryStream(picture.Data.Data);
						return stream;
					}
				}

				return null;
			}
			set
			{
				/**
				 * Unfortunately there seems to be no way to directly save a Stream into a TagLib Cover,
				 * so we have to save it to a temporary file and load it from there to TagLib
				 * */
				string tmppath = Path.GetTempFileName();

				try
				{
					FileStream fs = new FileStream(tmppath, FileMode.Create);
					BinaryWriter bw = new BinaryWriter(fs);
					BinaryReader br = new BinaryReader(value);

					byte[] buf = new byte[value.Length];
					value.Seek(0, SeekOrigin.Begin);
					value.Read(buf, 0, (int)value.Length);
					fs.Write(buf, 0, (int)value.Length);
					fs.Close();

					media.Tag.Pictures = new TagLib.IPicture[] { TagLib.Picture.CreateFromPath(tmppath) };
					media.Save();
				}
				finally
				{
					File.Delete(tmppath);
				}
			}
		}

		public int Length
		{
			get
			{
				return (int)((media.Properties.Duration.TotalMilliseconds < 1000)
							? (1000)
							: (media.Properties.Duration.TotalMilliseconds));
			}
		}

		/**
		 * As a work-around (ID3v2 does not denote a field to save something like a rating), we save our rating in the "DiscCount" field,
		 * as it is un-used by most other players (and you will not want any other player after having tried Crossfade ;-))
		 * */
		public int Rating
		{
			get
			{
				return (int)media.Tag.DiscCount;
			}
			set
			{
				media.Tag.DiscCount = (uint)value;
			}
		}

		public void save()
		{
			media.Save();
		}

	}
}
