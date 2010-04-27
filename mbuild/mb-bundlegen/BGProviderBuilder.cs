using System;
using System.Reflection;
using System.Collections.Generic;
using System.CodeDom;

using Mono.Build;
using Mono.Build.Bundling;

namespace MBBundleGen {

    // The classname here lets us distinguish between
    // a bundlegen ProviderBuilder and the Mono.Build
    // ProviderBuilder class, which a BGProviderBuilder
    // fills out in an Apply() method.

    public class BGProviderBuilder : StructureElement {

	string basisparam;
	
	public BGProviderBuilder (string name, NamespaceBuilder ns) : base (ns)
	{
	    if (Structure[name] != StructureParameterKind.Basis)
		throw ExHelp.App ("No such basis parameter {0} for provider", name);

	    basisparam = name;
	}

	List<BGTargetBuilder> targs = new List<BGTargetBuilder> ();

	public void AddTarget (BGTargetBuilder tb)
	{
	    targs.Add (tb);
	}

	public override bool Resolve (TypeResolveContext trc, bool errors)
	{
	    bool ret = false;

	    foreach (BGTargetBuilder tb in targs)
		ret |= tb.Resolve (trc, errors);

	    return ret;
	}

	Dictionary<string,Result> lits = new Dictionary<string,Result> ();

	public void AddLiteralTarget (string name, Result res)
	{
	    // FIXME: generalize this to not-necessarily-literal targets too.
	    if (lits.ContainsKey (name))
		throw ExHelp.App ("Redefining literal target {0}?", name);

	    lits[name] = res;
	}

      	public override void EmitApply (CodeMemberMethod apply, CodeArgumentReferenceExpression proj,
					CodeArgumentReferenceExpression declloc, 
					CodeArgumentReferenceExpression log, 
					CodeVariableReferenceExpression pb,
					CodeVariableReferenceExpression tb)
	{
	    apply.Statements.Add (new CodeCommentStatement ("Create provider for param " +
							    basisparam));

	    CodeMethodInvokeExpression mie = new CodeMethodInvokeExpression ();
	    mie.Method = new CodeMethodReferenceExpression (proj, "EnsureProvider");
	    mie.Parameters.Add (new CodeFieldReferenceExpression (CDH.This, basisparam));
	    mie.Parameters.Add (declloc);

	    apply.Statements.Add (new CodeAssignStatement (pb, mie));

	    // Tell the PB about our Structure arguments so it can
	    // use them to instantiate the objects we reference.
	    // FIXME: catch if the provider references a rule or template 
	    // that requires a structure we don't have.

	    //if (Structure != null) {
	    mie = new CodeMethodInvokeExpression ();
	    mie.Method = new CodeMethodReferenceExpression (pb, "AddContextStructure");
	    mie.Parameters.Add (CDH.This);
	    apply.Statements.Add (mie);

	    foreach (string param in Structure.Parameters) {
		if (Structure[param] != StructureParameterKind.Structure)
		    continue;
		
		mie = new CodeMethodInvokeExpression ();
		mie.Method = new CodeMethodReferenceExpression (pb, "AddContextStructure");
		mie.Parameters.Add (new CodeFieldReferenceExpression (CDH.This, param));
		apply.Statements.Add (mie);
	    }
	    //}

	    foreach (BGTargetBuilder iter in targs)
		iter.EmitApply (apply, pb, tb, log);

	    foreach (string key in lits.Keys) {
		CodeExpression val = CDH.ResultExpression (lits[key]);

		CodeMethodInvokeExpression cmie = new CodeMethodInvokeExpression ();
		cmie.Method = new CodeMethodReferenceExpression (pb, "DefineConstantTarget");
		cmie.Parameters.Add (new CodePrimitiveExpression (key));
		cmie.Parameters.Add (val);
		cmie.Parameters.Add (log);

		apply.Statements.Add (CDH.IfTrueReturnTrue (cmie));
	    }
	}
	
	public override string ToString ()
	{
	    return String.Format ("{0}/{1}", Structure, basisparam);
	}
    }
}
