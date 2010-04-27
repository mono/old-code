//
// ImageBrowser.cs - An image browser written in C#
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) 2002 Copyright, Ximian, Inc.
//

using Gtk;
using Gdk;
using GtkSharp;
using System;
using System.Collections;
using System.IO;

public class ImageViewer
{
	static void Main (string [] args)
	{
		Application.Init ();

		if (args.Length <= 0) {
			Console.WriteLine ("\nUSAGE: ImageBrowser.exe <directory>\n");
			return;
		}
	
		string dir = args [0];

		Gtk.Window window = new Gtk.Window ("Image Browser");
		Gtk.ScrolledWindow scroll = new Gtk.ScrolledWindow (new Adjustment (IntPtr.Zero), new Adjustment (IntPtr.Zero));

		ArrayList images = GetItemsFromDirectory (dir);
		
		Gtk.Table table = PopulateTable (images);
		
		window.Title = String.Format ("{0}: {1} ({2} images)", window.Title, dir, images.Count);
		window.SetDefaultSize (300, 200);
		window.DeleteEvent += Window_Delete;
		scroll.AddWithViewport (table);
		scroll.SetPolicy (PolicyType.Automatic, PolicyType.Automatic);
		window.Add (scroll);
		window.ShowAll ();
		Application.Run ();
	}

	public static Gtk.Table PopulateTable (ArrayList items)
	{
		Gtk.Table table = new Gtk.Table (0, 0, false);
		uint x = 0;
		uint y = 0;
		
		foreach (object item in items) {
			if (x > 4) {
				x = 0;
				y++;
			}
			table.Attach (((Gtk.Widget) item), x, x + 1, y, y + 1, AttachOptions.Fill, AttachOptions.Fill, 5, 5);
			x++;
		}

		return table;
	}

	public static ArrayList GetItemsFromDirectory (string directory)
	{
		DirectoryInfo dir_info = new DirectoryInfo (directory);

		if (dir_info.Exists == false)
			throw new DirectoryNotFoundException (directory + " does not exist.");

		FileInfo [] files = dir_info.GetFiles ();

		ArrayList file_items = new ArrayList (files.Length);

		int i = 0;
		foreach (FileInfo fi in files) {
			if (!fi.Name.EndsWith ("png") &&
			    !fi.Name.EndsWith ("jpg") &&
			    !fi.Name.EndsWith ("jpeg") &&
			    !fi.Name.EndsWith ("gif"))
				continue;
			else {
				file_items.Add (new ImageFileBox (fi));
				i++;
			}
		}

		return file_items;
	}

	static void Window_Delete (object o, EventArgs args)
	{
		//SignalArgs s = (SignalArgs) args;
		Application.Quit ();
		//s.RetVal = true;
	}
}

class ImageFileFrame : Gtk.Frame
{
	public ImageFileFrame (FileInfo fileInfo)
		: base (fileInfo.Name)
	{
		string filename = String.Format ("{0}/{1}", fileInfo.DirectoryName, fileInfo.Name);

		Pixbuf pixbuf = new Pixbuf (filename);
		pixbuf = pixbuf.ScaleSimple (64, 64, InterpType.Bilinear);
 		
		Gtk.Frame inner_frame = new Gtk.Frame (String.Empty);
		
		Gtk.Image image = new Gtk.Image (pixbuf);
		
		inner_frame.Add (image);
		inner_frame.ShadowType = ShadowType.In;
		inner_frame.BorderWidth = 10;
		this.Add (inner_frame);
		this.ShadowType = ShadowType.Out;
	}		
}

class ImageFileBox : Gtk.VBox
{
	public ImageFileBox (FileInfo fileInfo)
		: base (false, 0)
	{
		string filename = String.Format ("{0}/{1}", fileInfo.DirectoryName, fileInfo.Name);

		Pixbuf pixbuf = new Pixbuf (filename);
		pixbuf = pixbuf.ScaleSimple (64, 64, InterpType.Bilinear);

		Gtk.Image image = new Gtk.Image (pixbuf);
		Gtk.Label file_label = new Gtk.Label (fileInfo.Name);

		this.PackStart (image, false, false, 0);
		this.PackStart (file_label, false, false, 0);
	}
}
