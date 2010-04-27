using System;
using System.Xml;
using System.Reflection;

namespace MBBundleDoc {

	public abstract class DeprecatingSynchronizerBase : XmlSynchronizer {

		string this_version;

		public DeprecatingSynchronizerBase (string this_version, XmlNode top) : base () {
			if (this_version == null)
				throw new ArgumentNullException ("this_version");

			this.this_version = this_version;
			this.Top = top;
		}

		// Not the best place for this ...
		public const BindingFlags PubFlags = BindingFlags.Instance | BindingFlags.Public;

		const string deprecated_attr = "deprecated_since";
		const string exists_attr = "exists_since";

		protected override void InitializeItemElement (object o, XmlElement e) {
			e.SetAttribute (exists_attr, this_version);
		}

		protected override bool IsIgnoredElement (XmlElement e) {
			return (e.Attributes.GetNamedItem (deprecated_attr) != null);
		}

		protected override void HandleRemovedElement (XmlElement e) {
			e.SetAttribute (deprecated_attr, this_version);
		}
	}

	public abstract class DeprecatingSynchronizer : DeprecatingSynchronizerBase {

		protected Type t;

		public DeprecatingSynchronizer (string this_version, Type t, XmlNode top) : base (this_version, top) {
			if (t == null)
				throw new ArgumentNullException ("t");

			this.t = t;
		}
	}
}
