using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Crossfade
{
	public partial class gui_About : ComponentFactory.Krypton.Toolkit.KryptonForm
	{
		public gui_About()
		{
			InitializeComponent();
		}

		private void kryptonButton1_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}