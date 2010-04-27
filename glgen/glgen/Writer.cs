// GtkSharp.Generation.GenBase.cs - The Generatable base class.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001-2002 Mike Kestner

namespace GtkSharp.Generation {

	using System;
	using System.IO;

	public class Writer {

		public StreamWriter sw;
		
		public Writer (String fnm, String ns)
		{

			FileStream stream = new FileStream (fnm,
			                         FileMode.Create, 
			                         FileAccess.Write);
			sw = new StreamWriter(stream);

			sw.WriteLine ("// Generated File.  Do not modify.");
			sw.WriteLine ("// <c> 2002 Mark Crichton");
			sw.WriteLine ();
			sw.WriteLine ("using System;");
			sw.WriteLine ("using System.Runtime.InteropServices;");
			sw.WriteLine ("namespace " + ns + " {");
			sw.WriteLine ("\tpublic class " + ns + " {");

			return;
		}

		public void CloseWriter()
		{
			sw.WriteLine ();
			sw.WriteLine ("\t}");
			sw.WriteLine ("}");
			sw.Flush();
			sw.Close();
		}
		
	}
}

