//
// CloneRule.cs -- a rule that clones its argument
//

using System;
using System.Reflection;
using System.Collections;

using Mono.Build;

namespace Mono.Build.RuleLib {

    public class CloneRule : Rule {
		
	public CloneRule () : base () 
	{
	    specific_varies = true;
	}

	public override int NumArguments
	{
	    get { return base.NumArguments + 1; }
	}

	public override void ListArguments (IArgInfoSink sink)
	{
	    sink.AddArg (base.NumArguments, "value", typeof (Result), ArgFlags.Standard);
	}

	protected Result value;

	public override void FetchArgValues (IArgValueSource source)
	{
	    value = AsSingle<Result> (source.GetArgValue (base.NumArguments));
	}

	public override Type GeneralResultType
	{
	    get { return typeof (Result); } 
	}

	public override Type SpecificResultType
	{
	    get { return value.GetType (); }
	}

	public override Result Build (IBuildContext ctxt) 
	{
	    // A more orthodox subclass might call Rule.CreateResultObject
	    // and do some value.CopyValueTo (result), but this is simpler.

	    return (Result) value.Clone ();
	}

	public override Fingerprint GetFingerprint (IBuildContext ctxt, Fingerprint cached) 
	{
	    return GenericFingerprints.Null;
	}
    }
}
