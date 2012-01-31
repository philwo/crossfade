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
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using System.Xml.Serialization;
using ZeroconfService;

namespace Crossfade
{
	[XmlRoot("mediaDB")]
	public sealed class P2PDB_MediaDB
	{
		[XmlElement("media")]
		public List<Media> mediaList = new List<Media>();
	}

	public sealed class P2PDB
	{
		static readonly P2PDB instance = new P2PDB();
		NetService publishService;
		NetServiceBrowser nsBrowser;
		public IPAddress currentIP;

		static P2PDB()
		{
		}

		/** 
		*  FIXME 
		*/
		public static P2PDB Instance
		{
			get
			{
				return instance;
			}
		}

		P2PDB()
		{
			nsBrowser = new NetServiceBrowser();
		}

		/** 
		*  FIXME 
		*/
		public Media[] query(IPAddress ip, Query q)
		{
			try
			{
				if (q == null)
				{
					P2PDB_MediaDB mdb;

					Uri uri = new Uri("http://" + ip.ToString() + ":21000/crossfade.xml");
					HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
					HttpWebResponse response = (HttpWebResponse)request.GetResponse();

					Stream sr = response.GetResponseStream();
					mdb = (P2PDB_MediaDB)(new XmlSerializer(typeof(P2PDB_MediaDB)).Deserialize(sr));
					sr.Close();

					List<Media> newList = new List<Media>();

					foreach (Media m in mdb.mediaList)
					{
						if (m.uri.Scheme != Uri.UriSchemeFile) continue;
						if (m.SHA1Sum == null) continue;

						m.uri = new Uri("http://" + ip.ToString() + ":21000/" + m.SHA1Sum);
						newList.Add(m);
					}

					return newList.ToArray();
				}
				else
				{
					return null;
				}
			}
			catch (WebException)
			{
				return null;
			}
			catch (SocketException)
			{
				return null;
			}
		}

		NetService resolving = null;

		/** 
		*  FIXME 
		*/
		public void Resolve(NetService resolve)
		{
			if (resolving != null)
			{
				resolving.Stop();
			}

			resolve.DidResolveService += new NetService.ServiceResolved(Program.gui.p2p_DidResolveService);
			resolve.ResolveWithTimeout(10);
		}

		/** 
		*  FIXME 
		*/
		public void start()
		{
			if (Program.gui is ISynchronizeInvoke)
			{
				try
				{
					publishService = new NetService("", "_crossfade._tcp", System.Environment.MachineName, 21000);
					publishService.Publish();

					nsBrowser.InvokeableObject = (ISynchronizeInvoke)Program.gui;
					nsBrowser.DidFindService += new NetServiceBrowser.ServiceFound(Program.gui.p2p_DidFindService);
					nsBrowser.DidRemoveService += new NetServiceBrowser.ServiceRemoved(Program.gui.p2p_DidRemoveService);
					nsBrowser.SearchForService("_crossfade._tcp", "");
				}
				catch (DNSServiceException e)
				{
					System.Diagnostics.Debug.Print(String.Format("A DNSServiceException occured: {0}", e.Message));
					return;
				}
				catch (Exception e)
				{
					System.Diagnostics.Debug.Print(String.Format("An exception occured: {0}", e.Message));
					return;
				}
			}
		}

		/** 
		*  FIXME 
		*/
		public void stop()
		{
			nsBrowser.Stop();
			publishService.Stop();
		}
	}
}
