using System;
using System.Reflection;
using System.Collections.Generic;
using System.CodeDom;

using Mono.Build;
using Mono.Build.Bundling;

namespace MBBundleGen {

    public class NamespaceBuilder {

	string sname;
	TypeResolveContext trc;
	UserType paramstype;
	INamespaceParams nsparams;

	public NamespaceBuilder (string sname, TypeResolveContext ctxt)
	{
	    if (sname == null)
		throw new ArgumentNullException ("sname");
	    if (ctxt == null)
		throw new ArgumentNullException ("ctxt");

	    this.sname = sname;

	    trc = ctxt;
	    trc.SetNamespace (RealName);

	    Type t = ctxt.Driver.LookupExistingFQN (StructureName);

	    if (t != null) {
		paramstype = new UserType (t);
		nsparams = new ExistingNamespaceParams (t);
	    } else {
		paramstype = new UserType (StructureName);
		// will be defined either by parameters {} or at end
		// of parsing the namespace.
		// FIXME: if we have two separate namespace {} sections,
		// I think they will both make their own StructureBuilder
		// instance, and those will clash.
		nsparams = null; 
	    }
	}

	public string ShortName { get { return sname; } }

	public string RealName { get { return "MBuildDynamic." + sname; } }

	public string StructureName { 
	    get { 
		return RealName + "." + BundleManagerBase.DefaultStructureClass; 
	    } 
	}

	public static string MakeStructureName (string sname)
	{
	    return "MBuildDynamic." + sname + "." + 
		BundleManagerBase.DefaultStructureClass;
	}

	public UserType ParamsType { get { return paramstype; } }

	public INamespaceParams Params { get { return nsparams; } }

	public void SetUserParams (StructureBuilder sb)
	{
	    if (nsparams != null)
		throw new Exception ("Trying to define a second set of parameters " +
				     "for namespace " + sname);

	    nsparams = sb;
	}

	// Items

	List<TypeExpressedItem> items = new List<TypeExpressedItem> ();

	public IEnumerable<TypeExpressedItem> Items { get { return items; } }

	public void AddItem (TypeExpressedItem item)
	{
	    items.Add (item);
	}

	List<MetaRuleBuilder> metarules = new List<MetaRuleBuilder> ();

	public void AddMetaRule (MetaRuleBuilder meta)
	{
	    metarules.Add (meta);
	}

	// Usings

	public void AddUsing (string s)
	{
	    trc.AddUsing (s);
	}

	// The rest

	public bool Resolve (bool errors)
	{
	    bool ret = paramstype.Resolve (trc, errors);

	    foreach (MetaRuleBuilder mrb in metarules)
		ret |= mrb.Resolve (trc, errors);
	    foreach (TypeExpressedItem tei in items)
		ret |= tei.Resolve (trc, errors);

	    return ret;
	}
	
	public CodeNamespace Emit ()
	{
	    CodeNamespace ns = new CodeNamespace (RealName);

	    trc.ApplyToNamespace (ns);

	    foreach (TypeExpressedItem tei in items)
		ns.Types.Add (tei.Emit ());

	    return ns;
	}

	// Helper: converts a structure-bound target reference to a CodeExpression.
	// A "structure-bound target reference" is one of the following:
	//
	//    * plain: [target basename] (valid only if 'basis' is nonnull)
	//    * relative to a basis param: [param]/[target basename]
	//    * relative to a structure param: [param].[structure's basis param]/[target basename]
	//
	// (We could add: [structure param].[structure param]....[basis param]/[target basename])
	//
	// 'strukt' is an expression which, in the context of the whatever will evaluate
	// our return value, yields the StructureTemplate object whose parameters 
	// will be used to evaluate bases.
	//
	// 'basis' is an optional expression which yields a string representing
	// the "current" basis, if that is meaningful. If not, pass null.

	public CodeExpression MakeTargetNameExpr (string name, CodeExpression strukt, 
						  CodeExpression basis)
	{
	    int idx = name.IndexOf ('/');

	    CodeBinaryOperatorExpression e = new CodeBinaryOperatorExpression ();
	    e.Operator = CodeBinaryOperatorType.Add;

	    if (idx < 0) {
		if (basis == null)
		    throw new Exception ("Can't have bare target name in object without an " +
					 "associated basis.");

		e.Left = basis;
		e.Right = new CodePrimitiveExpression (name);
	    } else {
		string param = name.Substring (0, idx);
		string basename = name.Substring (idx + 1);

		if (basename.IndexOf ('/') != -1)
		    throw new Exception ("Target references must be relative to a " +
					 "parameter; can have at most one / in the text");
		
		int jdx = param.IndexOf ('.');
		
		e.Right = new CodePrimitiveExpression (basename);
		
		if (jdx < 0) {
		    if (!nsparams.HasParam (param))
			throw ExHelp.App ("Relative target name references unknown " +
					  "structure parameter {0}.", param);
		    
		    if (nsparams[param] == StructureParameterKind.Structure)
			throw ExHelp.App ("Target prefix parmeter {0} should be a `basis' but is a " +
					  "`structure'. Did you forget to add a member access to the structure?", param);
		    
		    if (nsparams[param] != StructureParameterKind.Basis)
			throw ExHelp.App ("Target prefix argument {0} must be of type `basis'.", param);
		    
		    e.Left = new CodeFieldReferenceExpression (strukt, param);
		} else {
		    // Member of a Structure parameter
		    
		    string member = param.Substring (jdx + 1);
		    param = param.Substring (0, jdx);
		    
		    if (!nsparams.HasParam (param))
			throw ExHelp.App ("Relative target name references unknown " +
					  "structure parameter {0}", param);
		    
		    if (nsparams[param] != StructureParameterKind.Structure)
			throw ExHelp.App ("Member access parameter must be of type `structure'.");
		
		    CodeExpression outer = 
			new CodeFieldReferenceExpression (strukt, param);
		    e.Left = new CodeFieldReferenceExpression (outer, member);
		}
	    }
	
	    return e;
	}


    }
}

