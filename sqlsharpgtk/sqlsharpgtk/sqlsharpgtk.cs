//
// SqlSharpGtk - Mono SQL Query For GTK#
//
// Author:
//     Daniel Morgan <monodanmorg@yahoo.com>
//
// (C)Copyright 2002-2007 by Daniel Morgan
//
// To be included with Mono as a SQL query tool licensed under the GPL license.
//

namespace Mono.Data.SqlSharp.GtkSharp 
{
	using System;
	using System.Collections;
	using System.Collections.Specialized;
	using System.Configuration;
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
	
	using Mono.Data;
	using Mono.Data.SqlSharp.DatabaseBrowser;

	public class SqlSharpGtk 
	{
		static int SqlWindowCount = 0;
		EditorTab currentEditorTab;

		private DataSourceConnectionCollection dataSources = null;
		private DataSourceConnection dataSource = null;
		private IDbConnection conn = null;
		public Provider provider = null;
		private Type adapterType = null;
		public Assembly providerAssembly = null;
		public string connectionString = "";	
		
		private Statusbar statusBar;
		private Toolbar toolbar;

		private int lastUnknownFile = 0;
		private int lastConnection = 0;

		private Gtk.Window win;

		public static readonly string ApplicationName = "Mono SQL# For GTK#";
		
		public static readonly string NotConnected = "{Not Connected}";
		public static readonly string NotPopulated = "{Not Populated}";

		private OutputResults outputResults;
		private QueryMode queryMode;

		private Notebook sourceFileNotebook;
		private ArrayList editorTabs = new ArrayList();

		private ComboBox combo;

		private DataTree tree;
		private string selectedDataSource = "";
		private int selectedDepth = 0;
		private string selectedType = "";
		private string selectedObject = "";
		private DataGrid selectedGrid = null;
		private TreeIter selectedIter;

		Tooltips tooltips1  = new Tooltips ();

		public SqlSharpGtk () 
		{
			dataSources = new DataSourceConnectionCollection ();
			CreateGui ();
			SqlWindowCount ++;
		}

		public void Show () 
		{
			win.ShowAll ();
		}

		public void CreateGui () 
		{
			win = new Gtk.Window (ApplicationName);
			win.DeleteEvent += new Gtk.DeleteEventHandler (OnWindow_Delete);
			win.BorderWidth = 4;
			win.SetDefaultSize (600, 500);
			
			VBox vbox = new VBox (false, 4);
			win.Add (vbox);
			
			// Menu Bar
			MenuBar mb = CreateMenuBar ();
			vbox.PackStart (mb, false, false, 0);

			// Tool Bar
			toolbar = CreateToolbar ();
			toolbar.ShowAll();
			vbox.PackStart (toolbar, false, false, 0);
			
			// Panels

			tree = new DataTree (); // meta data tree view (left pane)
			tree.View.ButtonReleaseEvent += new Gtk.ButtonReleaseEventHandler (OnDataTreeButtonRelease);
			tree.View.RowExpanded  += new Gtk.RowExpandedHandler (OnDataTreeRowExpanded);

			// SQL Editor amd Results (right pane)
			outputResults = OutputResults.DataGrid;
			sourceFileNotebook = new Notebook();
			sourceFileNotebook.Scrollable = true;
			NewEditorTab();
			sourceFileNotebook.SwitchPage += new Gtk.SwitchPageHandler(OnEditorTabSwitched);

			HPaned hpaned = new HPaned ();
			vbox.PackStart (hpaned, true, true, 0);
			hpaned.Add1 (tree);
			hpaned.Add2 (sourceFileNotebook);

			statusBar = new Statusbar ();
			statusBar.HasResizeGrip = false;
			SetStatusBarText ("Ready!");
			vbox.PackEnd (statusBar, false, false, 0);

			queryMode = QueryMode.Query;
		}

		void SetStatusBarText (string message) 
		{
			uint statusBarID = 1;

			statusBar.Pop (statusBarID);
			statusBar.Push (statusBarID, message);
		}

        [GLib.ConnectBefore]
		void OnDataGridButtonRelease (object o, ButtonReleaseEventArgs args) 
		{
			EventButton but = args.Event;
			
			if(but.Button == 3) {  /* Right Mouse Button is 3 */
				bool rowsSensitive = false;
				int rows = selectedGrid.RowCount;
				if (rows > 0)
					rowsSensitive = true;

				// popup a menu over the tree view
				Menu menu = new Menu ();
				MenuItem item;

				//item = new MenuItem ("Save As...");
				//item.Activated += new EventHandler (OnGridPopupMenu_SaveAs);
				//item.Sensitive = rowsSensitive;
				//item.Show();
				//menu.Append (item);

				item = new MenuItem ("Copy");
				item.Activated += new EventHandler (OnGridPopupMenu_Copy);
				item.Sensitive = rowsSensitive;
				item.Show();
				menu.Append (item);

				item = new MenuItem ("Select All");
				item.Activated += new EventHandler (OnGridPopupMenu_SelectAll);
				item.Sensitive = rowsSensitive;
				item.Show();
				menu.Append (item);

				item = new MenuItem ("Un-Select All");
				item.Activated += new EventHandler (OnGridPopupMenu_UnSelectAll);
				item.Sensitive = rowsSensitive;
				item.Show();
				menu.Append (item);

				menu.Popup (null, null, null, 3, Gtk.Global.CurrentEventTime);

                args.RetVal = false;
			}
		}

        [GLib.ConnectBefore]
		void OnDataGridButtonPress (object o, ButtonPressEventArgs args) 
		{
			EventButton but = args.Event;
			
			/* Right Mouse Button is 3 */
			if(but.Button == 3) {
				args.RetVal = true;

				// need to determine which grid was clicked
				if (currentEditorTab.grid != null)
					selectedGrid = currentEditorTab.grid;
				else if (currentEditorTab.gridResults != null) 
					selectedGrid = currentEditorTab.gridResults.FindGrid ((TreeView) o);
			}
		}

		//void OnGridPopupMenu_SaveAs (object o, EventArgs args) 
		//{
		//}

		void OnGridPopupMenu_SelectAll (object o, EventArgs args) 
		{
			selectedGrid.View.Selection.SelectAll ();
		}

		void OnGridPopupMenu_UnSelectAll (object o, EventArgs args) 
		{
			selectedGrid.View.Selection.UnselectAll ();			
		}

		// copy rows in the result grid to the clipboard.  
		// columns are separated by a tab
		// rows are separated by a carriage return
		// if rows are selected, those rows are copied.
		// if no rows are selected, all rows are copied.
		// if no rows are in grid, the clipboard is cleared
		void OnGridPopupMenu_Copy (object o, EventArgs args) 
		{
			StringBuilder sb = new StringBuilder ();
			TreeIter iter = TreeIter.Zero;
			int nColumns = selectedGrid.Store.NColumns;
			int c = 0;
			string val = String.Empty;
			
			TreePath[] paths = selectedGrid.View.Selection.GetSelectedRows ();
			if (paths.Length > 0) {
				// copy selected rows
				for (int p = 0; p < paths.Length; p++) {
					iter = TreeIter.Zero;
					selectedGrid.Store.GetIter (out iter, paths[p]);

					for (c = 0; c < nColumns; c++) {
						if (c > 0)
							sb.Append ('\t');
						val = selectedGrid.Store.GetValue (iter, c).ToString ();
						sb.Append (val);
					}
					sb.Append ("\r");
				}
			}
			else {
			
				// no rows selected, copy all rows
				bool found = selectedGrid.Store.IterChildren (out iter);
				for (c = 0; c < nColumns; c++) {
					if (c > 0)
						sb.Append ('\t');
					val = selectedGrid.Store.GetValue (iter, c).ToString ();
					sb.Append (val);
				}
				sb.Append ("\r");
				
				if (found) {
					found = selectedGrid.Store.IterNext (ref iter);
					while (found) {
						for (c = 0; c < nColumns; c++) {
							val = selectedGrid.Store.GetValue (iter, c).ToString ();
							if (c > 0)
								sb.Append ('\t');
							sb.Append (val);
						}
						sb.Append ("\r");
						found = selectedGrid.Store.IterNext (ref iter);
					}
				}
			}

			val = sb.ToString ().Replace ("\"", "");
			SetClipboardText (val);
		}

		public void SetClipboardText (string text) 
		{
			Clipboard.GetForDisplay (Display.OpenDefaultLibgtkOnly (), Gdk.Atom.Intern ("CLIPBOARD", true)).Text = text;
		}

		void OnDataTreeRowExpanded (object o, RowExpandedArgs args) 
		{
			// 1 Connections
			// 2   conn1
			// 2   conn2
			// 3     Tables
			// 4        table_name_display1, is_populated_status, table_owner, table_name
			// 4        table_name_display2, is_populated_status, table_owner, table_name
			// 5           Columns
			// 6              column_name1
			// 6              column_name2

			// RowExpandedArgs has:
			//  args.Iter
			//  args.Path

			TreeModel model = (TreeModel) tree.Store;

			if (args.Path.Depth == 5) 
			{
				string val = (string) model.GetValue (args.Iter, 0);
				if (val.Equals("Columns"))
				{
					TreeIter parent = args.Iter;
					TreeIter dataSourceIter = args.Iter;

					// get table name
					model.IterParent(out parent, args.Iter);
					string tableName = (string) tree.Store.GetValue (parent, 0); // tableName display
					string objOwner = (string) tree.Store.GetValue (parent, 2); // owner name
					string objName = (string) tree.Store.GetValue (parent, 3); // table name

					TreeIter iter;
					iter = parent;
					model.IterParent(out parent, iter);
					val = (string) tree.Store.GetValue (parent, 0); // Tables
					if (val.Equals("Tables")) 
					{
						// get data source
						model.IterParent(out dataSourceIter, parent);
						string dataSourceName = (string) model.GetValue (dataSourceIter, 0);
						ComboHelper.SetActiveText (combo, dataSourceName);
						
						TreeIter columnsIter = args.Iter;

						string populated = (string) tree.Store.GetValue (args.Iter, 1);
						if (!populated.Equals(NotPopulated))
							return;

						// setup schema browser
						Schema browser = new Schema (provider.Name, conn);
			
						if (browser.MetaData != null) 
						{
							SetStatusBarText ("Getting Meta Data: Tables Columns...");
							while (Application.EventsPending ()) 
								Application.RunIteration ();

							// get table columns
							MetaTable table = new MetaTable (objOwner, objName); // owner, tablename
							PopulateTableColumns(columnsIter, table, browser);

							model.SetValue (args.Iter, 1, "Populated");

							SetStatusBarText ("");

							// remove the NotPopulated node
							TreeIter popIter = columnsIter;
							if (tree.Store.IterChildren (out popIter, columnsIter)) 
								tree.Store.Remove (ref popIter);

						}
					}
				}
			}
		}

		void OnDataTreeButtonRelease (object o, ButtonReleaseEventArgs args) 
		{
			EventButton but = args.Event;
            			
			if(but.Button == 3) {  /* Right Mouse Button is 3 */
				// TreeView was right-clicked with the mouse 
				TreeIter iter;
				TreeModel model;
				TreeSelection selection = tree.View.Selection;
				if (selection.GetSelected (out model, out iter)) {
					selectedDepth = 0;
					selectedType = "";
					selectedObject = "";
					selectedIter = iter;
					bool sensitiveQuery = false;
					bool sensitiveRefresh = false;
					bool sensitiveView = false;

					TreePath tpath = model.GetPath(iter);

					string val = (string) model.GetValue (iter, 0);
					string pval = String.Empty;
					string dataSourceName = "";
					TreeIter parent = iter;
					TreeIter dataSourceIter = iter;
					
					if(tpath.Depth == 3 || tpath.Depth == 4) 
					{
						if (tpath.Depth == 4) 
						{
							model.IterParent(out parent, iter);
							pval = (string) model.GetValue (parent, 0);
						}
						else if (tpath.Depth == 3) 
						{
							parent = iter;
							pval = (string) model.GetValue (iter, 0);
						}

						switch(pval) 
						{
							case "Tables":
								model.IterParent(out dataSourceIter, parent);
								dataSourceName = (string) model.GetValue (dataSourceIter, 0);
								selectedDataSource = dataSourceName;
								selectedDepth = tpath.Depth; // 4
								Console.WriteLine("selectedDepth: " + selectedDepth.ToString());
								selectedType = "Tables";
								selectedObject = val;
								sensitiveQuery = true;
								break;
							case "Views":
								model.IterParent(out dataSourceIter, parent);
								dataSourceName = (string) model.GetValue (dataSourceIter, 0);
								selectedDataSource = dataSourceName;
								selectedDepth = 4;
								selectedType = "Views";
								selectedObject = val;
								sensitiveQuery = true;
								break;
							case "Procedures":
							case "Functions":
							case "Packages":
							case "Stored Procedures":
							case "External Procedures":
								model.IterParent(out dataSourceIter, parent);
								dataSourceName = (string) model.GetValue (dataSourceIter, 0);
								selectedDataSource = dataSourceName;
								selectedDepth = 4;
								selectedType = pval;
								selectedObject = val;
								sensitiveView = true;
								break;
						} 
					}
					else if (tpath.Depth == 6) 
					{
						model.IterParent(out parent, iter);
						pval = (string) model.GetValue (parent, 0);

						switch(pval) 
						{
							case "Columns":
								val = (string) model.GetValue (iter, 0);
								break;
						}   
					}

					// popup a menu over the tree view
					Menu menu = new Menu ();
					MenuItem item;

					item = new MenuItem ("Query");
					item.Activated += new EventHandler (OnTreePopupMenu_Query);
					item.Sensitive = sensitiveQuery;
					item.Show();
					menu.Append (item);

					item = new MenuItem ("Refresh");
					item.Activated += new EventHandler (OnTreePopupMenu_Refresh);
					item.Sensitive = true; //sensitiveRefresh;
					item.Show();
					menu.Append (item);

					item = new MenuItem ("View");
					item.Activated += new EventHandler (OnTreePopupMenu_View);
					item.Sensitive = sensitiveView;
					item.Show();
					menu.Append (item);

					menu.Popup (null, null, null, 3, Gtk.Global.CurrentEventTime); 
				}
			}
		}

		void OnTreePopupMenu_Query (object o, EventArgs args) 
		{
			if(selectedDepth == 4) {
				switch(selectedType) {
				case "Tables":
				case "Views":
					// Query data from a database table or view
					string sql = "SELECT * FROM " + selectedObject;

					EditorTab etab = NewEditorTab ();
					TextBuffer buf = etab.editor.Buffer;
					TextIter endTextIter = buf.EndIter;
					buf.Insert (ref endTextIter, sql);
					buf.Modified = false;
					string basefile = "";
					
					if (selectedType.Equals("Tables"))
						basefile = "Table - " + selectedObject;
					else
						basefile = "View - " + selectedObject;

					etab.label.Text = basefile;
					etab.basefilename = basefile;						
					UpdateTitleBar(etab);
					sourceFileNotebook.CurrentPage = -1;
					ComboHelper.SetActiveText (combo, selectedDataSource);
					Execute (ExecuteOutputType.Normal, "", sql);
					AppendText("");

					break;
				}
			}
		}

		void OnTreePopupMenu_Refresh (object o, EventArgs args) 
		{
			if(selectedDepth == 3) 
			{
				switch(selectedType) 
				{
					case "Tables":
						TreeIter iter;
						// remove child nodes
						if (tree.Store.IterChildren (out iter, selectedIter)) 
						{
							while (tree.Store.Remove (ref iter)) 
							{
							}
						}

						ComboHelper.SetActiveText (combo, selectedDataSource);

						// Re-Populate Tables
						PopulateTables (selectedIter, provider, conn, false);
						break;
				}
			}
		}

		void OnTreePopupMenu_View (object o, EventArgs args) 
		{
			if(selectedDepth == 4) {
				switch(selectedType) {
				case "Procedures":
				case "Fuctions":
				case "Packages":
				case "Stored Procedures":
				case "External Procedures":
					Schema browser = new Schema (provider.Name, conn);
					string sql = browser.GetSource (selectedObject, selectedType);				
					
					EditorTab etab = NewEditorTab ();
					TextBuffer buf = etab.editor.Buffer;
					TextIter endIter = buf.EndIter;
					buf.Insert (ref endIter, sql);
					buf.Modified = false;
					string basefile = "";
					
					basefile = selectedType + " - " + selectedObject;

					etab.label.Text = basefile;
					etab.basefilename = basefile;						
					UpdateTitleBar(etab);
					sourceFileNotebook.CurrentPage = -1;
					ComboHelper.SetActiveText (combo, selectedDataSource);
					
					AppendText("");

					break;
				}
			}
		}

		public EditorTab NewEditorTab () 
		{
			EditorTab tab = new EditorTab();		
			editorTabs.Add(tab);
			
			currentEditorTab = tab;

			// top pane - sql editor

			SqlEditorSharp editor;
			editor = new SqlEditorSharp ();
			editor.Tab = tab;
			editor.UseSyntaxHiLighting = false;
			editor.View.Show ();
			editor.View.KeyPressEvent +=
				new Gtk.KeyPressEventHandler(OnKeyPressEventKey);

			lastUnknownFile ++;
			string unknownFile = "Unknown" + 
				lastUnknownFile.ToString() + ".sql";
			Label label = new Label(unknownFile);
			label.Show();

			// bottom pane - output results
			Notebook resultsNotebook = new Notebook();
			tab.resultsNotebook = resultsNotebook;
			resultsNotebook.TabPos = PositionType.Bottom;
			resultsNotebook.SwitchPage += new 
				Gtk.SwitchPageHandler(OnResultsNotebookSwitched);
			
			DataGrid grid = CreateOutputResultsDataGrid ();
			grid.View.Selection.Mode = SelectionMode.Multiple;
			tab.grid = grid;
			grid.Show();
			Label label2 = new Label("Grid");
			label2.Show();
			
			ScrolledWindow swin = CreateOutputResultsTextView (tab);
			swin.Show();

			tab.editor = editor;
			tab.label = label;
			tab.filename = "";
			tab.basefilename = unknownFile;

			Label label3 = new Label("Log");
			label3.Show();

			// finish the notebooks

			tab.frame = new Frame();
			tab.frame.ShadowType = ShadowType.None;
			tab.frame.Add (grid);
			tab.GridPage = resultsNotebook.AppendPage(tab.frame, label2);
			tab.LogPage = resultsNotebook.AppendPage(swin, label3);

			resultsNotebook.ShowAll ();
			resultsNotebook.ResizeChildren ();

			if (outputResults == OutputResults.TextView)
				resultsNotebook.CurrentPage = 1;
			else if (outputResults == OutputResults.DataGrid)
				resultsNotebook.CurrentPage = 0;

			tab.Add1 (editor);
			tab.Add2 (resultsNotebook);

			sourceFileNotebook.AppendPage(tab, label);

			sourceFileNotebook.ShowAll ();
			sourceFileNotebook.ResizeChildren ();

			sourceFileNotebook.CurrentPage = -1;

			tab.page = sourceFileNotebook.CurrentPage;
			UpdateTitleBar(tab);

			SetFocusToEditor ();

			return tab;
		}

		DataGrid CreateOutputResultsDataGrid () 
		{
			DataGrid grid = new DataGrid ();

			grid.View.ButtonPressEvent +=
				new Gtk.ButtonPressEventHandler (OnDataGridButtonPress);

			grid.View.ButtonReleaseEvent +=
				new Gtk.ButtonReleaseEventHandler (OnDataGridButtonRelease);

			return grid;
		}

		ScrolledWindow CreateOutputResultsTextView (EditorTab tab) 
		{
			ScrolledWindow sw;
			sw = new ScrolledWindow (
				new Adjustment (0.0, 0.0, 0.0, 0.0, 0.0, 0.0), 
				new Adjustment (0.0, 0.0, 0.0, 0.0, 0.0, 0.0));
			sw.HscrollbarPolicy = Gtk.PolicyType.Automatic;
			sw.VscrollbarPolicy = Gtk.PolicyType.Automatic;
			sw.ShadowType = Gtk.ShadowType.In;		
			
			tab.textView = new TextView ();
			tab.textView.Editable = false;
			tab.textView.ModifyFont (Pango.FontDescription.FromString ("courier new"));
			sw.Add (tab.textView);		

			return sw;
		}

		void OnKeyPressEventKey(object o, Gtk.KeyPressEventArgs args) 
		{
			if (o is TextView) {
				switch(args.Event.Key) {
				case Gdk.Key.F5:
					ExecuteSQL (ExecuteOutputType.Normal, "", BatchExecuteMode.Command);
					break;
				case Gdk.Key.F6:
					ExecuteSQL (ExecuteOutputType.Normal, "", BatchExecuteMode.Script);
					break;
				case Gdk.Key.F7:
					ExecuteSQL (ExecuteOutputType.Normal, "", BatchExecuteMode.AsIs);
					break;
				}
			}
		}

		Toolbar CreateToolbar () 
		{
			Toolbar toolbar = new Toolbar ();
			toolbar.IconSize = IconSize.SmallToolbar;
			toolbar.ToolbarStyle = ToolbarStyle.Icons;

			ToolButton button1 = new Gtk.ToolButton (Stock.New); 
			button1.SetTooltip (tooltips1, "New", "New");
			button1.Clicked += new EventHandler(OnToolbar_FileNew);
			toolbar.Insert (button1, -1); 

			ToolButton button2 = new ToolButton (Stock.Open); 
			button2.SetTooltip (tooltips1, "Open", "Open");
			button2.Clicked += new EventHandler(OnToolbar_FileOpen);
			toolbar.Insert (button2, -1); 
						
			ToolButton button3 = new ToolButton (Stock.Save); 
			button3.SetTooltip (tooltips1, "Save", "Save");
			button3.Clicked += new EventHandler(OnToolbar_FileSave);
			toolbar.Insert (button3, -1); 
			
			Gtk.ToolButton button4 = new Gtk.ToolButton (Stock.Close); 
			button4.SetTooltip (tooltips1, "Close", "Close");
			button4.Clicked += new EventHandler(OnToolbar_FileClose);
			toolbar.Insert (button4, -1); 
			
			SeparatorToolItem sep = new SeparatorToolItem ();
			toolbar.Insert (sep, -1); 

			ToolButton button5 = new ToolButton (Stock.Execute); 
			button5.SetTooltip (tooltips1, "Execute Command\tF5", "Execute Command");
			button5.Clicked += new EventHandler(OnToolbar_ExecuteCommand);
			toolbar.Insert (button5, -1); 

			ToolButton button6 = new ToolButton (Stock.Execute); 
			button6.SetTooltip (tooltips1, "Execute Script", "Execute Script");
			button6.Clicked += new EventHandler(OnToolbar_ExecuteScript);
			toolbar.Insert (button6, -1); 
			
			ToolButton button7 = new ToolButton (Stock.GoDown); 
			button7.SetTooltip (tooltips1, "Output", "Output");
			button7.Clicked += new EventHandler(OnToolbar_ToggleResultsOutput);
			toolbar.Insert (button7, -1); 
			
			Gtk.ToolButton button8 = new Gtk.ToolButton (Stock.GoUp); 
			button8.SetTooltip (tooltips1, "Query Mode", "Query Mode");
			button8.Clicked += new EventHandler(OnToolbar_ToggleQueryMode);
			toolbar.Insert (button8, -1); 

			combo = ComboHelper.NewComboBox ();
			combo.Changed += new EventHandler (OnDataSourceChanged);
			combo.AppendText (NotConnected);
			ComboHelper.SetActiveText (combo, NotConnected);
			combo.Active = 0;
			ToolItem ti = new ToolItem();
			ti.Child = combo;
			toolbar.Insert (ti, -1); 

			return toolbar;
		}

		void SetFocusToEditor () 
		{
			if (sourceFileNotebook != null) {
				int page = sourceFileNotebook.CurrentPage;
				EditorTab tab = FindEditorTab (page);
				if (tab != null)
					tab.editor.View.GrabFocus ();
			}
		}

		void OnDataSourceChanged (object o, EventArgs args) 
		{
			ComboBox combo = o as ComboBox;
			if (o == null)
				return;

			TreeIter iter;

			if (combo.GetActiveIter (out iter)) {
				if (!combo.Model.GetValue (iter, 0).Equals(NotConnected)) {
					string datasourceName = combo.Model.GetValue (iter, 0).ToString();
					dataSource = dataSources[datasourceName];
					provider = dataSource.Provider;
					conn = dataSource.Connection;
					connectionString = dataSource.ConnectionString.GetConnectionString ();
				}
			}
			SetFocusToEditor ();
		}

		public MenuBar CreateMenuBar () 
		{
			MenuBar menuBar = new MenuBar ();
			Menu menu;
			MenuItem item;
			MenuItem barItem;
			
			// File menu
			menu = new Menu ();

			item = new MenuItem ("New SQL# _Window");
			item.Activated += new EventHandler (OnMenu_FileNewSqlWindow);
			menu.Append (item);

			menu.Append (new SeparatorMenuItem ());

			item = new MenuItem ("_New");
			item.Activated += new EventHandler (OnMenu_FileNew);
			menu.Append (item);

			item = new MenuItem ("_Open...");
			item.Activated += new EventHandler (OnMenu_FileOpen);
			menu.Append (item);

			item = new MenuItem ("_Save");
			item.Activated += new EventHandler (OnMenu_FileSave);
			menu.Append (item);

			item = new MenuItem ("Save _As...");
			item.Activated += new EventHandler (OnMenu_FileSaveAs);
			menu.Append (item);

			item = new MenuItem ("Close");
			item.Activated += new EventHandler (OnMenu_FileClose);
			menu.Append (item);

			menu.Append (new SeparatorMenuItem ());

			item = new MenuItem ("E_xit");
			item.Activated += new EventHandler (OnMenu_FileExit);
			menu.Append (item);

			barItem = new MenuItem ("_File");
			barItem.Submenu = menu;
			menuBar.Append (barItem);

			// Edit menu
			
			menu = new Menu ();

			item = new MenuItem ("Clear Text Output");
			item.Activated += new EventHandler (OnMenu_EditClearTextOutput);
			menu.Append (item);

			/* TODO: do the Edit menu - for now - comment out
			item = new MenuItem ("Cu_t");
			//item.Activated += new EventHandler (OnMenu_EditCut);
			menu.Append (item);

			item = new MenuItem ("_Copy");
			//item.Activated += new EventHandler (OnMenu_EditCopy);
			menu.Append (item);

			item = new MenuItem ("_Paste");
			//item.Activated += new EventHandler (OnMenu_EditPaste);
			menu.Append (item);

			item = new MenuItem ("_Delete");
			//item.Activated += new EventHandler (OnMenu_EditDelete);
			menu.Append (item);

			menu.Append (new SeparatorMenuItem ());

			item = new MenuItem ("_Find and Replace...");
			//item.Activated += new EventHandler (OnMenu_EditFindReplace);
			menu.Append (item);

			menu.Append (new SeparatorMenuItem ());

			item = new MenuItem ("_Options");
			//item.Activated += new EventHandler (OnMenu_EditOptions);
			menu.Append (item);
			*/

			barItem = new MenuItem ("_Edit");
			barItem.Submenu = menu;
			menuBar.Append (barItem);

			// Session menu
			menu = new Menu ();

			item = new MenuItem ("_Connect");
			item.Activated += new EventHandler (OnMenu_SessionConnect);
			menu.Append (item);

			item = new MenuItem ("_Disconnect");
			item.Activated += new EventHandler (OnMenu_SessionDisconnect);
			menu.Append (item);

			item = new MenuItem ("_Disconnect All");
			item.Activated += new EventHandler (OnMenu_SessionDisconnectAll);
			menu.Append (item);

			barItem = new MenuItem ("_Session");
			barItem.Submenu = menu;
			menuBar.Append (barItem);

			// Command menu
			menu = new Menu ();

			item = new MenuItem ("_Execute Command");
			item.Activated += new EventHandler (OnMenu_CommandExecuteCommand);
			menu.Append (item);

			item = new MenuItem ("_Execute Script");
			item.Activated += new EventHandler (OnMenu_CommandExecuteScript);
			menu.Append (item);

			item = new MenuItem ("_Execute With Output to XML");
			item.Activated += new EventHandler (OnMenu_CommandExecuteXML);
			menu.Append (item);

			item = new MenuItem ("_Execute With Output to CSV");
			item.Activated += new EventHandler (OnMenu_CommandExecuteCSV);
			menu.Append (item);

			barItem = new MenuItem ("_Command");
			barItem.Submenu = menu;
			menuBar.Append (barItem);

			// Help menu
			// Command menu
			menu = new Menu ();

			item = new MenuItem ("_About");
			item.Activated += new EventHandler (OnMenu_HelpAbout);
			menu.Append (item);

			barItem = new MenuItem ("_Help");
			barItem.Submenu = menu;
			menuBar.Append (barItem);

			return menuBar;
		}

		public void AppendTextWithoutScroll (string text) 
		{
			int page = sourceFileNotebook.CurrentPage;
			EditorTab tab = FindEditorTab(page);
			TextBuffer buf = tab.textView.Buffer;

			TextIter iter;
			buf.MoveMark(buf.InsertMark, buf.EndIter);
			if(text != null) {
				if (text.Equals ("") == false) {				
					iter = buf.EndIter;
					buf.Insert (ref iter, text);
				}
			}
			iter = buf.EndIter;
			buf.Insert (ref iter, "\n");
			buf.MoveMark (buf.SelectionBound, buf.EndIter);
		}

		// WriteLine() to output text to bottom TextView
		// for displaying result sets and logging messages
		public void AppendText (string text) 
		{
			int page = sourceFileNotebook.CurrentPage;
			EditorTab tab = FindEditorTab(page);
			TextBuffer buf = tab.textView.Buffer;

			AppendTextWithoutScroll(text);
			while (Application.EventsPending ()) 
				Application.RunIteration ();
			tab.textView.ScrollToMark (buf.InsertMark, 0.4, true, 0.0, 1.0);
		}

		void QuitApplication () 
		{
			// FIXME: sometimes gtk+ error happens, such as, opening file and then quitting
			//        fix the proper way of quiting this app

			for(int i = 0; i < dataSources.Count; i++) {
				DataSourceConnection c = dataSources[i];
				if (c.Connection.State == ConnectionState.Open) 
					c.Connection.Close ();
					c.Dispose ();
			}
			dataSources.Clear ();
			dataSources = null;
			dataSource = null;
			conn = null;

			SqlWindowCount --;
			if (SqlWindowCount == 0)
				Application.Quit ();
			else
				win.Destroy ();
		}

		void UpdateTitleBar(EditorTab tab) 
		{		
			string title = "";
			if(tab != null) {
				if(tab.filename.Equals(""))
					title = tab.label.Text + " - " + ApplicationName;
				else
					title = tab.filename + " - " + ApplicationName;
			}
			else {
				title = ApplicationName;
			}
			win.Title = title;
		}

		void OnEditorTabSwitched (object o, Gtk.SwitchPageArgs args) 
		{
			int page = (int) args.PageNum;
			currentEditorTab = FindEditorTab(page);
			UpdateTitleBar (currentEditorTab);
			DoEvents ();
			SetFocusToEditor ();
		}

		void OnResultsNotebookSwitched (object o, Gtk.SwitchPageArgs args) 
		{
			DoEvents ();
			SetFocusToEditor ();
		}

		void OnWindow_Delete (object o, Gtk.DeleteEventArgs args) 
		{
			QuitApplication();
		}

		void FileNewSqlWindow () 
		{
			SqlSharpGtk sqlSharp = new SqlSharpGtk ();
			sqlSharp.Show ();
		}

		void FileNew () 
		{
			NewEditorTab();
			sourceFileNotebook.CurrentPage = -1;
		}

		void FileOpen () 
		{
			new FileSelectionDialog ("Open File", new FileSelectionEventHandler (OnOpenFile));
		}

		void FileSave () 
		{
			int page = sourceFileNotebook.CurrentPage;
			EditorTab tab = FindEditorTab(page);

			if(tab.filename.Equals(""))
				SaveAs();
			else {
				SaveFile(tab.filename);
				tab.label.Text = tab.basefilename;
			}
		}

		void OnMenu_FileClose (object o, EventArgs args)
		{
			CloseEditor();
		}

		void OnMenu_FileNewSqlWindow (object o, EventArgs args) 
		{
			FileNewSqlWindow ();
		}

		void OnMenu_FileNew (object o, EventArgs args) 
		{
			FileNew ();
		}

		void OnMenu_FileOpen (object o, EventArgs args) 
		{
			FileOpen ();
		}

		void OnOpenFile (object o, FileSelectionEventArgs args) 
		{
			EditorTab etab = NewEditorTab();
			try {
				etab.editor.LoadFromFile (args.Filename);
			}
			catch(Exception openFileException) {
				Error("Error: Could not open file: \n" + 
					args.Filename + 
					"\n\nReason: " + 
					openFileException.Message);
				return;
			}
			
			TextBuffer buf = etab.editor.Buffer;
			buf.Modified = false;
			string basefile = Path.GetFileName (args.Filename);
			etab.label.Text = basefile;
			etab.basefilename = basefile;
			etab.filename = args.Filename;
			sourceFileNotebook.CurrentPage = -1;
			UpdateTitleBar(etab);

			o = null;
			args = null;
		}

		EditorTab FindEditorTab (int searchPage) 
		{
			EditorTab tab = null;
			for (int t = 0; t < editorTabs.Count; t++) {
				tab = (EditorTab) editorTabs[t];
				if (tab.page == searchPage)
					return tab;
			}
			return tab;
		}

		void OnMenu_FileSave (object o, EventArgs args) 
		{
			FileSave ();
		}

		void SaveFile (string filename) 
		{
			int page = sourceFileNotebook.CurrentPage;
			EditorTab etab = FindEditorTab(page);

			try {
				// FIXME: if file exists, ask if you want to 
				//        overwrite.   currently, it overwrites
				//        without asking.
				etab.editor.SaveToFile (filename);
			} 
			catch(Exception saveFileException) {
				Error("Error: Could not open file: \n" + 
					filename + 
					"\n\nReason: " + 
					saveFileException.Message);
				return;
			}
			TextBuffer buf = etab.editor.Buffer;
			buf.Modified = false;
		}

		void OnMenu_FileSaveAs (object o, EventArgs args) 
		{
			SaveAs();
		}

		void SaveAs() 
		{
			new FileSelectionDialog ("File Save As", new FileSelectionEventHandler (OnSaveAsFile));
		}

		void OnSaveAsFile (object o, FileSelectionEventArgs args) 
		{
			int page = sourceFileNotebook.CurrentPage;
			EditorTab etab = FindEditorTab(page);

			SaveFile(args.Filename);

			string basefile = Path.GetFileName (args.Filename);
			etab.label.Text = basefile;
			etab.basefilename = basefile;
			etab.filename = args.Filename;
			UpdateTitleBar(etab);
		}

		void CloseEditor () 
		{
			int page = sourceFileNotebook.CurrentPage;
			EditorTab tab = FindEditorTab(page);

			if(tab.editor.Buffer.Modified) {
				// TODO: if text modified, 
				// ask if user wants to save
				// before closing.
				// use MessageDialog to prompt
				RemoveEditorTab (tab, page);
			}
			else {
				RemoveEditorTab (tab, page);
			}
		}

		void RemoveEditorTab (EditorTab tab, int page) 
		{
			tab.editor.Clear();
			tab.editor.View.KeyPressEvent -=
				new Gtk.KeyPressEventHandler(OnKeyPressEventKey);
			tab.editor.Tab = null;
			tab.editor = null;
			tab.label = null;
			tab.grid = null;
			editorTabs.Remove(tab);
			sourceFileNotebook.RemovePage (page);
			sourceFileNotebook.QueueDraw();
			tab = null;
		}

		void OnMenu_FileExit (object o, EventArgs args) 
		{
			QuitApplication ();
		}

		void OnMenu_SessionConnect (object o, EventArgs args) 
		{	
			Login ();
		}

		void OnMenu_HelpAbout (object o, EventArgs args) 
		{	
			new AboutDialog ();
		}

		public void Login () 
		{
			new LoginDialog (this);
		}

		void OnMenu_EditClearTextOutput (object o, EventArgs args) 
		{	
			TextBuffer buf = currentEditorTab.textView.Buffer;
			TextIter startIter = buf.StartIter;
			TextIter endIter = buf.EndIter;
			buf.Delete (ref startIter, ref endIter);
			SetFocusToEditor ();
		}

		void OnMenu_SessionDisconnect (object o, EventArgs args) 
		{
			AppendText ("Disconnecting...");
			try {

				if (combo.Active < 0)
					return;

				ListStore comboStore = (ListStore) combo.Model;
				TreeIter comboIter;
				comboStore.IterNthChild(out comboIter, combo.Active);
				string dataSourceRemove  = comboStore.GetValue (comboIter, 0).ToString();

				if (dataSourceRemove.Equals(NotConnected))
					return;

				if (dataSourceRemove.Equals(""))
					return;

				TreeIter iter, iterParent;
				string tvalue = "";
				conn.Close ();
				conn = null;
				dataSources.Remove (dataSources[dataSourceRemove]);
				dataSource = null;
				provider = null;
				comboStore.Remove (ref comboIter);

				if (dataSources.Count > 0) {
					dataSource = dataSources[0];
					provider = dataSource.Provider;
					conn = dataSource.Connection;
					connectionString = dataSource.ConnectionString.GetConnectionString ();
					ComboHelper.SetActiveText (combo, dataSource.Name);
					
					// remove the selected connected data source from the left tree view
					tree.Store.IterChildren (out iterParent);
					tree.Store.IterChildren (out iter, iterParent);
					GLib.Value v = GLib.Value.Empty;
					tree.Store.GetValue (iter, 0, ref v);
					tvalue = (string) v.Val;
					if (tvalue.Equals (dataSourceRemove)) 
						tree.Store.Remove (ref iter);
					else {
						bool found = tree.Store.IterNext (ref iter);
						while (found == true) {
							v = GLib.Value.Empty;
							tree.Store.GetValue (iter, 0, ref v);
							tvalue = (string) v.Val;
							if (tvalue.Equals (dataSourceRemove)) {
								tree.Store.Remove (ref iter);
								found = false;
							}
							else
								found = tree.Store.IterNext (ref iter);
						}
					}
				}
				else {
					// removed last connected data source from tree view
					comboStore.Clear ();
					combo.AppendText (NotConnected);
					combo.Active = 0;

					tree.Store.IterChildren (out iterParent);
					tree.Store.IterChildren (out iter, iterParent);
					tree.Store.Remove (ref iter);						
				}			
			}
			catch (Exception e) {
				Error ("Error: Unable to disconnect." + e.Message);
				conn = null;
				return;
			}
			AppendText ("Disconnected.");
		}

		void OnMenu_SessionDisconnectAll (object o, EventArgs args) 
		{
			AppendText ("Disconnecting All...");
			try {
				for(int i = 0; i < dataSources.Count; i++) {
					DataSourceConnection c = dataSources[i];
					c.Connection.Close ();
				}
				dataSources.Clear ();
				conn = null;
				dataSource = null;
				provider = null;
				((ListStore) combo.Model).Clear ();
				combo.AppendText (NotConnected);
				combo.Active = 0;
				tree.Clear ();
			}
			catch (Exception e) {
				Error ("Error: Unable to disconnect." + e.Message);
				conn = null;
				return;
			}
			AppendText ("Disconnected.");
		}

		void OnToolbar_ToggleResultsOutput (System.Object obj, EventArgs ea) 
		{
			if (outputResults == OutputResults.TextView) 
				outputResults = OutputResults.DataGrid;
			else if (outputResults == OutputResults.DataGrid) 
				outputResults = OutputResults.TextView;
		}

		void OnToolbar_ToggleQueryMode (System.Object obj, EventArgs ea) 
		{
			if (queryMode == QueryMode.Query) 
				queryMode = QueryMode.NonQuery;
			else if (queryMode == QueryMode.NonQuery)
				queryMode = QueryMode.Query;
		}

		public void OnToolbar_FileNew (System.Object obj, EventArgs ea) 
		{
			FileNew ();
		}

		public void OnToolbar_FileOpen (System.Object obj, EventArgs ea) 
		{
			FileOpen ();
		}

		public void OnToolbar_FileSave (System.Object obj, EventArgs ea) 
		{
			FileSave ();
		}

		public void OnToolbar_FileClose (System.Object obj, EventArgs ea) 
		{
			CloseEditor ();
		}

		public void OnToolbar_ExecuteCommand (System.Object obj, EventArgs ea) 
		{
			DoEvents ();
			SetFocusToEditor ();
			ExecuteSQL (ExecuteOutputType.Normal, "", BatchExecuteMode.Command);
		}

		public void OnToolbar_ExecuteScript (System.Object obj, EventArgs ea) 
		{
			ExecuteSQL (ExecuteOutputType.Normal, "", BatchExecuteMode.Script);
		}

		// Execute SQL Commands in editor
		private bool ExecuteSQL (ExecuteOutputType outputType, string filename, BatchExecuteMode exeMode) 
		{
			int page = sourceFileNotebook.CurrentPage;
			EditorTab tab = FindEditorTab(page);

			TextIter start_iter, end_iter;
			TextBuffer buf = tab.editor.Buffer;

			string sql = "";	

			// get text from SQL editor
			try {				
				if(buf.GetSelectionBounds (out start_iter, out end_iter) == true) {
					sql = buf.GetText(start_iter, end_iter, false);
					exeMode = BatchExecuteMode.AsIs;
					if (Execute (outputType, filename, sql) == false) {
						AppendText("");
						return false;
					}
					AppendText("");
				}
				else {
					if (exeMode == BatchExecuteMode.AsIs) {
						// get entire text from buffer
						start_iter = tab.editor.Buffer.StartIter;
						end_iter = tab.editor.Buffer.EndIter;
						sql = buf.GetText(start_iter, end_iter, false);
						if (Execute (outputType, filename, sql) == false) {
							AppendText("");
							return false;
						}
						AppendText("");
					}
					else if (exeMode == BatchExecuteMode.Command) {
						// get command at cursor
						end_iter = tab.editor.Buffer.EndIter;
						sql = tab.editor.GetSqlStatementAtCursor(out end_iter);
						if (Execute (outputType, filename, sql) == false) {
							AppendText("");
							return false;
						}
						AppendText("");
					}
					else if (exeMode == BatchExecuteMode.Script) {
						start_iter = buf.GetIterAtOffset (0);
						sql = tab.editor.GetSqlStatementAtCursor(out start_iter);
						if (sql.Length > 0) {
							// move insert mark to end of SQL statement to be executed
							buf.MoveMark (buf.InsertMark, start_iter);
							buf.MoveMark (buf.SelectionBound, start_iter);
							while (Application.EventsPending ()) 
								Application.RunIteration ();

							if (Execute (outputType, filename, sql) == false) {
								tab.editor.View.ScrollToMark (buf.InsertMark, 0.4, true, 0.0, 1.0);
								return false;
							}
						}
						
						while(sql.Length > 0 && start_iter.IsEnd == false) {
							while (Application.EventsPending ()) 
								Application.RunIteration ();

							sql = tab.editor.GetNextSqlStatement(ref start_iter);
							if (sql.Length > 0) {
								// move insert mark to end of SQL statement to be executed
								buf.MoveMark (buf.InsertMark, start_iter);
								buf.MoveMark (buf.SelectionBound, start_iter);
								while (Application.EventsPending ()) 
									Application.RunIteration ();

								if (Execute (outputType, filename, sql) == false) {
									tab.editor.View.ScrollToMark (buf.InsertMark, 0.4, true, 0.0, 1.0);
									return false;
								}
							}
						}
						tab.editor.View.ScrollToMark (buf.InsertMark, 0.4, true, 0.0, 1.0);
						AppendText("");
					}
				}
			}
			catch (Exception et) {
				if (exeMode == BatchExecuteMode.Script)
					tab.editor.View.ScrollToMark (buf.InsertMark, 0.4, true, 0.0, 1.0);

				AppendText ("Error: Unable to execute SQL statement: " + et.Message);
				return false;
			}

			return true; // return true - success
		}
		
		private bool Execute (ExecuteOutputType outputType, string filename, string sql) 
		{
			int page = sourceFileNotebook.CurrentPage;
			EditorTab tab = FindEditorTab(page);

			// do not execute empty queries
			if (sql == null)
				return true;

			if (sql.Trim ().Equals(""))
				return true;

			string[] parms = sql.Split (new char[1] {' '});
			string userCmd = parms[0].ToUpper ();

			switch (userCmd) {
			case "CONNECT":
				if(parms.Length == 2)
					CreateConnection (parms[1]);
				else {
					Error ("CONNECT only has only one operand");
					return false;
				}
				return true;
			case "DISCONNECT":
				Error ("Disconnecting a data source not supported yet.");
				return false;
			default:
				break;
			}

			if (conn == null) {
				AppendText ("Error: Not Connected.");
				SetStatusBarText ("Error: Not Connected.");
				return false;
			}

			IDbCommand cmd = null;
			DataTable schemaTable = null;
			string msg = "";
			long rowsRetrieved = 0;
			int rowsAffected = 0;

			try {
				cmd = conn.CreateCommand ();
			}
			catch (Exception ec) {
				AppendText ("Error: Unable to create command to execute: " + ec.Message);
				return false;
			}
			
			try {
				cmd.CommandText = sql;
			}
			catch (Exception e) {
				AppendText ("Error: Unable to set SQL text to command.  Reason: " + e.Message);
				return false;
			}
			
			IDataReader reader = null;

			SetStatusBarText ("Executing...");
			DoEvents ();
			
			if (outputType == ExecuteOutputType.Normal ||
				outputType == ExecuteOutputType.CsvFile) {
				try {
					if (queryMode == QueryMode.Query)
						reader = cmd.ExecuteReader ();
					else if (queryMode == QueryMode.NonQuery) {
						rowsAffected = cmd.ExecuteNonQuery ();
						if (rowsAffected == -1) {
							msg = "SQL Command Executed.";
							AppendText (msg);
							SetStatusBarText (msg);
						}
						else {
							msg = "Rows Affected: " + rowsAffected.ToString ();
							AppendText (msg);
							SetStatusBarText (msg);
						}
					}
				}
				catch (Exception e) {
					msg = "SQL Error: " + e.Message;
					Error (msg);
					return false;
				}
			
				if (queryMode == QueryMode.Query && reader == null) {
					Error("Error: reader is null");
					return false;
				}
			}

			if (queryMode == QueryMode.Query) {
				try {
					if (outputResults == OutputResults.TextView && 
						outputType == ExecuteOutputType.Normal) {

						DisplayData (reader);
						// clean up
						reader.Close ();
						reader.Dispose ();
						reader = null;
					}
					else if(outputType == ExecuteOutputType.CsvFile) {
						schemaTable = reader.GetSchemaTable();
						if(schemaTable != null && reader.FieldCount > 0) {
							OutputDataToCsvFile(reader, schemaTable, filename);
						}
						else {
							AppendTextWithoutScroll("Command executed.");
							SetStatusBarText ("Command executed.");
						}
						// clean up
						reader.Close ();
						reader.Dispose ();
						reader = null;
					}
					else {
						switch(outputType) {
						case ExecuteOutputType.Normal:							
							ArrayList grds = new ArrayList ();
							bool bContinue = true;
							while (bContinue) {
								DataGrid grd = CreateOutputResultsDataGrid ();
								grd.View.Selection.Mode = SelectionMode.Multiple;
								grd.Show ();
								rowsRetrieved = grd.DataLoad (reader);
								if (reader.FieldCount == 0)	{
									SetStatusBarText ("SQL executed.");	
									AppendText ("SQL executed.");
								}
								else {
									SetStatusBarText ("Records Retrieved: " + rowsRetrieved.ToString ());
									AppendText ("Records Retrieved: " + rowsRetrieved.ToString ());	
								}
								grds.Add (grd);
								bContinue = reader.NextResult ();
							}

							if (tab.grid != null) 
							{
								tab.frame.Remove (tab.grid);
								tab.grid = null;
							}
							else if (tab.gridResults != null) 
							{
								tab.frame.Remove (tab.gridResults);
								tab.gridResults = null;
							}

							if (grds.Count == 1) {
								tab.grid = (DataGrid) grds [0];
								tab.frame.Add (tab.grid);
							}
							else {
								tab.gridResults = new MultiResultsGrid (grds);
								tab.frame.Add (tab.gridResults);
							}

							sourceFileNotebook.QueueDraw ();
							tab.resultsNotebook.ShowAll ();
							tab.resultsNotebook.ResizeChildren ();
							DoEvents ();
							tab.resultsNotebook.CurrentPage = tab.GridPage;

							reader.Close ();
							reader.Dispose ();
							reader = null;
							
							break;
						case ExecuteOutputType.XmlFile:
							AppendText ("Execute and output to XML file: " + filename + "...");
							SetStatusBarText ("Execute and output to XML file: " + filename + "...");
							DataSet dataSet = new DataSet();
							DataTable dataTable = LoadDataTable (cmd);  
							dataSet.Tables.Add(dataTable);
							dataSet.WriteXml(filename);
							dataSet = null;
							dataTable.Clear();
							dataTable.Dispose();
							dataTable = null;
							AppendText ("XML file written: " + filename);
							SetStatusBarText ("XML file written: " + filename);
							break;
						}
						cmd.Dispose();
						cmd = null;
					}
				}
				catch (Exception e) {
					msg = "Error Displaying Data: " + e.Message;
					Error (msg);
					return false;
				}
			}

			return true; // return true - success
		}

		public void CreateConnection (string setting) 
		{
			string connectionString = ConfigurationSettings.AppSettings[setting];
			ConnectionString cstring = new ConnectionString (connectionString);
			string providerName = cstring.Parameters["FACTORY"];
			OpenDataSource (ProviderFactory.Providers[providerName], cstring.GetConnectionString (), setting);
		}

		public void OutputDataToCsvFile(IDataReader rdr, DataTable dt, string file) 
		{
			AppendTextWithoutScroll ("Outputting results to CSV file " + file + "...");
			StreamWriter outputFilestream = null;
			try {
				outputFilestream = new StreamWriter(file);
			}
			catch(Exception e) {
				Error("Error: Unable to setup output results file. " + 
					e.Message);
				return;
			}

			StringBuilder strCsv = null;

			int col = 0;
			string dataValue = "";
			
			while(rdr.Read()) {
				strCsv = new StringBuilder();
				
				for(col = 0; col < rdr.FieldCount; col++) {
					if(col > 0)
						strCsv.Append(",");

					// column data
					if (rdr.IsDBNull(col) == true)
						dataValue = "\"\"";
					else {
						string obj = rdr.GetValue(col).ToString ();
						obj = obj.Replace("\"", "");
						dataValue = "\"" + obj + "\"";
					}
					strCsv.Append (dataValue);
				}
				outputFilestream.WriteLine (strCsv.ToString());
				strCsv = null;
			}
			strCsv = null;
			outputFilestream.Close ();
			outputFilestream = null;
			AppendTextWithoutScroll ("Outputting CSV file done.");
			SetStatusBarText ("Outputting CSV file done.");	
		}

		void OnMenu_CommandExecuteCommand (object o, EventArgs args) {
			ExecuteSQL (ExecuteOutputType.Normal, "", BatchExecuteMode.Command);
		}

		void OnMenu_CommandExecuteScript (object o, EventArgs args) {
			ExecuteSQL (ExecuteOutputType.Normal, "", BatchExecuteMode.Script);
		}

		void OnMenu_CommandExecuteXML (object o, EventArgs args) {
			ExecuteAndSaveResultsToFile (ExecuteOutputType.XmlFile);
		}

		void OnMenu_CommandExecuteCSV (object o, EventArgs args) {
			ExecuteAndSaveResultsToFile (ExecuteOutputType.CsvFile);
		}

		private ExecuteOutputType outType;

		void ExecuteAndSaveResultsToFile(ExecuteOutputType oType) 
		{
			outType = oType;
			new FileSelectionDialog ("Results File Save As", new FileSelectionEventHandler (OnSaveExeOutFile));
		}

		void OnSaveExeOutFile (object o, FileSelectionEventArgs args) 
		{
			ExecuteSQL (outType, args.Filename, BatchExecuteMode.Command);
		}

		public bool DisplayResult (IDataReader reader, DataTable schemaTable) 
		{
			StringBuilder line = null;
			StringBuilder hdrUnderline = null;
			string outData = "";
			int hdrLen = 0;
			
			int spacing = 0;
			int columnSize = 0;
			int c;
			
			char spacingChar = ' '; // a space
			char underlineChar = '='; // an equal sign

			string dataType; // .NET Type
			Type theType; 
			DataRow row; // schema row

			line = new StringBuilder ();
			hdrUnderline = new StringBuilder ();
			
			OutputLine ("");
			
			for (c = 0; c < reader.FieldCount; c++) {
				try {			
					DataRow schemaRow = schemaTable.Rows [c];
					string columnHeader = reader.GetName (c);
					if (columnHeader.Equals (""))
						columnHeader = "column";
					if (columnHeader.Length > 32)
						columnHeader = columnHeader.Substring (0,32);
					
					// spacing
					columnSize = (int) schemaRow ["ColumnSize"];
					theType = reader.GetFieldType (c);
					dataType = theType.ToString ();

					switch(dataType) {
					case "System.DateTime":
						columnSize = 25;
						break;
					case "System.Boolean":
						columnSize = 5;
						break;
					case "System.Byte":
						columnSize = 1;
						break;
					case "System.Single":
						columnSize = 12;
						break;
					case "System.Double":
						columnSize = 21;
						break;
					case "System.Int16":
					case "System.Unt16":
						columnSize = 5;
						break;
					case "System.Int32":
					case "System.UInt32":
						columnSize = 10;
						break;
					case "System.Int64":
						columnSize = 19;
						break;
					case "System.UInt64":
						columnSize = 20;
						break;
					case "System.Decimal":
						columnSize = 29;
						break;
					}

					if (columnSize < 0)
						columnSize = 32;
					if (columnSize > 32)
						columnSize = 32;

					hdrLen = columnHeader.Length;
					if (hdrLen < 0)
						hdrLen = 0;
					if (hdrLen > 32)
						hdrLen = 32;

					hdrLen = System.Math.Max (hdrLen, columnSize);

					line.Append (columnHeader);
					if (columnHeader.Length < hdrLen) {
						spacing = hdrLen - columnHeader.Length;
						line.Append (spacingChar, spacing);
					}
					hdrUnderline.Append (underlineChar, hdrLen);

					line.Append (" ");
					hdrUnderline.Append (" ");
				}
				catch (Exception e) {
					Error ("Error: Unable to display header: " + e.Message);
					return false;
				}
			}
			OutputHeader (line.ToString ());
			line = null;
			
			OutputHeader (hdrUnderline.ToString ());
			OutputHeader ("");
			hdrUnderline = null;		
								
			int numRows = 0;

			// column data
			try {
				while (reader.Read ()) {
					numRows++;
				
					line = new StringBuilder ();
					for(c = 0; c < reader.FieldCount; c++) {
						int dataLen = 0;
						string dataValue = "";
						outData = "";
					
						row = schemaTable.Rows [c];
						string colhdr = (string) reader.GetName (c);
						if (colhdr.Equals (""))
							colhdr = "column";
						if (colhdr.Length > 32)
							colhdr = colhdr.Substring (0, 32);

						columnSize = (int) row ["ColumnSize"];
						theType = reader.GetFieldType (c);
						dataType = theType.ToString ();

						switch (dataType) {
						case "System.DateTime":
							columnSize = 25;
							break;
						case "System.Boolean":
							columnSize = 5;
							break;
						case "System.Byte":
							columnSize = 1;
							break;
						case "System.Single":
							columnSize = 12;
							break;
						case "System.Double":
							columnSize = 21;
							break;
						case "System.Int16":
						case "System.Unt16":
							columnSize = 5;
							break;
						case "System.Int32":
						case "System.UInt32":
							columnSize = 10;
							break;
						case "System.Int64":
							columnSize = 19;
							break;
						case "System.UInt64":
							columnSize = 20;
							break;
						case "System.Decimal":
							columnSize = 29;
							break;
						}

						if (columnSize < 0)
							columnSize = 32;
						if (columnSize > 32)
							columnSize = 32;

						hdrLen = colhdr.Length;
						if (hdrLen < 0)
							hdrLen = 0;
						if (hdrLen > 32)
							hdrLen = 32;

						columnSize = System.Math.Max (colhdr.Length, columnSize);

						dataValue = "";
						dataLen = 0;

						if (!reader.IsDBNull (c)) {
							object o = reader.GetValue (c);
							if (o.GetType ().ToString ().Equals ("System.Byte[]"))
								dataValue = GetHexString ( (byte[]) o);
							else
								dataValue = o.ToString ();

							dataLen = dataValue.Length;
							
							if (dataLen <= 0) {
								dataValue = "";
								dataLen = 0;
							}
							if (dataLen > 32) {
								dataValue = dataValue.Substring (0, 32);
								dataLen = 32;
							}

							if (dataValue.Equals(""))
								dataLen = 0;
						}
						columnSize = Math.Max (columnSize, dataLen);
					
						if (dataLen < columnSize) {
							switch (dataType) {
							case "System.Byte":
							case "System.SByte":
							case "System.Int16":
							case "System.UInt16":
							case "System.Int32":
							case "System.UInt32":
							case "System.Int64":
							case "System.UInt64":
							case "System.Single":
							case "System.Double":
							case "System.Decimal":
								outData = dataValue.PadLeft (columnSize);
								break;
							default:
								outData = dataValue.PadRight (columnSize);
								break;
							}
						}
						else
							outData = dataValue;

						line.Append (outData);
						line.Append (" ");
					}
					OutputData (line.ToString ());
				}
			}
			catch (Exception rr) {
				Error ("Error: Unable to read next row: " + rr.Message);
				return false;
			}
		
			OutputLine ("\nRows retrieved: " + numRows.ToString ());
			AppendTextWithoutScroll ("");
			SetStatusBarText ("\nRows retrieved: " + numRows.ToString ());

			return true; // return true - success
		}

		public bool DisplayData (IDataReader reader) 
		{
			bool another = false;
			DataTable schemaTable = null;
			int ResultSet = 0;
			
			do {
				// by Default, data reader has the 
				// first Result set if any
				ResultSet++;
				
				if (reader.FieldCount > 0) {
					// SQL Query (SELECT)
					// RecordsAffected -1 and DataTable has a reference
					try {
						schemaTable = reader.GetSchemaTable ();
					}
					catch (Exception es) {
						Error ("Error: Unable to get schema table: " + es.Message);
						return false;
					}

					DisplayResult (reader, schemaTable);
				}
				else if (reader.RecordsAffected >= 0) {
					// SQL Command (INSERT, UPDATE, or DELETE)
					// RecordsAffected >= 0
					int records = 0;
					try {
						records = reader.RecordsAffected;
						AppendTextWithoutScroll ("SQL Command Records Affected: " + records);
						SetStatusBarText ("SQL Command Records Affected: " + records);
					}
					catch (Exception er) {
						Error ("Error: Unable to get records affected: " +
							er.Message);
						return false;
					}
				}
				else {
					// SQL Command (not INSERT, UPDATE, nor DELETE)
					// RecordsAffected -1 and DataTable has a null reference
					AppendTextWithoutScroll ("SQL Command Executed.");
					SetStatusBarText ("SQL Command Executed.");
				}
				
				// get next result set (if anymore is left)
				try {
					another = reader.NextResult ();
				}
				catch(Exception e) {
					Error ("Error: Unable to read next result: " +	e.Message);
					return false;
				}
			} while(another == true);

			return true; // return true - success
		}

		// used for outputting message, but if silent is set,
		// don't display
		public void OutputLine(string line) 
		{
			OutputData(line);
		}

		// used for outputting the header columns of a result
		public void OutputHeader(string line) 
		{
			OutputData(line);
		}

		// OutputData() - used for outputting data
		//  if an output filename is set, then the data will
		//  go to a file; otherwise, it will go to the Console.Error
		public void OutputData (string line) 
		{
			AppendTextWithoutScroll (line);
		}

		public void Error (string message) 
		{
			Console.Error.WriteLine (message);
			SetStatusBarText (message);
			AppendText (message);
		}

		public DbDataAdapter CreateDbDataAdapter (IDbCommand cmd) 
		{
			DbDataAdapter dbAdapter = null;

			System.Object ad = (System.Object) provider.CreateDataAdapter();

			// set property SelectCommand on DbDataAdapter
			PropertyInfo prop = adapterType.GetProperty("SelectCommand");
			prop.SetValue (ad, cmd, null);

			return dbAdapter;
		}

		public static string GetHexString (byte[] bytes) 
		{ 			
			string bvalue = "";
			
			StringBuilder sb2 = new StringBuilder();
			for (int z = 0; z < bytes.Length; z++) {
				byte byt = bytes[z];
				sb2.Append (byt.ToString("x"));
			}
			if (sb2.Length > 0)
				bvalue = "0x" + sb2.ToString ();
	
			return bvalue;
		}

		public DataTable LoadDataTable (IDbCommand dbcmd) 
		{
			string status = String.Empty;

			SqlSharpDataAdapter adapter = new SqlSharpDataAdapter (dbcmd);
			DataTable dataTable = new DataTable ();

			int rowsAddedOrRefreshed = 0;
			IDataReader reader = null;
			
			try {
				reader = dbcmd.ExecuteReader ();
				if (reader.FieldCount > 0)
					rowsAddedOrRefreshed = adapter.FillTable (dataTable, reader);
			}
			catch(Exception sqle) {
				status = "Error: " + sqle.Message;
			}

			if (status.Equals(String.Empty)) {
				int rowsAffected = reader.RecordsAffected;
				int fields = ((IDataRecord) reader).FieldCount;

				if (fields > 0) {
					status = "Rows Selected: " + rowsAddedOrRefreshed +
						"  Fields: " + fields;
				}
				else {
					status = "Rows Affected: " + rowsAffected;
				}
			}
			AppendText ("Status: " + status);
			
			if (reader != null && ! reader.IsClosed) {
				reader.Close();
			}
			adapter.Dispose();
			adapter = null;

			return dataTable;
		}

		public bool OpenDataSource (Provider theProvider, string theConnectionString, string connectionName) 
		{
			provider = theProvider;
			connectionString = theConnectionString;

			string msg;
			msg = "Attempt to open connection...";
			AppendText (msg);

			conn = null;

			try {
				switch (provider.Name) {
				case "System.Data.SqlClient":
					conn = new SqlConnection ();
					break;
				case "System.Data.Odbc":
					conn = new OdbcConnection ();
					break;
				case "System.Data.OleDb":
					conn = new OleDbConnection ();
					break;
				default:
					conn = provider.CreateConnection ();
					break;
				}
			} catch (Exception e) {
				msg = "Error: Unable to create Connection object. \n" + 
					"Check to make sure the provider is setup correctly in your config file. \n" + 
					e.Message;
				Error (msg);
				return false;
			}

			ConnectionString conString = new ConnectionString (connectionString);
			conn.ConnectionString = connectionString;
			
			try {
				conn.Open ();
				if( conn.State == ConnectionState.Open)
					AppendText ("Open was successfull.");
				else {
					AppendText ("Error: Open failed.");
					return false;
				}
			} catch (Exception e) {
				msg = "Error: Could not open data source: " + e.Message;
				Error (msg);
				conn = null;
				return false;
			}

			// database connected - do other things
			//SetStatusBarText ("Connected.");
			lastConnection++;
			string dataSourceName = "";
			if (connectionName.Equals(""))
				dataSourceName = lastConnection.ToString () + ":" + provider.Name;
			else
				dataSourceName = lastConnection.ToString () + ":" + connectionName;

			dataSource = new DataSourceConnection (dataSourceName, connectionName, provider, conString, conn);
			dataSources.Add (dataSource);
			
			combo.AppendText (dataSourceName);
			ComboHelper.SetActiveText (combo, dataSourceName);

			TreeIter iterDataSource = tree.Store.AppendValues (tree.RootIter, dataSourceName, "");
			tree.Store.SetValue(iterDataSource, 1, "");
			//string sz = tree.Store.GetValue(iterDataSource, 1).ToString();
			//AppendText("sz: " + sz);

			// TODO: only load meta data when the user expands a tree node
			//       or Refreshes
			SetStatusBarText ("Getting Meta Data...");
			while (Application.EventsPending ()) 
				Application.RunIteration ();
			
			PopulateTables (iterDataSource, provider, conn, true);
			//PopulateViews (iterDataSource, provider, conn);
			//PopulateProcedures (iterDataSource, provider, conn);
			
			SetStatusBarText ("Connected.");

			SetFocusToEditor ();

			return true;
		}

		public void PopulateTables (TreeIter parentIter, Provider provider, IDbConnection con, bool IsParent) 
		{
			TreeIter tablesIter = parentIter;
			if (IsParent == true)
				tablesIter = tree.Store.AppendValues (parentIter, "Tables", "");
			
			Schema browser = new Schema (provider.Name, con);
			
			if (browser.MetaData != null) {
				SetStatusBarText ("Getting Meta Data: Tables...");
				while (Application.EventsPending ()) 
					Application.RunIteration ();

				MetaTableCollection tables = browser.GetTables (false);		
				TreeIter iter;
				TreeIter columnsIter;
				foreach(MetaTable table in tables) {
					iter = tree.Store.AppendValues (tablesIter, table.ToString (), "", table.Owner, table.Name);
					columnsIter = tree.Store.AppendValues (iter, "Columns", NotPopulated);
					tree.Store.AppendValues (columnsIter, "NotPopulated", NotPopulated);
					//PopulateTableColumns(columnsIter, table, browser);
				}
			}
		}

		public void PopulateViews (TreeIter parentIter, Provider provider, IDbConnection con) 
		{
			TreeIter viewsIter = tree.Store.AppendValues (parentIter, "Views", "");
			
			Schema browser = new Schema (provider.Name, con);
			
			if (browser.MetaData != null) {
				SetStatusBarText ("Getting Meta Data: Views...");
				while (Application.EventsPending ()) 
					Application.RunIteration ();

				MetaViewCollection views = browser.GetViews (true);
				foreach (MetaView view in views)
					tree.Store.AppendValues (viewsIter, view.ToString(), "");
			}
		}

		void PopulatePackageProcedures (TreeIter parentIter, MetaProcedure parentProc) 
		{
			TreeIter procsIter = TreeIter.Zero;
			TreeIter argsIter = TreeIter.Zero;
			
			MetaProcedureCollection procs = parentProc.Procedures;

			string procType = "Procedures";
			bool first = true;
			foreach (MetaProcedure proc in procs) {
				if (proc.ProcedureType.Equals (procType)) {
					if (first) {
						procsIter = tree.Store.AppendValues (parentIter, procType);
						first = false;
					}	
					argsIter = tree.Store.AppendValues (procsIter, proc.ToString(), "");
					//PopulateProcedureArguments (argsIter, proc);
				}
			}

			procType = "Functions";
			first = true;
			foreach (MetaProcedure proc2 in procs) {
				if (proc2.ProcedureType.Equals (procType)) {
					if (first) {
						procsIter = tree.Store.AppendValues (parentIter, procType);
						first = false;
					}
					argsIter = tree.Store.AppendValues (procsIter, proc2.ToString(), "");
					//PopulateProcedureArguments (argsIter, proc2);
				}
			}
		}

		void PopulateProcedureArguments (TreeIter parentIter, MetaProcedure proc)
		{
			TreeIter argIter = tree.Store.AppendValues (parentIter, "Parameters");
			
			foreach (MetaProcedureArgument arg in proc.Arguments)
				tree.Store.AppendValues (argIter, arg.ToString (), "");
		}

		public void PopulateProcedures (TreeIter parentIter, Provider provider, IDbConnection con) 
		{
			TreeIter procsIter = parentIter;
			
			Schema browser = new Schema (provider.Name, con);
			
			if (browser.MetaData != null) {
				SetStatusBarText ("Getting Meta Data: Procudures...");
				while (Application.EventsPending ()) 
					Application.RunIteration ();

				MetaProcedureCollection procs = browser.GetProcedures ("");
				string procType = "~";
				foreach (MetaProcedure proc in procs) {
					if (!procType.Equals(proc.ProcedureType)) {
						procType = proc.ProcedureType;
						procsIter = tree.Store.AppendValues (parentIter, procType, "");
					}
					TreeIter procIter = tree.Store.AppendValues (procsIter, proc.ToString(), "");

					//if (proc.HasProcedures)
					//	PopulatePackageProcedures (procIter, proc);
					//else 
					//	PopulateProcedureArguments (procIter, proc);
				}
			}
		}

		public void PopulateTableColumns (TreeIter parentIter, MetaTable table, Schema browser) 
		{
			MetaTableColumnCollection columns = browser.GetTableColumns(table.Owner, table.Name);

			foreach(MetaTableColumn column in columns) {
				string nullable;
				if(column.Nullable == true)
					nullable = "Null";
				else
					nullable = "Not Null";
				
				string line = column.Name + " (" + 
					column.DataType + " (" + 
					column.Length + ", " +
					column.Precision + ", " + column.Scale + "), " +
					nullable + ")";
				tree.Store.AppendValues (parentIter, line, "");
			}		
		}

		public static void DoEvents () 
		{
			while (Application.EventsPending ()) 
				Application.RunIteration ();
		}

		public static int Main (string[] args) 
		{
			Application.Init ();
			SqlSharpGtk sqlSharp = new SqlSharpGtk ();

			sqlSharp.Show ();
			DoEvents();

			sqlSharp.Login ();
			Application.Run ();
			return 0;
		}
	}
}

