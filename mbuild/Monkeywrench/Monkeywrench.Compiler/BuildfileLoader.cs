//
// an implementation of a persistent build provider that references
// a source directory with a build file
//

using System;
using System.Collections;
using System.IO;

using Mono.Build;
using Mono.Build.Bundling;

namespace Monkeywrench.Compiler {

    public class BuildfileLoader {
	BuildfileParser parser;
	int errors;

	public BuildfileLoader (string buildfile, ProviderBuilder pb, IWarningLogger log)
	{
	    parser = BuildfileParser.CreateForFile (buildfile, pb, log);
	    // Console.WriteLine ("Parsing `{0}\' ... ", buildfile);
	    errors = parser.Parse ();
	}

	// pooblic

	public ProjectInfo PInfo {
	    get {
		return parser.PInfo;
	    }
	}

	public NameLookupContext InsideNameContext {
	    get {
		return parser.InsideNameContext;
	    }
	}

	public int ParseErrors { get { return errors; } }

	public IEnumerable GetSubBases () {
	    int numinside, numsub, numloads;

	    if (parser.InsideNameContext == null)
		numinside = 0;
	    else
		numinside = parser.WhereInside.Length;
	    
	    if (parser.Subdirs == null)
		numsub = 0;
	    else
		numsub = parser.Subdirs.Length;
	    
	    if (parser.ManualLoads == null)
		numloads = 0;
	    else
		numloads = parser.ManualLoads.Count;
	    
	    // TODO: if subdirs not specified explicitly,
	    // scan srcdir for subdirectories with buildfiles
	    // inside them. Maybe.
	    
	    string[] subbases = new string[numinside + numsub + numloads];
	    
	    for (int i = 0; i < numinside; i++)
		subbases[i] = parser.WhereInside[i];
	    
	    for (int i = 0; i < numsub; i++)
		subbases[i + numinside] = parser.Subdirs[i];
	    
	    if (parser.ManualLoads == null)
		return subbases;
	    
	    int j = 0;
	    
	    foreach (string key in parser.ManualLoads.Keys) {
		subbases[j + numinside + numsub] = key;
		j++;
	    }

	    return subbases;
	}

	protected BuildfileLoader LoadChildFile (string file, WrenchProject proj) {
	    if (!File.Exists (file)) {
		//throw new Exception ("Expected but didn't find buildfile at " + file);
		proj.Log.Error (2005, "Expected but didn't find a buildfile at " + file + ".", null);
		return null;
	    }
	    
	    BuildfileLoader bp = new BuildfileLoader (file, proj.Log);
	    
	    if (bp.ParseErrors > 0)
		return null;
	    return bp;
	}

	public BuildfileLoader LoadChildProvider (WrenchProject proj, string basis, 
						  string subbasis, out bool pub) {
	    // well, this is just a pain in the ass
	    
	    string path;
	    string file;
	    
	    if (parser.ManualLoads != null) {
		if (parser.ManualLoads.Contains (subbasis)) {
		    string me = basis.Substring (0, basis.Length - subbasis.Length);
		    path = proj.PathToBasisSource (me);
		    file = Path.Combine (path, (string) parser.ManualLoads[subbasis]);
		    
		    pub = true;
		    return LoadChildFile (file, proj);
		}
	    }
	    
	    pub = false;
	    
	    if (parser.InsideNameContext != null) {
		for (int i = 0; i < parser.WhereInside.Length; i++) {
		    if (subbasis == parser.WhereInside[i]) {
			WrenchProvider wp = new WrenchProvider ();
			wp.NameContext = (NameLookupContext) parser.InsideNameContext.Clone ();
			return wp;
		    }
		}
	    }
	    
	    path = proj.PathToBasisSource (basis);
	    file = Path.Combine (path, proj.Info.BuildfileName);
	    
	    return LoadChildFile (file, proj);
	}
    }
}
