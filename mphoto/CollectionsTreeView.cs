
using System;

public class CollectionsTreeView
	: Gtk.TreeView
{
	IImageRepository repo;
	CollectionsTreeStore store;

	public CollectionsTreeView ()
		: base ()
	{
		Gtk.TreeViewColumn NameCol = new Gtk.TreeViewColumn ();
		Gtk.CellRendererText NameRenderer = new Gtk.CellRendererText ();
		NameRenderer.Editable = true;
		NameRenderer.Edited += new Gtk.EditedHandler (CollectionNameEditedHandler);
		NameCol.Title = "Name";
		NameCol.PackStart (NameRenderer, true);
		NameCol.AddAttribute (NameRenderer, "markup", 0);
		NameCol.SortColumnId = 0;
		NameCol.Resizable = true;
		NameCol.FixedWidth = 150;
		NameCol.Sizing = Gtk.TreeViewColumnSizing.Autosize;
		this.AppendColumn (NameCol);

		Gtk.TreeViewColumn CountCol = new Gtk.TreeViewColumn ();
		Gtk.CellRendererText CountRenderer = new Gtk.CellRendererText ();
		CountCol.Title = "Images";
		CountCol.PackStart (CountRenderer, true);
		CountCol.AddAttribute (CountRenderer, "text", 1);
		CountCol.SortColumnId = 1;
		CountCol.Resizable = true;
		CountCol.Sizing = Gtk.TreeViewColumnSizing.Autosize;
		this.AppendColumn (CountCol);

		store = new CollectionsTreeStore ();
		this.Model = store;
	}

	public CollectionsTreeView (IImageRepository repo_in)
		: this ()
	{
		Repository = repo_in;
	}

	public IImageRepository Repository {
		get {
			return repo;
		}
		set {
			if (repo != value) {
				repo = value;
				store.Repository = value;
			}
		}
	}

	public CollectionsTreeStore Store {
		get {
			return store;
		}
	}

	void CollectionNameEditedHandler (object o, Gtk.EditedArgs ea)
	{
		string collid = store.GetCollectionAtPathString (ea.Path);
		IImageCollection coll = repo.GetCollection (collid);
		if (coll == null) {
			Console.WriteLine ("collection " + collid + " not found ");
			return;
		}

		if (coll.Name != ea.NewText) {
			coll.Name = ea.NewText;
			Console.WriteLine ("new name '" + ea.NewText + "' for collection '" + collid + "' set!");
		}
	}
}
