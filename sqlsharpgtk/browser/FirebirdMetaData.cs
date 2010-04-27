// FirebirdMetaData.cs - meta data for Firebird SQL Database
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
using System.Reflection;
using System.Runtime.InteropServices;
using Mono.Data.SqlSharp.DatabaseBrowser;

namespace Mono.Data.SqlSharp.DatabaseBrowser
{
	public class FirebirdMetaData : IMetaData
	{
		private IDbConnection con;
		
		private static MethodInfo schemaMethod = null; 

		public FirebirdMetaData()
		{
		}

		public FirebirdMetaData(IDbConnection connection) 
		{
			this.con = connection;
		}

		public MetaTableCollection GetTables(bool includeSystemTables) 
		{
			if(con.State != ConnectionState.Open)
				con.Open();

			MetaTableCollection tables = new MetaTableCollection ();
			DataTable table = GetSchema ("Tables", new string[] {null, null, null, "TABLE"});

			for (int r = 0; r < table.Rows.Count; r++) 
			{
				DataRow row = table.Rows[r];
				string tableName = row["TABLE_NAME"].ToString();
				MetaTable mtable = new MetaTable(tableName);
				tables.Add(mtable);
			}
			
			return tables;
		}

		public MetaTableColumnCollection GetTableColumns(string owner, string tableName)
		{
			if(con.State != ConnectionState.Open)
				con.Open();

			MetaTableColumnCollection columns = new MetaTableColumnCollection (owner, tableName);
			DataTable table2 = GetSchema ("Columns", new string[] {null, null, tableName, null});
			for (int r = 0; r < table2.Rows.Count; r++) 
			{
				DataRow row2 = table2.Rows[r];

				string columnName =	row2["COLUMN_NAME"].ToString();
				string dataType = row2["COLUMN_DATA_TYPE"].ToString();

				int columnSize = 0;
				if (row2["COLUMN_SIZE"] != DBNull.Value)
					columnSize = (int) row2["COLUMN_SIZE"];

				int precision = 0;
				if (row2["NUMERIC_PRECISION"] != DBNull.Value)
					precision = (int) row2["NUMERIC_PRECISION"];
					
				int scale = 0;
				if (row2["NUMERIC_SCALE"] != DBNull.Value)
					scale = (int) row2["NUMERIC_SCALE"];

				bool isNullable = false; // FIXME: is nullable
				//short n = 0;
				//if (row2["IS_NULLABLE"] != DBNull.Value)
				//	n = (short) row2["IS_NULLABLE"];
				//	
				//if (n == 1)
				//	isNullable = true;

				int pos = 0; // FIXME: ordinal position
				//if (row2["ORDINAL_POSITION"] != DBNull.Value)
				//	pos = (int) row2["ORDINAL_POSITION"];

				MetaTableColumn column = new MetaTableColumn(
					"",
					tableName,
					columnName,
					dataType,
					columnSize,
					precision,
					scale,
					isNullable,
					pos);

				columns.Add(column);
			}
			
			return columns;
		}

		public MetaViewCollection GetViews (bool includeSystem) 
		{
			if (con.State != ConnectionState.Open)
				con.Open();

			MetaViewCollection views = new MetaViewCollection ();

			DataTable table2 = GetSchema ("Views", new string[] {null, null, null});
			for (int r = 0; r < table2.Rows.Count; r++) {
				DataRow row2 = table2.Rows[r];
				string viewName = row2["VIEW_NAME"].ToString();
				MetaView view = new MetaView (viewName);
				views.Add (view);
			}

			return views;
		}

		private DataTable GetSchema (string sCollectionName, string[] sRestrictions) 
		{
			if (schemaMethod == null) 
			{
				// get the proper GetSchema method on the FbConnection object
				MethodInfo [] conMethodInfo = con.GetType ().GetMethods ();
		
				for (int m = 0; m < conMethodInfo.Length; m++) 
				{
					if (conMethodInfo[m].Name.Equals ("GetSchema")) 
					{
						ParameterInfo[] parms = conMethodInfo[m].GetParameters ();
         
						if (parms.Length == 2)
						{
							if (parms[0].ParameterType.ToString ().Equals ("System.String") &&
								parms[1].ParameterType.ToString ().Equals ("System.String[]")) 
							{
								schemaMethod = conMethodInfo [m];
								m = conMethodInfo.Length;
							}
						}
					}
				}
			}

			object parm0 = (object) sCollectionName;
			object parm1 = (object) sRestrictions;

			object[] oParms = new object[2];
			oParms[0] = parm0;
			oParms[1] = parm1;

			// invoke GetSchema method
			return (DataTable) schemaMethod.Invoke (con, oParms);
		}

		public MetaProcedureCollection GetProcedures(string owner) 
		{
			if (con.State != ConnectionState.Open)
				con.Open();

			MetaProcedureCollection procs = new MetaProcedureCollection ();

			DataTable table2 = null;
			DataRow row2 = null;
			table2 = GetSchema ("Procedures", new string[] {null, null, null});
			for (int r = 0; r < table2.Rows.Count; r++) {
				row2 = table2.Rows[r];
				string procName = row2["PROCEDURE_NAME"].ToString();
				MetaProcedure proc = new MetaProcedure ("", procName, "Procedures");
				procs.Add (proc);
				row2 = null;
			}
			table2 = null;

			table2 = GetSchema ("Functions", new string[] {null, null, null, null});
			for (int r = 0; r < table2.Rows.Count; r++) {
				row2 = table2.Rows[r];
				string funcName = row2["FUNCTION_NAME"].ToString();
				MetaProcedure proc = new MetaProcedure ("", funcName, "Functions");
				procs.Add (proc);
				row2 = null;
			}
			table2 = null;

			return procs;				
		}

		public string GetSource (string objectName, string objectType) 
		{
			return "";
		}
	}
}

