//
// SubstSink.cs -- a mini stream sink that performs variable substitutions
// in a variety of common styles
//

using System;
using System.Text.RegularExpressions;
using System.Collections;

using Mono.Build;

namespace Mono.Build.RuleLib {

	// todo: allow fancy shell styles such as '${foo:=fasfa}'

	[Flags]
	public enum SubstStyles {
		Autoconf = 1 << 0,
		PlainShell = 1 << 1,
		BraceShell = 1 << 2,
		Makefile = 1 << 3,
		Dos = 1 << 4,

		AllShell = PlainShell | BraceShell,
		All = Autoconf | AllShell | Makefile | Dos
	}

	public class SubstSink : SedSink {
		SubstStyles styles;

		public SubstSink (IMiniStreamSink dest, SubstStyles styles) : base (dest) {
			if ((int) styles == 0)
				throw new ArgumentException ("Can't have empty substitution style");

			this.styles = styles;
		}

		public SubstSink (IMiniStreamSink dest) : this (dest, SubstStyles.Autoconf) {}

		public SubstSink (SubstStyles styles) : this (null, styles) {}

		public SubstSink () : this (null, SubstStyles.Autoconf) {}

		// methods

		public void AddSubst (string key, string val) {
			if ((styles & SubstStyles.Autoconf) != 0)
				Add ('@' + key + '@', val);

			if ((styles & SubstStyles.PlainShell) != 0)
				Add ('$' + key, val);

			if ((styles & SubstStyles.BraceShell) != 0)
				Add ("${" + key + '}' , val);
				
			if ((styles & SubstStyles.Makefile) != 0)
				Add ("$(" + key + ')' , val);

			if ((styles & SubstStyles.Dos) != 0)
				Add ('%' + key + '%' , val);
		}

		public void AddDictionary (MBDictionary substs, IBuildContext ctxt)
		{
		    foreach (string key in substs.Keys) {
			Result r = substs[key];
			
			if (r is MBString)
			    AddSubst (key, ((MBString) r).Value);
			else if (r is MBFile) {
			    MBFile f = (MBFile) r;
			    
			    if (f.Dir.Storage != ResultStorageKind.System)
				ctxt.Logger.Warning (2044, "Substituting a relative filename into a text file -- " +
						     "this will cause problems when building from a different directory. " +
						     "Be very sure that you know what you're doing.", key);
			    else
				AddSubst (key, f.GetPath (ctxt));
			} else
			    AddSubst (key, r.ToString ());
		    }
		}
	}
}
