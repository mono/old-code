//
// A simple target
//

using System;
using System.Reflection;
using System.Collections;

namespace Mono.Build {

    public class SimpleTarget : TagTargetBase { 
		
	string[] deps;
	Result[] cdeps;
	Hashtable named_deps;
	
	public SimpleTarget (string name, Type rule, string[] deps, Result[] cdeps) 
	    : base (name, rule) 
	{
	    this.deps = deps;
	    this.cdeps = cdeps;
	    this.named_deps = null;

	    ApplyRuleTags ();
	}

	public SimpleTarget (string name, Type rule, string[] deps) 
	    : this (name, rule, deps, null) 
	{}

	// Dependencies

	void AddNamedInternal (string name, object dep) 
	{
	    if (named_deps == null)
		named_deps = new Hashtable ();
	    
	    named_deps[name] = dep;
	}

	public void AddNamedArg (string name, Result value) 
	{
	    AddNamedInternal (name, value);
	}

	public void AddNamedArg (string name, string dep) 
	{
	    AddNamedInternal (name, dep);
	}

	public override bool VisitDependencies (IDependencyVisitor<string,string> idv)
	{
	    if (deps != null)
		for (int i = 0; i < deps.Length; i++)
		    if (idv.VisitUnnamedTarget (deps[i]))
			return true;
	    
	    if (cdeps != null)
		for (int i = 0; i < cdeps.Length; i++)
		    if (idv.VisitUnnamedResult (cdeps[i]))
			return true;
	    
	    if (named_deps != null) {
		foreach (string arg in named_deps.Keys) {
		    bool res;
		    object o = named_deps[arg];

		    if (o is string)
			res = idv.VisitNamedTarget (arg, (string) o);
		    else
			res = idv.VisitNamedResult (arg, (Result) o);

		    if (res)
			return true;
		}
	    }

	    return false;
	}
    }
}
