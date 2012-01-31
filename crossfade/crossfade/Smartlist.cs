using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Crossfade
{
    public sealed class Smartlist
    {
        /* Sort order */
        public enum Sort { Title, Artist, Album, Year, Genre, Rating };

        /* Smartlist singleton */
		static readonly Smartlist instance = new Smartlist();

		// Explicit static constructor to tell C# compiler
		// not to mark type as beforefieldinit
		static Smartlist()
		{
		}

		Smartlist()
		{
		}

		public static Smartlist Instance
		{
			get
			{
				return instance;
			}
		}
    }
}
