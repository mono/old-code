using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

using NDesk.Options;

namespace Mono.Documentation {
public class Monodocs2SlashDoc {
	
	public static void Main(string[] args) {
		string file = null;
		bool showHelp = false;
		OptionSet p = new OptionSet () {
			{ "o|out=", 
				"The XML {FILE} to generate.\n" + 
				"If not specified, will create a set of files in the curent directory " +
				"based on the //AssemblyInfo/AssemblyName values within the documentation.\n" +
				"Use '-' to write to standard output.",
				v => file = v },
			{ "h|?|help",
				"Show this message and exit.",
				v => showHelp = v != null },
		};
		List<string> extra = p.Parse (args);
		if (extra.Count == 0 || showHelp) {
			Console.WriteLine ("usage: monodocs2slashdoc [OPTION]* DIRECTORIES");
			Console.WriteLine ();
			Console.WriteLine ("Converts mdoc(5)-style XML documentation into Microsoft XML Documentation.");
			Console.WriteLine ();
			Console.WriteLine ("Available Options:");
			p.WriteOptionDescriptions (Console.Out);
			return;
		}

		Process (extra, file);
	}

	private static void Process (List<string> dirs, string file)
	{
		Dictionary<string, XmlElement> outputfiles = new Dictionary<string, XmlElement> ();

		XmlDocument nsSummaries = new XmlDocument();
		nsSummaries.LoadXml("<namespaces/>");

		foreach (string dir in dirs)
			Process (dir, outputfiles, nsSummaries, file == null);

		if (outputfiles.Count > 0 && file != null) {
			List<string> files = new List<string> (outputfiles.Keys);
			files.Sort ();
			XmlDocument d = new XmlDocument ();
			d.AppendChild (d.CreateElement ("doc"));
			d.FirstChild.AppendChild (
					d.ImportNode (outputfiles [files [0]].SelectSingleNode ("/doc/assembly"), true));
			XmlElement members = d.CreateElement ("members");
			d.FirstChild.AppendChild (members);
			foreach (string f in files) {
				XmlElement from = (XmlElement) outputfiles [f];
				foreach (XmlNode n in from.SelectNodes ("/doc/members/*"))
					members.AppendChild (d.ImportNode (n, true));
			}
			using (TextWriter tw = file == "-" ? Console.Out : new StreamWriter (file))
				WriteXml (d.DocumentElement, tw);
			return;
		}

		// Write out each of the assembly documents
		foreach (string assemblyName in outputfiles.Keys) {
			XmlElement members = (XmlElement)outputfiles[assemblyName];
			Console.WriteLine(assemblyName + ".xml");
			using(StreamWriter sw = new StreamWriter(assemblyName + ".xml")) {
				WriteXml(members.OwnerDocument.DocumentElement, sw);
			}
		}
	
		// Write out a namespace summaries file.
		Console.WriteLine("NamespaceSummaries.xml");
		using(StreamWriter writer = new StreamWriter("NamespaceSummaries.xml")) {
			WriteXml(nsSummaries.DocumentElement, writer);
		}
	}

	private static void Process (string basepath, Dictionary<string, XmlElement> outputfiles, XmlDocument nsSummaries, bool implicitFiles)
	{
		if (System.Environment.CurrentDirectory == System.IO.Path.GetFullPath(basepath) && implicitFiles) {
			Console.WriteLine("Don't run this tool from your documentation directory, since some files could be accidentally overwritten.");
			return;
		}

		XmlDocument index_doc = new XmlDocument();
		index_doc.Load(Path.Combine(basepath, "index.xml"));
		XmlElement index = index_doc.DocumentElement;
		
		foreach (XmlElement assmbly in index.SelectNodes("Assemblies/Assembly")) {
			string assemblyName = assmbly.GetAttribute("Name");
			if (outputfiles.ContainsKey (assemblyName))
				continue;
			XmlDocument output = new XmlDocument();
			XmlElement output_root = output.CreateElement("doc");
			output.AppendChild(output_root);

			XmlElement output_assembly = output.CreateElement("assembly");
			output_root.AppendChild(output_assembly);
			XmlElement output_assembly_name = output.CreateElement("name");
			output_assembly.AppendChild(output_assembly_name);
			output_assembly_name.InnerText = assemblyName;
		
			XmlElement members = output.CreateElement("members");
			output_root.AppendChild(members);
			
			outputfiles.Add (assemblyName, members);
		}
			
		foreach (XmlElement nsnode in index.SelectNodes("Types/Namespace")) {
			string ns = nsnode.GetAttribute("Name");
			foreach (XmlElement typedoc in nsnode.SelectNodes("Type")) {
				string typename = typedoc.GetAttribute("Name");
				XmlDocument type = new XmlDocument();
				type.Load(Path.Combine(Path.Combine(basepath, ns), typename) + ".xml");
				
				string assemblyname = type.SelectSingleNode("Type/AssemblyInfo/AssemblyName").InnerText;
				XmlElement members = outputfiles [assemblyname];
				if (members == null) continue; // assembly is strangely not listed in the index
				
				string typeName = XmlDocUtils.ToEscapedTypeName (type.SelectSingleNode("Type/@FullName").InnerText);
				CreateMember("T:" + typeName, type.DocumentElement, members);
					
				foreach (XmlElement memberdoc in type.SelectNodes("Type/Members/Member")) {
					string name = typeName;
					switch (memberdoc.SelectSingleNode("MemberType").InnerText) {
						case "Constructor":
							name = "C:" + name + MakeArgs(memberdoc);
							break;
						case "Method":
							name = "M:" + name + "." + XmlDocUtils.ToEscapedMemberName (memberdoc.GetAttribute("MemberName")) + MakeArgs(memberdoc);
							if (memberdoc.GetAttribute("MemberName") == "op_Implicit" || memberdoc.GetAttribute("MemberName") == "op_Explicit")
								name += "~" + XmlDocUtils.ToTypeName (memberdoc.SelectSingleNode("ReturnValue/ReturnType").InnerText, memberdoc);
							break;
						case "Property":
							name = "P:" + name + "." + XmlDocUtils.ToEscapedMemberName (memberdoc.GetAttribute("MemberName")) + MakeArgs(memberdoc);
							break;
						case "Field":
							name = "F:" + name + "." + XmlDocUtils.ToEscapedMemberName (memberdoc.GetAttribute("MemberName"));
							break;
						case "Event":
							name = "E:" + name + "." + XmlDocUtils.ToEscapedMemberName (memberdoc.GetAttribute("MemberName"));
							break;
					}
					
					CreateMember(name, memberdoc, members);
				}
			}
		}
		foreach (XmlElement nsnode in index.SelectNodes("Types/Namespace")) {
			AddNamespaceSummary(nsSummaries, basepath, nsnode.GetAttribute("Name"));
		}
	}
	
	private static void AddNamespaceSummary(XmlDocument nsSummaries, string basepath, string currentNs) {
		string filename = Path.Combine(basepath, currentNs + ".xml");
		if (File.Exists(filename)) 	{
			XmlDocument nsSummary = new XmlDocument();
			nsSummary.Load(filename);
			XmlElement ns = nsSummaries.CreateElement("namespace");
			nsSummaries.DocumentElement.AppendChild(ns);
			ns.SetAttribute("name", currentNs);
			ns.InnerText = nsSummary.SelectSingleNode("/Namespace/Docs/summary").InnerText;
		}
	}
	
	private static void CreateMember(string name, XmlElement input, XmlElement output) {
		XmlElement member = output.OwnerDocument.CreateElement("member");
		output.AppendChild(member);
		
		member.SetAttribute("name", name);
		
		foreach (XmlNode docnode in input.SelectSingleNode("Docs"))
			member.AppendChild(output.OwnerDocument.ImportNode(docnode, true));
	}
	
	private static string MakeArgs (XmlElement member)
	{
		XmlNodeList parameters = member.SelectNodes ("Parameters/Parameter");
		if (parameters.Count == 0)
			return "";
		StringBuilder args = new StringBuilder ();
		args.Append ("(");
		args.Append (XmlDocUtils.ToTypeName (parameters [0].Attributes ["Type"].Value, member));
		for (int i = 1; i < parameters.Count; ++i) {
			args.Append (",");
			args.Append (XmlDocUtils.ToTypeName (parameters [i].Attributes ["Type"].Value, member));
		}
		args.Append (")");
		return args.ToString ();
	}

	private static void WriteXml(XmlElement element, System.IO.TextWriter output) {
		XmlTextWriter writer = new XmlTextWriter(output);
		writer.Formatting = Formatting.Indented;
		writer.Indentation = 4;
		writer.IndentChar = ' ';
		element.WriteTo(writer);
		output.WriteLine();	
	}
}

}
