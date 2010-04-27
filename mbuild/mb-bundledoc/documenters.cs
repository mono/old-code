// documenters.cs -- type documenters

using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Reflection;
using System.Collections;

namespace MBBundleDoc {

	public class ResultDocumenter : TypeDocumenter {

		public ResultDocumenter (BundleDocumenter owner) : base (owner) {}

		// Impl.

		public override bool IsTypeMatch (Type t) {
			return t.IsSubclassOf (typeof (Mono.Build.Result));
		}

		protected override string ElementName { get { return "result"; } }

		protected override void DocumentDetails (Type t, XmlElement top) {
			ResultDictItemSynchronizer rdis = new ResultDictItemSynchronizer (owner.AssemblyVersion, t, top);
			rdis.Synchronize ();
		}
	}

	public class ProviderDocumenter : TypeDocumenter {

		public ProviderDocumenter (BundleDocumenter owner) : base (owner) {}

		// Impl.

		public override bool IsTypeMatch (Type t) {
			return t.IsSubclassOf (typeof (Mono.Build.SimpleProvider));
		}

		protected override string ElementName { get { return "provider"; } }

		protected override string GetTypeName (Type t) {
			object[] attrs = t.GetCustomAttributes (typeof (Mono.Build.Bundling.ProviderPrefixAttribute), true);

			if (attrs.Length < 1)
				throw new Exception ("Provider has no ProviderPrefix!");

			// FIXME: can, theoretically, install one provider in more than
			// one place by giving it more than one ProviderPrefix attribute.
			// Should document all the prefixes, not just one.

			Mono.Build.Bundling.ProviderPrefixAttribute ppa = (Mono.Build.Bundling.ProviderPrefixAttribute) attrs[0];
			return ppa.Prefix;
		}

		protected override void DocumentDetails (Type t, XmlElement top) {
			ProviderTargetSynchronizer pts = new ProviderTargetSynchronizer (owner.AssemblyVersion, t, top);
			pts.Synchronize ();
		}
	}

	public class RuleDocumenter : TypeDocumenter {

		public RuleDocumenter (BundleDocumenter owner) : base (owner) {}

		// Impl.

		public override bool IsTypeMatch (Type t) {
			return t.IsSubclassOf (typeof (Mono.Build.Rule));
		}

		protected override string ElementName { get { return "rule"; } }

		protected override void DocumentDetails (Type t, XmlElement top) {
			RuleArgumentSynchronizer ras = new RuleArgumentSynchronizer (owner.AssemblyVersion, t, top);
			ras.Synchronize ();
		}
	}

	public class RegexMatcherDocumenter : TypeDocumenter {

		public RegexMatcherDocumenter (BundleDocumenter owner) : base (owner) {}

		// Impl.

		public override bool IsTypeMatch (Type t) {
			return t.IsSubclassOf (typeof (Mono.Build.RuleLib.RegexMatcher));
		}

		protected override string ElementName { get { return "regex_matcher"; } }

		protected override string GetTypeName (Type t) {
			Mono.Build.RuleLib.RegexMatcher rm = (Mono.Build.RuleLib.RegexMatcher) Activator.CreateInstance (t);
			return rm.GetRegex ();
		}

		protected override void DocumentDetails (Type t, XmlElement top) {
			Mono.Build.RuleLib.RegexMatcher rm = (Mono.Build.RuleLib.RegexMatcher) Activator.CreateInstance (t);

			XmlElement c = XmlSynchronizer.AssertChildElement (top, "regex");
			c.InnerText = rm.GetRegex ();

			c = XmlSynchronizer.AssertChildElement (top, "rule");
			c.InnerText = rm.GetRuleType ().ToString ();
		}
	}

	public class MatcherDocumenter : TypeDocumenter {

		public MatcherDocumenter (BundleDocumenter owner) : base (owner) {}

		// Impl.

		public override bool IsTypeMatch (Type t) {
			return (t.GetInterface ("Mono.Build.Bundling.IMatcher") != null);
		}

		protected override string ElementName { get { return "matcher"; } }

		protected override void DocumentDetails (Type t, XmlElement top) {
		}
	}

}
