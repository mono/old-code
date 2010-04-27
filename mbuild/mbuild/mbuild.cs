//
// mbuild.cs -- argument parsing and launching of the mbuild tool
//

using System;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Reflection;

#if EXPERIMENTAL_INSTALL
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
#endif

using Mono.GetOptions;

using Mono.Build;
using Mono.Build.Bundling;
using Monkeywrench;
using Monkeywrench.Compiler;

class ConsoleLogger : SimpleLogger {

    public ConsoleLogger () : base () {}

    void WritePrefixy (string text) 
    {
	string[] lines = text.Split ('\n');
	    
	for (int i = 0; i < lines.Length; i++)
	    Console.Error.WriteLine ("   {0}", lines[i]);
    }
    
    protected override void DoWarning (string location, int category, 
				       string text, string detail) 
    {
	if (location != null && location.Length > 0)
	    Console.Error.Write ("{0}: ", location);
	
	Console.Error.WriteLine ("warning {0}: {1}", category, text);
	
	if (detail != null)
	    WritePrefixy (detail);
    }
    
    protected override void DoError (string location, int category, 
				     string text, string detail) 
    {
	if (location != null && location.Length > 0)
	    Console.Error.Write ("{0}: ", location);
	
	Console.Error.WriteLine ("error {0}: {1}", category, text);
	
	if (detail != null)
	    WritePrefixy (detail);
    }
}

public enum QueryVariables {
	SrcDir,
	BuildDir
}

public class MBuildClient : Options {

	string select_tag = "default";
	string below_prefix = null;
	string buildfile_name = "Buildfile";

	bool dist_mode;
	string distdir = null;

	bool install_mode;
	bool install_is_uninstall = false;

	XmlTextWriter export_writer = null;
	XmlTextReader import_reader = null;
	bool exit_after_import = true;

	bool query_mode;
	QueryVariables query_var;
	string query_basis = null;

	[Option("Debug the build file parser", ' ', "debug-parser")]
	public bool debug_parser = false;

	[Option("Debug the logging data", ' ', "debug-logs")]
	public bool debug_logs = false;

	[Option("Profile the graph access", ' ', "profile-graph")]
	public bool profile_graph = false;

	OperationScope scope = OperationScope.HereAndBelow;
	OperationFunc func;
	int numbuilt, numskipped, numcleaned, numdisted, numinstalled;

	ConsoleLogger log = new ConsoleLogger ();

	public MBuildClient () : base () {
		BreakSingleDashManyLettersIntoManyOptions = true;
		ParsingMode = OptionsParsingMode.Linux;

		numbuilt = 0;
		numskipped = 0;
		numcleaned = 0;
		numdisted = 0;
		numinstalled = 0;

		func = new OperationFunc (WrenchOperations.Build);
	}

	public override WhatToDoNext DoAbout () {
		base.DoAbout ();
		return WhatToDoNext.AbandonProgram;
	}

	WhatToDoNext Check (bool fail)
	{
	    if (fail)
		return WhatToDoNext.AbandonProgram;
	    return WhatToDoNext.GoAhead;
	}

	// Scopes

	string cur_scope = null;

	bool SetScope (string thisscope)
	{
	    if (cur_scope == null) {
		cur_scope = thisscope;
		return false;
	    }

	    Console.Error.WriteLine ("Cannot simultaneously work in both `{0}\' and `{1}\' scopes",
				     cur_scope, thisscope);
	    return true;
	}

	[Option("Operate on targets in the current directory only.", 'l', "local")]
	public WhatToDoNext DoLocal () {
		scope = OperationScope.HereOnly;
		return Check (SetScope ("local"));
	}

	[Option("Operate on targets throughout the entire tree", 'g', "global")]
	public WhatToDoNext DoGlobal () {
		scope = OperationScope.Everywhere;
		return Check (SetScope ("global"));
	}

	[Option("Operate on targets below the prefix {PREFIX}", 'b', "below")]
	public WhatToDoNext DoBelow (string where) {
		scope = OperationScope.HereAndBelow;
		below_prefix = where;
		return Check (SetScope ("below"));
	}

	// Operations that are not part of a special mode

	string cur_op = null;

	bool SetOperation (string thisop)
	{
	    if (cur_op == null) {
		cur_op = thisop;
		return false;
	    }

	    Console.Error.WriteLine ("Cannot simultaneously invoke both `{0}\' and `{1}\' operations",
				     cur_op, thisop);
	    return true;
	}

	[Option("Just print target names", ' ', "print-targets")]
	public WhatToDoNext DoListMode () {
		func = new OperationFunc (PrintOperation);
		return Check (SetOperation ("list"));
	}

	[Option("Clean built objects", 'c', "clean")]
	public WhatToDoNext DoCleanMode () {
		func = new OperationFunc (WrenchOperations.Clean);
		return Check (SetOperation ("clean"));
	}

	[Option("Uncache target information", 'U', "uncache")]
	public WhatToDoNext DoUncacheMode () {
		func = new OperationFunc (WrenchOperations.Uncache);
		return Check (SetOperation ("uncache"));
	}

	[Option("Force a build of the selected targets", 'F', "force")]
	public WhatToDoNext DoForceMode () {
		func = new OperationFunc (WrenchOperations.ForceBuild);
		return Check (SetOperation ("force build"));
	}

	[Option("Show the values of the selected targets", 's', "show")]
	public WhatToDoNext DoShowMode () {
		func = new OperationFunc (ShowOperation);
		return Check (SetOperation ("show"));
	}

	[Option("Describe what installation steps would be performed", ' ', "describe-install")]
	public WhatToDoNext DoDescribeMode () {
		func = new OperationFunc (DescribeInstall);
		return Check (SetOperation ("describe installation"));
	}

	// Special major modes

	string cur_major_mode = null;

	bool SetMajorMode (string thismode)
	{
	    if (cur_major_mode == null) {
		cur_major_mode = thismode;
		return false;
	    }

	    Console.Error.WriteLine ("Cannot simultaneously invoke both `{0}\' and `{1}\' modes",
				     cur_major_mode, thismode);
	    return true;
	}

	string init_srcdir = null;

	[Option("Initialize a new build directory from {SOURCEDIR}", 'i', "init")]
	public WhatToDoNext DoInitMode (string dir) {
		if (dir == null || dir.Length == 0) {
			Console.Error.WriteLine ("Must provide a directory argument to init mode.");
			return WhatToDoNext.AbandonProgram;
		}

		init_srcdir = dir;
		select_tag = "prereq";

		string p = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
		p = Path.Combine (p, ".mbuild-init.config");

		if (File.Exists (p)) {
			try {
				import_reader = new XmlTextReader (p);
				exit_after_import = false;
			} catch (Exception e) {
				log.Warning (3020, "Could not open " + p, e.ToString ());
			}
		}

		return Check (SetMajorMode ("initialize") || SetOperation ("build prerequisites"));
	}

	[Option("Distribute the sources into {DISTDIR} (or [name]-[version] if no DISTDIR)", 'd', "dist")]
	public WhatToDoNext DoDistMode (string dir) {
		func = new OperationFunc (WrenchOperations.Distribute);
		distdir = dir;
		select_tag = null;
		dist_mode = true;
		return Check (SetMajorMode ("distribute") || SetOperation ("distribute"));
	}

	[Option("Run install routines", 'I', "install")]
	public WhatToDoNext DoInstallMode () {
#if EXPERIMENTAL_INSTALL
		func = new OperationFunc (DoRemoteInstall);
#else
		func = new OperationFunc (WrenchOperations.Install);
#endif
		//select_tag = null;
		install_mode = true;
		return Check (SetMajorMode ("install") || SetOperation ("install"));
	}

	[Option("Run uninstall routines", ' ', "uninstall")]
	public WhatToDoNext DoUninstallMode () {
#if EXPERIMENTAL_INSTALL
		func = new OperationFunc (DoRemoteInstall);
#else
		func = new OperationFunc (WrenchOperations.Uninstall);
#endif
		//select_tag = null;
		install_mode = true;
		install_is_uninstall = true;
		return Check (SetMajorMode ("uninstall") || SetOperation ("uninstall"));
	}

	[Option("Set values for the project's configuration options", 'C', "config")]
	public WhatToDoNext DoConfigMode () {
		func = new OperationFunc (ConfigOperation);
		select_tag = "config";
		scope = OperationScope.Everywhere;
		return Check (SetMajorMode ("configure") || SetOperation ("configure"));
	}

	[Option("Export results in XML format into {XMLFILE}", ' ', "export-xml")]
	public WhatToDoNext DoXmlExportMode (string file) {
		func = new OperationFunc (ExportAsXml);
		select_tag = null;

		if (file == null || file.Length == 0)
			export_writer = new XmlTextWriter (Console.Out);
		else
			export_writer = new XmlTextWriter (file, System.Text.Encoding.Default);

		export_writer.Formatting = Formatting.Indented;

		return Check (SetMajorMode ("XML export") || SetOperation ("XML export"));
	}

	[Option("Import results in the XML file {XMLFILE} as configured results", ' ', "import-xml")]
	public WhatToDoNext DoXmlImportMode (string file) {
		if (file == null || file.Length == 0) {
			Console.Error.WriteLine ("No XML input filename provided.");
			return WhatToDoNext.AbandonProgram;
		}

		try {
			import_reader = new XmlTextReader (file);
		} catch (Exception e) {
			Console.Error.WriteLine ("Cannot read XML input file: {0}", e.Message);
			return WhatToDoNext.AbandonProgram;
		}

		return WhatToDoNext.GoAhead;
	}

	[Option("Operate on every target in the database", 'a', "all")]
	public WhatToDoNext DoOperateAll () {
		select_tag = null;
		return WhatToDoNext.GoAhead;
	}

	[Option("Load {FILENAME} as the build file, instead of Buildfile", 'f', "file")]
	public WhatToDoNext DoSetBuildfile (string name) {
		if (name == null || name.Length == 0) {
			Console.Error.WriteLine ("Must provide a parameter to buildfile argument.");
			return WhatToDoNext.AbandonProgram;
		}

		buildfile_name = name;
		return WhatToDoNext.GoAhead;
	}

	[Option("Print out the path to the top source directory of this build", ' ', "get-topsrcdir")]
	public WhatToDoNext DoGetTopSrcDir () {
		query_mode = true;
		query_var = QueryVariables.SrcDir;
		query_basis = "/";
		return Check (SetMajorMode ("variable query") || SetOperation ("variable query"));
	}

	[Option("Print out the path to the top build directory of this build", ' ', "get-topbuilddir")]
	public WhatToDoNext DoGetTopBuildDir () {
		query_mode = true;
		query_var = QueryVariables.BuildDir;
		query_basis = "/";
		return Check (SetMajorMode ("variable query") || SetOperation ("variable query"));
	}

	[Option("Print out the path to the source directory for the basis {BASIS} (defaulting to the current basis)", ' ', "get-srcdir")]
	public WhatToDoNext DoGetSrcDir (string basis) {
		query_mode = true;
		query_var = QueryVariables.SrcDir;
		query_basis = basis;
		return Check (SetMajorMode ("variable query") || SetOperation ("variable query"));
	}

	[Option("Print out the path to the build directory for the basis {BASIS} (defaulting to the current basis)", ' ', "get-builddir")]
	public WhatToDoNext DoGetBuildDir (string basis) {
		query_mode = true;
		query_var = QueryVariables.BuildDir;
		query_basis = basis;
		return Check (SetMajorMode ("variable query") || SetOperation ("variable query"));
	}

	bool last_mode = false;

	[Option("Show the log of recent build actions", ' ', "last")]
	public WhatToDoNext DoLastMode () {
		last_mode = true;
		return Check (SetMajorMode ("show log"));
	}

	public bool CheckRemaining ()
	{
	    if (RemainingArguments.Length < 1)
		return false;

	    if (select_tag == null) {
		Console.Error.WriteLine ("Cannot use the `apply to all' option with an " + 
					 "explicit list of targets");
		return true;
	    }

	    if (cur_scope != null) {
		Console.Error.WriteLine ("Cannot use the scope option `{0}' with an explicit " +
					 "list of targets", cur_scope);
		return true;
	    }

	    return false;
	}

	// basic operations

	bool ShowOperation (WrenchProject proj, BuildServices bs) {
		Result res = bs.GetValue ().Result;

		if (res != null)
			Console.WriteLine ("{0} = {1}", bs.FullName, res);
		else
			Console.WriteLine ("{0} couldn't be evaluated.", bs.FullName);

		return false;
	}

	bool PrintOperation (WrenchProject proj, BuildServices bs) {
		Console.WriteLine ("{0}", bs.FullName);
		return false;
	}

	bool DescribeInstall (WrenchProject proj, BuildServices bs) {
		IResultInstaller iri;
		Result res;

		if (WrenchOperations.GetInstallerAndResult (proj, bs, out iri, out res))
			return true;

		if (iri == null)
			return false;
		
		Console.WriteLine (" + {0}", iri.DescribeAction (res, bs.Context));
		return false;
	}

	bool ExportAsXml (WrenchProject proj, BuildServices bs) {
		Result r = WrenchOperations.BuildValue (proj, bs).Result;
		if (r == null)
			return true;

		r.ExportXml (export_writer, bs.FullName);
		return false;
	}
	       
	// Config

	bool ConfigOperation (WrenchProject proj, BuildServices bs) {
		Result res = bs.GetValue ().Result;

		if (res == null)
			return true;

		// prompt
		Result rprompt;
		if (bs.GetTag ("prompt", out rprompt))
			return true;

		string prompt;

		if (rprompt == null) {
			log.PushLocation (bs.FullName);
			log.Warning (2017, "This configurable option does not have a \'prompt\' tag.", null);
			log.PopLocation ();
			prompt = String.Format ("Set value of {0}:", bs.FullName);
		} else {
			// TODO: i18n
			prompt = ((MBString) rprompt).Value;
		}

		if (res is MBBool)
			DoConfigBool (prompt, (MBBool) res);
		else if (res is MBString)
			DoConfigString (prompt, (MBString) res);
		else if (res is MBDirectory)
			DoConfigDirectory (prompt, (MBDirectory) res);
		else {
			string s = String.Format ("Current value is {0}", res);
			log.PushLocation (bs.FullName);
			log.Error (2018, "Don't know how to configure this option.", s);
			log.PopLocation ();
			return true;
		}

		bs.FixValue (res);
		return false;
	}

	void DoConfigBool (string prompt, MBBool res) {
		// default
		string dflt;

		if (res.Value)
			dflt = "y";
		else
			dflt = "n";

		Console.WriteLine ("{0} (y/n, default: {1})", prompt, dflt);

		string response = Console.ReadLine ();
		if (response == "y")
			res.Value = true;
		else if (response == "n")
			res.Value = false;
		else
			Console.WriteLine ("(Didn't understand; going with the default.)");
	}

	void DoConfigString (string prompt, MBString res) {
		Console.WriteLine ("{0} (default: {1})", prompt, res.Value);

		string response = Console.ReadLine ();

		if (response == "")
			Console.WriteLine ("(Didn't understand; going with the default.)");
		else
			res.Value = response;
	}

	void DoConfigDirectory (string prompt, MBDirectory res) {
		// FIXME: how to handle system/build/config? Right now just ignore it;
		// probably will work ok for most sane situations.

		Console.WriteLine ("{0} (default: {1})", prompt, res.SubPath);

		string response = Console.ReadLine ();

		if (response == "")
			Console.WriteLine ("(Didn't understand; going with the default.)");
		else			
			res.SubPath = response;
	}

	// WrenchOperations events

	bool BeforeBuildEvent (WrenchProject proj, BuildServices bs) {
		Console.WriteLine ("Building `{0}\' ...", bs.FullName);
		numbuilt++;
		return false;
	}

	bool BeforeSkipEvent (WrenchProject proj, BuildServices bs) {
		numskipped++;
		return false;
	}

	bool BeforeCleanEvent (WrenchProject proj, BuildServices bs) {
		Console.WriteLine (" + {0}", bs.FullName);
		numcleaned++;
		return false;
	}

	bool BeforeInstallEvent (WrenchProject proj, BuildServices bs) {
		Console.WriteLine (" + {0}", bs.FullName);
		numinstalled++;
		return false;
	}

	bool BeforeDistributeEvent (WrenchProject proj, BuildServices bs) {
		Console.WriteLine (" + {0}", bs.FullName);
		numdisted++;
		return false;
	}

	// util

	string GuessDistDir (WrenchProject proj) {
		string[] names = new string[] { "/project/name", "/project/version" };
		BuiltItem[] vals = proj.EvaluateTargets (names);

		if (vals == null)
			return null;

		return String.Format ("{0}-{1}",
				      (vals[0].Result as MBString).Value,
				      (vals[1].Result as MBString).Value);
	}

	void DoOperation (string here, ProjectManager pm) {
		if (dist_mode) {
			if (distdir == null || distdir.Length == 0)
				distdir = GuessDistDir (pm.Project);

			if (distdir == null)
				// error will be reported
				return;

			Directory.CreateDirectory (distdir);
			pm.SourceSettings.SetDistPath (distdir);

			Console.WriteLine ("Distributing to `{0}\' ...", distdir);

			if (scope != OperationScope.Everywhere && here != "/")
				Console.WriteLine ("   [Use -g parameter to create a full distribution]");
		}

		if (install_mode) {
			// TODO? maybe make $(DESTDIR) be an option

			if (install_is_uninstall)
				Console.WriteLine ("Uninstalling ...");
			else
				Console.WriteLine ("Installing ...");

			if (scope != OperationScope.Everywhere && here != "/")
				Console.WriteLine ("   [Use -g parameter to create a full installation]");
		}

		if (export_writer != null) {
			export_writer.WriteStartElement ("mbuild-results");
			export_writer.WriteStartElement ("metadata");
			export_writer.WriteEndElement ();
			export_writer.WriteStartElement ("targets");
		}

		// Not working yet.
		//if (pm.Project.LoadReferences ())
		//	return;

		TargetList tl;

		if (below_prefix != null) {
		    if (below_prefix[below_prefix.Length - 1] != '/')
			below_prefix += "/";
		    
		    tl = pm.Project.ListWithOptTag (select_tag).FilterScope (pm.Project.Graph, 
									     below_prefix, scope);
		} else if (RemainingArguments.Length == 0)
		    tl = pm.Project.ListWithOptTag (select_tag).FilterScope (pm.Project.Graph, 
									     here, scope);
		else
		    tl = pm.Project.FromUserList (here, RemainingArguments);

		tl.Operate (func);

		if (export_writer != null) {
			export_writer.WriteEndElement ();
			export_writer.WriteEndElement ();
			export_writer.Close ();
		}
	}

	public int RunQuery (string here, ProjectManager pm) {
		string r;

		if (query_basis == null || query_basis.Length < 1)
			query_basis = here;

		switch (query_var) {
		case QueryVariables.SrcDir:
			r = pm.SourceSettings.PathToBasisSource (query_basis);
			break;
		case QueryVariables.BuildDir:
			r = pm.SourceSettings.PathToBasisBuild (query_basis);
			break;
		default:
			r = null;
			break;
		}

		if (r == null) {
			Console.Error.WriteLine ("Could not retrieve the variable!");
			return 1;
		}

		Console.WriteLine ("{0}", r);
		return 0;
	}

	public int ShowLog (ProjectManager pm) 
	{
	    foreach (ActionLog.LogItem li in pm.Logger.SavedItems)
		Console.WriteLine ("{0}", li);
	    return 0;
	}

	public int ImportXml (string here, ProjectManager pm) {
		int num_imported = 0;

		while (!import_reader.EOF) {
			if (import_reader.NodeType != XmlNodeType.Element ||
			    import_reader.Name != "result") {
				import_reader.Read ();
				continue;
			}

			string id;
			Result r = Result.ImportXml (import_reader, out id, log);

			if (r == null)
				continue;

			if (id == null || id.Length == 0) {
				log.Warning (3019, "Found a result without associated ID; don't know where to import", r.ToString ());
				continue;
			}

			if (id[0] != '/') {
				log.Warning (3019, "This file configures a relative target name; " +
					     "behavior will vary depending on current directory", null);
				id = here + id;
			}

			BuildServices bs = pm.Project.GetTargetServices (id);
			if (bs == null)
				continue;

			Console.WriteLine (" + {0} = {1}", id, r);
			bs.FixValue (r);
			num_imported++;
		}

		Console.WriteLine ("Imported {0} result values", num_imported);

		return 0;
	}

	// Installation

#if EXPERIMENTAL_INSTALL
	Process install_client;
	IInstallerService install_svc;

	bool ChangePrivileges () {
		Mono.Unix.Stat buf;

		if (Mono.Unix.Syscall.stat (WrenchProvider.LogName, out buf) != 0) {
			log.Warning (9999, String.Format ("No file \"{0}\", not going to change privileges.",
							  WrenchProvider.LogName), null);
			return false;
		}

		uint target_uid = buf.st_uid;
		uint target_gid = buf.st_gid;

		Console.WriteLine ("Changing UID/GID from {0}/{1} to {2}/{3} based on owner of {4}", 
				   Mono.Unix.Syscall.getuid (), Mono.Unix.Syscall.getgid (),
				   target_uid, target_gid, WrenchProvider.LogName);

		if (Mono.Unix.Syscall.setgid (target_gid) != 0) {
			log.Error (9999, "Call to setgid failed?", null);
			return true;
		}

		if (Mono.Unix.Syscall.setuid (target_uid) != 0) {
			log.Error (9999, "Call to setuid failed?", null);
			return true;
		}

		return false;
	}

	public int PrepareInstallMode () {
		// remoting prep

		ChannelServices.RegisterChannel (new TcpChannel (9414));

		RemotingConfiguration.RegisterWellKnownServiceType (typeof (InstallerServiceNotify),
								    "MBuild.InstallerServiceNotify", 
								    WellKnownObjectMode.Singleton);

		// spawn the install client (a remoting server)

		string p = Assembly.GetExecutingAssembly ().Location;
		p = Path.GetDirectoryName (p);
		p = Path.Combine (p, "mb-install-client");

		ProcessStartInfo psi = new ProcessStartInfo (p);
		psi.UseShellExecute = false;
		//psi.RedirectStandardInput = true;
		//psi.RedirectStandardOutput = true;

		try {
			install_client = Process.Start (psi);
		} catch (Exception e) {
			log.Error (3021, "Cannot start privileged install client " + p, e.ToString ());
			return 1;
		}

		if (ChangePrivileges ())
			return 1;

		// FIXME: don't do this lame-ass polling.

		while (InstallerServiceNotify.Service == null) {
			//Console.WriteLine ("waiting for service to be set");
			System.Threading.Thread.Sleep (1000);

			if (install_client.HasExited) {
				log.Error (3022, "Install client has exited with an error: " + 
					   install_client.ExitCode, null);
				return 1;
			}
		}

		//Console.WriteLine ("got service yay");
		install_svc = InstallerServiceNotify.Service;

		//Console.WriteLine ("trying to get service");
		//install_svc = (IInstallerService) Activator.GetObject (typeof (IInstallerService),
		//						       "tcp://localhost:9414/MBuild.InstallerService");
		//Console.WriteLine ("got service: {0}", install_svc);
		//install_svc.DoneInstalling ();
		//Console.WriteLine ("told it to exit");

		return 0;
	}

	bool DoRemoteInstall (WrenchProject proj, BuildServices bs) {
		IResultInstaller iri;
		Result res;

		if (WrenchOperations.GetInstallerAndResult (proj, bs, out iri, out res))
			return true;

		if (iri == null)
			return false;
		
		bs.Logger.Log ("operation.install", bs.FullName);
		Console.WriteLine (" + {0}", bs.FullName);

		return install_svc.Install ((Result) iri, res, install_is_uninstall, 
					    new BuildContextProxy (bs.Context));
	}
#endif

	// Actual execution logic

	void NotifyRecompile (string graph, string whyfile)
	{
	    if (whyfile == null)
		Console.WriteLine ("Compiling to graph `{0}' ...", graph);
	    else
		Console.WriteLine ("Recompiling to graph `{0}' because of " + 
				   "changes to `{1}' ...", graph, whyfile);
	}

	public int Launch () {
#if EXPERIMENTAL_INSTALL
		if (install_mode) {
			int ret = PrepareInstallMode ();

			if (ret != 0)
				return ret;
		}
#endif

		BuildfileParser.DebugParser = debug_parser;
		ActionLog.DebugEvents = debug_logs;
		ProjectManager.ProfileStateUsage = profile_graph;
		Monkeywrench.Compiler.XmlGraphSerializer.DebugOutput = true; // FIXME
		string here = "/";

		ProjectManager pm = new ProjectManager ();
		pm.OnRecompile += NotifyRecompile;

		if (init_srcdir == null) {
		    // Should be able to recover the source settings

		    if (pm.LoadSource (log)) {
			// FIXME: recover if there are neighboring directories
			// with valid logs
			Console.Error.WriteLine ("Couldn't recover source directory information. " +
						 "You probably need to initialize this directory as a build directory:");
			Console.Error.WriteLine ("    \"mbuild -i [path to top source directory]\"");
			return 1;
		    }

		    here = SourceSettings.SubpathToBasis (pm.SourceSettings.CurrentSubpath);
		} else {
		    Console.WriteLine ("Initializing with source directory `{0}\' " + 
				       "and buildfiles called `{1}\'", init_srcdir, 
				       buildfile_name);

		    if (pm.CreateToplevel (init_srcdir, buildfile_name, log)) {
			Console.Error.WriteLine ("If you have already initialized this build directory, giving");
			Console.Error.WriteLine ("the `-i' flag is illegal. To recheck the build prerequisites, run:");
			Console.Error.WriteLine ("    \"mbuild --force +prereq\"");
			return 1;
		    }
		}

		if (pm.LoadRest (log))
		    return 1;

		if (query_mode)
			return RunQuery (here, pm);

		if (last_mode)
		    return ShowLog (pm);

		if (import_reader != null) {
			int res = ImportXml (here, pm);

			if (exit_after_import) {
				pm.Dispose ();
				return res;
			}
		}

		WrenchOperations.BeforeBuild += new OperationFunc (BeforeBuildEvent);
		WrenchOperations.BeforeSkip += new OperationFunc (BeforeSkipEvent);
		WrenchOperations.BeforeClean += new OperationFunc (BeforeCleanEvent);
		WrenchOperations.BeforeInstall += new OperationFunc (BeforeInstallEvent);
		WrenchOperations.BeforeUninstall += new OperationFunc (BeforeInstallEvent);
		WrenchOperations.BeforeDistribute += new OperationFunc (BeforeDistributeEvent);

		if (pm.Project != null) {
		    try {
			DoOperation (here, pm);
		    } catch (Exception e) {
			log.Error (3009, "Unhandled exception during operation", e.ToString ());
		    }
		}

#if EXPERIMENTAL_INSTALL
		if (install_mode) {
			install_svc.DoneInstalling ();
			install_client.WaitForExit ();

			if (install_client.ExitCode != 0)
				log.Error (3022, "Install process exited with an error code: " + 
					   install_client.ExitCode, null);
		}
#endif

		if (numcleaned > 0)
			Console.WriteLine ("Cleaned {0} targets", numcleaned);
		// this gets a little annoying
		//if (numbuilt > 0 || numskipped > 0)
		if (numbuilt > 0)
			Console.WriteLine ("Built {0} targets, {1} did not need rebuilding (with dups).", numbuilt, numskipped);
		if (numdisted > 0)
			Console.WriteLine ("Copied {0} targets for distribution", numdisted);
		if (numinstalled > 0) {
			if (install_is_uninstall)
				Console.WriteLine ("Uninstalled {0} targets", numinstalled);
			else
				Console.WriteLine ("Installed {0} targets", numinstalled);
		}
		if (log.NumWarnings > 0 || log.NumErrors > 0)
			Console.Error.WriteLine ("{0} warnings, {1} errors", 
						 log.NumWarnings, log.NumErrors);

		if (init_srcdir != null) {
		    //string self = Environment.CommandLine.Split (' ')[0];
		    // FIXME: the above returns the exe name, not the argv[0] of the
		    // mono process (which the wrapper script sets to $0 of the wrapper,
		    // which is what we want)

		    if (log.NumErrors > 0) {
			Console.WriteLine ("After you fix the problems reported above, you can");
			Console.WriteLine ("continue checking the build prerequisites by running:");
			Console.WriteLine ("   \"mbuild +prereq\"");
			Console.WriteLine ("To recheck ALL of the build prerequisites, run: ");
			Console.WriteLine ("   \"mbuild --force +prereq\"");
		    } else {
			Console.WriteLine ("To recheck the build prerequisites, run: ");
			Console.WriteLine ("   \"mbuild --force +prereq\"");
		    }
		}

		pm.Dispose ();
		return this.log.NumErrors;
	}

	public static int Main (string[] args) {
		MBuildClient client = new MBuildClient ();

		client.ProcessArgs (args);
		
		if (client.CheckRemaining ())
		    return 1;

		return client.Launch ();
	}

}
