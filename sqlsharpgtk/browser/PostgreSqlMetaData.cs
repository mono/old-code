// PostgreSqlMetaData.cs - meta data for PostgreSQL
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
	public class PostgreSqlMetaData : IMetaData
	{
		IDbConnection con;

		public PostgreSqlMetaData()
		{
		}

		public PostgreSqlMetaData(IDbConnection connection) 
		{
			con = connection;
		}

		public MetaTableCollection GetTables(bool includeSystemTables) 
		{
			if(con.State != ConnectionState.Open)
				con.Open();

			MetaTableCollection tables = new MetaTableCollection ();

			string sql = 
				"select schemaname, tablename " +
				"from pg_tables";

			sql = sql + " ORDER BY 1,2";

			IDbCommand cmd = con.CreateCommand();
			cmd.CommandText = sql;

			IDataReader reader = cmd.ExecuteReader();
			while(reader.Read()) {
				MetaTable table = new MetaTable(reader.GetString(0), reader.GetString(1));
				tables.Add(table);
			}
			reader.Close();
			reader = null;
			
			return tables;
		}

		public MetaViewCollection GetViews (bool includeSystem) 
		{
			if (con.State != ConnectionState.Open)
				con.Open ();

			MetaViewCollection views = new MetaViewCollection ();

			string sql = 
				"select schemaname, viewname " +
				"from pg_views";

			sql = sql + " ORDER BY 1,2";

			IDbCommand cmd = con.CreateCommand ();
			cmd.CommandText = sql;

			IDataReader reader = cmd.ExecuteReader ();
			while (reader.Read ()) {
				MetaView view = new MetaView (reader.GetString (0), reader.GetString (1));
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

			string sql = 
				"select c.relname as table_name, a.attname as column_name, " +
				" t.typname as date_type, a.attlen, a.atttypmod, a.attnotnull, a.attnum " +
				"from pg_class c, pg_attribute a, pg_type t " +
				"where c.relname = '" + tableName + "'" + 
				"and c.oid = a.attrelid " +
				"and a.attname not in ('tableoid','cmax','xmax','cmin','xmin','oid','ctid') " +
				"and t.oid = a.atttypid " +
				"and a.attnum > 0 " +
				"order by a.attnum";

			IDbCommand cmd = con.CreateCommand();
			cmd.CommandText = sql;

			IDataReader reader = cmd.ExecuteReader();
			while (reader.Read()) {	
				string sColumnName = reader.GetString(1);
				string sDataType = reader.GetString(2);
				int nLen = (int) reader.GetInt16(3);
				int nMod = (int) reader.GetInt32(4);
				bool bNullable = reader.GetBoolean(5);
				int nColId = (int) reader.GetInt16(6);

				int nSize = 0;
				int nPrecision = 0;
				int nScale = 0;

				switch(sDataType) 
				{
					case "int2":
					case "int4":
					case "int8":
						nSize = nLen;
						break;
					case "char":
					case "varchar":
					case "bpchar":
						nSize = nMod - 4;
						break;
					case "numeric":
						nPrecision = nMod / 65536;
						nScale = (nMod % 65536) - 4;
						break;
					case "text":
						break;
					default:
						nSize = nLen;
						break;
				}

				MetaTableColumn column = new MetaTableColumn(
					"",
					tableName,
					sColumnName,
					sDataType,
					nSize,
					nPrecision,
					nScale,
					bNullable,
					nColId);

				columns.Add(column);
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

			string sql = 
				"SELECT routine_schema, routine_name, routine_type " +
				"FROM information_schema.ROUTINES " +
				"ORDER BY 3,1,2";

			IDbCommand cmd = con.CreateCommand();
			cmd.CommandText = sql;

			IDataReader reader = cmd.ExecuteReader();
			while(reader.Read()) {			
				string procSchema = reader.GetString (0);
				string procName = reader.GetString (1);
				string procType = reader.GetString (2);

				if (procType.Equals("FUNCTION"))
					procType = "Functions";
				else if (procType.Equals("PROCEDURE"))
					procType = "Procedures";

				MetaProcedure proc = new MetaProcedure (procSchema, procName, procType);
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
