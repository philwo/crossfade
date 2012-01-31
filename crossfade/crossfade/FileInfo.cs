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
using System.Text.RegularExpressions;

namespace Crossfade
{
	/**
	 * Important Singleton class which tries to get as much information about a media object as possible. It calls numerous helper classes to fulfill this task, eg. TagLib, fiHash, etc.
	 * */
	public sealed class FileInfo
	{
		static readonly FileInfo instance = new FileInfo();

		/**
		 * Explicit static constructor to tell C# compiler not to mark type as beforefieldinit
		 * */
		static FileInfo()
		{
		}

		public static FileInfo Instance
		{
			get
			{
				return instance;
			}
		}

		FileInfo()
		{
		}

		/**
		 * Main function which gets passed a media object and tries to complete the missing information about this object. It calls TagLib to read ID3v2 tags, fiHash
		 * to calculate the SHA1-hashvalue, etc.
		 * 
		 * \param media The media object which should get completed.
		 * \return The media object with hopefully more information that before.
		 * */
		public Media getInfo(Media media)
		{
			if (media.uri.Scheme == Uri.UriSchemeFile)
			{
				System.IO.FileInfo fi = new System.IO.FileInfo(media.uri.LocalPath);

				if (media.MTime != fi.LastWriteTime)
				{
					System.Diagnostics.Debug.Print("MTime modified, re-reading FileInfo: " + media.uri);

					/* Filehash */
					try
					{
						if (media.Type != Media.MediaType.Video)
						{
							fiHash hash = new fiHash(media.uri);
							media.SHA1Sum = hash.SHA256;
						}

						if (media.uri.LocalPath.ToLower().EndsWith(".cda"))
						{
							media.Album = "Unknown Audio CD";
							media.Artist = "Unknown";
							media.Title = media.uri.LocalPath.ToString();
							
							/* fixme, get length of audio track here */
							//media.Length = ;

							string trackname = Path.GetFileNameWithoutExtension(media.uri.LocalPath);
							string alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ.";
							string tracknr = trackname.Trim(alphabet.ToCharArray());

							try
							{
								media.TrackNr = Convert.ToUInt32(tracknr);
							}
							catch (InvalidCastException)
							{
								media.TrackNr = 1;
							}
						}
						else if (media.Type == Media.MediaType.Audio)
						{
							/* Taglib */
							LibraryTagLib tl = new LibraryTagLib(media.uri);
							media.TrackNr = tl.Tracknr;
							media.Title = tl.Title;
							media.Album = tl.Album;
							media.Artist = tl.Artist;
							media.Year = tl.Year;
							media.Genre = tl.Genre;
							media.Comment = tl.Comment;
							media.Composer = tl.Composer;
							media.Copyright = tl.Copyright;
							media.Length = tl.Length;
							media.Rating = tl.Rating;
							if (media.Album == "" && media.Title == "" && media.Artist == "")
							{
								string media_file = media.uri.LocalPath;
								media.Title = media_file.Substring(media_file.LastIndexOf("\\") + 1);
							}
						}
						else if (media.Type == Media.MediaType.Picture)
						{
							string media_file = media.uri.LocalPath;
							media.Title = media_file.Substring(media_file.LastIndexOf("\\") + 1);
							media.Artist = "(Image)";
							media.Length = 10000;
						}
						else if (media.Type == Media.MediaType.Video)
						{
							string media_file = media.uri.LocalPath;
							media.Title = media_file.Substring(media_file.LastIndexOf("\\") + 1);
							media.Artist = "(Video)";
						}
						else
						{
							media.Title = "(Unknown)";
							media.Artist = "(Unknown)";
							media.Album = "(Unknown)";
						}

						/* LastWriteTime */
						media.MTime = fi.LastWriteTime;
					}
					catch (IOException ex)
					{
						throw ex;
					}
				}
				else
				{
					System.Diagnostics.Debug.Print("MTime not modified, skipping FileInfo: " + media.uri);
				}
			}

			return media;
		}
	}
}
