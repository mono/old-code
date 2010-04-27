//
// ImageViewer.cs: A Image Viewer written in C#
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// Copyright (C) 2002, Ximian. Inc.
//

using Gtk;
using Gdk;
using GtkSharp;
using Gdk.Imaging;
using System;

public class ImageViewer {

	static Gtk.Window window = null;
	static Gtk.FileSelection selections = null;
	static Gtk.Image image = null;
	static Gtk.Dialog about = null;

	public static void Main (string [] args)
	{
		if (args.Length <= 0) {
			Console.WriteLine ("\nUSAGE: ImageViewer.exe <filename>\n");
			return;
		}
		
		string filename = args [0];
		Application.Init ();
		window = new Gtk.Window ("Image Viewer");
		window.SetDefaultSize (200, 200);
		
		window.DeleteEvent += new EventHandler (Window_Delete);

		Gtk.ScrolledWindow scrolled_window = new Gtk.ScrolledWindow (new Adjustment (IntPtr.Zero), new Adjustment (IntPtr.Zero));

		Gtk.VBox vbox = new VBox (false, 2);
		Gtk.VBox menubox = new VBox (false, 0);

		// Pack menubar
		MenuBar mb = new MenuBar ();
		
		Menu file_menu = new Menu ();		
		MenuItem exit_item = new ImageMenuItem ("gtk-close", new Gtk.AccelGroup (IntPtr.Zero));
		MenuItem open_item = new ImageMenuItem ("gtk-open", new Gtk.AccelGroup (IntPtr.Zero));
		exit_item.Activated += new EventHandler (Window_Delete);
		open_item.Activated += new EventHandler (Window_Open);

		file_menu.Append (open_item);
		file_menu.Append (new Gtk.SeparatorMenuItem ());
		file_menu.Append (exit_item);
		MenuItem file_item = new MenuItem ("_File");
		file_item.Submenu = file_menu;

		mb.Append (file_item);
		menubox.PackStart (mb, false, false, 0);

		// Pack toolbar
		Gtk.Toolbar toolbar = new Gtk.Toolbar ();
		toolbar.InsertStock ("gtk-open", "Open", String.Empty, new Gtk.SignalFunc (Window_Open), IntPtr.Zero, 0);
		toolbar.InsertStock ("gtk-close", "Close", String.Empty, new Gtk.SignalFunc (Window_Delete), IntPtr.Zero, 1);		
		menubox.PackStart (toolbar, false, false, 0);
		vbox.PackStart (menubox, false, false, 0);

		Pixbuf pix = GetPixbufFromFile (filename);
		image = new Image (pix);

		Refresh (filename, pix);
		
		scrolled_window.AddWithViewport (image);
		vbox.PackStart (scrolled_window, true, true, 0);

		scrolled_window.SetPolicy (PolicyType.Automatic, PolicyType.Automatic);
		window.Add (vbox);
		window.ShowAll ();
		
		Application.Run ();
	}

	static void Refresh (string new_filename, Gdk.Pixbuf p)
	{
		window.Resize (p.Width + 25, p.Height + 75);
		window.Title = String.Format ("Image Viewer: {0}", new_filename);
	}

	static Gdk.Pixbuf GetPixbufFromFile (string filename)
	{
		try {
			Pixbuf p = new Pixbuf (filename);
			return p;

		} catch (GLib.GException e) {
			Console.WriteLine (e.GetType ());
			Console.WriteLine ("Cannot Open file.");
			Environment.Exit (1);
			return null;
		}
		
	}

	static void Window_Delete (object o, EventArgs args)
	{
		SignalArgs s = (SignalArgs) args;
		Application.Quit ();
		s.RetVal = true;
	}

	static void Window_Open (object o, EventArgs args)
	{
		Window_Open ();
	}

	static void Window_Delete ()
	{
		Application.Quit ();
	}

	static void Window_Open ()
	{
		selections = new Gtk.FileSelection ("Open... ");
		selections.Modal = true;
		selections.OkButton.Clicked += new EventHandler (OK_Clicked);
		selections.CancelButton.Clicked += new EventHandler (Cancel_Clicked);
		selections.ShowAll ();
	}

	static void OK_Clicked (object o, EventArgs args)
	{
		Pixbuf p = GetPixbufFromFile (selections.Filename);
		image.Pixbuf = p;

		Refresh (selections.Filename, p);
		
		selections.Hide ();
	}

	static void Cancel_Clicked (object o , EventArgs args)
	{
		selections.Hide ();
	}		
}

