// Schema.cs
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
	public class Schema
	{
		IMetaData metaData;

		public Schema()
		{
		}

		public Schema(string factory, IDbConnection con)
		{
			switch(factory) {
			// TODO: load from XML config file instead hard-coding
			case "System.Data.OracleClient":
				metaData = new OracleMetaData(con);
				break;
			case "System.Data.SqlClient":
				metaData = new SqlMetaData(con);
				break;
			case "ByteFX.Data.MySqlClient":
			case "MySql.Data":
				metaData = new MySqlMetaData(con);
				break;
			case "Npgsql":
				metaData = new PostgreSqlMetaData(con);
				break;
			case "FirebirdSql.Data.Firebird":
				metaData = new FirebirdMetaData(con);
				break;
			case "Mono.Data.SybaseClient":
				metaData = new SybaseMetaData(con);
				break;
			case "Mono.Data.SqliteClient":
				metaData = new SqliteMetaData(con);
				break;
			default:
				break;
			}
		}

		public IMetaData MetaData {
			get {
				return metaData;
			}

			set {
				metaData = value;
			}
		}

		public MetaTableCollection GetTables(bool includeSystem) 
		{
			try {
				return metaData.GetTables(false);
			} catch (NotImplementedException n) {
				return new MetaTableCollection ();
			}
		}

		public MetaTableColumnCollection GetTableColumns(string owner, string tableName) 
		{
			try {
				return metaData.GetTableColumns(owner, tableName);
			} catch (NotImplementedException n) { 
				return new MetaTableColumnCollection ();
			}
		}

		public MetaViewCollection GetViews(bool includeSystem) 
		{
			try {
				return metaData.GetViews (true);
			} catch (NotImplementedException n) {
				return new MetaViewCollection ();
			}
		}

		public MetaProcedureCollection GetProcedures(string owner)  
		{
			try {
				return metaData.GetProcedures (owner);
			} catch (NotImplementedException n) {
				return new MetaProcedureCollection ();
			}
		}

		public string GetSource (string objectName, string objectType) 
		{
			try {
				return metaData.GetSource (objectName, objectType);
			} catch (NotImplementedException n) {
				return "";
			}
		}
	}
}

