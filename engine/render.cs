//
// This is just a set of wrapper classes, we will be using Xslt to generate 
// real output.  For now we just have some templates
//
namespace Monodoc {
using System;
using System.IO;
using System.Xml;
using System.Xml.Xsl;

class HtmlRender {
	void Header (TextWriter output, string title)
	{
		output.WriteLine ("<html><head><title>{0}</title></head><body>", output, title);
	}

	void Footer (TextWriter output)
	{
		output.WriteLine ("</body></html>");
	}

	/// <summary>
	///   Renders the type information from the TypeDoc
	/// </summary>
	public void TypeRender (TextWriter output, TypeDoc td)
	{
		string type_name = td.GetTypeName ();
		string type_summary = td.GetTypeSummary ();
		
		Header (output, "Title");

		output.WriteLine ("{0} Class\n<p>\n{1}\n\n", type_name, type_summary);
		output.WriteLine ("<h3>Thread Safety</h3>\n{0}\n\n", td.GetThreadSafety ());
		Footer (output);
	}
}

public class TypeDoc {
	public XmlNode node;

	public TypeDoc (string file)
	{
		XmlDocument d = new XmlDocument ();

		d.Load (file);
		Init (d.DocumentElement);
	}
	
	public TypeDoc (XmlNode root)
	{
		Init (root);
	}

	void Init (XmlNode root)
	{
		this.node = root;
	}

	public string GetTypeName ()
	{
		return node.Attributes ["Name"].Value;
	}

	public string GetTypeSummary ()
	{
		XmlNode n = node.SelectSingleNode ("/Type/Docs/summary");

		return n.InnerXml;
	}

	public string GetThreadSafety ()
	{
		XmlNode n = node.SelectSingleNode ("/Type/ThreadSafetyStatement");

		return n.InnerXml;
	}
}

class Test {

	static void Main ()
	{
		TypeDoc td = new TypeDoc ("../class/corlib/en/System/Object.xml");
		HtmlRender render = new HtmlRender ();

		render.TypeRender (Console.Out, td);
	}
}
}
