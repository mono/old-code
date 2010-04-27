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

namespace Mono.Data.SqlSharp.DatabaseBrowser {
	public class MetaProcedureCollection : MarshalByRefObject, IList, ICollection, IEnumerable {	
		#region Fields

		private ArrayList list = new ArrayList ();

		#endregion // Fields

		#region Constructors

		public MetaProcedureCollection () {
		}

		#endregion // Constructors

		#region Properties

		public MetaProcedure this[int index] {
			get { 
				return (MetaProcedure) list[index]; 
			}
		}

		// get the MetaProcedure given its name.  if they are overloaded, it gets
		// the first MetaProcedure for the name
		public MetaProcedure this[string name] {
			get {
				MetaProcedure p = null;
				foreach(object o in list) {
					p = (MetaProcedure) o;
					if(p.Name.Equals(name)) {
						return p;
					}
				}
				throw new Exception("MetaProcedure not found");
			}
		}

		public MetaProcedure GetProcedure(string name) 
		{
			MetaProcedure p = null;
			foreach (object o in list) {
				p = (MetaProcedure) o;
				if (p.Name.Equals (name)) 
					return p;
			}

			throw new Exception("MetaProcedure not found");
		}

		public MetaProcedure[] GetProcedures(string name) 
		{
			ArrayList list = new ArrayList ();
			foreach (object o in list) {
				MetaProcedure p = (MetaProcedure) o;
				if (p.Name.Equals (name)) 
					list.Add (o);
			}

			if (list.Count == 0)
				throw new Exception("MetaProcedure not found");

			return (MetaProcedure[]) list.ToArray (typeof(MetaProcedure));
		}

		object IList.this[int index] {
			get { 
				return list[index]; 
			}			

			set {
				list[index] = value;
			}
		}

		public int Count {
			get { 
				return list.Count; 
			}
		}

		public bool IsFixedSize {
			get { 
				return false; 
			}
		}

		public bool IsReadOnly {
			get { 
				return true; 
			}
		}

		public bool IsSynchronized {
			get { 
				return false; 
			}
		}

		public object SyncRoot {
			get { 
				throw new InvalidOperationException (); 
			}
		}

		#endregion // Properties

		#region Methods

		public int Add (object o) 
		{
			return list.Add ((MetaProcedure) o);
		}

		public void Clear () 
		{
			list.Clear ();
		}

		public bool Contains (object o) 
		{
			return list.Contains ((MetaProcedure) o);
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
			return list.IndexOf ((MetaProcedure) o);
		}

		public void Insert (int index, object o) 
		{
			list.Insert (index, (MetaProcedure) o);
		}

		public void Remove (object o) 
		{
			list.Remove ((MetaProcedure) o);
		}

		public void RemoveAt (int index) 
		{
			list.RemoveAt (index);
		}

		#endregion // Methods
		
	}
}


