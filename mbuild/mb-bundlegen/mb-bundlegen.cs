//
// mb-bundlegen.cs -- CodeDOM-based bundle generation tool
//

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using Microsoft.VisualBasic;

using Mono.GetOptions;

namespace MBBundleGen {

	public class MainClass : Options {

		public MainClass () : base () {
			// Be more mcs-like in option handling
			//BreakSingleDashManyLettersIntoManyOptions = true;
			//ParsingMode = OptionsParsingMode.Linux;
		}

		// Options

		[Option("Debug the bundle file parser", ' ', "debug-parser")]
		public bool debug_parser = false;

		[Option("Save output in {FILENAME}", ' ', "out")]
		public string outfile = null;

		[Option("Only output the code that would be compiled", ' ', "debug-codegen")]
		public bool debug_codegen = false;

		[Option("Keep the temporary files used by the compiler", ' ', "debug-preserve")]
		public bool debug_preserve = false;

		[Option("Generate debugging information in the output assembly", 'g', "gen-debug-info")]
		public bool gen_debug_info = false;

		[Option("The bundle code is in language {LANG} (csharp,vb)", 'l', "lang")]
		public string source_language = "csharp";

		[Option("Sign the assembly with the strongname keypair in {KEYFILE}", 'k', "keyfile")]
		public string keyfile = null;

		[Option("The version string given to the assembly", 'v', "assembly-version")]
		public string assembly_version = null;

		List<string> refs = new List<string> ();

		[Option(-1, "Reference {ASSEMBLYNAME} in the compilation", 'r', "ref")]
		public WhatToDoNext DoReference (string name) {
			refs.Add (name);
			return WhatToDoNext.GoAhead;
		}

		protected ArrayList natives = new ArrayList ();

		[Option(-1, "Include native source file {FILENAME} in the compilation", 'n', "native")]
		public WhatToDoNext DoNative (string name) {
			natives.Add (name);
			return WhatToDoNext.GoAhead;
		}

		protected Hashtable resources = new Hashtable ();

		[Option(-1, "Embed resources {R1,R2,...} in the resulting assembly", ' ', "resource")]
		public WhatToDoNext DoResource (string name) {
			string id = Path.GetFileName (name);
			resources[id] = name;

			return WhatToDoNext.GoAhead;
		}

		public override WhatToDoNext DoAbout () {
			base.DoAbout ();
			return WhatToDoNext.AbandonProgram;
		}

		// Do it!

		CodeDomProvider prov = null;
		string resource_arg = null;

		public int Launch () {
			Parser.DebugParser = debug_parser;

			if (RemainingArguments.Length == 0) {
				DoAbout ();
				return 1;
			}

			if (keyfile == null) {
				Console.Error.WriteLine ("A keypair file must be specified to sign the assembly.");
				return 1;
			}

			if (assembly_version == null) {
				Console.Error.WriteLine ("An assembly version must be given to install the assembly.");
				return 1;
			}

			if (!File.Exists (keyfile)) {
				Console.Error.WriteLine ("The specified keypair file \"{0}\" does not exist.", keyfile);
				return 1;
			}


			if (outfile == null) {
				string inbase = Path.GetFileNameWithoutExtension (RemainingArguments[0]);

				// hacky ...
				outfile = "MBuildDynamic." + inbase + ".dll";
			}

			switch (source_language) {
			case "csharp":
				prov = new CSharpCodeProvider();
				resource_arg = "/resource:";
				break;
			case "vb":
				prov = new VBCodeProvider ();
				resource_arg = "/resource:"; // ???
				break;
			default:
				Console.Error.WriteLine ("Unknown source language \"{0}\" -- cannot continue.",
						   source_language);
				return 1;
			}

			Driver driver = new Driver ();

			foreach (string s in refs)
			    if (driver.LoadAssembly (s))
				return 1;

			for (int i = 0; i < RemainingArguments.Length; i++) {
			    if (driver.ParseFile (RemainingArguments[i]))
				return 1;
			}

			if (driver.Prepare ())
			    return 2;

			if (debug_codegen)
			    return DoDebugCodegen (driver.GetUnits (assembly_version, keyfile));

			foreach (string nfile in natives)
			    if (driver.AddNativeFile (nfile))
				return 1;

			return DoCompilePass (driver.GetUnits (assembly_version, keyfile));
		}

		int DoCompilePass (Dictionary<string,CodeCompileUnit> units) {
			CompilerParameters parms = new CompilerParameters ();

			// Params

			parms.GenerateExecutable = false;
			parms.OutputAssembly = outfile;
			parms.TempFiles.KeepFiles = debug_preserve;
			parms.IncludeDebugInformation = gen_debug_info;

			foreach (string assy in refs)
				parms.ReferencedAssemblies.Add (assy);

			foreach (string id in resources.Keys) {
				string file = (string) resources[id];

				parms.CompilerOptions += String.Format ("{0}{1},{2} ", resource_arg,
									file, id);
			}

			// Do it

			CodeCompileUnit[] array = new CodeCompileUnit[units.Count];
			units.Values.CopyTo (array, 0);

			CompilerResults res = prov.CompileAssemblyFromDom (parms, array);

			if (debug_preserve) {
				Console.Error.WriteLine ("Source files not being deleted:");
				foreach (string s in parms.TempFiles)
					Console.Error.WriteLine ("        {0}", s);
				Console.Error.WriteLine ();
			}

			if (res.Errors.Count > 0) {
				foreach (string s in res.Output)
					Console.Error.WriteLine ("{0}", s);

				foreach (CompilerError err in res.Errors)
					Console.Error.WriteLine ("{0}", err);
			}

			return res.NativeCompilerReturnValue;
		}

		int DoDebugCodegen (Dictionary<string,CodeCompileUnit> units) {
			IndentedTextWriter itw = new IndentedTextWriter(Console.Out,  "\t");
			CodeGeneratorOptions opts = new CodeGeneratorOptions();

			foreach (string name in units.Keys) {
				Console.WriteLine ("### Source: {0}", name);
				Console.WriteLine ();

				try {
					prov.GenerateCodeFromCompileUnit(units[name], itw, opts);
				} catch (Exception e) {
					Console.WriteLine ("Caught exception trying to generate code: {0}", e);
					return 1;
				}
			}

			itw.Close();            
			return 0;
		}

		public static int Main (string[] args) {
			MainClass options = new MainClass ();
			
			options.ProcessArgs (args);
			return options.Launch ();
		}
	}
	
}
