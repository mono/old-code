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
	public class MetaProcedure {
		private string name = "";
		private string owner = "";
		private string procedureType = "";
		private string language = "";
		
		private MetaProcedureArgumentCollection args;
		
		private bool hasProcs = false;
		private MetaProcedureCollection procs;
		
		// created date

		internal MetaProcedure() {
		}

		public MetaProcedure(string procOwner, string procName, string procType) {
			this.owner = procOwner;
			this.name = procName;
			this.procedureType = procType;
			args = new MetaProcedureArgumentCollection ();
		}

		public MetaProcedure(string procOwner, string procName, string procType, bool hasProcs) {
			this.owner = procOwner;
			this.name = procName;
			this.procedureType = procType;
			this.hasProcs = hasProcs;
			if (hasProcs)
				this.procs = new MetaProcedureCollection ();
		}

		public bool HasProcedures {
			get {
				return hasProcs;
			}
			set {
				hasProcs = value;
			}
		}

		public MetaProcedureCollection Procedures {
			get {
				return procs;
			}
		}

		public string Name {
			get {
				return name;
			}
			
			set {
				name = value;
			}
		}

		public string Owner {
			get {
				return owner;
			}

			set {
				owner = value;
			}
		}

		public string ProcedureType {
			get {
				return procedureType;
			}

			set {
				procedureType = value;
			}
		}

		public string Language {
			get {
				return language;
			}
		
			set {
				language = value;
			}
		}

		public MetaProcedureArgumentCollection Arguments {
			get {
				return args;
			}
		}

		public override string ToString() 
		{
			if(owner != null)
				if(owner.Equals("") == false)
					return owner + "." + name;

			return name;
		}
	}
}

