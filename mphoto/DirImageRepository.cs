/*
 * DirImageRepository.cs
 *
 * Author(s): Vladimir Vukicevic <vladimir@pobox.com>
 *
 * Copyright (C) 2002  Vladimir Vukicevic
 */

using System;
using System.IO;
using System.Diagnostics;
using System.Collections;

public class DirImageRepository : IImageRepository, ISearchableRepository {
	DirImageCollection [] directories;
	Thumbnailer thumbnailer;

	Hashtable keywords = new Hashtable ();
	Hashtable i_hash = new Hashtable ();
	
	int num_images;
	
	public DirImageRepository (string[] dirnames_in)
	{
		num_images = 0;
		
		ArrayList dirlist = new ArrayList ();
		for (int i = 0; i < dirnames_in.Length; i++) {
			string name = dirnames_in [i].TrimEnd (Util.DirSep);

			if (Directory.Exists (name))
				dirlist.Add (name);
		}
		directories = new DirImageCollection [dirlist.Count];
		for (int i = 0; i < directories.Length; i++){
			directories [i] = new DirImageCollection (this, i, (string) dirlist [i]);
			num_images += directories [i].Count;

			foreach (string s in directories [i].Keywords.Keys)
				keywords [s] = true;
		}
		
#if NO_MULTITHREAD
		thumbnailer = new Thumbnailer ();
#else
		thumbnailer = new Thumbnailer ();
		thumbnailer.OnThumbnailFinished += new Thumbnailer.OnThumbnailFinishedHandler (ThumbnailsAvailable);
#endif
	}

	public int CountImages ()
	{
		return num_images;
	}

	public string[] GetImageIDs ()
	{
		string[] iids = new string[num_images];
		int pos = 0;
		
		for (int i = 0; i < directories.Length; i++)
			i = directories [i].PopulateNames (iids, pos);
		return iids;
	}

	void LookupFromID (string id, out DirImageCollection image_dir, out string name)
	{
		int p = id.LastIndexOf ("/");
		string d = id.Substring (0, p);
		name = id.Substring (p + 1);

		for (int i = 0; i < directories.Length; i++){
			if (d == directories [i].Path){
				image_dir = directories [i];
				return;
			}
		}
		image_dir = null;
	}
	
	public ImageItem GetImage (string imageid)
	{
		if (!i_hash.Contains (imageid)) {
			FileInfo finfo = new FileInfo (imageid);

			ImageItem.ImageInfo iinfo = new ImageItem.ImageInfo ();
			iinfo.repo = this;
			iinfo.imageid = imageid;
			iinfo.filename = finfo.Name;
			iinfo.dirname = finfo.DirectoryName;
			iinfo.width = 0;
			iinfo.height = 0;
			iinfo.filesize = (int) finfo.Length;

			i_hash.Add (imageid, new ImageItem (iinfo));
		}

		return (ImageItem) i_hash[imageid];
	}

	public ImageItem AddImage (ImageItem item)
	{
		throw new InvalidOperationException ();
	}

	public void DeleteImage (ImageItem item)
	{
		throw new InvalidOperationException ();
	}

	public void DeleteImage (string imageid)
	{
		throw new InvalidOperationException ();
	}

	public int CountCollections ()
	{
		return directories.Length;
	}

	public string[] GetCollectionIDs ()
	{
		string[] ids = new string [directories.Length];
		for (int i = 0; i < directories.Length; i++)
			ids [i] = i.ToString();
		return ids;
	}

	public IImageCollection GetCollection (string collid)
	{
		int index = Convert.ToInt32 (collid);

		return directories [index];
	}

	public IImageCollection CreateCollection ()
	{
		throw new InvalidOperationException ();
	}

	public void DeleteCollection (IImageCollection theColl) 
	{
		throw new InvalidOperationException ();
	}

	public void DeleteCollection (string collid)
	{
		throw new  InvalidOperationException ();
	}

	public void DeleteThumbnails ()
	{
	}

	public ImageItem.ThumbnailInfo GetThumbnailForImage (ImageItem item)
	{
		string imageid = item.ImageID;
		string thumb_file;
		string lc_filename = item.Filename.ToLower ();
		if (lc_filename.EndsWith ("jpg") || lc_filename.EndsWith ("png") || lc_filename.EndsWith ("jpeg"))
			thumb_file = item.Dirname + Util.DirSep + ".thumbnails" + Util.DirSep + item.Filename;
		else
			thumb_file = item.Dirname + Util.DirSep + ".thumbnails" + Util.DirSep + item.Filename + ".jpg";

		FileInfo thumbfi = new FileInfo (thumb_file);
		if (!thumbfi.Exists) {
			DirectoryInfo thumb_dinfo = new DirectoryInfo (item.Dirname + Util.DirSep + ".thumbnails");
			Console.WriteLine (item.Dirname + Util.DirSep + ".thumbnails");
			thumb_dinfo.Create ();

			// we need to create a thumbnail
			Thumbnailer.ThumbnailerInfo tinfo = new Thumbnailer.ThumbnailerInfo ();
			tinfo.id = imageid;
			tinfo.source = item.Dirname + Util.DirSep + item.Filename;
			tinfo.target = thumb_file;

#if NO_MULTITHREAD
			thumbnailer.MakeThumbnail (tinfo);
			// fall through
#else
			thumbnailer.Thumbnail (tinfo);
			return null;
#endif
		}

		ImageItem.ThumbnailInfo ti = new ImageItem.ThumbnailInfo ();
		ti.filename = thumb_file;
		return ti;
	}

	public void ThumbnailsAvailable (Thumbnailer t, Thumbnailer.ThumbnailFinishedEventArgs tf)
	{
		string imageid = tf.tinfo.id;
		if (tf.tinfo.thumbnail != null) {
			GetImage (imageid).ImageThumbnail = tf.tinfo.thumbnail;
		} else {
			if (tf.tinfo.status == ThumbnailStatus.OK) {
				GetImage (imageid).RefreshThumbnail ();
			}
		}
	}

	public event RepositoryChangeHandler OnRepositoryChange;

	//
	// ISearchableRepository
	//
	bool ISearchableRepository.IsKeyword (string kw)
	{
		return keywords.Contains (kw);
	}
	
	void ISearchableRepository.AddKeyword (string kw)
	{
		keywords [kw] = true;
	}
	
	string [] ISearchableRepository.FindImagesByKeyword (string [] keywords)
	{
		ArrayList result = new ArrayList ();

		for (int i = 0; i < directories.Length; i++){
			ArrayList matches = directories [i].FindImagesByKeyword (keywords);
			
			result.AddRange (matches);
		}

		string [] string_res = new string [result.Count];
		result.CopyTo (string_res);
		return string_res;
	}
	
	void ISearchableRepository.AddImageKeyword (string imageid, string keyword)
	{
		DirImageCollection col;
		string name;
		
		LookupFromID (imageid, out col, out name);
		col.AddFileKeyword (name, keyword);
	}
	
	void ISearchableRepository.RemoveImageKeyword (string imageid, string keyword)
	{
	}
	
	string[] ISearchableRepository.GetImageKeywords (string imageid)
	{
		DirImageCollection col;
		string name;
		
		LookupFromID (imageid, out col, out name);
		if (col == null){
			Console.WriteLine ("Could not find image: {0}", imageid);
		}
		
		return col.GetFileKeywords (name);
	}
	
	void ISearchableRepository.SetImageKeywords (string imageid, string [] keywords)
	{
		DirImageCollection col;
		string name;
		
		LookupFromID (imageid, out col, out name);
		col.SetFileKeywords (name, keywords);
	}

	string [] ISearchableRepository.Keywords {
		get {
			ICollection kcol = keywords.Keys;
			
			string [] res = new string [kcol.Count];

			int i = 0;
			foreach (string s in kcol){
				res [i++] = s;
			}

			return res;
		}
	}
}
