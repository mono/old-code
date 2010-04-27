/*
 * DbImageCollection.cs
 *
 * Author(s): Vladimir Vukicevic <vladimir@pobox.com>
 *
 * Copyright (C) 2002  Vladimir Vukicevic
 */

using System;
using System.Data;
using System.Collections;
using System.IO;
using System.Threading;

public class DbImageCollection : IImageCollection {
	private DbImageRepository repo;
	private IDbConnection db_conn;
	private ArrayList id_array;

	private int freeze_count;
	private bool updates_pending;

	private int c_id;
	private string c_name;

	internal DbImageCollection (IDbConnection dbc,
				    int _c_id,
				    DbImageRepository _repo)
	{
		db_conn = dbc;
		id_array = new ArrayList ();
		c_id = _c_id;
		repo = _repo;
		freeze_count = 0;
		updates_pending = false;

		this.UpdateCollectionInfo ();
		this.UpdateImageIDs ();
	}

	~DbImageCollection ()
	{
	}

	// IImageCollection
	public IImageRepository Repo
	{
		get {
			return repo;
		}
	}

	// IImageCollection
	public int Count
	{
		get {
			return id_array.Count;
		}
	}

	// IImageCollection
	public string Name
	{
		get { 
			return c_name;
		}
		set {
			if (c_id != 0) {
				IDbCommand cmd = db_conn.CreateCommand ();
				cmd.CommandText = DbProvider.SqlHelper.sqlDbSetCollectionName (c_id, value);
				cmd.ExecuteNonQuery ();

				c_name = value;

				if (OnCollectionChange != null) {
					if (freeze_count == 0) {
						OnCollectionChange (this, EventArgs.Empty);
					} else {
						updates_pending = true;
					}
				}
			}
		}
	}

	// IImageCollection
	public string Description
	{
		get {
			return "Description not available";
		}
		set {
		}
	}

	// IImageCollection
	string IImageCollection.ID
	{
		get {
			return c_id.ToString();
		}
	}

	// IImageCollection
	public string[] ImageIDs
	{
		get {
			string[] iids = new string[id_array.Count];
			int i;
			for (i = 0; i < id_array.Count; i++) {
				iids[i] = id_array[i].ToString();
			}
			return iids;
		}
	}

	// IImageCollection
	public ImageItem this[string imageid]
	{
		get {
			return this[Convert.ToInt32 (imageid)];
		}
	}

	public ImageItem this[int iid]
	{
		get {
			return repo.GetImage (iid);
		}
	}

	// IImageCollection
	public void AddItem (ImageItem image)
	{
		if (image.Repo != repo) {
			throw new InvalidOperationException ();
		}

		AddItem (Convert.ToInt32 (image.ImageID));
	}

	// IImageCollection
	public void AddItem (string imageid)
	{
		AddItem (Convert.ToInt32 (imageid));
	}

	public void AddItem (int imageid)
	{
		if (c_id != 0 && !id_array.Contains (imageid)) {
			IDbCommand cmd = db_conn.CreateCommand ();
			cmd.CommandText = DbProvider.SqlHelper.sqlDbAddImageToCollection (c_id, imageid);
			cmd.ExecuteNonQuery ();

			id_array.Add (imageid);

			if (OnCollectionChange != null) {
				if (freeze_count == 0) {
					OnCollectionChange (this, EventArgs.Empty);
				} else {
					updates_pending = true;
				}
			}
		}
	}

	// IImageCollection
	public void DeleteItem (ImageItem image)
	{
		if (image.Repo != repo) {
			throw new InvalidOperationException ();
		}

		this.DeleteImage (Convert.ToInt32 (image.ImageID));
	}

	// IImageColection
	public void DeleteItem (string imageid)
	{
		this.DeleteImage (Convert.ToInt32 (imageid));
	}

	public void DeleteImage (int i_id)
	{
		if (c_id != 0) {
			id_array.Remove (i_id);

			IDbCommand cmd = db_conn.CreateCommand ();
			cmd.CommandText = DbProvider.SqlHelper.sqlDbDeleteImageFromCollection (c_id, i_id);
			cmd.ExecuteNonQuery ();

			if (OnCollectionChange != null) {
				if (freeze_count == 0) {
					OnCollectionChange (this, EventArgs.Empty);
				} else {
					updates_pending = true;
				}
			}
		}
	}

	// IImageColection
	public void FreezeUpdates ()
	{
		Interlocked.Increment (ref freeze_count);
	}

	// IImageColection
	public void ThawUpdates ()
	{
		if (freeze_count > 0) {
			Interlocked.Decrement (ref freeze_count);
		}

		if (freeze_count == 0 && updates_pending) {
			updates_pending = false;
			if (OnCollectionChange != null) {
				OnCollectionChange (this, EventArgs.Empty);
			}
		}
	}


	//
	// private methods
	//

	void UpdateCollectionInfo ()
	{
		if (c_id != 0) {
			IDbCommand cmd = db_conn.CreateCommand ();
			cmd.CommandText = DbProvider.SqlHelper.sqlDbGetCollectionInfo (c_id);
			IDataReader r = cmd.ExecuteReader ();

			if (r == null || !r.Read ()) {
				throw new InvalidOperationException ();
			}

			c_name = (string) r.GetValue (1);
			r.Close ();
		} else {
			c_name = "All Images";
		}
	}

	void UpdateImageIDs ()
	{
		IDbCommand cmd = db_conn.CreateCommand ();
		if (c_id != 0) {
			cmd.CommandText = DbProvider.SqlHelper.sqlDbGetImageIdsInCollection (c_id);
		} else {
			cmd.CommandText = DbProvider.SqlHelper.sqlDbGetImageIds ();
		}
		IDataReader r = cmd.ExecuteReader ();

		id_array.Clear ();
		if (r == null)
			return;

		while (r.Read ()) {
			object o = r.GetValue (0);
			int i_id;
			if (DbProvider.DbUsesStrings (r, 0))
				i_id = Convert.ToInt32 ((string) o);
			else
				i_id = (int) o;

//            Console.WriteLine ("Collection " + c_id + " Adding i_id: " + i_id);
			id_array.Add (i_id);
		}

		r.Close ();
	}

	// events
	public event CollectionChangeHandler OnCollectionChange;

}

