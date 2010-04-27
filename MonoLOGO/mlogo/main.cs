namespace Mono.Languages.Logo.Compiler {
	
	using System;
	using System.Collections;
	using System.IO;
	using Mono.Languages.Logo.Runtime;

	class Driver {
		IMessageStoreCollection funcs;
		LogoMessageTarget lmt;
		
		private void LoadFuncs () {
			funcs = new IMessageStoreCollection ();
			funcs.Add (new CTSMessageTarget (typeof (Funcs)));
			lmt = new LogoMessageTarget ();
			funcs.Add (lmt);
		}

		private InstructionList Parse (string filename) {
			Parser parser = new Parser (funcs, lmt);
			FileStream stream = new FileStream (filename, FileMode.Open);
			return parser.Parse (new StreamReader (stream));
		}

		private void Interpret (InstructionList tree) {
			Interpreter interp = new Interpreter (funcs);
			interp.Execute (tree);	
		}

		private void Compile (InstructionList tree) {
			Compiler compiler = Compiler.Create (funcs);
			compiler.Compile (tree);
		}

		private Driver () {
		}
		
		public static int Main (string[] args) {
			bool compile = false;
			string filename;

			if (args.Length < 1) {
				Console.WriteLine ("Usage: mlogo [/compile] filename");
				return 1;
			}

			if (args.Length > 1 && args[0] == "/compile") {
				compile = true;
				filename = args[1];
			} else {
				filename = args[0];
			}

			Driver driver = new Driver ();
			driver.LoadFuncs ();
			InstructionList tree = driver.Parse (filename);

			if (compile)
				driver.Compile (tree);
			else
				driver.Interpret (tree);
					
			return 0;
		}
	}
}

