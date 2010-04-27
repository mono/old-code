//
// An backend for IProviderPersistence implementations: as a table of
// results that can be looked up by their name and serialized
//

using System;
using System.Text;
using System.Collections;
using System.Runtime.Serialization;

using Mono.Build;

namespace Monkeywrench {

	[Serializable]
	public class StateTable : IEnumerable {

		Hashtable items;

		// .ctor

		public StateTable () {
			items = new Hashtable ();
		}

		// Interface stuff

		public IEnumerator GetEnumerator () {
			return items.GetEnumerator ();
		}

		// Members

		public BuiltItem this [string name] {
			get {
				return GetItem (name);
			}

			set {
				SetItem (name, value);
			}
		}

		public BuiltItem GetItem (string name) {
			if (name == null)
				throw new ArgumentNullException ("Null argument to StateTable.GetResult");

			if (!items.Contains (name))
				return BuiltItem.Null;

			return (BuiltItem) items[name];
		}

		public void SetItem (string name, BuiltItem bi) {
			if (name == null)
				throw new ArgumentNullException ("Result with null name in StateTable.AddResult");

			items[name] = bi;
		}
	}
}
