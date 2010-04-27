/*
 * ImageItem.cs
 *
 * Author(s): Vladimir Vukicevic <vladimir@pobox.com>
 *
 * Copyright (C) 2002  Vladimir Vukicevic
 */

using System;
using Gdk;

public class ImageItem {
	public enum ImageOrientation {
		Landscape,
		Portrait
	}

	public class ImageInfo {
		internal IImageRepository repo;

		public int width;
		public int height;
		public int filesize;

		public string dirname;
		public string filename;
		public string imageid;

		public string caption;
	}

	public class ThumbnailInfo {
		public string filename;
	}

	// private fields
	internal ImageInfo thisInfo;
	internal ThumbnailInfo thisThumbnailInfo;

	internal Pixbuf thisImagePixbuf;
	internal Pixbuf thisThumbnailPixbuf;

	// public constructors
	public ImageItem (ImageInfo iinfo)
	{
		thisInfo = iinfo;
	}

	// public properties
	public int Width {
		get { return thisInfo.width; }
	}

	public int Height {
		get { return thisInfo.height; }
	}

	public int Filesize {
		get { return thisInfo.filesize; }
	}

	public string Filename {
		get { return thisInfo.filename; }
	}

	public string Dirname {
		get { return thisInfo.dirname; }
	}

	public string FullFilename {
		get { return thisInfo.dirname + "/" + thisInfo.filename; }
	}

	public string ImageID {
		get { return thisInfo.imageid; }
	}

	public string Caption {
		get { return thisInfo.caption; }
		set {
			thisInfo.caption = value;
			if (OnImageInfoChange != null) {
				OnImageInfoChange (this, System.EventArgs.Empty);
			}
		}
	}

	public IImageRepository Repo {
		get { return thisInfo.repo; }
	}
        
	public ImageInfo Info {
		get { return thisInfo; }
	}

	public ThumbnailInfo ThumbInfo {
		get { return thisThumbnailInfo; }
	}

	public ImageOrientation Orientation {
		get {
			if (thisInfo.width < thisInfo.height) return ImageOrientation.Portrait;
			return ImageOrientation.Landscape;
		}
	}

	// public methods
	public Pixbuf Image {
		get {
			if (thisImagePixbuf == null) {
				try {
					thisImagePixbuf = new Pixbuf (thisInfo.dirname + "/" + thisInfo.filename);
				} catch (GLib.GException e) {
					// this image data isn't valid
					thisImagePixbuf = new Pixbuf (null, "invalid.png");
				}
			}

			return thisImagePixbuf;
		}
	}

	public Pixbuf ImageThumbnail {
		get {
			if (thisThumbnailPixbuf == null) {
				if (thisThumbnailInfo == null) {
					thisThumbnailInfo = thisInfo.repo.GetThumbnailForImage (this);
				}

				if (thisThumbnailInfo != null) {
//                    Console.WriteLine ("*** Creating Pixbuf from " + thisThumbnailInfo.filename);
					try {
						thisThumbnailPixbuf = new Pixbuf (thisThumbnailInfo.filename);
					} catch (Exception e) {
						// broken thumbnail
						Console.WriteLine ("*** Thumbnail " + thisThumbnailInfo.filename + " is broken");
						Console.WriteLine (" -- exception: " + ((GLib.GException) e).Message);
						thisThumbnailPixbuf = new Pixbuf (null, "broken.png");
					}
				} else {
//                    Console.WriteLine ("*** Returning LoadingImage");
//                    if (IconList.LoadingImage != null) {
//                        IconList.LoadingImage.Ref ();
//                        thisThumbnailPixbuf = IconList.LoadingImage;
//                    } else {
					thisThumbnailPixbuf = new Pixbuf (null, "loading.png");
//                    }
				}
			}

			return thisThumbnailPixbuf;
		}
		set {
			// someone is trying to give us a thumbnail to use for this session
			thisThumbnailPixbuf = value;
			if (OnThumbnailChange != null)
				OnThumbnailChange (this, EventArgs.Empty);
		}
	}

	public void RefreshThumbnail ()
	{
		thisThumbnailInfo = thisInfo.repo.GetThumbnailForImage (this);

		try {
			thisThumbnailPixbuf = new Pixbuf (thisThumbnailInfo.filename);
		} catch {
			thisThumbnailPixbuf = new Pixbuf (null, "broken.png");
		}

		if (OnThumbnailChange != null) {
			OnThumbnailChange (this, EventArgs.Empty);
		}
	}

	// public delegates & events
	public delegate void ImageInfoChangeHandler (ImageItem iitem, EventArgs unused);
	public event ImageInfoChangeHandler OnImageInfoChange;

	public delegate void ThumbnailChangeHandler (ImageItem iitem, EventArgs unused);
	public event ThumbnailChangeHandler OnThumbnailChange;
}
