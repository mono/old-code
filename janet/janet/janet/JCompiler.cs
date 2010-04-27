// JCompiler.cs: JANET compiler entry point
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
using System.Reflection;

using JANET.Printer;
using JANET.Compiler;
using JANET.Runtime;


namespace JANET.Compiler {

public class Compiler
	{
	private static TokenListNode CaptureTokens(Tokenizer tokenizer)
		{
		TokenListNode head = null;
		TokenListNode tail = null;
		while (true)
			{
			Token tok = tokenizer.Match();
			if (tok == null)
				break;
			
			TokenListNode newNode = new TokenListNode();
			newNode.token = tok;
			newNode.next = null;
			if (head == null)
				head = newNode;
			else
				tail.next = newNode;
			
			tail = newNode;
			}
		
		return head;
		} // CaptureTokens
	
	
	// Read JavaScript source text and output a C# file.  progClassName
	// is the name of the C# class we generate.  inputFileLabel denotes
	// the source of the input text; it is inserted in a comment in the
	// C# source.
	public static void CompileToCSharp( TextReader input, TextWriter output,
										string progClassName,
										string inputFileLabel,
										bool forEvalCode )
		{
		Tokenizer tokenizer = new Tokenizer(input);
		TokenListNode tokenList = CaptureTokens(tokenizer);
		
		Phase1Parser parser = new Phase1Parser(new Retokenizer(tokenList));
		
		ProgramInfo programInfo = new ProgramInfo();
		FunctionInfo rootFunc = new FunctionInfo(programInfo, null, null);
		
		parser.ParseProgram(rootFunc);
		
		PrettyPrinter pp = new PrettyPrinter(output);
		
		pp.Line("// JANET compiler output for source file " + inputFileLabel);
		pp.Line("// ");
		
		CSharpGenerator gen = new CSharpGenerator( rootFunc, pp, progClassName,
												   forEvalCode );
		Phase2Parser parser2 = new Phase2Parser( new Retokenizer(tokenList),
												 gen );
		parser2.ParseProgram(rootFunc);
		} // CompileToCSharp
	
	
	// Read JavaScript source text, output a C# file to disk, and compile
	// the C# file to produce a DLL.
	// 
	// The input, progClassName, and inputFileName parameters are as for
	// CompileToCSharp (above).  fileNameBase is used to generate the C#
	// and DLL file names (we append ".cs" or ".dll" to fileNameBase).
	public static void CompileToDLL( TextReader input,
									 string progClassName,
									 string inputFileLabel,
									 string fileNameBase,
									 bool forEvalCode,
									 out string dllFileName )
		{
		// Compile to a C# string.
		StringWriter output = new StringWriter();
		CompileToCSharp(input, output, progClassName, inputFileLabel, forEvalCode);
		string cSharpCode = output.ToString();
		
		// Compile the C# code.
		string csFileName = fileNameBase + ".cs";
		dllFileName = fileNameBase + ".dll";
		
		Stream cSharpStream = File.Open(csFileName, FileMode.Create);
		StreamWriter cSharpWriter = new StreamWriter(cSharpStream);
		cSharpWriter.Write(cSharpCode);
		cSharpWriter.Close();
		cSharpStream.Close();
		
		ProcessStartInfo psi = new ProcessStartInfo();
		psi.FileName = "cmd.exe";
		
		string compileString = "/c csc /debug+ /target:library /warn:0 /nologo";
		compileString += " /reference:obj\\JPrimitive.dll;obj\\JObjects.dll;obj\\JRuntime.dll";
		compileString += String.Format(" {0} > turtleCmd_temp.out", csFileName);
		
		if (File.Exists("turtleCmd_temp.out"))
			File.Delete("turtleCmd_temp.out");
		
		psi.Arguments = compileString;
		psi.WindowStyle = ProcessWindowStyle.Minimized;
		Process proc = Process.Start(psi);
		proc.WaitForExit();
		
		// Check for any error output from the compiler.
		int exitCode = proc.ExitCode;
		if (exitCode != 0)
			{
			if (File.Exists("turtleCmd_temp.out"))
				{
				StreamReader reader = File.OpenText("turtleCmd_temp.out");
				string outputText = reader.ReadToEnd();
				reader.Close();
				
				throw new Exception(String.Format(
					"csc returned exit code {0} with output text\n\r\n\r{1}",
					exitCode, outputText ));
				}
			else
				throw new Exception(String.Format( "csc returned exit code {0}",
												   exitCode ));
			}
		
		} // CompileToDLL
	
	
	// Read JavaScript source text, output a C# file to disk, compile
	// the C# file to produce a DLL, load the DLL, and return the
	// program class.
	// 
	// All parameters are as for CompileToDLL.
	public static Type CompileAndLoad( TextReader input,
									   string progClassName,
									   string inputFileLabel,
									   string fileNameBase )
		{
		string dllFileName;
		CompileToDLL( input, progClassName, inputFileLabel, fileNameBase,
					  false, out dllFileName );
		
		Assembly a = Assembly.LoadFrom(dllFileName);
		Type theType = a.GetType(progClassName);
		return theType;
		// ConstructorInfo constructor = theType.GetConstructor(Type.EmptyTypes);
		// object progInstance = constructor.Invoke(null);
		// return (IJANETProgram)progInstance;
		} // CompileAndLoad
	
	
	static Type[] joTypeArray = new Type[1] { typeof(JObject) };
	
	// Similar to CompileAndLoad, but the caller supplies the global-variable
	// object.  Used for running "eval" code.
	// 
	// HACK snewman 8/26/01: this is far from a complete implementation of
	// "eval".  For one thing, there's no way to supply the caller's
	// activation frame.
	public static Type CompileAndLoadForEval( TextReader input,
											  string progClassName,
											  string inputFileLabel,
											  string fileNameBase,
											  JObject globals )
		{
		string dllFileName;
		CompileToDLL( input, progClassName, inputFileLabel, fileNameBase,
					  true, out dllFileName );
		
		Assembly a = Assembly.LoadFrom(dllFileName);
		Type theType = a.GetType(progClassName);
		return theType;
		// ConstructorInfo constructor = theType.GetConstructor(joTypeArray);
		// object progInstance = constructor.Invoke(new object[1] {globals});
		// return (IJANETProgram)progInstance;
		} // CompileAndLoadForEval
	
	} // Compiler

} // namespace JANET.Compiler