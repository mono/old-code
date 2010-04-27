// MySqlMetaData.cs - meta data for MySql
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
	public class MySqlMetaData : IMetaData
	{
		private IDbConnection con;

		// INFORMATION_SCHEMA included in MySQL 5.0
		// default to true.  if GetTables fails, it will swtich to no exists
		private bool infoSchemaExists = true; 

		public MySqlMetaData()
		{
		}

		public MySqlMetaData(IDbConnection connection) 
		{
			con = connection;
		}

		public MetaTableCollection GetTables(bool includeSystemTables) 
		{
			if(con.State != ConnectionState.Open)
				con.Open();

			MetaTableCollection tables = new MetaTableCollection ();

			string sql  = "";
			if (infoSchemaExists)
				sql = "select table_schema, table_name, " +
					" table_type from INFORMATION_SCHEMA.TABLES";
			else
				sql = "SHOW TABLES";

			IDbCommand cmd = con.CreateCommand();
			cmd.CommandText = sql;

			IDataReader reader = null;
			try {
				reader = cmd.ExecuteReader();
			}
			catch(Exception e) {
				infoSchemaExists = false;
				sql = "SHOW TABLES";
				reader = cmd.ExecuteReader();
			}

			while (reader.Read ()) {
				MetaTable table = null;
				
				if (infoSchemaExists)
					table = new MetaTable (reader.GetString (0), reader.GetString (1));
				else
					table = new MetaTable (reader.GetString (0));

				tables.Add(table);
			}
			reader.Close( );
			reader = null;
			
			return tables;
		}

		public MetaViewCollection GetViews (bool includeSystem) 
		{
			if(con.State != ConnectionState.Open)
				con.Open();

			MetaViewCollection views = new MetaViewCollection ();

			if (!infoSchemaExists)
				return views;

			string sql = "select table_schema, table_name, " +
					" view_definition from INFORMATION_SCHEMA.VIEWS;";

			IDbCommand cmd = con.CreateCommand ();
			cmd.CommandText = sql;

			IDataReader reader = cmd.ExecuteReader ();
			while (reader.Read ()) {
				MetaView view = new MetaView (reader.GetString (0),
					reader.GetString (1));
				views.Add (view);
			}
			reader.Close ();
			reader = null;
			
			return views;


		}

		public MetaTableColumnCollection GetTableColumns(string owner, string tableName)
		{
			if(con.State != ConnectionState.Open)
				con.Open();

			MetaTableColumnCollection columns = new MetaTableColumnCollection (owner, tableName);

			string sql = "DESCRIBE " + tableName;

			if (infoSchemaExists)
				sql = "DESCRIBE " + owner + "." + tableName;

			// TODO: use INFORMATION_SCHEMA.TABLES instead of DESCRIBE

			IDbCommand cmd = con.CreateCommand();
			cmd.CommandText = sql;

			IDataReader reader = cmd.ExecuteReader();
			int c = 0;
			while(reader.Read())
			{			
				// 0 = Column name
				// 1 = Date Type (Length)
				// 2 = NULL
				string sDataType = reader.GetString(1);

				MetaTableColumn column = new MetaTableColumn(
					"",
					tableName,
					reader.GetString(0),
					sDataType,
					0,
					0,
					0,
					reader.GetString(2).Equals("NULL") ? true : false,
					c);
				columns.Add(column);
				c ++;
			}
			reader.Close();
			reader = null;
			
			return columns;
		}


		public MetaProcedureCollection GetProcedures(string owner) 
		{
			if(con.State != ConnectionState.Open)
				con.Open();

			MetaProcedureCollection procs = new MetaProcedureCollection ();

			// starting with MySQL 5.0, it supports stored procedures
			// prior version of MySQL did not have the INFORMATION_SCHEMA either
			if (!infoSchemaExists)
				return procs;

			string sql = 
				"SELECT routine_schema, routine_name " +
				" FROM information_schema.ROUTINES";

			IDbCommand cmd = con.CreateCommand();
			cmd.CommandText = sql;

			IDataReader reader = cmd.ExecuteReader();
			while(reader.Read()) {			
				string procSchema = reader.GetString (0);
				string procName = reader.GetString (1);
				string procType = "Procedures";

				MetaProcedure proc = new MetaProcedure (procSchema, procName, procType);
				procs.Add (proc);
			} 
			reader.Close ();
			reader = null;
			
			return procs;
		}

		public string GetSource (string objectName, string objectType) 
		{
			// TODO: get source to stored procedure from 
			// information_schema.routines and mysql.proc
			return "";
		}
	}
}
