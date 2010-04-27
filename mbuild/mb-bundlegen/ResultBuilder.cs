using System;
using System.Reflection;
using System.Collections.Generic;
using System.CodeDom;

using Mono.Build;

namespace MBBundleGen {

    public class ResultBuilder : TypeExpressedItem {

	// FIXME: Resurrect the 'implementation code' idea?
	// It was kind of useful I think.

	public ResultBuilder (string name, NamespaceBuilder ns, CodeLinePragma loc, TypeAttributes attr) : 
	    base (name, ns, loc, attr)
	{
	    BaseClass = new UserType (typeof (Result));
	}

	// Composite-ness

	static CodeVariableDeclarationStatement BaseTotalItems;

	static ResultBuilder ()
	{
	    // @{int base_total = base.NumFielduments;}

	    BaseTotalItems = new CodeVariableDeclarationStatement ();
	    BaseTotalItems.Name = "base_total";
	    BaseTotalItems.Type = new CodeTypeReference (typeof (int));
	    BaseTotalItems.InitExpression = 
		new CodePropertyReferenceExpression (CDH.Base, "TotalItems");
	}

	static CodeExpression BasePlusN (int n)
	{
	    CodeBinaryOperatorExpression idexpr = new CodeBinaryOperatorExpression ();
	    idexpr.Left = new CodeVariableReferenceExpression ("base_total");
	    idexpr.Operator = CodeBinaryOperatorType.Add;
	    idexpr.Right = new CodePrimitiveExpression (n);
	    return idexpr;
	}

	class FieldInfo {
	    public readonly int Idx;
	    public readonly UserType Type;
	    public readonly string Name;
	    public readonly CodeLinePragma Line;

	    public FieldInfo (int idx, UserType type, string name, CodeLinePragma line)
	    {
		Idx = idx;
		Type = type;
		Name = name;
		Line = line;
	    }

	    readonly static CodeAttributeDeclaration FieldAttr = 
		new CodeAttributeDeclaration ("Mono.Build.CompositeResultFieldAttribute");


	    public void Emit (CodeTypeDeclaration ctd, CodeMemberMethod copy, CodeExpression copyarray, 
			      CodeMemberMethod clone, CodeExpression other)
	    {
		// The field itself

		CodeMemberField f = new CodeMemberField ();
		f.Name = Name;
		f.Attributes = MemberAttributes.Public;
		f.LinePragma = Line;
		f.Type = Type.AsCodeDom;
		f.CustomAttributes.Add (FieldAttr);
		ctd.Members.Add (f);

		// Copy statement in CopyItems

		CodeIndexerExpression index = new CodeIndexerExpression ();
		index.TargetObject = copyarray;
		index.Indices.Add (ResultBuilder.BasePlusN (Idx));
	    
		CodeFieldReferenceExpression fld = CDH.ThisDot (Name);
		CodeMethodInvokeExpression conv = 
		    new CodeMethodInvokeExpression (CDH.This, "FieldAsResult", 
						    new CodeExpression[] { fld });
		CodeAssignStatement assg = 
		    new CodeAssignStatement (index, conv);

		copy.Statements.Add (assg);
	    
		// Add statement to clone result in CloneItems

		CodeFieldReferenceExpression lhs = 
		    new CodeFieldReferenceExpression (other, Name);
		conv = new CodeMethodInvokeExpression (CDH.This, "CloneField", 
						       new CodeExpression[] { fld });
		assg = new CodeAssignStatement (lhs, new CodeCastExpression (f.Type, conv));
		clone.Statements.Add (assg);
	    }

	}

	int comp_default_idx = -1;
	List<FieldInfo> fields = new List<FieldInfo> ();

	public void AddCompositeField (bool is_default, UserType type, string name, CodeLinePragma line)
	{
	    if (is_default) {
		if (comp_default_idx >= 0)
		    throw new Exception ("Can't have more than one default field in a composite result!");
		comp_default_idx = fields.Count;
	    }

	    fields.Add (new FieldInfo (fields.Count, type, name, line));
	}

	void EmitDefault (CodeTypeDeclaration ctd) 
	{
	    // Override the HasDefault property
	    
	    CodeMemberProperty p = new CodeMemberProperty ();
	    p.Name = "HasDefault";
	    p.Attributes = MemberAttributes.Public | MemberAttributes.Override;
	    p.Type = new CodeTypeReference (typeof (bool));
	    p.HasGet = true;
	    p.HasSet = false;
	    p.GetStatements.Add (new CodeMethodReturnStatement (CDH.True));
	    
	    ctd.Members.Add (p);
	    
	    // Override the Default property
	    
	    p = new CodeMemberProperty ();
	    p.Name = "Default";
	    p.Attributes = MemberAttributes.Public | MemberAttributes.Override;
	    p.Type = CDH.Result;
	    p.HasGet = true;
	    p.HasSet = false;
	    CodeExpression e = CDH.ThisDot (fields[comp_default_idx].Name);
	    p.GetStatements.Add (new CodeMethodReturnStatement (e));
	    
	    ctd.Members.Add (p);    
	}

	void EmitAsComposite (CodeTypeDeclaration ctd)
	{
	    if (fields.Count == 0)
		return;

	    if (comp_default_idx >= 0)
		EmitDefault (ctd);

	    // Prologues - TotalItems field

	    CodeMemberProperty ti = new CodeMemberProperty ();
	    ti.Name = "TotalItems";
	    ti.HasGet = true;
	    ti.HasSet = false;
	    ti.Type = new CodeTypeReference (typeof (int));
	    ti.Attributes = MemberAttributes.Family | MemberAttributes.Override;
	    
	    CodeBinaryOperatorExpression add = new CodeBinaryOperatorExpression ();
	    add.Left = new CodePropertyReferenceExpression (CDH.Base, "TotalItems");
	    add.Operator = CodeBinaryOperatorType.Add;
	    add.Right = new CodePrimitiveExpression (fields.Count);
	    
	    CodeStatement ret = new CodeMethodReturnStatement (add);
	    ti.GetStatements.Add (ret);
	    
	    ctd.Members.Add (ti);

	    // Prologues - CopyItems

	    CodeParameterDeclarationExpression p = CDH.Param (new CodeTypeReference (CDH.Result, 1), "r");
	    
	    CodeMemberMethod copy = new CodeMemberMethod ();
	    copy.Name = "CopyItems";
	    copy.ReturnType = null;
	    copy.Parameters.Add (p);
	    copy.Attributes = MemberAttributes.Family | MemberAttributes.Override;
	    
	    CDH.EmitBaseChainVoid (copy);

	    CodeExpression copyarray = new CodeArgumentReferenceExpression (p.Name);
	    copy.Statements.Add (BaseTotalItems);

	    // Prologues - CloneTo method
	    
	    p = CDH.Param (CDH.Result, "r");
	    
	    CodeMemberMethod clone = new CodeMemberMethod ();
	    clone.Name = "CloneTo";
	    clone.ReturnType = null;
	    clone.Parameters.Add (p);
	    clone.Attributes = MemberAttributes.Family | MemberAttributes.Override;
	    
	    CDH.EmitBaseChainVoid (clone);

	    CodeExpression cp = CDH.ARef (p.Name);

	    CodeVariableDeclarationStatement vds = new CodeVariableDeclarationStatement ();
	    vds.Name = "other";
	    vds.Type = new CodeTypeReference (ctd.Name);
	    vds.InitExpression = new CodeCastExpression (vds.Type, cp);
	    
	    clone.Statements.Add (vds);
	    CodeExpression other = CDH.VRef (vds.Name);
	    
	    // Now per-field statements

	    foreach (FieldInfo fi in fields)
		fi.Emit (ctd, copy, copyarray, clone, other);

	    // no epilogues.

	    ctd.Members.Add (copy);
	    ctd.Members.Add (clone);
	}

	// General

	public override bool Resolve (TypeResolveContext trc, bool errors)
	{
	    bool ret = base.Resolve (trc, errors);

	    foreach (FieldInfo fi in fields)
		ret |= fi.Type.Resolve (trc, errors);

	    return ret;
	}

	protected override void Emit (CodeTypeDeclaration ctd)
	{
	    EmitEmptyCtor (ctd);

	    CodeAttributeDeclaration attr = new CodeAttributeDeclaration ("System.Serializable");
	    ctd.CustomAttributes.Add (attr);

	    EmitAsComposite (ctd);
	}
    }
}
