// GtkSharp.Generation.CallbackGen.cs - The Callback Generatable.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2002 Mike Kestner

namespace GtkSharp.Generation {

	using System;
	using System.IO;
	using System.Xml;

	public class ConstGen : GenBase, IGeneratable  {

		private Parameters parms;

		public ConstGen (String ns, XmlElement elem) : base (ns, elem) 
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
                        string name = elem.GetAttribute("name");
                        string val = elem.GetAttribute("val");
                        sw.Write("\t\tpublic const uint ");
                        sw.Write(name + " = " + val);
                        sw.WriteLine(";");

                        Statistics.ConstsCount++;
		}
	}
}

