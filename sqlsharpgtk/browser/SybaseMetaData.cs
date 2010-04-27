// SybaseMetaData.cs - meta data for Sybase
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
	public class SybaseMetaData : IMetaData
	{
		IDbConnection con;

		public SybaseMetaData()
		{
		}

		public SybaseMetaData(IDbConnection connection) 
		{
			con = connection;
		}

		public MetaTableCollection GetTables(bool includeSystemTables) 
		{
			if(con.State != ConnectionState.Open)
				con.Open();

			MetaTableCollection tables = new MetaTableCollection ();

			string where;
			if(includeSystemTables == true)
				where = "'U','S'";
			else
				where = "'U'";

			string sql = 
				"SELECT su.name AS owner, so.name as table_name, so.id as table_id, " +
				" so.crdate as created_date, so.type as table_type " +
				"FROM dbo.sysobjects so, dbo.sysusers su " +
				"WHERE type IN (" + where + ") " +
				"AND su.uid = so.uid " +
				"ORDER BY 1, 2";

			IDbCommand cmd = con.CreateCommand();
			cmd.CommandText = sql;

			IDataReader reader = cmd.ExecuteReader();
			while(reader.Read())
			{
				MetaTable table = new MetaTable(reader.GetString(0),
					reader.GetString(1));
				tables.Add(table);
			}
			reader.Close();
			reader = null;
			
			return tables;
		}

		public MetaViewCollection GetViews (bool includeSystem) {
			if (con.State != ConnectionState.Open)
				con.Open();

			MetaViewCollection views = new MetaViewCollection ();

			string sql = 
				"SELECT su.name AS owner, so.name as table_name, so.id as table_id, " +
				" so.crdate as created_date, so.type as table_type " +
				"FROM dbo.sysobjects so, dbo.sysusers su " +
				"WHERE type = 'V' " +
				"AND su.uid = so.uid " +
				"ORDER BY 1, 2";

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

		private int GetInt(IDataReader reader, int field) 
		{
			if(reader.IsDBNull(field) == true)
				return 0;
			
			object v = reader[field].ToString();
			
			string ds = v.ToString();
			int iss = Int32.Parse(ds);
			return iss;
		}

		public MetaTableColumnCollection GetTableColumns(string owner, string tableName)
		{
			if(con.State != ConnectionState.Open)
				con.Open();

			MetaTableColumnCollection columns = new MetaTableColumnCollection (owner, tableName);

			// TODO: get is nullable
			string sql = 
				"select su.name as owner, so.name as table_name, sc.name as column_name,  " +
				" st.name as date_type, sc.length as column_length,  " +
				" sc.prec as data_preceision, sc.scale as data_scale, " +
				" 0 as isnullable, sc.colid as column_id " +
				"from dbo.syscolumns sc, dbo.sysobjects so, " +
				"     dbo.systypes st, dbo.sysusers su " +
				"where sc.id = so.id " +
				"and so.type in ('U','S') " +
				"and so.name = '" + tableName + "' " + 
				"and sc.usertype = st.usertype " +
				"and su.uid = so.uid " +
				"order by sc.colid";

			IDbCommand cmd = con.CreateCommand();
			cmd.CommandText = sql;

			IDataReader reader = cmd.ExecuteReader();
			while(reader.Read())
			{
				MetaTableColumn column = new MetaTableColumn(
					reader.GetString(0),
					reader.GetString(1),
					reader.GetString(2),
					reader.GetString(3),
					GetInt(reader,4),
					GetInt(reader,5),
					GetInt(reader,6),
					GetInt(reader,7) == 1 ? true : false,
					GetInt(reader,8));
				columns.Add(column);
			}
			reader.Close();
			reader = null;
			
			return columns;
		}


		public MetaProcedureCollection GetProcedures(string owner) 
		{
			if (con.State != ConnectionState.Open)
				con.Open();

			MetaProcedureCollection procs = new MetaProcedureCollection ();

			string sql =
				"SELECT	su.name AS proc_owner, so.name as proc_name, " +
				"   CASE so.type " +
				"      WHEN 'P' THEN 'Stored Procedures' " +
				"      WHEN 'X' THEN 'Extended Procedures' " +
				"      WHEN 'FN' THEN 'Functions' " +
				"   END AS proc_type_desc " +
				"FROM dbo.sysobjects so, dbo.sysusers su " +
				"WHERE type in ('P', 'X', 'FN') " +
				"AND su.uid = so.uid " +
				"ORDER BY 3, 1, 2";

			IDbCommand cmd = con.CreateCommand ();
			cmd.CommandText = sql;

			IDataReader reader = cmd.ExecuteReader ();
			while (reader.Read ()) {
				MetaProcedure proc = new MetaProcedure (reader.GetString (0),
					reader.GetString (1), reader.GetString (2));
				procs.Add (proc);
			}
			reader.Close ();
			reader = null;

			return procs;		
		}

		public string GetSource (string objectName, string objectType) 
		{
			return "";
		}
	}
}
