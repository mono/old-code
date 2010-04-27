//
// Metadata.cs : Metadata management for an album
//
// Author:
//   Ravi Pratap     (ravi@ximian.com)
// (C) 2002 Ximian, Inc.
//

using System;
using System.Text;
using System.IO;
using System.Xml;
using System.Collections;

public class Metadata {

	string dir;
	XmlDocument doc;

	string album_name;
	long   picture_count;
	
	PictureInfo [] picture_data;

	public Metadata (string path)
	{
		dir = path;

		doc = new XmlDocument ();

		string metadata_filename = dir + Path.DirectorySeparatorChar + "album-data.xml";

		if (File.Exists (metadata_filename))
			DumpXmlToArray (metadata_filename);
		
	}
	
	public void DumpArrayToXml ()
	{
		string metadata_filename = dir + Path.DirectorySeparatorChar + "album-data.xml";

		XmlTextWriter writer = new XmlTextWriter (metadata_filename, Encoding.UTF8);

		writer.WriteStartDocument (true);
		writer.WriteStartElement ("album", "www.ximian.com");
		writer.WriteAttributeString ("name", album_name);
	        writer.WriteAttributeString ("count", picture_count.ToString ());

		for (int i = 0; i < picture_count; ++i) {
			writer.WriteStartElement ("picture", "www.ximian.com");

			writer.WriteElementString ("location", "www.ximian.com", picture_data [i].Location);
			writer.WriteElementString ("title", "www.ximian.com", picture_data [i].Title);
			writer.WriteElementString ("date", "www.ximian.com", picture_data [i].Date);
			writer.WriteElementString ("keywords", "www.ximian.com", picture_data [i].Keywords);
			writer.WriteElementString ("comments", "www.ximian.com", picture_data [i].Comments);
			writer.WriteElementString ("index", "www.ximian.com", picture_data [i].Index.ToString ());
			
			writer.WriteEndElement ();
		}

		writer.WriteEndElement ();
		writer.WriteEndDocument ();
		writer.Close ();
	}

#if XML_READER_API
	
	public void DumpXmlToArray (string filename)
	{
		XmlTextReader reader = new XmlTextReader (filename);
		reader.WhitespaceHandling = WhitespaceHandling.None;

		int i = 0;
		while (reader.Read ()) {

			switch (reader.NodeType) {

			case XmlNodeType.Element :
				if (reader.Name == "album") {
					reader.MoveToNextAttribute ();
					album_name = reader.Value;
					reader.MoveToNextAttribute ();
					picture_count = Convert.ToInt64 (reader.Value);
				}
				
				break;
				
			case XmlNodeType.Text :
				if (reader.Name == "location")
					picture_data [i].Location = reader.Value;

				if (reader.Name == "title")
					picture_data [i].Title = reader.Value;

				if (reader.Name == "date")
					picture_data [i].Date = reader.Value;

				if (reader.Name == "keywords")
					picture_data [i].Keywords = reader.Value;

				if (reader.Name == "comments")
					picture_data [i].Comments = reader.Value;

				if (reader.Name == "index")
					picture_data [i].Index = Convert.ToInt64 (reader.Value);
				
			case XmlNodeType.EndElement :
				if (reader.Name == "picture") {
					i++;
					picture_data [i] = new PictureInfo ();
				}
				
				break;
				
			default :
				continue;
				break;
			}
		}

		reader.Close ();
	}

#else
	
	public void DumpXmlToArray (string filename)
	{
		doc.Load (filename);
		XmlNode root = doc.DocumentElement;
		XmlAttributeCollection attrs = root.Attributes;

		XmlAttribute attr;
		attr = (XmlAttribute) attrs [0];

		album_name = attr.InnerText;

		attr = (XmlAttribute) attrs [1];
		picture_count = Convert.ToInt64 (attr.InnerText);

		picture_data = new PictureInfo [picture_count];

		XmlNode picture = doc.FirstChild.FirstChild;

		int i = 0;
		while (picture != null) {

			XmlNodeList props = picture.ChildNodes;
			IEnumerator iter = props.GetEnumerator ();
			ArrayList data = new ArrayList ();
			
			while (iter.MoveNext ()) {
				XmlNode node = (XmlNode) iter.Current;

				string val = node.InnerText.Trim ();
				
				if (val != "") 
					data.Add (val);
			}
			
			if (picture.HasChildNodes) {
				picture_data [i] = new PictureInfo ((string) data [0], (string) data [1], (string) data [2],
								    (string) data [3], (string) data [4],
								    Convert.ToInt64 (data [5]));
				i++;
			}
			
			picture = picture.NextSibling;
		}
		
	}
	
#endif
	
}

struct PictureInfo {

	public string Location;
	public string Title;
	public string Date;
	public string Keywords;
	public string Comments;
	public long   Index;

	public PictureInfo (string location, string title, string date, string keys, string comments, long index)
	{
		Location = location;
		Title = title;
		Date = date;
		Keywords = keys;
		Comments = comments;
		Index = index;
	}
}
