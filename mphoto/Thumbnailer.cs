/*
 * Thumbnailer.cs
 *
 * Author(s): Vladimir Vukicevic <vladimir@pobox.com>
 *
 * Copyright (C) 2002  Vladimir Vukicevic
 *
 */

using System;
using System.IO;
using System.Threading;
using System.Collections;

using Gdk;

public enum ThumbnailStatus {
	OK,
	InvalidSource,
	UnableToCreate
}
    
public class Thumbnailer {

	static ArrayList threads_to_stop;

	public class ThumbnailerInfo {
		public string id;
		public string source;
		public string target;
		public Gdk.Pixbuf thumbnail;
		public ThumbnailStatus status;
	}

	public class ThumbnailFinishedEventArgs {
		public ThumbnailerInfo tinfo;
		public ThumbnailFinishedEventArgs (ThumbnailerInfo _tinfo) {
			tinfo = _tinfo;
		}
	}

	Stack thumb_stack;
	Queue thumbs_done;
	Thread thumbnail_worker;
	Semaphore thumbnail_sema;
	Gtk.ThreadNotify main_notify;

	static Thumbnailer ()
	{
		threads_to_stop = new ArrayList ();
	}

	public Thumbnailer ()
	{
		thumb_stack = new Stack ();
		thumbs_done = new Queue ();
		thumbnail_sema = new Semaphore ();

		ThreadStart tstart = new ThreadStart (Thumbnailer_Thread);
		thumbnail_worker = new Thread (tstart);
		thumbnail_worker.Start ();

		threads_to_stop.Add (thumbnail_worker);

		main_notify = new Gtk.ThreadNotify (new Gtk.ReadyEvent (ThumbnailFinished));
	}

	public static void KillThreads ()
	{
		foreach (Thread t in threads_to_stop) {
			t.Abort ();
		}
	}

	public void Thumbnail (ThumbnailerInfo tinfo)
	{
		lock (thumb_stack) {
			if (!thumb_stack.Contains (tinfo)) {
//                Console.WriteLine ("T: Got request for " + tinfo.source);
				thumb_stack.Push (tinfo);
				thumbnail_sema.Up ();
			}
		}
	}

	void ThumbnailFinished ()
	{
		lock (thumbs_done) {
			while (thumbs_done.Count > 0) {
				ThumbnailerInfo tinfo = (ThumbnailerInfo) thumbs_done.Dequeue ();
//                Console.WriteLine ("T: Finished for " + tinfo.source);
				if (OnThumbnailFinished != null)
					OnThumbnailFinished (this, new ThumbnailFinishedEventArgs (tinfo));
			}
		}
	}

	void Thumbnailer_Thread ()
	{
		Console.WriteLine ("T: Hello from the Thumbnailer thread");
		while (true) {
			try {
				thumbnail_sema.Down ();
			} catch {
				// ignore
			}

			ThumbnailerInfo tinfo;

			while (thumb_stack.Count > 0) {
				lock (thumb_stack) {
					tinfo = (ThumbnailerInfo) thumb_stack.Pop ();
				}

				MakeThumbnail (tinfo);

				lock (thumbs_done) {
					thumbs_done.Enqueue (tinfo);
				}
				main_notify.WakeupMain ();
			}
		}
	}

	public void MakeThumbnail (ThumbnailerInfo tinfo)
	{
		// first check and see if there's a thumbnail in the exif
#if USE_EXIF_THUMBS
		try {
			using (ExifData ed = new ExifData (tinfo.source)) {
				byte [] thumbData = ed.Data;
				if (thumbData.Length > 0) {
					Console.WriteLine ("Trying to write " + tinfo.target);
					// exif contains a thumbnail, so spit it out
					FileStream fs = File.Create (tinfo.target, Math.Min (thumbData.Length, 4096));
					fs.Write (thumbData, 0, thumbData.Length);
					fs.Close ();

					tinfo.thumbnail = null;
					tinfo.status = ThumbnailStatus.OK;
					return;
				}
			}
		} catch {
			Console.WriteLine ("** exif died for " + tinfo.target);
		}
#endif

		// if not found, use GdkPixbuf to scale
		try {
			using (Pixbuf image_pixbuf = new Pixbuf (tinfo.source)) {
				int thumb_width;
				int thumb_height;
				if (image_pixbuf.Width > image_pixbuf.Height) {
					thumb_width = 160;
					thumb_height = (int) (160 * ((float) image_pixbuf.Height / (float) image_pixbuf.Width));
				} else {
					thumb_height = 160;
					thumb_width = (int) (160 * ((float) image_pixbuf.Width / (float) image_pixbuf.Height));
				}

				Pixbuf thumb_pixbuf = image_pixbuf.ScaleSimple (thumb_width, thumb_height, InterpType.Tiles);

				// this will need to be fixed when this particular Gdk.Pixbuf function gets
				// better bindings
				try {
					tinfo.thumbnail = thumb_pixbuf;
					if (tinfo.target.EndsWith ("png")) {
						thumb_pixbuf.Savev (tinfo.target, "png", null, null);
					} else {
						thumb_pixbuf.Savev (tinfo.target, "jpeg", null, null);
					}
				} catch (GLib.GException e) {
					tinfo.status = ThumbnailStatus.UnableToCreate;
					return;
				}

				tinfo.status = ThumbnailStatus.OK;
			}
		} catch (GLib.GException e) {
			tinfo.status = ThumbnailStatus.InvalidSource;
			return;
		}
	}

	public delegate void OnThumbnailFinishedHandler (Thumbnailer t, ThumbnailFinishedEventArgs targs);
	public event OnThumbnailFinishedHandler OnThumbnailFinished;
}
