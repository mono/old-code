using System;
using System.Reflection;
using System.Collections.Generic;
using System.CodeDom;

using Mono.Build;
using Mono.Build.RuleLib;

namespace MBBundleGen {

    public class SourcefileRuleBuilder : TypeExpressedItem {

	FingerprintHelper fp = new FingerprintHelper ();

	public SourcefileRuleBuilder (string name, NamespaceBuilder ns, CodeLinePragma loc, TypeAttributes attr) : 
	    base (name, ns, loc, attr)
	{
	    BaseClass = new UserType (typeof (SourcefileRule));
	    fp.Add (name);
	}

	// Arguments

	public UserType ResultType;

	public override bool Resolve (TypeResolveContext trc, bool errors)
	{
	    return base.Resolve (trc, errors) | ResultType.Resolve (trc, errors);
	}

	protected override void Emit (CodeTypeDeclaration ctd)
	{
	    EmitEmptyCtor (ctd);
	    fp.EmitGetFingerprint (ctd);
	    RuleBuilder.EmitGeneralResult (ctd, ResultType);
	}
    }
}
