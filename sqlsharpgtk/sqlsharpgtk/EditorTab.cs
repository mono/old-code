//
// EditorTab.cs - a tab in the source notebook that has a SQL editor and results pane
//
// Authors:
//     Daniel Morgan <monodanmorg@yahoo.com>
//
// (c)copyright 2005-2007 Daniel Morgan
//

namespace Mono.Data.SqlSharp.GtkSharp {
	using System;
	using GLib;
	using Gtk;

	public class EditorTab : VPaned {
		public SqlEditorSharp editor; // SQL editor text view (top panel)
		public Label label;           // label for Notebook tab
		public string filename;       // full filename of file being edited
		public string basefilename;   // base filename (no path) being edited
		public int page;              // notebook page number
		public DataGrid grid;         // results data grid (bottom pane) - tab Grid)
		public TextView textView;     // results text view (bottom panel - tab Log)
		public MultiResultsGrid gridResults;
		public Frame frame;
		public int LogPage;
		public int GridPage;
		public Notebook resultsNotebook;
	}
}

