using System;
using System.Reflection;
using System.Collections.Generic;
using System.CodeDom;

using Mono.Build;
using Mono.Build.Bundling;

namespace MBBundleGen {

    public class BGTargetBuilder : EmittingInfoHolder {

	string name;

	public BGTargetBuilder (string name, BGProviderBuilder prov)
	{
	    RuleAsType = false;

	    this.name = name;

	    prov.AddTarget (this);
	    this.prov = prov;
	}

	BGProviderBuilder prov;

	public BGProviderBuilder Provider { get { return prov; } } 

	public string Name { get { return name; } }

	public override string ToString ()
	{
	    return name;
	}

	// Setup

	public bool Resolve (TypeResolveContext trc, bool errors)
	{
	    // We need to resolve the rule as a template now, rather
	    // than letting NameLookupContext do it, because NLC will
	    // be operating with an undefined set of 'usings', and
	    // probably won't be able to find the RTemplate associated
	    // with the rule.

	    if (Rule.ResolveExtension ("RTemplate", trc, errors))
		return true;

	    // Now we need to check that, if we have a template that
	    // uses a structure, that our provider can access such 
	    // a structure, so that the template can actually be 
	    // instantiated.
	    //
	    // FIXME: This code is exactly parallel to 
	    // StructureBoundItem.ContextualStructRef.

	    UserType ttmpl = Rule.ResolveUsedStructureType (trc, errors);

	    if (ttmpl == null)
		// Great, it's doesn't use anything, so whatever.
		return false;

	    // XXX shouldn't apply anymore -- unbound providers are now impossible.
	    // Would they have ever been useful?
	    //
	    //if (prov.Structure == null) {
	    //if (errors)
	    //	    Console.Error.WriteLine ("Target {0} in provider {1} references rule {2} that " +
	    //			     "is bound to structure {3}, but the provider is not " +
	    //			     "bound to a structure and so can provide no context",
	    //			     this, prov, Rule, ttmpl);
	    //return true;
	    //}

	    if (ttmpl.Equals (prov.NS.ParamsType))
		// It just depends on our containing structure. No problem.
		return false;

	    foreach (string param in prov.Structure.Parameters) {
		if (prov.Structure[param] != StructureParameterKind.Structure)
		    continue;

		if (ttmpl.Equals (prov.Structure.StructParamType (param)))
		    return false;
	    }

	    if (errors)
		// WORST ERROR MESSAGE EVAR
		Console.Error.WriteLine ("Target {0} in provider {1} references rule {2} that " +
					 "is bound to structure {3}, but the provider's containing " +
					 "structure {4} does not have an argument referencing that structure. " +
					 "You probably need to add another parameter to the containing structure.",
					 this, prov, Rule, ttmpl, prov.Structure);
	    return true;
	}

	public void EmitApply (CodeMemberMethod apply, 
			       CodeVariableReferenceExpression pb,
			       CodeVariableReferenceExpression tb,
			       CodeArgumentReferenceExpression log)
	{
	    apply.Statements.Add (new CodeCommentStatement ("Create target " + name));

	    // Set the TargetBuilder var and rule or template.

	    CodeMethodInvokeExpression cmie = new CodeMethodInvokeExpression ();
	    cmie.Method = new CodeMethodReferenceExpression (pb, "DefineTarget");
	    cmie.Parameters.Add (new CodePrimitiveExpression (name));
	    cmie.Parameters.Add (log);

	    apply.Statements.Add (new CodeAssignStatement (tb, cmie));
	    apply.Statements.Add (CDH.IfNullReturnTrue (tb));

	    Converter = delegate (string targ) {
		return prov.NS.MakeTargetNameExpr (targ, CDH.This, 
						   new CodeFieldReferenceExpression (pb, "Basis"));
	    };

	    EmitInfo (apply, tb);
	}
    }
}
