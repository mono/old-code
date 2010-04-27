using System;

using Mono.Build;

namespace Mono.Build.RuleLib {

    public enum ArchKind {
	Build,
	Host,
	Target
    }

    [Serializable]
    public class ArchKindResult : EnumResult<ArchKind> {

	public ArchKindResult () {}
	
	public ArchKindResult (ArchKind k)
	{
	    Value = k;
	}
	
	public static void AssertCanExecute (ArchKind k)
	{
	    if (k == ArchKind.Build)
		return;

	    string s = String.Format ("Cannot execute a program aimed at the {0} " +
				      "rather than Build architecture", k);

	    throw new InvalidOperationException (s);
	}

	public static void AssertCanExecute (ArchKindResult akr)
	{
	    AssertCanExecute (akr.Value);
	}
    }
}

