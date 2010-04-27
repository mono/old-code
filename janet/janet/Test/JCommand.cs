// JCompiler.cs: JANET compiler command-line tool
//
// Author: Steve Newman (steve@snewman.net)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Bitcraft, Inc.


#define TRACE

using System;
using System.IO;
using System.Text;
using System.Diagnostics;

using JANET.Printer;
using JANET.Compiler;


// HACK SN 7/14/01: this is a simple test driver, which needs to be replaced
// by a more robust command-line tool.
class MainApp
	{
	public static int Main(string[] args)
		{
		string progClassName;
		string inputFileLabel;
		
		StreamReader r = null;
		bool weBuiltReader = false;
		if (args.Length >= 1)
			{
			inputFileLabel = "\"" + args[0] + "\"";
			FileStream fs = new FileStream(args[0], FileMode.Open, FileAccess.Read);
			r = new StreamReader(fs);
			weBuiltReader = true;
			
			progClassName = "Program_";
			for (int i=0; i<args[0].Length; i++)
				{
				char c = args[0][i];
				if ( (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') ||
					 (c >= '0' && c <= '9') || c == '_' )
					progClassName += c;
				else
					break;
				}
			}
		else
			{
			inputFileLabel = "<stdin>";
			progClassName = "Program";
			}
		
		if (args.Length >= 2)
			progClassName = args[1];
		
		try
			{
			Compiler.CompileToCSharp( r, Console.Out, progClassName,
									  inputFileLabel, false );
			}
		catch (ParseError e)
			{
			Console.Error.WriteLine( "ParseError at line {0}, column {1} (byte position {2})",
									 e.loc.lineNum, e.loc.colNum, e.loc.absPosition );
			Console.Error.WriteLine( "Message: {0}", e.Message );
			}
		
		// while (true)
		// 	{
		// 	Token tok = tokenizer.Match();
		// 	if (tok == null)
		// 		break;
		// 	
		// 	Console.WriteLine(tok.rawText);
		// 	} // while (true)
		
		if (weBuiltReader)
			r.Close();
		
		return 0;
		} // Main
	
	} // MainApp
