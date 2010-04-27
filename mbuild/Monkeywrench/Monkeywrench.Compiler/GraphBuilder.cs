using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using Mono.Build;
using Mono.Build.Bundling;

namespace Monkeywrench.Compiler {

    public class GraphBuilder : ProjectBuilder {
	// This is very similar to an IGraphState, but
	// there are several annoying GraphState members that
	// I would rather not implement. Having this be an IGraphState
	// was useful during the compiler transition, but
	// I don't think it will be in the future.

	public GraphBuilder () 
	{
	    bm.SetProject (this);
	}

	// Tags. Straightforward.

	int cur_tag_id = 0;
	Dictionary<string,int> tag_ids_by_string = new Dictionary<string,int> ();
	Dictionary<int,string> tag_strings_by_id = new Dictionary<int,string> ();

	public int GetTagId (string tag)
	{
	    if (tag_ids_by_string.ContainsKey (tag))
		return tag_ids_by_string[tag];

	    tag_ids_by_string[tag] = cur_tag_id;
	    tag_strings_by_id[cur_tag_id] = tag;

	    return cur_tag_id++;
	}

	public IEnumerable<string> GetTags ()
	{
	    return tag_strings_by_id.Values;
	}

	public int NumTags { get { return cur_tag_id; } }

	// Dependencies of the graph.

	Dictionary<string,DependentItemInfo> dep_files = 
	    new Dictionary<string,DependentItemInfo> ();

	public void AddDependentFile (string fname, Fingerprint fp)
	{
	    if (dep_files.ContainsKey (fname))
		return;

	    DependentItemInfo dii = new DependentItemInfo ();
	    dii.Name = fname;
	    dii.Fingerprint = fp;

	    dep_files[fname] = dii;
	}

	public void AddDependentFile (string fname, string pathto)
	{
	    AddDependentFile (fname, Fingerprint.FromFile (pathto));
	}

	public IEnumerable<DependentItemInfo> GetDependentFiles ()
	{
	    return dep_files.Values;
	}

	List<DependentItemInfo> dep_bundles = new List<DependentItemInfo> ();

	void AddDependentBundles ()
	{
	    foreach (Assembly assy in bm.BundleAssemblies) {
		DependentItemInfo dii = new DependentItemInfo ();
		dii.Name = assy.GetName ().Name;
		dii.Fingerprint = Fingerprint.FromFile (assy.Location);;

		dep_bundles.Add (dii);
	    }
	}

	public IEnumerable<DependentItemInfo> GetDependentBundles ()
	{
	    return dep_bundles;
	}

	// Project Info

	ProjectInfo pinfo = null; // must be initialized elsewhere
	BundleManager bm = new BundleManager ();

	public ProjectInfo PInfo { get { return pinfo; } }

	public bool SetProjectInfo (ProjectInfo pinfo, IWarningLogger log)
	{
	    if (pinfo == null)
		throw new ArgumentException ();
	    if (this.pinfo != null)
		throw new InvalidOperationException ("Can only set project info once");

	    this.pinfo = pinfo;

	    if (pinfo.LoadBundles (bm, log))
		return true;

	    AddDependentBundles ();

	    return false;
	}

	public BundleManager Bundles { get { return bm; } }

	// Providers -- mostly implemented by ProjectBuilder

	short cur_provider_id = 0;

	protected override ProviderBuilder CreateProvider (string basis)
	{
	    return new WrenchProvider (this, basis, cur_provider_id++);
	}
	    
	public short NumProviders { 
	    get { return cur_provider_id; }
	}

	// Targets

	Dictionary<int,TargetBuilder> targs_by_id 
	    = new Dictionary<int,TargetBuilder> ();

	internal void AddTarget (TargetBuilder tb)
	{
	    int id = ((WrenchTarget) tb).Id;

	    if (targs_by_id.ContainsKey (id))
		throw ExHelp.App ("Somehow got a dup target id? ({0:x})", id);

	    targs_by_id[id] = tb;
	}

	public TargetBuilder GetTargetBuilder (int tid)
	{
	    if (!targs_by_id.ContainsKey (tid))
		// FIXME: ExHelp.
		throw new InvalidOperationException ("Unknown target ID " + tid.ToString ());

	    return targs_by_id[tid];
	}

	public IEnumerable<TargetBuilder> GetTargets ()
	{
	    // Returns list of TargetBuilders
	    return targs_by_id.Values;
	}

	///////////////////////////////////////////
	// Target Requests

#if LATER
	public int RequestTarget (string name) 
	{
	    int i = name.LastIndexOf ('/');
	    string basename = name.Substring (i + 1);
	    string basis = name.Substring (0, i + 1); // include trailing slash
	    
	    ProviderBuilder pb = GetProvider (basis);
	    TargetBuilder tb = pb.GetTarget (basename);

	    if (tb == null)
		tb = pb.RequestTarget (basename);
	    else
		return ((WrenchTarget) tb).Id;
	}
#endif

	public int LookupTarget (string name) 
	{
	    int i = name.LastIndexOf ('/');
	    string basename = name.Substring (i + 1);
	    string basis = name.Substring (0, i + 1); // include trailing slash
	    
	    return ((WrenchProvider) GetProvider (basis)).LookupTarget (basename);
	}
	
	///////////////////////////////////////////
	// Done

	public bool Finish (IWarningLogger log)
	{
	    foreach (WrenchProvider wp in Providers) {
		if (!wp.Claimed) {
		    log.Error (2001, "Something referenced a target in the provider " +
			       wp.Basis + " but it never was registered.", null);
		    return true;
		}

		if (wp.DoneModifying)
		    continue;

		if (wp.Finish (bm, log))
		    return true;
	    }

	    return false;
	}
    }
}
