//
// BatchExecuteMode.cs - how should you execute the SQL in the text editor
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

	public enum BatchExecuteMode {
		Command = 1, // execute SQL statement at cursor (SQL statements are separated by a delimiter - a semicolon)
		Script = 2,  // multiple SQL statements separated by a delimiter - a semicolon
		         // however, execute each SQL statement one-at-a-time.  also,
		         // start executing the first SQL statement at cursor
		AsIs = 3    // exeucte SQL as-is
	}
}

