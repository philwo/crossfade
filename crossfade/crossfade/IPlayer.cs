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

namespace Crossfade {
	/**
	 * The interface a player plug-in has to implement, so the Player singleton can use it for playing a media file.
	 * */
	public interface IPlayer
	{
		/**
		 * Opens a media file and optionally updates some information in the media object, which was discovered after opening the file
		 * \param media The media object which should get opened
		 * */
		void open(ref Media media);

		/**
		 * Start playing the previously opened media object
		 * */
		void play();

		/**
		 * Stops the playback
		 * */
		void stop();

		/**
		 * Pauses or unpauses the playback
		 * \param state true = pause, false = unpause
		 * */
		void setPaused(bool state);

		/**
		 * Returns whether the playback has been paused
		 * */
		bool getPaused();

		/**
		 * Mutes or unmutes the sound
		 * \param state true = mute, false = unmute
		 * */
		void setMuted(bool state);

		/**
		 * Returns whether the sound has been muted
		 * */
		bool getMuted();

		/**
		 * Seeks to the specified position
		 * \param position The position in milliseconds
		 * */
		void setPosition(float position);

		/**
		 * Returns the current playback position
		 * \return Playback position in milliseconds
		 * */
		float getPosition();

		/**
		 * Sets the current volume
		 * \param volume Volume between 0 and 100
		 * */
		void setVolume(float volume);

		/**
		 * Returns the current volume
		 * \return Volume between 0 and 100
		 * */
		float getVolume();

		/**
		 * Should get called about every 250ms. Allows the plug-in to do several things, like crossfading or similar
		 * */
		void tick();

		/**
		 * Returns whether the plug-in is crossfading between two tracks at the moment
		 * */
        bool getIsFading();

		/**
		 * Sets the time two tracks should get crossfaded
		 * \param ft Fadetime in milliseconds
		 * */
		void setFadetime(int ft);

		/**
		 * Sets the time a picture should get shown (because a picture has no defined length on its own)
		 * \param pic The time the picture should get shown in milliseconds
		 * */
		void setPicshowtime(int pic);
	}
}
