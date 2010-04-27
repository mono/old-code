//
// SqlSharpGtk - Mono SQL Query For GTK#
//
// Author:
//     Daniel Morgan <monodanmorg@yahoo.com>
//
// (C)Copyright 2004 by Daniel Morgan
//
// To be included with Mono as a SQL query tool licensed under the GPL license.
//

using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Drawing;
using System.Text;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.InteropServices;
using System.Diagnostics;

using Gdk;
using Gtk;

namespace Mono.Data.SqlSharp.GtkSharp 
{
	public class DataTree : VBox
	{
		private TreeStore _treeStore = null;
		private TreeView _treeView = null;
		private TreeIter _rootIter;

		public DataTree () : base(false, 4) 
		{		
			ScrolledWindow sw = new ScrolledWindow ();
			this.PackStart (sw, true, true, 0);

			NewTreeStore ();
			
			_treeView = new TreeView (_treeStore);	
			_treeView.AppendColumn ("Connections", new CellRendererText (), "text", 0);
			_treeView.HeadersVisible = false;

			sw.Add (_treeView);
		}

		public TreeIter RootIter 
		{
			get 
			{
				return _rootIter;
			}
		}

		public TreeStore Store 
		{
			get 
			{
				return _treeStore;
			}
		}

		public TreeView View 
		{
			get 
			{
				return _treeView;
			}
		}

		private void NewTreeStore() 
		{
			// objectDisplayName, status, ObjectOwner, ObjectName, ObjectSubName
			_treeStore = new TreeStore (typeof (string), typeof(string), typeof(string), typeof(string), typeof(string));
			if (_treeView != null)
				_treeView.Model = _treeStore;
			_rootIter = _treeStore.AppendValues ("Connections", "");
		}

		public void Clear () 
		{
			NewTreeStore ();
		}
	}
}

