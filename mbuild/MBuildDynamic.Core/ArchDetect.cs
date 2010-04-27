using System;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;

using Mono.Build;
using Mono.Build.RuleLib;

// based on automake-1.9.2 config.guess timestamp 2005-07-08

namespace MBuildDynamic.Core.Native {

    internal class ArchDetect {
	ArchDetect () {}

	// FIXME: It is a bug that we need a uname program to detect the 
	// architecture. We should be able to run on Windows machines that
	// don't have cygwin type tools installed.

	//public static string Detect (BinaryInfo uname, IBuildContext ctxt)
	public static string Detect (IBuildContext ctxt)
	{
	    // config.guess lines 132 - 134: skip. Tough for "Pyramid OSx when
	    // run in the BSD universe"

	    //string uname_machine = Launcher.GetToolStdout (uname, "-m", false, ctxt);
	    //string uname_release = Launcher.GetToolStdout (uname, "-r", false, ctxt);
	    //string uname_system  = Launcher.GetToolStdout (uname, "-s", false, ctxt);
	    //string uname_version = Launcher.GetToolStdout (uname, "-v", false, ctxt);

	    // config.guess lines 143 - 1264
	    // Leave this for later. Not exactly a priority

	    // FIXME. Duh.
	    return "i386-pc-linux-gnu";
	}
    }
}
