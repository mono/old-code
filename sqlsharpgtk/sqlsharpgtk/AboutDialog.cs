// AboutDialog.cs
//
// Author:
//     Daniel Morgan <monodanmorg@yahoo.com>
//
// (C)Copyright 2004-2006 by Daniel Morgan
//
// To be included with Mono as a SQL query tool licensed under the GPL license.
//

namespace Mono.Data.SqlSharp.GtkSharp 
{
	using Gtk;
	using Pango;
	using System;
	using System.Collections;
	using System.Collections.Specialized;
	using System.Configuration;
	using System.Data;
	using System.Diagnostics;
	using System.Drawing;
	using System.IO;
	using System.Text;
	using System.Reflection;
	using System.Runtime.Remoting;

	public class AboutDialog 
	{	
		public string VERSION = "0.4";

		Dialog dialog;

		public AboutDialog () 
		{ 
			CreateGui ();
		}

		public void CreateGui() 
		{
			dialog = new Dialog ();
			
			dialog.AllowGrow = true;
			dialog.Title = "About";
			dialog.BorderWidth = 3;
			dialog.VBox.BorderWidth = 5;
			dialog.HasSeparator = false;
		
			Table table = new Table (4, 1, false);
			table.ColumnSpacing = 4;
			table.RowSpacing = 4;
			Label label = null;
			
			label = new Label ("About Mono SQL# For GTK#");
			table.Attach (label, 0, 1, 0, 1);

			label = new Label ("sqlsharpgtk");
			table.Attach (label, 0, 1, 1, 2);

			label = new Label (VERSION);
			table.Attach (label, 0, 1, 2, 3);

			label = new Label ("(C) Copyright 2002-2006 Daniel Morgan");
			table.Attach (label, 0, 1, 3, 4);

			table.Show();

			dialog.VBox.PackStart (table, false, false, 10);

			Button button = null;
			button = new Button (Stock.Ok);
			button.Clicked += new EventHandler (Ok_Action);
			button.CanDefault = true;
			dialog.ActionArea.PackStart (button, true, true, 0);
			button.GrabDefault ();

			dialog.Modal = true;

			dialog.ShowAll ();
		}

		void Ok_Action (object o, EventArgs args) 
		{
			dialog.Destroy ();
			dialog = null;
		}
	}
}

