// OutputFileRule.cs -- a rule that has a definite "output file";
// it defaults to .target, but can be overridden

using System;

using Mono.Build;

namespace Mono.Build.RuleLib {

    public abstract class OutputFileRule : Rule {
	public OutputFileRule () : base () { }

	public override int NumArguments { get { return base.NumArguments + 1; } }

	protected virtual string OutputArgName {
	    get { return "output"; }
	}

	public override void ListArguments (IArgInfoSink sink)
	{
	    sink.WantTargetName (false);
	    sink.AddArg (base.NumArguments, OutputArgName, typeof (MBString), ArgFlags.Optional);
	}

	protected string target_name;
	protected string output;

	public override void FetchArgValues (IArgValueSource source)
	{
	    target_name = source.GetTargetName ();
	    output = AsOptionalRef<string,MBString> (source.GetArgValue (base.NumArguments));
	}

	public string GetOutputName (IBuildContext ctxt) 
	{
	    if (output != null)
		return output;

	    if (target_name == null) {
		ctxt.Logger.Error (2030, "No output filename specified", null);
		return null;
	    }

	    return target_name;
	}
    }
}
