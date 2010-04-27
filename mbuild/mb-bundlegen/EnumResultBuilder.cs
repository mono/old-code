using System;
using System.Reflection;
using System.Collections.Generic;
using System.CodeDom;

using Mono.Build;

namespace MBBundleGen {

    public class EnumResultBuilder {

	TheResult result;
	TheEnum enumer;

	public EnumResultBuilder (string name, NamespaceBuilder ns, CodeLinePragma loc, TypeAttributes attr)
	{
	    if (name == null)
		throw new ArgumentNullException ();
	    
	    enumer = new TheEnum (name, ns, loc, attr);
	    result = new TheResult (name + "Result", ns, name, loc, attr);
	}

	public TypeExpressedItem Enum { get { return enumer; } }

	public TypeExpressedItem Result { get { return result; } }

	class TheResult : TypeExpressedItem {

	    UserType etype;

	    public TheResult (string name, NamespaceBuilder ns, string ename, CodeLinePragma loc, TypeAttributes attr)
		: base (name, ns, loc, attr)
	    {
		etype = new UserType (ename);

		BaseClass = new UserType (typeof (EnumResult<>));
		BaseClass.AddTypeArgument (etype);
	    }

	    public string DefaultName;

	    protected override void Emit (CodeTypeDeclaration type)
	    {
		type.CustomAttributes.Add (new CodeAttributeDeclaration ("System.Serializable"));

		// Initialized constructor

		CodeFieldReferenceExpression left = CDH.ThisDot ("Value");

		CodeConstructor ctor = EmitEmptyCtor (type);
		ctor.Parameters.Add (CDH.Param (etype, "init"));
		ctor.Statements.Add (new CodeAssignStatement (left, CDH.VRef ("init")));

		// Default constructor.

		ctor = EmitEmptyCtor (type);

		CodeTypeReferenceExpression tre = 
		    new CodeTypeReferenceExpression (etype.AsCodeDom);
		CodeFieldReferenceExpression right = 
		    new CodeFieldReferenceExpression (tre, DefaultName);
		ctor.Statements.Add (new CodeAssignStatement (left, right));
	    }

	}

	class TheEnum : TypeExpressedItem {

	    public TheEnum (string name, NamespaceBuilder ns, CodeLinePragma loc, TypeAttributes attr) : 
		base (name, ns, loc, attr)
	    {
		BaseClass = new UserType (typeof (int));
	    }

	    List<string> fields = new List<string> ();

	    public void AddField (string name)
	    {
		fields.Add (name);
	    }

	    protected override void Emit (CodeTypeDeclaration type)
	    {
		type.IsEnum = true;

		for (int i = 0; i < fields.Count; i++) {
		    CodeMemberField f = new CodeMemberField ();
		    f.Name = fields[i];
		    //f.LinePragma = ...
		    f.InitExpression = new CodePrimitiveExpression (i);

		    type.Members.Add (f);
		}
	    }
	}

	// Frontend

	public void AddField (string name, bool is_default)
	{
	    enumer.AddField (name);

	    if (!is_default)
		return;

	    if (result.DefaultName != null)
		throw new Exception ("Cannot have two default enumeration values");

	    result.DefaultName = name;
	}

	// We don't implement Register, Resolve, and Emit;
	// instead, the caller should build this object,
	// then extract the Enum and Result members and
	// use them directly.
    }
}
