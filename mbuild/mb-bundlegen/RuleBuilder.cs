using System;
using System.Reflection;
using System.Collections.Generic;
using System.CodeDom;

using Mono.Build;
using Mono.Build.Bundling;

namespace MBBundleGen {

    public class RuleBuilder : TypeExpressedItem {

	// note that we do not include argument names / types in the
	// fingerprint hash. If the code behind the rule doesn't change, the results
	// derived from the rule shouldn't be rebuilt, even if the arguments
	// names, types, numbers, etc do vary. At least, it seems like
	// that should be the case to me.
	//
	// Like TemplateBuilder, we are only optionally bound to a structure.

	FingerprintHelper fp = new FingerprintHelper ();

	public RuleBuilder (string name, NamespaceBuilder ns, CodeLinePragma loc, TypeAttributes attr) : 
	    base (name, ns, loc, attr)
	{
	    BaseClass = RuleClass;
	    fp.Add (name);
	}

	// Arguments

	readonly static UserType RuleClass = new UserType (typeof (Rule));
	static CodeVariableDeclarationStatement BaseNumArgs;

	static RuleBuilder ()
	{
	    // @{int base_total = base.NumArguments;}

	    BaseNumArgs = new CodeVariableDeclarationStatement ();
	    BaseNumArgs.Name = "base_total";
	    BaseNumArgs.Type = new CodeTypeReference (typeof (int));
	    BaseNumArgs.InitExpression = 
		new CodePropertyReferenceExpression (CDH.Base, "NumArguments");
	}

	static CodeExpression BasePlusN (int n)
	{
	    CodeBinaryOperatorExpression idexpr = new CodeBinaryOperatorExpression ();
	    idexpr.Left = new CodeVariableReferenceExpression ("base_total");
	    idexpr.Operator = CodeBinaryOperatorType.Add;
	    idexpr.Right = new CodePrimitiveExpression (n);
	    return idexpr;
	}

	public class ArgInfo {
	    public readonly int Idx;
	    public readonly UserType Type;
	    public readonly ArgCount Count;
	    public readonly string Name;
	    public readonly CodeExpression Flags;

	    readonly static CodeTypeReference TRArgFlags = 
		new CodeTypeReference (typeof (ArgFlags));
	    readonly static CodeTypeReferenceExpression TREArgFlags = 
		new CodeTypeReferenceExpression (TRArgFlags);
	    
	    readonly static CodeFieldReferenceExpression Optional = 
		new CodeFieldReferenceExpression (TREArgFlags, "Optional");
	    readonly static CodeFieldReferenceExpression Multi = 
		new CodeFieldReferenceExpression (TREArgFlags, "Multi");
	    readonly static CodeFieldReferenceExpression Default = 
		new CodeFieldReferenceExpression (TREArgFlags, "Default");
	    readonly static CodeFieldReferenceExpression Standard = 
		new CodeFieldReferenceExpression (TREArgFlags, "Standard");
	    readonly static CodeFieldReferenceExpression Ordered = 
		new CodeFieldReferenceExpression (TREArgFlags, "Ordered");
	    readonly static CodeFieldReferenceExpression DefOrd = 
		new CodeFieldReferenceExpression (TREArgFlags, "DefaultOrdered");
	
	    readonly static CodeBinaryOperatorType Or = CodeBinaryOperatorType.BitwiseOr;

	    public ArgInfo (int idx, UserType type, ArgCount count, string name, 
			    bool is_default, bool is_ordered)
	    {
		Idx = idx;
		Type = type;
		Count = count;
		Name = name;

		switch (count) {
		case ArgCount.ZeroOrMore:
		    Flags = new CodeBinaryOperatorExpression (Optional, Or, Multi);
		    break;
		case ArgCount.OneOrMore:
		    Flags = Multi;
		    break;
		case ArgCount.Optional:
		    Flags = Optional;
		    break;
		case ArgCount.Standard:
		    Flags = Standard;
		    break;
		default:
		    throw new Exception ("Unsupported ArgCount kind " + Count.ToString ());
		}

		if (is_ordered) {
		    Flags = new CodeBinaryOperatorExpression (Flags, Or, Ordered);
			
		    if (is_default)
			Flags = new CodeBinaryOperatorExpression (Flags, Or, DefOrd);
		} else if (is_default) {
		    Flags = new CodeBinaryOperatorExpression (Flags, Or, Default);
		}
	    }

	    RuleArgConversion conv;
	    UserType arg_class;

	    public bool Resolve (TypeResolveContext trc, bool errors)
	    {
		if (Type.Resolve (trc, errors))
		    return true;

		arg_class = Type.ResultConversion;

		if (arg_class == null) {
		    conv = RuleArgConversion.None;
		    arg_class = Type;
		} else {
		    // The arg type is some primitive type (bool, string)
		    // that we must convert from a result wrapper

		    Type type = Type.AsSystem;

		    if (type == null)
			throw new Exception ("Can't handle convertible user types");

		    if (type.IsValueType)
			conv = RuleArgConversion.ToValueType;
		    else
			conv = RuleArgConversion.ToRefType;
		}

		return arg_class.Resolve (trc, errors);
	    }

	    public void EmitField (CodeTypeDeclaration type)
	    {
		// Field definition expression

		CodeTypeReference fieldtype = null;

		switch (Count) {
		case ArgCount.OneOrMore:
		    goto case ArgCount.ZeroOrMore;
		case ArgCount.ZeroOrMore:
		    fieldtype = new CodeTypeReference (Type.AsCodeDom, 1);
		    break;
		case ArgCount.Optional:
		    if (conv != RuleArgConversion.ToValueType)
			goto case ArgCount.Standard;

		    fieldtype = new CodeTypeReference ("System.Nullable`1");
		    fieldtype.TypeArguments.Add (Type.AsCodeDom);
		    break;
		case ArgCount.Standard:
		    fieldtype = Type.AsCodeDom;
		    break;
		}

		CodeMemberField fieldmem = new CodeMemberField (fieldtype, Name);
		fieldmem.Attributes = MemberAttributes.Family;
		type.Members.Add (fieldmem);
	    }

	    readonly static CodeTypeReferenceExpression TRERule = 
		new CodeTypeReferenceExpression (CDH.Rule);

	    public void EmitStatements (CodeMemberMethod list, CodeVariableReferenceExpression sink,
					CodeMemberMethod fetch, CodeVariableReferenceExpression source,
					CodeMemberMethod clear)
	    {
		CodeExpression idexpr = RuleBuilder.BasePlusN (Idx);

		// List

		CodeMethodInvokeExpression invoke = CDH.AddBareInvoke (list, sink, "AddArg");
		invoke.Parameters.Add (idexpr);
		invoke.Parameters.Add (new CodePrimitiveExpression (Name));
		invoke.Parameters.Add (new CodeTypeOfExpression (arg_class.AsCodeDom));
		invoke.Parameters.Add (Flags);

		// Fetch

		invoke = new CodeMethodInvokeExpression ();
		invoke.Method = new CodeMethodReferenceExpression (source, "GetArgValue");
		invoke.Parameters.Add (idexpr);

		CodeMethodReferenceExpression mref = new CodeMethodReferenceExpression ();
		mref.TargetObject = TRERule;

		if (conv == RuleArgConversion.None) {
		    switch (Count) {
		    case ArgCount.OneOrMore:
			goto case ArgCount.ZeroOrMore;
		    case ArgCount.ZeroOrMore:
			mref.MethodName = "AsArray";
			mref.TypeArguments.Add (Type.AsCodeDom);
			break;
		    case ArgCount.Optional:
			mref.MethodName = "AsOptional";
			mref.TypeArguments.Add (Type.AsCodeDom);
			break;
		    case ArgCount.Standard:
			mref.MethodName = "AsSingle";
			mref.TypeArguments.Add (Type.AsCodeDom);
			break;
		    }
		} else {
		    mref.TypeArguments.Add (Type.AsCodeDom);
		    mref.TypeArguments.Add (arg_class.AsCodeDom);

		    switch (Count) {
		    case ArgCount.OneOrMore:
			goto case ArgCount.ZeroOrMore;
		    case ArgCount.ZeroOrMore:
			mref.MethodName = "AsArrayConv";
			break;
		    case ArgCount.Optional:
			if (conv == RuleArgConversion.ToValueType)
			    mref.MethodName = "AsOptionalValue";
			else
			    mref.MethodName = "AsOptionalRef";
			break;
		    case ArgCount.Standard:
			mref.MethodName = "AsSingleConv";
			break;
		    }
		}
		
		CodeMethodInvokeExpression inv2 = new CodeMethodInvokeExpression ();
		inv2.Method = mref;
		inv2.Parameters.Add (invoke);

		CodeAssignStatement assg = new CodeAssignStatement (CDH.ThisDot (Name), inv2);
		fetch.Statements.Add (assg);

		// ClearArgValues. Can't clear standard value types -- eg, 'bool member;'.
		// Those we trust will get set again when FetchArgValues is called next time.

		if (conv != RuleArgConversion.ToValueType || Count != ArgCount.Standard) {
		    assg = new CodeAssignStatement (CDH.ThisDot (Name), CDH.Null);
		    clear.Statements.Add (assg);
		}
	    }
	}

	List<ArgInfo> arguments = new List<ArgInfo> ();

	public void AddArgument (UserType type, ArgCount count, string name, bool is_default,
				 bool is_ordered)
	{
	    arguments.Add (new ArgInfo (arguments.Count, type, count, name, 
					is_default, is_ordered));
	}

	string target_arg_name = null;
	bool target_arg_reqd = false;

	public void AddTargetArgument (ArgCount count, string name, bool is_default, 
				       bool is_ordered)
	{
	    if (count == ArgCount.Standard)
		target_arg_reqd = true;
	    else if (count == ArgCount.Optional)
		target_arg_reqd = false;
	    else
		throw new Exception ("A .target argument cannot be multiple-valued");

	    if (is_ordered)
		throw new Exception ("A .target argument cannot be ordered");

	    target_arg_name = name;
	}

	void EmitTargetArg (CodeMemberMethod list, CodeVariableReferenceExpression sink,
			    CodeMemberMethod fetch, CodeVariableReferenceExpression source,
			    CodeMemberMethod clear, CodeTypeDeclaration cur_type)
	{
	    // Field definition expression

	    CodeMemberField fieldmem = new CodeMemberField (CDH.String, target_arg_name);
	    fieldmem.Attributes = MemberAttributes.Family;
	    cur_type.Members.Add (fieldmem);

	    CodeFieldReferenceExpression fieldref = CDH.ThisDot (target_arg_name);

	    // ListArguments

	    CodeMethodInvokeExpression invoke = new CodeMethodInvokeExpression ();
	    invoke.Method = new CodeMethodReferenceExpression (sink, "WantTargetName");
	    invoke.Parameters.Add (new CodePrimitiveExpression (target_arg_reqd));
	    list.Statements.Add (invoke);

	    // FetchArgValues

	    invoke = new CodeMethodInvokeExpression ();
	    invoke.Method = new CodeMethodReferenceExpression (source, "GetTargetName");

	    CodeAssignStatement assg = new CodeAssignStatement (fieldref, invoke);
	    fetch.Statements.Add (assg);

	    // ClearArgValues

	    assg = new CodeAssignStatement (fieldref, CDH.Null);
	    clear.Statements.Add (assg);
	}

	CodeTypeReference ArgInfoSink = new CodeTypeReference (typeof (IArgInfoSink));
	CodeTypeReference ArgValueSource = new CodeTypeReference (typeof (IArgValueSource));

	void EmitArguments (CodeTypeDeclaration cur_type)
	{
	    CodeMemberMethod list, fetch, clear;
	    CodeVariableReferenceExpression sink, source;

	    // ListArguments prologue

	    list = new CodeMemberMethod ();
	    list.Name = "ListArguments";
	    list.Attributes = MemberAttributes.Public | MemberAttributes.Override;
	    list.ReturnType = null;
	    list.Parameters.Add (CDH.Param (ArgInfoSink, "sink"));

	    CDH.EmitBaseChainVoid (list);

	    sink = CDH.VRef ("sink");

	    // FetchArgValues prologue

	    fetch = new CodeMemberMethod ();
	    fetch.Name = "FetchArgValues";
	    fetch.Attributes = MemberAttributes.Public | MemberAttributes.Override;
	    fetch.ReturnType = null;
	    fetch.Parameters.Add (CDH.Param (ArgValueSource, "source"));

	    CDH.EmitBaseChainVoid (fetch);

	    source = new CodeVariableReferenceExpression ("source");

	    // ClearArgValues prologue

	    clear = new CodeMemberMethod ();
	    clear.Name = "ClearArgValues";
	    clear.Attributes = MemberAttributes.Public | MemberAttributes.Override;
	    clear.ReturnType = null;

	    CDH.EmitBaseChainVoid (clear);

	    // Per-arg statements

	    if (target_arg_name != null)
		EmitTargetArg (list, sink, fetch, source, clear, cur_type);

	    if (arguments.Count > 0) {
		list.Statements.Add (BaseNumArgs);
		fetch.Statements.Add (BaseNumArgs);
	    }

	    for (int i = 0; i < arguments.Count; i++) {
		arguments[i].EmitStatements (list, sink, fetch, source, clear);
		arguments[i].EmitField (cur_type);
	    }

	    // No epilogues.

	    cur_type.Members.Add (list);
	    cur_type.Members.Add (fetch);
	    cur_type.Members.Add (clear);

	    // NumArguments property override

	    CodeMemberProperty p = new CodeMemberProperty ();
            p.Name = "NumArguments";
            p.HasGet = true;
            p.HasSet = false;
            p.Type = new CodeTypeReference (typeof (int));
            p.Attributes = MemberAttributes.Public | MemberAttributes.Override;

            CodeBinaryOperatorExpression add = new CodeBinaryOperatorExpression ();
            add.Left = new CodePropertyReferenceExpression (CDH.Base, "NumArguments");
            add.Operator = CodeBinaryOperatorType.Add;
            add.Right = new CodePrimitiveExpression (arguments.Count);

            p.GetStatements.Add (new CodeMethodReturnStatement (add));

            cur_type.Members.Add (p);
	}

	// The build function

	bool bf_manual;
	UserType bf_ret_class;
	string bf_ret_arg;
	string bf_context_arg;
	NativeCode bf_code;

	public void SetBuildFunc (bool is_manual, UserType ret_class, string ret_arg,
				  string context_arg, NativeCode code)
	{
	    bf_manual = is_manual;
	    bf_ret_class = ret_class;
	    bf_ret_arg = ret_arg;
	    bf_context_arg = context_arg;
	    bf_code = code;

	    fp.Add (code);
	}

	void EmitBuildFunc (CodeTypeDeclaration cur_type)
	{
	    // FIXME: allow retclass to be 'bool', 'string', etc and do the 
	    // appropriate conversions.

	    CodeParameterDeclarationExpression p = CDH.Param (CDH.IContext, bf_context_arg);

	    CodeMemberMethod meth = new CodeMemberMethod ();
	    meth.Name = "Build";
	    meth.Attributes = MemberAttributes.Public | MemberAttributes.Override;
	    meth.ReturnType = CDH.Result;
	    meth.Parameters.Add (p);

	    if (bf_manual)
		EmitBuildManual (meth);
	    else
		EmitBuildWrapped (cur_type, meth);

	    cur_type.Members.Add (meth);

	    // We know this. In Resolve() we check that OverrideResultType is
	    // not set if we have a build func (which would lead to
	    // emitting the 'general result type' code twice)

	    EmitGeneralResult (cur_type, bf_ret_class);
	}

	void EmitBuildManual (CodeMemberMethod meth)
	{
	    meth.Statements.Add (bf_code.AsStatement);
	}

	void EmitBuildWrapped (CodeTypeDeclaration cur_type, CodeMemberMethod meth)
	{
	    CodeMethodInvokeExpression inv =
		new CodeMethodInvokeExpression (CDH.This, "CreateResultObject", 
						new CodeExpression[0] {});
	    CodeCastExpression cast = 
		new CodeCastExpression (bf_ret_class.AsCodeDom, inv);
	    CodeVariableDeclarationStatement vd = 
		new CodeVariableDeclarationStatement (bf_ret_class.AsCodeDom, bf_ret_arg, cast);
		
	    meth.Statements.Add (vd);
	    
	    CodeVariableReferenceExpression r1 = CDH.VRef (bf_ret_arg);
	    CodeVariableReferenceExpression r2 = CDH.VRef (bf_context_arg);

	    inv = new CodeMethodInvokeExpression (CDH.This, "BuildImpl",
						  new CodeExpression[2] { r1, r2 });

	    meth.Statements.Add (CDH.IfTrueReturnNull (inv));
	    meth.Statements.Add (new CodeMethodReturnStatement (r1));

	    // BuildImpl

	    CodeParameterDeclarationExpression p1 = CDH.Param (bf_ret_class.AsCodeDom, bf_ret_arg);
	    CodeParameterDeclarationExpression p2 = CDH.Param (CDH.IContext, bf_context_arg);

	    meth = new CodeMemberMethod ();
	    meth.Name = "BuildImpl";
	    meth.Attributes = MemberAttributes.Private;
	    meth.ReturnType = CDH.Bool;
	    meth.Parameters.Add (p1);
	    meth.Parameters.Add (p2);
	    meth.Statements.Add (bf_code.AsStatement);
	    cur_type.Members.Add (meth);
	}

	// Result type management. Static for SourcefileRuleBuilder.

	public NativeCode SpecificResultCode = null;

	public void EmitSpecificResult (CodeTypeDeclaration ctd)
	{
	    if (SpecificResultCode != null)
		EmitSpecificResult (ctd, SpecificResultCode);
	}

	public static void EmitSpecificResult (CodeTypeDeclaration ctd, NativeCode code)
	{
            // SpecificResultType property

            CodeMemberProperty srt = new CodeMemberProperty ();
            srt.Name = "SpecificResultType";
            srt.Attributes = MemberAttributes.Public | MemberAttributes.Override;
            srt.Type = CDH.Type;
            srt.HasGet = true;
            srt.HasSet = false;
            srt.GetStatements.Add (code.AsStatement);

            ctd.Members.Add (srt);
	}

	public UserType OverrideResultType;

	protected void EmitGeneralResult (CodeTypeDeclaration ctd)
	{
	    if (OverrideResultType != null)
		EmitGeneralResult (ctd, OverrideResultType);
	}

	public static void EmitGeneralResult (CodeTypeDeclaration ctd, UserType rtype)
	{
	    CodeMemberProperty grt = new CodeMemberProperty ();
            grt.Name = "GeneralResultType";
            grt.Attributes = MemberAttributes.Public | MemberAttributes.Override;
            grt.Type = CDH.Type;
            grt.HasGet = true;
            grt.HasSet = false;

            CodeExpression rt = new CodeTypeOfExpression (rtype.AsCodeDom);
            grt.GetStatements.Add (new CodeMethodReturnStatement (rt));

            ctd.Members.Add (grt);
	}

	// Misc

	public override void AddNativeMember (NativeCode nc)
	{
	    base.AddNativeMember (nc);
	    fp.Add (nc);
	}

	// General

	public override bool Resolve (TypeResolveContext trc, bool errors)
	{
	    bool ret = base.Resolve (trc, errors);

	    if (bf_ret_class != null) {
		// Null if rule doesn't have a build func of its
		// own (eg, it inherits)
		ret |= bf_ret_class.Resolve (trc, errors);

		if (errors && OverrideResultType != null) {
		    Console.Error.WriteLine ("Can't have a build function and separately " + 
					     "specify a general result type in {0}", ClassName);
		    return true;
		}
	    }

	    if (OverrideResultType != null)
		ret |= OverrideResultType.Resolve (trc, errors);

	    for (int i = 0; i < arguments.Count; i++)
		ret |= arguments[i].Resolve (trc, errors);

	    return ret;
	}

	protected override void Emit (CodeTypeDeclaration ctd)
	{
	    CodeConstructor ctor = EmitEmptyCtor (ctd);

	    if (SpecificResultCode != null) {
		CodeAssignStatement assg = new CodeAssignStatement ();
		assg.Left = new CodeFieldReferenceExpression (CDH.This, "specific_varies");
		assg.Right = CDH.True;

		ctor.Statements.Add (assg);
	    }

	    if (bf_ret_class != null)
		// Some classes override parameters but not build functions
		EmitBuildFunc (ctd);

	    EmitGeneralResult (ctd);
	    EmitSpecificResult (ctd);
	    EmitArguments (ctd);
	    fp.EmitGetFingerprint (ctd);
	}
    }
}
