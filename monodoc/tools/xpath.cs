/* A tool to run an XPath expression on an XML file
   in standard input, outputting the result to
   standard output. */

using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;

public class xpath {

	public static void Main(string[] arg) {
		if (arg.Length < 1) throw new ArgumentException("Must pass one or two command line arguments.");
	
		StringWriter sw = new StringWriter();
		string s;
		while ((s = Console.ReadLine()) != null) {
			sw.WriteLine(s);
		}
		
		XmlDocument d = new XmlDocument();
		d.LoadXml(sw.ToString());
		
		object ret;
		
		if (arg.Length == 1) {
			ret = d.CreateNavigator().Evaluate(arg[0]);
		} else if (arg.Length == 2 && arg[0] == "-expr") {
			ret = d.CreateNavigator().Evaluate(arg[1]);
		} else if (arg.Length == 2 && arg[0] == "-node") {
			ret = d.SelectSingleNode(arg[1]);
		} else {
			throw new ArgumentException("Bad command line arguments.");
		}
		
		if (ret is XPathNodeIterator) {
			XPathNodeIterator iter = (XPathNodeIterator)ret;
			while (iter.MoveNext()) {
				Console.WriteLine(iter.Current);
			}
		} else if (ret is XmlNode) {
			Console.WriteLine(((XmlNode)ret).InnerXml);
		} else {
			Console.WriteLine(ret);
		}
	}
}
