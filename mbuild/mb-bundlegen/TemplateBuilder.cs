using System;
using System.Reflection;
using System.Collections.Generic;
using System.CodeDom;

using Mono.Build;
using Mono.Build.Bundling;

namespace MBBundleGen {

    public class TemplateBuilder : StructureBoundItem {

	EmittingInfoHolder info = new EmittingInfoHolder ();
	    
	// A TemplateBuilder may never actually reference a structure.
	// In that case, UseStructure () isn't called and UsesStructure
	// is therefore false.

	public TemplateBuilder (string name, NamespaceBuilder ns, CodeLinePragma loc,
				TypeAttributes attr) : base (name, ns, loc, attr)
	{
	    BaseClass = TTType;
	}

	readonly static UserType TTType = new UserType (typeof (TargetTemplate));

	// If we need to access our owning structure, record the fact.

	void CheckStructRel (SingleValue<string> val)
	{
	    if (val.IsTarget)
		UseStructure ();
	}

	// Bridge to the EmittingInfoHolder

	public UserType RuleType {
	    get { return info.Rule; }
	    set { info.Rule = value; }
	}

	public void AddDep (SingleValue<string> val)
	{
	    info.AddDep (val);
	    CheckStructRel (val);
	}

	public void AddDep (string arg, SingleValue<string> val)
	{
	    info.AddDep (arg, val);
	    CheckStructRel (val);
	}
	
	public void AddDefault (string arg, SingleValue<string> val)
	{
	    info.SetDefault (arg, val);
	    CheckStructRel (val);
	}
	
	public void AddTag (string tag, SingleValue<string> val)
	{
	    info.AddTag (tag, val);
	    CheckStructRel (val);
	}

	// Emit
	// Constructor -- can be fun if we need to chain to base

	UserType basestruct = null;

	public override bool Resolve (TypeResolveContext trc, bool errors)
	{
	    if (base.Resolve (trc, errors))
		return true;

	    if (RuleType != null)
		if (RuleType.Resolve (trc, errors))
		    return true;

	    if (BaseClass.Equals (TTType))
		return false;
	    
	    basestruct = BaseClass.ResolveUsedStructureType (trc, errors);
	    return false;
	}

	CodeConstructor EmitConstructor (CodeTypeDeclaration ctd)
	{
	    // The ctor that inits the field and chains to the parent if necc.

	    CodeConstructor ctor = EmitEmptyCtor (ctd);
	    CodeArgumentReferenceExpression stmpl = CDH.ARef ("stmpl");

	    ctor.Parameters.Add (CDH.Param (NS.ParamsType, "stmpl"));

	    if (UsesStructure) {
		CodeAssignStatement assg = new CodeAssignStatement ();
		assg.Left = CDH.ThisDot ("stmpl");
		assg.Right = stmpl;
		ctor.Statements.Add (assg);
	    }
	    
	    ctor.BaseConstructorArgs.Add (ContextualStructRef (basestruct, stmpl));
	    return ctor;
	}

	protected CodeConstructor EmitTemplate (CodeTypeDeclaration ctd)
	{
	    EmitAttribute (ctd);

	    // A(n optional) field pointing to our bound structure

	    if (UsesStructure) {
		CodeMemberField f = new CodeMemberField ();
		f.Name = "stmpl";
		f.Attributes = MemberAttributes.Private;
		f.Type = NS.ParamsType.AsCodeDom;
		ctd.Members.Add (f);
	    }

	    // The template apply method.

	    CodeMemberMethod meth = new CodeMemberMethod ();
	    meth.Name = "ApplyTemplate";
	    meth.Attributes = MemberAttributes.Public | MemberAttributes.Override;
	    meth.ReturnType = null;
	    meth.Parameters.Add (CDH.Param (typeof (TargetBuilder), "tb"));

	    CDH.EmitBaseChainVoid (meth);

	    CodeVariableReferenceExpression tb = CDH.VRef ("tb");

	    if (UsesStructure) {
		info.Converter = delegate (string val) {
		    return NS.MakeTargetNameExpr (val, CDH.ThisDot ("stmpl"), null);
		};
	    }

	    info.EmitInfo (meth, tb);
	    ctd.Members.Add (meth);

	    // Constructor

	    return EmitConstructor (ctd);
	}

	protected override void Emit (CodeTypeDeclaration ctd)
	{
	    EmitTemplate (ctd);
	}
    }
}
