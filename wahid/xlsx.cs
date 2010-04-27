//
// xlsx.cs: Support for loading OOXML spreadsheets
//
// Authors:
//   Miguel de Icaza (miguel@novell.com)
//
// Copyright 2008 Novell, Inc (http://www.novell.com).
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.IO;
using System.Collections.Generic;

using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;


using System.Reflection;

namespace Wahid.SpreadsheetML {

	public enum CellType {
		Boolean,
		Error,
		InlineString,	// value inside <is> not inside <v> in the cell.
		Number,
		SharedString,
		FormulaString
	}
	
	public class LoadException : Exception {
		public LoadException (string msg) : base (msg) {}
	}
	
	public class Relationship {
		public enum RType {
			SharedStrings,
			Worksheet,
			Styles,
			Theme,
			CalcChain,
			Unknown
		}

		public string Id;
		public string Target;
		public RType  Type;

		public static RType Lookup (string s)
		{
			switch (s){
			case "http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings":
				return RType.SharedStrings;
			case "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet":
				return RType.Worksheet;
			case "http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles":
				return RType.Styles;
			case "http://schemas.openxmlformats.org/officeDocument/2006/relationships/theme":
				return RType.Theme;
			case "http://schemas.openxmlformats.org/officeDocument/2006/relationships/calcChain":
				return RType.CalcChain;
			}
			return RType.Unknown;
		}
		
		public Relationship (RType type, string id, string target)
		{
			Id = id;
			Target = target;
			Type = type;
		}
		
	}

	static public class OOXML {
		public static XNamespace ns_workbook = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
		public static XNamespace ns_rels = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

		public static XName c     = "c";
		public static XName f     = ns_workbook + "f";
		public static XName r     = "r";
		public static XName s     = "s";
		public static XName t     = "t";
		public static XName v     = ns_workbook + "v";
		public static XName is_   = "is";

		public static void ParseReference (string s, out int col, out int row)
		{
			col = 0;
			row = 0;

			if (s == null)
				return;
			
			foreach (char c in s){
				if (c >= 'A' && c <= 'Z'){
					col = col * 26 + ((int) c) - 'A';
				} else {
					row = row * 10 + ((int) c - '0');
				}
			}
		}

		public static CellType GetCellType (XElement xcell, CellType defvalue)
		{
			string type = (string) xcell.Attribute (t);

			switch (type){
			case "b":
				return CellType.Boolean;
			case "e":
				return CellType.Error;
			case "inlineStr":
				return CellType.InlineString;
			case "n":
				return CellType.Number;
			case "s":
				return CellType.SharedString;
			case "str":
				return CellType.FormulaString;
			case null:
				return defvalue;
			}
			throw new LoadException ("Unknown type for cell: " + type);				
		}
	}
	
	public class Xlsx {
		public Workbook Workbook { get; private set; }
		public string Error { get; private set; }

		Dictionary<string,Relationship> relationships;
		List<Value> sharedstrings;
		
		ZipAccess loader;

		public static Xlsx Load (string filename)
		{
			Xlsx x = new Xlsx (new DirAccess (filename));

			x.Load ();

#if false
			try {
			} catch (Exception e){
				if (x.Error == null)
					x.Error = "Generic loading error";
				x.Error += e;
				x.Workbook = null;
			}
#endif
	
			return x;
		}
		
		Xlsx (ZipAccess loader)
		{
			Workbook = new Workbook ();
			this.loader = loader;
			Error = null;
		}

		TextReader Get (string part)
		{
			try {
				return new StreamReader (loader.Get (part));
			} catch {
				Error = "Failure to retrieve part " + part;
				throw new LoadException (Error);
			}
		}

		class Dingus {
			public Dingus (string s)
			{
				Console.WriteLine (s);
			}
		}
		
		void LoadRelationships ()
		{
			relationships = new Dictionary<string,Relationship> ();
			
			XElement xrel = XElement.Load (Get ("xl/_rels/workbook.xml.rels"));

			var x = 
				from rel in xrel.Elements ()
				let type = Relationship.Lookup ((string) rel.Attribute ("Type"))
				where type != Relationship.RType.Unknown
				select new Relationship (type,
							 (string) rel.Attribute ("Id"),
							 (string) rel.Attribute ("Target"));
			relationships = x.ToDictionary (f => f.Id);
		}

		void LoadSharedStrings ()
		{
			sharedstrings = new List<Value> ();
			
			XElement xsharedstrings = null;
			
			try {
				xsharedstrings = XElement.Load (Get ("xl/sharedStrings.xml"));
			} catch {
				// not an error to not have sharedStrings
				return;
			}

			foreach (var ssi in xsharedstrings.Elements ()){
				XElement first = ssi.FirstNode as XElement;
				
				if (first.Name == OOXML.t)
					sharedstrings.Add (new StringValue (first.Value));
				else
					sharedstrings.Add (new RichStringValue (first));
			}
		}

		void LoadSheetData (Sheet sheet, XElement xsheetData)
		{
			// <row>
			foreach (XElement xrow in xsheetData.Elements ()){
				int row = Int32.Parse ((string) xrow.Attribute ("r"));

				// <c> 
				foreach (XElement xcell in xrow.Elements ()){
					int cell_col, cell_row;

					// r=
					OOXML.ParseReference ((string) xcell.Attribute (OOXML.r), out cell_col, out cell_row);

					if (row != cell_row){
						// this should not happen

						Error = "Document contains cell references outside of their row";
						throw new Exception ();
					}
					
					Cell cell = sheet.CreateCell (cell_col, cell_row);

					//CellType type = OOXML.GetCellType (xcell, CellType.Number);
					string type = (string) xcell.Attribute (OOXML.t);

					foreach (XElement cval in xcell.Elements ()){
						if (type == "inlineStr"){
							if (cval.Name == OOXML.is_)
								cell.Value = new RichStringValue (cval);
							else
								throw new LoadException ("Cell type is InlineString, but found other values");
						}

						if (cval.Name == OOXML.f){
							cell.Formula = new Formula (cval.Value);
							continue;
						}
						
						if (cval.Name == OOXML.v){
							string s = cval.Value;
							bool v;
							
							switch (type){
							case "b":
								if (bool.TryParse (s, out v))
									cell.Value = new BoolValue (v);
								else
									cell.Value = new BoolValue (int.Parse (s) == 1);
								break;
								
							case "e":
								cell.Value = new ErrorValue (s);
								break;

							case "n":
								cell.Value = new NumberValue (double.Parse (s));
								break;

							case "s":
								// Lookup the string in the share string table
								cell.Value = sharedstrings [Int32.Parse (s)];
								break;
								
							case "str":
								cell.Value = new StringValue (cval.Value);
								break;
							}
						}
					}
				}
			}
		}
				
		void LoadSheet (Sheet sheet, XElement xsheet)
		{
			XName xcols = OOXML.ns_workbook + "cols";
			XName xsheetData = OOXML.ns_workbook + "sheetData";

			foreach (XElement se in xsheet.Elements ()){
				if (se.Name == xsheetData)
					LoadSheetData (sheet, se);
			}
		}
		
		void LoadWorkbook ()
		{
			XElement xwork = XElement.Load (Get ("xl/workbook.xml"));
			
			// Since Linq.XPath is not working, work around it
			var sheets_and_sources = from l in xwork.Elements ()
				let xsheets = OOXML.ns_workbook + "sheets"
				let xrid = OOXML.ns_rels + "id"
				where l.Name == xsheets
				from s in l.Elements ()
				  let rtarget = relationships [(string) s.Attribute (xrid)].Target
				  let sheet = Workbook.CreateSheet ((string)s.Attribute ("name"),
								    (int) s.Attribute ("sheetId"))
				  select new { sheet, rtarget } ;

			foreach (var ss in sheets_and_sources)
				LoadSheet (ss.sheet, XElement.Load (Get ("xl/" + ss.rtarget)));
		}

		void Load ()
		{
			LoadRelationships ();
			// LoadStyle ();
			LoadSharedStrings ();
			LoadWorkbook ();
		}
	}
}