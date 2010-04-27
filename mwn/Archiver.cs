//
// Mono Weekly News Archives Generator
//
// (c) 2003, Jaime Anguiano Olarra (jaime@gnome.org)
//

using System;
using System.IO;
using System.Collections;

public class Archiver {
	Queue entries;
	ArrayList rssfiles;
	Queue descriptions;
	StreamWriter writer;
	
	public Archiver ()
	{
		rssfiles = new ArrayList ();
		descriptions = new Queue ();
		writer = new StreamWriter ("archives.html");
		WriteFile ("archeader");
		GetRSSDescriptions ();
		GenArchivesCells ();
		WriteFile ("contact");
		writer.Flush ();
		writer.Close ();
		writer = null;
		GenRSS();
	}

	public void WriteFile (string file)
	{
		StreamReader reader = new StreamReader (file);
		string line;
		while ((line = reader.ReadLine()) != null)
			writer.WriteLine (line);
		reader.Close ();
		reader = null;
	}
	
	public void GenArchivesCells ()
	{
		entries = new Queue ();
		rssfiles = new ArrayList ();
		DirectoryInfo di = new DirectoryInfo ("archives");
		FileInfo[] files = di.GetFiles ();
		// We get the RSS files that are available
		foreach (FileInfo file in files)
		{
			string fileName = file.Name;
 			Console.WriteLine ("File: " + fileName);
			if (fileName.EndsWith ("rss")) {
				rssfiles.Add (fileName);
				Console.WriteLine ("Adding RSS file: " + fileName);
			}
		}
		int ctr = 0;
		// We iterate over all the files again.
		foreach (FileInfo file in files)
		{
			string fileName = file.Name;
			string rssName = "Not available";
			string rssLink = rssName;
			// if this is an archived html
			if (!fileName.EndsWith ("rss"))
			{
				// Check if there is a RSS file available
				foreach (object obj in rssfiles)
				{
					string name = obj.ToString();
					if (name.StartsWith (fileName.Substring (0, 8)))
					{
						rssName = name;
						rssLink = "<a href=\"archives/" + rssName + "\">" + rssName + "</a>";
					}
				}

				writer.WriteLine ("<tr><td>" + ctr + "</td><td>" + (++ctr).ToString() +  "</td><td><a href=\"archives/" + fileName + "\">" + fileName + "</a></td><td>" + rssLink + "</td></tr>");
			}
		}
	}

	public void GetRSSDescriptions ()
	{
		DirectoryInfo di = new DirectoryInfo ("archives");
		foreach (FileInfo file in di.GetFiles())
		{
			if ((file.Name).EndsWith (".mwn.rss"))
			{
				StreamReader reader = new StreamReader ("archives/" + file.Name);
				string line;
				while ((line = reader.ReadLine ()) != null)
				{
					if (line.StartsWith ("1.")) 
						descriptions.Enqueue (line);
				}
			}
		}
	}
	
	public void GenRSS ()
	{
		StreamWriter rsswriter = new StreamWriter ("archives.rss");
		rsswriter.WriteLine ("<?xml version=\"1.0\"?>");
		rsswriter.WriteLine ("<rss version=\"0.92\">");
		rsswriter.WriteLine ("<channel>");
		rsswriter.WriteLine ("<title>Mono Weekly News Archives</title>");
		rsswriter.WriteLine ("<link>http://monoevo.sf.net/mwn/archives.html</link>");
		rsswriter.WriteLine ("<description>Archives of the weekly news from the Mono project: a portable implementation of the .NET Framework</description>");
		rsswriter.WriteLine ("<webMaster>jaime@gnome.org</webMaster>");
		rsswriter.WriteLine ("<managingEditor>jaime@gnome.org</managingEditor>");
		rsswriter.WriteLine ("<pubDate>" + DateTime.Now + "</pubDate>");

		string address = "http://monoevo.sf.net/mwn/archives/";

		int ctr = 0;
		foreach (string s in entries)
		{
			rsswriter.WriteLine ("<item>");
			rsswriter.WriteLine ("<title>" + ++ctr + "</title>");
			rsswriter.WriteLine ("<link>" + address + s + "</link>");
			rsswriter.WriteLine ("<description>");
			for (int i=0; i < descriptions.Count; i++)
			{
				rsswriter.WriteLine (descriptions.Dequeue() + "||");
			}
			rsswriter.WriteLine ("</description>");
		}
		rsswriter.WriteLine ("</channel>\n</rss>");
		rsswriter.Flush ();
		rsswriter.Close ();
		rsswriter = null;
	}

	public static void Main ()
	{
		Archiver a = new Archiver ();
	}
}
