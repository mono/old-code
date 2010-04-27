// OracleMetaData.cs - meta data for Oracle
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
using System.Text;
using System.Globalization;

namespace Mono.Data.SqlSharp.DatabaseBrowser
{
	public class OracleMetaData : IMetaData
	{
		private IDbConnection con;

		public OracleMetaData()
		{
		}

		public OracleMetaData(IDbConnection connection) 
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
				where = "";
			else
				where = "WHERE OWNER NOT IN ('SYS','SYSTEM','MDSYS','CTXSYS','WMSYS','WKSYS')";

			string sql = "SELECT OWNER, TABLE_NAME, TABLESPACE_NAME " +
						 "FROM ALL_TABLES " +
						 where + 
						 " ORDER BY 1, 2";

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

		public MetaViewCollection GetViews (bool includeSystem) 
		{
			if (con.State != ConnectionState.Open)
				con.Open();

			MetaViewCollection views = new MetaViewCollection ();

			string where = "";
			if (includeSystem == true)
				where = "";
			else
				where = "WHERE OWNER NOT IN ('SYS','SYSTEM','MDSYS','CTXSYS','WMSYS','WKSYS')";

			string sql = "SELECT OWNER, VIEW_NAME " +
				"FROM ALL_VIEWS " +
				where +
				" ORDER BY 1, 2";

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

		private int GetInt (IDataReader reader, int field) 
		{
			if (reader.IsDBNull(field) == true)
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

			MetaTableColumnCollection columns = new MetaTableColumnCollection (owner,tableName);

			string sql = 
				"SELECT OWNER, TABLE_NAME, COLUMN_NAME, " +
				"DATA_TYPE, DATA_LENGTH, DATA_PRECISION, DATA_SCALE, NULLABLE, COLUMN_ID " +
				"FROM ALL_TAB_COLUMNS " +
				"WHERE OWNER = '" + owner + "' " +
				"AND TABLE_NAME = '" + tableName + "' " +
				"ORDER BY COLUMN_ID";

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
					reader.GetString(7).Equals("N") ? false : true,
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
				"SELECT OWNER, OBJECT_NAME, " +
				" DECODE(OBJECT_TYPE,'PROCEDURE','Procedures','FUNCTION','Functions','PACKAGE','Packages') AS OBJ_TYPE " +
				"FROM ALL_OBJECTS " +
				"WHERE OBJECT_TYPE IN ('PROCEDURE','FUNCTION','PACKAGE') " +
				"ORDER BY 3, 1, 2";

			IDbCommand cmd = con.CreateCommand ();
			cmd.CommandText = sql;

			IDataReader reader = cmd.ExecuteReader ();
			while (reader.Read ()) {
				string procOwner = reader.GetValue(0).ToString ();
				string procName = reader.GetValue(1).ToString ();
				string procType = reader.GetValue(2).ToString ();

				MetaProcedure proc = null;
				if (procType.Equals("Packages")) {
					proc = new MetaProcedure (procOwner, procName, procType, true);
					GetPackageProcedures (proc);
				}
				else {
					proc = new MetaProcedure (procOwner, procName, procType);
					GetProcedureArguments (proc);
				}

				// add to stored object to collection
				procs.Add (proc);
			}
			reader.Close ();
			reader = null;

			return procs;		
		}

		// get procedures/functions for a given stored package and get the
		// arguments for each procedure/function
		private void GetPackageProcedures (MetaProcedure pkg) 
		{
			if (con.State != ConnectionState.Open)
				con.Open();

			// GET ARGUMENTS FOR PROCEDURES/FUNCTIONS FOR PACKAGES
			string sql = 
				"SELECT OBJECT_NAME, OVERLOAD, NVL(ARGUMENT_NAME,'(RETURN)') AS ARGUMENT_NAME, " +
				"	POSITION, SEQUENCE, " +
				"	IN_OUT AS DIRECTION, " +
				"	DECODE(TYPE_NAME, NULL, DATA_TYPE, TYPE_OWNER || '.' || TYPE_NAME) AS DATA_TYPE " +
				"FROM SYS.ALL_ARGUMENTS " +
				"WHERE OWNER = '" + pkg.Owner + "' " +
				"AND PACKAGE_NAME = '" + pkg.Name + "' " +
				"AND DATA_LEVEL = 0 " +
				"ORDER BY OBJECT_NAME, OVERLOAD, POSITION, SEQUENCE, DATA_LEVEL";

			IDbCommand cmd = con.CreateCommand ();
			cmd.CommandText = sql;

			IDataReader reader = cmd.ExecuteReader ();

			// Notes:
			// 1. an Oracle stored package can overloaded functions or procedures
			// 2. stand-alone functions or procedures can not be overloaded
			// 3. a procedure with no arguments will still have one row - data_type will be null
			string previousProcName = "~";
			string previousOverload = "~";
			MetaProcedure proc = null;
			string procType = "Procedures";
			while (reader.Read ()) {
				string procName = reader.GetString (0);
				string argName = reader.GetString (2);

				string overload = String.Empty;
				if (!reader.IsDBNull (1))
					overload = reader.GetString (1);

				string direction = String.Empty;
				if (!reader.IsDBNull (5))
					direction = reader.GetString (5);
				
				string dataType = String.Empty;
				if (!reader.IsDBNull (6)) 
					dataType = reader.GetString (6);

				if (!procName.Equals (previousProcName) || !previousOverload.Equals (overload)) {
					if (argName.Equals ("(RETURN)") && (!dataType.Equals (String.Empty))) {
						procType = "Functions";
						direction = String.Empty;
					}
					else
						procType = "Procedures";

					proc = new MetaProcedure (String.Empty, procName, procType);
					pkg.Procedures.Add (proc);
					
					previousProcName = procName;
					previousOverload = overload;
				}

				if (!dataType.Equals (String.Empty)) {
					MetaProcedureArgument arg = new MetaProcedureArgument (pkg.Owner, procName, procType,
						argName, direction, dataType);
					proc.Arguments.Add (arg);
				}
			}
			reader.Close ();
			reader = null;
		}

		// get arguments for stand-alone stored procedures/functions
		private void GetProcedureArguments (MetaProcedure proc) 
		{
			if (con.State != ConnectionState.Open)
				con.Open();

			// GET ARGUMENTS FOR STAND-ALONE PROCEDURES/FUNCTIONS
			string sql = "SELECT OBJECT_NAME, OVERLOAD, NVL(ARGUMENT_NAME,'(RETURN)') AS ARGUMENT_NAME, " +
				"	POSITION, SEQUENCE, " +
				"	IN_OUT AS DIRECTION, " +
				"	DECODE(TYPE_NAME, NULL, DATA_TYPE, TYPE_OWNER || '.' || TYPE_NAME) AS DATA_TYPE " +
				"FROM SYS.ALL_ARGUMENTS " +
				"WHERE OWNER = '" + proc.Owner + "' " +
				"AND OBJECT_NAME = '" + proc.Name + "' " +
				"AND PACKAGE_NAME IS NULL " + 
				"AND DATA_LEVEL = 0 AND DATA_TYPE IS NOT NULL " +
				"ORDER BY OBJECT_NAME, OVERLOAD, POSITION, SEQUENCE, DATA_LEVEL";

			IDbCommand cmd = con.CreateCommand ();
			cmd.CommandText = sql;
			IDataReader reader = cmd.ExecuteReader ();
			string procType = "Procedures";
			while (reader.Read ()) {
				string procName = reader.GetString (0);
				string argName = reader.GetString (2);
 
				if (argName.Equals ("(RETURN)"))
					procType = "Functions";
				else
					procType = "Procedures";

				string direction = reader.GetString (5);
				if (argName.Equals ("(RETURN)"))
					direction = String.Empty;
				string dataType = reader.GetString (6);
				MetaProcedureArgument arg = new MetaProcedureArgument(proc.Owner, procName, procType,
					argName, direction, dataType);

				proc.Arguments.Add (arg);
			}
			reader.Close ();
			reader = null;
		}

		public string GetSource (string objectName, string objectType) 
		{
			string owner = "";
			string name = "";

			GetOwnerAndName (objectName, out owner, out name);
	
			string procType = "";
			switch (objectType) {
			case "Procedures":
				procType = " = 'PROCEDURE'";
				break;
			case "Functions":
				procType = " = 'FUNCTION'";
				break;
			case "Packages":
				procType = " IN ('PACKAGE','PACKAGE BODY')";
				break;
			}
			
			string sql = String.Format (
				"SELECT TYPE, TEXT " +
				"FROM ALL_SOURCE " +
				"WHERE OWNER = '{0}' " +
				"AND NAME = '{1}' " +
				"AND TYPE {2} " +
				"ORDER BY TYPE, LINE", owner, name, procType);
			
			IDbCommand cmd = con.CreateCommand ();
			cmd.CommandText = sql;
			IDataReader reader = cmd.ExecuteReader ();

			StringBuilder sb = new StringBuilder ();

			while (reader.Read ()) {
				string text = reader.GetString (1);
				sb.Append (text);
			}

			reader.Close ();
			reader = null;

			return sb.ToString ();
		}

		// takes a object name like "owner.name" and parses it into "owner" and "name" strings
		private void GetOwnerAndName (string objectName, out string owner, out string name) 
		{
			int idx = objectName.IndexOf (".");
			owner = objectName.Substring (0, idx);
			name = objectName.Substring (idx + 1);
		}

		// TODO: use this to get the errors after compiling a
		//  stored procedure, function, or package
		//  maybe even creating a DbError and DbErrorCollection class
		public IDataReader GetErrors (string objectName, string objectType) 
		{
			string owner = "";
			string name = "";

			GetOwnerAndName (objectName, out owner, out name);
			
			string procType = "";
			switch (objectType) {
			case "Procedures":
				procType = " = 'PROCEDURE'";
				break;
			case "Functions":
				procType = " = 'FUNCTION'";
				break;
			case "Packages":
				procType = " IN ('PACKAGE','PACKAGE BODY')";
				break;
			}
			
			string sql = String.Format (
				"SELECT LINE, POSITION, POSITION, TEXT, ATTRIBUTE " +
				"FROM ALL_ERRORS " +
				"WHERE OWNER = '{0}' " +
				"AND NAME = '{1}' " +
				"AND TYPE {2} " +
				"ORDER BY SEQUENCE", owner, name, procType);
			
			IDbCommand cmd = con.CreateCommand ();
			cmd.CommandText = sql;
			IDataReader reader = cmd.ExecuteReader ();
			return reader;
		}
	}
}
