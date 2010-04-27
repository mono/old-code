// mb-import.cs -- Tool to install bundles associated
// with a distribution into the GAC.

using System;
using System.Collections;
using System.Reflection;
using System.IO;

using Mono.GetOptions;

using Mono.Build;
using Mono.Build.Bundling;
using Mono.Build.RuleLib;
using Monkeywrench;
using MBuildDynamic.Core.Clr;

class ConsoleLogger : SimpleLogger, IBuildLogger {

	public ConsoleLogger () : base () {}

	// FIXME: graphical!

	void WritePrefixy (string text) {
		string[] lines = text.Split ('\n');

		for (int i = 0; i < lines.Length; i++)
			Console.Error.WriteLine ("   {0}", lines[i]);
	}

	protected override void DoWarning (string location, int category, string text, string detail) {
		if (location != null)
			Console.Error.Write ("{0}: ", location);

		Console.Error.WriteLine ("warning {0}: {1}", category, text);

		if (detail != null)
			WritePrefixy (detail);

		if (OnWarning != null)
			OnWarning (location, category, text, detail);
	}

	protected override void DoError (string location, int category, string text, string detail) {
		if (category == 2020)
			// Suppress 'cannot load assembly' errors, which we may get
			// when probing the GAC for bundles.
			return;

		if (location != null)
			Console.Error.Write ("{0}: ", location);

		Console.Error.WriteLine ("error {0}: {1}", category, text);

		if (detail != null)
			WritePrefixy (detail);

		if (OnError != null)
			OnError (location, category, text, detail);
	}

	public void Log (string category, string text, object extra) {
	}

	public void Log (string category, string text) {
	}

	public event LogEvent OnWarning;

	public event LogEvent OnError;
}

public class MBImport : Options {

	public MBImport () : base () {
		//BreakSingleDashManyLettersIntoManyOptions = true;
		//ParsingMode = OptionsParsingMode.Linux;
	}

	public override WhatToDoNext DoAbout () {
		base.DoAbout ();
		return WhatToDoNext.AbandonProgram;
	}

	[Option("Only show what would be done; don't actually import bundles.", 'n', "dry-run")]
	public bool dry_run = false;

	[Option("The program to run as gacutil", ' ', "gacutil")]
	public string gacutil_binary = "gacutil";

	[Option("The GAC root", 'r', "root")]
	public string root = "/usr/lib";

	// App

	ConsoleLogger log = new ConsoleLogger ();
	BundleManager bm = new BundleManager ();
	MiniBuildContext ctxt;
	GacutilProgram gacprog;
	int num_imported = 0;

	bool ImportDll (MBDirectory dir, FileInfo dll) {
		string[] pieces = dll.Name.Split ('_');

		if (pieces.Length != 2) {
			log.Error (9999, "Invalid bundle filename (expected exactly one underscore): " + dll.Name, null);
			return true;
		}

		// pieces[1] is <version>.dll, so strip off the .dll part
		string shortname = pieces[0].Substring (BundleManager.MBuildPrefix.Length);
		string version = pieces[1].Substring (0, pieces[1].Length - 4);

		AssemblyName aname = BundleManager.MakeName (shortname, version);

		if (bm.LoadBundle (aname, log) == false) {
			Console.WriteLine (" + Bundle {0}/{1} already imported", shortname, version);
			return false;
		}

		if (dry_run) {
			Console.WriteLine (" + Would import {0}/{1}", shortname, version);
			return false;
		}

		Console.WriteLine (" + Importing {0}/{1}", shortname, version);

		AssemblyFile assy = new AssemblyFile ();
		assy.Dir = dir;
		assy.Name = dll.Name;

		// FIXME
		//if (gacprog.InstallAssembly (assy, root, MBImport.Config.CompatCode, false, ctxt))
		if (gacprog.InstallAssembly (assy, root, "mbuild-0.0", false, ctxt))
			return true;

		num_imported++;
		return false;
	}

	bool RealImport (string dir) {
		DirectoryInfo di = new DirectoryInfo (dir);

		if (!di.Exists) {
			log.Error (9999, "The directory \"" + dir + "\" does not exist.", null);
			return true;
		}

		FileInfo[] dlls = di.GetFiles (BundleManager.MBuildPrefix + "*_*.dll");

		if (dlls.Length == 0) {
			log.Error (9999, "There are no bundle files in \"" + dir + "\".", null);
			return true;
		}

		MBDirectory as_mbd = new MBDirectory (ResultStorageKind.System, di.FullName);
		bool fail = false;

		for (int i = 0; i < dlls.Length; i++)
			fail |= ImportDll (as_mbd, dlls[i]);

		return fail;
	}

	bool ImportOneDirectory (string dir) {
		string subdir = Path.Combine (dir, WrenchProject.BundleDistributionDir);
		if (Directory.Exists (subdir))
			return RealImport (subdir);

		return RealImport (dir);
	}

	// A mini build to configure the GacutilProgram

	class MiniBuildContext : IBuildContext, IBuildManager {

		MBImport import;

		public MiniBuildContext (MBImport import) {
			this.import = import;
		}

		// IBuildContext

		public MBDirectory WorkingDirectory {
			get {
				throw new InvalidOperationException ("Shouldn't need working dir to configure gacutil.");
			}
		}

		public MBDirectory SourceDirectory {
			get {
				throw new InvalidOperationException ("Shouldn't need source dir to configure gacutil.");
			}
		}

		public string PathTo (MBDirectory d) {
			switch (d.Storage) {
			case ResultStorageKind.System:
				return d.SubPath;
			default:
				throw new InvalidOperationException ("Shouldn't need internal path to configure gacutil.");
			}
		}

		public string DistPath (MBDirectory d) {
			throw new InvalidOperationException ("Shouldn't need distpath to configure gacutil.");
		}

		public IBuildLogger Logger { get { return import.log; } }

		// IBuildManager

		public BuiltItem[] EvaluateTargets (string[] targets) {
			if (targets.Length == 0)
				return new BuiltItem[0];

			Console.Error.WriteLine ("Trying to evaluate: ");
			foreach (string s in targets)
				Console.Error.WriteLine ("        {0}", s);

			throw new InvalidOperationException ("Shouldn't need other targets to configure gacutil.");
		}
	}

	bool InitGacutil () {
		ctxt = new MiniBuildContext (this);
		ExternalBinaryInfo ebi = new ExternalBinaryInfo (gacutil_binary);
		ConfigureGacutilProgram configure = new ConfigureGacutilProgram ();

		ArgCollector ac = configure.GetCollector ();
		ac.Add (ebi, log);
		if (ac.FinalizeArgs (ctxt, log))
			return true;

		Result r = configure.Build (ac, ctxt);
		if (r == null)
			return true;

		gacprog = (GacutilProgram) r;
		return false;
	}

	int DoIt () {
		if (InitGacutil ())
			return 1;

		bool fail = false;

		if (RemainingArguments.Length == 0)
			fail |= ImportOneDirectory (".");
		else {
			for (int i = 0; i < RemainingArguments.Length; i++)
				fail |= ImportOneDirectory (RemainingArguments[i]);
		}

		Console.WriteLine ("Imported {0} bundles total.", num_imported);

		if (fail)
			return 1;
		return 0;
	}

	// Go, go, go

	public static int Main (string[] args) {
		MBImport program = new MBImport ();

		program.ProcessArgs (args);
		return program.DoIt ();
	}
}
