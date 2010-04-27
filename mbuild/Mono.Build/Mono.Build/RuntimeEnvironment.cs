using System;

namespace Mono.Build {

    public static class RuntimeEnvironment {

	// Learn something from autoconf: Do not add a 
	// 'RunningOnUnix' property and try and guess things
	// based only on that. We should test for specific
	// features, e.g., "FilesystemDistinguishesCase".
	//
	// Also, these should all be detected at runtime if
	// at all possible, rather than compile-time. (Note
	// name of the class.) 

	// Returns whether we can use the routines provided
	// in Mono.Unix. Of course, we still need to link against
	// the library ... And right now, this is just testing
	// whether we're on a Unix system, which is not the same
	// thing as what we claim to be testing.

	public static bool MonoUnixSupported {
	    get {
		if (((int) Environment.OSVersion.Platform) != 0)
		    return true;

		// This was added in .Net 2.0
		if (Environment.OSVersion.Platform == PlatformID.Unix)
		    return true;

		return false;
	    }
	}

	public static bool RunningOnMono {
	    get {
		return (Type.GetType ("System.MonoType", false) != null);
	    }
	}
    }
}
