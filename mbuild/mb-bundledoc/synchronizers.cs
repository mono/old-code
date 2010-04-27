// synchronizers.cs -- synchronizers used by the BundleDocumenter

using System;
using System.Xml;
using System.Collections;
using System.Reflection;

using Mono.Build;
using Mono.Build.Bundling;

namespace MBBundleDoc {

	public class ResultDictItemSynchronizer : DeprecatingSynchronizer {
		
		public ResultDictItemSynchronizer (string v, Type t, XmlNode top) : base (v, t, top) {}

		// Impl

		protected override IEnumerable SynchronizedItems {
			get {
				return t.GetProperties (PubFlags);
			}
		}

		Type mbdia = typeof (MonoBuildDictItemAttribute);

		protected override bool IsIgnoredItem (object o) {
			PropertyInfo pi = (PropertyInfo) o;

			object[] attrs = pi.GetCustomAttributes (mbdia, false);
			return (attrs.Length == 0);
		}

		protected override string ElementName { get { return "dictitem"; } }

		protected override string GetItemIdentifier (object o) {
			PropertyInfo pi = (PropertyInfo) o;

			return pi.Name;
		}

		protected override void SynchronizeItem (object o, XmlElement e) {
			AssertDocsElement (e);
		}
	}

	public class ProviderTargetSynchronizer : DeprecatingSynchronizer {
		
		public ProviderTargetSynchronizer (string v, Type t, XmlNode top) : base (v, t, top) {}

		// Impl

		protected override IEnumerable SynchronizedItems {
			get {
				SimpleProvider sp = (SimpleProvider) Activator.CreateInstance (t);

				return sp.Targets;
			}
		}

		protected override string ElementName { get { return "target"; } }

		protected override string GetItemIdentifier (object o) {
			ITarget target = (ITarget) o;
			return target.Name;
		}

		protected override void SynchronizeItem (object o, XmlElement e) {
			ITarget target = (ITarget) o;

			AssertDocsElement (e);

			XmlElement rule = AssertChildElement (e, "rule");
			rule.InnerText = target.RuleType.ToString ();
		}
	}

	public class RuleArgumentSynchronizer : DeprecatingSynchronizer {
		
		public RuleArgumentSynchronizer (string v, Type t, XmlNode top) : base (v, t, top) {}

		// Impl

		protected override IEnumerable SynchronizedItems {
			get {
				Rule r = (Rule) Activator.CreateInstance (t);
				ArgCollector ac = new ArgCollector (r);
				return r.GetCollector ();
			}
		}

		protected override string ElementName { get { return "argument"; } }

		protected override string GetItemIdentifier (object o) {
			ArgInfo ai = (ArgInfo) o;
			return ai.name;
		}

		protected override void SynchronizeItem (object o, XmlElement e) {
			ArgInfo ai = (ArgInfo) o;

			AssertDocsElement (e);

			XmlElement type = AssertChildElement (e, "type");
			type.InnerText = ai.type.ToString ();

			if (ai.default_to != null) {
				XmlElement dflt = AssertChildElement (e, "default_to");
				dflt.InnerText = ai.default_to;
			}

			string flags = "";
			if ((ai.flags & ArgFlags.Optional) != 0)
				flags += "optional ";
			if ((ai.flags & ArgFlags.Multi) != 0)
				flags += "multi ";
			if ((ai.flags & ArgFlags.Default) != 0)
				flags += "default ";
			if ((ai.flags & ArgFlags.Ordered) != 0)
				flags += "ordered ";
			if ((ai.flags & ArgFlags.DefaultOrdered) != 0)
				flags += "default_ordered ";

			XmlElement f = AssertChildElement (e, "flags");
			f.InnerText = flags;
		}
	}

	public class NamespaceSynchronizer : DeprecatingSynchronizerBase {
		
		IEnumerable names;

		public NamespaceSynchronizer (IEnumerable names, string v, XmlNode top) : 
			base (v, top) {
			this.names = names;
		}

		// Impl

		protected override IEnumerable SynchronizedItems {
			get {
				return names;
			}
		}

		protected override string ElementName { get { return "namespace"; } }

		protected override string GetItemIdentifier (object o) {
			string ns = (string) o;

			if (StrUtils.StartsWith (ns, BundleDocumenter.MBuildPrefix))
				ns = ns.Substring (BundleDocumenter.MBuildPrefix.Length);

			return ns;
		}

		protected override void SynchronizeItem (object o, XmlElement e) {
			AssertDocsElement (e);
		}
	}

}
