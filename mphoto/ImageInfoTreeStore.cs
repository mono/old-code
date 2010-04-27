using System;
using GLib;
using Gtk;
using GtkSharp;
using System.IO;
using System.Runtime.InteropServices;

public class ImageInfoTreeStore : Gtk.TreeStore
{
	IconList which_list;
	TreeView my_view;

	TreeIter iter_file_name, iter_file_size, iter_file_date;
    
	// we really want to store the exif tag in the model
	// and then somehow hide the rows that have data that we don't care about

	public ImageInfoTreeStore ()
		: base (typeof (string), typeof (string))
	{
	}

	public IconList IconList
	{
		get {
			return which_list;
		}
		set {
			if (which_list != value) {
				if (which_list != null)
					which_list.SelectionChanged -= new SelectionChange (ListSelectionChanged);

				this.Clear ();
				which_list = value;

				if (which_list != null)
					which_list.SelectionChanged += new SelectionChange (ListSelectionChanged);
			}
		}
	}

	public TreeView TreeView {
		get {
			return my_view;
		}
		set {
			my_view = value;
		}
	}

	TreeIter MakeNode (TreeIter parent, out TreeIter iter, string text)
	{
		iter = this.Append (parent);
		this.SetValue (iter, 0, new GLib.Value (text));
		return iter;
	}

	TreeIter MakeNode (TreeIter parent, out TreeIter iter, string text, string value)
	{
		iter = this.Append (parent);
		this.SetValue (iter, 0, new GLib.Value (text));
		this.SetValue (iter, 1, new GLib.Value (value));
		return iter;
	}

	TreeIter MakeNode (out TreeIter iter, string text, string value)
	{
		this.Append (out iter);
		this.SetValue (iter, 0, new GLib.Value (text));
		this.SetValue (iter, 1, new GLib.Value (value));
		return iter;
	}

	bool MakeExifNode (TreeIter parent, TreeIter iter, ExifData ed, ExifTag tag)
	{
		string val = ed.Lookup (tag);
		if (val == null || val == "")
			return false;

		MakeNode (parent, out iter, ExifUtil.GetTagTitle (tag) + ":", ed.Lookup (tag));
		return true;
	}
        
	void UpdateForFile (string filename)
	{
		this.Clear ();

		TreeIter first_level_iter = new TreeIter ();
		using (ExifData ed = new ExifData (filename)) {
			TreeIter child_iter;
			string s;
			FileInfo fi = new FileInfo (filename);
            
			MakeNode (out first_level_iter, "File:", fi.Name);
			MakeNode (out first_level_iter, "Date:", ed.Lookup (ExifTag.DateTime));

			string ws = ed.Lookup (ExifTag.PixelXDimension);
			string hs = ed.Lookup (ExifTag.PixelYDimension);

			if (ws == null || ws == "") {
				ws = ed.Lookup (ExifTag.ImageWidth);
				hs = ed.Lookup (ExifTag.ImageLength);
			}
			if (ws != null && ws != "") {
				int width;
				int height;
				try {
					width = Convert.ToInt32 (ws);
					height = Convert.ToInt32 (hs);
				} catch {
					width = 0;
					height = 0;
				}

				if (width != 0)
					MakeNode (out first_level_iter, "Dimensions:", width + "x" + height);
				else
					MakeNode (out first_level_iter, "Dimensions:", "unknown");
			}
			MakeNode (out first_level_iter, "Size:", String.Format ("{0:0,0}", fi.Length) + " bytes");
			this.Append (out first_level_iter);
			this.SetValue (first_level_iter, 0, new GLib.Value ("<b>Exposure</b>"));

			child_iter = new TreeIter ();
			if (!MakeExifNode (first_level_iter, child_iter, ed, ExifTag.ApertureValue))
				MakeExifNode (first_level_iter, child_iter, ed, ExifTag.FNumber);
			if (!MakeExifNode (first_level_iter, child_iter, ed, ExifTag.ShutterSpeedValue))
				MakeExifNode (first_level_iter, child_iter, ed, ExifTag.ExposureTime);
			MakeExifNode (first_level_iter, child_iter, ed, ExifTag.FocalLength);
			MakeExifNode (first_level_iter, child_iter, ed, ExifTag.ISOSpeedRatings);
			MakeExifNode (first_level_iter, child_iter, ed, ExifTag.MeteringMode);
			MakeExifNode (first_level_iter, child_iter, ed, ExifTag.ExposureBiasValue);
			MakeExifNode (first_level_iter, child_iter, ed, ExifTag.FlashEnergy);
			MakeExifNode (first_level_iter, child_iter, ed, ExifTag.WhiteBalance);
            
//                this.Append (out first_level_iter);
//                this.SetValue (first_level_iter, 0, new GLib.Value ("<b>Image</b>"));
//                child_iter = new TreeIter ();

			this.Append (out first_level_iter);
			this.SetValue (first_level_iter, 0, new GLib.Value ("<b>Equipment</b>"));

			child_iter = new TreeIter ();
			MakeExifNode (first_level_iter, child_iter, ed, ExifTag.Make);
			MakeExifNode (first_level_iter, child_iter, ed, ExifTag.Model);
			MakeExifNode (first_level_iter, child_iter, ed, ExifTag.Software);
			MakeExifNode (first_level_iter, child_iter, ed, ExifTag.ExifVersion);
			MakeExifNode (first_level_iter, child_iter, ed, ExifTag.SensingMethod);
//            MakeExifNode (first_level_iter, child_iter, ed, ExifTag.FocalPlaneXResolution);
//            MakeExifNode (first_level_iter, child_iter, ed, ExifTag.FocalPlaneYResolution);
//            MakeExifNode (first_level_iter, child_iter, ed, ExifTag.FocalPlaneResolutionUnit);
		}
		my_view.ExpandAll ();
	}

	public void ListSelectionChanged ()
	{
		if (which_list == null || which_list.Adapter == null)
			return;

		int c = which_list.Selection.Count;
		int first_set = -1;
		for (int i = 0; i < c; i++) {
			if (which_list.Selection[i]) {
				if (first_set == -1) {
					first_set = i;
				} else {
					first_set = -2;
				}
			}
		}
        
		if (first_set >= 0) {
			this.UpdateForFile (which_list.Adapter.GetFullFilename (first_set));
		}
	}
}
