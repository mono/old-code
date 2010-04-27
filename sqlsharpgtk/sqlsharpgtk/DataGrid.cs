//
// DataGrid - a DataGrid for GTK# which 
//            uses the TreeView with a ListStore tree model
//    
// Based on the sample/TreeViewDemo.cs
//
// Author: Kristian Rietveld <kris@gtk.org>
//         Daniel Morgan <monodanmorg@yahoo.com>
//
// (c) 2002 Kristian Rietveld
// (c) 2002-2007 Daniel Morgan
//

namespace Mono.Data.SqlSharp.GtkSharp
{
	using System;
	using System.Data;
	using System.Collections;
	using System.ComponentModel;
	using System.Drawing;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using System.Text;
	
	using GLib;
	using Gtk;

	public class DataGrid : VBox
	{
		private ListStore store;
		private TreeView treeView;

		public ArrayList gridColumns;

		public DataGrid () : base(false, 4) 
		{		
			ScrolledWindow sw = new ScrolledWindow ();
			this.PackStart (sw, true, true, 0);

			treeView = new TreeView (store);
			treeView.HeadersVisible = true;
			treeView.ModifyFont (Pango.FontDescription.FromString ("courier new"));

			gridColumns = new ArrayList(0);

			sw.Add (treeView);

			store = new ListStore (GLib.GType.String);
			dataMember = "";
			dataSource = null;
		}

		private object dataSource;

		private string dataMember;

		// if in single-select mode, gets the selected row if a row is selected
		// if no row is selected, -1 is returned
		// if in multiple-select mode, gets the first selected row
		public TreePath SelectedPath { 
			get {
				TreeIter iter;
				TreeModel model;
				TreeSelection selection = treeView.Selection;
				if (selection.GetSelected (out model, out iter)) {
					TreePath[] path = selection.GetSelectedRows (out model);
					return path[0]; // return selected row 
				}
				else
					return null; // not selected
			}
		}

		// if in single-select mode, gets the selected row if a row is selected
		// if no row is selected, -1 is returned
		// if in multiple-select mode, gets the first selected row
		public int SelectedRow { 
			get {
				TreeIter iter;
				TreeModel model;
				TreeSelection selection = treeView.Selection;
				if (selection.GetSelected (out model, out iter)) {
					TreePath[] path = selection.GetSelectedRows (out model);
					return path[0].Indices[0]; // return selected row 
				}
				else
					return -1; // not selected
			}
		}

		// if in single-select mode, gets the selected inter if a row is selected
		// if no row is selected, TreeIter.Zero is returned
		public TreeIter SelectedIter {
			get {
				TreeIter iter;
				TreeModel model;
				TreeSelection selection = treeView.Selection;
				if (selection.GetSelected (out model, out iter))
					return iter; // return seelcted iter
				else
					return TreeIter.Zero; // not selected
			}
		}

		// if in multiple-select mode, gets the selected inters if any are selected
		// if no rows are selected, an array with zero length is returned
		public TreeIter[] SelectedIters {
			get {				
				TreePath[] paths = SelectedPaths;
				if (paths.Length == 0)
					return new TreeIter[0];
				TreeIter[] iters = new TreeIter[paths.Length];
				for (int p = 0; p < paths.Length; p++) {
					TreeIter iter = TreeIter.Zero;
					Store.GetIter (out iter, paths[p]);
					iters[p] = iter;
				}
				paths = null;
				return iters;
			}
		}

		// if in TreeSelection.Mode = SelectionMode.Multiple
		// returns an array of TreePath for the selected rows
		public TreePath[] SelectedPaths {
			get {
				return View.Selection.GetSelectedRows ();
			}
		}

		// if in TreeSelection.Mode = SelectionMode.Multiple
		// returns an array of int for the selected rows
		public int[] SelectedRows {
			get {
				TreePath[] paths = SelectedPaths;
				if (paths.Length == 0)
					return new int[0];
				int[] selectedRows = new int[paths.Length];
				for (int p = 0; p < paths.Length; p++)
					selectedRows[p] = paths[p].Indices[0];
				return selectedRows;
			}
		}

		public int SelectedRowCount {
			get {
				return View.Selection.CountSelectedRows ();
			}
		}

		public int RowCount {
			get {
				return Store.IterNChildren ();
			}
		}

		public int ColumnCount {
			get {
				return gridColumns.Count;
			}
		}

		public TreeView View 
		{
			get 
			{
				return treeView;
			}
		}

		public object DataSource 
		{
			get 
			{
				return dataSource;
			}
			set 
			{
				dataSource = value;
			}
		}

		public string DataMember 
		{
			get 
			{
				return dataMember;
			}
			set 
			{
				dataMember = value;
			}
		}

		public ListStore Store 
		{
			get 
			{
				return store;
			}
		}

		public ArrayList Columns 
		{
			get {
				return gridColumns;
			}
		}

		// sets the column count.  beware, it clears
		// use this if you are going to load each column and row yourself
		// instead of using DataBind() or DataLoad()
		public void SetColumnCount (int columnCount) 
		{
			Clear ();
			dataMember = "";
			dataSource = null;

			GLib.GType[] theTypes = new GLib.GType[columnCount];
			gridColumns = new ArrayList ();
			for (int col = 0; col < columnCount; col++) {
				theTypes[col] = GLib.GType.String;
				gridColumns.Add (new DataGridColumn ());
			}
			store.ColumnTypes = theTypes;
		}

		// alternative to DataBind() - load from a data reader
		public long DataLoad (IDataReader reader)
		{
			long rowsRetrieved = 0;

			Clear ();
			dataMember = "";
			dataSource = null;
			
			if (reader.FieldCount > 0) {
				DataTable schema = reader.GetSchemaTable ();
				GLib.GType[] theTypes = new GLib.GType[reader.FieldCount];
				gridColumns = new ArrayList(reader.FieldCount);

				IDataRecord record = (IDataRecord) reader;
				
				int col = 0;
				for (col = 0; col < reader.FieldCount; col ++)	{
					DataGridColumn gridCol = new DataGridColumn ();
					gridCol.ColumnName = record.GetName (col);
					
					try {
						gridCol.DataType = (Type) schema.Rows [col] ["DataType"];
					}
					catch (Exception e) {
						gridCol.DataType = typeof (string);
					}
					
					theTypes [col] = GLib.GType.String;
					gridColumns.Add (gridCol);
				}
				store.ColumnTypes = theTypes;

				TreeIter iter = new TreeIter ();
				
				while (reader.Read ()) {			
					rowsRetrieved ++;
					iter = NewRow ();

					object oValue = null;
					string sValue = "";

					for (col = 0; col < reader.FieldCount; col ++) {													
						oValue = reader.GetValue (col);
						if (reader.IsDBNull (col))
							sValue = "";
						else {
							oValue = reader[col];
							sValue = "";

							if (oValue.GetType ().ToString ().Equals ("System.Byte[]")) 
								sValue = GetHexString ((byte[]) oValue);
							else 
							{
								sValue = oValue.ToString ();

								// work-around for padding numerics on the right
								// gtk# 2.10 added Alignment property to TreeViewColumn
								// but this app is built with gtk# 2.8
								// also, provide custom formatting of columns
								// such as, dates
								DataGridColumn gcol = ((DataGridColumn) gridColumns [col]);
								if (!sValue.Equals(String.Empty))
									if (!gcol.Format.Equals(String.Empty))
										switch (oValue.GetType().ToString() )
										{
											case "System.DateTime":
												sValue = ((DateTime) oValue).ToString (gcol.Format);
												break;
										}
								int maxSize = gcol.MaxSize;
								if (gcol.MaxSize > 0 || gcol.Alignment == Pango.Alignment.Right)
									sValue = sValue.PadLeft (maxSize);							
							}
						}					
						
						SetColumnValue (iter, col, sValue);
					}
				}

				treeView.Model = store;
				AutoCreateTreeViewColumns ();
			}
			return rowsRetrieved;
		}

		// load data from a data table or data set
		public long DataBind () 
		{
			long rowsRetrieved = 0;

			Clear ();

			System.Object o = null;
			o = GetResolvedDataSource (DataSource, DataMember);
			IEnumerable ie = (IEnumerable) o;
			ITypedList tlist = (ITypedList) o;
			TreeIter iter = new TreeIter ();
									
			PropertyDescriptorCollection pdc = tlist.GetItemProperties (new PropertyDescriptor[0]);
			gridColumns = new ArrayList(pdc.Count);

			// define the columns in the treeview store
			// based on the schema of the result
			GLib.GType[] theTypes = new GLib.GType[pdc.Count];
			for (int col = 0; col < pdc.Count; col++) {
				theTypes[col] = GLib.GType.String;
			}
			store.ColumnTypes = theTypes;

			int colndx = -1;
			foreach (PropertyDescriptor pd in pdc) {

				colndx ++;

				DataGridColumn gridCol = new DataGridColumn ();
				gridCol.ColumnName = pd.Name;		
				gridColumns.Add (gridCol);
			}

			foreach (System.Object obj in ie) {
				ICustomTypeDescriptor custom; 
				PropertyDescriptorCollection properties;
				
				custom = (ICustomTypeDescriptor) obj;
				properties = custom.GetProperties ();
				
				rowsRetrieved ++;
				iter = NewRow ();
				int cv = 0;

				foreach (PropertyDescriptor property in properties) {
					object oPropValue = property.GetValue (obj);
					string sPropValue = "";
					if (oPropValue.GetType ().ToString ().Equals ("System.Byte[]")) 
						sPropValue = GetHexString ((byte[]) oPropValue);
					else
						sPropValue = oPropValue.ToString ();
										
					SetColumnValue (iter, cv, sPropValue);
					cv++;			
				}
			}

			treeView.Model = store;
			AutoCreateTreeViewColumns ();
			return rowsRetrieved;
		}

		// borrowed from Mono's System.Web implementation
		protected IEnumerable GetResolvedDataSource(object source, string member) 
		{
			if (source != null && source is IListSource) {
				IListSource src = (IListSource) source;
				IList list = src.GetList ();
				if (!src.ContainsListCollection) {
					return list;
				}
				if (list != null && list is ITypedList) {

					ITypedList tlist = (ITypedList) list;
					PropertyDescriptorCollection pdc = tlist.GetItemProperties (new PropertyDescriptor[0]);
					if (pdc != null && pdc.Count > 0) {
						PropertyDescriptor pd = null;
						if (member != null && member.Length > 0) {
							pd = pdc.Find (member, true);
						} else {
							pd = pdc[0];
						}
						if (pd != null) {
							object rv = pd.GetValue (list[0]);
							if (rv != null && rv is IEnumerable) {
								return (IEnumerable)rv;
							}
						}
						throw new Exception ("ListSource_Missing_DataMember");
					}
					throw new Exception ("ListSource_Without_DataMembers");
				}
			}
			if (source is IEnumerable) {
				return (IEnumerable)source;
			}
			return null;
		}

		public void Clear () 
		{
			if (store != null) 
			{
				store.Clear ();
				store = null;
				store = new ListStore (GLib.GType.String);
			}
			else
				store = new ListStore (GLib.GType.String);

			if (gridColumns != null) 
			{
				for (int c = 0; c < gridColumns.Count; c++) 
				{
					DataGridColumn gridCol = (DataGridColumn) gridColumns[c];
					if (gridCol.TreeViewColumn != null) 
					{
						treeView.RemoveColumn (gridCol.TreeViewColumn);
						gridCol.TreeViewColumn = null;
					}
				}
				gridColumns.Clear ();
				gridColumns = null;
			}
		}

		public TreeIter NewRow () 
		{ 
			return store.Append();
		}

		public void AddRow (object[] columnValues) 
		{	
			TreeIter iter = NewRow ();			
			for(int col = 0; col < columnValues.Length; col++) {
				string cellValue = columnValues[col].ToString ();
				SetColumnValue (iter, col, cellValue);
			}
		}

		public void SetColumnValue (TreeIter iter, int column, string value) 
		{
			GLib.Value cell = new GLib.Value (value);
			store.SetValue (iter, column, cell);	
		}

		public void SetColumnValue (TreeIter iter, int column, byte[] value) 
		{
			string svalue = SqlSharpGtk.GetHexString (value);
			SetColumnValue (iter, column, svalue);
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

		public object GetCellValue (TreeIter rowIter, int column) 
		{
			return Store.GetValue (rowIter, column);
		}

		public object GetCellString (TreeIter rowIter, int column) 
		{
			return GetCellValue (rowIter, column).ToString ();
		}

		private void AutoCreateTreeViewColumns () 
		{
			for (int col = 0; col < gridColumns.Count; col++) {
				// escape underscore _ because it is used
				// as the underline in menus and labels
				string name = ((DataGridColumn) gridColumns [col]).ColumnName.Replace ("_", "__");
				TreeViewColumn tvc = CreateColumn (col, name);
				AppendColumn (tvc);
			}
		}

		public int AppendColumn(TreeViewColumn tvc) 
		{
			return treeView.AppendColumn (tvc);
		}

		public TreeViewColumn CreateColumn (int columnNum, string columnName) 
		{
			TreeViewColumn treeViewCol = new TreeViewColumn ();		 
			CellRendererText renderer = new CellRendererText ();
			renderer.Family = "courier new";
			//renderer.Alignment = Pango.Alignment.Left; // only available in gtk# 2.10+
						
			treeViewCol.Title = columnName;
			treeViewCol.PackStart (renderer, true);
			treeViewCol.AddAttribute (renderer, "text", columnNum);

			DataGridColumn gridCol = (DataGridColumn) gridColumns[columnNum];
			gridCol.TreeViewColumn = treeViewCol;
			
			return treeViewCol;
		}

		public TreeViewColumn CreateColumn (int columnNum, string columnName, Pango.Alignment alignment) 
		{
			TreeViewColumn treeViewCol = new TreeViewColumn ();		 
			CellRendererText renderer = new CellRendererText ();
			renderer.Family = "courier new";
			//renderer.Alignment = alignment; // only available in gtk# 2.10+
						
			treeViewCol.Title = columnName;
			treeViewCol.PackStart (renderer, true);
			treeViewCol.AddAttribute (renderer, "text", columnNum);

			DataGridColumn gridCol = (DataGridColumn) gridColumns[columnNum];
			gridCol.TreeViewColumn = treeViewCol;
			
			return treeViewCol;
		}
	}
}
