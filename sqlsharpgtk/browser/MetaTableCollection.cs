// MetaTableCollection.cs
//
// Author:
//     Daniel Morgan <danielmorgan@verizon.net>
//
// (C)Copyright 2004 by Daniel Morgan
//
// To be included with Mono as a SQL query tool licensed under the LGPL license.
//

using System;
using System.Data;
using System.Collections;

namespace Mono.Data.SqlSharp.DatabaseBrowser
{
	public class MetaTableCollection : MarshalByRefObject, IList, ICollection, IEnumerable 
	{	
		#region Fields

		private ArrayList list = new ArrayList ();

		#endregion // Fields

		#region Constructors

		public MetaTableCollection () 
		{
		}

		#endregion // Constructors

		#region Properties

		public MetaTable this[int index] 
		{
			get 
			{ 
				return (MetaTable) list[index]; 
			}
		}

		public MetaTable this[string name] 
		{
			get 
			{
				MetaTable p = null;
				foreach(object o in list) 
				{
					p = (MetaTable) o;
					if(p.Name.Equals(name)) 
					{
						return p;
					}
				}
				throw new Exception("MetaTable not found");
			}
		}

		object IList.this[int index] 
		{
			get 
			{ 
				return list[index]; 
			}			

			set 
			{
				list[index] = value;
			}
		}

		public int Count 
		{
			get 
			{ 
				return list.Count; 
			}
		}

		public bool IsFixedSize 
		{
			get 
			{ 
				return false; 
			}
		}

		public bool IsReadOnly 
		{
			get 
			{ 
				return true; 
			}
		}

		public bool IsSynchronized 
		{
			get 
			{ 
				return false; 
			}
		}

		public object SyncRoot 
		{
			get 
			{ 
				throw new InvalidOperationException (); 
			}
		}

		#endregion // Properties

		#region Methods

		public int Add (object o) 
		{
			return list.Add ((MetaTable) o);
		}

		public void Clear () 
		{
			list.Clear ();
		}

		public bool Contains (object o) 
		{
			return list.Contains ((MetaTable) o);
		}

		public void CopyTo (Array array, int index) 
		{
			list.CopyTo (array, index);
		}

		public IEnumerator GetEnumerator () 
		{
			return list.GetEnumerator ();
		}

		public int IndexOf (object o) 
		{
			return list.IndexOf ((MetaTable) o);
		}

		public void Insert (int index, object o) 
		{
			list.Insert (index, (MetaTable) o);
		}

		public void Remove (object o) 
		{
			list.Remove ((MetaTable) o);
		}

		public void RemoveAt (int index) 
		{
			list.RemoveAt (index);
		}

		#endregion // Methods
		
	}
}
