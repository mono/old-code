// GtkSharp.Generation.CodeGenerator.cs - The main code generation engine.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace GtkSharp.Generation {

	using System;
	using System.IO;
	using System.Collections;
	using System.Xml;

	public class CodeGenerator  {

		public static int Main (string[] args)
		{
			if (args.Length != 2) {
				Console.WriteLine ("Usage: glgen <filename> <outputfile>");
				return 0;
			}

			Parser p = new Parser (args[0]);
			p.Parse ();
			Writer writer = new Writer(args[1], p.Namespace);
			
			foreach (IGeneratable gen in SymbolTable.Generatables) {
				gen.Generate (writer.sw);
			}

			writer.CloseWriter();

			Statistics.Report();
			return 0;
		}

	}
}
