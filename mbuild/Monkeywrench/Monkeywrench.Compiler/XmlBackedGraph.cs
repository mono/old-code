using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

using Mono.Build;

namespace Monkeywrench.Compiler {

    public class XmlBackedGraph : IGraphState {

	XmlDocument doc;
	XPathNavigator nav;
	IWarningLogger log;

	public XmlBackedGraph (XmlDocument doc, IWarningLogger log)
	{
	    this.doc = doc;

	    nav = doc.CreateNavigator ();

	    this.log = log;
	}

	public XmlBackedGraph (string f, IWarningLogger log) 
	{
	    doc = new XmlDocument ();
	    doc.Load (f);

	    nav = doc.CreateNavigator ();

	    this.log = log;
	}

	// util

#if MAYBE_LATER
	object Eval (string format, params object[] args)
	{
	    string s = String.Format (format, args);
	    object o = nav.Evaluate (s);
	    //Console.WriteLine ("{0} -> .{1}. .{2}.", s, o, o.GetType ());
	    return o;
	}
#endif

	string SEval (string format, params object[] args)
	{
	    string s = String.Format (format, args);
	    string res = (string) nav.Evaluate ("string(" + s + ")");
	    //Console.WriteLine ("{0} -> \"{1}\"", s, res);
	    return res;
	}

	double NEval (string format, params object[] args)
	{
	    string s = String.Format (format, args);
	    double res = (double) nav.Evaluate ("number(" + s + ")");
	    //Console.WriteLine ("{0} -> {1}", s, res);
	    return res;
	}

	XmlNode SelectSingle (string format, params object[] args)
	{
	    string s = String.Format (format, args);
	    XmlNode res = doc.SelectSingleNode (s);
	    //Console.WriteLine ("{0} -> {1}", s, res);
	    return res;
	}

	XmlNodeList Select (string format, params object[] args)
	{
	    string s = String.Format (format, args);
	    XmlNodeList res = doc.SelectNodes (s);
	    //Console.WriteLine ("{0} -> {1}", s, res);
	    return res;
	}

	Result ReadResult (XmlNode item)
	{
	    string ignored;

	    XmlNodeReader xnr = new XmlNodeReader (item["result"]);
	    xnr.Read (); // no idea why this is needed
			       
	    return Result.ImportXml (xnr, out ignored, log);
	}

	// Providers

	string basiscache_val;
	short basiscache_id = -1;

	public string GetProviderBasis (short id)
	{
	    if (id == basiscache_id)
		return basiscache_val;

	    basiscache_id = id;
	    basiscache_val = SEval ("/mbuild-graph/providers/p[@id={0}]/@basis", id);

	    return basiscache_val;
	}

	public string GetProviderDeclarationLoc (short id)
	{
	    return SEval ("/mbuild-graph/providers/p[@id={0}]/@decl_loc", id);
	}

	public short GetProviderId (string basis)
	{
	    return (short) NEval ("/mbuild-graph/providers/p[@basis=\"{0}\"]/@id", basis);
	}

	public short NumProviders { 
	    get {
		return (short) ((short) NEval ("/mbuild-graph/providers/p[last()]/@id") + 1);
	    }
	}

	public int GetProviderTargetBound (short id)
	{
	    short ntargs = (short) NEval ("/mbuild-graph/providers/p[@id={0}]/@ntargs", id);

	    return (int) (id << 16) + ntargs;
	}

	// Targets

	public string GetTargetName (int tid)
	{
	    return SEval ("/mbuild-graph/targets/t[@id={0}]/@name", tid);
	}

	public int GetTargetId (string target)
	{
	    // This is gross, but can't think of a better way. Not
	    // actually used that much except for EvaluateDefaultHack
	    // I think?

	    int idx = target.LastIndexOf ('/');
	    string basis = target.Substring (0, idx + 1);
	    string basename = target.Substring (idx + 1);

	    short pid = GetProviderId (basis);
	    int bound = GetProviderTargetBound (pid);

	    for (int i = ((int) pid) << 16; i < bound; i++) {
		if (GetTargetName (i) == basename)
		    return i;
	    }

	    throw new ArgumentException ("No such target", target);
	}

	public Type GetTargetRuleType (int tid)
	{
	    string tname = SEval ("/mbuild-graph/targets/t[@id={0}]/@rule", tid);
	    return Type.GetType (tname);
	}

	int GetDepTarget (XmlNode n)
	{
	    return Int32.Parse (n.Attributes["id"].Value);
	}

	int GetDepArg (XmlNode n)
	{
	    return Int32.Parse (n.Attributes["arg"].Value);
	}

	public bool ApplyTargetDependencies (int tid, ArgCollector ac, IWarningLogger logger)
	{
	    XmlNodeList nl = Select ("/mbuild-graph/targets/t[@id={0}]/deps/*", tid);

	    bool ret;

	    ac.AddTargetName (GetTargetName (tid));

	    foreach (XmlNode n in nl) {
		switch (n.Name) {
		case "dor":
		    ret = ac.AddDefaultOrdered (ReadResult (n), log);
		    break;
		case "dot":
		    ret = ac.AddDefaultOrdered (GetDepTarget (n), log);
		    break;
		case "nr":
		    ret = ac.Add (GetDepArg (n), ReadResult (n), log);
		    break;
		case "nt":
		    ret = ac.Add (GetDepArg (n), GetDepTarget (n), log);
		    break;
		case "ur":
		    ret = ac.Add (ReadResult (n), log);
		    break;
		case "ut":
		    ac.Add (GetDepTarget (n));
		    ret = false;
		    break;
		case "Dr":
		    ret = ac.SetDefault (GetDepArg (n), ReadResult (n), log);
		    break;
		case "Dt":
		    ret = ac.SetDefault (GetDepArg (n), GetDepTarget (n), log);
		    break;
		default:
		    throw new Exception ("Unexpected dep element " + n.Name);
		}

		if (ret)
		    return true;
	    }

	    return false;
	}

	// Tags

	public int GetTagId (string tag)
	{
	    return (int) NEval ("/mbuild-graph/tags/tag[@name=\"{0}\"]/@id", tag);
	}

	public string GetTagName (int tag)
	{
	    return SEval ("/mbuild-graph/tags/tag[@id={0}]/@name", tag);
	}

	XmlNode tagcache_node;
	int tagcache_tid = -1;
	int tagcache_tag = -1;

	public object GetTargetTag (int tid, int tag)
	{
	    if (tid != tagcache_tid || tag != tagcache_tag) {
		tagcache_tid = tid;
		tagcache_tag = tag;
		tagcache_node = 
		    SelectSingle ("/mbuild-graph/targets/t[@id={0}]/tags/*[@id={1}]", tid, tag);
	    }

	    if (tagcache_node == null)
		return null;

	    if (tagcache_node.Name == "rt") {
		return ReadResult (tagcache_node);
	    } else if (tagcache_node.Name == "tt") {
		return Int32.Parse (tagcache_node.Attributes["target"].Value);
	    } else
		throw new Exception ("Unexpected tag node: " + tagcache_node.ToString ());
	}
	
	public IEnumerable<TargetTagInfo> GetTargetsWithTag (int tag)
	{
	    XmlNodeList nl = doc.SelectNodes (String.Format ("/mbuild-graph/targets/t/tags/*[@id={0}]", 
							     tag));

	    foreach (XmlNode n in nl) {
		TargetTagInfo tti;

		int targ = Int32.Parse (n.ParentNode.ParentNode.Attributes["id"].Value);

		if (n.Name == "rt")
		    tti = new TargetTagInfo (tag, targ, ReadResult (n));
		else if (n.Name == "tt") {
		    int val = Int32.Parse (n.Attributes["target"].Value);
		    tti = new TargetTagInfo (tag, targ, val);
		} else
		    throw new Exception ("Unexpected tag node: " + tagcache_node.ToString ());
		
		yield return tti;
	    }
	}

	// Dependent items

	IEnumerable<DependentItemInfo> ListDependents (XmlNodeList nl)
	{
	    foreach (XmlNode n in nl) {
		DependentItemInfo dii = new DependentItemInfo ();
		dii.Name = n.Attributes["name"].Value;
		dii.Fingerprint = (Fingerprint) ReadResult (n);
		yield return dii;
	    }
	}

	public IEnumerable<DependentItemInfo> GetDependentFiles ()
	{
	    return ListDependents (doc.SelectNodes ("/mbuild-graph/dependents/file"));
	}

	public IEnumerable<DependentItemInfo> GetDependentBundles ()
	{
	    return ListDependents (doc.SelectNodes ("/mbuild-graph/dependents/bundle"));
	}

	// Project Info

	ProjectInfo pinfo = null;

	public ProjectInfo GetProjectInfo ()
	{
	    if (pinfo != null)
		return pinfo;

	    pinfo = new ProjectInfo ();

	    pinfo.Name = SEval ("/mbuild-graph/project-info/@name");
	    pinfo.Version = SEval ("/mbuild-graph/project-info/@version");
	    pinfo.CompatCode = SEval ("/mbuild-graph/project-info/@compat-code");
	    pinfo.BuildfileName = SEval ("/mbuild-graph/project-info/@buildfile-name");

	    foreach (XmlNode n in doc.SelectNodes ("/mbuild-graph/project-info/ref")) {
		string text = n.Attributes["name"].Value;
		System.Reflection.AssemblyName aname = 
		    System.Reflection.AssemblyName.GetAssemblyName (text);
		//System.Reflection.AssemblyName aname = new System.Reflection.AssemblyName (text);
		pinfo.AddRef (aname);
	    }

	    return pinfo;
	}
    }
}
