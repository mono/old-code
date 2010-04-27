using System;
using System.Reflection;
using System.Collections.Generic;
using System.CodeDom;

using Mono.Build; // ExHelp

namespace MBBundleGen {

    // Baseclass for bundle items that are expressed as types.

    public abstract class TypeExpressedItem {

	string namebase;
	CodeLinePragma location;
	TypeAttributes attrs;
	UserType baseclass;

	protected TypeExpressedItem (string namebase, NamespaceBuilder ns, CodeLinePragma loc, TypeAttributes attrs)
	{
	    if (namebase == null)
		throw new ArgumentNullException ();

	    this.namebase = namebase;
	    location = loc;
	    this.attrs = attrs;

	    this.ns = ns;
	    ns.AddItem (this);
	}

	NamespaceBuilder ns;

	public NamespaceBuilder Namespace { get { return ns; } }

	protected CodeLinePragma Location { get { return location; } }

	protected virtual string GetClassName (string namebase)
	{
	    return namebase;
	}

	public string ClassName { get { return GetClassName (namebase); } }

	public string FullName { get { return ns.RealName + "." + ClassName; } }
       
	public override string ToString () { return FullName; } 

	public CodeTypeReference CodeSelfType {
	    get { return new CodeTypeReference (FullName); }
	}

	public UserType BaseClass {
	    get { return baseclass; }

	    set { baseclass = value; }
	}

	// Native members

	List<NativeCode> native_members;

	public virtual void AddNativeMember (NativeCode nc)
	{
	    if (native_members == null)
		native_members = new List<NativeCode> ();

	    native_members.Add (nc);
	}

	// Interface implementations

	List<UserType> implements;

	public void AddInterfaceImpl (UserType itype)
	{
	    if (implements == null)
		implements = new List<UserType> ();

	    implements.Add (itype);
	}

	// Resolve

	bool registered = false;

	public virtual bool Resolve (TypeResolveContext trc, bool errors)
	{
	    if (!registered) {
		trc.Driver.DefineUserType (FullName, this);
		registered = true;
	    }

	    if (errors && baseclass == null) {
		Console.Error.WriteLine ("No base class set for item {0}", this);
		return true;
	    }
		
	    bool ret = baseclass.Resolve (trc, errors);

	    if (implements != null) {
		foreach (UserType ut in implements)
		    ret |= ut.Resolve (trc, errors);
	    }

	    return ret;
	}

	public UserType ResolveSelfType (TypeResolveContext trc, bool errors)
	{
	    UserType ut = new UserType (FullName);

	    if (ut.Resolve (trc, errors) && errors)
		throw ExHelp.App ("Failed to resolve SelfType of {0}?", this);

	    return ut;
	}

	// Emit

	protected abstract void Emit (CodeTypeDeclaration ctd);

	public CodeTypeDeclaration Emit ()
	{
	    CodeTypeDeclaration ctd = new CodeTypeDeclaration (ClassName);
	    ctd.LinePragma = location;
	    ctd.TypeAttributes |= attrs;
	    ctd.BaseTypes.Add (baseclass.AsCodeDom);

	    if (native_members != null) {
		foreach (NativeCode nc in native_members)
		    ctd.Members.Add (nc.AsMember);
	    }

	    if (implements != null) {
		foreach (UserType ut in implements)
		    ctd.BaseTypes.Add (ut.AsCodeDom);
	    }

	    Emit (ctd);

	    return ctd;
	}

	public static CodeConstructor EmitEmptyCtor (CodeTypeDeclaration type)
	{
	    CodeConstructor ctor = new CodeConstructor ();
	    ctor.Attributes = MemberAttributes.Public;
	    
	    type.Members.Add (ctor);

	    return ctor;
	}
    }
}
