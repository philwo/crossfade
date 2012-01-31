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
using System.Windows.Forms;

using System.IO;
using System.Runtime.Remoting;
using System.Reflection;
using System.Security.Policy;
using System.Security;
using System.Xml.Serialization;
using System.Threading;
using System.Diagnostics;
using System.Net;

namespace Crossfade
{
	/**
	 * Our main program class, contains the Main function
	 * */
	public static class Program
	{
		public static IGUI gui;
		public static bool bIs64Bit;
		public static string MusicPath;
		public static HTTPServer serv;

		/**
		 * Callback for the Quit-element in the SysTray Contextmenu
		 * */
		static private void onQuit(System.Object sender, System.EventArgs e)
		{
			Application.Exit();
		}

		/**
		 * Callback for the Play-element in the SysTray Contextmenu
		 * */
		static private void onPlay(System.Object sender, System.EventArgs e)
		{
			if (Player.Instance.isPlaying) Player.Instance.mediaChangeIntended = true;
			Media tmp = Playlist.Instance[Playlist.Instance.Cursor];

			try
			{
				Player.Instance.stop();
				Player.Instance.open(ref tmp);
				Player.Instance.play();
			}
			catch (WebException)
			{
				MessageBox.Show("The server has locked the file. Please try again later.", "WebException", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			catch (InvalidOperationException)
			{
				onPlay(sender, e);
			}
		}

		/**
		 * Callback for the Next-element in the SysTray Contextmenu
		 * */
		static private void onNext(System.Object sender, System.EventArgs e)
		{
			Playlist.Instance.Cursor = Playlist.Instance.next();
			onPlay(sender, e);
		}

		/**
		 * Callback for the Previous-element in the SysTray Contextmenu
		 * */
		static private void onPrev(object sender, EventArgs e)
		{
			Playlist.Instance.Cursor = Playlist.Instance.prev();
			onPlay(sender, e);
		}

		/**
		 * Callback for the Stop-element in the SysTray Contextmenu
		 * */
		static private void onStop(object sender, EventArgs e)
		{
			Player.Instance.stop();
		}

		/**
		 * Callback for the Pause-element in the SysTray Contextmenu
		 * */
		static private void onPause(object sender, EventArgs e)
		{
			Player.Instance.togglePause();
		}

		/**
		 * The main entry point for the application.
		 * */
		[STAThread]
		static void Main()
		{
			/* So much as for "It's very easy to figure out if we're running on a x64 or x86 platform" ;-) */
			//bIs64Bit = (IntPtr.Size == 8);

			/**
			 * Get our music / media folder from the settings, or if set to "default", get it from Windows
			 * */
			if (Crossfade.Properties.Settings.Default.MusicFolder == "default")
			{
				MusicPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
			}
			else
			{
				MusicPath = Crossfade.Properties.Settings.Default.MusicFolder;
			}

			/**
			 * Unfortunately, x64 support is not included at this time, because our support libraries (dnssd / fmod / directx)
			 * don't support it, so we have to compile for x86 platform instead of "Any CPU".
			 * */

			/*if (!File.Exists("fmodex32.dll") ||
				!File.Exists("fmodex64.dll")) throw new FileNotFoundException("fmodex32/64.dll not found!");

			if (File.Exists("fmodex.dll")) File.Delete("fmodex.dll");

			if (bIs64Bit)
				File.Copy("fmodex64.dll", "fmodex.dll");
			else
				File.Copy("fmodex32.dll", "fmodex.dll");*/

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			/**
			 * Create the SYSTRAY-ICON
			 * */
			NotifyIcon trayicon = new NotifyIcon();
			trayicon.Text = "CrossFade";
			trayicon.Icon = Crossfade.Properties.Resources.systray;
			trayicon.Visible = true;
			trayicon.DoubleClick += new EventHandler(trayicon_DoubleClick);

			ContextMenu ctxmenu = new ContextMenu();

			ctxmenu.MenuItems.Add(new MenuItem("&Play", new EventHandler(onPlay)));
			ctxmenu.MenuItems.Add(new MenuItem("&Stop", new EventHandler(onStop)));
			ctxmenu.MenuItems.Add(new MenuItem("&Pause", new EventHandler(onPause)));
			ctxmenu.MenuItems.Add(new MenuItem("&Previous", new EventHandler(onPrev)));
			ctxmenu.MenuItems.Add(new MenuItem("&Next", new EventHandler(onNext)));
			ctxmenu.MenuItems.Add(new MenuItem("&Quit", new EventHandler(onQuit)));
			trayicon.ContextMenu = ctxmenu;

			/**
			 * Show the splash screen
			 * */
			gui_SplashScreen.ShowSplashScreen();

			// Create AppDomainSetup
			//string exeAssembly = Assembly.GetEntryAssembly().FullName;

			//AppDomainSetup ads = new AppDomainSetup();
			//ads.ApplicationBase = System.Environment.CurrentDirectory;
			//ads.DisallowBindingRedirects = false;
			//ads.DisallowCodeDownload = true;
			//ads.ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;

			//// Create Untrusted Zone
			//Evidence secInfo = new Evidence();
			//secInfo.AddHost(new Zone(SecurityZone.Untrusted));
			//Evidence secInfo = null;

			//// Create Webserver Context
			//AppDomain ad = AppDomain.CreateDomain("Webserver", secInfo, ads);
			//HTTPServer serv = (HTTPServer)ad.CreateInstanceAndUnwrap(exeAssembly, typeof(HTTPServer).FullName);
			//serv.Start(MusicPath);

			serv = null;
			try
			{
				gui_SplashScreen.SetProgressBar(5);

				/**
				 * Initialize the Player Singleton
				 * */
				gui_SplashScreen.SetStatus("Creating player ...");
				Player.Instance.bootstrap();
				gui_SplashScreen.SetProgressBar(50);

				/**
				 * Initialize the Media Database Singleton and commit it
				 * */
				gui_SplashScreen.SetStatus("Creating media library ...");
				MediaDB.Instance.bootstrap();
				MediaDB.Instance.commit();
				gui_SplashScreen.SetProgressBar(50);

				/**
				 * Initialize the Playlist Singleton
				 * */
				gui_SplashScreen.SetStatus("Creating playlist ...");
				Playlist.Instance.bootstrap();
				gui_SplashScreen.SetProgressBar(50);

				/**
				 * Initialize our GUI
				 * */
				gui_SplashScreen.SetStatus("Loading GUI ...");
				gui = (IGUI)(new gui_krypton());
				gui_SplashScreen.SetProgressBar(50);

				/**
				 * Initialize the Peer-to-Peer Filesharing Singleton
				 * */
				gui_SplashScreen.SetStatus("Creating Peer2Peer-Service ...");
				serv = new HTTPServer();
				serv.Start(MusicPath);
				P2PDB.Instance.start();
				gui_SplashScreen.SetProgressBar(-1);

				Application.Run((Form)gui);
			}
			finally
			{
				P2PDB.Instance.stop();
				if (serv != null) serv.Stop();
				//AppDomain.Unload(ad);
			}
		}

		static void trayicon_DoubleClick(object sender, EventArgs e)
		{
			gui.Show();
		}
	}
}
