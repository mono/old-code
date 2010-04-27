using System;
using System.Reflection;
using System.Collections.Generic;
using System.CodeDom;

using Mono.Build;

namespace MBBundleGen {

    //public static class CodeDomHelpers {
    public static class CDH {

	public readonly static CodeTypeReference String = 
	    new CodeTypeReference (typeof (string));
	public readonly static CodeTypeReference Byte = 
	    new CodeTypeReference (typeof (byte));
	public readonly static CodeTypeReference Bool = 
	    new CodeTypeReference (typeof (bool));
	public readonly static CodeTypeReference Type = 
	    new CodeTypeReference (typeof (Type));

	public readonly static CodeTypeReference Result = 
	    new CodeTypeReference ("Mono.Build.Result");
	public readonly static CodeTypeReference Rule = 
	    new CodeTypeReference ("Mono.Build.Rule");
	public readonly static CodeTypeReference Fingerprint = 
	    new CodeTypeReference ("Mono.Build.Fingerprint");
	public readonly static CodeTypeReference IContext = 
	    new CodeTypeReference ("Mono.Build.IBuildContext");
	public readonly static CodeTypeReference ILog = 
	    new CodeTypeReference ("Mono.Build.IWarningLogger");
	//public readonly static CodeTypeReference MBBool = 
	//    new CodeTypeReference ("Mono.Build.MBBool");
	//public readonly static CodeTypeReference MBString = 
	//    new CodeTypeReference ("Mono.Build.MBString");
	
	public readonly static CodeExpression This = 
	    new CodeThisReferenceExpression ();
	public readonly static CodeExpression Base = 
	    new CodeBaseReferenceExpression ();
	public readonly static CodeExpression Null = 
	    new CodePrimitiveExpression (null);	
	public readonly static CodeExpression True = 
	    new CodePrimitiveExpression (true);	
	public readonly static CodeExpression False = 
	    new CodePrimitiveExpression (false);	

	public static CodeParameterDeclarationExpression Param (CodeTypeReference type, string name)
	{
	    return new CodeParameterDeclarationExpression (type, name);
	}

	public static CodeParameterDeclarationExpression Param (UserType type, string name)
	{
	    return Param (type.AsCodeDom, name);
	}

	public static CodeParameterDeclarationExpression Param (Type type, string name)
	{
	    return Param (new CodeTypeReference (type), name);
	}

	public static CodeVariableDeclarationStatement Variable (CodeTypeReference type, string name)
	{
	    return new CodeVariableDeclarationStatement (type, name);
	}

	public static CodeVariableReferenceExpression VRef (string name)
	{
	    return new CodeVariableReferenceExpression (name);
	}

	public static CodeArgumentReferenceExpression ARef (string name)
	{
	    return new CodeArgumentReferenceExpression (name);
	}

	public static CodeConditionStatement IfTrueReturnNull (CodeExpression condition)
	{
	    CodeConditionStatement cond = new CodeConditionStatement ();
	    cond.Condition = condition;
	    cond.TrueStatements.Add (new CodeMethodReturnStatement (Null));
	    return cond;
	}

	public static CodeConditionStatement IfTrueReturnTrue (CodeExpression condition)
	{
	    CodeConditionStatement cond = new CodeConditionStatement ();
	    cond.Condition = condition;
	    cond.TrueStatements.Add (new CodeMethodReturnStatement (True));
	    return cond;
	}

	public static CodeConditionStatement IfNullReturnTrue (CodeExpression val)
	{
	    CodeBinaryOperatorExpression cmp = new CodeBinaryOperatorExpression ();
	    cmp.Left = val;
	    cmp.Right = Null;
	    cmp.Operator = CodeBinaryOperatorType.IdentityEquality;

	    return IfTrueReturnTrue (cmp);
	}

	public static CodeFieldReferenceExpression ThisDot (string name)
	{
	    return new CodeFieldReferenceExpression (This, name);
	}

	public static CodeMethodInvokeExpression AddBareInvoke (CodeMemberMethod meth, 
								CodeExpression obj, string method)
	{
	    CodeMethodInvokeExpression mie = new CodeMethodInvokeExpression ();
	    mie.Method = new CodeMethodReferenceExpression (obj, method);

	    meth.Statements.Add (mie);

	    return mie;
	}

	public static CodeMethodInvokeExpression EmitBaseChainVoid (CodeMemberMethod meth)
	{
	    CodeMethodInvokeExpression mie = new CodeMethodInvokeExpression ();
	    mie.Method = new CodeMethodReferenceExpression (Base, meth.Name);

	    foreach (CodeParameterDeclarationExpression pde in meth.Parameters)
		mie.Parameters.Add (ARef (pde.Name));

	    meth.Statements.Add (mie);

	    return mie;
	}

	public static CodeMethodInvokeExpression EmitBaseChainBool (CodeMemberMethod meth)
	{
	    CodeMethodInvokeExpression mie = new CodeMethodInvokeExpression ();
	    mie.Method = new CodeMethodReferenceExpression (Base, meth.Name);

	    foreach (CodeParameterDeclarationExpression pde in meth.Parameters)
		mie.Parameters.Add (ARef (pde.Name));

	    meth.Statements.Add (IfTrueReturnTrue (mie));

	    return mie;
	}

	public static CodeExpression ResultExpression (Mono.Build.Result r)
	{
	    if (r == null)
		throw new ArgumentNullException ();

	    CodeObjectCreateExpression oce = new CodeObjectCreateExpression ();

	    if (r is MBString) {
		oce.CreateType = new CodeTypeReference (typeof (MBString));
		oce.Parameters.Add (new CodePrimitiveExpression (((MBString) r).Value));
	    } else if (r is MBBool) {
		oce.CreateType = new CodeTypeReference (typeof (MBBool));
		oce.Parameters.Add (new CodePrimitiveExpression (((MBBool) r).Value));
	    } else
		throw ExHelp.InvalidOp ("Don't know how to emit literal result {0}?", r);

	    return oce;
	}
    }
}
