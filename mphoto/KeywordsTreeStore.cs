
using System;
using System.Collections;

public class KeywordsTreeStore
	: Gtk.TreeStore
{
	IconList which_list;
	IImageRepository repo;
	ArrayList image_ids;
	Hashtable kw_to_image_ids;

	public KeywordsTreeStore ()
		: base (typeof (string), typeof (string))
	{
		image_ids = new ArrayList ();
		kw_to_image_ids = new Hashtable ();
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

	public IImageRepository Repository
	{
		get {
			return repo;
		}
		set {
			if (repo != null)
				repo.OnRepositoryChange -= new RepositoryChangeHandler (RepositoryChanged);

			repo = value;

			if (repo != null)
				repo.OnRepositoryChange += new RepositoryChangeHandler (RepositoryChanged);
		}
	}

	public string[] CurrentIDs {
		get {
			string[] ids = new string[image_ids.Count];
			image_ids.CopyTo (ids, 0);
			return ids;
		}
	}

	public void ListSelectionChanged ()
	{
		if (which_list == null || which_list.Adapter == null) {
			UpdateFromID (null);
			return;
		}

		ArrayList ids = new ArrayList ();
		int c = which_list.Selection.Count;
		for (int i = 0; i < c; i++) {
			if (which_list.Selection[i]) {
				ids.Add (which_list.Adapter.GetImageID (i));
			}
		}

		if (ids.Count == 0) {
			UpdateFromID (null);
		} else {
			UpdateFromID (ids);
		}
	}

	private void UpdateFromID (ArrayList ids)
	{
		Console.WriteLine ("here, ids: {0}", ids);
		this.Clear ();
		image_ids.Clear ();
		kw_to_image_ids.Clear ();

		ISearchableRepository srepo = repo as ISearchableRepository;
		if (srepo == null) 
			return;

		if (ids == null)
			return;

		foreach (string imageid in ids) {
			image_ids.Add (imageid);
			string[] kwds = srepo.GetImageKeywords (imageid);
			foreach (string kw in kwds) {
				if (!kw_to_image_ids.Contains (kw)) {
					kw_to_image_ids.Add (kw, new ArrayList ());
				}
				((ArrayList) kw_to_image_ids[kw]).Add (imageid);
			}
		}

		Gtk.TreeIter iter = new Gtk.TreeIter ();
		foreach (string kw in kw_to_image_ids.Keys) {
			this.Append (out iter);
			if (((ArrayList) kw_to_image_ids[kw]).Count == image_ids.Count) {
				// this keyword is present on all the images
				this.SetValue (iter, 0, new GLib.Value (kw));
			} else {
				// this keyword is present only on some images
				this.SetValue (iter, 0, new GLib.Value ("<i>" + kw + "</i>"));
			}
			this.SetValue (iter, 1, new GLib.Value (kw));
		}
	}

	public void RepositoryChanged (IImageRepository r, RepositoryChangeEventArgs rcea)
	{
		if (!image_ids.Contains (rcea.WhichID))
			return;

		if (rcea.ChangeType == RepositoryChangeEventArgs.RepositoryChangeType.ImageRemoved) {
			UpdateFromID (null);
			return;
		}

		if (rcea.ChangeType == RepositoryChangeEventArgs.RepositoryChangeType.ImageChanged) {
			UpdateFromID ((ArrayList) image_ids.Clone ());
		}
	}
}
