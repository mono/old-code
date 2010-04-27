using Monotalk.CSharp;

namespace Monotalk.Indexer
{
	using System;
	using System.Collections;

	public class Part
	{
		public int fileID;
		public int startRow, endRow, startCol, endCol;

		public Part (int id, int sr, int er)
		{
			fileID = id;
			startRow = sr;
			endRow = er;
		}

		public Part (int id, TokenValue tv)
		{
			fileID = id;
			startRow = tv.sRow;
			endRow = tv.eRow;
		}
	}

	public class MemberDB
	{
		Hashtable members;

		public MemberDB ()
		{
			members = new Hashtable ();
		}

		public Part this [string member] {
			set {
				members [member] =  value;
			}
			get {
				return (Part) members [member];
			}
		}
	}

	public class TypeRecord
	{
		public string name;
		public MemberDB db;
		public Part part;

		public TypeRecord (string type)
		{
			db = new MemberDB ();
			name = type;
		}

		public TypeRecord (string type, Part p) : this (type)
		{
			part = p;
		}

		public MemberDB MemberDB {
			get {
				return db;
			}
		}
	}

	public class TypeDB
	{
		Hashtable types;

		public TypeDB ()
		{
			types = new Hashtable ();
		}

		public TypeRecord this [string type] {
			set {
				types [type] = value;
			}
			get {
				return (TypeRecord) types [type];
			}
		}
	}

	public class SourceDB
	{
		private Hashtable namespaces;
		private int types = 0;
		private int members = 0;

		public SourceDB ()
		{
			namespaces = new Hashtable ();
		}

		public int Members {
			get {
				return members;
			}
		}

		public int Types {
			get {
				return types;
			}
		}

		public TypeDB this [string Namespace] {
			set {
				namespaces [Namespace] =  value;
			}
			get {
				return (TypeDB) namespaces [Namespace];
			}
		}

		public Part LookupMember (string Namespace, string  type, string member)
		{
			TypeDB tdb = namespaces [Namespace] as TypeDB;

			Console.WriteLine ("lookup {0}.{1}.{2}", Namespace, type, member);
			if (tdb != null) {
				TypeRecord tr = tdb [type];
				if (tr != null)
					return tr.MemberDB [member];
			}

			return null;
		}

		public Part LookupType (string Namespace, string  type)
		{
			TypeDB tdb = namespaces [Namespace] as TypeDB;

			Console.WriteLine ("lookup namespace: {0} type: {1}", Namespace, type);
			if (tdb != null) {
				TypeRecord tr = tdb [type];
				if (tr != null)
					return tr.part;
			}

			return null;
		}

		public void AddMember (string Namespace, string type, string member, Part part)
		{
			TypeDB tdb = this [Namespace] as TypeDB;

			Console.WriteLine ("add {0}.{1}.{2}", Namespace, type, member);
			if (tdb == null)
				tdb = this [Namespace] = new TypeDB ();
			TypeRecord tr = tdb [type];
			if (tr == null) {
				tr = tdb [type] = new TypeRecord (type);
				types ++;
			}
			if (tr.MemberDB [member] == null)
				members ++;
			tr.MemberDB [member] = part;
		}

		public void AddType (string Namespace, string type, Part part)
		{
			TypeDB tdb = this [Namespace] as TypeDB;

			Console.WriteLine ("add type namespace: {0} type: {1}", Namespace, type);
			if (tdb == null)
				tdb = this [Namespace] = new TypeDB ();
			TypeRecord tr = tdb [type];
			if (tr == null)
				tdb [type] = new TypeRecord (type, part);
			else
				tr.part = part;
		}
	}
}
