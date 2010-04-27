// MetaProcedureCollection.cs
//
// Author:
//     Daniel Morgan <danielmorgan@verizon.net>
//
// (C)Copyright 2005 by Daniel Morgan
//
// To be included with Mono as a SQL query tool licensed under the LGPL license.
//

using System;
using System.Data;
using System.Collections;

namespace Mono.Data.SqlSharp.DatabaseBrowser
{
	public class MetaProcedureArgumentCollection : MarshalByRefObject, IList, ICollection, IEnumerable 
	{	
		#region Fields

		private ArrayList list = new ArrayList ();

		#endregion // Fields

		#region Constructors

		public MetaProcedureArgumentCollection () 
		{
		}

		#endregion // Constructors

		#region Properties

		public MetaProcedureArgument this[int index] 
		{
			get { 
				return (MetaProcedureArgument) list[index]; 
			}
		}

		public MetaProcedureArgument this[string name] 
		{
			get {
				MetaProcedureArgument p = null;
				foreach (object o in list) {
					p = (MetaProcedureArgument) o;
					if (p.ArgumentName.Equals (name)) 
						return p;
				}
				throw new Exception ("MetaProcedureArgument not found");
			}
		}

		object IList.this[int index] 
		{
			get { 
				return list[index]; 
			}			

			set {
				list[index] = value;
			}
		}

		public int Count 
		{
			get { 
				return list.Count; 
			}
		}

		public bool IsFixedSize 
		{
			get { 
				return false; 
			}
		}

		public bool IsReadOnly 
		{
			get { 
				return true; 
			}
		}

		public bool IsSynchronized 
		{
			get { 
				return false; 
			}
		}

		public object SyncRoot 
		{
			get { 
				throw new InvalidOperationException (); 
			}
		}

		#endregion // Properties

		#region Methods

		public int Add (object o) 
		{
			return list.Add ((MetaProcedureArgument) o);
		}

		public void Clear () 
		{
			list.Clear ();
		}

		public bool Contains (object o) 
		{
			return list.Contains ((MetaProcedureArgument) o);
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
			return list.IndexOf ((MetaProcedureArgument) o);
		}

		public void Insert (int index, object o) 
		{
			list.Insert (index, (MetaProcedureArgument) o);
		}

		public void Remove (object o) 
		{
			list.Remove ((MetaProcedureArgument) o);
		}

		public void RemoveAt (int index) 
		{
			list.RemoveAt (index);
		}

		#endregion // Methods
		
	}
}
