//
// OutputResults.cs - execute SQL statement as a Query or NonQuery
//                    Query calls IDbCommand.ExecuteReader ()
//                    NonQuery calls IDbCommand.ExecuteNonQuery ()
//
// Authors:
//     Daniel Morgan <monodanmorg@yahoo.com>
//
// (c)copyright 2005 Daniel Morgan
//

namespace Mono.Data.SqlSharp.GtkSharp {
	using System;
	using GLib;
	using Gtk;

	public enum QueryMode {
		Query = 1,    // execute SQL as a Query (SELECT)
		NonQuery = 2  // execute NonQuery (UPDATE, CREATE TABLE, etc...)
	}
}

