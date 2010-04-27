/*
 * MphotoToplevel.cs
 *
 * Author(s): Vladimir Vukicevic <vladimir@pobox.com>
 *
 * Copyright (C) 2002  Vladimir Vukicevic
 *
 */

/*
 * this file is a pretty gross mishmash of all sorts of junk,
 * mostly dealing with the UI, the glade signal handlers,
 * and some interconnecting goop.
 *
 * there are a few things here that are prime candidates for splitting out
 */

using System;
using System.Collections;
using System.Text;
using System.Runtime.InteropServices;

using GLib;
using Gtk;
using GtkSharp;
using Gnome;
using Glade;
using System.IO;

public class MphotoToplevel : Program {
	public static MphotoToplevel GlobalMphotoToplevel = null;

	private Glade.XML gxml;
	private IImageRepository repo;
	private IImageCollection collection;

	private Gtk.Widget toplevel;
	private Gtk.Notebook leftside_notebook;
	private Gtk.Notebook top_level_notebook;

	private Gtk.Window search_dialog;
#if HAVE_LIBEOG
	private EogUiImage image_ui;
#endif
	private IconList icon_list; /* not from glade */

	private Gtk.Label label_zoom_value;

	private CollectionsTreeView collections_tree_view;
	private ImageInfoTreeView imageinfo_tree_view;
	private KeywordsWidget keywords_widget;

	Glade.XMLCustomWidgetHandler cwh;

	[DllImport("libc.so.6")]
	public static extern void kill (int pid, int sig);

	// cheap way to set breakpoints
	public void StopHere ()
	{
		Console.WriteLine ("StopHere");
		// --break is broken
//        kill (0, 5);
	}

	public unsafe MphotoToplevel (IImageRepository _repo, string[] args, params object[] props)
		: base ("Mphoto", "0.0", Modules.UI, args, props)
	{
		cwh = new Glade.XMLCustomWidgetHandler (GladeCustomWidgetHandler);
		Glade.XML.SetCustomHandler (cwh);
		gxml = new Glade.XML (null, "mphoto.glade", null, null);

		Console.WriteLine ("Autoconnect");
		gxml.Autoconnect (this);

//        CreateCustomWidget ("browser_icon_list");
//        CreateCustomWidget ("collections_tree_view");
//        CreateCustomWidget ("imageinfo_tree_view");
//        CreateCustomWidget ("eog_image_view");
//        CreateCustomWidget ("keywords_widget");

		this.leftside_notebook = (Gtk.Notebook) gxml["browser_left_notebook"];
		this.top_level_notebook = (Gtk.Notebook) gxml["top_level_notebook"];

		this.toplevel = gxml["mphoto_browser"];
		this.label_zoom_value = (Gtk.Label) gxml["label_zoom_value"];
		this.search_dialog = (Gtk.Window) gxml["search_dialog"];

		this.Repository = _repo;

		// initialize the tree views to the side
		imageinfo_tree_view.IconList = icon_list;
		keywords_widget.IconList = icon_list;

		collections_tree_view.RowActivated += new RowActivatedHandler (CollectionsTreeViewActivateHandler);
		icon_list.Activated += new EventHandler (IconlistActivated);

		GlobalMphotoToplevel = this;
	}

	void CreateCustomWidget (string wname) {
		Gtk.Widget w = GladeCustomWidgetHandler (null, null, wname, null, null, 0, 0);
		Gtk.Bin bin = (Gtk.Bin) gxml[wname + "_box"];
		bin.Add (w);
	}

	private IImageRepository Repository {
		get {
			return repo;
		}
		set {
			repo = value;
			IImageCollection collection = null;

			if (repo != null) {
				Console.WriteLine ("Repo has " + repo.CountCollections() + " collections");
				string firstcollection = null;
				foreach (string cid in repo.GetCollectionIDs()) {
					if (firstcollection == null) {
						firstcollection = cid;
						break;
					}
				}
				if (firstcollection != null)
					collection = repo.GetCollection (firstcollection);
			} 

			collections_tree_view.Repository = repo;
			keywords_widget.Repository = repo;
			SelectCollection (collection);
		}
	}

	public Widget Toplevel {
		get {
			return toplevel;
		}
	}

//    public Glade.XML GladeXML {
//        get {
//            return gxml;
//        }
//    }

	public void SelectCollection (IImageCollection c)
	{
		collection = c;
		if (icon_list != null) {
			icon_list.Adapter = new CollectionIconListAdapter (collection);
		}
        
		top_level_notebook.CurrentPage = 0;
	}

	//
	// glade signal handlers
	//

	// custom widget creation callback
	public Gtk.Widget GladeCustomWidgetHandler (Glade.XML xml, string func_name, string name, string s1, string s2, int i1, int i2)
	{
		Console.WriteLine ("customWidgetHandler: widget for " + name);

		if (name == "browser_icon_list") {
			icon_list = new IconList ();
			Scrollbar scroll = new VScrollbar (icon_list.Adjustment);
			Box box = new HBox (false, 0);

			box.PackStart (icon_list, true, true, 0);
			box.PackStart (scroll, false, true, 0);

			box.ShowAll ();
			return box;
		}

#if HAVE_LIBEOG
		if (name == "eog_image_view") {
			image_ui = new EogUiImage ();
			Console.WriteLine ("Handle: " + image_ui.Handle);
			image_ui.Show ();
			return image_ui;
		}
#else
		if (name == "eog_image_view") {
			Gtk.Widget w = new Gtk.Label ("EOG support not enabled; Viewer is disabled.");
			w.Show ();
			return w;
		}
#endif

		if (name == "collections_tree_view") {
			collections_tree_view = new CollectionsTreeView ();
			collections_tree_view.Show ();
			return collections_tree_view;
		}

		if (name == "imageinfo_tree_view") {
			imageinfo_tree_view = new ImageInfoTreeView ();
			imageinfo_tree_view.Show ();
			return imageinfo_tree_view;
		}

		if (name == "keywords_widget") {
			keywords_widget = new KeywordsWidget ();
			keywords_widget.Show ();
			return keywords_widget;
		}

		Console.WriteLine ("Returning nil");
		return null;
	}

	// switch left-pane to collections view
	public void on_pane_collections_activate (object o, EventArgs ea)
	{
		leftside_notebook.CurrentPage = 0;
	}

	// switch left pane to image information view
	public void on_pane_iinfo_activate (object o, EventArgs ea)
	{
		leftside_notebook.CurrentPage = 1;
	}

	// switch left pane to comment view
	public void on_pane_comment_activate (object o, EventArgs ea)
	{
		leftside_notebook.CurrentPage = 2;
	}

	// switch left pane to keyword view
	public void on_pane_keywords_activate (object o, EventArgs ea)
	{
		leftside_notebook.CurrentPage = 3;
	}

	public void on_window_delete (object o, DeleteEventArgs a)
	{
		Quit ();

		a.RetVal = true;
	}

	public void on_quit1_activate (object o, EventArgs ea)
	{
		Environment.Exit (0);
		Quit ();
	}


	public void on_new_collection_activate (object o, EventArgs ea)
	{
		if (repo == null)
			return;

		IImageCollection c = repo.CreateCollection ();
		SelectCollection (c);
	}

	public void on_ncollection_activate (object o, EventArgs ea)
	{
		Console.WriteLine ("ncoll");
		if (repo == null)
			return;

		IImageCollection c = repo.CreateCollection ();
		SelectCollection (c);
	}

	public void on_import_directory_activate (object o, EventArgs ea)
	{
		Console.WriteLine ("import_directory");
		if (repo == null || collection == null)
			return;

		Console.WriteLine ("dialog");
		Dialog d = (Dialog) gxml["add_directory_dialog"];
		FileEntry entered_dir = (FileEntry) gxml["add_directory_entry"];

		d.Modal = false;

		int buttonPressed = d.Run ();
		if (buttonPressed == (int) ResponseType.Ok) {
			Console.WriteLine ("Importing from: " + entered_dir.Filename);

			DirectoryImageImporter di = new DirectoryImageImporter ();
			di.ImportUri (entered_dir.Filename, collection);
		} else {
			Console.WriteLine ("Cancelled");
		}
		d.Hide ();
	}

	public void on_cut1_activate (object o, EventArgs ea)
	{
		DoEditCopy ();
		DoEditDelete ();
	}

	public void on_copy1_activate (object o, EventArgs ea)
	{
		DoEditCopy ();
	}

	public void on_paste1_activate (object o, EventArgs ea)
	{
		DoEditPaste ();
	}

	public void on_clear1_activate (object o, EventArgs ea)
	{
		DoEditDelete ();
	}

	public void on_properties1_activate (object o, EventArgs ea)
	{
	}

	public void on_preferences1_activate (object o, EventArgs ea)
	{
	}

	public void on_about1_activate (object o, EventArgs ea)
	{
		Console.WriteLine ("About!");
		Dialog about_box =
		new Gnome.About ("MPhoto",
				 "0.0",
				 "Copyright (C) 2002  Ximian, Inc.\nCopyright (C) 2002 Vladimir Vukicevic",
				 null,
				 new string[] { "Vladimir Vukicevic <vladimir@pobox.com>",
						"Miguel de Icaza <miguel@ximian.com> (IconList widget)",
						"Ravi Pratap <ravi@ximian.com> (IconList selection)" },
				 new string[] { },
				 null,
				 new Gdk.Pixbuf (Gdk.Colorspace.Rgb, false, 8, 32, 32));
		Console.WriteLine ("About!");
		about_box.Show ();
		Console.WriteLine ("About!");
		about_box.Run ();
	}

	public void on_button_zoom_in_activate (object o, EventArgs ea)
	{
		if (top_level_notebook.CurrentPage == 0) {
			// icon view
			float new_zoom = icon_list.Zoom + 0.1f;
			if (new_zoom > 2.0f)
				new_zoom = 2.0f;
			// this should be handled by the icon_view
			label_zoom_value.Text = (int) (new_zoom * 100.0f) + "%";
			icon_list.Zoom = new_zoom;
		} else if (top_level_notebook.CurrentPage == 1) {
			// preview
#if HAVE_LIBEOG
			EogImageView view = image_ui.ImageView;
			if (view.Pixbuf.Handle == IntPtr.Zero)
				return;

			view.Zoom += 0.1;
			label_zoom_value.Text = (int) (view.Zoom * 100.0f) + "%";
#endif
		}
	}

	public void on_button_zoom_out_activate (object o, EventArgs ea)
	{
		if (top_level_notebook.CurrentPage == 0) {
			// icon view
			float new_zoom = icon_list.Zoom - 0.1f;
			if (new_zoom < 0.1f)
				new_zoom = 0.1f;
			label_zoom_value.Text = (int) (new_zoom * 100.0f) + "%";
			icon_list.Zoom = new_zoom;
		} else if (top_level_notebook.CurrentPage == 1) {
#if HAVE_LIBEOG
			EogImageView view = image_ui.ImageView;
			if (view.Pixbuf.Handle == IntPtr.Zero)
				return;

			view.Zoom -= 0.1;
			label_zoom_value.Text = (int) (view.Zoom * 100.0f) + "%";
#endif
		}
	}

	// search stuff
	public void on_button_find_clicked (object o, EventArgs ea)
	{
		if (search_dialog == null)
			search_dialog = (Gtk.Window) gxml["search_window"];

		search_dialog.Show ();
	}

	public void on_search_button_save_activate (object o, EventArgs ea)
	{
		Console.WriteLine ("Save Search");
	}

	public void on_search_button_close_activate (object o, EventArgs ea)
	{
		Console.WriteLine ("Close");
		if (search_dialog != null)
			search_dialog.Hide ();
	}

	public void on_search_button_find_activate (object o, EventArgs ea)
	{
		Gtk.TextView tv = (Gtk.TextView) gxml["search_simple_keywords_view"];
		Gtk.TextBuffer tb = tv.Buffer;
		string kwstring = tb.GetText (tb.StartIter, tb.EndIter, false).Trim ();

//        Console.WriteLine("kwstring: " + kwstring);

		if (kwstring == "")
			return;

		string[] kws = kwstring.Split(',', '\n', '\r');

		if (kws.Length <= 0)
			return;

		for (int i = 0; i < kws.Length; i++) {
			kws[i] = kws[i].Trim ();
			Console.WriteLine ("kw " + i + ": " + kws[i]);
		}
		IIconListAdapter search_adapter = new SimpleSearchIconListAdapter
            (repo, kws);
        icon_list.Adapter = search_adapter;
    }

    //
    // event handlers
    //

    public void IconlistActivated (object o, EventArgs ea)
    {
        int c = icon_list.Selection.Count;
        int first_set = -1;
        for (int i = 0; i < c; i++) {
            if (icon_list.Selection[i]) {
                if (first_set == -1) {
                    first_set = i;
                }
            }
        }

        if (first_set >= 0) {
            string imageid = ((CollectionIconListAdapter) icon_list.Adapter).GetImageID (first_set);
            ImageItem iitem = collection[imageid];
#if HAVE_LIBEOG
            image_ui.ImageView.Pixbuf = iitem.Image;
            image_ui.ZoomFit ();
            this.top_level_notebook.CurrentPage = 1;
#endif
        }
    }

    public void CollectionsTreeViewActivateHandler (object o, RowActivatedArgs row_act_args)
    {
        string collid = collections_tree_view.Store.GetCollectionAtPath (row_act_args.Path);
        IImageCollection coll = repo.GetCollection (collid);
        if (coll == null) {
            Console.WriteLine ("collection " + collid + " not found ");
            return;
        }

        SelectCollection (coll);
    }

    // edit menu things

    public void DoEditCopy ()
    {
        Console.WriteLine ("DoEditCopy: ");
        Gtk.Clipboard cb = Gtk.Clipboard.Get (Gdk.Atom.Intern ("PRIMARY", false));

        Gtk.TargetEntry[] entries = new Gtk.TargetEntry[5];
        entries[0].Target = "STRING";
        entries[0].Flags = 0;
        entries[0].Info = 0;
        entries[1].Target = "TEXT";
        entries[1].Flags = 0;
        entries[1].Info = 0;
        entries[2].Target = "COMPOUND_TEXT";
        entries[2].Flags = 0;
        entries[2].Info = 0;
        entries[3].Target = "UTF8_TEXT";
        entries[3].Flags = 0;
        entries[3].Info = 0;
        entries[4].Target = "MPHOTO_IMAGEID_LIST";
        entries[4].Flags = 0;
        entries[4].Info = 1;


        cb_currently_selected.Clear ();

        for (int i = 0; i < icon_list.Selection.Count; i++) {
            if (icon_list.Selection[i]) {
                cb_currently_selected.Add (icon_list.Adapter.GetImageID (i));
                Console.WriteLine ("   adding: " + icon_list.Adapter.GetImageID (i));
            }
        }

        // cb.Set (entries, new Gtk.ClipboardGetFunc (CbGetFunc), new Gtk.ClipboardClearFunc (CbClearFunc), null);
    }

    public void DoEditPaste ()
    {
        Gtk.Clipboard cb = Gtk.Clipboard.Get (Gdk.Atom.Intern ("PRIMARY", false));

        Gtk.SelectionData sd = cb.WaitForContents (Gdk.Atom.Intern ("MPHOTO_IMAGEID_LIST", false));

        string s = sd.Text;

        Console.WriteLine ("PASTE: '" + s + "'");
        string[] imageids = s.Split (';');

        collection.FreezeUpdates ();
        foreach (string iid in imageids) {
            if (iid.Length == 0) continue;
            collection.AddItem (iid);
        }
        collection.ThawUpdates ();
    }

    public void DoEditDelete ()
    {
        ArrayList to_delete = new ArrayList ();
        for (int i = 0; i < icon_list.Selection.Count; i++) {
            if (icon_list.Selection[i]) {
                to_delete.Add (icon_list.Adapter.GetImageID (i));
            }
        }

        collection.FreezeUpdates ();
        foreach (string iid in to_delete) {
            collection.DeleteItem (iid);
        }
        collection.ThawUpdates ();
    }

    // clipboard callbacks
    static ArrayList cb_currently_selected = new ArrayList ();

    void CbGetFunc (Gtk.Clipboard cb, ref Gtk.SelectionData sd, uint info, object o)
    {
        if (info == 0) {
            // string
            // so we want to return filenames

            StringBuilder sb = new StringBuilder ();
            foreach (string iid in cb_currently_selected) {
                sb.Append (repo.GetImage (iid).FullFilename);
                sb.Append (" ");
            }

            sd.Text = sb.ToString ();
        } else if (info == 1) {
            // mphoto_imageid_list
            StringBuilder sb = new StringBuilder ();
            foreach (string iid in cb_currently_selected) {
                sb.Append (iid);
                Console.WriteLine (" cb append: " + iid);
                sb.Append (";");
            }
            // we cheat here and tell it to just set it as a string
            sd.Text = sb.ToString ();
        }
    }

    void CbClearFunc (Gtk.Clipboard cb, object o)
    {
    }

	void OnPageChange (object sender, uint page)
	{
		if (page == 0) {
			label_zoom_value.Text = (int) (icon_list.Zoom * 100.0f) + "%";
		} else if (page == 1) {
#if HAVE_LIBEOG
			EogImageView view = image_ui.ImageView;
			label_zoom_value.Text = (int) (view.Zoom * 100.0f) + "%";
#endif
		} else {
			Console.WriteLine ("Got page: {0}!?", page);
		}
	}

	void OnSwitchPage (object sender, SwitchPageArgs args)
	{
		OnPageChange (sender, args.PageNum);
	}
}
