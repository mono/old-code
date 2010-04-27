using System;
using System.Reflection;
using System.Collections.Generic;
using System.CodeDom;

using Mono.Build;
using Mono.Build.Bundling;

namespace MBBundleGen {

    public class MetaRuleBuilder {

	RuleBuilder rb;
	RuleTemplateBuilder tmpl;

	public RuleBuilder Rule { get { return rb; } }
	public RuleTemplateBuilder Template { get { return tmpl; } }

	public UserType BaseClass;

	public bool Resolve (TypeResolveContext trc, bool errors)
	{
	    if (BaseClass.ResolveExtension ("RTemplate", trc, errors))
		return true;

	    bool? ret = BaseClass.ResolvesAsRule (trc, errors);
	    if (ret == null)
		return true;

	    if ((bool) ret) {
		// We derive from a Rule. Our Template's base class
		// should be the default TargetTemplate. The rule's
		// BaseClass is our BaseClass.
		rb.BaseClass = BaseClass;
		return false;
	    }

	    UserType rule = BaseClass.ResolveBoundRuleType (trc, errors);
	    if (rule == null) {
		Console.Error.WriteLine ("Rule baseclass {0} of {1} is not either a rule " + 
					 "or a rule-bound template", BaseClass, this);
		return true;
	    }

	    rb.BaseClass = rule;
	    tmpl.BaseClass = BaseClass;
	    return false;
	}

	static readonly UserType RuleType = new UserType (typeof (Rule));

	public MetaRuleBuilder (string name, NamespaceBuilder ns, CodeLinePragma loc, TypeAttributes attr)
	{
	    rb = new RuleBuilder (name, ns, loc, attr);
	    tmpl = new RuleTemplateBuilder (name + "RTemplate", rb, ns, loc, attr);

	    ns.AddMetaRule (this);
	    BaseClass = RuleType;
	}
    }
}
