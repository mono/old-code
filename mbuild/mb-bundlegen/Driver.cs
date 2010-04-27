using System;
using System.Reflection;
using System.Collections.Generic;
using System.CodeDom;
using System.IO;

using Mono.Build;

namespace MBBundleGen {

    public class Driver {

	public Driver ()
	{
	}

	// Assemblies

	List<Assembly> assemblies = new List<Assembly> ();

	public IEnumerable<Assembly> Assemblies {
	    get { return assemblies; }
	}

	public bool LoadAssembly (string s)
	{
	    Assembly assy;
	    
	    // Based from mcs/driver.cs
	    
	    try {
		char[] path_chars = { '/', '\\' };
		
		if (s.IndexOfAny (path_chars) != -1)
		    assy = Assembly.LoadFrom (s);
		else {
		    string news = s;
		    
		    if (news.EndsWith (".dll") || news.EndsWith (".exe"))
			news = s.Substring (0, s.Length - 4);
		    assy = Assembly.Load (news);
		}
	    } catch (Exception e) {
		Console.Error.WriteLine ("Unable to load assembly `{0}': {1}",
					 s, e.Message);
		return true;
	    }
	    
	    assemblies.Add (assy);
	    return false;
	}

	// Namespaces

	Dictionary<string,NamespaceBuilder> namespaces = 
	    new Dictionary<string,NamespaceBuilder> ();

	public NamespaceBuilder GetNamespace (string name)
	{
	    if (!namespaces.ContainsKey (name))
		namespaces[name] = new NamespaceBuilder (name, new TypeResolveContext (this));

	    return namespaces[name];
	}

	// Types

	Dictionary<string,TypeExpressedItem> user_types = 
	    new Dictionary<string,TypeExpressedItem> ();

	bool definitions_made = false;

	public void DefineUserType (string tname, TypeExpressedItem tei)
	{
	    //Console.WriteLine ("Registering user type: {0}", tname);

	    if (user_types.ContainsKey (tname))
		throw ExHelp.App ("Trying to redefine type {0}", tname);

	    user_types[tname] = tei;
	    definitions_made = true;
	}

	public TypeExpressedItem GetUserTypeItem (string tname)
	{
	    if (!user_types.ContainsKey (tname))
		throw ExHelp.App ("Trying to get undefined user type {0}", tname);

	    return user_types[tname];
	}

	// Lookups

	public Type LookupExistingFQN (string full)
	{
	    Type t = null;

	    foreach (Assembly assy in Assemblies) {
		Type hit = assy.GetType (full, false);

		if (hit == null)
		    continue;

		if (t != null) {
		    string s = String.Format ("Two assemblies define the type {0}: {1} " +
					      "and {2}", full, t.Assembly, assy);
		    throw new Exception (s);
		}

		t = hit;
	    }

	    return t;
	}

	public UserType LookupFQN (string full, bool can_fail)
	{
	    if (user_types.ContainsKey (full))
		return new UserType (full);

	    Type t = LookupExistingFQN (full);

	    if (t != null)
		return new UserType (t);
		
	    if (!can_fail)
		throw new Exception ("Could not resolve the type name " + full);

	    return null;
	}

	// Running things

	Dictionary<string,Parser> parsers =
	    new Dictionary<string,Parser> ();

	public bool ParseFile (string file)
	{
	    Parser p = Parser.CreateForFile (file, this);
	    int res = p.Parse ();
				
	    if (res != 0)
		return true;

	    parsers[file] = p;
	    return false;
	}

	Dictionary<string,string> natives =
	    new Dictionary<string,string> ();

	public bool AddNativeFile (string nfile)
	{
	    if (!File.Exists (nfile)) {
		Console.Error.WriteLine ("Native source file {0} doesn't exist.", nfile);
		return true;
	    }

	    // Ok, it's a little goofy to read the whole file
	    // into a string and write it out to a temp file.
	    // But this is me being lazy.
	    
	    using (StreamReader sr = new StreamReader (nfile)) {
		natives[nfile] = sr.ReadToEnd ();
	    }

	    return false;
	}

	CodeCompileUnit CreateGlobalUnit (string assembly_version, string keyfile)
	{
	    CodeCompileUnit unit = new CodeCompileUnit ();
			
	    CodeAttributeDeclaration decl = new CodeAttributeDeclaration ("Mono.Build.Bundling.MonoBuildBundleAttribute");
	    unit.AssemblyCustomAttributes.Add (decl);

	    CodeAttributeArgument arg = new CodeAttributeArgument (new CodePrimitiveExpression (true));
	    decl = new CodeAttributeDeclaration ("System.Reflection.AssemblyDelaySignAttribute");
	    decl.Arguments.Add (arg);
	    unit.AssemblyCustomAttributes.Add (decl);

	    arg = new CodeAttributeArgument (new CodePrimitiveExpression (assembly_version));
	    decl = new CodeAttributeDeclaration ("System.Reflection.AssemblyVersionAttribute");
	    decl.Arguments.Add (arg);
	    unit.AssemblyCustomAttributes.Add (decl);
	    
	    arg = new CodeAttributeArgument (new CodePrimitiveExpression (keyfile));
	    decl = new CodeAttributeDeclaration ("System.Reflection.AssemblyKeyFileAttribute");
	    decl.Arguments.Add (arg);
	    unit.AssemblyCustomAttributes.Add (decl);

	    return unit;
	}

	public bool Prepare ()
	{
	    definitions_made = true;
	    int n = 0;

	    while (definitions_made) {
		definitions_made = false;

		foreach (Parser p in parsers.Values)
		    p.Resolve (false);

		if (n++ > 100)
		    throw ExHelp.App ("Infinite loop of user type declarations???");

		// definitions_made gets set if any resolve
		// function defines a new usertype, which might
		// play a role in type resolution.
	    }

	    bool ret = false;

	    // Now all the user types have been defined, so
	    // if any type resolution fails, it is an error.

	    foreach (Parser p in parsers.Values)
		ret |= p.Resolve (true);

	    return ret;
	}

	public Dictionary<string,CodeCompileUnit> GetUnits (string assembly_version, string keyfile)
	{
	    Dictionary<string,CodeCompileUnit> d = 
		new Dictionary<string,CodeCompileUnit> ();

	    d["internally generated"] = CreateGlobalUnit (assembly_version, keyfile);

	    foreach (string k in parsers.Keys)
		d[k] = parsers[k].Emit ();

	    foreach (string k in natives.Keys)
		d[k] = new CodeSnippetCompileUnit (natives[k]);

	    return d;
	}
    }
}
