
using System;

public class KeywordsWidget
	: Gtk.VBox
{
	IImageRepository repo;
	KeywordsTreeStore store;
	KeywordsTreeView tree_view;
	Gtk.Entry keyword_entry;

	public class KeywordsTreeView
		: Gtk.TreeView
	{
		public KeywordsTreeView ()
			: base ()
		{
			Gtk.TreeViewColumn KeywordCol = new Gtk.TreeViewColumn ();
			Gtk.CellRendererText KeywordRenderer = new Gtk.CellRendererText ();
			KeywordCol.Title = "Keyword";
			KeywordCol.PackStart (KeywordRenderer, true);
			KeywordCol.AddAttribute (KeywordRenderer, "markup", 0);
			KeywordCol.Sizing = Gtk.TreeViewColumnSizing.Autosize;
			this.AppendColumn (KeywordCol);

			this.HeadersVisible = false;
		}
	}

	public KeywordsWidget ()
		: base (false, 3)
	{
		tree_view = new KeywordsTreeView ();
		keyword_entry = new Gtk.Entry ();
		store = new KeywordsTreeStore ();
		tree_view.Model = store;

		keyword_entry.Activated += new EventHandler (KeywordEntryActivated);

		this.PackStart (keyword_entry, false, true, 0);
		this.PackStart (tree_view, true, true, 0);

		this.ShowAll ();
	}

	public KeywordsWidget (IconList icon_list)
		: this ()
	{
		store.IconList = icon_list;
	}

	public void KeywordEntryActivated (object o, EventArgs ea)
	{
		string[] imageids = store.CurrentIDs;
		if (imageids == null || imageids.Length == 0)
			return;

		ISearchableRepository srepo = repo as ISearchableRepository;
		if (srepo == null)
			return;

		string kwstring = keyword_entry.Text;
		kwstring = kwstring.Trim ();
		if (kwstring == "")
			return;

		string[] kws = kwstring.Split(',', '\n', '\r');
		if (kws.Length == 0)
			return;

		for (int i = 0; i < kws.Length; i++) {
			kws[i] = kws[i].Trim ();
			if (kws[i] != "") {
				foreach (string iid in imageids) {
					srepo.AddImageKeyword (iid, kws[i]);
				}
			}
		}

		keyword_entry.Text = "";
	}

	public IconList IconList {
		get {
			return store.IconList;
		}
		set {
			store.IconList = value;
		}
	}

	public IImageRepository Repository {
		get {
			return repo;
		}
		set {
			repo = value;
			store.Repository = value;
		}
	}

	public KeywordsTreeView TreeView {
		get {
			return tree_view;
		}
	}

	public KeywordsTreeStore Store {
		get {
			return store;
		}
		set {
			store = value;
		}
	}
}

