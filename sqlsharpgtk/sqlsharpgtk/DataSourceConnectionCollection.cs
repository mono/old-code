//
// DataSourceConnectionCollection.cs
//
// Author:
//     Daniel Morgan <monodanmorg@yahoo.com>
//
// (C)Copyright 2004 by Daniel Morgan
//
// To be included with Mono as a SQL query tool licensed under the GPL license.
//

namespace Mono.Data.SqlSharp.GtkSharp
{
	using System;
	using System.Data;
	using System.Collections;

	public class DataSourceConnectionCollection : MarshalByRefObject, IList, ICollection, IEnumerable 
	{	
		#region Fields

		ArrayList list = new ArrayList ();

		#endregion // Fields

		#region Constructors

		public DataSourceConnectionCollection () 
		{
		}

		#endregion // Constructors

		#region Properties

		public DataSourceConnection this[int index] 
		{
			get 
			{ 
				return (DataSourceConnection) list[index]; 
			}
		}

		public DataSourceConnection this[string name] 
		{
			get 
			{
				DataSourceConnection c = null;
				foreach(object o in list) 
				{
					c = (DataSourceConnection) o;
					if(c.Name.ToUpper().Equals(name.ToUpper())) 
					{
						return c;
					}
				}
				throw new Exception("DataSourceConnection not found");
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
			return list.Add ((DataSourceConnection) o);
		}

		public void Clear () 
		{
			list.Clear ();
		}

		public bool Contains (object o) 
		{
			return list.Contains ((DataSourceConnection) o);
		}

		public void CopyTo (Array array, int index) 
		{
			list.CopyTo (array, index);
		}

		public string[] Names 
		{
			get {
					string[] names = new string[list.Count];
					DataSourceConnection c = null;
					int i = 0;
					foreach(object o in list) 
					{
						c = (DataSourceConnection) o;
						names[i] = c.Name;
						i ++;
					}
					return names;
			}
		}

		public IEnumerator GetEnumerator () 
		{
			return list.GetEnumerator ();
		}

		public int IndexOf (object o) 
		{
			return list.IndexOf ((DataSourceConnection) o);
		}

		public void Insert (int index, object o) 
		{
			list.Insert (index, (DataSourceConnection) o);
		}

		public void Remove (object o) 
		{
			list.Remove ((DataSourceConnection) o);
		}

		public void RemoveAt (int index) 
		{
			list.RemoveAt (index);
		}

		#endregion // Methods
		
	}
}
