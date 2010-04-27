using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using Mono.Build;

namespace Mono.Build.Bundling {

    public abstract class TargetBuilder : TargetInfoHolder<Type,TargetBuilder,string,string> {

	protected TargetBuilder (ProviderBuilder pb)
	{
	    this.pb = pb;
	}
		
	// properties
	
	ProviderBuilder pb;

	public ProviderBuilder Owner { get { return pb; } }
	
	protected string name;
	
	public string Name { get { return name; } }
	
	public string FullName {
	    get {
		return pb.Basis + name;
	    }
	}

	// Validity management

	TargetValidity validity = TargetValidity.Referenced;

	internal TargetValidity Validity { get { return validity; } }

	internal bool Define (IWarningLogger log)
	{
	    if (validity == TargetValidity.Defined) {
		log.Error (2009, "Trying to redefine the target " + FullName, 
			   null);
		return true;
	    }

	    validity = TargetValidity.Defined;
	    return false;
	}

	internal void Request ()
	{
	    // This sets the Validity to *at least* Requested state.

	    if (validity == TargetValidity.Referenced)
		validity = TargetValidity.Requested;
	}

	// Builder interface.

	string template_name = null;

	public string TemplateName { 
	    get { return template_name; }
	    set { template_name = value; }
	}

	public Type RuleType {
	    get { return Rule; }
	    set { Rule = value; }
	}

	SingleValue<TargetBuilder> ConvertValue (SingleValue<string> value)
	{
	    if (value.IsResult)
		return new SingleValue<TargetBuilder> ((Result) value);

	    string target = (string) value;
	    target = StrUtils.CanonicalizeTarget (target, pb.Basis);

	    TargetBuilder tb;

	    if (target.StartsWith (pb.Basis))
		tb = pb.Owner.RequestTarget (target);
	    else
		tb = pb.Owner.ReferenceTarget (target);

	    return new SingleValue<TargetBuilder> (tb);
	}

	// All of these just pass off to TargetInfoHolder with
	// conversion of the SingleValues.

	public void AddDep (SingleValue<string> sv)
	{
	    AddDep (ConvertValue (sv));
	}

	public void AddDep (Result r)
	{
	    // FIXME: compat
	    AddDep (new SingleValue<TargetBuilder> (r));
	}

	public void AddDep (string target)
	{
	    // FIXME: compat
	    AddDep (new SingleValue<string> (target));
	}

	public void AddDep (string arg_name, SingleValue<string> sv)
	{
	    AddDep (arg_name, ConvertValue (sv));
	}

	public void AddDep (string arg_name, Result r)
	{
	    // FIXME: compat
	    AddDep (arg_name, new SingleValue<TargetBuilder> (r));
	}

	public void AddDep (string arg_name, string target)
	{
	    // FIXME: compat
	    AddDep (arg_name, new SingleValue<string> (target));
	}

	public void AddDefaultOrdered (SingleValue<string> sv)
	{
	    AddDefaultOrdered (ConvertValue (sv));
	}

	public void AddDefaultOrdered (Result r)
	{
	    // FIXME: compat
	    AddDefaultOrdered (new SingleValue<TargetBuilder> (r));
	}

	public void AddDefaultOrdered (string target)
	{
	    // FIXME: compat
	    AddDefaultOrdered (new SingleValue<string> (target));
	}

	public void SetDefault (string arg, SingleValue<string> sv)
	{
	    SetDefault (arg, ConvertValue (sv));
	}

	// Subclasses should implement whatever rule-guessing logic
	// they feel like; return null and report an error if
	// guessing fails.

	protected abstract TargetTemplate InferTemplate (TypeResolver res, IWarningLogger log);

	protected bool ResolveTemplate (TypeResolver res, IWarningLogger log) 
	{
	    TargetTemplate tmpl;

	    if (TemplateName != null)
		tmpl = res.ResolveName (TemplateName, log);
	    else if (Rule == null)
		// This is sketchy: assume that if we have a rule by now,
		// we need no templating; if we don't, assume that we
		// want to do inference
		tmpl = InferTemplate (res, log);
	    else
		return false;

	    if (tmpl == null)
		// Error will already be reported
		return true;
		
	    tmpl.ApplyTemplate (this);

	    if (Rule == null) {
		log.Error (9999, "Target did not have its rule set by " + 
			   "its primary template {0}", tmpl.ToString ());
		return true;
	    }
		
	    if (!Rule.IsSubclassOf (typeof (Mono.Build.Rule))) {
		string s = String.Format ("Invalid rule `{0}\' for target {1}: not a subclass of Rule", 
					  Rule, FullName);
		log.Error (2029, s, Rule.FullName);
		return true;
	    }

	    return false;
	}

	// tags

	public void AddTag (string name, SingleValue<string> value) 
	{
	    AddTag (name, ConvertValue (value));
	}
    }
}
