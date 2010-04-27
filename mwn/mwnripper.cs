using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Xml;
using System.Threading;
using System.Collections;

namespace MonoWeeklyNews {
public class Ripper {
	private static ArrayList items;
	private static XmlDocument doc;
	private static bool interview = false;
	private static string interviewed = null;
	private static string interview_file = null;
	private static bool interview_written = false;
	private static uint mail_idx;
	
	public static void Main ()
	{
		bool TRUE = true;
		StreamWriter writer = new StreamWriter ("issue.html");
		Console.WriteLine ("\n\tMono Weekly News editing tool\n\t\tv 0.1\n");
		Console.WriteLine ("\tThis tool is just for editing purpouses.");
		Console.WriteLine ("\tUse the contributing tools available at the MWN site if\n" +
				   "\tyou want to contribute some news.\n");
		
		while (TRUE) {
			PrintMenu ();
			switch (Convert.ToInt32(Console.ReadLine ())) {
			case 0:
				LoadEntries ();
				break;
			case 1:
				string s, b, c;
				Console.Write ("Enter subject: ");
				s = Console.ReadLine ();
				Console.WriteLine ("Enter body:");
				b = Console.ReadLine ();
				Console.Write ("Enter contributor:");
				c = Console.ReadLine ();
				items.Add (new news_item ((byte) (items.Count + 1), s, b, c));
				break;
			case 2:
				int old;
				Console.Write ("Enter current element's index: ");
				old = (Convert.ToInt32(Console.ReadLine ()) - 1);
				Console.Write ("Enter new index: ");
				ChangeOrder (old, (Convert.ToInt32(Console.ReadLine ()) - 1));
				break;
			case 3:
				Console.Write ("Enter item index: ");
				items[Convert.ToInt32(Console.ReadLine ())] = null;
				break;
			case 4:
				foreach (news_item ni in items)
				Console.WriteLine (ni.Index + ":\t" + ni.Headline + "\n" + ni.Content);
				break;
			case 5:
				SaveEntries ();
				break;
			case 6:
				WriteHeader (writer);
				WriteNewsItems (writer);
				break;
			case 7:
				WriteInterview (writer);
				break;
			case 8:
				WriteMailActivity (writer);
				break;
			case 9:
				WriteCvsStatistics (writer);
				WriteFoot (writer);
				writer.Flush ();
				break;
			case 10:
				GenerateRSSFeed ();
				GeneratePrintVersion ();
				TRUE = false;
				break;
			default:
				Console.WriteLine ("Unknown option");
				break;
			}
		}
		writer.Close ();
		writer = null;
	}
	
	private static void PrintMenu ()
	{
		string[] menu;
		menu = new string [11] {"0. Load entries.", "1. Add entry.", "2. Order entry.", "3. Remove entry.", "4. Show entries.",
					"5. Save entries.", "6. Write header and entries to issue.", "7. Write interview to issue.",
				        "8. Write mailing list activity to issue.", "9. Write CVS statistics to issue.", "10. Quit.\n"};
		for (uint i=0; i < menu.Length; ++i)
			Console.WriteLine ("\t" + menu[i]);
		Console.Write ("mwnripper> ");
	}

	private static void LoadEntries ()
	{
		DirectoryInfo dinfo;
		XmlNodeList input;
		string subject, body, contributor;

		byte FOUND = 0;
		items = new ArrayList ();
		dinfo = new DirectoryInfo (".");
		doc = new XmlDocument ();
		foreach (FileInfo file in dinfo.GetFiles())
		{
			if (file.Name.EndsWith (".mwnitem")) {
				doc.Load (file.Name);
				++FOUND;
				input = doc.GetElementsByTagName ("mwnitem");
				foreach (XmlNode xmlitem in input)
				{
					byte index;
					subject = xmlitem.Attributes["subject"].Value;
					body = xmlitem.Attributes["body"].Value;
					contributor = xmlitem.Attributes["contrib"].Value;
					if (xmlitem.Attributes[0].Name == "index")
						index = (xmlitem.Attributes["index"].Value != null) ? Convert.ToByte (xmlitem.Attributes["index"].Value) : FOUND;
					items.Add (new news_item (FOUND, subject, body, contributor));
					Console.WriteLine (subject + " " + contributor);
				}
			}
		}
		doc =null;
		if (FOUND == 0)
			items = null;
	}

	private static void ChangeOrder (int oldValue, int newValue)
	{
		news_item item, tmp;

		if (newValue < oldValue) {
			tmp = (news_item) items[newValue];
			item = (news_item) items[oldValue];
			item.Index = (byte) (newValue + 1);
			items[newValue] = item;
			tmp.Index = (byte) (newValue + 2);
			items[newValue + 1] = tmp;
			for (int i = newValue + 1; i < items.Count; ++i) {
				int j;

				j = i + 1;
				if (j == oldValue) 
					continue;
				item = (news_item) items[i];
				item.Index = (byte) i;
				items[j] = item;
			}
		}
	}
	
	private static void SaveEntries ()
	{
		if (items == null)
			return;
		foreach (news_item it in items)
		{
			string title;
			string[] fragments;
			XmlElement item;
			XmlAttribute subject_attr, body_attr, contrib_attr, index_attr;

			fragments = it.Headline.Split (' ');
			title = null;
			foreach (string s in fragments)
			{
				title += s;
			}
			doc = new XmlDocument ();
			doc.AppendChild (doc.CreateProcessingInstruction ("xml", "version='1.0'"));
			item = doc.CreateElement ("mwnitem");
			index_attr = doc.CreateAttribute ("index");
			index_attr.Value = (it.Index).ToString();
			subject_attr = doc.CreateAttribute ("subject");
			subject_attr.Value = it.Headline;
			body_attr = doc.CreateAttribute ("body");
			body_attr.Value = it.Content;
			contrib_attr = doc.CreateAttribute ("contrib");
			contrib_attr.Value = it.Contributor;
			item.Attributes.Append (index_attr);
			item.Attributes.Append (subject_attr);
			item.Attributes.Append (body_attr);
			item.Attributes.Append (contrib_attr);
			doc.AppendChild (item);
			doc.Save ((it.Index).ToString() + "." + (title = title.Substring (0, 8)) + ".mwnitem");
		}
	}

	private static void WriteHeader (StreamWriter writer)
	{
		string line;

		StreamReader reader = new StreamReader ("header");
		while ((line = reader.ReadLine ()) != null) {
			if (line.IndexOf ("ate:") > 0) {
				line = "Date: " + String.Format ("{0:D}", DateTime.Now);
			}
			writer.WriteLine (line);
		}
		writer.Flush ();
		reader.Close ();
	}

	private static void WriteNewsItems (StreamWriter writer)
	{
		uint ctr = 2;
		string icon = null;
		
		writer.WriteLine ("<b>Table of contents</b>");
		writer.WriteLine ("<ul><li><b>1. Headlines:</b><br /><ul>");
		foreach (news_item item in items) {
			writer.WriteLine ("<li><a href=\"#" + item.Index + "\">1." + item.Index + " " + item.Headline + "</a></li>");
		}
		writer.WriteLine ("</li></ul>");
		Console.Write ("Do we have an interview? [N/y]: ");
		if ((Console.ReadLine ()).ToUpper () == "Y") {
			interview = true;
			Console.Write ("Name of the developer: ");
			interviewed = Console.ReadLine ();
			Console.Write ("Interview filename: ");
			interview_file = Console.ReadLine ();
		}
		if (interview)
			writer.WriteLine ("<li><b>" + (ctr++).ToString() + ". Meet the team!. This week <a href=\"#interview\">" + interviewed + "</a></b></li>");
		mail_idx = ctr++;
		writer.WriteLine ("<li><b>" + (mail_idx).ToString() + ". <a href=\"#mail\">Mailing lists activity</a></b></li>");
		writer.WriteLine ("<li><b>" + (mail_idx + 1).ToString() + ". <a href=\"#cvs\">CVS activity</a></li></b>");
		writer.WriteLine ("</ul>");
		foreach (news_item item in items) {
			if (item.Headline.IndexOf ("onodoc") > 0 || item.Headline.IndexOf ("docum") > 0 || item.Headline.IndexOf ("book") > 0)
				icon = "pixmaps/docs.png";
			if (item.Headline.IndexOf ("VB") > 0 || item.Headline.IndexOf ("Mbas") > 0)
				icon = "pixmaps/gnomebasic.png";
			if (icon == null)
				icon = "pixmaps/Rupert.png";
			writer.WriteLine ("<h3><a name=\"" + item.Index + "\"></a><img src=\"" + icon + "\">1." + item.Index + " " + item.Headline + "</h3>");
			writer.WriteLine ("<p>" + item.Content + "</p>");
		}
		if (interview) 
			WriteInterview (writer);
	}

	private static void WriteInterview (StreamWriter writer)
	{
		byte ctr = 0;
		string line;
		
		if (interview_written)
			return;
		if (interview_file == null) {
			Console.Write ("Enter file to link to: ");
			interview_file = Console.ReadLine ();
		}
		if (interviewed == null) {
			Console.Write ("Name of the developer: ");
			interviewed = Console.ReadLine ();
		}
		writer.WriteLine ("<a name=\"interview\"</a><h2>2. Meet the team!. This week <a href=\"" + interview_file + "\">" + interviewed + "</a></h2>");
		StreamReader reader = new StreamReader (interview_file);
		while (ctr < 3) {
			line = reader.ReadLine ();
			if (line.StartsWith ("<p>") || ctr > 0) {
				ctr++;
				writer.WriteLine (line);
			}
		}
		reader.Close ();
		writer.WriteLine ("...<a href=\"" + interview_file + "\">continue</a>");
		interview_written = true;
	}

	private static void WriteMailActivity (StreamWriter writer)
	{
		string response;
		string mailactivity;
		
		writer.WriteLine ("<a name=\"mail\"</a><h2>" + (mail_idx).ToString() + ". Mailing lists activity</h2>");
		Console.Write ("Read from 'mailactivity' file? (Y/n): ");
		response = (Console.ReadLine ()).ToUpper();
		if (response == "N" || response == "NO") {
			Console.WriteLine ("Warning!, we highly encourage you to write it\n"
					   + "into a file instead of entering it on the fly\n"
					   + "at least save a copy of the text before saving.");
			Console.WriteLine ("Enter the mail activity in a single line.");
			mailactivity = Console.ReadLine ();
		} else {
			StreamReader reader;
			reader = new StreamReader ("mailactivity");
			mailactivity = reader.ReadToEnd ();
			reader.Close ();
			reader = null;
		}
		writer.WriteLine (mailactivity);
	}

	private static void WriteCvsStatistics (StreamWriter writer)
	{
		string line;
		FileInfo file = null;
		uint ctr = 0;
		
		file = new FileInfo ("cvsstats.html");
		if (file != null) {
			StreamReader reader = new StreamReader ("cvsstats.html");
			while ((line = reader.ReadLine ()) != null) {
				if (ctr++ == 0)
					line = "<h2><a name=\"cvs\">" + (mail_idx + 1).ToString() + ". CVS Statistics</a></h2><center>";
				writer.WriteLine (line);
			}
			writer.WriteLine ("</center>");
			reader.Close ();
			reader = null;
		} else
			Console.WriteLine ("Error: 'cvsstats.html' file not found. You might want to run monocvsspy.exe.");
	}

	private static void WriteFoot (StreamWriter writer)
	{
		string line;
		Hashtable contributors = new Hashtable ();
		
		writer.WriteLine ("<h3>Contributors to this issue</h3>\n<p>This issue has been possible due to the effort of: ");
		writer.WriteLine ("<ul>");
		foreach (news_item item in items)
		{
			if (contributors[item.Contributor] != null)
				continue;
			line = item.Contributor.Replace ("<", "&lt;");
			line = line.Replace (">", "&gt;");
			writer.WriteLine ("<li>" + line + "</li>");
			contributors.Add (item.Contributor, item.Contributor);
		}
		writer.WriteLine ("</ul></p>");
		StreamReader reader = new StreamReader ("footer");
		while ((line = reader.ReadLine ()) != null)
			writer.WriteLine (line);
		reader.Close ();
		reader = null;
	}

	private static void GenerateRSSFeed ()
	{
		StreamWriter rssw = new StreamWriter ("mwn.rss");
		rssw.WriteLine ("<?xml version=\"1.0\"?>");
		rssw.WriteLine ("<rss version=\"0.92\">");
		rssw.WriteLine ("<channel>");
		rssw.WriteLine ("<title>Mono Weekly News</title>");
		rssw.WriteLine ("<link>http://monoevo.sf.net/mwn/index.html</link>");
		rssw.WriteLine ("<description>Weekly news from the Mono project: a portable implementation of the .NET Framework</description>");
		rssw.WriteLine ("<webMaster>jaime@gnome.org</webMaster>");
		rssw.WriteLine ("<managingEditor>jaime@gnome.org</managingEditor>");
		rssw.WriteLine ("<pubDate>" + DateTime.Now + "</pubDate>");

		foreach (news_item item in items)
		{
			string descrip;
			rssw.WriteLine ("<item>");
			rssw.WriteLine ("<title>\n" + item.Headline + "\n</title>");
			// Dedicated to Joe Shaw too :-)
			rssw.WriteLine ("<link>http://monoevo.sf.net/mwn/index.html#" + item.Index + "</link>");
			descrip = item.Content.Replace ("\"", "&quot;");
			descrip = descrip.Replace ("<", "&lt;");
			descrip = descrip.Replace (">", "&gt;");
			rssw.WriteLine ("<description>" + descrip + "</description></item>");
		}
		rssw.WriteLine ("</channel>");
		rssw.WriteLine ("</rss>");
		rssw.Flush ();
		rssw.Close ();
		rssw = null;
	}

	private static void GeneratePrintVersion ()
	{
		byte ctr = 2;
		StreamWriter w = new StreamWriter ("issue.txt");
		w.WriteLine ("Mono Weekly News: " + String.Format ("{0:D}", DateTime.Now) + "\n");
		w.WriteLine ("Table of contents:\n");
		foreach (news_item item in items)
		{
			w.WriteLine ("1." + (item.Index).ToString() + " " + item.Headline);
		}
		
		if (interview)
			w.WriteLine ((ctr++).ToString() + ". Interview with " + interviewed);
		w.WriteLine ((ctr++).ToString() + ". Mailing lists activity");
		w.WriteLine ((ctr).ToString() + ". CVS statistics\n");

		foreach (news_item item in items)
		{
			string content = null;
			bool copy = true;
			
			w.WriteLine ("\n1." + (item.Index).ToString() + " " + item.Headline + "\n");
			for (int i=0; i < item.Content.Length; i++) {
				if (item.Content[i] == '<')
					copy = false;
				if (item.Content[i] == '>')
					copy = true;
				if (copy && item.Content[i] != '>')
					content += (item.Content[i]).ToString();
			}
			
			for (int i=0; i < content.Length; i++) {
				if (i % 60 == 0 && i > 0) {
					while (i < content.Length - 1) {
						if (!Char.IsWhiteSpace (content, i))
							w.Write (content[i++]);
						else {
							w.WriteLine ();
							i++;
							break;
						}
					}
				}
				w.Write (content[i]);
			}
			w.WriteLine ();
		}
		w.WriteLine ();
		if (interview)
			w.WriteLine ("2. Interview with " + interviewed);
		w.WriteLine ((ctr - 1).ToString() + ". Mailing lists activity");
		w.WriteLine ((ctr).ToString() + ". CVS Statistics\n\n");
		StreamReader r = new StreamReader ("cvsstats.html");
		string line;
		string l = null;
		
		while ((line = r.ReadLine ()) != null)
		{
			if (!line.StartsWith ("<tr><td>"))
				continue;
			if (line.IndexOf ("Author") > 0)
				line = "  Author\t\tCommits\n";
			if (line.IndexOf ("Module") > 0) {
				line = "\n\n  Module\t\tCommits\n";
				l = "  ";
			}
			if (line.EndsWith ("</td></tr>")) {
				line = line.Replace ("<tr><td>", l + "");
				line = line.Replace ("</td><td>", "    ");
				line = line.Replace ("</td></tr>", "");
			}
			w.WriteLine (line);
		}
		
		w.WriteLine ("\n\nThis is a printer friendly ASCII version of the MWN.");
		w.WriteLine ("Visit http://monoevo.sf.net/mwn/index.html to get the "
			+ "HTML version of it.");
		w.WriteLine ("(C) 2002-2003 The Mono Weekly News team");
		w.Flush ();
		w.Close ();
	}
}

public struct news_item {
	byte index;
	string headline, content, contributor;

	public news_item (byte index, string headline, string content, string contributor)
	{
		this.index = index;
		this.headline = headline;
		this.content = content;
		this.contributor = contributor;
	}

	public byte Index { get { return index; } set { index = value; }}
	public string Headline { get { return headline; } set { headline = value; }}
	public string Content { get { return content; } set { content = value; }}
	public string Contributor { get { return contributor; } set { contributor = value; }}
}
}
