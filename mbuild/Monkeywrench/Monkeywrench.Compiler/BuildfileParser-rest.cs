using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

using Mono.Build;
using Mono.Build.Bundling;
using Mono.Build.RuleLib;

namespace Monkeywrench.Compiler {

    public partial class BuildfileParser {

	BuildfileTokenizer lexer;

	string location_base;
	string topsrc;
	string resource_subdir;
	WrenchProvider wp;
	IWarningLogger log;

	NameLookupContext cur_nlc;

	public BuildfileParser (StreamReader reader, string topsrc, string resource_subdir, 
				string location_base, WrenchProvider wp, IWarningLogger log)
	{
	    lexer = new BuildfileTokenizer (reader);

	    this.topsrc = topsrc;
	    this.resource_subdir = resource_subdir;
	    this.location_base = location_base;
	    this.wp = wp;
	    this.log = log;

	    cur_nlc = wp.NameContext;
	}

	public static BuildfileParser CreateForFile (string topsrc, string srcrel,
						     WrenchProvider wp, IWarningLogger log)
	{
	    string file = Path.Combine (topsrc, srcrel);
	    string resdir = Path.GetDirectoryName (srcrel);

	    if (resdir.Length == 0)
		resdir = ".";

	    FileStream fs = new FileStream (file, FileMode.Open, FileAccess.Read);
	    BufferedStream bs = new BufferedStream (fs);
	    StreamReader reader = new StreamReader (bs);

	    BuildfileParser p = new BuildfileParser (reader, topsrc, resdir, 
						     file, wp, log);
	    p.AddResourceFile (Path.GetFileName (srcrel));

	    return p;
	}

	// Execution

	static int yacc_verbose_flag = 1;

	public static bool DebugParser = false;

	public int Parse ()
	{
	    int nerr_orig = log.NumErrors;

	    try {
		if (DebugParser)
		    yyparse (lexer, new yydebug.yyDebugSimple ());
		else
		    yyparse (lexer);
		lexer.Cleanup ();
	    } catch (yyParser.yyException yyex) {
		Error (2038, "Parse error in build file", yyex.Message);
	    } catch (Exception e) {
		Error (1001, "Exception while parsing build file.", e.ToString ());
	    }
	    
	    return log.NumErrors - nerr_orig;
	}

	// Misc

	public BuildfileTokenizer Lexer {
	    get { return lexer; }
	}  

	// Helpers

	class LinkList {
	    public SingleValue<string> item;
	    public LinkList prev;

	    public LinkList (SingleValue<string> item) : this (item, null) {}

	    public LinkList (SingleValue<string> item, LinkList prev) 
	    {
		this.item = item;
		this.prev = prev;
	    }

	    public LinkList JoinPrev (LinkList prev)
	    {
		this.prev = prev;
		return this;
	    }
	}

	static LinkList Reverse (LinkList l) 
	{
	    // Algorithm copied from g_slist_reverse.
	    // Reverses in-place
	    
	    LinkList prev = null;
	    
	    while (l != null) {
		LinkList next = l.prev;
		l.prev = prev;
		
		prev = l;
		l = next;
	    }
	    
	    return prev;
	}

	static string CanonicalizeBasis (string basis)
	{
	    if (basis[basis.Length - 1] == '/')
		return basis;

	    return basis + '/';
	}

	// Logging with location

	string Location 
	{ 
	    get {
		return location_base + ':' + lexer.LineNum.ToString ();
	    }
	}

	void Warning (int category, string text, string detail) 
	{
	    log.PushLocation (Location);
	    log.Warning (category, text, detail);
	    log.PopLocation ();
	}

	void WarningV (int category, string fmt, params object[] items) 
	{
	    Warning (category, String.Format (fmt, items), null);
	}

	void Error (int category, string text, string detail) 
	{
	    log.PushLocation (Location);
	    log.Error (category, text, detail);
	    log.PopLocation ();
	}

	void ErrorV (int category, string fmt, params object[] items) 
	{
	    Error (category, String.Format (fmt, items), null);
	}

	// Resource files

	int nresource = 0;

	const string ResourceTarget = ".monkeywrench_resource_";

	string AddResourceFile (string name) 
	{
	    string srcrel = Path.Combine (resource_subdir, name);
	    string path = Path.Combine (topsrc, srcrel);

	    ((GraphBuilder) wp.Owner).AddDependentFile (srcrel, path);

	    string target = ResourceTarget + nresource.ToString ();
	    nresource++;

	    TargetBuilder tb = wp.DefineTarget (target, log);
	    tb.RuleType = typeof (SourcefileRule);
	    tb.AddDep (new MBString (name)); // override the name

	    return path;
	}

	// Response files

	void LoadResponseFile (TargetBuilder tb, string arg, string file) 
	{
	    string path = AddResourceFile (file);

	    using (Stream stream = File.OpenRead (path)) {
		StreamReader reader = new StreamReader (stream);

		string line;

		while ((line = reader.ReadLine ()) != null) {
		    // FIXME: enforce line chomping? ... sure!
		    line = line.Trim ();

		    if (line == "" || line[0] == '#')
			continue;

		    if (arg != null)
			tb.AddDep (arg, line);
		    else
			tb.AddDep (line);
		}
	    }
	}

	// Using statements

	void UseNamespace (string name)
	{
	    if (cur_nlc.UseIdent (name, resource_subdir, log))
		// shouldn't happen -- cur_nlc doesn't have its
		// project set yet, so we can't check if all the namespace
		// references are actually valid.
		throw new Exception ("Unexpected early namespace error???");
	}
	
	// Project Info

	ProjectInfo pinfo;

	public ProjectInfo PInfo { get { return pinfo; } }

	void StartProjectInfo ()
	{
	    if (pinfo != null)
		ErrorV (9999, "Cannot have two project [] statements in a Buildfile");
	    else
		pinfo = new ProjectInfo ();
	}

	void AddProjectProperty (string name, string val)
	{
	    switch (name) {
	    case "name":
		pinfo.Name = val;
		break;
	    case "version":
		pinfo.Version = val;
		break;
	    case "compat-code":
		pinfo.CompatCode = val;
		break;
	    default:
		WarningV (2004, "Unknown project property `{0}' (value: `{1}').", name, val);
		break;
	    }
	}

	void AddUnversionedReference (string name)
	{
	    pinfo.AddRef (BundleManager.MakeName (name));
	}

	void AddVersionedReference (string name, string vers)
	{
	    pinfo.AddRef (BundleManager.MakeName (name, vers));
	}

	// Inside statements

	public class InsideInfo {
	    List<string> bases;
	    NameLookupContext nlc;

	    public InsideInfo ()
	    {
		nlc = new NameLookupContext ();
		bases = new List<string> ();
	    }

	    public IEnumerable<string> Bases { get { return bases; } }

	    public NameLookupContext Context { get { return nlc; } }

	    public void Add (string basis)
	    {
		bases.Add (BuildfileParser.CanonicalizeBasis (basis));
	    }
	}

	List<InsideInfo> insides = new List<InsideInfo> ();

	public IEnumerable<InsideInfo> Insides { get { return insides; } }

	InsideInfo cur_inside;

	void StartInside ()
	{
	    cur_inside = new InsideInfo ();
	    insides.Add (cur_inside);

	    cur_nlc = cur_inside.Context;
	}

	void AddInsideBasis (string basis)
	{
	    cur_inside.Add (basis);
	}

	void FinishInside ()
	{
	    cur_nlc = wp.NameContext;
	}

	// subdirs

	List<string> subdirs = new List<string> ();

	public IEnumerable<string> Subdirs { get { return subdirs; } }

	void AddSubdir (string dir)
	{
	    if (dir.IndexOfAny (Path.GetInvalidPathChars ()) != -1) {
		ErrorV (2002, "Item `{0}' in subdirs statement contains invalid " +
			"path characters.", dir);
		return;
	    }

	    subdirs.Add (CanonicalizeBasis (dir));
	}

	// Loads

	Dictionary<string,string> loads;

	public IDictionary<string,string> ManualLoads { get { return loads; } }

	void DoLoadStatement (string file, string basis)
	{
	    if (basis[0] == '/') {
		ErrorV (2011, "Basis `{0}' (declared in file `{1}') must be relative " +
			"to the declaring basis.", basis, file);
		return;
	    }

	    if (loads == null)
		loads = new Dictionary<string,string> ();

	    loads [basis + "/"] = file;
	}

	// Target definitions

	WrenchTarget cur_targ = null;

	void StartTarget (string name)
	{
	    if (CheckTargetName (name))
		return;

	    cur_targ = (WrenchTarget) wp.DefineTarget (name, log);
	    // FIXME: if the fails we log an error but the rest of the code
	    // will nullref almost instantly. Gets the job done, but ugly.
	    cur_arg_name = null;
	}

	void FinishTarget ()
	{
	    cur_targ = null;
	    cur_arg_name = null;
	}

	void SetTargetTemplateName (string tmpl_name)
	{
	    cur_targ.TemplateName = tmpl_name;
	}

	string cur_arg_name;

	void AddDepCurrent (SingleValue<string> sv)
	{
	    if (cur_arg_name == null)
		cur_targ.AddDep (sv);
	    else
		cur_targ.AddDep (cur_arg_name, sv);
	}

	void ApplyOrderedDeps (LinkList list)
	{
	    list = Reverse (list);

	    if (cur_arg_name == null)
		for (; list != null; list = list.prev)
		    cur_targ.AddDefaultOrdered (list.item);
	    else
		for (; list != null; list = list.prev)
		    cur_targ.AddDep (cur_arg_name, list.item);
	}

	void AddResponseDependencies (string responsefile)
	{
	    LoadResponseFile (cur_targ, cur_arg_name, responsefile);
	}

	public static readonly char[] IllegalTargetChars = new char[] { '+', ':', '/' };

	bool CheckTargetName (string name) 
	{
	    if (name[0] == '.') {
		ErrorV (2003, "User-defined target name `{0}' may not start with a period", name);
		return true;
	    }

	    int idx = name.IndexOfAny (IllegalTargetChars);

	    if (idx != -1) {
		ErrorV (2008, "User-defined target name '{0}' may not contain the character `{1}'.",
			name, name[idx]);
		return true;
	    }

	    if (!wp.CanDefineTarget (name)) {
		ErrorV (2009, "Target `{0}' already defined.", name);
		return true;
	    }
	    
	    return false;
	}

	void SetTargetAsValue (SingleValue<string> val)
	{
	    cur_targ.RuleType = typeof (CloneRule);
	    cur_targ.AddDep (val);
	}

	// Template multi-apply

	string cur_apply_name = null;

	void ApplyTemplate (string name)
	{
	    if (CheckTargetName (name))
		return;
	    if (cur_apply_name == null)
		throw ExHelp.App ("Trying to apply template without having set template name??");

	    TargetBuilder tb = wp.DefineTarget (name, log);
	    tb.TemplateName = cur_apply_name;
	}

	// Constructed targets

	int nconstructed = 0;

	class ConstructedInfo {
	    public TargetBuilder tb;
	    public ConstructedInfo parent;

	    const string ConstructedTarget = ".implicit_";

	    string GenTargetName (BuildfileParser bp)
	    {
		return String.Format (".implicit_{0}_line_{1}", bp.nconstructed++,
				      bp.lexer.LineNum);
	    }

	    public ConstructedInfo (ConstructedInfo parent)
	    {
		this.parent = parent;
	    }

	    public bool Define (BuildfileParser bp, IWarningLogger log)
	    {
		this.tb = bp.wp.DefineTarget (GenTargetName (bp), log);

		return tb == null;
	    }
	}

	ConstructedInfo cur_constructed = null;

	void StartConstructed ()
	{
	    cur_constructed = new ConstructedInfo (cur_constructed);

	    if (cur_constructed.Define (this, log))
		// FIXME
		throw new Exception ("Error defining implicit target???");
	}

	void FinishConstructed ()
	{
	    cur_constructed = cur_constructed.parent;
	}

	// Dictionaries

	void SetDictionaryRule ()
	{
	    cur_constructed.tb.RuleType = typeof (MakeDictionaryRule);
	}

	void AddDictionaryValue (TargetBuilder tb, string key, SingleValue<string> val)
	{
	    cur_constructed.tb.AddDep ("keys", new MBString (key));
	    cur_constructed.tb.AddDep ("vals", val);
	}

	// Boolean expression helping

	void SetBoolOpsRule ()
	{
	    cur_constructed.tb.TemplateName = "Core.BooleanHelper";
	}

	void DecodeBoolOps (BoolOps ops)
	{
	    ops.DecodeInto (cur_constructed.tb, log);
	}

	enum OpCode {
	    Not = -1,
	    And = -2,
	    Or = -3
	}

	class BoolOps {
	    public int[] commands;
	    public string[] targets;

	    private BoolOps () {}

	    public BoolOps (string targ) 
	    {
		commands = new int[1] { 0 };
		targets = new string[1] { targ };
	    }

	    public BoolOps (OpCode unary_op, string targ) 
	    {
		if (unary_op != OpCode.Not)
		    throw new InvalidOperationException ();

		commands = new int[2] { 0, (int) unary_op };
		targets = new string[1] { targ };
	    }

	    public BoolOps CombineUnary (OpCode unary_op)
	    {
		if (unary_op != OpCode.Not)
		    throw new InvalidOperationException ();

		BoolOps res = new BoolOps ();
		res.targets = targets;

		res.commands = new int[commands.Length + 1];
		commands.CopyTo (res.commands, 0);
		res.commands[commands.Length] = (int) unary_op;

		return res;
	    }

	    public BoolOps CombineBinary (OpCode op, BoolOps right)
	    {
		if (op == OpCode.Not)
		    throw new InvalidOperationException ();

		Hashtable targmap = new Hashtable ();
		ArrayList targlist = new ArrayList ();
		int targindex = 0;
		int lclen = commands.Length, rclen = right.commands.Length;
		BoolOps res = new BoolOps ();

		int idx = 0;
		res.commands = new int[lclen + rclen + 1];

		for (int i = 0; i < lclen; i++) {
		    if (commands[i] < 0) {
			res.commands[idx++] = commands[i];
			continue;
		    }

		    string targ = targets[commands[i]];

		    if (targmap.Contains (targ))
			res.commands[idx++] = (int) targmap[targ];
		    else {
			targmap[targ] = targindex;
			targlist.Add (targ);
			res.commands[idx++] = targindex++;
		    }
		}
		
		for (int i = 0; i < rclen; i++) {
		    if (right.commands[i] < 0) {
			res.commands[idx++] = right.commands[i];
			continue;
		    }
		    
		    string targ = right.targets[right.commands[i]];
		    
		    if (targmap.Contains (targ))
			res.commands[idx++] = (int) targmap[targ];
		    else {
			targmap[targ] = targindex;
			targlist.Add (targ);
			res.commands[idx++] = targindex++;
		    }
		}
		
		res.commands[idx] = (int) op;
		
		res.targets = new string[targlist.Count];
		targlist.CopyTo (res.targets);
		
		return res;
	    }

	    public void DecodeInto (TargetBuilder tb, IWarningLogger log)
	    {
		// This limitation is only because I am lazy and want one character
		// per BooleanHelper command.

		if (targets.Length > 10)
		    throw new InvalidOperationException ("Limited to 10 or fewer boolean operands");

		for (int i = 0; i < targets.Length; i++) 
		    tb.AddDefaultOrdered (targets[i]);

		StringBuilder sb = new StringBuilder ();

		for (int i = 0; i < commands.Length; i++) {
		    switch (commands[i]) {
		    case (int) OpCode.Not: sb.Append ('!'); break;
		    case (int) OpCode.And: sb.Append ('&'); break;
		    case (int) OpCode.Or: sb.Append ('|'); break;
		    case 0: sb.Append ('0'); break;
		    case 1: sb.Append ('1'); break;
		    case 2: sb.Append ('2'); break;
		    case 3: sb.Append ('3'); break;
		    case 4: sb.Append ('4'); break;
		    case 5: sb.Append ('5'); break;
		    case 6: sb.Append ('6'); break;
		    case 7: sb.Append ('7'); break;
		    case 8: sb.Append ('8'); break;
		    case 9: sb.Append ('9'); break;
		    default:
			throw new ArgumentException ("Don't know how to handle boolean ops command " +
						     commands[i].ToString ());
		    }
		}

		tb.AddDep (new MBString (sb.ToString ()));
	    }
	}

	// Formatted string

	void SetupFormat (string format, LinkList list)
	{
	    cur_constructed.tb.TemplateName = "Core.FormatHelper";
	    cur_constructed.tb.AddDep ("format", new MBString (format));

	    list = Reverse (list);

	    for (; list != null; list = list.prev)
		cur_constructed.tb.AddDefaultOrdered (list.item);
	}

	// Conditionals

	void SetupConditional (SingleValue<string> cond, SingleValue<string> ifyes, 
			       SingleValue<string> ifno)
	{
	    cur_constructed.tb.TemplateName = "Core.Switch";

	    cur_constructed.tb.AddDep ("cases", cond);
	    cur_constructed.tb.AddDefaultOrdered (ifyes);
	    cur_constructed.tb.AddDep ("cases", MBBool.True);
	    cur_constructed.tb.AddDefaultOrdered (ifno);
	}

    }
}
