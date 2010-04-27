using System;
using GLib;
using Gtk;
using GtkSharp;
using System.IO;

public class ImageInfoStoreTwo : TreeStore {
	public ImageInfoStoreTwo ()
		: base ((int)TypeFundamentals.TypeString,
			(int)TypeFundamentals.TypeString)
	{

	}
}
