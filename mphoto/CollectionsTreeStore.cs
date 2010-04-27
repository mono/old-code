
using System;
using GLib;

public class CollectionsTreeStore 
	: Gtk.ListStore
{
	IImageRepository repo;

	public enum ColNames {
		NameCol,
		CountCol,
		InternalIDCol
	}

	public CollectionsTreeStore ()
		: base (typeof (string), typeof (int), typeof (string))
	{
	}

	public CollectionsTreeStore (IImageRepository repo_in)
		: this ()
	{
		Repository = repo_in;
	}

	public IImageRepository Repository {
		get {
			return repo;
		}
		set {
			if (repo != null) {
				foreach (string cid in repo.GetCollectionIDs()) {
					repo.GetCollection (cid).OnCollectionChange -= new CollectionChangeHandler (CollectionChanged);
				}
				repo.OnRepositoryChange -= new RepositoryChangeHandler (RepositoryChanged);
			}

			repo = value;

			if (repo != null) {
				repo.OnRepositoryChange += new RepositoryChangeHandler (RepositoryChanged);

				foreach (string cid in repo.GetCollectionIDs()) {
					repo.GetCollection (cid).OnCollectionChange += new CollectionChangeHandler (CollectionChanged);
				}
			}

			Refresh ();
		}
	}

	public void Refresh ()
	{
		this.Clear ();

		if (repo != null) {
			Gtk.TreeIter iter = new Gtk.TreeIter ();
			foreach (string coll_id in repo.GetCollectionIDs()) {
				IImageCollection icoll = repo.GetCollection (coll_id);
				iter = this.Append ();

				this.SetValue (iter, (int) ColNames.NameCol, new GLib.Value (icoll.Name));
				this.SetValue (iter, (int) ColNames.CountCol, new GLib.Value (icoll.Count));
				this.SetValue (iter, (int) ColNames.InternalIDCol, new GLib.Value (icoll.ID));
			}
		}
	}

	public string GetCollectionAtPathString (string str_path)
	{
		Gtk.TreeIter iter = new Gtk.TreeIter ();

		if (!this.GetIterFromString (out iter, str_path)) {
			Console.WriteLine ("Spurious NameEditedHandler for unknown path " + str_path);
			return null;
		}

		GLib.Value gval = new GLib.Value ();
		this.GetValue (iter, (int) ColNames.InternalIDCol, ref gval);
		return (string) gval;
	}

	public string GetCollectionAtPath (Gtk.TreePath path)
	{
		Gtk.TreeIter iter = new Gtk.TreeIter ();

		if (!this.GetIter (out iter, path)) {
			Console.WriteLine ("Spurious NameEditedHandler for unknown path " + path);
			return null;
		}

		GLib.Value gval = new GLib.Value ();
		this.GetValue (iter, (int) ColNames.InternalIDCol, ref gval);
		return (string) gval;
	}

	void CollectionChanged (IImageCollection coll, EventArgs ea)
	{
		Console.WriteLine ("collectionChanged! " + coll.ID);
		Gtk.TreeIter iter = new Gtk.TreeIter ();
		bool valid;

		valid = this.GetIterFirst (out iter);
		while (valid) {
			GLib.Value gv = new GLib.Value ();
			this.GetValue (iter, (int) ColNames.InternalIDCol, ref gv);
			if ((string) gv == coll.ID) {
				// match
				break;
			}

			valid = this.IterNext (ref iter);
		}

		if (valid) {
			this.SetValue (iter, (int) ColNames.NameCol, new GLib.Value (coll.Name));
			this.SetValue (iter, (int) ColNames.CountCol, new GLib.Value (coll.Count));
		}
	}

	void RepositoryChanged (IImageRepository repo, RepositoryChangeEventArgs ea)
	{
		if (ea.ChangeType == RepositoryChangeEventArgs.RepositoryChangeType.CollectionAdded ||
		    ea.ChangeType == RepositoryChangeEventArgs.RepositoryChangeType.CollectionRemoved)
		{
			if (ea.ChangeType == RepositoryChangeEventArgs.RepositoryChangeType.CollectionAdded) {
				repo.GetCollection (ea.WhichID).OnCollectionChange += new CollectionChangeHandler (CollectionChanged);
			} else if (ea.ChangeType == RepositoryChangeEventArgs.RepositoryChangeType.CollectionRemoved) {
				repo.GetCollection (ea.WhichID).OnCollectionChange -= new CollectionChangeHandler (CollectionChanged);
			}
			Refresh ();
		}
	}
}
