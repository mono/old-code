using System;
using System.Reflection;
using System.Collections.Generic;

using Mono.Build;
using Mono.Build.Bundling;

namespace MBBundleGen {

    public class ExistingNamespaceParams : INamespaceParams {

	public ExistingNamespaceParams (Type t) 
	{
	    // Populate our tables.

	    MemberInfo[] mems = t.GetMembers ();

	    for (int i = 0; i < mems.Length; i++) {
		MemberInfo mi = mems[i];

		object[] attrs = mi.GetCustomAttributes (typeof (StructureParameterAttribute), 
							 false);

		if (attrs == null || attrs.Length == 0)
		    continue;

		sparams[mi.Name] = ((StructureParameterAttribute) attrs[0]).Kind;

		if (sparams[mi.Name] == StructureParameterKind.Structure) {
		    Type stype = null;

		    if (mi is PropertyInfo)
			stype = (mi as PropertyInfo).PropertyType;
		    else if (mi is FieldInfo)
			stype = (mi as FieldInfo).FieldType;

		    structtypes[mi.Name] = new UserType (stype);
		}
	    }
	}

	// Parameter tables

	Dictionary<string,StructureParameterKind> sparams = 
	    new Dictionary<string,StructureParameterKind> ();
	Dictionary<string,UserType> structtypes = 
	    new Dictionary<string,UserType> ();

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
		    throw ExHelp.InvalidOp ("Trying to get type of undefined parameter `{0}'",
					    name);
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
    }
}
