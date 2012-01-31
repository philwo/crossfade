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
using System.Net;
using System.Xml;
using System.Collections;
using System.IO;
using System.Diagnostics;

namespace Musicbrainz
{
	/** 
	*  This Class is to complete the album tag with using the Artist(Interpret) and the Title.
	*  It can also search a cover for the album.
	*/
	public class musicbrainz
	{

		InterpretInfo[] Interpreten = new InterpretInfo[25];
		int InterpretInfoCount;
		CDInfo[] CDInfos = new CDInfo[25];
		int CDInfoCount;

		/** 
		*  struct which is used to be a return-value for the musicbrainz-functions. 
		*/
		public struct CDInfo
		{
			public string cdName;
			public string cdID;
			public string coverURL;
			public string cdASIN;
		}

		/** 
		*  struct which is used to be a return-value for the musicbrainz-functions.  
		*/
		public struct InterpretInfo
		{
			public string Interpret;
			public int score;
		}


		/** 
		* Takes interpret and titel and query musicbrainz for cd which contains this track of this artist 
		* \param interpret A String which contains the artist
		* \param titel A String which contains the trackname of the titel
		* \return CDInfo struct which previously was defined.
		*/
		public CDInfo[] get_CDs(string interpret, string titel)
		{
			CDInfoCount = 0;
			System.Net.WebClient webClient = new System.Net.WebClient();
			XmlDocument xml = new XmlDocument();
			string url = "http://www.uk.musicbrainz.org/ws/1/track/?type=xml&query=" + titel + "%20AND%20artist:%22" + interpret + "%22%20AND%20type:1";
			using (Stream stream = webClient.OpenRead(url))
			{
				xml.Load(stream);
			}
			get_CDs_xml(xml, 0);            
			return CDInfos;
		}

		/** 
		*  Querys musicbrainz to get the CDs from a artist. 
		 * \param interpret A String which contains the artist
		 * \param albumtitel A String which contains the albumtitel from the artist
		 * \return a Array of a struct which was previously declared
		*/
		public CDInfo[] get_CDs_from_Album(string interpret, string albumtitel)
		{
			CDInfoCount = 0;
			System.Net.WebClient webClient = new System.Net.WebClient();
			XmlDocument xml = new XmlDocument();
			string url = "http://www.uk.musicbrainz.org/ws/1/release/?type=xml&query="+ albumtitel +"%20AND%20artist:%22" + interpret + "%22%20AND%20type:1";
			try
			{
				using (Stream stream = webClient.OpenRead(url))
				{
					xml.Load(stream);
				}
			}
			catch (WebException e)
			{
				throw e;
			}
			get_CDs_xml(xml, 0);
			return CDInfos;
			
		}

		private void get_CDs_xml(XmlNode node, int level)
		{
			if (node.NodeType == XmlNodeType.Element)
			{
				string release_id = "";
				XmlAttributeCollection nodeattributes = node.Attributes;
				for (int i = 0; i < nodeattributes.Count; i++)
				{
					if (nodeattributes[i].Name == "id" && node.Name == "release")
					{
						release_id = nodeattributes[i].Value;
						CDInfos[CDInfoCount].cdName = node.FirstChild.InnerText;
						CDInfos[CDInfoCount].cdID = release_id;

						// Mit CD-ID wird get_spec_CD aufgerufen 
						get_spec_CD(release_id);
						CDInfoCount++;
					}

				}
			}
			foreach (XmlNode child in node.ChildNodes)
			{
				get_CDs_xml(child, level + 1);
			}

		}

		//######################################################
		/** 
		*  querys musicbrainz with a CDID to analyze the response to get a ASIN for the Cover 
		* \param cdid A String which contains the CDID from musicbrainz
		*/
		private void get_spec_CD(string cdid)
		{
			// öffnet eine xml und sucht dann mithilfe der rekursive funktion die Asin darauß
			System.Net.WebClient webClient = new System.Net.WebClient();
			XmlDocument xml = new XmlDocument();
			string url = "http://www.uk.musicbrainz.org/ws/1/release/" + cdid + "?type=xml&inc=tracks";
			try
			{
				using (Stream stream = webClient.OpenRead(url))
				{
					xml.Load(stream);
				}
			}
			catch (WebException e)
			{
				throw e;
			}
			get_spec_CD_xml(xml, 0);

		}
		private void get_spec_CD_xml(XmlNode node, int level)
		{
			// Sucht Rekursiv nach der Node namens asin um mit dieser das Bild herauszubekommen
			if (node.NodeType == XmlNodeType.Element)
			{

				//box_ausgabe.Text +="\n Ausgabe: "+node.Name+"->"+node.InnerText+"\n";

				if (node.Name == "asin")
				{
					CDInfos[CDInfoCount].cdASIN = node.InnerText;
					CDInfos[CDInfoCount].coverURL = "http://ec1.images-amazon.com/images/P/" + node.InnerText + ".01.M.jpg";
				}

			}
			foreach (XmlNode child in node.ChildNodes)
			{
				get_spec_CD_xml(child, level + 1);
			}

		}

		//######################################################

		/** 
		*  Checks the Intepret. The Artist must be known by musicbrainz.
		 * \param interpret A string which contains the artist
		 * \return a Array of a previously declared struct
		*/
		public InterpretInfo[] get_Interpret(string interpret)
		{
			InterpretInfoCount = 0;
			// WebClient kann webseiten lesen
			System.Net.WebClient webClient = new System.Net.WebClient();
			// Das XML-Dokument
			XmlDocument xml = new XmlDocument();
			string url = "http://www.uk.musicbrainz.org/ws/1/artist/?type=xml&name=" + interpret;
			//box_ausgabe.Text += url+"\n";
			try
			{
				using (Stream stream = webClient.OpenRead(url))
				{
					// Dokument aus Website lesen
					xml.Load(stream);
				}
			}
			catch (WebException e) {
				throw e;
			}
			// xml verarbeiten:
			// Beispiel:
			get_Interpret_xml(xml, 0);
			return Interpreten;
		}
		private void get_Interpret_xml(XmlNode node, int level)
		{

			if (node.NodeType == XmlNodeType.Element)
			{
				bool score_boolean = false;
				string score = "";
				XmlAttributeCollection nodeattributes = node.Attributes;

				for (int i = 0; i < nodeattributes.Count; i++)
				{
					if (nodeattributes[i].Name == "ext:score")
					{
						score = nodeattributes[i].Value;
						score_boolean = true;
					}

				}
				if (score_boolean)
				{
					Interpreten[InterpretInfoCount].Interpret = node.FirstChild.InnerText;
					Interpreten[InterpretInfoCount].score = int.Parse(score);
					InterpretInfoCount++;
				}
			}
			foreach (XmlNode child in node.ChildNodes)
			{
				get_Interpret_xml(child, level + 1);
			}

		}

	}
}
