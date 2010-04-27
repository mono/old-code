//
// a dictionary of other results
// FIXME: Move to Dictionary`2. The sorting is important though.

using System;
using System.Collections;
using System.Text;
using System.Runtime.Serialization;
using System.Xml;

namespace Mono.Build {

	[Serializable]
	public class MBDictionary : Result {

		SortedList store;

		public MBDictionary () : base () {
			store = new SortedList ();
		}

		public MBDictionary (IDictionary value) : base () {
			store = new SortedList (value);
		}

		// ~IDictionary

		public Result this [string key] {
			get {
				return (Result) store[key];
			}

			set {
				store[key] = value;
			}
		}

		public ICollection Keys { get { return store.Keys; } }

		public ICollection Values { get { return store.Values; } }

		public void Add (string key, Result value) {
			store.Add (key, value);
		}

		public bool Contains (string key) {
			if (key == null)
				throw new ArgumentNullException ();

			return store.Contains (key);
		}

		public void Clear () {
			store.Clear ();
		}

		public void Remove (string key) {
			store.Remove (key);
		}

		public IDictionaryEnumerator GetEnumerator () {
			return store.GetEnumerator ();
		}

		public string GetString (string key) {
			Result res = (Result) store[key];

			if (res == null)
				return null;

			if (!(res is MBString))
				throw new Exception ("Key " + key + " does not map to a string");

			return (res as MBString).Value;
		}

		public void SetString (string key, string val) {
			if (val == null)
				store[key] = null;
			else
				store[key] = new MBString (val);
		}

		// Cloning

		protected override void CloneTo (Result dest) {
			MBDictionary ddest = (MBDictionary) dest;
			ddest.store = (SortedList) store.Clone ();
		}

		// Equality

		protected override bool ContentEquals (Result other)
		{
		    MBDictionary d = (MBDictionary) other;

		    if (store.Count != d.store.Count)
			return false;

		    foreach (string k in store.Keys) {
			if (!d.store.ContainsKey (k))
			    return false;

			Result mine = (Result) store[k];
			
			if (! mine.Equals (d.store[k]))
			    return false;
		    }

		    return true;
		}

		protected override int InternalHash ()
		{
		    int hash = 0;

		    foreach (string k in store.Keys) {
			hash ^= k.GetHashCode ();
			hash ^= store[k].GetHashCode ();
		    }

		    return hash;
		}

		// XML

		protected override void ExportXml (XmlWriter xw) {
			foreach (string s in store.Keys) {
				Result r = this[s];
				r.ExportXml (xw, s);
			}
		}

		protected override bool ImportXml (XmlReader xr, IWarningLogger log) {
			int d = xr.Depth;

			while (!xr.EOF && xr.Depth >= d) {
				if (xr.NodeType != XmlNodeType.Element) {
					xr.Read ();
					continue;
				}

				if (xr.NodeType != XmlNodeType.Element || xr.Name != "result") {
					log.Warning (3019, "Expected 'result' element in dictionary but got '" +
						     xr.Name + "' instead during XML import.", null);
					xr.Read ();
					continue;
				}

				string id;
				Result r = Result.ImportXml (xr, out id, log);

				if (r == null)
					return true;

				this[id] = r;
			}

			return false;
		}

		// Fingerprinting

		public override Fingerprint GetFingerprint (IBuildContext ctxt, Fingerprint cached) {
			return new CompositeFingerprint (store.Values, ctxt, cached);
		}

		// Object

		public override string ToString () {
			StringBuilder sb = new StringBuilder ("{");
			string lame = "";

			foreach (string k in store.Keys) {
				sb.AppendFormat ("{0} {1} = {2}", lame, k, store[k]);
				lame = ",";
			}

			sb.Append (" }");
			return sb.ToString ();
		}
	}
}
