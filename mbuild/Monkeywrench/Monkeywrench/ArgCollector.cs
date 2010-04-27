//
// ArgCollector.cs -- utility class for collecting arguments to rules.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using Mono.Build;

namespace Monkeywrench {

    public class ArgCollector : IArgInfoSink, IArgValueSource {

	struct ArgData {
	    public string Name;
	    public Type Type;
	    public object DefaultTo;
	    public ArgFlags Flags;
	    public List<Result> Values;
	    public List<Fingerprint> FPs;
	    
	    public void Init (string name, Type type, ArgFlags flags)
	    {
		Name = name;
		Type = type;
		DefaultTo = null;
		Flags = flags;
		Values = new List<Result> ();
		FPs = new List<Fingerprint> ();
	    }

	    public override string ToString ()
	    {
		string marker;
			
		if ((Flags & ArgFlags.Optional) != 0) {
		    if ((Flags & ArgFlags.Multi) != 0)
			marker = "*";
		    else
			marker = "?";
		} else {
		    if ((Flags & ArgFlags.Multi) != 0)
			marker = "+";
		    else
			marker = "";
		}

		string opts = "";
			
		if ((Flags & ArgFlags.Default) != 0)
		    opts += "d";
		if ((Flags & ArgFlags.Ordered) != 0)
		    opts += "o";
		if ((Flags & ArgFlags.DefaultOrdered) != 0)
		    opts += "D";

		string defstr = "";

		if (DefaultTo != null)
		    defstr = " (= " + DefaultTo + ")";
		
		return String.Format ("({0}){1}{2} {3}{4}", 
				      opts, Type, marker, Name, defstr);
	    }

	    public void AddResult (Result r, Fingerprint fp, int index_into_values) 
	    {
		if (index_into_values == -1) {
		    Values.Add (r);
		    FPs.Add (fp);
		} else {
		    Values[index_into_values] = r;
		    FPs[index_into_values] = fp;
		}
	    }

	    public void SetDefault (int dflt)
	    {
		// FIXME: SingleValue-friendly.
		DefaultTo = dflt;
	    }

	    public void SetDefault (Result dflt)
	    {
		DefaultTo = dflt;
	    }

	    public int AddTargetPlaceholder () 
	    {
		Values.Add (null);
		FPs.Add (null);

		// Return index_into_values for later updates
		return Values.Count - 1;
	    }

	    public bool Finalize (IBuildManager manager, IWarningLogger log)
	    {
		if (DefaultTo != null && Values.Count < 1) {
		    if (DefaultTo is Result)
			AddResult ((Result) DefaultTo, null, -1);
		    else {
			BuiltItem[] bi = manager.EvaluateTargets (new int[1] { (int) DefaultTo });
		
			if (bi == null || bi[0].Result == null)
			    return true;
		    
			AddResult (bi[0].Result, bi[0].ResultPrint, -1);
		    }
		}
	    
		if ((Flags & ArgFlags.Optional) == 0 && Values.Count < 1) {
		    log.Error (3015, String.Format ("Argument \"{0}\" is not optional but has no values", this), null);
		    return true;
		}
	    
		if ((Flags & ArgFlags.Multi) == 0 && Values.Count > 1) {
		    log.Error (3016, String.Format ("Argument \"{0}\" is not multiple-valued but has multiple values", this), null);
		    return true;
		}

		return false;
	    }

	    public Result[] CopyValues ()
	    {
		Result[] res = new Result[Values.Count];
		Values.CopyTo (res);
		return res;
	    }

	    public void CopyFingerprintData (IList<Fingerprint> list, IBuildContext ctxt) 
	    {
		if ((Flags & ArgFlags.Ordered) != 0) {
		    // This arg is ordered, go with the ordering
		    // we've been given.
		    
		    for (int i = 0; i < FPs.Count; i++) {
			if (FPs[i] == null)
			    list.Add (Values[i].GetFingerprint (ctxt, null));
			else
			    list.Add (FPs[i]);
		    }
		} else {
		    // this arg is unordered. Sort fp's as we go so a build file reorder
		    // doesn't affect the fp.
		    
		    SortedList fps = new SortedList ();
		    
		    for (int i = 0; i < FPs.Count; i++) {
			if (FPs[i] == null)
			    fps.Add (Values[i].GetFingerprint (ctxt, null), null);
			else
			    fps.Add (FPs[i], null);
		    }
		    
		    foreach (Fingerprint fp in fps.Keys)
			list.Add (fp);
		}
	    }
	}

	ArgData[] args;

	public ArgCollector (Rule rule)
	{
	    args = new ArgData[rule.NumArguments];
	    
	    rule.ListArguments (this);
	}

	bool TypeIs (Type left, Type right) {
	    if (left.Equals (right) || left.IsSubclassOf (right))
		return true;
	    return false;
	}
	
	// IArgInfoSink

	int default_ordered_id = -1;

	bool want_target_name = false;
	bool need_target_name = false;

	public void WantTargetName (bool required)
	{
	    want_target_name = true;

	    if (required)
		need_target_name = true;
	}

	public void AddArg (int aid, string name, Type type, ArgFlags flags) 
	{
	    if (name[0] == '.')
		throw new ArgumentException ("Unknown special argument " + name);
	    if (aid < 0 || aid >= args.Length)
		throw new ArgumentOutOfRangeException ("id");
	    if (args[aid].Name != null)
		throw new Exception ("Argument " + name + " already defined");

	    args[aid].Init (name, type, flags);

	    // FIXME: Need to check for two defaults specified for the
	    // same arg type.

	    if ((flags & ArgFlags.DefaultOrdered) != 0)
		default_ordered_id = aid;
	}

	// Finalization (not GC finalization, just 'done modifying') of the collector.

	bool args_finalized = false;

	public bool ArgsFinalized { get { return args_finalized; } }
	
	void CheckNotEvaluated () 
	{
	    if (args_finalized)
		throw new InvalidOperationException ("Cannot perform this operation after evaluating arguments.");
	}
	
	void CheckEvaluated () 
	{
	    if (!args_finalized)
		throw new InvalidOperationException ("Cannot perform this operation before evaluating arguments.");
	}
	
	// Adding values and deferred target arg eval

	struct TargetArg {
	    public int aid;
	    public int target;
	    public int index_into_values;
	    
	    public TargetArg (int aid, int target, int iiv) {
		this.aid = aid;
		this.target = target;
		this.index_into_values = iiv;
	    }
	    
	    public TargetArg (int aid, int target) : this (aid, target, -1) {
	    }

	    public TargetArg (int target) : this (-1, target, -1) {
	    }
	}

	List<TargetArg> deferred = new List<TargetArg> ();

	void AddNamed (int aid, int target) 
	{
	    CheckNotEvaluated ();

	    // Add a placeholder into the values arraylist. This way
	    // we preserve order which will be needed if the arg is
	    // marked as Ordered.

	    int index_into_values = args[aid].AddTargetPlaceholder ();

	    deferred.Add (new TargetArg (aid, target, index_into_values));
	}

	bool AddNamed (int aid, Result r, Fingerprint fp, 
		       int index_into_values, IWarningLogger logger) 
	{
	    if (aid < 0 || aid >= args.Length) {
		string s = String.Format ("Trying to add {0} to nonexistant " +
					  "argument ID {1}.", r, aid);
		logger.Error (2042, s, r.ToString ());
		return true;
	    }

	    Result subres = CompositeResult.FindCompatible (r, args[aid].Type);

	    if (subres == null) {
		string err = String.Format ("Argument value should be of type {0}, but " +
					    "its type is {1}", args[aid].Type, r.GetType ());
		logger.Error (2036, err, r.ToString ());
		return true;
	    }

	    if (subres == r)
		args[aid].AddResult (r, fp, index_into_values);
	    else
		// FIXME: the FP is really invalid, right?
		args[aid].AddResult (r, null, index_into_values);

	    return false;
	}
	
	public bool Add (Result r, Fingerprint fp, IWarningLogger logger) 
	{
	    if (r == null)
		throw new ArgumentNullException ();
	    
	    Type t = r.GetType ();
	    List<int> possible_args = new List<int> ();
	    Type best_match = typeof (Result);
	    
	    for (int i = 0; i < args.Length; i++) {
		Type atype = args[i].Type;
		
		// Cannot add an unnamed arg to an ordered arg
		if ((args[i].Flags & ArgFlags.Ordered) != 0)
		    continue;
		
		// Prune out the egregiously wrong arguments (not even a superclass of the result)
		if (! TypeIs (t, atype))
		    continue;
		
		// Prune out those that have been bettered
		if (! TypeIs (atype, best_match))
		    continue;
		
		// If we've narrowed the type further, we don't want any of the
		// previous vaguer matches.
		if (atype.IsSubclassOf (best_match)) {
		    possible_args.Clear ();
		    best_match = atype;
		}
		     
		possible_args.Add (i);
	    }
	    
	    //Console.WriteLine ("Finished with {0} possible arguments", possible_args.Count);
	    
	    if (possible_args.Count == 1) {
		args[possible_args[0]].AddResult (r, fp, -1);
		return false;
	    }
	    
	    if (possible_args.Count > 0) {
		
		// Several possible choices. Check for a default
		foreach (int aid in possible_args) {
		    if ((args[aid].Flags & ArgFlags.Default) != 0) {
			args[aid].AddResult (r, fp, -1);
			return false;
		    }
		}
		
		// No dice. Ambiguity not tolerated. Ah, computers.
		
		StringBuilder sb = new StringBuilder ();
		sb.AppendFormat ("Ambiguous dependency of type {0} could " +
				 "be one of these arguments:", t);
		foreach (int aid in possible_args)
		    sb.AppendFormat (" {0}", args[aid].Name);
		
		logger.Error (2035, sb.ToString (), r.ToString ());
		return true;
	    }

	    // Maybe this is a composite result, and it has a default?
	    // We recurse here, so we tunnel through the composites
	    // sequentially. It's correct to check at every step, rather
	    // than calling FindCompatible, since we don't know what 
	    // type we're looking for.
	    
	    if (r is CompositeResult) {
		CompositeResult cr = (CompositeResult) r;
		
		if (cr.HasDefault) {
		    // See note above about losing FP info in composite results.
		    // this case happens when we are guessing the arg; te 
		    //logger.Warning (9999, "LOSING FINGERPRINT INFO in AC (2)", r.ToString ());
		    
		    if (Add (cr.Default, null, logger) == false)
			return false;
		    
		    // if that didn't work, continue
		    // and give a warning about the container
		    // Result, not the default.
		}
	    }
	    
	    // Bummer.
	    
	    string s = String.Format ("Dependency {0} of type {1} isn't compatible " +
				      "with any defined arguments.", r, t);
	    logger.Error (2034, s, null);
	    return true;
	}

	public bool Add (Result r, IWarningLogger logger) 
	{
	    return Add (r, null, logger);
	}
	
	public void Add (int target) 
	{
	    deferred.Add (new TargetArg (target));
	}
	
	public bool Add (int aid, Result r, Fingerprint fp, IWarningLogger logger) 
	{
	    if (r == null)
		throw new ArgumentNullException ();
	    
	    return AddNamed (aid, r, fp, -1, logger);
	}
	
	public bool Add (int aid, Result r, IWarningLogger logger) 
	{
	    return Add (aid, r, null, logger);
	}
	
	public bool Add (int aid, int target, IWarningLogger logger) 
	{
	    if (aid < 0 || aid > args.Length) {
		string s = String.Format ("Trying to add target #{0} to invalid " +
					  "argument ID #{1}.", target, aid);
		logger.Error (2042, s, null);
		return true;
	    }
	    
	    AddNamed (aid, target);
	    return false;
	}

	public bool AddDefaultOrdered (Result r, Fingerprint fp, IWarningLogger logger) 
	{
	    if (r == null)
		throw new ArgumentNullException ();
	    
	    if (default_ordered_id < 0) {
		logger.Error (2037, "Trying to add a dependency to the default " + 
			      "ordered argument, but no default ordered argument is defined",
			      r.ToString ());
		return true;
	    }
	    
	    return AddNamed (default_ordered_id, r, fp, -1, logger);
	}
	
	public bool AddDefaultOrdered (Result r, IWarningLogger logger) 
	{
	    return AddDefaultOrdered (r, null, logger);
	}
	
	public bool AddDefaultOrdered (int target, IWarningLogger logger) 
	{
	    if (default_ordered_id < 0) {
		logger.Error (2037, "Trying to add a dependency to the default " + 
			      "ordered argument, but no default ordered argument is defined",
			      target.ToString ());
		return true;
	    }
	    
	    return Add (default_ordered_id, target, logger);
	}
	
	public bool SetDefault (int aid, Result r, IWarningLogger logger) 
	{
	    if (aid < 0 || aid >= args.Length) {
		string s = String.Format ("Trying to set default {0} of nonexistant " +
					  "argument ID {1}.", r, aid);
		logger.Error (2042, s, r.ToString ());
		return true;
	    }

	    args[aid].SetDefault (r);
	    return false;
	}
	
	public bool SetDefault (int aid, int target, IWarningLogger logger) 
	{
	    if (aid < 0 || aid >= args.Length) {
		string s = String.Format ("Trying to set default {0} of nonexistant " +
					  "argument ID {1}.", target, aid);
		logger.Error (2042, s, target.ToString ());
		return true;
	    }

	    args[aid].SetDefault (target);
	    return false;
	}

	string target_name;

	public void AddTargetName (string name)
	{
	    target_name = name;
	}
	
	// evaluation
	
	public bool FinalizeArgs (IBuildManager manager, IWarningLogger logger) 
	{
	    int i;
	    
	    // evaluate deferred args
	    
	    int[] ids = new int[deferred.Count];
	    for (i = 0; i < deferred.Count; i++)
		ids[i] = deferred[i].target;
	    
	    BuiltItem[] bis = manager.EvaluateTargets (ids);
	    if (bis == null)
		return true;
	    
	    for (i = 0; i < deferred.Count; i++) {
		int aid = deferred[i].aid;
		
		if (aid < 0) {
		    if (Add (bis[i].Result, bis[i].ResultPrint, logger))
			return true;
		} else {
		    if (AddNamed (aid, bis[i].Result, bis[i].ResultPrint,
				  deferred[i].index_into_values, logger))
			return true;
		}
	    }
	    
	    deferred.Clear ();
	    
	    // check counts and types, apply defaults if needed
	    
	    for (i = 0; i < args.Length; i++) {
		if (args[i].Finalize (manager, logger))
		    return true;
	    }
	    
	    // Check target

	    if (need_target_name && target_name == null) {
		logger.Error (3015, "Rule needs target name but it has not been set", null);
		return true;
	    }

	    // all done
	    
	    args_finalized = true;
	    return false;
	}
	
	// IArgValueSource

	public string GetTargetName ()
	{
	    if (!want_target_name)
		throw new InvalidOperationException ("Rule asks for target name but didn't " +
						     "call WantTargetName!");
	    return target_name;
	}

	public Result[] GetArgValue (int aid)
	{
	    if (aid < 0 || aid > args.Length)
		throw new ArgumentOutOfRangeException ("Invalid argument ID " + aid);
	    
	    CheckEvaluated ();
	    
	    return args[aid].CopyValues ();
	}
	
	// Fingerprinting is a little weird.. Note that ArgCollector
	// is not a traditional IFingerprintable, because it doesn't
	// honor cached fp's in a normal way.
	
	internal void CopyFingerprintData (IList<Fingerprint> list, IBuildContext ctxt) 
	{
	    CheckEvaluated ();
	    
	    // First, sort the arguments by name

	    SortedList names = new SortedList ();

	    for (int aid = 0; aid < args.Length; aid++)
		names.Add (args[aid].Name, aid);

	    // Now add the results in each arg.

	    foreach (int aid in names.Values)
		args[aid].CopyFingerprintData (list, ctxt);
	}
    }
}
