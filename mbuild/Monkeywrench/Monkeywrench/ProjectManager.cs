//
// ProjectManager.cs -- class that loads buildfiles and creates providers
// for a Project.
//

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using Mono.Build;
using Mono.Build.Bundling;

using Monkeywrench.Compiler;

namespace Monkeywrench {

    public delegate void RecompileHandler (string graphfile, string whyfile);

    public class ProjectManager : IDisposable {

	BundleManager bm; // FIXME: need to sync this up with GraphBuilder.bm somehow
	SourceSettings ss;
	ActionLog log;
	IGraphState graph;
	WrenchProject proj;

	public ProjectManager () 
	{
	    this.bm = new BundleManager ();
	}
	
	public SourceSettings SourceSettings { get { return ss; } }

	public WrenchProject Project { get { return proj; } }

	public BundleManager Bundles { get { return bm; } }
	
	public ActionLog Logger { get { return log; } }

	// Loading

	public bool LoadSource (IWarningLogger uilog) 
	{
	    if ((ss = SourceSettings.Load (uilog)) == null)
		return true;

	    return false;
	}

	public bool CreateToplevel (string topsrc, string bfname, IWarningLogger uilog) 
	{
	    if ((ss = SourceSettings.CreateToplevel (topsrc, bfname, uilog)) == null)
		return true;

	    return false;
	}

	public static bool ProfileStateUsage = false;

	public bool LoadRest (IWarningLogger uilog) 
	{
	    if ((log = ActionLog.Load (ss, uilog)) == null)
		return true;

	    if ((graph = GetGraph ()) == null)
		return true;

	    if (ProfileStateUsage)
		graph = new GraphStateProfiler (graph);

	    proj = new WrenchProject (ss, graph, log);
	    return false;
	}

	// Graph saving / loading

	public const string Graph = "graph.bin";

	public event RecompileHandler OnRecompile;

	void RecompileNoGraph (string gfile)
	{
	    if (OnRecompile != null)
		OnRecompile (gfile, null);
	}

	void RecompileDepChanged (string gfile, string dep)
	{
	    if (OnRecompile != null)
		OnRecompile (gfile, dep);
	}

	IGraphState GetGraph ()
	{
	    string gfile = ss.PathToStateItem (Graph);

	    try {
		if (!File.Exists (gfile))
		    RecompileNoGraph (gfile);
		else {
		    IGraphState gs = LoadSavedGraph (gfile, true);

		    if (gs != null)
			return gs;
		}
	    } catch (Exception e) {
		log.Error (9999, "Exception trying to recover compiled graph", e.Message);
	    }

	    GraphBuilder gb = GraphCompiler.Compile (ss, log);
	    if (gb == null) {
		File.Delete (gfile);
		return null;
	    }

	    BinaryGraphSerializer.Write (gb, gfile);

	    return LoadSavedGraph (gfile, false);
	}

	IGraphState LoadSavedGraph (string gfile, bool check_rebuild)
	{
	    BinaryLoadedGraph blg = BinaryLoadedGraph.Load (gfile, log);

	    if (blg.GetProjectInfo ().LoadBundles (bm, log))
		return null;

	    // Do this after loading bundles to check dependent assemblies.

	    if (check_rebuild) {
		string whyfile = GraphNeedsRebuild (blg);

		if (whyfile != null) {
		    RecompileDepChanged (gfile, whyfile);
		    return null;
		}
	    }

	    return blg;
	}

	// returns the reason why we need to recompile or null if no need
	string GraphNeedsRebuild (IGraphState gs)
	{
	    foreach (DependentItemInfo dii in gs.GetDependentFiles ()) {
		string f = ss.PathToSourceRelative (dii.Name);

		Fingerprint fp = Fingerprint.FromFile (f);

		if (fp != dii.Fingerprint)
		    return f;
	    }

	    // This is a bit gross, but it's called once.
	    // We've just loaded the bundles straight from the graph's
	    // ProjectInfo, but the pinfo may not be in sync with the
	    // DII list.

	    Dictionary<string,Assembly> namemap = new Dictionary<string,Assembly> ();
	    int num_pinfo = 0, num_dii = 0;

	    foreach (Assembly assy in bm.BundleAssemblies) {
		namemap[assy.GetName ().Name] = assy;
		num_pinfo++;
	    }

	    foreach (DependentItemInfo dii in gs.GetDependentBundles ()) {
		if (!namemap.ContainsKey (dii.Name))
		    return dii.Name;

		Fingerprint fp = Fingerprint.FromFile (namemap[dii.Name].Location);

		if (fp != dii.Fingerprint)
		    return dii.Name;

		num_dii++;
	    }

	    if (num_pinfo != num_dii)
		return "[mismatch in number of referenced bundles]";

	    return null;
	}

	// IDisposeable
	
	bool disposed = false;
	
	public void Dispose () {
	    if (disposed)
		return;
	    
	    if (proj != null) {
		proj.Dispose ();
		proj = null;
	    }

	    if (log != null) {
		log.Save (ss);
		log = null;
	    }

	    if (ProfileStateUsage) {
		Console.Error.WriteLine ("GRAPH STATE USAGE PROFILE:");
		Console.Error.WriteLine ("{0}", graph);
	    }

	    graph = null;
	    ss = null;
	    bm = null;

	    disposed = true;
	}
    }
}
