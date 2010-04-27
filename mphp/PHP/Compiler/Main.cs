using TUVienna.CS_CUP.Runtime;
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Globalization;


namespace PHP.Compiler {


	public class MainClass {

		public static string Info;
		public static string Help;
		public static ArrayList PossibleOptions;

		static MainClass() {
			// info message
			StringBuilder infoSB = new StringBuilder();
			infoSB.Append("Mono PHP Compiler v0.2 by Raphael Romeikat" + Environment.NewLine);
			infoSB.Append("Licensed under GNU General Public License" + Environment.NewLine);
			infoSB.Append("http://php4mono.sourceforge.net" + Environment.NewLine);
			Info = infoSB.ToString();
			// help message
			StringBuilder helpSB = new StringBuilder();
			helpSB.Append("Usage: mono mPHP.exe [options] <source file>" + Environment.NewLine);
			helpSB.Append("Options:" + Environment.NewLine);
			helpSB.Append("-out:<file>             Specifies output file" + Environment.NewLine);
			helpSB.Append("-target:<kind>          Specifies the target (short: -t:)" + Environment.NewLine);
			helpSB.Append("                        <kind> is one of: exe, library" + Environment.NewLine);
			helpSB.Append("-reference:<file list>  References the specified assembly (short: -r:)" + Environment.NewLine);
			helpSB.Append("                        Files in <file list> separated by , or ;" + Environment.NewLine);
			helpSB.Append("-nowarn                 Disables warnings" + Environment.NewLine);
			helpSB.Append("-help                   Displays this usage message (short: -?)" + Environment.NewLine);
			Help = helpSB.ToString();
			// possible options
			PossibleOptions = new ArrayList();
			PossibleOptions.Add("out");
			PossibleOptions.Add("target");
			PossibleOptions.Add("t");
			PossibleOptions.Add("reference");
			PossibleOptions.Add("r");
			PossibleOptions.Add("nowarn");
			PossibleOptions.Add("?");
			PossibleOptions.Add("help");
		}

		public static void Main(string[] args) {
			try {
				// display info message
				Console.WriteLine(Info);

				// is there any parameter?
				if (args.Length == 0) {
					Report.Error(000);
					Console.WriteLine(Help);
					return;
				}

				// process parameters for options
				ArrayList desiredOptions = new ArrayList();
				string sourceFilename = null;
				for (int i = 0; i < args.Length; i++) {
					string option = args[i];
					// determine which option it is
					if (option.StartsWith("/"))
						option = option.Remove(0, 1);
					else if (option.StartsWith("--"))
						option = option.Remove(0, 2);
					else if (option.StartsWith("-"))
						option = option.Remove(0, 1);
					string desiredOption = option;
					if (option.ToLower().StartsWith("out:"))
						desiredOption = "out";
					else if (option.ToLower().StartsWith("target:"))
						desiredOption = "target";
					else if (option.ToLower().StartsWith("t:"))
						desiredOption = "t";
					else if (option.ToLower().StartsWith("reference:"))
						desiredOption = "reference";
					else if (option.ToLower().StartsWith("r:"))
						desiredOption = "r";
					// is option valid?
					if (PossibleOptions.Contains(desiredOption))
						desiredOptions.Add(option);
					else if (i == args.Length - 1)
						sourceFilename = option;
					else {
						Report.Error(004, desiredOption);
						return;
					}
				}
				// help option
				if (desiredOptions.Contains("?") || desiredOptions.Contains("help")) {
					Console.WriteLine(Help);
					return;
				}
				// determine source file
				if (sourceFilename == null) {
					Report.Error(000);
					Console.WriteLine(Help);
					return;
				}
				FileInfo sourceFile = null;
				StreamReader sourceFileStreamReader = null;
				try {
					sourceFile = new FileInfo(sourceFilename);
					FileStream sourceFileStream = new FileStream(sourceFile.FullName, FileMode.Open, FileAccess.Read);
					sourceFileStreamReader = new StreamReader(sourceFileStream, new UTF8Encoding());
				}
				catch (Exception) {
					Report.Error(001, sourceFile.FullName);
					return;
				}
				// other options
				foreach (string option in desiredOptions) {
					// out option
					if (option.StartsWith("out:"))
						PEmitter.OutputFile = new FileInfo(option.Remove(0, 4));
					// target option
					if (option.StartsWith("target:") || option.StartsWith("t:")) {
						string desiredTarget;
						if (option.StartsWith("target:"))
							desiredTarget = option.Remove(0, 7);
						else
							desiredTarget = option.Remove(0, 2);
						if (desiredTarget == "exe")
							PEmitter.Target = PEmitter.EXECUTABLE;
						else if (desiredTarget == "library")
							PEmitter.Target = PEmitter.LIBRARY;
						else {
							Report.Error(005, desiredTarget);
							return;
						}
					}
					// reference option
					if (option.StartsWith("reference:") || option.StartsWith("r:")) {
						string desiredReferences;
						if (option.StartsWith("reference:"))
							desiredReferences = option.Remove(0, 10);
						else
							desiredReferences = option.Remove(0, 2);
						char[] separators = new char[] {',', ';'};
						string[] references = desiredReferences.Split(separators);
						foreach (string reference in references)
							if (reference != "")
								SymbolTable.GetInstance().AddExternalAssembly(reference);
					}
					// nowarn option
					if (option == "nowarn") {
						Report.WarningsEnabled = false;
					}
				}
				// if no output file specified, use souce file name and adjust file extension
				if (PEmitter.OutputFile == null)
					PEmitter.OutputFile = new FileInfo(sourceFile.FullName);
				if (PEmitter.Target == PEmitter.EXECUTABLE && !PEmitter.OutputFile.Name.EndsWith(".exe")) {
					int indexLastDot = PEmitter.OutputFile.FullName.LastIndexOf('.');
					if (indexLastDot == -1)
						PEmitter.OutputFile = new FileInfo(PEmitter.OutputFile.FullName + ".exe");
					else
						PEmitter.OutputFile = new FileInfo(PEmitter.OutputFile.FullName.Substring(0, indexLastDot) + ".exe");
				}
				else if (PEmitter.Target == PEmitter.LIBRARY && !PEmitter.OutputFile.Name.EndsWith(".dll")) {
					int indexLastDot = PEmitter.OutputFile.FullName.LastIndexOf('.');
					if (indexLastDot == -1)
						PEmitter.OutputFile = new FileInfo(PEmitter.OutputFile.FullName + ".dll");
					else
						PEmitter.OutputFile = new FileInfo(PEmitter.OutputFile.FullName.Substring(0, indexLastDot) + ".dll");
				}

				// set formatting to US Amiercan (needed e.g. for type Double)
				System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
				// initialize parser
				Console.WriteLine("Parsing from file " + sourceFile.FullName + "...");
				Parser yy = new Parser(new Scanner(sourceFileStreamReader));
				// parse input stream
				AST ast = (AST)yy.parse().value;
				// create module
				PEmitter.BeginModule();
				// transform AST to create class __MAIN and function __MAIN()
				//Console.WriteLine("Working on class __MAIN and function __MAIN()...");
				new MainMethodVisitor().Visit(ast);
				// ensure every class (except __MAIN) has a constructor
				//Console.WriteLine("Working on constructors...");
				new ConstructorVisitor().Visit(ast);
				// ensure every method has a return statement and cut unreachable statements after a return
				//Console.WriteLine("Working on return statements...");
				new ReturnStatementVisitor().Visit(ast);
				// ensure there is no break/continue without a loop
				//Console.WriteLine("Working on loops...");
				new LoopVisitor().Visit(ast);
				// reorder class declarations by inheritance
				//Console.WriteLine("Working on interitance...");
				new InheritanceVisitor().Visit(ast);
				// collect all types so they may be used before being declared
				//Console.WriteLine("Working on types...");
				new TypesVisitor().Visit(ast);
				// collect all class variables and functions so they may be used before being declared
				//Console.WriteLine("Working on class variables and functions...");
				new ClassVariableAndFunctionsVisitor().Visit(ast);
				// build symbol table
				//Console.WriteLine("Building symbol table...");
				new SymbolTableVisitor().Visit(ast);
				// emit CIL code
				//Console.WriteLine("Emitting CIL code...");
				new EmitterVisitor().Visit(ast);
				// save module
				PEmitter.EndModule();
				// report success
				string success = "Compiling to file " + PEmitter.OutputFile.FullName + " succeeded";
				if (Report.NumberOfWarnings > 1)
					success += " with " + Report.NumberOfWarnings + " warnings";
				else if (Report.NumberOfWarnings == 1)
					success += " with 1 warning";
				Console.WriteLine(success);
			 }
			catch (Exception e) {
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
			}
		}

	}


}