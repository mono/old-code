// GtkSharp.Generation.Parameters.cs - The Parameters Generation Class.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001-2002 Mike Kestner

namespace GtkSharp.Generation {

	using System;
	using System.Xml;

	public class Parameters  {
		
		private XmlElement elem;
		private string call_string;
		private string name_string;
		private bool is_callback;
		private bool unknown_types;

		public Parameters (XmlElement elem) {
			
			this.elem = elem;
			is_callback = false;
		}

		public string CallString {
			get {
				return call_string;
			}
		}

		public string NameString {
			get {
				return name_string;
			}
		}

		public bool IsCallback {
			get {
				return is_callback;
			}
		}

		public bool UnknownTypes {
			get {
				return unknown_types;
			}
		}
		public bool Validate ()
		{
			call_string = "";
			name_string = "";
			string mang_name;
			bool need_sep = false;
			
			foreach (XmlNode parm in elem.ChildNodes) {
				//if (parm.Name != "parameter") {
				//	continue;
				//}

				XmlElement p_elem = (XmlElement) parm;
				string type = p_elem.GetAttribute("type");
				string cs_type = SymbolTable.GetCSType(type);

				if (cs_type == "") 
					return false;

				if (cs_type == "_ERROR_")
					unknown_types = true;

				if (p_elem.GetAttribute("array") == "ss") {
					cs_type="IntPtr";
				}
				
				if (p_elem.GetAttribute("array") == "t") {
					cs_type += "[]";

					if (cs_type == "void[]")
						cs_type = "IntPtr";
				}
				
				if (need_sep) {
					call_string += ", ";
					name_string += ", ";
				} else {
					need_sep = true;
				}

				mang_name = MangleName(p_elem.GetAttribute("name"));

				call_string += (cs_type);
				call_string += (" " + mang_name);
				name_string += (mang_name);
			}

			if (call_string == "void ") {
				call_string = "";
				name_string = "";
			}
			
			return true;
		}

		private string MangleName(string name)
		{
			switch (name) {
				case "string":
					return "str1ng";
				case "event":
					return "evnt";
				case "object":
					return "objekt";
				case "params":
					return "prms";
				case "base":
					return "bse";
				case "ref":
					return "fer";
				default:
					break;
			}
			return name;
		}
	}
}

