// GtkSharp.Generation.CallbackGen.cs - The Callback Generatable.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2002 Mike Kestner

namespace GtkSharp.Generation {

	using System;
	using System.IO;
	using System.Xml;

	public class FuncGen : GenBase, IGeneratable  {

		private Parameters parms;

		public FuncGen (String ns, XmlElement elem) : base (ns, elem) 
		{
			if (elem ["parameters"] != null)
				parms = new Parameters (elem ["parameters"]);
		}

		public String MarshalType {
			get
			{
				return QualifiedName;
			}
		}

		public String CallByName (String var_name)
		{
			return var_name;
		}

		public String FromNative(String var)
		{
			return var;
		}

		public void Generate (StreamWriter sw)
		{
			string lname, call;

                        if (parms != null) {
                        	parms.Validate();
                                call = "(" + parms.CallString + ")";
                                lname = "(" + parms.NameString + ")";
                        } else {
                        	call = "(BUG);";
                        	lname = "THIS_IS_A_BUG";
                        }

			string rettype = elem.GetAttribute("type");
			string name = elem.GetAttribute("name");
                        string s_ret = SymbolTable.GetCSType(rettype);

			if (parms.UnknownTypes || s_ret == "_ERROR_") {
				Console.WriteLine("skipped: " + name);
				Statistics.IgnoreCount++;
				return;
			}

                        if (s_ret == "void[]")
                        	s_ret = "IntPtr";

                        sw.WriteLine("\t\t[DllImport(\"" + SymbolTable.GetDllName(ns) + 
                        "\", CallingConvention=CallingConvention.Cdecl)]");
                        sw.WriteLine("\t\tstatic extern " + s_ret + " " + name + " " + call + ";");

                        sw.Write("\t\tpublic static " + s_ret + " ");
                        sw.Write(name.TrimStart(ns.ToCharArray()) + " ");
                        sw.WriteLine(call);
                        sw.WriteLine("\t\t{");

                        if (s_ret == "void") {
                        	sw.WriteLine("\t\t\t" + name + lname + ";");
                        } else {
                        	sw.WriteLine("\t\t\treturn " + name + lname + ";");
                        }

                        sw.WriteLine("\t\t}");
                        sw.WriteLine("");

			Statistics.FuncsCount++;
		}
	}
}

