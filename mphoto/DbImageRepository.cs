/*
 * DbImageRepository.cs
 *
 * Author(s): Vladimir Vukicevic <vladimir@pobox.com>
 *
 * Copyright (C) 2002  Vladimir Vukicevic
 */
/*
 * Implements an ImageRepository based on a database accessed through ADO.NET
 */

using System;
using System.Data;
using System.Collections;
using System.IO;
using System.Threading;


public class DbImageRepository : IImageRepository, ISearchableRepository
{
	private IDbConnection db_conn;
	private ArrayList c_id_array;
	private ArrayList i_id_array;
	private Thumbnailer thumbnailer;
	private Hashtable collections_hash;
	private Hashtable images_hash;
	private Hashtable id_to_keyword_hash;
	private Hashtable keywords_hash;

	public DbImageRepository (string dbstring)
	{
		db_conn = DbProvider.GetProvider (dbstring);
		c_id_array = new ArrayList ();
		i_id_array = new ArrayList ();
		collections_hash = new Hashtable ();
		images_hash = new Hashtable ();
		id_to_keyword_hash = new Hashtable ();
		keywords_hash = new Hashtable ();

#if NO_MULTITHREAD
		thumbnailer = new Thumbnailer ();
#else
		thumbnailer = new Thumbnailer ();
		thumbnailer.OnThumbnailFinished += new Thumbnailer.OnThumbnailFinishedHandler (ThumbnailsAvailable);
#endif

		this.UpdateCollections ();
		this.UpdateImages ();
		this.UpdateKeywords ();
	}

	// IImageRepository
	public int CountImages ()
	{
		return i_id_array.Count;
	}

	// IImageRepository
	public string[] GetImageIDs ()
	{
		string[] iids = new string[i_id_array.Count];
		int i;
		for (i = 0; i < i_id_array.Count; i++) {
			iids[i] = i_id_array[i].ToString();
		}
		return iids;
	}

	// IImageRepository
	public ImageItem GetImage (string imageid)
	{
		return GetImage (Convert.ToInt32 (imageid));
	}

	public ImageItem GetImage (int i_id)
	{
		if (!images_hash.Contains (i_id)) {
			ImageItem.ImageInfo iinfo;

			IDbCommand cmd = db_conn.CreateCommand ();
			cmd.CommandText = DbProvider.SqlHelper.sqlDbGetImageInfo (i_id);
			IDataReader r = cmd.ExecuteReader ();

			if (r == null)
				throw new InvalidOperationException ();

			r.Read ();

			iinfo = new ImageItem.ImageInfo ();
			iinfo.repo = this;
			if (DbProvider.DbUsesStrings (r, 0)) {
				iinfo.imageid = (string) r.GetValue (0);
				iinfo.filename = (string) r.GetValue (1);
				iinfo.dirname = (string) r.GetValue (2);
				iinfo.width = Convert.ToInt32 ((string) r.GetValue (3));
				iinfo.height = Convert.ToInt32 ((string) r.GetValue (4));
				iinfo.caption = (string) r.GetValue (5);
				// iinfo.public = (bool) r.GetValue (6);
				iinfo.filesize = Convert.ToInt32 ((string) r.GetValue (7));
			} else {
				iinfo.imageid = (string) r.GetValue (0);
				iinfo.filename = (string) r.GetValue (1);
				iinfo.dirname = (string) r.GetValue (2);
				iinfo.width = (int) r.GetValue (3);
				iinfo.height = (int) r.GetValue (4);
				iinfo.caption = (string) r.GetValue (5);
				// iinfo.public = (bool) r.GetValue (6);
				iinfo.filesize = (int) r.GetValue (7);
			}

			r.Close ();
        
			images_hash.Add (i_id, new ImageItem (iinfo));
		}

		return (ImageItem) images_hash[i_id];
	}

	// IImageRepository
	public ImageItem AddImage (ImageItem item)
	{
		// This may be not necessary
		// we may want to allow adding an image that already has a repo and.. copy it?
		if (item.Repo != null) {
			throw new InvalidOperationException ();
		}

		IDbCommand cmd = db_conn.CreateCommand ();
		cmd.CommandText = DbProvider.SqlHelper.sqlDbAddImage (item.Filename,
								      item.Dirname,
								      item.Width,
								      item.Height,
								      item.Caption,
								      item.Filesize);
		cmd.ExecuteNonQuery ();

		int new_iid;

		// FIXME -- libgda doesn't export sqlite_last_insert_rowid, and there's no way to SELECT it
		// so we do some magic to figure out the id
		cmd.CommandText =
		"SELECT i_id FROM mp_images WHERE i_filename = '" + item.Filename + "' AND i_dirname = '" + item.Dirname + "'";
//        Console.WriteLine ("SQL: " + cmd.CommandText);
		IDataReader r = cmd.ExecuteReader ();
		if (r == null || !r.Read ()) {
			throw new InvalidOperationException ();
		}

		if (DbProvider.DbUsesStrings (r, 0)) {
			new_iid = Convert.ToInt32 ((string) r.GetValue (0));
		} else {
			new_iid = (int) r.GetValue (0);
		}

		r.Close ();

		i_id_array.Add (new_iid);

		item.thisInfo.imageid = new_iid.ToString();
		item.thisInfo.repo = this;

		if (OnRepositoryChange != null) {
			RepositoryChangeEventArgs rcea = new RepositoryChangeEventArgs
			(RepositoryChangeEventArgs.RepositoryChangeType.ImageAdded, new_iid.ToString ());
			OnRepositoryChange (this, rcea);
		}
		return item;
	}

	// IImageRepository
	public void DeleteImage (ImageItem item)
	{
		if (item.Repo != this) {
			throw new InvalidOperationException ();
		}

		DeleteImage (Convert.ToInt32 (item.ImageID));
	}

	// IImageRepository
	public void DeleteImage (string imageid)
	{
		DeleteImage (Convert.ToInt32 (imageid));
	}
    
	public void DeleteImage (int i_id)
	{
		// first figure out which collections this belongs in
		IDbCommand cmd = db_conn.CreateCommand ();

		ArrayList the_colls = new ArrayList ();

		cmd.CommandText = DbProvider.SqlHelper.sqlDbGetImageCollections (i_id);
		IDataReader r = cmd.ExecuteReader ();
		while (r.Read ()) {
			if (DbProvider.DbUsesStrings (r, 0)) {
				the_colls.Add (Convert.ToInt32 ((string) r.GetValue (0)));
			} else {
				the_colls.Add ((int) r.GetValue (0));
			}
		}
		r.Close ();

		foreach (int c_id in the_colls) {
			this.GetCollection (c_id).DeleteImage (i_id);
		}

		cmd.CommandText = DbProvider.SqlHelper.sqlDbDeleteImage (i_id);
		cmd.ExecuteNonQuery ();
	}

	// IImageRepository
	public int CountCollections ()
	{
		return c_id_array.Count;
	}

	// IImageRepository
	public string[] GetCollectionIDs ()
	{
		string[] cids = new string[c_id_array.Count];
		int i;
		for (i = 0; i < c_id_array.Count; i++) {
			cids[i] = c_id_array[i].ToString();
		}
		return cids;
	}

	// IImageRepository
	IImageCollection IImageRepository.GetCollection (string collid)
	{
		return GetCollection (Convert.ToInt32 (collid));
	}

	public DbImageCollection GetCollection (int c_id)
	{
		Console.WriteLine ("GetCollection c_id: " + c_id);
		if (!collections_hash.Contains (c_id)) {
			DateTime d1 = DateTime.Now;
			collections_hash.Add (c_id, new DbImageCollection (db_conn, c_id, this));
			DateTime d2 = DateTime.Now;
			Console.WriteLine ("Add took: " + (d2 - d1) + " ticks");
		}

		return (DbImageCollection) collections_hash[c_id];
	}

	// IImageRepository
	IImageCollection IImageRepository.CreateCollection ()
	{
		return CreateCollection ();
	}

	public DbImageCollection CreateCollection ()
	{
		IDbCommand cmd = db_conn.CreateCommand ();
		cmd.CommandText =
		DbProvider.SqlHelper.sqlDbNewCollection ("Collection " + c_id_array.Count);
		cmd.ExecuteNonQuery ();

		// sucky sucky
		cmd.CommandText =
		DbProvider.SqlHelper.sqlDbGetCollectionByName ("Collection " + c_id_array.Count);
		IDataReader r = cmd.ExecuteReader ();
		int new_cid = -1;
		while (r.Read ()) {
			if (DbProvider.DbUsesStrings (r, 0)) {
				new_cid = Convert.ToInt32 ((string) r.GetValue (0));
			} else {
				new_cid = (int) r.GetValue (0);
			}
		}

		if (new_cid == -1) {
			throw new ApplicationException ("Couldn't find newly created collection!");
		}

		c_id_array.Add (new_cid);
		if (OnRepositoryChange != null) {
			RepositoryChangeEventArgs rcea = new RepositoryChangeEventArgs
			(RepositoryChangeEventArgs.RepositoryChangeType.CollectionAdded, new_cid.ToString ());
			OnRepositoryChange (this, rcea);
		}
		return GetCollection (new_cid);
	}

	// IImageRepository
	public void DeleteCollection (IImageCollection theColl)
	{
		if (theColl.Repo != this) {
			throw new InvalidOperationException ();
		}

		DeleteCollection (Convert.ToInt32 (theColl.ID));
	}

	public void DeleteCollection (string collid)
	{
		DeleteCollection (Convert.ToInt32 (collid));
	}

	public void DeleteCollection (int c_id)
	{
		// FIXME
		return;
	}


	// IImageRepository
	public void DeleteThumbnails ()
	{
		return;
	}

	// IImageRepository
	public ImageItem.ThumbnailInfo GetThumbnailForImage (ImageItem item)
	{
		if (item.Repo != this) {
			throw new InvalidOperationException ();
		}

//        Console.WriteLine ("GetThumbnailForImage: " + item.ImageID);

		int i_id = Convert.ToInt32 (item.ImageID);

		IDbCommand cmd = db_conn.CreateCommand ();
		cmd.CommandText = 
		DbProvider.SqlHelper.sqlDbGetThumbnail (i_id);

		string thumb_file;

		object o = cmd.ExecuteScalar ();
		if (o == null) {
			// no thumbnail in database, check if one exists in the filesystem
			string thumb_dir = item.Dirname + Util.DirSep + ".thumbnails";
			if (item.Filename.ToLower().EndsWith (".jpg")) {
				thumb_file = thumb_dir + Util.DirSep + item.Filename;
			} else {
				thumb_file = thumb_dir + Util.DirSep + item.Filename + ".jpg";
			}

			FileInfo thumb_finfo = new FileInfo (thumb_file);
			if (!thumb_finfo.Exists) {
				DirectoryInfo thumb_dinfo = new DirectoryInfo (thumb_dir);
				thumb_dinfo.Create ();

				Thumbnailer.ThumbnailerInfo tinfo = new Thumbnailer.ThumbnailerInfo ();
				tinfo.id = item.ImageID;
				tinfo.source = item.Dirname + Util.DirSep + item.Filename;
				tinfo.target = thumb_file;

#if NO_MULTITHREAD
				thumbnailer.MakeThumbnail (tinfo);
				// fall through
#else
				thumbnailer.Thumbnail (tinfo);
				// thumbnailing in progress, return null
				return null;
#endif
			}

			// thumb file exists from something else, but wasn't present in our database
			// save it
			SaveThumbToDb (i_id, thumb_file);
		} else {
			thumb_file = (string) o;
		}

		ImageItem.ThumbnailInfo ti = new ImageItem.ThumbnailInfo ();
		ti.filename = thumb_file;

//        Console.WriteLine (" -- returning " + thumb_file);
		return ti;
	}

	// ISearchableRepository
	public string[] Keywords {
		get {
			string[] kws = new string[keywords_hash.Keys.Count];
			keywords_hash.Keys.CopyTo (kws, keywords_hash.Keys.Count);
			return kws;
		}
	}

	public bool IsKeyword (string kw)
	{
		return keywords_hash.ContainsKey (kw);
	}

	void ISearchableRepository.AddKeyword (string kw)
	{
		AddKeyword (kw);
	}

	public int AddKeyword (string kw)
	{
		if (!IsKeyword (kw)) {
			IDbCommand cmd = db_conn.CreateCommand ();
			cmd.CommandText = DbProvider.SqlHelper.sqlDbAddKeyword (kw);
			cmd.ExecuteNonQuery ();

			cmd.CommandText = DbProvider.SqlHelper.sqlDbGetKeyword (kw);
			int k_id = Convert.ToInt32 ((string) cmd.ExecuteScalar ());

			keywords_hash.Add (kw, k_id);
			id_to_keyword_hash.Add (k_id, kw);
			return k_id;
		} else {
			return (int) keywords_hash[kw];
		}
	}

	public void AddImageKeyword (string imageid, string keyword)
	{
		int i_id = Convert.ToInt32 (imageid);
		int k_id = AddKeyword (keyword);

		IDbCommand cmd = db_conn.CreateCommand ();
		cmd.CommandText = DbProvider.SqlHelper.sqlDbAddImageKeyword (i_id, k_id);
		cmd.ExecuteNonQuery ();

		if (OnRepositoryChange != null) {
			RepositoryChangeEventArgs rcea = new RepositoryChangeEventArgs
			(RepositoryChangeEventArgs.RepositoryChangeType.ImageChanged, imageid);
			OnRepositoryChange (this, rcea);
		}
	}

	public void RemoveImageKeyword (string imageid, string keyword)
	{
		int i_id = Convert.ToInt32 (imageid);
		int k_id = (int) keywords_hash [keyword];

		IDbCommand cmd = db_conn.CreateCommand ();
		cmd.CommandText = DbProvider.SqlHelper.sqlDbAddImageKeyword (i_id, k_id);
		cmd.ExecuteNonQuery ();

		if (OnRepositoryChange != null) {
			RepositoryChangeEventArgs rcea = new RepositoryChangeEventArgs
			(RepositoryChangeEventArgs.RepositoryChangeType.ImageChanged, imageid);
			OnRepositoryChange (this, rcea);
		}
	}

	public int[] FindImagesByKeyword (string[] keywords)
	{
		// just return null if we have no images
		if (CountImages() == 0)
			return new int[0];

		int[] k_ids = new int[keywords.Length];

		for (int i = 0; i < keywords.Length; i++) {
			if (!IsKeyword (keywords[i]))
				return new int [0];

			k_ids[i] = (int) keywords_hash[keywords[i]];
		}

		IDbCommand cmd = db_conn.CreateCommand ();
		cmd.CommandText = DbProvider.SqlHelper.sqlDbSearchImagesForKeywords (k_ids);
		IDataReader r = cmd.ExecuteReader ();

		ArrayList result = new ArrayList ();
		while (r.Read ()) {
			int i_id = Convert.ToInt32 ((string) r[0]);
			// sqlite doesn't understand unique
			if (!result.Contains (i_id))
				result.Add (i_id);
		}

		int[] result_ids = new int[result.Count];
		result.CopyTo (result_ids);
		return result_ids;
	}

	string[] ISearchableRepository.FindImagesByKeyword (string[] keywords)
	{
		int[] int_ids = FindImagesByKeyword (keywords);
		string[] string_ids = new string[int_ids.Length];

		for (int i = 0; i < int_ids.Length; i++) {
			string_ids[i] = int_ids[i].ToString ();
		}

		return string_ids;
	}

	public int[] GetImageKeywordIDs (int i_id)
	{
		IDbCommand cmd = db_conn.CreateCommand ();
		cmd.CommandText = DbProvider.SqlHelper.sqlDbGetImageKeywords (i_id);
		IDataReader r = cmd.ExecuteReader ();

		if (r == null)
			return new int[0];

		ArrayList ids_list = new ArrayList ();
		while (r.Read ()) {
			int k_id = Convert.ToInt32 ((string) r[0]);
//            Console.WriteLine ("K_ID: " + r[0]);
			if (!ids_list.Contains (k_id))
				ids_list.Add (k_id);
		}

		int[] out_ids = new int[ids_list.Count];
		ids_list.CopyTo (out_ids);
		return out_ids;
	}

	public string[] GetImageKeywords (string imageid)
	{
		int[] int_ids = GetImageKeywordIDs (Convert.ToInt32 (imageid));
		string[] string_keywords = new string[int_ids.Length];

		for (int i = 0; i < int_ids.Length; i++) {
//            Console.WriteLine ("K_STR[" + i + "]: " + (string) id_to_keyword_hash[int_ids[i]]);
			string_keywords[i] = (string) id_to_keyword_hash[int_ids[i]];
		}

		return string_keywords;
	}

	public void SetImageKeywords (string imageid, string[] keywords)
	{
		int i_id = Convert.ToInt32 (imageid);

		ArrayList k_ids = new ArrayList ();

		for (int i = 0; i < keywords.Length; i++) {
			AddKeyword (keywords[i]);
			int k_id = (int) keywords_hash[keywords[i]];
			if (!k_ids.Contains (k_id))
				k_ids.Add (k_id);
		}

		// wtb transactions..
		IDbCommand cmd = db_conn.CreateCommand ();
		cmd.CommandText = DbProvider.SqlHelper.sqlDbDeleteImageKeywords (i_id);
		cmd.ExecuteNonQuery ();

		foreach (int k_id in k_ids) {
			cmd.CommandText = DbProvider.SqlHelper.sqlDbAddImageKeyword (i_id, k_id);
			cmd.ExecuteNonQuery ();
		}

		if (OnRepositoryChange != null) {
			RepositoryChangeEventArgs rcea = new RepositoryChangeEventArgs
			(RepositoryChangeEventArgs.RepositoryChangeType.ImageChanged, imageid);
			OnRepositoryChange (this, rcea);
		}
	}

	// private methods

	void SaveThumbToDb (int i_id, string thumb_file)
	{
		IDbCommand cmd = db_conn.CreateCommand ();
		cmd.CommandText =
		DbProvider.SqlHelper.sqlDbDeleteThumbnail (i_id);
		cmd.ExecuteNonQuery ();

		cmd.CommandText = 
		DbProvider.SqlHelper.sqlDbSetThumbnailFilename (i_id, thumb_file);
		cmd.ExecuteNonQuery ();

//        Console.WriteLine ("SaveThumbToDb: " + thumb_file);
	}

	void ThumbnailsAvailable (Thumbnailer t, Thumbnailer.ThumbnailFinishedEventArgs tf)
	{
		int i_id = Convert.ToInt32 (tf.tinfo.id);

		if (tf.tinfo.status == ThumbnailStatus.OK) {
			// save the thumbnail info to the db
			// only if it was created and was OK
			SaveThumbToDb (i_id, tf.tinfo.target);
		}

		if (tf.tinfo.thumbnail != null) {
			GetImage (i_id).ImageThumbnail = tf.tinfo.thumbnail;
		} else {
			if (tf.tinfo.status == ThumbnailStatus.OK) {
				// tell the image item to refresh itself,
				// since we didn't have a pixbuf given to us
				GetImage (i_id).RefreshThumbnail ();
			}
		}
	}

	void UpdateCollections ()
	{
		IDbCommand cmd = db_conn.CreateCommand ();
		cmd.CommandText = DbProvider.SqlHelper.sqlDbGetCollectionIds ();
		IDataReader r = cmd.ExecuteReader ();

		c_id_array.Clear ();

		// there's always going to be a cid 0,
		// meaning all images
		c_id_array.Add (0);

		if (r == null)
			return;

		while (r.Read ()) {
			object o = r.GetValue (0);
			int c_id;
			if (DbProvider.DbUsesStrings (r, 0))
				c_id = Convert.ToInt32 ((string) o);
			else
				c_id = (int) o;

//            Console.WriteLine ("Discovered collection " + c_id);
			c_id_array.Add (c_id);
		}

		r.Close ();
	}

	void UpdateImages ()
	{
		IDbCommand cmd = db_conn.CreateCommand ();
		cmd.CommandText = DbProvider.SqlHelper.sqlDbGetImageIds ();
		IDataReader r = cmd.ExecuteReader ();

		i_id_array.Clear ();

		if (r == null)
			return;

		while (r.Read ()) {
			object o = r.GetValue (0);
			int i_id;
			if (DbProvider.DbUsesStrings (r, 0))
				i_id = Convert.ToInt32 ((string) o);
			else
				i_id = (int) o;

//            Console.WriteLine ("Discovered image " + i_id);
			i_id_array.Add (i_id);
		}

		r.Close ();
	}

	void UpdateKeywords ()
	{
		IDbCommand cmd = db_conn.CreateCommand ();
		cmd.CommandText = DbProvider.SqlHelper.sqlDbGetKeywords ();
		IDataReader r = cmd.ExecuteReader ();

		id_to_keyword_hash.Clear ();
		keywords_hash.Clear ();

		if (r == null)
			return;

		while (r.Read ()) {
			if (DbProvider.DbUsesStrings (r, 0)) {
				keywords_hash.Add ((string) r[1], Convert.ToInt32 ((string) r[0]));
				id_to_keyword_hash.Add (Convert.ToInt32 ((string) r[0]), (string) r[1]);
			} else {
				keywords_hash.Add ((string) r[1], (int) r[0]);
				id_to_keyword_hash.Add ((int) r[0], (string) r[1]);
			}
		}
	}

	public event RepositoryChangeHandler OnRepositoryChange;
}
