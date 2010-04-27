// SqliteMetaData.cs - meta data for SQL Lite 2.x, 3.x databases
//
// Author:
//     Daniel Morgan <danielmorgan@verizon.net>
//
// (C)Copyright 2005 by Daniel Morgan
//
// To be included with Mono as a SQL query tool licensed under the LGPL license.
//

using System;
using System.Data;

namespace Mono.Data.SqlSharp.DatabaseBrowser
{
	public class SqliteMetaData : IMetaData
	{
		IDbConnection con;

		public SqliteMetaData()
		{
		}

		public SqliteMetaData(IDbConnection connection) 
		{
			con = connection;
		}

		public MetaTableCollection GetTables(bool includeSystemTables) 
		{
			if(con.State != ConnectionState.Open)
				con.Open();

			MetaTableCollection tables = new MetaTableCollection ();

			string sql = 
				"SELECT name " +
				"FROM sqlite_master " +
				"WHERE type = 'table' " +
				"ORDER BY name";

			IDbCommand cmd = con.CreateCommand();
			cmd.CommandText = sql;

			IDataReader reader = cmd.ExecuteReader();
			while(reader.Read())
			{
				MetaTable table = new MetaTable(reader.GetString(0));
				tables.Add(table);
			}
			reader.Close();
			reader = null;
			
			return tables;
		}

		public MetaViewCollection GetViews (bool includeSystem) 
		{
			throw new NotImplementedException ();
		}

		public MetaTableColumnCollection GetTableColumns(string owner, string tableName)
		{
			throw new NotImplementedException ();
		}


		public MetaProcedureCollection GetProcedures(string owner) 
		{
			throw new NotImplementedException ();
		}

		public string GetSource (string objectName, string objectType) 
		{
			return "";
		}
	}
}
