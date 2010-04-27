using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using Mono.Build;

namespace Mono.Build.Bundling {

    // This class holds "target-like" information.
    //
    // That information is specifically:
    //   * a rule item
    //   * a list of "unnamed" (not associated with a specific 
    //     argument) dependencies
    //   * a set of lists of "named" dependencies (associated with an argument)
    //   * a set of optional default argument values (one per argument)
    //   * a list of values for the "default ordered" argument
    //   * a set of tag names associated with values
    //
    // Each value is a SingleResult<>, so it can either be a literal
    // result or a reference to another target.
    //
    // The implementation is completely braindead, BTW. But until it
    // causes problems, I'm sticking with it.
    //
    // This class is used in many places, since many things look like
    // MBuild targets. Most notably, TargetBuilder. If you don't want
    // to expose the whole interface, I recommend adding a TargetInfoHolder
    // field to your object and routing requests to it, rather than
    // subclassing.

    public class TargetInfoHolder<T_rule,T_targ,T_arg,T_tag> {

	public TargetInfoHolder () {}

	// Rule Type

	T_rule rule;

	public T_rule Rule {
	    get { return rule; }
	    set { rule = value; }
	}

	// Unnamed deps

	List<SingleValue<T_targ>> unnamed_deps;

	public void AddDep (SingleValue<T_targ> sv)
	{
	    if (unnamed_deps == null)
		unnamed_deps = new List<SingleValue<T_targ>> ();

	    unnamed_deps.Add (sv);
	}

	public bool HasUnnamedDeps {
	    get { return unnamed_deps != null; }
	}

	public IEnumerable<SingleValue<T_targ>> UnnamedDeps {
	    get {
		if (unnamed_deps == null)
		    return new SingleValue<T_targ> [0];
		return unnamed_deps;
	    }
	}

	// Named deps

	Dictionary<T_arg,List<SingleValue<T_targ>>> named_deps;

	public void AddDep (T_arg arg, SingleValue<T_targ> sv)
	{
	    if (named_deps == null)
		named_deps = new Dictionary<T_arg,List<SingleValue<T_targ>>> ();
	    
	    List<SingleValue<T_targ>> list;
	    
	    if (!named_deps.ContainsKey (arg)) {
		list = new List<SingleValue<T_targ>> ();
		named_deps[arg] = list;
	    } else
		list = named_deps[arg];
	    
	    list.Add (sv);
	}

	public bool HasNamedDeps {
	    get { return named_deps != null; }
	}

	public IEnumerable<T_arg> ArgsWithDeps {
	    get {
		if (named_deps == null)
		    return new T_arg [0];
		return named_deps.Keys;
	    }
	}

	public IEnumerable<SingleValue<T_targ>> GetArgDeps (T_arg arg)
	{
	    if (named_deps == null || !named_deps.ContainsKey (arg))
		return new SingleValue<T_targ> [0];

	    return named_deps[arg];
	}

	// Default ordered
	// 
	// We must distinguish these from simple unnamed deps
	// because we can't have random unnamed dependencies clogging up
	// the ordered argument. For instance, if "a" is a string
	// argument and "b" is a default ordered string list, then
	// the dependences [ "foo" [ "a", "b", "c" ] ] would result
	// in b = "foo", "a", "b", "c".
	
	List<SingleValue<T_targ>> deford_deps;

	public void AddDefaultOrdered (SingleValue<T_targ> sv)
	{
	    if (deford_deps == null)
		deford_deps = new List<SingleValue<T_targ>> ();

	    deford_deps.Add (sv);
	}

	public bool HasDefaultOrderedDeps {
	    get { return deford_deps != null; }
	}

	public IEnumerable<SingleValue<T_targ>> DefaultOrderedDeps {
	    get {
		if (deford_deps == null)
		    return new SingleValue<T_targ> [0];
		return deford_deps;
	    }
	}

	// Default values

	Dictionary<T_arg,SingleValue<T_targ>> defaults;

	public void SetDefault (T_arg arg, SingleValue<T_targ> sv)
	{
	    if (defaults == null)
		defaults = new Dictionary<T_arg,SingleValue<T_targ>> ();

	    defaults[arg] = sv;
	}

	public bool HasDefaults {
	    get { return defaults != null; }
	}

	public IEnumerable<T_arg> ArgsWithDefaults {
	    get {
		if (defaults == null)
		    return new T_arg [0];
		return defaults.Keys;
	    }
	}

	public SingleValue<T_targ> GetArgDefault (T_arg arg)
	{
	    if (defaults == null || !defaults.ContainsKey (arg))
		throw new InvalidOperationException ();

	    return defaults[arg];
	}

	// Tags

	Dictionary<T_tag,SingleValue<T_targ>> tags;
		
	public void AddTag (T_tag name, SingleValue<T_targ> value) 
	{
	    if (tags == null)
		tags = new Dictionary<T_tag,SingleValue<T_targ>> ();
	    
	    tags[name] = value;
	}

	public bool HasTags {
	    get { return tags != null; }
	}

	public IEnumerable<T_tag> TagsWithValues {
	    get {
		if (tags == null)
		    return new T_tag [0];
		return tags.Keys;
	    }
	}

	public SingleValue<T_targ> GetTagValue (T_tag tag)
	{
	    if (tags == null || !tags.ContainsKey (tag))
		throw new InvalidOperationException ();

	    return tags[tag];
	}

    }
}
