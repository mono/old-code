//
// OutputResults.cs - if output type is normal, then results go to the results pane,
//                    either to the TextView or DataGrid
//
// Authors:
//     Daniel Morgan <danielmorgan@verizon.net>
//
// (c)copyright 2005 Daniel Morgan
//

namespace Mono.Data.SqlSharp.GtkSharp {
	using System;
	using GLib;
	using Gtk;

	public enum OutputResults {
		TextView,
		DataGrid
	}
}

