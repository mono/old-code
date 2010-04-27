
//
// This is just a test for the Icon List widget, so that it can be run
// without the surrounding gnome env (esp under windows)
//

using Gtk;
using GtkSharp;
using System;
using System.Drawing;

public class GtkMphoto {
	public static int Main (string[] args) {
//		Window topwin;
		IconList icon_list;
		IImageCollection collection;
		IIconListAdapter adapter;

		if (args.Length == 0) {
			Console.WriteLine ("Usage: GtkMphoto /path/to/files");
			return 0;
		}

		Application.Init ();

		Window topwin = new Window ("GtkMphoto");
		topwin.DefaultSize = new Size (600, 400);
		topwin.DeleteEvent += new DeleteEventHandler (Window_Delete);

		icon_list = new IconList ();
		Scrollbar vsb = new VScrollbar (icon_list.Adjustment);
		Box box = new HBox (false, 0);
		box.PackStart (icon_list, true, true, 0);
		box.PackStart (vsb, false, true, 0);

		box.ShowAll ();
		topwin.Add (box);

		IImageRepository repo = new DirImageRepository (args);

		string[] collIds = repo.GetCollectionIDs();
		string firstcid = collIds [0];
		collection = repo.GetCollection (firstcid);

		icon_list.Adapter = new CollectionIconListAdapter (collection);

		topwin.Show ();
		Application.Run ();
		return 0;
	}

	static void Window_Delete (object obj, DeleteEventArgs args)
	{
		Application.Quit ();
		args.RetVal = true;
	}
}
