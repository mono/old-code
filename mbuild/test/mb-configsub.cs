using System;
using MBuildDynamic.Core.Native;

class X {

    public static int Main (string[] args)
    {
	Architecture arch = new Architecture ();

	if (args.Length != 1) {
	    Console.Error.WriteLine ("This program takes exactly one argument.");
	    return 1;
	}

	try {
	    arch.SetFromString (args[0]);
	} catch (Exception e) {
	    Console.Error.WriteLine ("Exception parsing input string \"{0}\": {1}",
				     args[0], e);
	    return 1;
	}

	Console.WriteLine ("{0}", arch.ToString ());
	return 0;
    }
}
