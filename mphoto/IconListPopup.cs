/*
 * IconListPopup.cs
 *
 * Author(s): Vladimir Vukicevic <vladimir@pobox.com>, Miguel de Icaza <miguel@ximian.com>
 *
 * Copyright (C) 2002  Vladimir Vukicevic
 *
 */

using System;
using System.Text;
using System.Collections;
using Gtk;
using Gdk;

public class IconListPopup {
	IconList icon_list;
	int item_clicked;
	
	public IconListPopup () {
	}

	public IconListPopup (IconList il, int item_clicked)
	{
		this.item_clicked = item_clicked;
		IconList = il;
	}

	public IconList IconList {
		get {
			return icon_list;
		}
		set {
			icon_list = value;
		}
	}

	public void Activate (Gdk.EventButton eb)
	{
		Gtk.Menu popup_menu = new Gtk.Menu ();
		bool have_selection = true;
		bool have_multi = false;

		if (icon_list.CountSelected <= 0) {
			have_selection = false;
		} else if (icon_list.CountSelected > 1) {
			have_multi = true;
		}

		if (icon_list.CountSelected > 0) {
			GtkUtil.MakeMenuItem (popup_menu, "Copy Image Location", new EventHandler (Action_CopyImageLocation));
			GtkUtil.MakeMenuItem (popup_menu, "Remove Image", new EventHandler (Action_RemoveImage));
			GtkUtil.MakeMenuSeparator (popup_menu);
		}
		
		GtkUtil.MakeMenuItem (popup_menu, (have_multi ? "Cut Images" : "Cut Image"),
				      new EventHandler (Action_CutImage), false);
		GtkUtil.MakeMenuItem (popup_menu, (have_multi ? "Copy Images" : "Copy Image"),
				      new EventHandler (Action_CopyImage), have_selection);
		GtkUtil.MakeMenuItem (popup_menu, "Paste Images",
				      new EventHandler (Action_PasteImage), true);
		GtkUtil.MakeMenuItem (popup_menu, (have_multi ? "Delete Images" : "Delete Image"),
				      new EventHandler (Action_DeleteImage), have_selection);
#if false
		GtkUtil.MakeMenuSeparator (popup_menu);
		GtkUtil.MakeMenuItem (popup_menu, (have_multi ? "Rotate Images CW" : "Rotate Image CW"),
				      new EventHandler (Action_RotateImageCW), have_selection);
		GtkUtil.MakeMenuItem (popup_menu, (have_multi ? "Rotate Images CCW" : "Rotate Image CCW"),
				      new EventHandler (Action_RotateImageCCW), have_selection);
		GtkUtil.MakeMenuItem (popup_menu, (have_multi ? "Rotate Images 180" : "Rotate Image 180"),
				      new EventHandler (Action_RotateImage180), have_selection);
#endif
		
		popup_menu.Popup (null, null, null, IntPtr.Zero, eb.Button, eb.Time);
	}

	void Action_CutImage (object o, EventArgs ea)
	{
		MphotoToplevel.GlobalMphotoToplevel.DoEditCopy ();
		MphotoToplevel.GlobalMphotoToplevel.DoEditDelete ();
	}

	void Action_CopyImage (object o, EventArgs ea)
	{
		MphotoToplevel.GlobalMphotoToplevel.DoEditCopy ();
	}

	void Action_PasteImage (object o, EventArgs ea)
	{
		MphotoToplevel.GlobalMphotoToplevel.DoEditPaste ();
	}

	void Action_DeleteImage (object o, EventArgs ea)
	{
		MphotoToplevel.GlobalMphotoToplevel.DoEditDelete ();
	}

	void Action_CopyImageLocation (object o, EventArgs a)
	{
		Clipboard clipboard = Clipboard.Get (Atom.Intern ("PRIMARY", false));
		
		string name = System.IO.Path.GetFullPath (IconList.Adapter.GetFullFilename (item_clicked));
		clipboard.SetText (name);
		
	}

	void Action_RemoveImage (object o, EventArgs a)
	{
		IconList.Adapter.DeleteItem (item_clicked);
	}
	
	void Action_RotateImageCW (object o, EventArgs ea)
	{
	}

	void Action_RotateImageCCW (object o, EventArgs ea)
	{
	}

	void Action_RotateImage180 (object o, EventArgs ea)
	{
	}
}
