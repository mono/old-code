/*
 * IImageRepository.cs
 *
 * Author(s): Vladimir Vukicevic <vladimir@pobox.com>
 *
 * Copyright (C) 2002  Vladimir Vukicevic
 */

using System;

public delegate void RepositoryChangeHandler (IImageRepository repo, RepositoryChangeEventArgs args);

public class RepositoryChangeEventArgs {
	public enum RepositoryChangeType {
		ImageAdded,
		ImageRemoved,
		ImageChanged,
		CollectionAdded,
		CollectionRemoved,
		OtherChange
	}

	public RepositoryChangeType ChangeType;
	public string WhichID;

	public RepositoryChangeEventArgs (RepositoryChangeType rt, string which)
	{
		ChangeType = rt;
		WhichID = which;
	}
}

public interface IImageRepository {
	//
	// functions dealing with the whole set of images in this repo
	int CountImages ();
	string[] GetImageIDs ();
	ImageItem GetImage (string imageid);

	// adding an image will return the same imageitem back,
	// but will fill in its repo field.  Adding an imageitem
	// that already has a repo set is an error, and an
	// exception can be raised.  there is no current way to
	// copy images between repos.
	ImageItem AddImage (ImageItem item);

	void DeleteImage (ImageItem item);
	void DeleteImage (string imageid);

	//
	// functions dealing with collections of images present in this repo
	int CountCollections ();
	string[] GetCollectionIDs ();
	IImageCollection GetCollection (string collid);
	IImageCollection CreateCollection ();
	void DeleteCollection (IImageCollection theColl);
	void DeleteCollection (string collid);

	// thumbnail generation
    
	// deletehumbnails flushes all the thumbnails for this repository
	// and forces them to be regenerated.  this can be quite expensive,
	// but useful if some thumbnails are out of sync.
	void DeleteThumbnails ();

	// gets thumbnail information for the image (such as a thumbnail
	// file).  if the structure were appropriately extended,
	// embedded exif thumbnails could be returned as well.
	ImageItem.ThumbnailInfo GetThumbnailForImage (ImageItem item);

	// event fired whenever a change in the repository happens
	// (i.e. image added/removed or collection added/removed)
	event RepositoryChangeHandler OnRepositoryChange;
}

