// BuildServices.cs -- class allowing persistence of a build operation

using System;
using System.Collections.Generic;

using Mono.Build;

using Monkeywrench.Compiler;

namespace Monkeywrench {

    public class BuildServices {
	// FIXME: some kind of lifecycle management to tell the
	// project when we're done with the ProviderInfo, so
	// that the corresponding provider can be unloaded.
	WrenchProject.ProviderInfo pi;
	WrenchProject proj;

	int tid;
	string target; // fixme: just use accessors and eliminate members?
	string basename;

	Rule rule;
	ArgCollector ac;

	Fingerprint cur_build_fp;
			
	private BuildServices () {}

	internal BuildServices (WrenchProject proj, int tid) 
	{
	    this.proj = proj;
	    this.tid = tid;

	    pi = proj.GetTargetInfo (tid);

	    basename = proj.Graph.GetTargetName (tid);

	    string basis = proj.Graph.GetProviderBasis ((short) (tid >> 16));
	    target = basis + basename;
	}

	internal BuildServices (WrenchProject proj, TargetTagInfo tti)
	    : this (proj, tti.Target)
	{
	    cached_tag = tti.Tag;
	    cached_tag_rval = tti.ValResult;
	    cached_tag_tval = tti.ValTarget;
	}

	public int Id { get { return tid; } }

	public string FullName { get { return target; } }

	public string BaseName { get { return basename; } }

	public WrenchProject Project { get { return proj; } }

	public IBuildContext Context { get { return pi.context; } }

	public IBuildLogger Logger { get { return proj.Log; } }
			
	public Rule Rule {
	    get {
		if (rule != null)
		    return rule;

		return GetRule ();
	    }
	}

	public ArgCollector Args {
	    get {
		if (ac != null)
		    return ac;
		
		return GetArgs ();
	    }
	}

	// Tags

	int cached_tag = -1;
	Result cached_tag_rval = null;
	int cached_tag_tval = -1;

	bool GetOneTarget (int tid, out Result val)
	{
	    // point of recursion!

	    BuiltItem[] bis = proj.BuildManager.EvaluateTargets (new int[] { tid });

	    if (bis == null) {
		val = null;
		return true;
	    }

	    val = bis[0].Result;
	    return false;
	}

	public bool GetTag (int tag, out Result val) 
	{
	    val = null;

	    if (cached_tag == tag) {
		if (cached_tag_rval != null) {
		    val = cached_tag_rval;
		    return false;
		}

		return GetOneTarget (cached_tag_tval, out val);
	    }
		
	    object o = proj.Graph.GetTargetTag (tid, tag);

	    if (o == null)
		return false;

	    if (o is Result) {
		val = (Result) o;
		return false;
	    }

	    return GetOneTarget ((int) o, out val);
	}

	public bool GetTag (string tag, out Result val)
	{
	    return GetTag (proj.Graph.GetTagId (tag), out val);
	}

	public bool HasTag (int tag, out bool has_it) 
	{
	    // Kind of an unfortunate signature, but
	    // the out bool parameter ought to force the caller
	    // to pay attention to the return value's meaning

	    Result r;

	    has_it = false;

	    if (GetTag (tag, out r))
		return true;

	    if (r == null)
		return false;

	    if (!(r is MBBool)) {
		string s = String.Format ("Expected tag \'{0}\' to be a boolean value; got type {1}",
					  proj.Graph.GetTagName (tag), r.GetType ().ToString ());
		proj.Log.PushLocation (target);
		proj.Log.Error (2007, s, r.ToString ());
		proj.Log.PopLocation ();
		return true;
	    }

	    has_it = (r as MBBool).Value;
	    return false;
	}

	public bool HasTag (string tag, out bool has_it) 
	{
	    return HasTag (proj.Graph.GetTagId (tag), out has_it);
	}

	// Value getting / setting

	public bool TryValueRecovery (out BuiltItem val) 
	{
	    // Need to be able to distinguish between "not cached"
	    // and "error in checking build readiness" (~ "error
	    // evaluating deps")

	    val = BuiltItem.Null;
	    proj.Log.PushLocation (target);

	    if (CheckBuildReadiness ()) {
		proj.Log.PopLocation ();
		return true;
	    }

	    val = pi.pp.GetItem (basename);

	    if (!val.IsValid)
		goto done;
			
	    if (!val.Result.Check (pi.context)) {
		proj.Log.Log ("project.external_invalid", basename);
		proj.Log.Warning (3008, "The cached result was not valid; rebuilding.", val.Result.ToString ());
		val = BuiltItem.Null;
		goto done;
	    }

	    if (val.IsFixed)
		goto done;

	    if (cur_build_fp != val.BuildPrint) {
		val = BuiltItem.Null;
		goto done;
	    }
	    
	    // Update the fingerprint in case that's needed. This can only
	    // happen with results stored external to the state file (eg MBFiles);
	    // might want a flag on Results to indicate whether they reference
	    // data outside the cache. Save the new FP ASAP so any other references
	    // this catch any changes to the value (so build fp's get updated
	    // and things depending on this result get rebuilt).

	    val.ResultPrint = val.Result.GetFingerprint (pi.context, val.ResultPrint);
	    CacheValue (val);
	    
	    // We got it from the cache successfully we can do so in the future.
	    
	    proj.SetTargetState (tid, WrenchProject.TargetState.BuiltOk);

	done:
	    proj.Log.PopLocation ();
	    return false;
	}

	public BuiltItem GetRawCachedValue () 
	{
	    return pi.pp.GetItem (basename);
	}

	public BuiltItem BuildValue () 
	{
	    proj.SetTargetState (tid, WrenchProject.TargetState.Building);

	    if (CheckBuildReadiness ()) {
		proj.SetTargetState (tid, WrenchProject.TargetState.BuiltError);
		return BuiltItem.Null;
	    }

	    proj.Log.PushLocation (target);
	    proj.Log.Log ("project.build", target);

	    BuiltItem res;
	    int errors_before = proj.Log.NumErrors;

	    try {
		res.Result = Rule.Build (pi.context);
	    } catch (Exception e) {
		proj.Log.Error (3009, "Unhandled exception during build", e.ToString ());
		res.Result = null;
	    }
			
	    Rule.ClearArgValues ();

	    if (res.Result == null) {
		if (proj.Log.NumErrors == errors_before)
		    proj.Log.Error (4001, "Build failed without any error being reported",
				  null);

		UncacheValue ();
		proj.SetTargetState (tid, WrenchProject.TargetState.BuiltError);
		proj.Log.PopLocation ();
		return BuiltItem.Null;
	    }

	    if (!res.Result.Check (pi.context)) {
		proj.Log.Error (3000, "Result fails check after build", res.Result.ToString ());
		UncacheValue ();
		proj.SetTargetState (tid, WrenchProject.TargetState.BuiltError);
		proj.Log.PopLocation ();
		return BuiltItem.Null;
	    }

	    res.ResultPrint = res.Result.GetFingerprint (pi.context, null);
	    res.BuildPrint = cur_build_fp;
	    
	    CacheValue (res);
	    proj.SetTargetState (tid, WrenchProject.TargetState.BuiltOk);
	    proj.Log.PopLocation ();
	    return res;
	}

	// Unconditionally gets the value. If an unavoidable failure happens,
	// returns BuiltItem.Null.

	public BuiltItem GetValue () 
	{
	    BuiltItem res;
	    
	    if (TryValueRecovery (out res))
		return BuiltItem.Null;

	    if (res.IsValid)
		return res;

	    return BuildValue ();
	}
	
	public void CacheValue (BuiltItem res) 
	{
	    pi.pp.SetItem (basename, res);
	}

	public void UncacheValue () 
	{
	    pi.UncacheValue (basename);
	}
	
	public void FixValue (Result r) 
	{
	    pi.FixValue (basename, r, null);
	}

	// Private implementation

	Rule GetRule () 
	{
	    Type t = proj.Graph.GetTargetRuleType (tid);
					
	    if (t == null) {
		proj.Log.PushLocation (target);
		proj.Log.Error (1007, "Target has null rule type.", null);
		proj.Log.PopLocation ();
		return null;
	    }

	    rule = (Rule) Activator.CreateInstance (t);
	    return rule;
	}
			
	ArgCollector GetArgs () 
	{
	    if (Rule == null)
		return null;
	    
	    proj.Log.PushLocation (target);
			
	    ac = new ArgCollector (rule);
			
	    if (proj.Graph.ApplyTargetDependencies (tid, ac, proj.Log)) {
		// Error will be reported (with the correct location!)
		proj.Log.PopLocation ();
		return null;
	    }

	    proj.Log.PopLocation ();
	    return ac;
	}

	bool CheckBuildReadiness () 
	{
	    if (Rule == null || Args == null)
		// Error will have been reported
		return true;

	    if (!Args.ArgsFinalized) {
		if (Args.FinalizeArgs (proj.BuildManager, proj.Log))
		    return true;
	    }

	    if (cur_build_fp == null)
		cur_build_fp = MakeBuildPrint (Rule, Args, pi.context);

	    // Do this after setting cur_build_fp in case the
	    // rule somehow is incorrently including arg state
	    // in its fingerprint.

	    Rule.FetchArgValues (Args);

	    return false;
	}

	public static Fingerprint MakeBuildPrint (Rule rule, ArgCollector args, IBuildContext ctxt)
	{
	    List<Fingerprint> fl = new List<Fingerprint> ();

	    fl.Add (rule.GetFingerprint (ctxt, null));
	    args.CopyFingerprintData (fl, ctxt);
	    
	    return new CompositeFingerprint (fl, ctxt, null);
	}
    }
}
					
