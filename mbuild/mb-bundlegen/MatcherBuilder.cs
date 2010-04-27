using System;
using System.Reflection;
using System.Collections.Generic;
using System.CodeDom;

using Mono.Build;
using Mono.Build.Bundling;
using Mono.Build.RuleLib;

namespace MBBundleGen {

    public class MatcherBuilder : StructureBoundItem {

	static int matcher_serial = 0;
	int serial;

	public MatcherBuilder (NamespaceBuilder ns, CodeLinePragma loc) : 
	    base ("RegexMatcher", ns, loc, TypeAttributes.Public)
	{
	    BaseClass = new UserType (typeof (RegexMatcher));
	    serial = matcher_serial++;
	}

	protected override string GetClassName (string namebase)
	{
	    return namebase + serial;
	}

	public string Regex;
	public MatcherKind Kind;
	public UserType MatchType;

	public override bool Resolve (TypeResolveContext trc, bool errors)
	{
	    bool ret = base.Resolve (trc, errors);
	    ret |= MatchType.ResolveExtension ("RTemplate", trc, errors);

	    // RegexMatcher will handle distinguishing between when
	    // we return a Rule type and a TargetTemplate type, but we need
	    // to check if we require a binding.

	    bool? foo = MatchType.ResolvesAsRule (trc, errors);

	    if (foo == null)
		// MatchType unresolved, can't say.
		return true;

	    if ((bool) foo) {
		// We point to a Rule, and ResolveUsedStructureType will
		// complain if we call it on MatchType.

		if (UsesStructure)
		    throw ExHelp.App ("Odd, MatcherBuilder {0} is using its structure" +
				      "even though it points to a plain rule {1}", this,
				      MatchType);

		return false;
	    }

	    // We have a template, so see if it uses a structure

	    UserType stype = MatchType.ResolveUsedStructureType (trc, errors);

	    if (errors) {
		if (stype == null && UsesStructure) {
		    // XXX is this actually an error? Shouldn't happen, I think.
		    Console.Error.WriteLine ("Matcher {0} is bound to structure {1} but its " +
					     "associated template {2} is unbound", this, 
					     Params, MatchType);
		    return true;
		} else if (stype != null && !UsesStructure) {
		    UseStructure ();
		} else if (stype != null && !NS.ParamsType.Equals (stype)) {
		    // FIXME: see if Structure has a member of type stype.
		    Console.Error.WriteLine ("Matcher {0} is bound to the structure {1}  but its " +
					     "associated template {2} is bound to {3}", this, 
					     Params, MatchType, stype);
		    return true;
		}
	    }

	    return ret;
	}

	CodeExpression KindExpression {
	    get {
		CodeFieldReferenceExpression cfre = new CodeFieldReferenceExpression ();
		cfre.TargetObject = new CodeTypeReferenceExpression (typeof (MatcherKind));
		cfre.FieldName = Enum.GetName (typeof (MatcherKind), Kind);
		return cfre;
	    }
	}

	protected override void Emit (CodeTypeDeclaration ctd)
	{
	    EmitAttribute (ctd);

	    // Constructor

	    CodeConstructor ctor = EmitEmptyCtor (ctd);

	    ctor.Parameters.Add (CDH.Param (NS.ParamsType, "stmpl"));
	    ctor.BaseConstructorArgs.Add (CDH.ARef ("stmpl"));

	    // Matcher attr
		
	    CodeAttributeDeclaration kattr = 
		new CodeAttributeDeclaration ("Mono.Build.Bundling.MatcherAttribute");
	    kattr.Arguments.Add (new CodeAttributeArgument (KindExpression));
	    ctd.CustomAttributes.Add (kattr);

	    // GetRegex override

	    CodeMemberMethod method = new CodeMemberMethod ();
	    method.Name = "GetRegex";
	    //method.Attributes = MemberAttributes.Family | MemberAttributes.Override;
	    method.Attributes = MemberAttributes.Public | MemberAttributes.Override;
	    method.ReturnType = CDH.String;
	    method.LinePragma = Location;

	    CodeExpression val = new CodePrimitiveExpression (Regex);
	    CodeStatement ret = new CodeMethodReturnStatement (val);
	    ret.LinePragma = Location;
	    method.Statements.Add (ret);
	    ctd.Members.Add (method);

	    // GetMatchType override

	    method = new CodeMemberMethod ();
	    method.Name = "GetMatchType";
	    //method.Attributes = MemberAttributes.Family | MemberAttributes.Override;
	    method.Attributes = MemberAttributes.Public | MemberAttributes.Override;
	    method.ReturnType = CDH.Type;
	    method.LinePragma = Location;
	    
	    val = new CodeTypeOfExpression (MatchType.AsCodeDom);
	    ret = new CodeMethodReturnStatement (val);
	    ret.LinePragma = Location;
	    method.Statements.Add (ret);
	    ctd.Members.Add (method);
	}

	public override string ToString ()
	{
	    return String.Format ("[{0} -> {1}]", Regex, MatchType);
	}
    }
}
