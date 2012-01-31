using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Reflection;
using System.Diagnostics;

namespace Crossfade
{
	public class ObjectComparer<ComparableObject> : IComparer<ComparableObject>
	{
		#region Constructor
		public ObjectComparer()
		{
		}

		public ObjectComparer(string p_propertyName)
		{
			//We must have a property name for this comparer to work
			this.PropertyName = p_propertyName;
		}

		public ObjectComparer(string p_propertyName, bool p_MultiColumn)
		{
			//We must have a property name for this comparer to work
			this.PropertyName = p_propertyName;
			this.MultiColumn = p_MultiColumn;
		}
		#endregion

		#region Property
		private bool _MultiColumn;
		private string _propertyName;

		public bool MultiColumn
		{
			get
			{
				return _MultiColumn;
			}
			set
			{
				_MultiColumn = value;
			}
		}

		public string PropertyName
		{
			get
			{
				return _propertyName;
			}
			set
			{
				_propertyName = value;
			}
		}
		#endregion

		#region IComparer<ComparableObject> Members
		/// <summary>
		/// This comparer is used to sort the generic comparer
		/// The constructor sets the PropertyName that is used
		/// by reflection to access that property in the object to 
		/// object compare.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public int Compare(ComparableObject x, ComparableObject y)
		{
			Type t = x.GetType();

			if (_MultiColumn) // Multi Column Sorting
			{
				string[] sortExpressions = _propertyName.Trim().Split(',');

				for (int i = 0; i < sortExpressions.Length; i++)
				{
					string fieldName, direction = "ASC";

					if (sortExpressions[i].Trim().EndsWith(" DESC"))
					{
						fieldName = sortExpressions[i].Replace(" DESC", "").Trim();
						direction = "DESC";
					}
					else
					{
						fieldName = sortExpressions[i].Replace(" ASC", "").Trim();
					}

					//Get property by name
					PropertyInfo val = t.GetProperty(fieldName);
					if (val != null)
					{

						//Compare values, using IComparable interface of the property's type
						int iResult = Comparer.DefaultInvariant.Compare(val.GetValue(x, null), val.GetValue(y, null));
						if (iResult != 0)
						{
							//Return if not equal
							if (direction == "DESC")
							{
								//Invert order
								return -iResult;
							}
							else
							{
								return iResult;
							}
						}
					}
					else
					{
						throw new ArgumentException(fieldName + " is not a valid property to sort on.  It doesn't exist in the Class.");
					}
				}
				//Objects have the same sort order
				return 0;
			}
			else
			{
				PropertyInfo val = t.GetProperty(this.PropertyName);
				if (val != null)
				{
					return Comparer.DefaultInvariant.Compare(val.GetValue(x, null), val.GetValue(y, null));
				}
				else
				{
					throw new ArgumentException(this.PropertyName + " is not a valid property to sort on.  It doesn't exist in the Class.");
				}
			}
		}
		#endregion
	}
}
