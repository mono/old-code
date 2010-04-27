// MakeDictionaryRule.cs -- create a dictionary, for easier Buildfile writing

using System;
using Mono.Build;

namespace Monkeywrench {

    public class MakeDictionaryRule : Rule {

	public MakeDictionaryRule () : base () {}

	public override int NumArguments { get { return base.NumArguments + 2; } }

	public override void ListArguments (IArgInfoSink sink)
	{
	    base.ListArguments (sink);
	    int base_total = base.NumArguments;
	    sink.AddArg (base_total + 0, "keys", typeof (MBString), ArgFlags.Ordered | ArgFlags.Multi);
	    sink.AddArg (base_total + 1, "vals", typeof (Result), ArgFlags.Ordered | ArgFlags.Multi);
	}
	
	string[] keys;
	Result[] vals;
	
	public override void FetchArgValues (IArgValueSource source)
	{
	    base.FetchArgValues (source);
	    int base_total = base.NumArguments;
	    keys = Rule.AsArrayConv<string,MBString> (source.GetArgValue (base_total + 0));
	    vals = Rule.AsArray<Result> (source.GetArgValue (base_total + 1));
	}
	
	public override void ClearArgValues ()
	{
	    base.ClearArgValues ();
	    keys = null;
	    vals = null;
	}
	
	public override Type GeneralResultType
	{
	    get { return typeof (MBDictionary); }
	}

	public override Result Build (IBuildContext ctxt) {
	    MBDictionary dict = (MBDictionary) CreateResultObject ();

	    if (keys.Length != vals.Length) {
		ctxt.Logger.Error (1010, "Unequal number of keys and values for internal dictionary rule??", null);
		return null;
	    }
	    
	    for (int i = 0; i < keys.Length; i++)
		dict[keys[i]] = vals[i];
	    
	    return dict;
	}
	
	public override Fingerprint GetFingerprint (IBuildContext ctxt, Fingerprint cached) {
	    return GenericFingerprints.Null;
	}
    }
}
