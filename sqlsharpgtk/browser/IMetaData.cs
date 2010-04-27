// IMetaData.cs
//
// Author:
//     Daniel Morgan <danielmorgan@verizon.net>
//
// (C)Copyright 2004-2005 by Daniel Morgan
//
// To be included with Mono as a SQL query tool licensed under the LGPL license.
//

using System;
using System.Data;

namespace Mono.Data.SqlSharp.DatabaseBrowser
{
	public interface IMetaData
	{
		MetaTableCollection GetTables (bool includeSystemTables);
		MetaTableColumnCollection GetTableColumns (string owner, string tableName);
		MetaViewCollection GetViews (bool includeSystem);
		MetaProcedureCollection GetProcedures(string owner);
		string GetSource (string objectName, string objectType);
	}
}
