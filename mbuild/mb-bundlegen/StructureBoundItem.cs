using System;
using System.Reflection;
using System.Collections.Generic;
using System.CodeDom;

using Mono.Build;
using Mono.Build.Bundling;

namespace MBBundleGen {
    
    // This is a class that has operations that may be bound to a 
    // particular structure type. For now, this is always because the
    // object references the structure's configuration parameters.

    public abstract class StructureBoundItem : TypeExpressedItem {

	public StructureBoundItem (string name, NamespaceBuilder ns, CodeLinePragma loc, 
				   TypeAttributes attr) : base (name, ns, loc, attr)
	{
	    this.ns = ns;
	}

	NamespaceBuilder ns;

	public NamespaceBuilder NS { get { return ns; } }

	public INamespaceParams Params { get { return ns.Params; } }

	// Common need - track whether we actually use any of the 
	// structure's information.

	bool uses_structure = false;

	protected void UseStructure ()
	{
	    uses_structure = true;
	}

	public bool UsesStructure { get { return uses_structure; } }

	public override bool Resolve (TypeResolveContext trc, bool errors)
	{
	    if (base.Resolve (trc, errors))
		return true;

	    UserType t = BaseClass.ResolveUsedStructureType (trc, errors);

	    if (t != null)
		// If our base class needs a structure, we need our
		// structure.
		uses_structure = true;

	    return false;
	}

	// Return an expression that yields an appropriate StructureTemplate
	// instance of type bstruct from our current context. stmpl is an expression
	// that returns an instance of the structure that we are bound to.
	// Typical usage:
	//
	//    UserType bstruct = BaseClass.ResolveUsedStructureType (trc, errors);
	//    ...
	//    ctor.BaseConstructorArgs.Add (ContextualStructRef (bstruct, stmpl));
	//
	// If bstruct is null, CDH.Null is returned -- so in the above case, we
	// will pass null to the base constructor, which is appropriate since it
	// needs no structure.

	public CodeExpression ContextualStructRef (UserType bstruct, CodeExpression stmpl)
	{
	    if (bstruct == null)
		return CDH.Null;

	    if (bstruct.Equals (NS.ParamsType))
		// Sweet! We are referring to ourselves.
		return stmpl;

	    // We need some nontrivial different structure
	    // type. Search our structure for a member that points to
	    // a structure of that type.

	    string foundparam = null;

	    foreach (string param in Params.Parameters) {
		if (Params[param] != StructureParameterKind.Structure)
		    continue;

		if (!bstruct.Equals (Params.StructParamType (param)))
		    continue;

		// FIXME: better model that handles this.
		if (foundparam != null)
		    throw ExHelp.App ("Ambiguous structure chain: structure {0} contains two parameters " +
				      "of type {1}: {2} and {3}", Params, bstruct, foundparam, param);

		foundparam = param;
	    }

	    if (foundparam == null)
		throw ExHelp.App ("Missing structure chain: structure {0} needs a parameter of type {1} " +
				  "to allow chaining of type {2} to its base class {3}", Params,
				  bstruct, this, BaseClass);

	    return new CodeFieldReferenceExpression (stmpl, foundparam);
	}

	public void EmitAttribute (CodeTypeDeclaration type)
	{
	    CodeExpression boundtype = new CodeTypeOfExpression (ns.ParamsType.AsCodeDom);

	    CodeAttributeDeclaration a = 
		new CodeAttributeDeclaration ("Mono.Build.Bundling.StructureBindingAttribute");
	    a.Arguments.Add (new CodeAttributeArgument (boundtype));
	    a.Arguments.Add (new CodeAttributeArgument (new CodePrimitiveExpression (uses_structure)));

	    type.CustomAttributes.Add (a);
	}
    }
}
