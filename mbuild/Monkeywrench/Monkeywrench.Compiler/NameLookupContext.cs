//
// NameLookupContext.cs -- an environment in which we look up names of targets
// and names in bundles.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using Mono.Build;
using Mono.Build.Bundling;

namespace Monkeywrench.Compiler {

    public class NameLookupContext : TypeResolver, ICloneable {

	// Manager
	
	BundleManager bm;
	
	public BundleManager Manager { get { return bm; } }
	
	public bool SetManager (BundleManager bm, IWarningLogger logger) 
	{
	    if (bm == null)
		throw new ArgumentNullException ();
	    if (this.bm != null)
		throw new Exception ("Already have manager set");

	    this.bm = bm;
	    
	    // This is pretty weak.

	    foreach (string ns in nsqueue.Keys)
	    	if (UseNamespace (ns, nsqueue[ns], logger))
	    	    return true;

	    return false;
	}
	
	// Lookup. nsqueue maps namespaces to decllocs

	List<string> namespaces = new List<string> ();
	Dictionary<string,string> nsqueue = new Dictionary<string,string> ();
	Dictionary<Type,StructureTemplate> known_structs = new Dictionary<Type,StructureTemplate> ();

	public bool UseIdent (string ident, string declloc, IWarningLogger log)
	{
	    if (ident[0] == '/')
		throw new Exception ("Boo"); // UseStructureTemplate ();
	    else if (ident.IndexOf ('/') < 0)
		return UseNamespace (ident, declloc, log);
	    
	    log.Error (9999, "Don't know how to use[] the identifier", ident);
	    return true;
	}

	//public bool UseRawNamespace (string ns, IWarningLogger log) 
	public void UseRawNamespace (string ns) 
	{
	    // Here we only look at the types in the namespace, and
	    // don't do any structure template initialization.
	    // No declloc is needed because we won't be declaring
	    // any providers or anything like that.

	    namespaces.Add (ns + ".");
	}
	
	public bool UseNamespace (string ns, string declloc, IWarningLogger log)
	{
	    if (bm == null) {
		// No bundle manager yet. Save the info; once our manager is
		// set, we can do all the loading and structure template instantiation.
		nsqueue[ns] = declloc;
		return false;
	    }

	    // Standard initialization.

	    UseRawNamespace ("MBuildDynamic." + ns);

	    // Now we load the associated structure template and instantiate
	    // anything that needs instantiation.

	    // XXX this was later before; why???
	    if (declloc == null)
		    throw ExHelp.App ("! {0}, {1}", ns, declloc);

	    StructureTemplate st = bm.UseNamespaceTemplate (ns, declloc, log);

	    if (st == null)
		return true;
	    
	    known_structs[st.GetType ()] = st;
	    return false;
	}

	public void UseStructure (StructureTemplate st)
	{
	    Type stype = st.GetType ();

	    known_structs[stype] = st;

	    UseRawNamespace (stype.Namespace);
	}

#if NO	
	public Type LookupFQN (string name, IWarningLogger logger) 
	{
	    Type t;

	    // fully qualified name. Assume that the name must be found --
	    // null return value can/does not distinguish between 'type not
	    // found' and 'error occurred when attempting to find type'.

	    if (bm.LookupType (name, out t, logger))
		return null;
		
	    if (t == null) {
		logger.Error (2023, "Type lookup failed -- did you forget a using [] directive?", name);
		return null;
	    }
		
	    return t;
	}
#endif

	IEnumerable<Type> VisibleTypes {
	    get {
		foreach (Type t in bm.BundleTypes) {
		    // FIXME: slooow algo. Could build a hashtable first.
		    if (namespaces.Contains (t.Namespace + "."))
			yield return t;
		}
	    }
	}

	public override bool ResolveName (string name, out TargetTemplate tmpl, IWarningLogger log) 
	{
	    tmpl = null;

	    Type t;

	    if (ResolveType (name, out t, log))
		return true;

	    if (t.IsSubclassOf (typeof (Rule))) {
		Type t2 = TemplateForRule (t, log);

		if (t2 == null) {
		    // So yeah this is kinda dumb namespacing.
		    tmpl = new Mono.Build.RuleLib.RegexMatcher.RuleOnlyTemplate (t);
		    return false;
		}

		t = t2;
	    }

	    if (!t.IsSubclassOf (typeof (TargetTemplate))) {
		string s = String.Format ("Type {0} (resolved from {1}) should be a TargetTemplate but isn't",
					  t.FullName, name);
		log.Error (2022, s, null);
		return true;
	    }

	    object o;

	    if (InstantiateBoundType (t, out o, log))
		return true;

	    tmpl = (TargetTemplate) o;
	    return false;
	}

	public bool ResolveType (string name, out Type t, IWarningLogger log) 
	{
	    if (name == null)
		throw new ArgumentNullException ();
	    if (bm == null)
		throw new Exception ("Need to set manager before performing type lookup.");

	    t = null;

	    if (name.IndexOf ('.') != -1) {
		if (bm.LookupType (name, out t, log))
		    return true;
	    } else {
		foreach (string ns in namespaces) {
		    Type match = null;

		    if (bm.LookupType (ns + name, out match, log))
			return true;
		
		    if (match == null)
			continue;
		
		    if (t != null) {
			string s = String.Format ("Ambiguous type reference: {0} could be {1} or {2}",
						  name, t.FullName, match.FullName);
			log.Error (2022, s, null);
			return true;
		    }
		
		    t = match;
		}
	    }

	    if (t == null) {
		log.Error (2022, "Could not resolve type name " + name, null);
		return true;
	    }

	    return false;
	}

	Type TemplateForRule (Type rtype, IWarningLogger log)
	{
	    Type match = null;

	    foreach (Type t in VisibleTypes) {
		if (!t.IsSubclassOf (typeof (TargetTemplate)))
		    continue;

		object[] attrs = t.GetCustomAttributes (typeof (RuleBindingAttribute), false);

		if (attrs == null || attrs.Length == 0)
		    continue;

		if (!(attrs[0] as RuleBindingAttribute).RuleType.Equals (rtype))
		    continue;

		if (match != null) {
		    string s = String.Format ("Two potential templates associated with rule {0}: " +
					      "{1} or {2}", rtype, match, t);
		    log.Warning (9999, s, null);
		    break;
		}
		
		match = t;
	    }

	    return match; // may be null;
	}

	public bool InstantiateBoundType (Type t, out object result, IWarningLogger log)
	{
	    object init_obj;
	    result = null;

	    object[] attrs = t.GetCustomAttributes (typeof (StructureBindingAttribute), false);

	    if (attrs == null || attrs.Length == 0)
		init_obj = null;
	    else {
		StructureBindingAttribute sba = attrs[0] as StructureBindingAttribute;
		if (!sba.UsesStructure)
		    init_obj = null;
		else {
		    Type stype = sba.StructureType;
	       
		    if (!known_structs.ContainsKey (stype)) {
			string s = String.Format ("Type {0} must be created in the context of a {1} structure, " +
						  "but none is referenced in this scope.", t, stype);
			log.Error (9999, s, null);
			return true;
		    }

		    init_obj = known_structs[stype];
		}
	    }

	    result = Activator.CreateInstance (t, init_obj);

	    return false;
	}

	// matching

	Dictionary<MatcherKind,List<IMatcher>> matchers = 
	    new Dictionary<MatcherKind,List<IMatcher>> ();
	bool mlist_had_error;

	public override IEnumerable<IMatcher> GetMatchers (MatcherKind kind, IWarningLogger log)
	{
	    if (bm == null)
		throw new Exception ("Need to set manager before performing type lookup.");

	    if (!matchers.ContainsKey (kind)) {
		mlist_had_error = false;
		matchers[kind] = new List<IMatcher> (ListMatchers (kind, log));

		if (mlist_had_error) {
		    matchers[kind] = null;
		    return null;
		}
	    }

	    return matchers[kind];
	}
	 
	IEnumerable<IMatcher> ListMatchers (MatcherKind kind, IWarningLogger log)
	{
	    mlist_had_error = false;

	    foreach (Type t in VisibleTypes) {
		object[] attrs = t.GetCustomAttributes (typeof (MatcherAttribute), false);

		if (attrs == null || attrs.Length == 0)
		    continue;

		if (((MatcherAttribute) attrs[0]).Kind != kind)
		    continue;

		object o;

		if (InstantiateBoundType (t, out o, log)) {
		    mlist_had_error = true;
		    yield break;
		}

		yield return (IMatcher) o;
	    }
	}

	// blah
	
	public object Clone () 
	{
	    if (this.bm != null)
		throw new InvalidOperationException ();
	    
	    NameLookupContext clone = new NameLookupContext ();
	    clone.nsqueue = new Dictionary<string,string> (nsqueue);
	    clone.known_structs = new Dictionary<Type,StructureTemplate> (known_structs);
	    clone.namespaces = new List<string> (namespaces);
	    return clone;
	}
    }
}
