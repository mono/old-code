//
// The main build logic implementation
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Mono.Build;
using Mono.Build.Bundling;

using Monkeywrench.Compiler;

namespace Monkeywrench {
    
    public class WrenchProject : IDisposable {
	SourceSettings ss;
	ActionLog log;
	IGraphState graph;
	IBuildManager manager;
	
	public WrenchProject (SourceSettings ss, IGraphState graph, 
			      ActionLog log) 
	{
	    this.ss = ss;
	    this.log = log;
	    this.graph = graph;
	    this.manager = WrenchOperations.MakeManager (this);
	}

	public SourceSettings SourceSettings { get { return ss ; } }

	public ActionLog Log { get { return log; } }

	public IGraphState Graph { get { return graph; } }

	public IBuildManager BuildManager {
	    get { return manager; }
	    
	    set {
		if (value == null)
		    throw new ArgumentNullException ();
		manager = value;
	    }
	}

	///////////////////////////////////////////
	// InfoContext: IBuildContext implementations

	internal class InfoContext : IBuildContext {
	    WrenchProject proj;
	    MBDirectory wd, sd;
	    
	    public InfoContext (string decl_loc, WrenchProject proj) {
		this.proj = proj;
		
		// even works for "/"
		wd = new MBDirectory (ResultStorageKind.Built, decl_loc);
		sd = new MBDirectory (ResultStorageKind.Source, decl_loc);
	    }
	    
	    public MBDirectory WorkingDirectory { 
		get { return wd; }
	    }
	    
	    public MBDirectory SourceDirectory { 
		get { return sd; }
	    }
	    
	    public string PathTo (MBDirectory dir) {
		return proj.ss.PathTo (dir);
	    }
	    
	    public string DistPath (MBDirectory dir) {
		return proj.ss.DistPath (dir);
	    }
	    
	    public IBuildLogger Logger {
		get { return proj.log; }
	    }
	}

	///////////////////////////////////////////
	// Per-provider state management

	internal class ProviderInfo {
	    public short id;
	    public IProviderPersistence pp;
	    public InfoContext context;

	    // TODO: Use varying persistence storages; right now
	    // we only ever use FileStateTable.

	    public ProviderInfo (short id, WrenchProject proj) 
	    {
		this.id = id;

		pp = FileStateTable.Load (GetStatePath (proj), proj.Log);

		context = new InfoContext (proj.Graph.GetProviderDeclarationLoc (id), proj);
	    }
	    
	    public bool Close (WrenchProject proj)
	    {
		return pp.Save (GetStatePath (proj), proj.Log);
	    }

	    public const string StateSuffix = "state.dat";

	    string GetStatePath (WrenchProject proj)
	    {
		string basis = proj.Graph.GetProviderBasis (id);
		return proj.ss.PathToStateItem (SourceSettings.BasisToFileToken (basis) + 
						StateSuffix);
	    }

	    public bool InitializeBuilddir (WrenchProject proj)
	    {
		string declloc = proj.Graph.GetProviderDeclarationLoc (id);
		string decldir = proj.ss.PathToBuildRelative (declloc);

		try {
		    Directory.CreateDirectory (decldir);
		} catch (IOException) {
		    // Dir already exists, presumed OK.
		    return false; 
		}

		// Do this here (not in the try) just in case
		// something here could raise an IOException
		// that might be masked.

		return proj.ss.SaveForSubpath (declloc, proj.log);
	    }

	    public void UncacheValue (string basename) {
		pp.SetItem (basename, BuiltItem.Null);
	    }
	    
	    public void FixValue (string basename, Result r, Fingerprint fp) {
		if (fp == null)
		    fp = r.GetFingerprint (context, null);
		
		BuiltItem bi = new BuiltItem (r, fp, GenericFingerprints.Null);
		pp.SetItem (basename, bi);
	    }
	}
	
	// ID to ProviderInfo
	Dictionary<short,ProviderInfo> providers = 
	    new Dictionary<short,ProviderInfo> ();

	ProviderInfo GetProviderInfo (short id)
	{
	    if (providers.ContainsKey (id))
		return providers[id];

	    ProviderInfo pi = new ProviderInfo (id, this);

	    if (pi.InitializeBuilddir (this))
		return null;

	    providers[id] = pi;
	    return pi;
	}

	internal ProviderInfo GetTargetInfo (int tid) 
	{
	    return GetProviderInfo ((short) (tid >> 16));
	}

	///////////////////////////////////////////
	// building

	public enum TargetState {
	    Unknown = 0,
	    Building,
	    BuiltOk,
	    BuiltError
	}

	public BuiltItem[] EvaluateTargets (string[] names)
	{
	    int[] ids = new int[names.Length];

	    for (int i = 0; i < names.Length; i++)
		ids[i] = graph.GetTargetId (names[i]);

	    return manager.EvaluateTargets (ids);
	}

	// FIXME: for the love of god, purge items from this
	// hashtable at some point.

	Dictionary<int,TargetState> builds =
	    new Dictionary<int,TargetState> ();

	public TargetState GetTargetState (int tid) 
	{
	    if (builds == null || !builds.ContainsKey (tid))
		return TargetState.Unknown;

	    return builds[tid];
	}

	internal void SetTargetState (int tid, TargetState state) 
	{
	    builds[tid] = state;
	}

	public BuildServices GetTargetServices (int tid) 
	{
	    return new BuildServices (this, tid);
	}

	public BuildServices GetTargetServices (string name)
	{
	    return GetTargetServices (graph.GetTargetId (name));
	}

	// Listing targets

	IEnumerable<int> EnumAllIds ()
	{
	    short pmax = graph.NumProviders;

	    for (short i = 0; i < pmax; i++) {
		int tmax = graph.GetProviderTargetBound (i);

		for (int tid = (int) ((((uint) i) << 16) & 0xFFFF0000); tid < tmax; tid++)
		    yield return tid;
	    }
	}
	
	IEnumerable<BuildServices> FromIds (IEnumerable<int> tids)
	{
	    foreach (int tid in tids)
		yield return GetTargetServices (tid);
	}
	
	internal TargetList ListIds (IEnumerable<int> tids)
	{
	    return new TargetList (FromIds (tids));
	}

	public TargetList ListAll ()
	{
	    return new TargetList (FromIds (EnumAllIds ()));
	}

	IEnumerable<BuildServices> FromNames (string here, IEnumerable<string> names)
	{
	    foreach (string name in names) {
		if (name[0] != '/')
		    yield return GetTargetServices (StrUtils.CanonicalizeTarget (name, here));
		else
		    yield return GetTargetServices (name);
	    }
	}

	public TargetList ListNames (string here, IEnumerable<string> names)
	{
	    return new TargetList (FromNames (here, names));
	}

	IEnumerable<BuildServices> FromTagInfos (IEnumerable<TargetTagInfo> ttis)
	{
	    foreach (TargetTagInfo tti in ttis)
		yield return new BuildServices (this, tti);
	}
	
	public TargetList ListWithOptTag (string tagstr)
	{
	    int tag = -1;

	    if (tagstr == null)
		return ListAll ();

	    tag = graph.GetTagId (tagstr);

	    if (tag < 0) 
		// Tag not defined, so nothing has it
		return TargetList.Null;

	    // FromTagInfos gives us those marked with the tag;
	    // FilterTag checks that the tag value is true.

	    return new TargetList (FromTagInfos (graph.GetTargetsWithTag (tag))).FilterTag (tag);
	}

	IEnumerable<BuildServices> EnumUserList (string here, IEnumerable<string> list)
	{
	    foreach (string s in list) {
		if (s[0] != '+') {
		    if (s[0] != '/')
			yield return GetTargetServices (StrUtils.CanonicalizeTarget (s, here));
		    else
			yield return GetTargetServices (s);
		} else {
		    int tagid = graph.GetTagId (s.Substring (1));

		    if (tagid < 0)
			continue;

		    foreach (BuildServices bs in FromTagInfos (graph.GetTargetsWithTag (tagid)))
			yield return bs;
		}
	    }
	}

	public TargetList FromUserList (string here, IEnumerable<string> list)
	{
	    return new TargetList (EnumUserList (here, list));
	}
	
	///////////////////////////////////////////
	// IDisposable

	bool disposed;

	public void Close () {
	    Dispose ();
	}
	
	public void Dispose () { 
	    if (disposed)
		return;

	    foreach (ProviderInfo pi in providers.Values)
		pi.Close (this);

	    providers = null;
	    disposed = true;

	    GC.SuppressFinalize (this);
	}
    }
}
