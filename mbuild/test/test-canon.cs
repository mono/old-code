using System;
using Mono.Build;

class TestCanonicalize {

    public static int Main (string[] args)
    {
	if (args.Length < 1 || args.Length > 2) {
	    Console.Error.WriteLine ("Usage: test-canon.exe [targetname] (basis)");
	    return 1;
	}

	try {
	    if (args.Length == 1)
		Console.WriteLine ("{0}", StrUtils.CanonicalizeTarget (args[0], null));
	    else
		Console.WriteLine ("{0}", StrUtils.CanonicalizeTarget (args[0], args[1]));
	} catch (Exception e) {
	    Console.Error.WriteLine ("Exception: {0}", e.Message);
	    return 1;
	}

	return 0;
    }
}
