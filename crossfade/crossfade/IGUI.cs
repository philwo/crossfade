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
using System.Diagnostics;
using ZeroconfService;

namespace Crossfade
{   /**
	 * Contains the definitions of the various viewmodes the GUI should adapt to during its life.
	 * */
	public enum Viewmode
	{
		Playlist,
		Picture,
		Video
	}

    /**
	 * Interface a Crossfade-GUI class has to implement.
	 * */
	public interface IGUI
	{
		/**
		 * A GUI needs to have some kind of Show() function which tells the GUI, that it should attract the users attention.
		 * */
		void Show();

		/**
		 * Callback which is called by the Player, when the current media has changed, for example because it ended and the next one is playing now.
		 * */
		void onMediaChanged();

		/**
		 * We need a PictureBox where we can draw nice pictures to. This function asks the GUI to return us one.
		 * \return The PictureBox where the picture is drawn to.
		 * */
		System.Windows.Forms.PictureBox getDrawingCanvas();

		/**
		 * We need a second PictureBox for videos (we can't use the first one because DirectX does some nasty stuff to it after showing a video).
		 * \return The PictureBox where the video is drawn to.
		 * */
		System.Windows.Forms.PictureBox getVideoCanvas();

		/**
		 * We need to tell the form which kind of media is played at the moment - should the Playlist be shown, or the PictureBox for pictures,
		 * or the one for videos? The GUI knows about this, after we have called this function. ;)
		 * \param vm The Viewmode the form should adapt to.
		 * */
		void setViewmode(Viewmode vm);

		/**
		 * This callback is called by the P2PDB whenever a service we discovered at some earlier time has disappeared.
		 * \param browser The NetServiceBrowser object which discovered the change
		 * \param service The service which disappeared
		 * \param moreComing A flag which tells the GUI if there are more calls to this function scheduled, so updating can be done at once
		 * */
		void p2p_DidRemoveService(NetServiceBrowser browser, NetService service, bool moreComing);

		/**
		 * This callback is called by the P2PDB whenever a new service was discovered.
		 * \param browser The NetServiceBrowser object which discovered the new service
		 * \param service The new service
		 * \param moreComing A flag which tells the GUI if there are more calls to this function scheduled, so updating can be done at once
		 * */
		void p2p_DidFindService(NetServiceBrowser browser, NetService service, bool moreComing);

		/**
		 * This callback is called by the P2PDB whenever we finished resolving an IP for a given service
		 * \param service The service which was resolved
		 * */
		void p2p_DidResolveService(NetService service);

		/**
		 * Tells the GUI, that it should check for new playlists and smartlists in the MediaDB
		 * */
		void refreshPlayAndSmartlists();
	}
}
