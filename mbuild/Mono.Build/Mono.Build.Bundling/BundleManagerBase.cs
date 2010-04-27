//
// BundleManagerBase.cs -- does useful stuff with loaded bundle assemblies
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Text;

using Mono.Build;

namespace Mono.Build.Bundling {

    public abstract class BundleManagerBase {

	public const string MBuildPrefix = "MBuildDynamic.";

	// Bundle table hole
	
	public abstract IEnumerable<Assembly> BundleAssemblies { get; }

	// Namespace management

	public IEnumerable<Type> BundleTypes {
	    get {
		foreach (Assembly assy in BundleAssemblies)
		    foreach (Type t in assy.GetExportedTypes ())
			yield return t;
	    }
	}
	
	public bool LookupType (string name, out Type t, IWarningLogger logger) 
	{
	    // We need the out parameter because returning null could either mean
	    // "no match" (ok) or "error during lookup" (bad, and we should stop).
	    // Currently, the only error condition is that the type is defined
	    // more than one bundle; it is annoying to have to make all the callers
	    // of this function jump through hoops, but it is the kind of circumstance
	    // that can occur.

	    if (name == null)
		throw new ArgumentNullException ();
	    
	    //if (!StrUtils.StartsWith (name, MBuildPrefix))
	    //name = MBuildPrefix + name;
	    
	    t = null;
	    
	    // efficiency? hah!
	    
	    foreach (Assembly assy in BundleAssemblies) {
		Type match = assy.GetType (name);
		
		if (match == null)
		    continue;
		
		if (t != null) {
		    logger.Error (2024, String.Format ("Two assemblies define the type" +
						       "{0}: {1} and {2}",
						       name, t.Assembly.FullName, assy.FullName), null);
		    return true;
		}
		
		t = match;
	    }
	    
	    return false;
	}

	// Default-created structures
	// XXX re-refactor this stuff

	Dictionary<string,StructureTemplate> sinfo = new Dictionary<string,StructureTemplate> ();
	Dictionary<string,bool> sused = new Dictionary<string,bool> ();

	public const string DefaultStructureClass = "_DefaultStructure";

	public StructureTemplate GetNamespaceTemplate (string ns, IWarningLogger log)
	{
	    if (sinfo.ContainsKey (ns))
		return sinfo[ns];

	    string cfg = MBuildPrefix + ns + "." + DefaultStructureClass;
	    Type t = null;

	    if (LookupType (cfg, out t, log)) {
		log.Error (9999, "Error looking up namespacep parameter class " + cfg,
			   null);
		return null;
	    }

	    if (t == null) {
		log.Error (9999, "No bundle defines the namespace parameter class" +
			   cfg, null);
		return null;
	    }

	    StructureTemplate stmpl = (StructureTemplate) Activator.CreateInstance (t);

	    MethodInfo mi = t.GetMethod ("ApplyDefaults");
	    object ret = mi.Invoke (stmpl, new object[] { this, log });

	    if ((bool) ret)
		return null;

	    sinfo[ns] = stmpl;
	    sused[ns] = false;

	    return stmpl;
	}

	// Don't just retrieve the template instance; also Apply() it to our project,
	// instantiating whatever providers or whatever the template defines.

	public StructureTemplate UseNamespaceTemplate (string ns, string declloc,
						       IWarningLogger log)
	{
	    if (proj == null)
		throw new InvalidOperationException ("Must call SetProject before UseNamespaceMaster!");

	    StructureTemplate tmpl = GetNamespaceTemplate (ns, log);

	    if (tmpl == null)
		return null;

	    if (sused[ns])
		return tmpl;

	    if (tmpl.Apply (proj, declloc, log))
		return null;

	    sused[ns] = true;
	    return tmpl;
	}

	// Rule template helpers

	public bool GetTemplateForRule (string ns, Type rtype, out TargetTemplate ttmpl, IWarningLogger log)
	{
	    ttmpl = null;

	    StructureTemplate stmpl = GetNamespaceTemplate (ns, log);

	    if (stmpl == null || !sused[ns]) {
		log.Error (9999, "Trying to use structure template of namespace " +
			   ns + ", but either it wasn't defined or it hasn't been " +
			   "initialized yet.", null);
		return true;
	    }

	    // See if we can find a type that has a StructureBinding to tmpl
	    // and a RuleBinding to rtype.

	    string pns = MBuildPrefix + ns;
	    Type ttype = null, stype = stmpl.GetType ();

	    foreach (Assembly assy in BundleAssemblies) {
		foreach (Type t in assy.GetExportedTypes ()) {
		    if (t.Namespace != pns)
			continue;

		    if (!t.IsSubclassOf (typeof (TargetTemplate)))
			continue;

		    object[] attrs = t.GetCustomAttributes (typeof (StructureBindingAttribute), false);

		    if (attrs.Length == 0)
			continue;

		    StructureBindingAttribute sba = (StructureBindingAttribute) attrs[0];

		    if (sba.StructureType == null)
			continue;

		    if (!sba.StructureType.Equals (stype))
			continue;

		    attrs = t.GetCustomAttributes (typeof (RuleBindingAttribute), false);

		    if (attrs.Length == 0)
			continue;

		    if (!((RuleBindingAttribute) attrs[0]).RuleType.Equals (rtype))
			continue;

		    if (ttype != null) {
			log.Warning (9999, "Two hits for rule template: " + ttype.ToString () +
				     " and " + t.ToString (), rtype.ToString ());
		    }

		    ttype = t;
		}
	    }

	    if (ttype != null)
		ttmpl = (TargetTemplate) Activator.CreateInstance (ttype, stmpl);

	    return false;
	}

	// Misc

	ProjectBuilder proj;

	public void SetProject (ProjectBuilder proj)
	{
	    if (proj == null)
		throw new ArgumentNullException ();

	    this.proj = proj;
	}
    }
}
