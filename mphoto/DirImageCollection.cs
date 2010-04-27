/*
 * DirImageRepository.cs
 *
 * Author(s): Vladimir Vukicevic <vladimir@pobox.com>
 *            Miguel de Icaza (miguel@ximian.com)
 *
 * Copyright (C) 2002  Vladimir Vukicevic
 * Copyright (C) 2003  Miguel de Icaza
 *
 * The keyword implementation is not very efficient.  It should probably
 * use per-file hash tables instead of goig back and forth between representations
 *
 */

using System;
using System.IO;
using System.Collections;
using System.Xml;

public class DirImageCollection : IImageCollection {
	XmlDocument metadata;
	string metafile;
	string name;
	string [] images;
	int image_count;
	DirImageRepository repo;
	int idx;
	Hashtable keywords;

	public DirImageCollection (DirImageRepository repo, int idx, string name)
	{
		this.repo = repo;
		this.idx = idx;
		this.name = name;
		string [] files = Directory.GetFiles (name);

		foreach (string file in files){
			string lf = file.ToLower ();
			if (lf.EndsWith (".jpg") ||
			    lf.EndsWith (".jpeg") ||
			    lf.EndsWith (".png") ||
			    lf.EndsWith (".gif") ||
			    lf.EndsWith (".tif") ||
			    lf.EndsWith (".tiff"))
				image_count++;
		}

		images = new string [image_count];

		int i = 0;
		foreach (string file in files){
			string lf = file.ToLower ();
			if (lf.EndsWith (".jpg") ||
			    lf.EndsWith (".jpeg") ||
			    lf.EndsWith (".png") ||
			    lf.EndsWith (".gif") ||
			    lf.EndsWith (".tif") ||
			    lf.EndsWith (".tiff"))
				images [i++] = file;
		}

		metadata = new XmlDocument ();
		metafile = name + "/metadata.xml";
		if (File.Exists (metafile)){
			metadata.Load (metafile);
		} else {
			metadata.LoadXml ("<?xml version=\"1.0\"?><images></images>");
		}

		keywords = new Hashtable ();
		XmlNodeList keyword_nodes = metadata.SelectNodes ("/images/image/keywords");
		foreach (XmlNode n in keyword_nodes){
			string [] keyword_list = KeywordsFromNode (n);

			foreach (string keyword in keyword_list)
				keywords [keyword] = true;
		}
	}

	string [] KeywordsFromNode (XmlNode n)
	{
		string keyword_text = n.InnerText;
		if (keyword_text == null || keyword_text == "")
			return new string [0];

		string x = keyword_text.Substring (1, keyword_text.Length-2);
		return x.Split (',');
	}

	public string [] GetFileKeywords (string filename)
	{
		Console.WriteLine ("Get keywords for: {0}", filename);
		XmlNode n = metadata.SelectSingleNode (String.Format ("/images/image[@name=\"{0}\"]/keywords", filename));

		if (n == null){
			Console.WriteLine ("Nothing!", filename);
			return new string [0];
		}
		return KeywordsFromNode (n);
	}

	XmlNode GetFileNode (string filename)
	{
		XmlNode n = metadata.SelectSingleNode (String.Format ("/images/image[@name=\"{0}\"]", filename));
		if (n != null)
			return n;

		XmlNode images_node = metadata.SelectSingleNode ("/images");
		XmlElement image_node = metadata.CreateElement ("image");
		image_node.SetAttribute ("name", filename);
		images_node.AppendChild (image_node);

		return image_node;
	}
	
	public void AddFileKeyword (string filename, string keywords)
	{
		string [] current = GetFileKeywords (filename);
		string [] newk = new string [current.Length + 1];

		current.CopyTo (newk, 0);
		newk [current.Length] = keywords;

		SetFileKeywords (filename, newk);
	}
	
	public void SetFileKeywords (string filename, string [] new_keywords)
	{
		XmlNode image_node = GetFileNode (filename);

		XmlNode keywords_node = image_node.SelectSingleNode ("keywords");
		if (keywords_node == null){
			keywords_node = metadata.CreateElement ("keywords");
			image_node.AppendChild (keywords_node);
		}
		keywords_node.RemoveAll ();

		//
		// We insert ",KEYWORDS-SEPARATED-BY-COMMA," in the list, so we can
		// search for KEYWORD by doing a string search for ",KEYWORD,"
		
		XmlText text_node = metadata.CreateTextNode ("," + String.Join (",", new_keywords) + ",");
		keywords_node.AppendChild (text_node);

		keywords.Clear ();
		foreach (string s in keywords)
			keywords [s] = true;

		metadata.Save (metafile);
	}

	public ArrayList FindImagesByKeyword (string [] search)
	{
		XmlNodeList keyword_nodes = metadata.SelectNodes ("/images/image/keywords");
		string [] texts = new string [keyword_nodes.Count];
		ArrayList result = null;
		
		for (int i = 0; i < keyword_nodes.Count; i++)
			texts [i] = keyword_nodes [i].InnerText;
		
		for (int i = 0; i < search.Length; i++){
			foreach (string s in texts){
				if (s.IndexOf ("," + search [i] + ",") != -1){
					Console.WriteLine ("Found a match: " + keyword_nodes [i].ParentNode ["name"].InnerText);
					if (result == null)
						result = new ArrayList ();
					result.Add (keyword_nodes [i].ParentNode ["name"].InnerText);
				}
			}
		}

		return result;
	}
	
	public Hashtable Keywords {
		get {
			return keywords;
		}
	}
	
	public int PopulateNames (string [] s, int start)
	{
		for (int i = 0; i < images.Length; i++)
			s [start+i] = Name + "/" + images [i];
		return start + images.Length;
	}

	public string ID
	{
		get {
			return idx.ToString ();
		}
	}

	public string Name {
		get {
			int p = name.LastIndexOf ("/");
			if (p == -1)
				return name;
			else
				return name.Substring (p + 1);
		}
		set { }
	}

	public string Path {
		get {
			return name;
		}
	}

	public IImageRepository Repo
	{
		get {
			return repo;
		}
	}

	public int Count
	{
		get {
			return image_count;
		}
	}

	public string Description
	{
		get {
			return name;
		}
		set { }
	}

	public string[] ImageIDs
	{
		get {
			return (string []) images.Clone ();
		}
	}

	public ImageItem this[string index]
	{
		get {
			return repo.GetImage (index);
		}
	}
	
	public void FreezeUpdates ()
	{
	}
	
	public void ThawUpdates ()
	{
	}
	
	public void AddItem (ImageItem image)
	{
		throw new InvalidOperationException ();
	}
	
	public void AddItem (string imageid)
	{
		throw new InvalidOperationException ();
	}
	
	public void DeleteItem (ImageItem image)
	{
		throw new InvalidOperationException ();
	}
	
	public void DeleteItem (string imageid)
	{
		throw new InvalidOperationException ();
	}

	public event CollectionChangeHandler OnCollectionChange;
}
