using System;
using GLib;
using Gtk;
using GtkSharp;
using System.IO;

class ImageInfo : ScrolledWindow {
	TreeView tv;
	TreeStore store;

	TreeIter iter_file_name, iter_file_size, iter_file_date;
	
	public ImageInfo () : base (null, null)
	{
		store = new TreeStore ((int)TypeFundamentals.TypeString,
				       (int)TypeFundamentals.TypeString);
		
		tv = new TreeView (store);
		tv.HeadersVisible = true;

		TreeViewColumn NameCol = new TreeViewColumn ();
		CellRenderer NameRenderer = new CellRendererText ();

		NameCol.Title = "Name";
		NameCol.PackStart (NameRenderer, true);
		NameCol.AddAttribute (NameRenderer, "markup", 0);
		tv.AppendColumn (NameCol);

		TreeViewColumn ValueCol = new TreeViewColumn ();
		CellRenderer ValueRenderer = new CellRendererText ();
		ValueCol.Title = "Value";
		ValueCol.PackStart (ValueRenderer, false);
		ValueCol.AddAttribute (ValueRenderer, "text", 1);
		tv.AppendColumn (ValueCol);

		//
		// Populate tree
		//

		TreeIter iter = new TreeIter ();
		PopulateGeneral (out iter);
		PopulateDetails (out iter);
		Add (tv);
	}

	TreeIter MakeNode (TreeIter parent, out TreeIter iter, string text)
	{
		store.Append (out iter, parent);
		store.SetValue (iter, 0, new GLib.Value (text));
		return iter;
	}

	void PopulateGeneral (out TreeIter parent)
	{
		store.Append (out parent);
		store.SetValue (parent, 0, new GLib.Value ("<b>General</b>"));
		store.SetValue (parent, 1, new GLib.Value (""));

		TreeIter iter = new TreeIter ();
		iter_file_name = MakeNode (parent, out iter, "File:");
		iter_file_size = MakeNode (parent, out iter, "Size:");
		iter_file_date = MakeNode (parent, out iter, "Date:");
	}

	TreeIter [] det_iter;
	Array det_values;
	void PopulateDetails (out TreeIter parent)
	{
		store.Append (out parent);
		store.SetValue (parent, 0, new GLib.Value ("<b>Details</b>"));
		store.SetValue (parent, 1, new GLib.Value (""));
		
		det_values = Enum.GetValues (typeof (ExifTag));
		det_iter = new TreeIter [det_values.Length];
		
		int i = 0;
		TreeIter iter = new TreeIter ();
		foreach (object v in det_values){
			MakeNode (parent, out iter, ExifUtil.GetTagTitle ((ExifTag) v) + ":");
			det_iter [i] = iter;

			i++;
		}
	}

	GLib.Value blank = new GLib.Value ("");
		
	public void ClearInfo ()
	{
		store.SetValue (iter_file_name, 1, blank);
		store.SetValue (iter_file_size, 1, blank);
		store.SetValue (iter_file_date, 1, blank);

		int i = 0;
		foreach (object v in det_values){
			store.SetValue (det_iter [i++], 1, blank);
		}
	}

	public void UpdateInfo (string path)
	{
		FileInfo fi = new FileInfo (path);
		
		store.SetValue (iter_file_name, 1, new GLib.Value (fi.Name));
		store.SetValue (iter_file_size, 1, new GLib.Value (String.Format ("{0:0,0}", fi.Length)));
		store.SetValue (iter_file_date, 1, new GLib.Value (File.GetLastWriteTime (path).ToString ()));

		using (ExifData ed = new ExifData (path)){
			int i = 0;
			
			foreach (object v in det_values){
				store.SetValue (det_iter [i], 1, new GLib.Value (ed.Lookup ((ExifTag) v)));
				i++;
			}
		}
	}
}
