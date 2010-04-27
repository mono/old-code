using System;
using System.Reflection;
using System.Collections.Generic;
using System.CodeDom;
using System.Text;

using Mono.Build; // ExHelp
using Mono.Build.Bundling; // StructureBindingAttribute

namespace MBBundleGen {

    public class UserType {

	object t;
	List<UserType> typeargs;
	bool resolved = false;

	public UserType (string usertype)
	{
	    if (usertype == null)
		throw new ArgumentNullException ();

	    t = usertype;
	}

	public UserType (Type systype)
	{
	    if (systype == null)
		throw new ArgumentNullException ();

	    t = systype;
	}

	public bool Equals (UserType other)
	{
	    if (other == null)
		return false;

	    if (IsUser) {
		if (other.IsSystem)
		    return false;

		if ((t as string) != (other.t as string))
		    return false;
	    } else {
		if (other.IsUser)
		    return false;

		if (!AsSystem.Equals (other.AsSystem))
		    return false;
	    }

	    if (typeargs != null) {
		if (other.typeargs == null)
		    return false;

		if (typeargs.Count != other.typeargs.Count)
		    return false;

		for (int i = 0; i < typeargs.Count; i++)
		    if (!typeargs[i].Equals (other.typeargs[i]))
			return false;
	    } else {
		if (other.typeargs != null)
		    return false;
	    }

	    return true;
	}

	public void AddTypeArgument (UserType arg)
	{
	    Type sys = t as Type;

	    if (sys != null && !sys.IsGenericType)
		throw ExHelp.InvalidOp ("Cannot add type argument on known non-generic type {0}", sys);

	    if (typeargs == null)
		typeargs = new List<UserType> ();

	    typeargs.Add (arg);
	}

	public bool Resolve (TypeResolveContext trc, bool errors)
	{
	    if (resolved)
		return false;

	    if (IsUser) {
	        UserType newval = trc.Resolve ((string) t, errors);

		if (newval == null) {
		    if (errors)
			Console.Error.WriteLine ("Failed to resolve type {0}", t);
		    return true;
		}

		this.t = newval.t;
	    }

	    if (typeargs != null) {
		bool ret = false;

		foreach (UserType ut in typeargs)
		    ret |= ut.Resolve (trc, errors);

		if (ret)
		    return ret;
	    }

	    resolved = true;
	    return false;
	}

	public bool ResolveExtension (string ext, TypeResolveContext trc, bool errors)
	{
	    if (errors && typeargs != null)
		// Don't want to think about how this should be handled.
		throw new InvalidOperationException ();

	    string exname;

	    if (IsUser)
		exname = ((string) t) + ext;
	    else
		exname = ((Type) t).FullName + ext;

	    UserType exval = trc.Resolve (exname, false);
	    if (exval != null) {
		this.t = exval.t;
		resolved = true;
		return false;
	    }

	    return Resolve (trc, errors);
	}

	public bool IsUser { get { return t is string; } }

	public bool IsSystem { get { return t is Type; } }

	public Type AsSystem { 
	    get { 
		Type sys = (Type) t;

		if (typeargs != null) {
		    Type[] args = new Type[typeargs.Count];

		    for (int i = 0; i < typeargs.Count; i++)
			args[i] = typeargs[i].AsSystem;

		    sys = sys.MakeGenericType (args);
		}

		return sys;
	    }
	}

	public string FullName {
	    get {
		if (!resolved)
		    throw new InvalidOperationException ();

		if (t is string)
		    return (string) t;

		return ((Type) t).FullName;
	    }
	}

	public CodeTypeReference AsCodeDom {
	    get {
		if (!resolved)
		    throw new InvalidOperationException ();

		CodeTypeReference tr;

		if (t is string)
		    tr = new CodeTypeReference ((string) t);
		else
		    tr = new CodeTypeReference ((Type) t);

		if (typeargs != null) {
		    foreach (UserType ut in typeargs)
			tr.TypeArguments.Add (ut.AsCodeDom);
		}

		return tr;
	    }
	}

	public UserType ResultConversion {
	    get {
		if (!resolved)
		    throw new InvalidOperationException ();
		if (IsUser)
		    // Someday this could be possible?
		    return null;

		Type type = (Type) t;

		if (type.Equals (typeof (bool)))
		    return new UserType (typeof (Mono.Build.MBBool));
		if (type.Equals (typeof (string)))
		    return new UserType (typeof (Mono.Build.MBString));
		// ....

		return null;
	    }
	}

	public UserType ValueConversion {
	    get {
		if (!resolved)
		    throw new InvalidOperationException ();
		if (IsUser)
		    // Again, someday ...
		    return null;

		Type type = (Type) t;

		if (typeof (Mono.Build.MBBool).IsAssignableFrom (type))
		    return new UserType (typeof (bool));
		if (typeof (Mono.Build.MBString).IsAssignableFrom (type))
		    return new UserType (typeof (string));
		// ....

		return null;
	    }
	}

	public UserType ResolveUsedStructureType (TypeResolveContext trc, bool errors)
	{
	    if (!resolved) {
		if (errors)
		    throw new InvalidOperationException ();
		return null;
	    }

	    UserType ret;

	    if (IsUser) {
		// User type. Use our lookup tables

		StructureBoundItem sbi = trc.Driver.GetUserTypeItem ((string) t) as StructureBoundItem;

		if (sbi == null) {
		    if (errors)
			throw ExHelp.App ("Expected structure bound item but got {0} for {1}",
					  trc.Driver.GetUserTypeItem ((string) t), this);
		    return null;
		}

		if (!sbi.UsesStructure)
		    return null;

		ret = sbi.NS.ParamsType;
	    } else {
		// System type. Use reflection.

		Type type = (Type) t;

		object[] attrs = type.GetCustomAttributes (typeof (StructureBindingAttribute), false);

		if (attrs.Length == 0)
		    throw ExHelp.App ("Expected type {0} to have a StructureBindingAttribute " +
				      "but it didn't", type);

		StructureBindingAttribute attr = (StructureBindingAttribute) attrs[0];

		if (!attr.UsesStructure)
		    return null;

		ret = new UserType (attr.StructureType);
	    }

	    if (ret.Resolve (trc, errors)) {
		if (errors)
		    throw ExHelp.App ("Failed to resolve bound structure type {0}", ret);
		return null;
	    }

	    return ret;
	}

	public bool? ResolvesAsRule (TypeResolveContext trc, bool errors)
	{
	    if (!resolved) {
		if (errors)
		    throw new InvalidOperationException ();
		return null;
	    }

	    if (IsSystem)
		// System type. Use reflection.
		return typeof (Rule).IsAssignableFrom ((Type) t);

	    // User type. Use our lookup tables

	    TypeExpressedItem tei = trc.Driver.GetUserTypeItem ((string) t);
	    return (tei is RuleBuilder || tei is SourcefileRuleBuilder);
	}

	public UserType ResolveBoundRuleType (TypeResolveContext trc, bool errors)
	{
	    if (!resolved) {
		if (errors)
		    throw new InvalidOperationException ();
		return null;
	    }

	    UserType ret;

	    if (IsUser) {
		// User type. Use our lookup tables

		RuleTemplateBuilder rtb = trc.Driver.GetUserTypeItem ((string) t) as RuleTemplateBuilder;

		if (errors && rtb == null)
		    throw ExHelp.App ("Expected a rule template but got {0} for {1}",
				      trc.Driver.GetUserTypeItem ((string) t), this);

		ret = new UserType (rtb.Rule.FullName);
	    } else {
		// System type. Use reflection.

		Type type = (Type) t;

		object[] attrs = type.GetCustomAttributes (typeof (RuleBindingAttribute), false);

		if (errors && attrs.Length == 0)
		    throw ExHelp.App ("Expected type {0} to have a RuleBindingAttribute " +
				      "but it didn't. This type is not allowed to be a rule baseclass.", type);

		ret = new UserType (((RuleBindingAttribute) attrs[0]).RuleType);
	    }

	    if (ret.Resolve (trc, errors)) {
		if (errors)
		    throw ExHelp.App ("Failed to resolve bound rule type {0}", ret);
		return null;
	    }

	    return ret;
	}

	public override string ToString ()
	{
	    if (IsUser)
		return String.Format ("{0} (being compiled)", t);

	    Type type = (Type) t;

	    return String.Format ("{0} ({1})", type.FullName, type.Assembly);
	}
    }
}
