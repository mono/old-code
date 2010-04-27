// MultiResultsGrid - allows a result to have multiple grids
//
// Author: 
//   Daniel Morgan <monodanmorg@yahoo.com>
//
// (c) 2006-2007 Daniel Morgan
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

	public class MultiResultsGrid : VBox 
	{
		VBox gridbox;
		ArrayList results;

		public MultiResultsGrid (ArrayList grids) : base (false, 4) 
		{
			if (grids == null)
				throw new Exception("grids is null");
			if (grids.Count == 0)
				throw new Exception("no grids to add");

			results = grids;

			ScrolledWindow sw = new ScrolledWindow ();
			this.PackStart (sw, true, true, 0);

			gridbox = new VBox (false, 4);

			IEnumerator ienum = results.GetEnumerator ();
			while (ienum.MoveNext ()) {
				DataGrid grid = (DataGrid) ienum.Current;
				gridbox.PackStart (grid, true, true, 0);
			}

			sw.AddWithViewport (gridbox);			
		}
		
		public DataGrid FindGrid (TreeView tv) 
		{
			IEnumerator ienum = results.GetEnumerator ();
			while (ienum.MoveNext ()) {
				DataGrid grid = (DataGrid) ienum.Current;
				if (grid.View == tv)
					return grid;
			}
			throw new Exception ("grid not found");
		}
	}
}

