/*
 * SimpleSearchIconListAdapter.cs
 *
 * Author(s): Vladimir Vukicevic <vladimir@pobox.com>
 *
 * Copyright (C) 2002  Vladimir Vukicevic
 */

using System;
using System.Collections;

using GLib;
using Gtk;
using GtkSharp;
using Gdk;

public class SimpleSearchIconListAdapter : IIconListAdapter
{
	IImageRepository repo;
	ISearchableRepository keyrepo;
	string[] keywords;
	string[] image_ids;
	IconList icon_list;
	Hashtable image_id_to_index;
	bool valid;

	public SimpleSearchIconListAdapter (IImageRepository repo_in, string[] keywords_in)
	{
		repo = repo_in;
		keywords = keywords_in;

		image_id_to_index = new Hashtable ();

		// ask the repo to do the actual search for us
		keyrepo = repo_in as ISearchableRepository;
		if (keyrepo == null) {
			valid = false;
			return;
		}

		image_ids = keyrepo.FindImagesByKeyword (keywords);

		for (int i = 0; i < image_ids.Length; i++) {
			repo.GetImage(image_ids[i]).OnThumbnailChange += new ImageItem.ThumbnailChangeHandler (ImageThumbnailChanged);
			image_id_to_index.Add (image_ids[i], i);
		}

		valid = true;
	}

	public string[] Keywords {
		get {
			return keywords;
		}
	}

	// IIconListAdapter methods
	public int Count {
		get {
			if (!valid)
				return 0;
			return image_ids.Length;
		}
	}

	public Pixbuf this[int index] {
		get {
			if (!valid)
				return null;
			return repo.GetImage (image_ids[index]).ImageThumbnail;
		}
	}

	public IconList IconList {
		get {
			return icon_list;
		}
		set {
			icon_list = value;
			if (icon_list != null)
				icon_list.SetPreviewSize (160, 160);
		}
	}

	public string GetFullFilename (int index)
	{
		return repo.GetImage (image_ids[index]).FullFilename;
	}

	public void DeleteItem (int index)
	{
		// not implemented
	}

	public string GetImageID (int index)
	{
		return image_ids[index];
	}

	public IImageRepository Repo
	{
		get {
			return repo;
		}
	}

	void ImageThumbnailChanged (ImageItem iitem, EventArgs unused)
	{
		int index = (int) image_id_to_index[iitem.ImageID];

		if (icon_list != null)
			icon_list.RedrawItem (index);
	}
}
