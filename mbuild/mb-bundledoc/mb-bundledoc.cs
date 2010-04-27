//
// mb-bundledoc.cs -- Bundle XML documentation tool
//

using System;
using System.IO;
using System.Reflection;

using Mono.GetOptions;

namespace MBBundleDoc {

	public class MainClass : Options {

		public MainClass () : base () {
			// Be more mcs-like in option handling
			//BreakSingleDashManyLettersIntoManyOptions = true;
			//ParsingMode = OptionsParsingMode.Linux;
		}

		// Options

		public override WhatToDoNext DoAbout () {
			base.DoAbout ();
			return WhatToDoNext.AbandonProgram;
		}

		// Do it!

		Assembly assy;
		string docpath;

		public int Launch () {
			if (RemainingArguments.Length != 2) {
				DoAbout ();
				return 1;
			}

			try {
				assy = Assembly.LoadFrom (RemainingArguments[0]);
			} catch (Exception e) {
				Console.Error.WriteLine ("Could not load assembly {0}: {1}",
							 RemainingArguments[0], e);
				return 1;
			}

			docpath = RemainingArguments[1];
			if (!Directory.Exists (docpath)) {
				Console.Error.WriteLine ("Documentation directory {0} does not exist!",
							 docpath);
				return 1;
			}

			BundleDocumenter bd = new BundleDocumenter (assy, docpath);

			try {
				bd.Document ();
			} catch (Exception e) {
				Console.Error.WriteLine ("Error in docs generation: {0}", e);
				return 1;
			}

			return 0;
		}

		public static int Main (string[] args) {
			MainClass options = new MainClass ();
			
			options.ProcessArgs (args);
			return options.Launch ();
		}
	}
	
}
