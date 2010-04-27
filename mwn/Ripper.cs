
//
// NewsRipper -- a simple utility to help you building
// Mono Weekly News issues.
//
// (c) 2003, Jaime Anguiano Olarra (jaime@gnome.org)
//
// It's a very dirty and quick program. Don't use it!. ;-)
//

using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Xml;
using System.Threading;
using System.Collections;

public class NewsRipper {
	static FileStream output = new FileStream ("issue.html", FileMode.Create);
	StreamWriter writer = new StreamWriter (output);
	ArrayList headlines;
	ArrayList newsItems;
	bool withInterview = false;
	
	public NewsRipper ()
	{
		WriteHeader ();
		WriteTOC ();
		WriteNewsItems ();
		WriteInterview (this.withInterview);
		WriteMailActivity ();
		WriteCVSActivity ();
		GenRSS ();
		WriteAbout ();
	}

	void WriteHeader ()
	{
		Console.WriteLine ("Writing header...");
		StreamReader reader = new StreamReader ("header");
		string line;
		while ((line = reader.ReadLine ()) != null)
		{
			writer.WriteLine (line);
		}
		reader.Close ();
		reader = null;
	}

	void WriteTOC ()
	{
		int ctr = 2;
		
		DirectoryInfo di = new DirectoryInfo (".");
		newsItems = new ArrayList ();
		headlines = new ArrayList ();
				
		FileInfo[] files = di.GetFiles ();
		foreach (FileInfo fi in files)
		{
			string name = fi.Name;
			if (name.EndsWith ("news")) {
				StreamReader reader = new StreamReader (name);
				string headline = reader.ReadLine ();
				headlines.Add (headline);
				newsItems.Add (name);
				reader.Close ();
				reader = null;
			}
			if (name.EndsWith ("interview")) {
				withInterview = true;
			}
		}

		writer.WriteLine ("\t\t<ul>");
		writer.WriteLine ("\t\t<li>1. Headlines</li><ul>");
		foreach (string s in headlines)
		{
			writer.WriteLine ("\t\t<li><a href=\"#" + s + "\">" + s + "</a></li>");
			Console.WriteLine (s); 
		}
		writer.WriteLine ("\t\t</ul>");
		string str;
		if (withInterview) {
			string s = ((ctr++).ToString() + ". Meet the team");
			writer.WriteLine ("<li><a href=\"#meet\">" + s + "</a></li>");
		}
		str = ((ctr++).ToString() + ". Mailing lists activity");
		writer.WriteLine ("<li><a href=\"#mail\">" + str + "</a></li>");
		str = ((ctr++).ToString() + ". CVS Activity");
		writer.WriteLine ("<li><a href=\"#cvs\">" + str + "</a></li>");
		str = ((ctr).ToString() + ". About this issue");
		writer.WriteLine ("<li><a href=\"#about\">" + str + "</a></li></ul>");
	}

	void WriteNewsItems ()
	{
		bool includesMagicWord = false;
		for (int i=0; i < newsItems.Count; ++i)
		{
			StreamReader reader = new StreamReader ((newsItems[i]).ToString());
			string line;
			string headline = (headlines[i]).ToString();
			writer.WriteLine ("<h3><a name=\"" + headline + "\">" + headline + "</h3>");

			
			if (headline.IndexOf ("FreeBSD") != -1) {
				string pixmap = "Daemon.png\"></td><td>";
				writer.WriteLine ("<table><tr><td><img src=\"pixmaps/" + pixmap + "</td><td>");
				includesMagicWord = true;
			}

			if (headline.IndexOf ("Mono 0.") != -1) {
				string pixmap = "version.png\"></td><td>";
				writer.WriteLine ("<table><tr><td><img src=\"pixmaps/" + pixmap + "</td><td>");
				includesMagicWord = true;
			}

			
			if (headline.IndexOf ("MPhoto") != -1) {
				string pixmap = "Digital-Camera2.png\"></td><td>";
				writer.WriteLine ("<table><tr><td><img src=\"pixmaps/" + pixmap + "</td><td>");
				includesMagicWord = true;
			}
			
			if (headline.IndexOf ("Debian") != -1) {
				string pixmap = "Box-DEB.png\"></td><td>";
				writer.WriteLine ("<table><tr><td><img src=\"pixmaps/" + pixmap + "</td><td>");
				includesMagicWord = true;
			}

			if (headline.IndexOf ("Monodoc") != -1) {
				string pixmap = "docs.png\"></td><td>";
				writer.WriteLine ("<table><tr><td><img src=\"pixmaps/" + pixmap + "</td><td>");
				includesMagicWord = true;
			}

			if (	headline.IndexOf ("Acacia") != -1 ||
				headline.IndexOf ("Platano") != -1) {
				string pixmap = "Applications.png\"></td><td>";
				writer.WriteLine ("<table><tr><td><img src=\"pixmaps/" + pixmap + "</td><td>");
				includesMagicWord = true;
			}

			if (	(headline.ToUpper()).IndexOf ("TEAM") != -1 ||
				(headline.ToUpper()).IndexOf ("MONOERS") != -1) {
				string pixmap = "monoers.png\"></td><td>";
				writer.WriteLine ("<table><tr><td><img src=\"pixmaps/" + pixmap + "</td><td>");
				includesMagicWord = true;
			}

			if (headline.IndexOf ("Java") != -1 ||
				headline.IndexOf ("JScript") != -1) {
				string pixmap = "GNOME-Text-Java.png\"></td><td>";
				writer.WriteLine ("<table><tr><td><img src=\"pixmaps/" + pixmap + "</td><td>");
				includesMagicWord = true;
			}
			
			if (headline.IndexOf ("embeded") != -1) {
				string pixmap = "Handspring_Prism.png\"></td><td>";
				writer.WriteLine ("<table><tr><td><img src=\"pixmaps/" + pixmap + "</td><td>");
				includesMagicWord = true;
			}
			
			if (headline.IndexOf ("Wine") != -1 ||
				headline.IndexOf ("Forms") != -1) {
				string pixmap = "Wine.png\"></td><td>";
				writer.WriteLine ("<table><tr><td><img src=\"pixmaps/" + pixmap + "</td><td>");
				includesMagicWord = true;
			}

			if (headline.IndexOf ("Mozilla") != -1 ||
				headline.IndexOf ("GtkMoz") != -1) {
				string pixmap = "Mozilla-Star.png\"></td><td>";
				writer.WriteLine ("<table><tr><td><img src=\"pixmaps/" + pixmap + "</td><td>");
				includesMagicWord = true;
			}

			if (headline.IndexOf ("MonoBasic") != -1 ||
				headline.IndexOf ("mbas") != -1 ||
				headline.IndexOf ("Visual Basic") != -1) {
				string pixmap = "gnomebasic.png\"></td><td>";
				writer.WriteLine ("<table><tr><td><img src=\"pixmaps/" + pixmap + "</td><td>");
				includesMagicWord = true;
			}

			if (headline.IndexOf ("Virtuoso") != -1 ||
				headline.IndexOf ("OpenLink") != -1) {
				string pixmap = "openlink.png\"></td><td>";
				writer.WriteLine ("<table><tr><td><img src=\"pixmaps/" + pixmap + "</td><td>");
				includesMagicWord = true;
			}

			if (headline.IndexOf ("MCS") != -1 ||
				headline.IndexOf ("mcs") != -1) {
				string pixmap = "mcs.png\"></td><td>";
				writer.WriteLine ("<table><tr><td><img src=\"pixmaps/" + pixmap + "</td><td>");
				includesMagicWord = true;
			}

			
			if (!includesMagicWord) {
				string pixmap = "Rupert.png\"></td><td>";
				writer.WriteLine ("<table><tr><td><img src=\"pixmaps/" + pixmap + "</td><td>");
			}
			
			while ((line = reader.ReadLine ()) != null)
			{
				if (line.StartsWith ("1."))
					continue;
				writer.WriteLine ("\t\t" + line);
			}
			includesMagicWord = false;
			writer.WriteLine ("</td></tr></table>");
			writer.WriteLine ("\n\n");
			reader.Close ();
			reader = null;
		}
	}

	void WriteInterview (bool addInterview)
	{
		if (!addInterview)
			return;

		StreamReader reader = new StreamReader ("interview");
		string line;
		line = reader.ReadLine ();
		writer.WriteLine ("<h3><a name=\"meet\">2. Meet the team. This week " + line  + "</h3>");
		
		writer.WriteLine ("\t\t<p>The Mono team is integrated by contributors all ");
		writer.WriteLine ("\t\taround the world that are working really hard to get ");
                writer.WriteLine ("\t\tthis project going further. In this section we will ");
	        writer.WriteLine ("\t\tbe meeting this people so we can know more about them");
                writer.WriteLine ("\t\tand what they are doing.</p>");
	
		while ((line = reader.ReadLine ()) != null)
		{
			writer.WriteLine ("\t\t" + line);
		}
		reader.Close ();
		reader = null;
	}
	
	void WriteMailActivity ()
	{
		string n = "<h3><a name=\"mail\">2. ";
		if (withInterview)
			n = "<h3><a name=\"mail\">3. ";
		writer.WriteLine (n + "Mailing lists activity</h3>");

		StreamReader reader = new StreamReader ("mailsummary");
		string line;
		while ((line = reader.ReadLine ()) != null)
			writer.WriteLine (line);
		reader.Close ();
		reader = null;
	}

	static void GetMonoPatches ()
	{
		string url = "http://lists.ximian.com/archives/public/mono-patches/";
		Console.Write ("Enter year-month.txt (f.ex. 2003-April.txt): ");
		HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url + Console.ReadLine ());
		Console.WriteLine ("\n\tPlease wait, downloading a mono-patches mailing-list archive (~15) may take sometime...");
		HttpWebResponse res = null;
		Console.WriteLine ("\n");
		try {
			res = (HttpWebResponse) req.GetResponse ();
		}
		catch (Exception e) {
			Console.WriteLine (e);
		}
		StreamReader sr = new StreamReader (res.GetResponseStream(), Encoding.ASCII);
		StreamWriter sw = new StreamWriter ("cvslog-prev");
		string line;
		while ((line = sr.ReadLine ()) != null)
		{
			sw.WriteLine (line);
		}
		sr.Close ();
		sw.Flush ();
		sw.Close ();
		sr = null;
		sw = null; 
	}

	static void ParsePatches ()
	{
		StreamReader reader = new StreamReader ("cvslog-prev");
		StreamWriter cvslogWriter = new StreamWriter ("cvslog");
		string line;
		string prev = "\n";
		Console.Write("Enter starting date (f.ex. 08 Apr): ");
		string search = Console.ReadLine ();
		Console.WriteLine ("\nSeaching " + search + "...");
		bool markFound = false;

		while ((line = reader.ReadLine ()) != null)
		{
			string candidate;
			if (line.StartsWith ("Date:")) {
			    candidate = line.Substring (11, 6);
			    if (candidate == search)
				    markFound = true;
			}
			if (markFound) {
				if (line.StartsWith ("From:"))
					cvslogWriter.WriteLine (line);
				if (line.StartsWith ("Subject:"))
					cvslogWriter.WriteLine (line);
			}
			prev = line;
		}
	}

	void GetCVSStatistics ()
	{
		Hashtable authors = new Hashtable ();

		int tctr = 0;
		int actr = 1;
		string l;
		StreamReader r = new StreamReader ("cvslog");
		while ((l = r.ReadLine ()) != null)
		{
			if (l.StartsWith ("From:"))
			{
				string[] frags = l.Split (new char[] {' '});
				string author = null;
				try {
					// We skip "From:", "cvs email" and the
					// last field.
					for (int j=2; j < frags.Length - 1; ++j)
					{
						author += " " + frags[j];
					}

					authors.Add (actr, author);
					++actr;
					Console.WriteLine ("Adding author: " + author);
				}
				catch (Exception e)
				{ // with this we prevent to have one author
				  // more than once
				}
			}
				
			if (l.StartsWith ("Subject"))
			    ++tctr;
		}
		r.Close ();
		r = null;
		
		writer.WriteLine ("<p>\n<b>Authors:</b>" + authors.Count + "</p>");

		
		writer.WriteLine ("<p>\n<b>Total of commits:</b> " + tctr + "</p>"); 

		string[] modules;
		const int MODULES_LENGTH = 21;
		modules = new string[MODULES_LENGTH] {
			"mono",
			"mono/mono",
			"mono/doc",
			"mono/jit",
			"mcs",
			"mcs/mcs",
			"mcs/mbas",
			"mcs/class",
			"mcs/class/corlib",
			"gtk-sharp",
			"XML",
			"System.Web",
			"xsp",
			"mod_mono",
			"janet",
			"monodoc",
			"debugger",
			"Remoting",
			"JScript",
			"LOGO",
			"Forms"};

		writer.WriteLine ("<table border=\"1\" bgcolor=\"#a8a1a1\"><tr><td><b>Module</b></td><td><b>Commits</b></td></tr>");
		for (int i=0; i < MODULES_LENGTH; ++i)
		{
			StreamReader logReader = new StreamReader ("cvslog");
			string line;
			int ctr = 0;
			while ((line = logReader.ReadLine ()) != null)
			{
				if (line.StartsWith ("From"))
				    continue;
				if (line.IndexOf (modules[i]) != -1) 
				    ++ctr;
			}
			logReader.Close ();
			logReader = null;
			
			if (ctr < 4)
				continue;
			
			writer.WriteLine ("<tr><td>" + modules[i] + "</td><td>" + ctr + "</td></tr>");
		}
		writer.WriteLine ("</table>");
	}
	
	void WriteCVSActivity ()
	{
		string n = "<h3><a name=\"cvs\">3. ";
		if (withInterview)
			n = "<h3><a name=\"cvs\">4. ";
		writer.WriteLine (n + "CVS Activity</h3>");

		GetCVSStatistics ();
	}

	void WriteAbout ()
	{
		StreamReader reader;
		reader = new StreamReader ("contributors");
		
		string line;
		line = reader.ReadLine ();
		string n = "<h3><a name=\"about\">4. ";
		if (withInterview)
			n = "<h3><a name=\"about\">5. ";

		writer.WriteLine (n + line + "</h3>\n");

		writer.WriteLine ("<table>");
		while ((line = reader.ReadLine ()) != null)
		{
			writer.WriteLine ("\t<tr><td>" + line + "</td></tr>");
		}
		writer.WriteLine ("</table>");
		writer.WriteLine ("\n\n\t<center><a href=\"mailto:jaime@gnome.org\">Contact</a></center>");
		writer.WriteLine ("</html>");
		writer.Flush ();
		writer.Close ();
		writer = null;
		reader.Close ();
		reader = null;
	}

	void GenRSS ()
	{
		DirectoryInfo di = new DirectoryInfo (".");
		// This is dedicated to Joe Shaw :-)
		Queue descriptions = new Queue (); 

		// Yep we are duplicating the work but it's not worth the pay
		// making this more efficient. KISS principle :-)
		FileInfo[] files = di.GetFiles ();
		foreach (FileInfo fi in files)
		{
			string name = fi.Name;
			if (name.EndsWith ("news")) {
				StreamReader reader = new StreamReader (name);
				string headline = reader.ReadLine ();
				// We skip these two because of the file format.
				string whiteline = reader.ReadLine ();
				string pline = reader.ReadLine ();
				// We want this:
				string description;
				description = reader.ReadLine () + "...";
				if (description.IndexOf ('<') != -1)
					description = headline + "...";
				descriptions.Enqueue (description);
				
				reader.Close ();
				reader = null;
			}
		}

		StreamWriter writer = new StreamWriter ("mwn.rss");
		writer.WriteLine ("<?xml version=\"1.0\"?>");
		writer.WriteLine ("<rss version=\"0.92\">");
		writer.WriteLine ("<channel>");
		writer.WriteLine ("<title>Mono Weekly News</title>");
		writer.WriteLine ("<link>http://monoevo.sf.net/mwn/index.html</link>");
		writer.WriteLine ("<description>Weekly news from the Mono project: a portable implementation of the .NET Framework</description>");
		writer.WriteLine ("<webMaster>jaime@gnome.org</webMaster>");
		writer.WriteLine ("<managingEditor>jaime@gnome.org</managingEditor>");
		writer.WriteLine ("<pubDate>" + DateTime.Now + "</pubDate>");
				
		foreach (string s in headlines)
		{
			writer.WriteLine ("<item>");
			writer.WriteLine ("<title>\n" + s + "\n</title>");
			// Dedicated to Joe Shaw too :-)
			writer.WriteLine ("<link>http://monoevo.sf.net/mwn/index.html#" + s + "</link>");
			writer.WriteLine ("<description>" + descriptions.Dequeue() + "</description></item>");
		}
		writer.WriteLine ("</channel>");
		writer.WriteLine ("</rss>");
		writer.Flush ();
		writer.Close ();
		writer = null;
	}

	public static void SetDate (string date)
	{
		StreamReader reader = new StreamReader ("issue.html");
		StreamWriter writer = new StreamWriter ("issueWithDate.html");
		string line;
		while ((line = reader.ReadLine ()) != null)
		{
			if (line.IndexOf("Date:") != -1)
				line = date;
				
			writer.WriteLine (line);
		}
		writer.Flush ();
		writer.Close ();
		writer = null;
		reader.Close ();
		reader = null;
	}
	
	static string GetRSSURL ()
	{
		return "http://monoevo.sf.net/mwn/mwn.rss";
	}
	
	public static void Main (string[] args)
	{
		string date = null;
		NewsRipper ripper;
		if (args.Length > 0) {
			switch (args[0])
			{
			case "--date":
				date = args[1];
				break;
			case "--get-patches":
				ThreadStart ts = new ThreadStart (NewsRipper.GetMonoPatches);
				Thread t = new Thread (ts);
				t.Start ();
				return;
			case "--help":
				Usage ();
				break;
			case "--parse-patches":
				ParsePatches ();
				break;
			default:
				Console.WriteLine ("Unknow argument");
				break;
			}
		}
		
		ripper = new NewsRipper ();
		if (date != null)
			SetDate (date);
	}

	static void Usage ()
	{
		Console.WriteLine ("\t--get-patches\tRetrieve the Mono Patches archive");
		Console.WriteLine ("\t--help\tPrint this help");
		Console.WriteLine ("\t--working-mode\tRun till entering 'Q'");
	}
}
