using System;
using System.Reflection;
using System.Collections.Generic;
using System.CodeDom;

using Mono.Build;
using Mono.Build.Bundling;

namespace MBBundleGen {

    public class StructureBuilder : TypeExpressedItem, INamespaceParams {

	public StructureBuilder (NamespaceBuilder ns, CodeLinePragma loc, TypeAttributes attr) 
	    : base (BundleManagerBase.DefaultStructureClass, ns, loc, attr)
	{
	    BaseClass = new UserType (typeof (StructureTemplate));

	    ns.SetUserParams (this);
	}

	// Parameter tables

	Dictionary<string,StructureParameterKind> sparams = 
	    new Dictionary<string,StructureParameterKind> ();
	Dictionary<string,UserType> structtypes = 
	    new Dictionary<string,UserType> ();
	Dictionary<string,string> defaults = 
	    new Dictionary<string,string> ();

	public void AddBasisParam (string name, string dflt)
	{
	    if (sparams.ContainsKey (name))
		throw ExHelp.App ("Redefinition of structure parameter `{0}'", name);

	    sparams[name] = StructureParameterKind.Basis;
	    defaults[name] = dflt;
	}

	public void AddTargetParam (string name, string dflt)
	{
	    if (sparams.ContainsKey (name))
		throw ExHelp.App ("Redefinition of structure parameter `{0}'", name);

	    sparams[name] = StructureParameterKind.Target;
	    defaults[name] = dflt;
	}

	public void AddStructureParam (UserType type, string name, string dflt)
	{
	    if (sparams.ContainsKey (name))
		throw ExHelp.App ("Redefinition of structure parameter `{0}'", name);

	    sparams[name] = StructureParameterKind.Structure;
	    structtypes[name] = type;
	    defaults[name] = dflt;
	}

	public IEnumerable<string> Parameters { 
	    get { return sparams.Keys; }
	}

	public bool HasParam (string name)
	{
	    return sparams.ContainsKey (name);
	}

	public StructureParameterKind this[string name]
	{
	    get {
		if (!sparams.ContainsKey (name))
		    throw ExHelp.InvalidOp ("Trying to get type of undefined parameter `{0}'", name);

		return sparams[name];
	    }
	}

	public UserType StructParamType (string name)
	{
	    if (!sparams.ContainsKey (name))
		throw ExHelp.InvalidOp ("Trying to get type of undefined parameter `{0}'", name);

	    if (sparams[name] != StructureParameterKind.Structure)
		throw ExHelp.InvalidOp ("Trying to get type of non-structure parameter `{0}'", name);

	    return structtypes[name];
	}

	// Elements

	List<StructureElement> elts = new List<StructureElement> ();

	public void AddElement (StructureElement elt)
	{
	    elts.Add (elt);
	}

	public override bool Resolve (TypeResolveContext trc, bool errors)
	{
	    bool res = base.Resolve (trc, errors);

	    foreach (StructureElement elt in elts)
		res |= elt.Resolve (trc, errors);
	    foreach (UserType ut in structtypes.Values)
		res |= ut.Resolve (trc, errors);

	    return res;
	}

	// Emission

	readonly static CodeTypeReference ProjBuilder = 
	    new CodeTypeReference (typeof (ProjectBuilder));
	readonly static CodeTypeReference ProvBuilder = 
	    new CodeTypeReference (typeof (ProviderBuilder));
	readonly static CodeTypeReference TargBuilder = 
	    new CodeTypeReference (typeof (TargetBuilder));

	protected override void Emit (CodeTypeDeclaration ctd)
	{
	    EmitEmptyCtor (ctd);

	    // Parameter fields

	    foreach (string name in sparams.Keys)
		EmitParameter (ctd, name);

	    // ApplyDefaults

	    EmitApplyDefaults (ctd);

	    // Apply prologue

	    CodeParameterDeclarationExpression p1 =
		new CodeParameterDeclarationExpression (ProjBuilder, "proj");
	    CodeParameterDeclarationExpression p2 =
		new CodeParameterDeclarationExpression (CDH.String, "declloc");
	    CodeParameterDeclarationExpression p3 =
		new CodeParameterDeclarationExpression (CDH.ILog, "log");

	    CodeMemberMethod apply = new CodeMemberMethod ();
	    apply.Name = "Apply";
	    apply.Attributes = MemberAttributes.Public | MemberAttributes.Override;
	    apply.ReturnType = CDH.Bool;
	    apply.Parameters.Add (p1);
	    apply.Parameters.Add (p2);
	    apply.Parameters.Add (p3);

	    CDH.EmitBaseChainBool (apply);

	    // Elements

	    if (elts.Count > 0) {
		apply.Statements.Add (CDH.Variable (ProvBuilder, "pb"));
		apply.Statements.Add (CDH.Variable (TargBuilder, "tb"));

		CodeArgumentReferenceExpression proj = CDH.ARef ("proj");
		CodeArgumentReferenceExpression declloc = CDH.ARef ("declloc");
		CodeArgumentReferenceExpression log = CDH.ARef ("log");
		CodeVariableReferenceExpression pb = CDH.VRef ("pb");
		CodeVariableReferenceExpression tb = CDH.VRef ("tb");
		
		foreach (StructureElement se in elts)
		    se.EmitApply (apply, proj, declloc, log, pb, tb);
	    }

	    // Apply epilogue

	    apply.Statements.Add (new CodeMethodReturnStatement (CDH.False));
	    ctd.Members.Add (apply);
	}

	// Emitting ApplyDefaults

	readonly static CodeTypeReference BMBase = 
	    new CodeTypeReference (typeof (BundleManagerBase));

	void EmitApplyDefaults (CodeTypeDeclaration ctd)
	{
	    CodeMemberMethod meth = new CodeMemberMethod ();
	    meth.Name = "ApplyDefaults";
	    meth.Attributes = MemberAttributes.Public | MemberAttributes.Override;
	    meth.ReturnType = CDH.Bool;
	    meth.Parameters.Add (CDH.Param (BMBase, "bmb"));
	    meth.Parameters.Add (CDH.Param (CDH.ILog, "log"));

	    CodeArgumentReferenceExpression bmb = CDH.ARef ("bmb");
	    CodeArgumentReferenceExpression log = CDH.ARef ("log");

	    // Set the parameters

	    foreach (string param in sparams.Keys) {
		StructureParameterKind kind = sparams[param];
		string val = defaults[param];

		CodeAssignStatement assg = new CodeAssignStatement ();
		assg.Left = CDH.ThisDot (param);

		switch (kind) {
		case StructureParameterKind.Basis:
		    if (val[val.Length - 1] != '/')
			// Canonicalize basis names.
			val += '/';
		    goto case StructureParameterKind.Target;
		case StructureParameterKind.Target:
		    assg.Right = new CodePrimitiveExpression (val);
		    break;
		case StructureParameterKind.Structure:
		    CodeMethodInvokeExpression mie = new CodeMethodInvokeExpression ();
		    mie.Method = new CodeMethodReferenceExpression (bmb, "GetNamespaceTemplate");
		    mie.Parameters.Add (new CodePrimitiveExpression (val));
		    mie.Parameters.Add (log);
		    UserType stype = structtypes[param];
		    assg.Right = new CodeCastExpression (stype.AsCodeDom, mie);
		    break;
		}

		meth.Statements.Add (assg);

		if (kind == StructureParameterKind.Structure)
		    meth.Statements.Add (CDH.IfNullReturnTrue (assg.Left));
	    }
								       
	    // All done.

	    meth.Statements.Add (new CodeMethodReturnStatement (CDH.False));

	    ctd.Members.Add (meth);
	}

	// Emitting parameters

	readonly static CodeTypeReferenceExpression SPKind = 
	    new CodeTypeReferenceExpression (typeof (StructureParameterKind));

	void EmitParameter (CodeTypeDeclaration ctd, string name)
	{
	    StructureParameterKind type = sparams[name];
	    
	    CodeTypeReference fieldtype = null;
	    string field = null;

	    switch (type) {
	    case StructureParameterKind.Basis:
		fieldtype = CDH.String;
		field = "Basis";
		break;
	    case StructureParameterKind.Target:
		fieldtype = CDH.String;
		field = "Target";
		break;
	    case StructureParameterKind.Structure:
		fieldtype = structtypes[name].AsCodeDom;
		field = "Structure";
		break;
	    }

	    CodeExpression val = 
		new CodeFieldReferenceExpression (SPKind, field);
	    CodeAttributeDeclaration attr = 
		new CodeAttributeDeclaration ("Mono.Build.Bundling.StructureParameterAttribute");
	    attr.Arguments.Add (new CodeAttributeArgument (val));

	    CodeMemberField f = new CodeMemberField (fieldtype, name);
	    f.Attributes = MemberAttributes.Public;
	    f.CustomAttributes.Add (attr);
	    ctd.Members.Add (f);
	}
    }
}
