/*
 * CollectionIconListAdapter.cs
 *
 * Author(s): Vladimir Vukicevic <vladimir@pobox.com>
 *
 * Copyright (C) 2002  Vladimir Vukicevic
 */

using System;
using System.Collections;
using Gdk;

public class CollectionIconListAdapter : IIconListAdapter {
	IImageRepository repo;
	IImageCollection collection;
	ArrayList image_ids;
	Hashtable image_id_to_index;
	IconList icon_list;

	public CollectionIconListAdapter (IImageCollection coll)
	{
		icon_list = null;
		collection = coll;
		image_ids = new ArrayList ();
		image_id_to_index = new Hashtable ();

		if (coll != null) {
			repo = coll.Repo;

			UpdateFromCollection ();
			coll.OnCollectionChange += new CollectionChangeHandler (CollectionChanged);
		}
	}

	~CollectionIconListAdapter ()
	{
	}

	public int Count
	{
		get {
			return image_ids.Count;
		}
	}

	public Pixbuf this[int index]
	{
		get {
			if (collection == null)
				return null;
			return collection[(string) image_ids[index]].ImageThumbnail;
		}
	}

	public IconList IconList
	{
		get {
			return icon_list;
		}
		set {
			icon_list = value;
			if (icon_list != null)
				icon_list.SetPreviewSize (160, 160);
		}
	}

	public IImageCollection Collection
	{
		get {
			return collection;
		}
	}

	public IImageRepository Repo
	{
		get {
			return repo;
		}
	}

	public string GetImageID (int index)
	{
		if (collection == null)
			return null;
		return (string) image_ids[index];
	}

	public string GetFullFilename (int index)
	{
		if (collection == null)
			return null;
		return collection[(string) image_ids[index]].FullFilename;
	}

	public void DeleteItem (int index)
	{
		collection.DeleteItem ((string) image_ids[index]);
	}

	void ImageThumbnailChanged (ImageItem iitem, EventArgs unused)
	{
		int index = (int) image_id_to_index[iitem.ImageID];

		if (icon_list != null)
			icon_list.RedrawItem (index);
	}

	void CollectionChanged (IImageCollection coll, EventArgs unused)
	{
		UpdateFromCollection ();
	}

	void UpdateFromCollection ()
	{
		if (image_ids.Count != 0 && collection != null) {
			foreach (string imageid in image_ids) {
				collection[imageid].OnThumbnailChange -= new ImageItem.ThumbnailChangeHandler (ImageThumbnailChanged);
			}
		}

		image_ids = new ArrayList (collection.ImageIDs);
		UpdateIndexHash ();

		// now put back the image event handlers for the new set of images
		foreach (string imageid in image_ids) {
			collection[imageid].OnThumbnailChange += new ImageItem.ThumbnailChangeHandler (ImageThumbnailChanged);
		}

		if (icon_list != null)
			icon_list.Refresh ();
	}

	void UpdateIndexHash ()
	{
		image_id_to_index.Clear ();

		for (int i = 0; i < image_ids.Count; i++) {
			image_id_to_index.Add (image_ids[i], i);
		}
	}
}

