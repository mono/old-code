// BundleDocumenter.cs -- actual documenter stuff

using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Reflection;
using System.Collections;

namespace MBBundleDoc {

	public class BundleDocumenter {
		Assembly assy;
		string docpath;
		string assy_vers;

		Hashtable namespaces = new Hashtable ();

		ResultDocumenter result;
		ProviderDocumenter provider;
		RuleDocumenter rule;
		RegexMatcherDocumenter regex;
		MatcherDocumenter matcher;

		public BundleDocumenter (Assembly assy, string docpath) {
			this.assy = assy;
			this.docpath = docpath;

			assy_vers = assy.GetName ().Version.ToString ();

			result = new ResultDocumenter (this);
			provider = new ProviderDocumenter (this);
			rule = new RuleDocumenter (this);
			regex = new RegexMatcherDocumenter (this);
			matcher = new MatcherDocumenter (this);
		}

		public Assembly Assembly { get { return assy; } }

		public string DocPath { get { return docpath; } }

		public string AssemblyVersion { get { return assy_vers; } }

		public void Document () {
			foreach (Type t in assy.GetTypes ()) {
				if (t.IsAbstract)
					continue;

				DocumentType (t);
			}

			DocumentAssembly ();
		}

		void DocumentType (Type t) {
			if (result.IsTypeMatch (t))
				result.DocumentType (t);
			else if (provider.IsTypeMatch (t))
				provider.DocumentType (t);
			else if (rule.IsTypeMatch (t))
				rule.DocumentType (t);
			else if (regex.IsTypeMatch (t))
				regex.DocumentType (t);
			else if (matcher.IsTypeMatch (t))
				matcher.DocumentType (t);
			else {
				// <PrivateImplementationDetails> types cause spew here
				//Console.Error.WriteLine ("Ignoring type {0}", t.FullName);
				return;
			}

			namespaces[t.Namespace] = true;
		}

		void DocumentAssembly () {
			string path = String.Format ("bundle{0}bundle.xml", Path.DirectorySeparatorChar);
			XmlDocument doc = LoadXmlDocument (path, "bundle");

			string name = assy.GetName ().Name;
			if (name.StartsWith (MBuildPrefix))
				name = name.Substring (MBuildPrefix.Length);

			doc.DocumentElement.SetAttribute ("name", name);
			XmlSynchronizer.AssertDocsElement (doc.DocumentElement);

			NamespaceSynchronizer nss = new NamespaceSynchronizer (namespaces.Keys, assy_vers, 
									       doc.DocumentElement);
			nss.Synchronize ();

			WriteXmlDocument (path, doc);
		}

		// IO util

		public const string MBuildPrefix = "MBuildDynamic.";

		public string MakeDocumentPath (Type t) {
			string ns = t.Namespace;

			if (ns.StartsWith (MBuildPrefix))
				ns = ns.Substring (MBuildPrefix.Length);

			return Path.Combine (ns, t.Name + ".xml");
		}

		public XmlDocument LoadXmlDocument (string name, string root) {
			XmlDocument doc = new XmlDocument ();
			doc.PreserveWhitespace = true;

			string path = Path.Combine (docpath, name);

			if (File.Exists (path)) {
				doc.Load (path);

				if (doc.DocumentElement.Name != root) {
					string s = String.Format ("In {0}, expected document root element " + 
								  "{1}, but got {2}", path, root,
								  doc.DocumentElement.Name);
					throw new Exception (s);
				}
			} else {
				XmlElement rootelem = doc.CreateElement (root);
				doc.AppendChild (rootelem);
			}

			return doc;
		}

		public void WriteXmlDocument (string name, XmlDocument doc) {
			string path = Path.Combine (docpath, name);

			if (!Directory.Exists (Path.GetDirectoryName (path)))
				Directory.CreateDirectory (Path.GetDirectoryName (path));

			Stream s = new FileStream (path, FileMode.Create);
			StreamWriter sw = new StreamWriter (s, Encoding.UTF8);
			XmlTextWriter w = new XmlTextWriter (sw);

			w.Formatting = Formatting.Indented;
			w.Indentation = 8;
			w.IndentChar = ' ';
			doc.WriteTo (w);
			
			// Keeps on appending newlines. Sigh.
			//sw.WriteLine (); // shut up emacs

			w.Close ();
		}
	}
}
