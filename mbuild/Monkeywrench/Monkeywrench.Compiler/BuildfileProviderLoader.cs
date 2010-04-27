using System;
using System.IO;
using System.Collections;

using Mono.Build;

namespace Monkeywrench.Compiler {

    internal class BuildfileProviderLoader : ProviderLoaderBase {

	SourceSettings ss;
	string srcrel;

	public BuildfileProviderLoader (string basis, string decl_loc,
					SourceSettings ss, string srcrel) : base (basis, decl_loc)
	{
	    this.ss = ss;

	    if (srcrel != null)
		this.srcrel = srcrel;
	    else
		this.srcrel = Path.Combine (SourceSettings.BasisToSubpath (basis), 
					    ss.BuildfileName);
	}

	public BuildfileProviderLoader (string basis, string decl_loc, 
					SourceSettings ss) : this (basis, decl_loc, ss, null)
	{}

	public BuildfileProviderLoader (string basis, SourceSettings ss) : this (basis, null, ss, null)
	{}

	public override bool Initialize (WrenchProvider wp, IWarningLogger log, Queue children)
	{
	    bool is_top = (basis == "/");

	    string topsrc = ss.PathToSourceRelative ("");
	    string file = ss.PathToSourceRelative (srcrel);

	    BuildfileParser parser = BuildfileParser.CreateForFile (topsrc, srcrel, wp, log);

	    if (parser.Parse () > 0)
		// Parse errors
		return true;

	    // FIXME: tell the parser whether a project[] section is OK and have
	    // it signal the error. That way we get line info.

	    if (!is_top && parser.PInfo != null) {
		log.Error (2006, "Found a project[] directive in a non-toplevel buildfile", file);
		return true;
	    } else if (is_top && parser.PInfo == null) {
		log.Error (2006, "Toplevel buildfile did not have a project[] directive", file);
		return true;
	    }

	    if (is_top) {
		// kinda ugly.
		parser.PInfo.BuildfileName = Path.GetFileName (srcrel);

		if (((GraphBuilder) wp.Owner).SetProjectInfo (parser.PInfo, log))
		    return true;

		children.Enqueue (new ProjectProviderLoader (parser.PInfo));
	    }

	    foreach (string sub in parser.Subdirs)
		children.Enqueue (new BuildfileProviderLoader (basis + sub, ss));

	    foreach (BuildfileParser.InsideInfo ii in parser.Insides) {
		foreach (string s in ii.Bases)
		    children.Enqueue (new InsideProviderLoader (basis + s, ii.Context));
	    }

	    if (parser.ManualLoads != null) {
		foreach (string k in parser.ManualLoads.Keys) {
		    string srel = (string) parser.ManualLoads[k];
		    children.Enqueue (new BuildfileProviderLoader (basis + k, DeclarationLoc, ss, srel));
		}
	    }
	    
	    return false;
	}
    }

}
