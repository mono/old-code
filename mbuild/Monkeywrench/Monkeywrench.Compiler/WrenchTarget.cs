// WrenchTarget.cs -- Monkeywrench implementation of the 
// TargetBuilder abstract class.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using Mono.Build;
using Mono.Build.Bundling;

namespace Monkeywrench.Compiler {

    public class WrenchTarget : TargetBuilder {
	// FIXME: store the location at which the target was defined
	// so that we can report better error messages.

	short id;

	internal WrenchTarget (WrenchProvider wp, string name, short id) : base (wp) 
	{
	    this.name = name;
	    this.id = id;
	}

	// properties
	
	public short ShortId { 
	    get { return id; }
	}

	public int Id {
	    get {
		return (((int) ((WrenchProvider) Owner).Id) << 16) | ((int) id);
	    }
	}
	
	public bool VisitDependencies (IDependencyVisitor idv)
	{
	    foreach (SingleValue<TargetBuilder> sv in UnnamedDeps) {
		if (idv.VisitUnnamed (sv))
		    return true;
	    }

	    foreach (SingleValue<TargetBuilder> sv in DefaultOrderedDeps) {
		if (idv.VisitDefaultOrdered (sv))
		    return true;
	    }

	    Rule rinst = (Rule) Activator.CreateInstance (Rule);
	    Dictionary<string,int> argmap = rinst.MakeArgNameMap ();

	    foreach (string arg in ArgsWithDeps) {
		int argid = argmap[arg];

		foreach (SingleValue<TargetBuilder> sv in GetArgDeps (arg)) {
		    if (idv.VisitNamed (argid, sv))
			return true;
		}
	    }

	    foreach (string arg in ArgsWithDefaults) {
		if (idv.VisitDefaultValue (argmap[arg], GetArgDefault (arg)))
		    return true;
	    }

	    return false;
	}

	// tags. We need to list tags and add them later because
	// the parser reads the tags *before* the target's template
	// is applied, so the template's tag settings will override
	// ours. The template model is really what is sane (eg,
	// subclasses can override superclass tags, which makes sense),
	// so work around here.
	//
	// What really needs to be fixed is applying the template after
	// parsing. If we could just resolve types before parsing, then
	// we could just mutate an already-template TargetBuiler and
	// everything would be peachy keen. But doing that would require
	// reading the project [] section first, or putting the project
	// info into a separate file, which I really want to avoid.

	Dictionary<string,SingleValue<string>> wait_tags;
		
	public void AddWaitTag (string name, SingleValue<string> value) 
	{
	    if (wait_tags == null)
		wait_tags = new Dictionary<string,SingleValue<string>> ();
	    
	    wait_tags[name] = value;
	}

	void ApplyWaitTags ()
	{
	    if (wait_tags == null)
		return;

	    foreach (string key in wait_tags.Keys)
		AddTag (key, wait_tags[key]);

	    wait_tags = null;
	}

	public IEnumerable<KeyValuePair<string,SingleValue<TargetBuilder>>> Tags {
	    get {
		foreach (string tag in TagsWithValues)
		    yield return new KeyValuePair<string,SingleValue<TargetBuilder>> (tag, GetTagValue (tag));
	    }
	}

	// fixup

	string DirectTransformDep {
	    get {
		if (HasNamedDeps)
		    return null;

		bool first = true;
		string dtdep = null;

		foreach (SingleValue<TargetBuilder> sv in UnnamedDeps) {
		    if (!first)
			// More than one unnamed dep?
			return null;

		    if (sv.IsResult)
			return null;
		    
		    dtdep = ((TargetBuilder) sv).Name;
		    first = false;
		}

		return dtdep;
	    }
	}

	protected override TargetTemplate InferTemplate (TypeResolver res, IWarningLogger log)
	{
	    TargetTemplate tmpl = null;
	    string dtname = DirectTransformDep;
		    
	    if (dtname != null) {
		if (res.TryMatch (dtname, MatcherKind.DirectTransform, out tmpl, log))
		    return null;

		if (tmpl != null)
		    return tmpl;
	    }
		    
	    if (Rule == null) {
		if (res.TryMatch (Name, MatcherKind.Target, out tmpl, log))
		    return null;
	    }
		    
	    if (tmpl == null)
		log.Error (2028, "Cannot guess the template or rule for this target", FullName);

	    return tmpl;
	}

	internal bool DoFixup (NameLookupContext nlc, IWarningLogger log) 
	{
	    log.PushLocation (FullName);

	    try {
		if (ResolveTemplate (nlc, log))
		    return true;

		ApplyWaitTags ();

		// Now register our tags with the GraphBuilder

		GraphBuilder gb = (GraphBuilder) Owner.Owner;

		foreach (string tag in TagsWithValues) {
		    if (gb.GetTagId (tag) < 0)
			return true;
		}
		
		// Now make sure that all of our argument names are valid.
		
		Rule rinst = (Rule) Activator.CreateInstance (Rule);
		Dictionary<string,int> argmap = rinst.MakeArgNameMap ();
		
		foreach (string arg in ArgsWithDeps) {
		    if (argmap.ContainsKey (arg))
			continue;
			
		    string s = String.Format ("Argument `{0}' does not exist in rule " +
					      "`{1}'", arg, Rule);
		    log.Error (2024, s, null);
		    return true;
		}
	    } finally {
		log.PopLocation ();
	    }

	    return false;
	}

	public IEnumerable<KeyValuePair<string,SingleValue<int>>> IdTags {
	    get {
		foreach (string tag in TagsWithValues) {
		    SingleValue<TargetBuilder> inval = GetTagValue (tag);
		    SingleValue<int> outval;

		    if (inval.IsResult)
			outval = new SingleValue<int> ((Result) inval);
		    else {
			WrenchTarget wt = (WrenchTarget) (TargetBuilder) inval;
			outval = new SingleValue<int> (wt.Id);
		    }

		    yield return new KeyValuePair<string,SingleValue<int>> (tag, outval);
		}
	    }
	}
	
    }
}
