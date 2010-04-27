// GtkSharp.Generation.Parser.cs - The XML Parsing engine.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace GtkSharp.Generation {

	using System;
	using System.Collections;
	using System.Xml;

	public class Parser  {
		
		private XmlDocument doc;
		private String ns_name;

		public Parser (String filename)
		{
			doc = new XmlDocument ();

			try {

				doc.Load (filename);

			} catch (XmlException e) {

				Console.WriteLine ("Invalid XML file.");
				Console.WriteLine (e.ToString());
			}

		}

		public String Namespace
		{
			get
			{
				return ns_name;
			}
		}

		public void Parse ()
		{
			XmlElement root = doc.DocumentElement;

			if ((root == null) || !root.HasChildNodes) {
					Console.WriteLine ("No Namespaces found.");
					return;
			}

			foreach (XmlNode ns in root.ChildNodes) {
				if (ns.Name != "namespace") {
					continue;
				}

				XmlElement elem = (XmlElement) ns;
				ParseNamespace (elem);
			}
		}

		private void ParseNamespace (XmlElement ns)
		{
			ns_name = ns.GetAttribute ("name");

			foreach (XmlNode def in ns.ChildNodes) {

				if (def.NodeType != XmlNodeType.Element) {
					continue;
				}

				XmlElement elem = (XmlElement) def;
				
				switch (def.Name) {

				case "type":
					string aname = elem.GetAttribute("aname");
					string atype = elem.GetAttribute("atype");
					if ((aname == "") || (atype == ""))
						continue;
					SymbolTable.AddAlias (aname, atype);
					break;
					
				case "func":
					SymbolTable.AddType (new FuncGen (ns_name, elem));
					break;

				case "const":
					SymbolTable.AddType (new ConstGen (ns_name, elem));
					break;

				default:
					Console.WriteLine ("Unexpected node named " + def.Name);
					break;
				}
			}
		}
	}
}
