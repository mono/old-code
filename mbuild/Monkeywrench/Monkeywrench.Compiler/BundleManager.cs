//
// BundleManager.cs -- Implements loading and tracking of bundles.
// The interesting things that happen with the list of bundles
// mostly occur in BundleManagerBase
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Text;

using Mono.Build;
using Mono.Build.Bundling;

namespace Monkeywrench.Compiler {

    public class BundleManager : BundleManagerBase {

	public BundleManager () {}

	// Bundle table
	
	Dictionary<string,Assembly> bundles = 
	    new Dictionary<string,Assembly> ();
	
	public override IEnumerable<Assembly> BundleAssemblies {
	    get {
		return bundles.Values;
	    }
	}

	Assembly this [AssemblyName aname] {
	    get {
		if (bundles.ContainsKey (aname.Name))
		    return bundles[aname.Name];
		return null;
	    }
	    
	    set {
		bundles[aname.Name] = value;
	    }
	}

	// Loading bundles
	
	public static AssemblyName MakeName (string shortname, string version) 
	{
	    byte[] pktok = typeof (Result).Assembly.GetName ().GetPublicKeyToken ();
	    
	    AssemblyName n = new AssemblyName ();
	    n.Name = MBuildPrefix + shortname;
	    n.SetPublicKeyToken (pktok);

	    if (version != null)
		n.Version = new Version (version);

	    return n;
	}

	public static AssemblyName MakeName (string shortname) 
	{
	    return MakeName (shortname, null);
	}

	public bool LoadBundle (AssemblyName aname, IWarningLogger logger) 
	{
	    return LoadBundle (aname, logger, true);
	}

	public bool LoadBundle (string path, IWarningLogger logger) 
	{
	    return LoadBundle (path, logger, true);
	}

	public bool LoadBundle (Assembly assy, IWarningLogger logger) 
	{
	    return LoadBundle (assy, null, logger, true);
	}

	bool LoadBundle (AssemblyName aname, IWarningLogger logger, 
			 bool expecting_bundle) 
	{
	    Assembly assy;
	    
	    if (this[aname] != null)
		return false;
	    
	    try {
		assy = System.AppDomain.CurrentDomain.Load (aname);
	    } catch (Exception e) {
		logger.Error (2020, "Could not load the referenced assembly " + aname.ToString (), 
			      String.Format ("{0}: {1}", e.GetType (), e.Message));
		return true;
	    }
	    
	    return LoadBundle (assy, aname.Version, logger, expecting_bundle);
	}

	bool LoadBundle (string path, IWarningLogger logger, 
			 bool expecting_bundle) 
	{
	    Assembly assy;

	    try {
		assy = System.AppDomain.CurrentDomain.Load (path);
	    } catch (Exception e) {
		logger.Error (2020, "Could not load the referenced assembly " + path, 
			      e.Message);
		return true;
	    }
	    
	    return LoadBundle (assy, null, logger, expecting_bundle);
	}

	bool LoadBundle (Assembly assy, Version expected_version, 
			 IWarningLogger logger, bool expecting_bundle) 
	{
	    object[] bundleattrs = assy.GetCustomAttributes (typeof (MonoBuildBundleAttribute), false);
	    if (bundleattrs.Length == 0) {
		if (expecting_bundle) {
		    logger.Error (4001, "The assembly is not a bundle: no MonoBuildBundleAttribute.", assy.FullName);
		    return true;
		}
		
		// This is so a bundle assembly can depend on non-MBuild assemblies and
		// our dependent assembly loading below won't freak out.
		return false;
	    }
	    
	    AssemblyName aname = assy.GetName ();
	    
	    if (expected_version != null && aname.Version != expected_version) {
		string s = String.Format ("Bundle version requirement mismatch: want {0} version {1}, " +
					  "loaded version {3}", aname.Name, expected_version, aname.Version);
		logger.Error (2033, s, null);
		return true;
	    }
	    
	    this[aname] = assy;
	    
	    // load info from referenced bundles first (the assemblies are
	    // already loaded, but we load the type information for them.)
	    
	    foreach (AssemblyName aref in assy.GetReferencedAssemblies ()) {
		if (LoadBundle (aref, logger, false))
		    return true;
	    }
	    
	    return false;
	}
    }
}
