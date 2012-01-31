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
using System.Security.Cryptography;
using System.Diagnostics;

namespace Crossfade
{
	/**
	 * Helper class for FileInfo, which calculates hash-values for files
	 * */
	public class fiHash
	{
		private string filename;

		fiHash()
		{
		}

        /**
         * Creates the fiHash-object and prepares the internal fields for the call to GetFileStream.
		 * 
		 * \param file An Uri-object pointing to the file, that the hash should be calculated for. Only supports file:/ URLs!
        */
		public fiHash(Uri file)
		{
			if (file.Scheme != Uri.UriSchemeFile)
				throw new ArgumentException("Unsupported Uri Schema!");

			filename = file.LocalPath;
		}

        /**
         * Helper function to get a FileStream for a specified filename
		 * 
		 * \param pathName The file which should be returned as a stream
		 * \return The FileStream for the specified file
        */
		private FileStream GetFileStream(string pathName)
		{
			try
			{
				return (new System.IO.FileStream(pathName, System.IO.FileMode.Open,
					System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite));
			}
			catch (IOException ex)
			{
				throw ex;
			}
		}

        /**
         * Returns the SHA256 hash-value of the loaded file
        */
		public string SHA256
		{
			get
			{
				try
				{
					string strResult = "";
					string strHashData = "";

					byte[] arrbytHashValue;
					FileStream oFileStream = null;

					SHA1CryptoServiceProvider oSHA1Hasher =
						new SHA1CryptoServiceProvider();

					oFileStream = GetFileStream(filename);
					arrbytHashValue = oSHA1Hasher.ComputeHash(oFileStream);
					oFileStream.Close();

					strHashData = System.BitConverter.ToString(arrbytHashValue);
					strHashData = strHashData.Replace("-", "");
					strResult = strHashData;

					return (strResult);
				}
				catch (IOException ex)
				{
					throw ex;
				}

			}
		}
	}
}
