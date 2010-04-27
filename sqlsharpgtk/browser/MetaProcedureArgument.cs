// MetaProcedure.cs - represents a stored procedure or function
// 
// For Oracle, can also represent a package
// For a meta data provider, they do not have to subclass MetaProcedure.
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
using System.Text;

namespace Mono.Data.SqlSharp.DatabaseBrowser
{
	public class MetaProcedureArgument {
		private string procname = "";
		private string owner = "";
		private string argument = "";
		private string procedureType = "";
		private string direction = "";
		private string dataType = "";

		internal MetaProcedureArgument() 
		{
		}

		public MetaProcedureArgument(string procOwner, string procName, string procType,
						string argument, string direction, string dataType) 
		{
			this.owner = procOwner;
			this.procname = procName;
			this.procedureType = procType;
			this.argument = argument;
			this.direction = direction;
			this.dataType = dataType;
		}

		public string ProcedureName 
		{
			get 
			{
				return procname;
			}
			
			set 
			{
				procname = value;
			}
		}

		public string Owner 
		{
			get {
				return owner;
			}

			set {
				owner = value;
			}
		}

		public string ProcedureType 
		{
			get {
				return procedureType;
			}

			set {
				procedureType = value;
			}
		}

		public string ArgumentName {
			get {
				return argument;
			}

			set {
				argument = value;
			}
		}

		public string Direction {
			get {
				return direction;
			}

			set {
				direction = value;
			}
		}

		public string DataType {
			get {
				return dataType;
			}

			set {
				dataType = value;
			}
		}

		public override string ToString() 
		{
			return String.Format ("{0} {1} {2}", argument, direction, dataType);
		}
	}
}

