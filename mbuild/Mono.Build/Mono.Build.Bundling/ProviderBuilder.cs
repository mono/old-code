using System;
using System.Collections;
using System.Collections.Generic;

using Mono.Build;

namespace Mono.Build.Bundling {

    public abstract class ProviderBuilder {

	protected ProviderBuilder (ProjectBuilder proj, string basis) 
	{
	    this.proj = proj;
	    this.basis = basis;
	}

	// Properties

	protected ProjectBuilder proj;

	public ProjectBuilder Owner { get { return proj; } }

	string basis;

	public string Basis { get { return basis; } }

	string decl_loc;

	public string DeclarationLoc { 
	    get { return decl_loc; } 
	}

	// "Claiming"

	public bool Claimed { get { return decl_loc != null; } }

	public void Claim (string newloc)
	{
	    if (decl_loc != null)
		throw ExHelp.InvalidOp ("Cannot claim provider with new DeclLoc `{0}': " +
					"it has already been claimed with `{1}'", 
					newloc, decl_loc);
	    if (newloc == null)
		throw new ArgumentNullException ();

	    decl_loc = newloc;
	}

	public void WeakClaim (string newloc)
	{
	    if (decl_loc != null)
		return;

	    if (newloc == null || newloc.Length == 0)
		throw new ArgumentNullException ();

	    decl_loc = newloc;
	}

	// Context -- used for the lookup of names of this provider's
	// targets. This needs to be here for providers created by 
	// structure templates, which need context to instantiate any
	// structure-bound items they may reference

	public abstract void AddContextStructure (StructureTemplate st);

	// Target management

	Dictionary<string,TargetBuilder> targets = 
	    new Dictionary<string,TargetBuilder> ();

	protected abstract TargetBuilder CreateTarget (string name);

	// Throws an exception if the target with the given
	// name has not yet been defined -- this function should only
	// be called if the given target is known to exist by now.

	public TargetBuilder GetTarget (string name)
	{
	    if (!targets.ContainsKey (name) || 
		targets[name].Validity != TargetValidity.Defined) {
		throw ExHelp.Argument ("name", "The target {0}{1} should be " +
				       "defined by now, but isn't.", basis, name);
	    }

	    return targets[name];
	}

	bool done_modifying = false;

	public bool DoneModifying { get { return done_modifying; } }

	TargetBuilder EnsureTarget (string name)
	{
	    TargetBuilder tb;

	    if (targets.ContainsKey (name))
		tb = targets[name];
	    else {
		if (done_modifying)
		    throw ExHelp.InvalidOp ("Cannot create new target {0} because its " +
					    "provider is been marked as finished", name);

		tb = CreateTarget (name);
		targets[name] = tb;
	    }

	    return tb;
	}

	// Returns null and logs an error if the target with the given
	// name has already been defined.

	public TargetBuilder DefineTarget (string name, IWarningLogger log)
	{
	    if (done_modifying)
		throw ExHelp.InvalidOp ("Cannot define target {0} because its " +
					"provider is now unmodifiable", name);

	    TargetBuilder tb = EnsureTarget (name);

	    if (tb.Define (log))
		return null;

	    return tb;
	}

	public bool CanDefineTarget (string name)
	{
	    if (done_modifying)
		return false;

	    if (!targets.ContainsKey (name))
		return true;

	    return targets[name].Validity != TargetValidity.Defined;
	}

	public TargetBuilder RequestTarget (string name)
	{
	    // We can request targets via a SetDefault call via ApplyTemplate
	    // via ResolveTemplate during target fixup, at which point providers
	    // will have their done_modifying flag set. So only signal the error
	    // if the request would have actually created a new target, not if 
	    // it is effectively a noop.

	    if (done_modifying && !targets.ContainsKey (name))
		throw ExHelp.InvalidOp ("Cannot request target {0} because its " +
					"provider is now unmodifiable", name);

	    TargetBuilder tb = EnsureTarget (name);

	    tb.Request ();

	    return tb;
	}

	public TargetBuilder ReferenceTarget (string name)
	{
	    if (targets.ContainsKey (name))
		return targets[name];

	    if (done_modifying)
		throw ExHelp.InvalidOp ("Cannot reference new target {0} because its " +
					"provider is been marked as finished", name);

	    TargetBuilder tb = CreateTarget (name);
	    targets[name] = tb;
	    return tb;
	}

	public IEnumerable<TargetBuilder> DefinedTargets {
	    get {
		foreach (TargetBuilder tb in targets.Values)
		    if (tb.Validity == TargetValidity.Defined)
			yield return tb;
	    }
	}

	public IEnumerable<KeyValuePair<string,TargetValidity>> AllValidities {
	    get {
		foreach (KeyValuePair<string,TargetBuilder> kvp in targets) {
		    KeyValuePair<string,TargetValidity> v = 
			new KeyValuePair<string,TargetValidity> (kvp.Key, 
								 kvp.Value.Validity);
		    yield return v;
		}
	    }
	}

	public bool DoneRequesting (TypeResolver res, IWarningLogger log) 
	{
	    if (done_modifying)
		throw ExHelp.InvalidOp ("Cannot call DoneRequesting twice on a provider");

	    foreach (KeyValuePair<string,TargetBuilder> kvp in targets) {
		TargetValidity tv = kvp.Value.Validity;
		string fullname = Basis + kvp.Key;

		if (tv == TargetValidity.Referenced) {
		    // FIXME: this error message sucks
		    log.Error (2000, "A target was referenced from another provider " +
			       "that was not formally allowed to request it, and the " +
			       "target was not defined by its provider", fullname);
		    return true;
		}

		if (tv == TargetValidity.Requested) {
		    TargetTemplate tmpl;

		    if (res.TryMatch (kvp.Key, MatcherKind.Dependency, out tmpl, log))
			return true;
			
		    if (tmpl == null) {
			log.Error (2027, "Could not guess a rule for a target " + 
				   "implicitly defined in a dependency. Do you " +
				   "need to add a using [] line?", fullname);
			return true;
		    }
		    
		    kvp.Value.Define (log);
		    tmpl.ApplyTemplate (kvp.Value);
		}
	    }

	    done_modifying = true;
	    return false;
	}

	// Utility, for bundlegen

	public bool DefineConstantTarget (string name, Result r, IWarningLogger log)
	{
	    TargetBuilder tb = DefineTarget (name, log);

	    if (tb == null)
		return true;

	    tb.RuleType = typeof (Mono.Build.RuleLib.CloneRule);
	    tb.AddDep (r);
	    return false;
	}
    }
}	
