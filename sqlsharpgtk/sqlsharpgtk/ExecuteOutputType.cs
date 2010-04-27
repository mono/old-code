//
// ExecuteOutputType.cs - when SQL gets exected, output is normal, or to a
//                        XML file, or CSV file.  Normal being the results pane
//
// Authors:
//     Daniel Morgan <monodanmorg@yahoo.com>
//
// (c)copyright 2005-2006 Daniel Morgan
//

namespace Mono.Data.SqlSharp.GtkSharp {
	using System;
	using GLib;
	using Gtk;

	// TODO: output to other file types, such as, tab separated, OpenDocument spreadsheet,
	//       HTML file, fixed format

	public enum ExecuteOutputType {
		Normal = 1,   // output us normal (to a TextView or DataGrid)
		XmlFile = 2,  // output to an XML file - TODO: create xml file via DataSet
		CsvFile = 3   // output to a CSV file
	}
}


