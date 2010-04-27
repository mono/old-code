// MetaTableColumnCollection.cs
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
	public class MetaTableColumnCollection : MarshalByRefObject, IList, ICollection, IEnumerable 
	{	
		#region Fields

		ArrayList list = new ArrayList ();
		string owner;
		string tableName;

		#endregion // Fields

		#region Constructors

		public MetaTableColumnCollection () 
		{
		}

		public MetaTableColumnCollection (string tableName) 
		{
			this.tableName = tableName;
		}

		public MetaTableColumnCollection (string owner, string tableName) 
		{
			this.owner = owner;
			this.tableName = tableName;
		}

		#endregion // Constructors

		#region Properties

		public MetaTableColumn this[int index] 
		{
			get 
			{ 
				return (MetaTableColumn) list[index]; 
			}
		}

		public MetaTableColumn this[string name] 
		{
			get 
			{
				MetaTableColumn p = null;
				foreach(object o in list) 
				{
					p = (MetaTableColumn) o;
					if(p.Name.Equals(name)) 
					{
						return p;
					}
				}
				throw new Exception("MetaTableColumn not found");
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

		public string TableName 
		{
			get 
			{
				return tableName;
			}

			set 
			{
				tableName = value;
			}
		}

		public string Owner 
		{
			get
			{
				return owner;
			}

			set 
			{
				owner = value;
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
			return list.Add ((MetaTableColumn) o);
		}

		public void Clear () 
		{
			list.Clear ();
		}

		public bool Contains (object o) 
		{
			return list.Contains ((MetaTableColumn) o);
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
			return list.IndexOf ((MetaTableColumn) o);
		}

		public void Insert (int index, object o) 
		{
			list.Insert (index, (MetaTableColumn) o);
		}

		public void Remove (object o) 
		{
			list.Remove ((MetaTableColumn) o);
		}

		public void RemoveAt (int index) 
		{
			list.RemoveAt (index);
		}

		#endregion // Methods
		
	}
}
