//
// DataGridColumn - a column for a DataGrid
//    
// Based on the sample/TreeViewDemo.cs
//
// Author: Daniel Morgan <monodanmorg@yahoo.com>
//
// (c) 2002-2007 Daniel Morgan

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

	public class DataGridColumn 
	{
		private string columnName = String.Empty;
		private TreeViewColumn treeViewColumn = null;
		private Type dataType = typeof(string);
		private Pango.Alignment alignment = Pango.Alignment.Left;
		private int columnSize = -1;
		private string format = String.Empty;
        		
		public string ColumnName {
			get {
				return columnName;
			}
			set {
				columnName = value;
			}
		}

		public TreeViewColumn TreeViewColumn {
			get {
				return treeViewColumn;
			}
			set {
				treeViewColumn = value;
			}
		}

		public Type DataType {
			get {
				return dataType;
			}
			set {
				SetDataType (value);
			}
		}

		public Pango.Alignment Alignment {
			get {
				return alignment;
			}
			set {
				alignment = value;
			}
		}

		public int MaxSize {
			get {
				return columnSize;
			}
		}

		public string Format {
			get {
				return format;
			}
			set {
				format = value;
			}
		}

		private void SetDataType (Type type) 
		{
			dataType = type;
			alignment = Pango.Alignment.Right;
			format = String.Empty;

			switch (dataType.ToString ()) {
			case "System.DateTime":
				format = "yyyy/MM/dd HH:mm:ss:fff";
				columnSize = 25;
				alignment = Pango.Alignment.Left;
				break;
			case "System.Boolean":
				columnSize = 5;
				break;
			case "System.Byte":
			case "System.SByte":
				columnSize = 3;
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
			case "System.UInt64":
				columnSize = 20;
				break;
			case "System.Decimal":
				columnSize = 29;
				break;
			default:
				columnSize = -1;
				alignment = Pango.Alignment.Left;
				break;
			}
		}
	}
}

