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

namespace Crossfade
{
	/* A query for the smartlist */
	public class Query
	{
		/* Definitionen der Query-Komponenten */
		public enum MatchLHS
		{
			All,
			SHA1,
			TrackNr,
			Title,
			Album,
			Artist,
			Rating,
			Year,
			Genre,
			Comment
		}

		public enum SortBy
		{
			Smart,
			TrackNr,
			Artist,
			Album,
			Title,
			Year
		}

		public enum MatchOp
		{
			Is,
			IsNot,
			Contains
		}

		public class Match
		{
			public MatchLHS lhs;
			public MatchOp op;
			public String rhs;
		}

		public enum SortOrder
		{
			Ascending,
			Descending,
			Random
		}

		public class Sort
		{
			public SortOrder order;
			public SortBy by;
		}

		public enum Conjunction
		{
			And,
			Or
		}

		/* Query-Komponenten */
		public string name;
		public List<Match> match = null;
		public Sort sort = new Sort();
		public int numberOfResults = -1;
		public Conjunction conj = Conjunction.And;

		/* Konstruktoren */
		Query()
		{
		}

		/** 
		*  FIXME 
		*/
		public Query(List<Match> match)
		{
			this.match = match;
		}

		/** 
		*  FIXME 
		*/
		public Query(List<Match> match, Sort sort)
		{
			this.match = match;
			this.sort = sort;
		}

		/** 
		*  FIXME 
		*/
		public Query(List<Match> match, int numberOfResults)
		{
			this.match = match;
			this.numberOfResults = numberOfResults;
		}

		/** 
		*  FIXME 
		*/
		public Query(List<Match> match, Sort sort, int numberOfResults)
		{
			this.match = match;
			this.sort = sort;
			this.numberOfResults = numberOfResults;
		}
	}

	public class queryEngine
	{
		private List<Media> _mediaList = new List<Media>();

		/** 
		*  FIXME 
		*/
		public queryEngine()
		{
		}

		/** 
		*  FIXME 
		*/
		public Media[] query(Query q)
		{
			List<Media> resultSet = new List<Media>();

			foreach (Media m in _mediaList)
			{
				bool matches;

				switch (q.conj)
				{
					case Query.Conjunction.And:
						matches = true;
						break;
					case Query.Conjunction.Or:
						matches = false;
						break;
					default:
						throw new ArgumentException("Invalid Query.Conjunction!");
				}

				foreach (Query.Match match in q.match)
				{
					bool valid = checkMediaForMatch(m, match);

					if (q.conj == Query.Conjunction.And)
					{
						if (!valid)
						{
							matches = false;
							break;
						}
						else
						{
						}
					}
					else if (q.conj == Query.Conjunction.Or)
					{
						if (!valid)
						{
						}
						else
						{
							matches = true;
							break;
						}
					}
				}

				if (matches)
					resultSet.Add(m);
			}

			// Sort
			switch (q.sort.by)
			{
				case Query.SortBy.Album:
					resultSet.Sort(new ObjectComparer<Media>("Album"));
					break;
				case Query.SortBy.Artist:
					resultSet.Sort(new ObjectComparer<Media>("Artist"));
					break;
				case Query.SortBy.Smart:
					resultSet.Sort(new ObjectComparer<Media>("Artist ASC, Album ASC, TrackNr ASC", true));
					break;
				case Query.SortBy.Title:
					resultSet.Sort(new ObjectComparer<Media>("Title"));
					break;
				case Query.SortBy.TrackNr:
					resultSet.Sort(new ObjectComparer<Media>("TrackNr"));
					break;
				case Query.SortBy.Year:
					resultSet.Sort(new ObjectComparer<Media>("Year"));
					break;
			}

			// Limit to numberOfResults
			if (q.numberOfResults > -1)
			{
				int numResults = Math.Min(q.numberOfResults, resultSet.Count);

				Media[] output = new Media[numResults];
				resultSet.CopyTo(0, output, 0, numResults);
				return output;
			}
			else
			{
				return resultSet.ToArray();
			}
		}

		private static bool checkMediaForMatch(Media m, Query.Match match)
		{
			bool valid = false;
			List<string> lhs = new List<string>(3);

			switch (match.lhs)
			{
				case Query.MatchLHS.All:
					lhs.Add(m.Artist);
					lhs.Add(m.Album);
					lhs.Add(m.Title);
					lhs.Add(m.uri.ToString());
					break;
				case Query.MatchLHS.SHA1:
					lhs.Add(m.SHA1Sum);
					break;
				case Query.MatchLHS.TrackNr:
					lhs.Add(m.TrackNr.ToString());
					break;
				case Query.MatchLHS.Title:
					lhs.Add(m.Title);
					break;
				case Query.MatchLHS.Album:
					lhs.Add(m.Album);
					break;
				case Query.MatchLHS.Artist:
					lhs.Add(m.Artist);
					break;
				case Query.MatchLHS.Rating:
					lhs.Add(m.Rating.ToString());
					break;
				case Query.MatchLHS.Year:
					lhs.Add(m.Year.ToString());
					break;
				case Query.MatchLHS.Genre:
					lhs.Add(m.Genre);
					break;
				case Query.MatchLHS.Comment:
					lhs.Add(m.Comment);
					break;
				default:
					throw new ArgumentException("Invalid matchLHS!");
			}

			foreach (string target in lhs)
			{
				try
				{
					switch (match.op)
					{
						case Query.MatchOp.Contains:
							valid = valid || target.ToUpper().Contains(match.rhs.ToUpper());
							break;
						case Query.MatchOp.Is:
							valid = valid || (String.Compare(target, match.rhs, true) == 0);
							break;
						case Query.MatchOp.IsNot:
							valid = valid || (String.Compare(target, match.rhs, true) != 0);
							break;
					}
				}
				catch (NullReferenceException)
				{
					System.Diagnostics.Debug.Print("NullReferenceException in queryEngine, not matching target");
				}
			}

			return valid;
		}

		/** 
		*  FIXME 
		*/
		public Media[] Data
		{
			get
			{
				return _mediaList.ToArray();
			}
			set
			{
				_mediaList = new List<Media>(value);
			}
		}
	}
}
