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
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;

namespace Crossfade
{
	/**
	 * Container class for the easy management of smartlists
	 * */
	public class Smartlists
	{
		private List<Query> smartlists = new List<Query>();
		
		/**
		 * Returns the smartlist with the specified name
		 * \param name The name of the smartlist
		 * */
		public Query this[string name]
		{
			get
			{
				foreach (Query q in smartlists)
				{
					if (name == q.name)
					{
						return q;
					}
				}
				return null;
			}
		}

		/**
		 * Adds or updates a smartlist in our container
		 * \param query The smartlist which should get saved
		 * */
		public void setSmartlist(Query query)
		{
			foreach (Query q in smartlists)
			{
				if (query.name == q.name)
				{
					System.Diagnostics.Debug.Print("Replaced query with name " + q.name + " with newer version");
					smartlists.Remove(q);
					break;
				}
			}

			smartlists.Add(query);
		}

		/**
		 * Removes a smartlist from our container
		 * \param query The smartlist which should get removed (only the name is checked for equivalence)
		 * */
		public void removeSmartlist(Query query)
		{
			foreach (Query q in smartlists)
			{
				if (query.name == q.name)
				{
					System.Diagnostics.Debug.Print("Deleted query with name " + q.name);
					smartlists.Remove(q);
					break;
				}
			}
		}

		/**
		 * Returns all smartlists we have stored as an array
		 * */
		public Query[] Items
		{
			get
			{
				return smartlists.ToArray();
			}
		}
	}

	/**
	 * A class which stores information about the playlists we have discovered when rebuilding our media database
	 * */
	public class PlaylistFile
	{
		/**
		 * The easy-to-read name of the playlist
		 * */
		public string name;

		/**
		 * The filename of the playlist
		 * */
		public Uri filename;

		public PlaylistFile()
		{
		}

		public PlaylistFile(Uri filename)
		{
			this.name = new System.IO.FileInfo(filename.LocalPath).Name;
			this.filename = filename;
		}
	}

	/**
	 * The absolutely uber-mediaDatabase class which
	 *  - stores information about all media objects, playlists and smartlists,
	 *  - gets automatically (de-)serialized from/into XML,
	 *  - is the persistent backing store for all our critical objects
	 * */
	[XmlRoot("mediaDB")]
	public sealed class MediaDB
	{
		static readonly MediaDB instance = MediaDB.bootstrapMediaDB();
		static private string xmlLocation;

		private List<string> watchFolders = new List<string>();
		private List<string> mediaExtensions = new List<string>();
		private List<string> playlistExtensions = new List<string>();

		[XmlElement("media")]
		public List<Media> mediaList = new List<Media>();
		public Smartlists smartlists = new Smartlists();
		[XmlIgnore]
		public List<PlaylistFile> playlists = new List<PlaylistFile>();

		private enum MDBAction
		{
			Add,
			Replace,
			Skip
		}

		static MediaDB()
		{
		}

		/**
		 * We need to consider the case, where we need the media database to exist, but we don't know yet
		 * whether we can deserialize it from stored XML-data, or whether we have to rebuild it from scratch.
		 * Thus we need a dedicated creator method which tries to deserialize and falls back to rebuilding on an error.
		 * */
		static MediaDB bootstrapMediaDB()
		{
			MediaDB mdb;
			MediaDB.xmlLocation = Program.MusicPath + @"\crossfade.xml";

			if (!File.Exists(MediaDB.xmlLocation))
			{
				mdb = new MediaDB();
			}
			else
			{
				TextReader sr = null;
				try
				{
					sr = new StreamReader(xmlLocation);
					mdb = (MediaDB)(new XmlSerializer(typeof(MediaDB)).Deserialize(sr));
					sr.Close();
				}
				catch (InvalidOperationException)
				{
					System.Diagnostics.Debug.Print("Could not load persistent media database, rebuilding ...");
					mdb = new MediaDB();
				}
				finally
				{
					if (sr != null) sr.Close();
				}
			}

			return mdb;
		}

		public static MediaDB Instance
		{
			get
			{
				return instance;
			}
		}


		/**
		 * bootstrap() methods are only used for maintaining a fixed creation order of our singletons
		 * */
		public void bootstrap()
		{
			rebuild();
		}

		MediaDB()
		{
			/**
			 * Define some file extensions which belong to media objects (only these will get indexed)
			 * */
			mediaExtensions.Add(".mp3");
			mediaExtensions.Add(".ogg");
			mediaExtensions.Add(".flac");
			mediaExtensions.Add(".wma");
			mediaExtensions.Add(".png");
			mediaExtensions.Add(".jpg");
			mediaExtensions.Add(".gif");
			mediaExtensions.Add(".flac");
			mediaExtensions.Add(".avi");
			mediaExtensions.Add(".mpg");
			mediaExtensions.Add(".ogm");
			mediaExtensions.Add(".mkv");
			mediaExtensions.Add(".wmv");
			mediaExtensions.Add(".asf");
			playlistExtensions.Add(".m3u");

			/**
			 * We index the here specified directory
			 * */
			addFolder(Program.MusicPath);
		}

		/**
		 * Serializes the MediaDB object into XML which gets stored in our music folder
		 * */
		public void commit()
		{
			XmlSerializer s = new XmlSerializer(typeof(MediaDB));
			TextWriter w = new StreamWriter(xmlLocation);
			s.Serialize(w, this);
			w.Close();
		}

		/**
		 * Adds a new media object to the database
		 * 
		 * \param media The media object which is to be added to the database
		 * */
		private void addFile(Media media)
		{
			MDBAction add;
			Media mInDB = null;

			if (media.uri.Scheme == Uri.UriSchemeFile)
			{
				System.IO.FileInfo fi = new System.IO.FileInfo(media.uri.LocalPath);

				foreach (Media m in mediaList)
				{
					if (m.uri == media.uri)
					{
						// File already in mediaDB
						System.Diagnostics.Debug.Print("Already in MDB: " + media.uri);
						mInDB = m;
						break;
					}
					//else if (m.SHA1Sum == media.SHA1Sum)
					//{
					//    // Duplicate file
					//    System.Diagnostics.Debug.Print("Duplicate file: " + media.uri + " vs. " + m.uri);
					//}
				}

				if (mInDB == null)
				{
					// If it's not in the database, then it should get added
					add = MDBAction.Add;
				}
				else
				{
					// Check the date of last modification
					if (fi.LastWriteTime != mInDB.MTime)
					{
						// It's been modified since our last indexing, replace the old version
						add = MDBAction.Replace;
					}
					else
					{
						// It's the same file, we can keep the information we already have in the database
						add = MDBAction.Skip;
					}
				}
			}
			else
			{
				/* not-file:/-URIs don't have a modification date, so we add it, if it's not in the database, and keep the version in the database, if the same URI already
				 * exists in the MediaDB */
				foreach (Media m in mediaList)
				{
					if (m.uri == media.uri)
					{
						add = MDBAction.Skip;
						break;
					}
				}

				add = MDBAction.Add;
			}

			switch (add)
			{
				case MDBAction.Add:
					System.Diagnostics.Debug.Print("Adding media: " + media.uri);
					mediaList.Add(FileInfo.Instance.getInfo(media));
					break;
				case MDBAction.Replace:
					System.Diagnostics.Debug.Print("Replacing media: " + media.uri);
					mediaList.Remove(mInDB);
					mediaList.Add(FileInfo.Instance.getInfo(media));
					break;
				case MDBAction.Skip:
					System.Diagnostics.Debug.Print("Skipping media: " + media.uri);
					break;
			}
			gui_SplashScreen.SetProgressBar(1);
		}

		private void addFolder(string folder)
		{
			watchFolders.Add(folder);
		}

		/**
		 * Checks for media objects in our watchFolders and adds them to the database.
		 * Can be used to rebuild the media database from scratch (if the database is empty,
		 * all media files need to be indexed again)
		 * */
		public void rebuild()
		{
			/* Löschen von Dateien aus der Mediendatenbank, die nicht mehr existieren */
			for (int i = mediaList.Count - 1; i >= 0; i--)
			{
				Media m = mediaList[i];

				if (m.uri.Scheme == Uri.UriSchemeFile)
				{
					if (!File.Exists(m.uri.LocalPath)) mediaList.RemoveAt(i);
				}
			}

			/* watchFolders nach neuen Mediendateien durchsuchen */
			foreach (string folder in watchFolders)
			{
				if (Directory.Exists(folder))
				{
					foreach (string file in System.IO.Directory.GetFiles(folder, "*", SearchOption.AllDirectories))
					{
						System.IO.FileInfo fi = new System.IO.FileInfo(file);

						if (mediaExtensions.Contains(fi.Extension.ToLower()))
						{
							addFile(new Media(new Uri(fi.FullName)));
						}
						else if (playlistExtensions.Contains(fi.Extension.ToLower()))
						{
							playlists.Add(new PlaylistFile(new Uri(fi.FullName)));
						}
					}
				}
			}
		}

		/**
		 * Helper function. Returns a media object which has the specified hash
		 * \param hash The hash-value of the media object we're searching for
		 * \return The media object with the specified hash-value or null, if not found
		 * */
		public Media HashToMedia(string hash)
		{
			foreach (Media m in mediaList)
			{
				if (m.SHA1Sum == hash) return m;
			}

			return null;
		}
	}

	/**
	 * The media object. Stores all information about the media (hash-value, tags, modification date, length, rating, etc.)
	 * Gets serialized into XML, so we can store it in our database
	 * */
	[XmlRoot("media")]
	public class Media
	{

		/**
		 * We describe the location of each media object with an URI, even if it's stored on the local hard-disk. This generalized handling makes it easy
		 * to implement methods which
		 *  a) work always, regardless whether it's a local file or a http-stream and
		 *  b) can easily differentiate between different types of access (local file, http-stream, crossfade-http-stream with port 21000, etc.)
		 * 
		 * Thanks to the mighty great .NET, we can't serialize the Uri object into XML so we need a funny hack (see below)
		 * */
		[XmlIgnore]
		public Uri uri
		{
			get
			{
				return _uri;
			}
			set
			{
				_uri = value;
			}
		}

		private Uri _uri;

		/* Danke .NET - wer hätte schon gedacht, dass man mal eine Uri serialisieren will! */
		/**
		 * Thank you .NET - nobody would have known in advance, that someone actually wants to serialize an Uri object ... into a STRING !!!
		 * So we create a fake property, which maps the Uri into a string, which can then be serialized by .NET to XML.
		 * */
		[XmlAttribute("uri")]
		public string _uriSerializable
		{
			get
			{
				return _uri.ToString();
			}
			set
			{
				_uri = new Uri(value);
			}
		}

		private string _sha1sum;
		private uint _tracknr;
		private string _title;
		private string _album;
		private string _artist;
		private uint _year;
		private string _genre;
		private string _comment;
		private string _composer;
		private string _copyright;
		private DateTime _mtime;
		private int _rating;
		private int _length;

		/**
		 * The SHA1-hashvalue of the media object
		 * */
		[XmlAttribute("sha1sum")]
		public string SHA1Sum
		{
			get
			{
				return _sha1sum;
			}
			set
			{
				_sha1sum = value;
			}
		}

		/**
		 * The date of last modification of the media file. Only valid if the uri scheme is file:/
		 * */
		[XmlAttribute("mtime")]
		public DateTime MTime
		{
			get
			{
				return _mtime;
			}
			set
			{
				_mtime = value;
			}
		}

		/**
		 * ID3v2: Track number
		 * */
		[XmlAttribute("tracknr")]
		public uint TrackNr
		{
			get
			{
				return _tracknr;
			}
			set
			{
				_tracknr = value;
			}
		}

		/**
		 * ID3v2: Title
		 * */
		[XmlAttribute("title")]
		public string Title
		{
			get
			{
				return _title;
			}
			set
			{
				_title = value;
			}
		}

		/**
		 * ID3v2: Album
		 * */
		[XmlAttribute("album")]
		public string Album
		{
			get
			{
				return _album;
			}
			set
			{
				_album = value;
			}
		}

		/**
		 * ID3v2: Artist
		 * */
		[XmlAttribute("artist")]
		public string Artist
		{
			get
			{
				return _artist;
			}
			set
			{
				_artist = value;
			}
		}

		/**
		 * ID3v2: Year
		 * */
		[XmlAttribute("year")]
		public uint Year
		{
			get
			{
				return _year;
			}
			set
			{
				_year = value;
			}
		}

		/**
		 * ID3v2: Genre
		 * */
		[XmlAttribute("genre")]
		public string Genre
		{
			get
			{
				return _genre;
			}
			set
			{
				_genre = value;
			}
		}

		/**
		 * ID3v2: Comment
		 * */
		[XmlAttribute("comment")]
		public string Comment
		{
			get
			{
				return _comment;
			}
			set
			{
				_comment = value;
			}
		}

		/**
		 * ID3v2: Composer
		 * */
		[XmlAttribute("composer")]
		public string Composer
		{
			get
			{
				return _composer;
			}
			set
			{
				_composer = value;
			}
		}

		/**
		 * ID3v2: Copyright
		 * */
		[XmlAttribute("copyright")]
		public string Copyright
		{
			get
			{
				return _copyright;
			}
			set
			{
				_copyright = value;
			}
		}

		/**
		 * The length of the media object in milliseconds
		 * */
		[XmlAttribute("length")]
		public int Length
		{
			get
			{
				return _length;
			}
			set
			{
				_length = value;
			}
		}

		/**
		 * Rating in stars / points between 0 (no rating) and 5 (highest rating)
		 * */
		[XmlAttribute("rating")]
		public int Rating
		{
			get
			{
				return _rating;
			}
			set
			{
				if (value >= 0 && value <= 5)
				{
					_rating = value;
				}
				else
				{
					throw new ArgumentOutOfRangeException("Rating out of Borders");
				}
			}
		}

		/** 
		 * Cover image, only valid for local audio files. Read and written by TagLib.
		 * */
		[XmlIgnore]
		public Stream Cover
		{
			get
			{
				if (uri.Scheme == Uri.UriSchemeFile)
				{
					if (Type == MediaType.Audio && !uri.LocalPath.ToLower().EndsWith(".cda"))
					{
						LibraryTagLib tl = new LibraryTagLib(uri);
						return tl.Cover;
					}
					else
						return null;
				}
				else
					return null;
			}
			set
			{
				// We need to remember the current media object and the position ...
				Media akt_media = Playlist.Instance[Playlist.Instance.Cursor];
				float last_position = 0;

				if (akt_media != null && akt_media.uri == this.uri)
				{
					last_position = Player.Instance.Position;

					// So we can stop the player to update the tag (thank you Windows) ...
					Player.Instance.stop();
				}

				LibraryTagLib tl = new LibraryTagLib(uri);

				tl.Cover = value;
				tl.save();


				if (akt_media != null && akt_media.uri == this.uri)
				{
					// and immediately re-open the media object and return to the last position, so playback continues with only a minimal gap.
					Player.Instance.open(ref akt_media);
					Player.Instance.play();
					Player.Instance.Position = last_position;
				}
			}
		}

		/**
		 * Valid media types
		 * */
		public enum MediaType
		{
			Audio,
			Picture,
			Video,
			Unknown
		}

		/**
		 * Tries to determine the type of the media object (see MediaType)
		 * */
		[XmlIgnore]
		public MediaType Type
		{
			get
			{
				if (uri.LocalPath.ToLower().EndsWith(".mp3") ||
						uri.LocalPath.ToLower().EndsWith(".wav") ||
						uri.LocalPath.ToLower().EndsWith(".ogg") ||
						uri.LocalPath.ToLower().EndsWith(".flac") ||
						uri.LocalPath.ToLower().EndsWith(".cda") ||
						uri.LocalPath.ToLower().EndsWith(".wma"))
					return MediaType.Audio;
				else if (uri.LocalPath.ToLower().EndsWith(".png") ||
						uri.LocalPath.ToLower().EndsWith(".jpg") ||
						uri.LocalPath.ToLower().EndsWith(".gif") ||
						uri.LocalPath.ToLower().EndsWith(".flac"))
					return MediaType.Picture;
				else if (uri.LocalPath.ToLower().EndsWith(".avi") ||
						uri.LocalPath.ToLower().EndsWith(".mpg") ||
						uri.LocalPath.ToLower().EndsWith(".ogm") ||
						uri.LocalPath.ToLower().EndsWith(".mkv") ||
						uri.LocalPath.ToLower().EndsWith(".wmv") ||
						uri.LocalPath.ToLower().EndsWith(".asf"))
					return MediaType.Video;
				else return MediaType.Unknown;
			}
		}

		public Media()
		{
			Length = -1;
		}

		public Media(Uri uri)
		{
			this.uri = uri;
			Length = -1;
		}

		/**
		 * Writes the changed fields to the ID3v2 tag using TagLib
		 * Only works on local audio files.
		 * */
		public void commitToTag()
		{
			if (this.uri.Scheme != Uri.UriSchemeFile) return;

			Media akt_media = Playlist.Instance[Playlist.Instance.Cursor];
			float last_position = 0;

			bool wasPlayling = Player.Instance.isPlaying;
			if (akt_media != null && akt_media.uri == this.uri && Player.Instance.isPlaying)
			{
				last_position = Player.Instance.Position;
				Player.Instance.stop();
			}
			try
			{
				LibraryTagLib tl = new LibraryTagLib(this.uri);
				tl.Tracknr = this.TrackNr;
				tl.Title = this.Title;
				tl.Album = this.Album;
				tl.Artist = this.Artist;
				tl.Year = this.Year;
				tl.Genre = this.Genre;
				tl.Comment = this.Comment;
				tl.Composer = this.Composer;
				tl.Copyright = this.Copyright;
				tl.Rating = this.Rating;
				tl.save();
			}
			catch (IOException ex)
			{
				throw ex;
			}

			if (akt_media != null && akt_media.uri == this.uri && wasPlayling)
			{
				Player.Instance.open(ref akt_media);
				Player.Instance.play();
				Player.Instance.Position = last_position;
			}
		}
	}
}
