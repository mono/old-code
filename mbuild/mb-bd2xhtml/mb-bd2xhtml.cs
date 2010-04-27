//
// mb-bd2xhtml.cs -- Convert bundle XML documentation into XHTML
//

using System;
using System.Reflection;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

using Mono.GetOptions;

namespace MBBd2XHtml {

	public class MainClass : Options {

		public MainClass () : base () {
			// Be more mcs-like in option handling
			//BreakSingleDashManyLettersIntoManyOptions = true;
			//ParsingMode = OptionsParsingMode.Linux;
		}

		// Options

		[Option("Specify a non-default stylesheet to use", 's', "stylesheet")]
		public string extra_stylesheet = null;

		public override WhatToDoNext DoAbout () {
			base.DoAbout ();
			return WhatToDoNext.AbandonProgram;
		}

		string outpath;

		// Table-of-contents file processing

		XmlDocument tocfile = new XmlDocument ();
		Hashtable namespaces = new Hashtable ();

		void SetupTocfile () {
			XmlElement e = tocfile.CreateElement ("monkeyguide");
			tocfile.AppendChild (e);

			XmlElement intro = tocfile.CreateElement ("intro");
			e.AppendChild (intro);

			XmlElement summary = tocfile.CreateElement ("doc");
			summary.SetAttribute ("name", "Summary");
			summary.SetAttribute ("href", "bundle/bundle.xhtml");
			intro.AppendChild (summary);
		}

		void WriteTocfile () {
			string path = Path.Combine (outpath, "toc.xml");
			FileStream s = new FileStream (path, FileMode.Create, FileAccess.Write);
			StreamWriter output = new StreamWriter (s);

			tocfile.Save (output);
			output.Close ();
		}

		void AddNamespaceDoc (string ns, string title, string file) {
			XmlElement e;

			if (ns == "bundle")
				return;

			if (namespaces.Contains (ns))
				e = (XmlElement) namespaces[ns];
			else {
				e = tocfile.CreateElement ("part");
				e.SetAttribute ("name", ns);
				tocfile.DocumentElement.AppendChild (e);

				namespaces[ns] = e;
			}

			XmlElement doc = tocfile.CreateElement ("doc");
			doc.SetAttribute ("name", title);
			doc.SetAttribute ("href", file);
			e.AppendChild (doc);
		}

		// Per-file processing

		XslTransform xform = new XslTransform ();
		XsltArgumentList args = new XsltArgumentList ();
		XmlResolver resolver = new XmlUrlResolver ();

		void ProcessFile (string file) {
			string ns;
			string pathbase = MakeOutputPathBase (file, out ns);
			string path = Path.Combine (outpath, pathbase);
			string dir = Path.GetDirectoryName (path);

			if (!Directory.Exists (dir))
				Directory.CreateDirectory (dir);

			XPathDocument input = new XPathDocument (file);
			AddNamespaceDoc (ns, GetDocumentTitle (input), pathbase);

			FileStream s = new FileStream (path, FileMode.Create, FileAccess.Write);
			StreamWriter output = new StreamWriter (s);

			xform.Transform (input, args, output, resolver);
			output.Close ();
		}

		string MakeOutputPathBase (string file, out string ns) {
			string dir = Path.GetDirectoryName (file);
			int i = dir.LastIndexOf (Path.DirectorySeparatorChar);

			ns = dir.Substring (i + 1);
			return Path.ChangeExtension (Path.Combine (ns, Path.GetFileName (file)), "xhtml");
		}

		string GetDocumentTitle (XPathDocument doc) {
			XPathNavigator nav = doc.CreateNavigator ();
			nav.MoveToFirstChild ();

			string node = nav.LocalName;
			string name = nav.GetAttribute ("name", "");

			switch (node) {
			case "result":
				node = "Result";
				break;
			case "provider":
				node = "Provider";
				break;
			case "rule":
				node = "Rule";
				break;
			case "regex_matcher":
				node = "Regex Matcher";
				break;
			case "matcher":
				node = "Matcher";
				break;
			}

			return String.Format ("{0} {1}", name, node);
		}

		// Stylesheet loading

		const string ResourceName = "bd2xhtml.xsl";

		public XmlTextReader LoadStylesheet () {
			Stream s;

			if (extra_stylesheet == null)
				s = Assembly.GetExecutingAssembly ().GetManifestResourceStream (ResourceName);
			else {
				if (!File.Exists (extra_stylesheet)) {
					Console.Error.WriteLine ("Could not open stylesheet file {0}!", extra_stylesheet);
					return null;
				}

				s = new FileStream (extra_stylesheet, FileMode.Open, FileAccess.Read);
			}

			return new XmlTextReader (s);
		}

		// Main logic

		public int Launch () {
			if (RemainingArguments.Length < 2) {
				DoAbout ();
				return 1;
			}

			outpath = RemainingArguments[0];
			if (!Directory.Exists (outpath)) {
				Console.Error.WriteLine ("Documentation output directory {0} does not exist!",
							 outpath);
				return 1;
			}

			try {
				XmlTextReader r = LoadStylesheet ();
				if (r == null)
					return 1;

				xform.Load (r);

				SetupTocfile ();

				for (int i = 1; i < RemainingArguments.Length; i++) {
					string file = RemainingArguments[i];

					if (!File.Exists (file)) {
						Console.Error.WriteLine ("Input file {0} does not exist!",
									 file);
						return 1;
					}

					ProcessFile (file);
				}

				WriteTocfile ();
			} catch (Exception e) {
				Console.Error.WriteLine ("Error: {0}", e);
				return 1;
			}

			return 0;
		}

		public static int Main (string[] args) {
			MainClass options = new MainClass ();
			
			options.ProcessArgs (args);
			return options.Launch ();
		}
	}
	
}
