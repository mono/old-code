/*
 * IImageImporter.cs
 *
 * Author(s): Vladimir Vukicevic <vladimir@pobox.com>
 *
 * Copyright (C) 2002  Vladimir Vukicevic
 */

using Gdk;
using System;

public delegate void CollectionChangeHandler (IImageCollection coll, EventArgs unused);

public interface IImageCollection {
	IImageRepository Repo { get; }
	int Count { get; }
	string Name { get; set; }
	string Description { get; set; }
	string ID { get; }

	// returns an array of image ids that are contained in this repo
	// these are repo-wide
	string[] ImageIDs { get; }

	// returns an item corresponding to the appropriate imageid in this
	// collection
	ImageItem this[string index] { get; }

	// if many images are going to be added/deleted, updates should be
	// frozen before this hapepns -- this way, the collection change
	// event won't be fired until the final thaw
	void FreezeUpdates ();
	void ThawUpdates ();

	// add or delete items to/from this collection
	// the item must reside in the same repository as this item (i.e.
	// it must have been dded before using the repository's addimage
	// inteface)
	void AddItem (ImageItem image);
	void AddItem (string imageid);
	void DeleteItem (ImageItem image);
	void DeleteItem (string imageid);

	event CollectionChangeHandler OnCollectionChange;
}
