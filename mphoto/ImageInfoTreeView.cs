
using System;

public class ImageInfoTreeView
	: Gtk.TreeView
{
	ImageInfoTreeStore store;

	public ImageInfoTreeView ()
		: base ()
	{
		Gtk.TreeViewColumn NameCol = new Gtk.TreeViewColumn ();
		Gtk.CellRendererText NameRenderer = new Gtk.CellRendererText ();
		NameRenderer.Background = "#eeeeee";
		NameCol.Title = "Name";
		NameCol.Resizable = true;
		NameCol.FixedWidth = 200;
		NameCol.PackStart (NameRenderer, true);
		NameCol.AddAttribute (NameRenderer, "markup", 0);
		this.AppendColumn (NameCol);

		Gtk.TreeViewColumn ValueCol = new Gtk.TreeViewColumn ();
		Gtk.CellRendererText ValueRenderer = new Gtk.CellRendererText ();
		ValueCol.Title = "Value";
		ValueCol.Resizable = true;
		ValueCol.PackStart (ValueRenderer, false);
		ValueCol.AddAttribute (ValueRenderer, "text", 1);
		this.AppendColumn (ValueCol);

		store = new ImageInfoTreeStore ();
		store.TreeView = this;  // so that we can get expanded when new data is written

		this.Model = store;
		this.Selection.Mode = Gtk.SelectionMode.None;
	}

	public ImageInfoTreeView (IconList icon_list)
		: this ()
	{
		store.IconList = icon_list;
	}

	public IconList IconList {
		get {
			return store.IconList;
		}
		set {
			store.IconList = value;
		}
	}

	public ImageInfoTreeStore Store {
		get {
			return store;
		}
	}
}
