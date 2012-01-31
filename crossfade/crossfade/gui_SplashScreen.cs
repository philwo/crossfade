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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

namespace Crossfade
{
	public partial class gui_SplashScreen : Form
	{
		static gui_SplashScreen mySplash = null;
		static Thread myThread = null;

		private double m_dblOpacityIncrement = .1;
		private double m_dblOpacityDecrement = .1;
		private const int TIMER_INTERVAL = 50;

		private string m_sStatus;
        private int m_sProgressBar;

		public gui_SplashScreen()
		{
			InitializeComponent();

			this.ClientSize = pictureBox1.Size;

			pictureBox1.Controls.Add(lblStatus);
			lblStatus.BackColor = Color.FromArgb(0, lblStatus.BackColor);

			this.Opacity = .0;
			timer1.Interval = TIMER_INTERVAL;
			timer1.Start();
		}

		static public void SetStatus(string newStatus)
		{
			if (mySplash == null)
				return;

			mySplash.m_sStatus = newStatus;
		}

        static public void SetProgressBar(int ProgressBar)
        {
            if (mySplash == null)
                return;

            if(ProgressBar < 0)
                 mySplash.m_sProgressBar = 500;
            else
                mySplash.m_sProgressBar = (mySplash.m_sProgressBar+ProgressBar)%500;
        }


		static private void ShowForm()
		{
			mySplash = new gui_SplashScreen();

			try
			{
				Application.Run(mySplash);
			}
			catch (InvalidOperationException)
			{
				// Quick-Hack wegen Sebastians Laptop ...
			}
			catch (Win32Exception)
			{
				// Quick-Hack wegen Sebastians Laptop ...
			}
		}

		static public void ShowSplashScreen()
		{
			if (mySplash != null)
				return;

			myThread = new Thread(new ThreadStart(gui_SplashScreen.ShowForm));
			myThread.IsBackground = true;
			myThread.SetApartmentState(ApartmentState.STA);
			myThread.Start();
		}

		static public void CloseForm()
		{
			if (mySplash != null)
			{
				mySplash.m_dblOpacityIncrement = -mySplash.m_dblOpacityDecrement;
			}

			myThread = null;
			mySplash = null;
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			if (m_dblOpacityIncrement > 0)
			{
				if (this.Opacity < 1)
					this.Opacity += m_dblOpacityIncrement;
			}
			else
			{
				if (this.Opacity > 0)
					this.Opacity += m_dblOpacityIncrement;
				else
					this.Close();
			}

			lblStatus.Text = m_sStatus;			
            pbProgress.Value = m_sProgressBar;
		}
	}
}