using System;
using System.Reflection;
using System.Collections.Generic;
using System.CodeDom;

using Mono.Build;
using Mono.Build.Bundling;

namespace MBBundleGen {

    public class RuleTemplateBuilder : TemplateBuilder {

	// Just adds a member pointing to its associated RuleBuilder
	// Consider definining an abstract RuleTargetTemplate superclass
	// that all rule-bound templates derive from? Don't see a need
	// to do that.

	RuleBuilder rb;
	public RuleBuilder Rule { get { return rb; } }

	public RuleTemplateBuilder (string name, RuleBuilder rb, NamespaceBuilder ns, CodeLinePragma loc,
				    TypeAttributes attr) : base (name, ns, loc, attr)
	{
	    this.rb = rb;
	}

	public override bool Resolve (TypeResolveContext trc, bool errors)
	{
	    if (base.Resolve (trc, errors))
		return true;

	    RuleType = rb.ResolveSelfType (trc, errors);
	    return (RuleType == null);
	}

	protected override void Emit (CodeTypeDeclaration ctd)
	{
	    EmitTemplate (ctd);

	    // Our attribute

	    CodeAttributeDeclaration a = 
		new CodeAttributeDeclaration ("Mono.Build.Bundling.RuleBindingAttribute");
	    a.Arguments.Add (new CodeAttributeArgument (new CodeTypeOfExpression (Rule.CodeSelfType)));

	    ctd.CustomAttributes.Add (a);
	}
    }
}
