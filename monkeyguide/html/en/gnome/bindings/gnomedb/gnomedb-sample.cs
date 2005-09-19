// GnomeDb sample application
//
// Author: Marius Andreiana (marius galuna ro)
//
// Copyright (c) Marius Andreiana, 2003
// License: GPL


using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Drawing;
using Gtk;
using GtkSharp;
using Gnome;
using Glade;
using GnomeDb;
using Gda;


namespace GnomeDbSample {

	public class GnomeDbSample {
		Gnome.Program program;
		Gda.Client client;
		Gda.Connection dbc;
		Gda.DataModel gdm;
		Gda.ParameterList gpl;
		Gda.Command gc;
		
		GnomeDb.Browser db_browser;
		GnomeDb.Form db_form;
		GnomeDb.Grid db_grid;
		GnomeDb.List db_list;
		GnomeDb.Combo db_combo;
		
		[Glade.Widget] Gnome.App app;
		[Glade.Widget] Gtk.VBox vbox;
		
		
		public static void Main( string[] args )
		{
			new GnomeDbSample( args );
		}

		public GnomeDbSample( string[] args ) 
		{
			program = new Program( "gnomedb-sample", "1.0", Modules.UI , args );
			XML ui = new Glade.XML( "gnomedb-sample.glade", "app", null );
			ui.Autoconnect( this );

			db_browser = new GnomeDb.Browser();
			vbox.PackStart( db_browser, true, true, 0 );

			client = new Gda.Client();
			dbc = client.OpenConnection( "monotest", "mono", "mono",
					     Gda.ConnectionOptions.ReadOnly );
			if (dbc == null) {
				Console.WriteLine( "DB connection failed" );
				return;
			}
			db_browser.Connection = dbc;
			gc = new Gda.Command();
			gpl = new Gda.ParameterList();
			
			db_form = new GnomeDb.Form();
			vbox.PackStart( db_form, true, true, 0 );
			
			db_grid = new GnomeDb.Grid();
			vbox.PackStart( db_grid, true, true, 0 );
			
			db_list = new GnomeDb.List();
			vbox.PackStart( db_list, true, true, 0 );
			
			db_combo = new GnomeDb.Combo();
			vbox.PackStart( db_combo, true, true, 0 );

			app.ShowAll();
			program.Run();
		}


		public void on_app_delete_event( object o, DeleteEventArgs args ) 
		{
			close_app();
		}
		
		public void on_menu_quit_activate( object o, EventArgs args ) 
		{
			close_app();
		}
		
		public void on_connect_clicked( System.Object obj, EventArgs e ) 
		{
			GnomeDb.LoginDialog dialog;
			dialog = new GnomeDb.LoginDialog( "Select data source" );
				
			if (dialog.Run() == true) {
				dbc = client.OpenConnection( dialog.Dsn, dialog.Username, dialog.Password,
							     Gda.ConnectionOptions.ReadOnly );
				if (dbc != null) {
					db_browser.Connection = dbc;
				}
			}
			dialog.Destroy();
		}
		
		public void on_insert_clicked( System.Object obj, EventArgs e ) 
		{
			
			string sql = "INSERT INTO customers VALUES ( 10, 'Marius Andreiana', 'Lujerului nr. 6' )";
			gc.text = sql;
			dbc.ExecuteNonQuery( gc, gpl );
		}
		
		public void on_select_clicked( System.Object obj, EventArgs e ) 
		{
			
			string sql = "SELECT * FROM customers";
			gc.text = sql;
			gdm = dbc.ExecuteSingleCommand( gc, gpl );
			db_form.Model = gdm;
			db_grid.Model = gdm;
			
			sql = "SELECT name FROM customers";
			gc.text = sql;
			gdm = dbc.ExecuteSingleCommand( gc, gpl );
			db_list.Model = gdm;
			db_combo.Model = gdm;
		}
		
		public void close_app()
		{
			program.Quit();
		}
	}
}
