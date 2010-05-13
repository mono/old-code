//
// TypeFactory.cs: Generic factory implementation
//
// Author: Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002 Jonathan Pryor
//

using System;
using System.Collections;
using System.Diagnostics;
using System.Text;

namespace Mono.TypeReflector
{
	public class TypeFactoryEntry
	{
		private string key, desc;
		private Type type;

		public TypeFactoryEntry (string key, string description, Type type)
		{
			this.key = key;
			this.desc = description;
			this.type = type;
		}

		public string Key {
			get {return key;}
		}

		public string Description {
			get {return desc;}
		}

		public Type Type {
			get {return type;}
		}
	}

	public class TypeFactory : IDictionary
	{
		private static BooleanSwitch info = new BooleanSwitch (
			"type-factory",
			"Information about creating types.");

		// type: IDictionary<string, TypeFactoryEntry>
		private IDictionary entries = new Hashtable ();

		//
		// ICollection interface
		//
		public int Count {
			get {return entries.Count;}
		}

		public bool IsSynchronized {
			get {return entries.IsSynchronized;}
		}

		public object SyncRoot {
			get {return entries.SyncRoot;}
		}

		public void CopyTo (Array array, int index)
		{
			entries.CopyTo (array, index);
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return entries.GetEnumerator ();
		}

		//
		// IDictionary interface
		//

		public bool IsFixedSize {
			get {return false;}
		}

		public bool IsReadOnly {
			get {return false;}
		}

		public object this [object key] {
			get {return entries[key];}
			set {entries[key] = value;}
		}

		public ICollection Keys {
			get {return entries.Keys;}
		}

		public ICollection Values {
			get {return entries.Values;}
		}

		void IDictionary.Add (object key, object value)
		{
			Add (key, (TypeFactoryEntry) value);
		}

		void IDictionary.Clear ()
		{
			/* do nothing */
		}

		public bool Contains (object key)
		{
			return entries.Contains (key);
		}

		public IDictionaryEnumerator GetEnumerator ()
		{
			return entries.GetEnumerator ();
		}

		public void Remove (object key)
		{
			entries.Remove (key);
		}

		//
		// New methods
		//

		public void Add (object key, TypeFactoryEntry value)
		{
			entries.Add (key, value);
		}

		public void Add (TypeFactoryEntry entry)
		{
			Add (entry.Key, entry);
		}

		private object CreateInstance (Type type)
		{
			return Activator.CreateInstance (type);
		}

		public object Create (object key)
		{
			TypeFactoryEntry entry = null;
			try {
				entry = (TypeFactoryEntry) entries[key];
				object o = CreateInstance (entry.Type);
				IPolicy policy = o as IPolicy;
				if (policy != null) {
					policy.FactoryKey = entry.Key;
					policy.Description = entry.Description;
				}
				return o;
			}
			catch (Exception e) {
				Trace.WriteLineIf (info.Enabled, string.Format (
					"Exception creating ({0}, {1}): {2})", 
					key, entry == null ? null : entry.Type, e.ToString()));
				return null;
			}
		}
	}
}

