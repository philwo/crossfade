// CsHTTPServer
//
// rmortega77@yahoo.es
// The use of this software is subject to the following agreement
//
// 1. Don't use it to kill.
// 2. Don't use to lie.
// 3. If you learned something give it back.
// 4. If you make money with it, consider sharing with the author.
// 5. If you do not complies with 1 to 5, you may not use this software.
//
// If you have money to spare, and found useful, or funny, or anything 
// worth on this software, and want to contribute with future free 
// software development.
// You may contact the author at rmortega77@yahoo.es 
// Contributions can be from money to hardware spareparts (better), or 
// a bug fix (best), or printed bibliografy, or thanks... 
// just write me.

using System;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Reflection;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Xml;
using System.Xml.Serialization;

using Microsoft.Win32;

namespace Crossfade
{
	public class HTTPServer : CsHTTPServer
	{
		public string Folder;

		public HTTPServer()	: base()
		{
			this.Folder = null;
		}

		public override void OnResponse(ref HTTPRequestStruct rq, ref HTTPResponseStruct rp)
		{
			string path = this.Folder + "\\" + rq.URL.PathAndQuery.ToString().Replace("/", "\\");
			Media media = null;

			if (Directory.Exists(path))
			{
				if (File.Exists(path + "default.htm"))
					path += "\\default.htm";
				else
				{
					string[] dirs = Directory.GetDirectories(path);
					string[] files = Directory.GetFiles(path);

					rp.Headers["Content-Type"] = "text/html";

					string bodyStr = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\">\n";
					bodyStr += "<HTML><HEAD>\n";
					bodyStr += "<META http-equiv=Content-Type content=\"text/html; charset=windows-1252\">\n";
					bodyStr += "</HEAD>\n";
					bodyStr += "<BODY><p>Folder listing, to do not see this add a 'default.htm' document\n<p>\n";
					for (int i = 0; i < dirs.Length; i++)
						bodyStr += "<br><a href = \"" + rq.URL + Path.GetFileName(dirs[i]) + "/\">[" + Path.GetFileName(dirs[i]) + "]</a>\n";
					for (int i = 0; i < files.Length; i++)
						bodyStr += "<br><a href = \"" + rq.URL + Path.GetFileName(files[i]) + "\">" + Path.GetFileName(files[i]) + "</a>\n";
					bodyStr += "</BODY></HTML>\n";

					rp.BodyData = Encoding.ASCII.GetBytes(bodyStr);
					return;
				}
			}

			if ((media = MediaDB.Instance.HashToMedia(rq.URL.PathAndQuery.TrimStart(new char[] { '/' }))) != null)
			{
				System.Diagnostics.Debug.Assert(media.uri.Scheme == Uri.UriSchemeFile);
				path = media.uri.LocalPath;
			}

			if (rq.URL.PathAndQuery.ToString() == "/crossfade.xml")
			{
				P2PDB_MediaDB mdb = new P2PDB_MediaDB();
				mdb.mediaList = MediaDB.Instance.mediaList;

				XmlSerializer s = new XmlSerializer(typeof(P2PDB_MediaDB));
				Stream w = new MemoryStream();
				s.Serialize(w, mdb);
				w.Seek(0, SeekOrigin.Begin);
				rp.fs = w;

				rp.Headers["Content-Type"] = "text/xml";
				rp.Headers["Last-Modified"] = DateTime.Now.ToString("r");
				rp.Headers["Accept-Ranges"] = "none";
				rp.Headers["Content-Length"] = w.Length;
				rp.Headers["Connection"] = "close";
			}
			else if (File.Exists(path))
			{
				RegistryKey rk = Registry.ClassesRoot.OpenSubKey(Path.GetExtension(path), true);

				// Get the data from a specified item in the key.
				String s = (String)rk.GetValue("Content Type");

				// Open the stream and read it back.
				rp.fs = File.Open(path, FileMode.Open);
				if (s != "")
				    rp.Headers["Content-Type"] = s;

				System.IO.FileInfo fi = new System.IO.FileInfo(path);
				rp.Headers["Last-Modified"] = fi.LastWriteTime.ToString("r");
				rp.Headers["Accept-Ranges"] = "none";
				rp.Headers["Content-Length"] = fi.Length.ToString();
				rp.Headers["Connection"] = "close";
			}
			else
			{
				rp.status = (int)RespState.NOT_FOUND;
				rp.Headers["Accept-Ranges"] = "none";
				rp.Headers["Connection"] = "close";
				rp.Headers["Content-Type"] = "text/html";

				string bodyStr = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\">\n";
				bodyStr += "<HTML><HEAD>\n";
				bodyStr += "<META http-equiv=Content-Type content=\"text/html; charset=windows-1252\">\n";
				bodyStr += "</HEAD>\n";
				bodyStr += "<BODY>File not found!!</BODY></HTML>\n";

				rp.BodyData = Encoding.ASCII.GetBytes(bodyStr);
			}
		}

		public override void Start(string theFolder)
		{
			this.Folder = theFolder;
			base.Start(theFolder);
		}
	}

	/// <summary>
	/// Summary description for CsHTTPServer.
	/// </summary>
	public abstract class CsHTTPServer : MarshalByRefObject
	{
		private int portNum = 21000;
		private TcpListener listener;
		System.Threading.Thread Thread;

		public Hashtable respStatus;

		public string Name = "Crossfade/1.0";

		public bool IsAlive
		{
			get
			{
				return this.Thread.IsAlive;
			}
		}

		public CsHTTPServer()
		{
			respStatusInit();
		}

		public CsHTTPServer(int thePort)
		{
			portNum = thePort;
			respStatusInit();
		}

		private void respStatusInit()
		{
			respStatus = new Hashtable();

			respStatus.Add(200, "200 OK");
			respStatus.Add(201, "201 Created");
			respStatus.Add(202, "202 Accepted");
			respStatus.Add(204, "204 No Content");

			respStatus.Add(301, "301 Moved Permanently");
			respStatus.Add(302, "302 Redirection");
			respStatus.Add(304, "304 Not Modified");

			respStatus.Add(400, "400 Bad Request");
			respStatus.Add(401, "401 Unauthorized");
			respStatus.Add(403, "403 Forbidden");
			respStatus.Add(404, "404 Not Found");

			respStatus.Add(500, "500 Internal Server Error");
			respStatus.Add(501, "501 Not Implemented");
			respStatus.Add(502, "502 Bad Gateway");
			respStatus.Add(503, "503 Service Unavailable");
		}

		public void Listen()
		{

			bool done = false;

			listener = new TcpListener(portNum);
			//listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			listener.Start();

			WriteLog("Listening On: " + portNum.ToString());

			while (!done)
			{
				WriteLog("Waiting for connection...");

				try
				{
					CsHTTPRequest newRequest = new CsHTTPRequest(listener.AcceptTcpClient(), this);
					Thread Thread = new Thread(new ThreadStart(newRequest.Process));
					Thread.Name = "HTTP Request";
					Thread.Start();
				}
				catch (SocketException)
				{
					System.Diagnostics.Debug.Print("SocketException on CsHTTPRequest-Loop!");
				}
			}

		}

		public void WriteLog(string EventMessage)
		{
			System.Diagnostics.Debug.Print(EventMessage);
		}

		public virtual void Start(string theFolder)
		{
			this.Thread = new Thread(new ThreadStart(this.Listen));
			this.Thread.Start();
		}

		public void Stop()
		{
			listener.Stop();
			this.Thread.Abort();
		}

		public abstract void OnResponse(ref HTTPRequestStruct rq, ref HTTPResponseStruct rp);
	}
}
