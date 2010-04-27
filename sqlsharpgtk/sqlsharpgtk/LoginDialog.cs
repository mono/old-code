// LoginDialog.cs
//
// Author:
//     Daniel Morgan <monodanmorg@yahoo.com>
//
// (C)Copyright 2002-2006 by Daniel Morgan
//
// To be included with Mono as a SQL query tool licensed under the GPL license.
//

namespace Mono.Data.SqlSharp.GtkSharp 
{
	using Gtk;
	using Pango;
	using Mono.Data;
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

	public class LoginDialog 
	{	
		static readonly int COL_Setting = 0;
		static readonly int COL_Provider = 1;
		static readonly int COL_Server = 2;
		static readonly int COL_Database = 3;
		static readonly int COL_UserID = 4;
		static readonly int COL_Options = 5;
		static readonly int COL_ConnectionString = 6;

		Dialog dialog;
		Entry connection_entry;
		Entry server_entry;
		Entry database_entry;
		Entry userid_entry;
		Entry password_entry;
		Entry other_entry;
		SqlSharpGtk sqlSharp;
		ComboBox providerCombo;

		int providerSelected = 0;
		Provider[] providers;
		DataGrid grid;
		Statusbar statusBar;

		string serverName = "";
		string databaseName = "";
		string useridName = "";
		string passwordName = "";

		string selectedSetting = "";

		public LoginDialog (SqlSharpGtk sqlSharpGtk) 
		{ 
			sqlSharp = sqlSharpGtk;
			
			grid = new DataGrid ();
			providerCombo = ComboBox.NewText ();

			PopulateProviders ();
			PopulateAppSettings ();
			
			CreateGui ();
			
			SetStatusBarText ("Ready.");
		}

		public void PopulateProviders () 
		{
			if (ProviderFactory.Providers.Count == 0)
				Console.Error.WriteLine ("Provider Count is 0.  Need to verify you have a sqlsharpgtk.exe.config file and that Mono.Data.dll can be found.");
			providers = new Provider[ProviderFactory.Providers.Count];
			int p = 0;
			foreach(Provider provider in ProviderFactory.Providers) 
			{
				providers[p] = provider;
				p++;
			}
		}

		public void PopulateAppSettings () 
		{
			int columnCount = 8;

			grid.SetColumnCount (columnCount);

			TreeViewColumn tvc;
			
			tvc = grid.CreateColumn (COL_Setting, "Setting");
			grid.View.AppendColumn (tvc);

			tvc = grid.CreateColumn (COL_Provider, "Provider");
			grid.View.AppendColumn (tvc);

			tvc = grid.CreateColumn (COL_Server, "Server");
			grid.View.AppendColumn (tvc);

			tvc = grid.CreateColumn (COL_Database, "Database");
			grid.View.AppendColumn (tvc);

			tvc = grid.CreateColumn (COL_UserID, "User ID");
			grid.View.AppendColumn (tvc);

			tvc = grid.CreateColumn (COL_Options, "Options");
			grid.View.AppendColumn (tvc);

			tvc = grid.CreateColumn (COL_ConnectionString, "ConnectionString");
			grid.View.AppendColumn (tvc);

			NameValueCollection settings = ConfigurationSettings.AppSettings;

			int num = 1;
			foreach ( String sName in settings.AllKeys )  
			{
				string connectionString = settings[sName];

				ConnectionString conString = new ConnectionString (connectionString);

				string factory = conString.Parameters["FACTORY"];

				string server = conString.Parameters["SERVER"];
				if (server == null)
					server = conString.Parameters["DATA SOURCE"];
				if (server == null)
					server = conString.Parameters["NETWORK ADDRESS"];
				if (server == null)
					server = conString.Parameters["ADDRESS"];
				if (server == null)
					server = conString.Parameters["ADDR"];

				string database = conString.Parameters["DATABASE"];
				if (database == null)
					database = conString.Parameters["INITIAL CATALOG"];

				string userid = conString.Parameters["USER ID"];
				if (userid == null)
					userid = conString.Parameters["USER"];
				if (userid == null)
					userid = conString.Parameters["UID"];

				string otherOptions = conString.GetOtherOptions ();

				TreeIter row = grid.NewRow ();

				grid.SetColumnValue (row, COL_Setting, sName);
				grid.SetColumnValue (row, COL_Provider, factory);
				grid.SetColumnValue (row, COL_Server, server);
				grid.SetColumnValue (row, COL_Database, database);
				grid.SetColumnValue (row, COL_UserID, userid);
				grid.SetColumnValue (row, COL_Options, otherOptions);
				grid.SetColumnValue (row, COL_ConnectionString, connectionString);

				num ++;
			}

			grid.View.Model = grid.Store;

			grid.View.Selection.Changed += new EventHandler (OnSelectionChanged);
		}

		public void CreateGui() 
		{
			dialog = new Dialog ();
			
			dialog.AllowGrow = true;
			dialog.Title = "Login";
			dialog.BorderWidth = 2;
			dialog.VBox.BorderWidth = 2;
			dialog.HasSeparator = false;

			Frame frame = new Frame ("Connection");
			frame.BorderWidth = 2;
		
			Table table = new Table (7, 2, false);
			table.ColumnSpacing = 2;
			table.RowSpacing = 2;
			Label label = null;

			label = new Label ("_Provider");
			label.Xalign = 1.0f;
			label.Ypad = 8;
			table.Attach (label, 0, 1, 0, 1, AttachOptions.Fill, AttachOptions.Fill, 1, 1);
			LoadProviderCombo();
			if (providerCombo.Model.IterNChildren() > 0)
				providerCombo.Active = 0;
			providerSelected = providerCombo.Active;
			table.Attach (providerCombo, 1, 8, 0, 1, AttachOptions.Fill, AttachOptions.Fill, 10, 1);
						
			label = new Label ("_Connection String");
			label.Xpad = 2;
			label.Ypad = 8;
			label.Xalign = 1.0f;
			table.Attach (label, 0, 1, 1, 2, AttachOptions.Fill, AttachOptions.Fill, 1, 1);
			connection_entry = new Entry ();
			connection_entry.Changed += new EventHandler (OnConnectionEntryChanged);
			table.Attach (connection_entry, 1, 8, 1, 2, AttachOptions.Fill, AttachOptions.Fill, 10, 1);
			
			label = new Label ("_Server");
			label.Xalign = 1.0f;
			label.Ypad = 8;
			table.Attach (label, 0, 1, 2, 3, AttachOptions.Fill, AttachOptions.Fill, 1, 1);
			server_entry = new Entry ();
			server_entry.Changed += new EventHandler (OnParameterChanged);
			table.Attach (server_entry, 1, 8, 2, 3, AttachOptions.Fill, AttachOptions.Fill, 10, 1);

			label = new Label ("_Database");
			label.Xalign = 1.0f;
			label.Ypad = 8;
			table.Attach (label, 0, 1, 3, 4, AttachOptions.Fill, AttachOptions.Fill, 1, 1);
			database_entry = new Entry ();
			database_entry.Changed += new EventHandler (OnParameterChanged);
			table.Attach (database_entry, 1, 8, 3, 4, AttachOptions.Fill, AttachOptions.Fill, 10, 1);

			label = new Label ("_User ID");
			label.Xalign = 1.0f;
			label.Ypad = 8;
			table.Attach (label, 0, 1, 4, 5, AttachOptions.Fill, AttachOptions.Fill, 1, 1);
			userid_entry = new Entry ();
			userid_entry.Changed += new EventHandler (OnParameterChanged);
			table.Attach (userid_entry, 1, 8, 4, 5, AttachOptions.Fill, AttachOptions.Fill, 10, 1);

			label = new Label ("_Password");
			label.Xalign = 1.0f;
			label.Ypad = 8;
			table.Attach (label, 0, 1, 5, 6, AttachOptions.Fill, AttachOptions.Fill, 1, 1);
			password_entry = new Entry ();
			password_entry.Visibility = false;
			password_entry.Changed += new EventHandler (OnParameterChanged);
			table.Attach (password_entry, 1, 8, 5, 6, AttachOptions.Fill, AttachOptions.Fill, 10, 1);

			label = new Label ("_Other");
			label.Xalign = 1.0f;
			label.Ypad = 8;
			table.Attach (label, 0, 1, 6, 7, AttachOptions.Fill, AttachOptions.Fill, 1, 1);
			other_entry = new Entry ();
			other_entry.Changed += new EventHandler (OnParameterChanged);
			table.Attach (other_entry, 1, 8, 6, 7, AttachOptions.Fill, AttachOptions.Fill, 10, 1);

			table.Show();
			frame.Add (table);

			dialog.VBox.PackStart (frame, false, false, 5);
			
			Frame appSettingFrame = new Frame ("App Settings");
			appSettingFrame.Add (grid);
			dialog.VBox.PackStart (appSettingFrame, true, true, 10);

			Button button = null;
			button = new Button (Stock.Ok);
			button.Clicked += new EventHandler (Connect_Action);
			button.CanDefault = true;
			dialog.ActionArea.PackStart (button, true, true, 0);
			button.GrabDefault ();

			button = new Button (Stock.Cancel);
			button.Clicked += new EventHandler (Dialog_Cancel);
			dialog.ActionArea.PackStart (button, true, true, 0);
			dialog.Modal = true;
			dialog.SetDefaultSize (500, 500);

			statusBar = new Statusbar ();
			statusBar.HasResizeGrip = false;
			dialog.VBox.PackEnd (statusBar, false, false, 0);

			SetStatusBarText ("Ready!");

			dialog.ShowAll ();
		}

		void OnConnectionEntryChanged (object o, EventArgs args) 
		{
		}

		void OnParameterChanged (object o, EventArgs args) 
		{
			connection_entry.Text = RebuildConnectionString ();
		}

		string RebuildConnectionString () 
		{
			ConnectionString cstr = null;

			if (!other_entry.Text.Equals("")) 
			{
				cstr = new ConnectionString (other_entry.Text);	
			}
			else 
				cstr = new ConnectionString ();

			if (!server_entry.Text.Equals("")) 
			{
				if (serverName.Equals(""))
					serverName = "SERVER";

				cstr.Add (serverName, server_entry.Text);
			}

			if (!database_entry.Text.Equals("")) 
			{
				if (databaseName.Equals(""))
					databaseName = "DATABASE";

				cstr.Add (databaseName, database_entry.Text);
			}

			if (!userid_entry.Text.Equals("")) 
			{
				if (useridName.Equals(""))
					useridName = "USER ID";

				cstr.Add (useridName, userid_entry.Text);
			}

			if (!password_entry.Text.Equals("")) 
			{
				if (passwordName.Equals(""))
					passwordName = "PASSWORD";

				cstr.Add (passwordName, password_entry.Text);
			}

			return cstr.GetConnectionString ();
		}

		public void LoadProviderCombo() 
		{
			for(int i = 0; i < providers.Length; i++) 
				providerCombo.AppendText (providers[i].Name);
			
			providerCombo.Changed += new EventHandler (OnProviderChanged);
		}

		void ParseConnectionStringIntoFields (string val)
		{
			ConnectionString conString = new ConnectionString (val);

			serverName = "";
			databaseName = "";
			useridName = "";
			passwordName = "";
				
			// Provider Name - Factory
			string factory = conString.Parameters["FACTORY"];
			if (factory == null) 
				factory = providers[providerSelected].Name;

			// Server / Data Source / Network Address / Address / Addr
			string server = conString.Parameters["SERVER"];
			if (server == null) 
			{
				server = conString.Parameters["DATA SOURCE"];
				if (server == null) 
				{
					server = conString.Parameters["NETWORK ADDRESS"];
					if (server == null) 
					{
						server = conString.Parameters["ADDRESS"];
						if (server == null) 
						{
							server = conString.Parameters["ADDR"];
							if (server != null)
								serverName = "ADDR";
						}
						else
							serverName = "ADDRESS";
					}
					else
						serverName = "NETWORK ADDRESS";
				}
				else
					serverName = "DATA SOURCE";
			}
			else
				serverName = "SERVER";
				
			// Database / Initial Catalog
			string database = conString.Parameters["DATABASE"];
			if (database == null) 
			{
				database = conString.Parameters["INITIAL CATALOG"];
				if (database != null)
					databaseName = "INITIAL CATALOG";
			}
			else
				databaseName = "DATABASE";
				
			// User ID / UID / User
			string userid = conString.Parameters["USER ID"];
			if (userid == null) 
			{
				userid = conString.Parameters["UID"];
				if (userid == null) 
				{
					userid = conString.Parameters["USER"];
					if (userid != null)
						useridName = "USER";
				}
				else
					useridName = "UID";
			}
			else
				useridName = "USER ID";
				
			// Password / PWD
			string password = conString.Parameters["PASSWORD"];
			if (password == null) 
			{
				password = conString.Parameters["PWD"];
				if (password != null)
					passwordName = "PWD";
			}
			else
				passwordName = "PASSWORD";

			server_entry.Text = server == null ? "" : server;
			database_entry.Text = database == null ? "" : database;
			userid_entry.Text = userid == null ? "" : userid;
			password_entry.Text = password == null ? "" : password;

			// Other Options
			other_entry.Text = conString.GetOtherOptions ();

			connection_entry.Text = conString.GetConnectionString ();

			ComboHelper.SetActiveText (providerCombo, factory);
		}

		void OnSelectionChanged (object o, EventArgs args) 
		{
			selectedSetting = "";

			TreeIter iter;
			TreeModel model;
			TreeSelection selection = grid.View.Selection;

			if (selection.GetSelected (out model, out iter))
			{
				selectedSetting = (string) model.GetValue (iter, COL_Setting);
				string conString = (string) model.GetValue (iter, COL_ConnectionString);
				ParseConnectionStringIntoFields (conString);
			}
		}

		void OnProviderChanged (object o, EventArgs args) 
		{
			providerSelected = providerCombo.Active;
		}

		void Connect_Action (object o, EventArgs args) 
		{
			try {

				SetStatusBarText ("Connecting...");
				sqlSharp.OpenDataSource (providers[providerSelected], connection_entry.Text, selectedSetting);
				SetStatusBarText ("Connected.");
				
			} catch (Exception e) {
				string emsg = "Error: Unable to connect.  Reason: " + e;
				SetStatusBarText (emsg);
				sqlSharp.AppendText (emsg);
			}

			grid.Clear ();
			grid.DataSource = null;
			grid.DataMember = "";
			grid = null;

			dialog.Destroy ();
			dialog = null;
		}

		void Dialog_Cancel (object o, EventArgs args) 
		{
			grid.Clear ();
			grid.DataSource = null;
			grid.DataMember = "";
			grid = null;

			dialog.Destroy ();
			dialog = null;
		}

		void SetStatusBarText (string message) 
		{
			uint statusBarID = 1;

			statusBar.Pop (statusBarID);
			statusBar.Push (statusBarID, message);
		}
	}
}

