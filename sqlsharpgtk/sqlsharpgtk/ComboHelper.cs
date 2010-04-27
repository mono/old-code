//
// ComboHelper.cs - provide easy functions / properties for handling a Gtk.ComboBox
//
// Authors:
//     Daniel Morgan <monodanmorg@yahoo.com>
//
// (c)copyright 2005 Daniel Morgan
//

namespace Mono.Data.SqlSharp.GtkSharp 
{
	using System;
	using GLib;
	using Gtk;

	public class ComboHelper : ComboBox 
	{
		public static ComboBox NewComboBox () 
		{
			return ComboBox.NewText ();
		}

		public static string GetActiveText(ComboBox cbox) 
		{
			if (cbox.Active < 0)
				return "";

			TreeIter iter;
			cbox.GetActiveIter (out iter);
			string cvalue = ((ListStore) cbox.Model).GetValue (iter, 0).ToString();
			return cvalue;
		}

		public static bool SetActiveText (ComboBox cbox, string text) 
		{
			// returns true if found, false if not found

			string tvalue;
			TreeIter iter;
			ListStore store = (ListStore) cbox.Model;

			store.IterChildren (out iter);
			tvalue  = store.GetValue (iter, 0).ToString();
			if (tvalue.Equals (text)) {
				cbox.SetActiveIter (iter);
				return true;
			}
			else {
				bool found = store.IterNext (ref iter);
				while (found == true) {
					tvalue = store.GetValue (iter, 0).ToString();
					if (tvalue.Equals (text)) {
						cbox.SetActiveIter (iter);
						return true;
					}
					else
						found = store.IterNext (ref iter);
				}
			}

			return false; // not found
		}
	}
}


