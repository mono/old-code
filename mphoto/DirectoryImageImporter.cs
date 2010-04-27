/*
 * DirectryImageImporter.cs
 *
 * Author(s): Vladimir Vukicevic <vladimir@pobox.com>
 *
 * Copyright (C) 2002  Vladimir Vukicevic
 */

using System;
using System.IO;
using System.Diagnostics;

public class DirectoryImageImporter : IImageImporter {
	public DirectoryImageImporter () {
		// really nothing to be done here
	}

	public bool CanImportUri (string uri)
	{
		char[] uri_chars = uri.ToCharArray ();
		if (uri_chars[0] == '/') {
			return true;
		}

		return false;
	}

	public void ImportUri (string uri, IImageCollection coll)
	{
		DoRealImport (uri, coll.Repo, coll);
	}

	public void ImportUri (string uri, IImageRepository repo)
	{
		DoRealImport (uri, repo, null);
	}

	private void DoRealImport (string uri, IImageRepository repo, IImageCollection coll)
	{
		if (!CanImportUri (uri)) {
			throw new InvalidOperationException ();
		}
		DirectoryInfo sourcedir = new DirectoryInfo (uri);
		FileInfo[] finfos = sourcedir.GetFiles ();

		if (coll != null)
			coll.FreezeUpdates ();

		foreach (FileInfo fi in finfos) {
			string lf = fi.FullName.ToLower ();
			if (lf.EndsWith (".jpg") ||
			    lf.EndsWith (".jpeg") ||
			    lf.EndsWith (".png") ||
			    lf.EndsWith (".gif") ||
			    lf.EndsWith (".tif") ||
			    lf.EndsWith (".tiff"))
			{
//                int[] wh = GetImageDimensions (fi.FullName);

				ImageItem.ImageInfo iinfo = new ImageItem.ImageInfo ();
//                iinfo.width = wh[0];
//                iinfo.height = wh[1];
				iinfo.width = 0;
				iinfo.height = 0;
				iinfo.filesize = (int) fi.Length;

				iinfo.dirname = uri;
				iinfo.filename = fi.Name;

				ImageItem iitem = new ImageItem (iinfo);
				iitem = repo.AddImage (iitem);
				if (coll != null)
					coll.AddItem (iitem);
			}
		}

		if (coll != null)
			coll.ThawUpdates ();
	}

	int[] GetImageDimensions (string imagefile)
	{
		int[] wh = new int[2];
		bool found_exif_data = false;

		using (ExifData ed = new ExifData (imagefile)) {
			string sw, sh;
			sw = ed.Lookup (ExifTag.PixelXDimension);
			sh = ed.Lookup (ExifTag.PixelYDimension);
			if (sw == null || sw == "") {
				sw = ed.Lookup (ExifTag.ImageWidth);
				sh = ed.Lookup (ExifTag.ImageLength);
			}
			if (sw != null && sw != "") {
				wh[0] = Convert.ToInt32 (sw);
				wh[1] = Convert.ToInt32 (sh);
				found_exif_data = true;
			}
		}

		if (!found_exif_data) {
#if SLOW_WH_DISCOVERY
			Process proc = new Process ();
			proc.StartInfo.FileName = "identify";
			proc.StartInfo.Arguments = "-format \"%w %h\" " + imagefile;
			proc.StartInfo.UseShellExecute = false;
			proc.StartInfo.RedirectStandardOutput = true;
			proc.Start ();
			string procout = proc.StandardOutput.ReadToEnd ();
			proc.WaitForExit ();
			int result = proc.ExitCode;
			proc.Close ();

			char[] splitchars = {' '};
			string[] whstring = procout.Split(splitchars);

			wh[0] = Convert.ToInt32 (whstring[0]);
			wh[1] = Convert.ToInt32 (whstring[1]);
#else
			wh[0] = 0;
			wh[1] = 1;
#endif
		}

		return wh;
	}
}
