using System;
using System.Reflection;
using System.Collections.Generic;
using System.CodeDom;

using Mono.Build;
using Mono.Build.Bundling;

namespace MBBundleGen {

    public delegate CodeExpression TargetConverter (string val);

    public class EmittingInfoHolder : TargetInfoHolder<UserType,string,string,string> {

	public EmittingInfoHolder () {}

	TargetConverter conv;

	public TargetConverter Converter {
	    get { return conv; }
	    set { conv = value; }
	}

	public bool RuleAsType = true; // see EmitRuleType

	void EmitRuleType (CodeMemberMethod meth, CodeExpression tb)
	{
	    if (Rule == null)
		return;

	    CodeAssignStatement assg = new CodeAssignStatement ();

	    if (RuleAsType) {
		assg.Left = new CodePropertyReferenceExpression (tb, "RuleType");
		assg.Right = new CodeTypeOfExpression (Rule.AsCodeDom);
	    } else {
		assg.Left = new CodePropertyReferenceExpression (tb, "TemplateName");
		assg.Right = new CodePrimitiveExpression (Rule.FullName);
	    }

	    meth.Statements.Add (assg);
	}

	public CodeObjectCreateExpression ValueExpression (SingleValue<string> val)
	{
	    CodeObjectCreateExpression oce = new CodeObjectCreateExpression ();
	    oce.CreateType = new CodeTypeReference (typeof (SingleValue<string>));

	    if (val.IsResult)
		oce.Parameters.Add (CDH.ResultExpression ((Result) val));
	    else {
		if (conv == null)
		    throw new InvalidOperationException ();

		oce.Parameters.Add (conv ((string) val));
	    }

	    return oce;
	}


	void EmitAdd (CodeMemberMethod meth, CodeExpression tb, 
		      string methname, string arg, SingleValue<string> val)
	{
	    CodeMethodInvokeExpression mie = new CodeMethodInvokeExpression ();
	    mie.Method = new CodeMethodReferenceExpression (tb, methname);

	    if (arg != null)
		mie.Parameters.Add (new CodePrimitiveExpression (arg));

	    mie.Parameters.Add (ValueExpression (val));
	    meth.Statements.Add (mie);
	}

	public void EmitInfo (CodeMemberMethod meth, CodeExpression tb)
	{
	    EmitRuleType (meth, tb);

	    foreach (SingleValue<string> val in UnnamedDeps)
		EmitAdd (meth, tb, "AddDep", null, val);

	    foreach (string arg in ArgsWithDeps) {
		foreach (SingleValue<string> val in GetArgDeps (arg))
		    EmitAdd (meth, tb, "AddDep", arg, val);
	    }

	    foreach (SingleValue<string> val in DefaultOrderedDeps)
		EmitAdd (meth, tb, "AddDefaultOrdered", null, val);

	    foreach (string arg in ArgsWithDefaults)
		EmitAdd (meth, tb, "SetDefault", arg, GetArgDefault (arg));

	    foreach (string tag in TagsWithValues)
		EmitAdd (meth, tb, "AddTag", tag, GetTagValue (tag));
	}
    }
}
