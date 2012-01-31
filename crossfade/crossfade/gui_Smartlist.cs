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

namespace Crossfade
{
	public partial class gui_Smartlist : ComponentFactory.Krypton.Toolkit.KryptonForm
	{
		public gui_Smartlist()
		{
			InitializeComponent();
		}

		private void gui_Smartlist_Load(object sender, EventArgs e)
		{
			cbxMethod.SelectedIndex = 0;
			cbxSortBy.SelectedIndex = 0;
			cbxSortOrder.SelectedIndex = 0;
			tbName.Focus();
		}

		public void loadSmartlist(Query q)
		{
			tbName.Text = q.name;
			tbNumResults.Text = (q.numberOfResults == -1) ? "" : q.numberOfResults.ToString();

			switch (q.conj)
			{
				case Query.Conjunction.And:
					cbxMethod.SelectedIndex = 0;
					break;
				case Query.Conjunction.Or:
					cbxMethod.SelectedIndex = 1;
					break;
			}

			switch (q.sort.by)
			{
				case Query.SortBy.Smart:
					cbxSortBy.SelectedIndex = 0;
					break;
				case Query.SortBy.TrackNr:
					cbxSortBy.SelectedIndex = 1;
					break;
				case Query.SortBy.Artist:
					cbxSortBy.SelectedIndex = 2;
					break;
				case Query.SortBy.Album:
					cbxSortBy.SelectedIndex = 3;
					break;
				case Query.SortBy.Title:
					cbxSortBy.SelectedIndex = 4;
					break;
				case Query.SortBy.Year:
					cbxSortBy.SelectedIndex = 5;
					break;
			}

			switch (q.sort.order)
			{
				case Query.SortOrder.Ascending:
					cbxSortOrder.SelectedIndex = 0;
					break;
				case Query.SortOrder.Descending:
					cbxSortOrder.SelectedIndex = 1;
					break;
			}

			for (int i = 0; i < 6 && i < q.match.Count; i++)
			{
				Query.Match m = q.match[i];

				int lhs, op;

				switch (m.lhs)
				{
					case Query.MatchLHS.All:
						lhs = 0;
						break;
					case Query.MatchLHS.SHA1:
						lhs = 1;
						break;
					case Query.MatchLHS.TrackNr:
						lhs = 2;
						break;
					case Query.MatchLHS.Title:
						lhs = 3;
						break;
					case Query.MatchLHS.Album:
						lhs = 4;
						break;
					case Query.MatchLHS.Artist:
						lhs = 5;
						break;
					case Query.MatchLHS.Rating:
						lhs = 6;
						break;
					case Query.MatchLHS.Year:
						lhs = 7;
						break;
					case Query.MatchLHS.Genre:
						lhs = 8;
						break;
					case Query.MatchLHS.Comment:
						lhs = 9;
						break;
					default:
						lhs = 0;
						break;
				}

				switch (m.op)
				{
					case Query.MatchOp.Is:
						op = 0;
						break;
					case Query.MatchOp.IsNot:
						op = 1;
						break;
					case Query.MatchOp.Contains:
						op = 2;
						break;
					default:
						op = 0;
						break;
				}

				switch (i)
				{
					case 0:
						cbxMatch1.SelectedIndex = lhs;
						cbxOp1.SelectedIndex = op;
						tbWith1.Text = m.rhs;
						break;
					case 1:
						cbxMatch2.SelectedIndex = lhs;
						cbxOp2.SelectedIndex = op;
						tbWith2.Text = m.rhs;
						break;
					case 2:
						cbxMatch3.SelectedIndex = lhs;
						cbxOp3.SelectedIndex = op;
						tbWith3.Text = m.rhs;
						break;
					case 3:
						cbxMatch4.SelectedIndex = lhs;
						cbxOp4.SelectedIndex = op;
						tbWith4.Text = m.rhs;
						break;
					case 4:
						cbxMatch5.SelectedIndex = lhs;
						cbxOp5.SelectedIndex = op;
						tbWith5.Text = m.rhs;
						break;
					case 5:
						cbxMatch6.SelectedIndex = lhs;
						cbxOp6.SelectedIndex = op;
						tbWith6.Text = m.rhs;
						break;
				}
			}
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			if (tbName.Text == "")
			{
				MessageBox.Show("Sie müssen der Smartlist einen Namen geben!", "Smartlist", MessageBoxButtons.OK, MessageBoxIcon.Stop);
				return;
			}

			if (tbWith1.Text == "")
			{
				MessageBox.Show("You have to specify at least the first query component!", "Smartlist", MessageBoxButtons.OK, MessageBoxIcon.Stop);
				return;
			}

			if ((tbWith1.Text != "" && (cbxMatch1.SelectedIndex == -1 || cbxOp1.SelectedIndex == -1)) ||
				(tbWith2.Text != "" && (cbxMatch2.SelectedIndex == -1 || cbxOp2.SelectedIndex == -1)) ||
				(tbWith3.Text != "" && (cbxMatch3.SelectedIndex == -1 || cbxOp3.SelectedIndex == -1)) ||
				(tbWith4.Text != "" && (cbxMatch4.SelectedIndex == -1 || cbxOp4.SelectedIndex == -1)) ||
				(tbWith5.Text != "" && (cbxMatch5.SelectedIndex == -1 || cbxOp5.SelectedIndex == -1)) ||
				(tbWith6.Text != "" && (cbxMatch6.SelectedIndex == -1 || cbxOp6.SelectedIndex == -1)))
			{
				MessageBox.Show("If you enter a right-hand-side, you have to select the match-target and the operator!", "Smartlist", MessageBoxButtons.OK, MessageBoxIcon.Stop);
				return;
			}

			List<Query.Match> matchList = new List<Query.Match>();

			Query.Match m1 = createMatch(tbWith1.Text, cbxMatch1.SelectedIndex, cbxOp1.SelectedIndex);
			Query.Match m2 = createMatch(tbWith2.Text, cbxMatch2.SelectedIndex, cbxOp2.SelectedIndex);
			Query.Match m3 = createMatch(tbWith3.Text, cbxMatch3.SelectedIndex, cbxOp3.SelectedIndex);
			Query.Match m4 = createMatch(tbWith4.Text, cbxMatch4.SelectedIndex, cbxOp4.SelectedIndex);
			Query.Match m5 = createMatch(tbWith5.Text, cbxMatch5.SelectedIndex, cbxOp5.SelectedIndex);
			Query.Match m6 = createMatch(tbWith6.Text, cbxMatch6.SelectedIndex, cbxOp6.SelectedIndex);

			if (m1 != null) matchList.Add(m1);
			if (m2 != null) matchList.Add(m2);
			if (m3 != null) matchList.Add(m3);
			if (m4 != null) matchList.Add(m4);
			if (m5 != null) matchList.Add(m5);
			if (m6 != null) matchList.Add(m6);

			Query q = new Query(matchList);

			q.name = tbName.Text;

			try
			{
				if (tbNumResults.Text == "")
					q.numberOfResults = -1;
				else
					q.numberOfResults = Convert.ToInt32(tbNumResults.Text);
			}
			catch (InvalidCastException)
			{
				MessageBox.Show("You have to enter a valid number in the 'Maximum number of results' field", "Smartlist", MessageBoxButtons.OK, MessageBoxIcon.Stop);
				return;
			}

			switch (cbxMethod.SelectedIndex)
			{
				case 0:
					q.conj = Query.Conjunction.And;
					break;
				case 1:
					q.conj = Query.Conjunction.Or;
					break;
			}

			switch (cbxSortBy.SelectedIndex)
			{
				case 0:
					q.sort.by = Query.SortBy.Smart;
					break;
				case 1:
					q.sort.by = Query.SortBy.TrackNr;
					break;
				case 2:
					q.sort.by = Query.SortBy.Artist;
					break;
				case 3:
					q.sort.by = Query.SortBy.Album;
					break;
				case 4:
					q.sort.by = Query.SortBy.Title;
					break;
				case 5:
					q.sort.by = Query.SortBy.Year;
					break;
			}

			switch (cbxSortOrder.SelectedIndex)
			{
				case 0:
					q.sort.order = Query.SortOrder.Ascending;
					break;
				case 1:
					q.sort.order = Query.SortOrder.Descending;
					break;
			}

			System.Diagnostics.Debug.Print("narf");
			MediaDB.Instance.smartlists.setSmartlist(q);
			MediaDB.Instance.commit();
			Program.gui.refreshPlayAndSmartlists();
			Close();
		}

		private Query.Match createMatch(string rhs, int cbxMatch, int cbxOp)
		{
			Query.Match m = null;

			if (rhs != "")
			{
				m = new Query.Match();

				switch (cbxMatch)
				{
					case 0:
						m.lhs = Query.MatchLHS.All;
						break;
					case 1:
						m.lhs = Query.MatchLHS.SHA1;
						break;
					case 2:
						m.lhs = Query.MatchLHS.TrackNr;
						break;
					case 3:
						m.lhs = Query.MatchLHS.Title;
						break;
					case 4:
						m.lhs = Query.MatchLHS.Album;
						break;
					case 5:
						m.lhs = Query.MatchLHS.Artist;
						break;
					case 6:
						m.lhs = Query.MatchLHS.Rating;
						break;
					case 7:
						m.lhs = Query.MatchLHS.Year;
						break;
					case 8:
						m.lhs = Query.MatchLHS.Genre;
						break;
					case 9:
						m.lhs = Query.MatchLHS.Comment;
						break;
				}

				switch (cbxOp)
				{
					case 0:
						m.op = Query.MatchOp.Is;
						break;
					case 1:
						m.op = Query.MatchOp.IsNot;
						break;
					case 2:
						m.op = Query.MatchOp.Contains;
						break;
				}

				m.rhs = rhs;
			}

			return m;
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}