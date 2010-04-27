using System;
using System.Reflection;
using System.Collections.Generic;
using System.CodeDom;

namespace MBBundleGen {

    // I should really read the C# name resolution spec.
    // This is all made up.

    public class TypeResolveContext {

	Driver driver;
	List<string> implicit_usings = new List<string> ();
	List<string> explicit_usings = new List<string> ();

	public TypeResolveContext (Driver driver)
	{
	    this.driver = driver;
	}

	public void AddUsing (string s)
	{
	    explicit_usings.Add (s);
	}

	public Driver Driver { get { return driver; } }

	string cur_ns;

	public void SetNamespace (string ns)
	{
	    if (ns == null)
		throw new ArgumentNullException ();
	    if (cur_ns != null)
		throw new InvalidOperationException ();

	    implicit_usings.Add (ns);

	    cur_ns = ns;
	    int idx;

	    while ((idx = ns.LastIndexOf ('.')) >= 0) {
		ns = ns.Substring (0, idx);
		implicit_usings.Add (ns);
	    }
	}
	
	// Lookups

	IEnumerable<string> AllUsings {
	    get {
		foreach (string s in implicit_usings)
		    yield return s;
		foreach (string s in explicit_usings)
		    yield return s;
	    }
	}

	public UserType Resolve (string tname, bool errors)
	{
	    if (tname.IndexOf ('.') >= 0)
		return driver.LookupFQN (tname, !errors);

	    // Not fully-qualified. Spelunk namespaces.

	    UserType ut = null;

	    //Console.WriteLine ("Trying to look up {0}", tname);

	    foreach (string ns in AllUsings) {
		string full = ns + "." + tname;
		UserType hit = driver.LookupFQN (full, !errors);

		//Console.WriteLine ("       {0} -> {1}", full, hit);

		if (hit == null)
		    continue;

		if (errors && ut != null && !ut.Equals (hit)) {
		    string s = String.Format ("Ambiguous type name {0}: could be {1} " +
					      "or {2}", tname, ut, hit);
		    throw new Exception (s);
		}

		ut = hit;
	    }

	    if (errors && ut == null)
		throw new Exception ("Could not resolve the type name " + tname);

	    return ut;
	}

	// Util

	public void ApplyToNamespace (CodeNamespace ns)
	{
	    foreach (string s in explicit_usings)
		ns.Imports.Add (new CodeNamespaceImport (s));
	}
    }
}
