using System;
using System.Collections.Generic;

namespace Mono.Build.Bundling {

    public abstract class StructureTemplate {

	// The functions return true on error, as usual.
	//
	// Right now we don't inherit structure templates from one another,
	// so this function doesn't need to be virtual. I can't see a case in which
	// inheriting is what one wants to do, but maybe I'm wrong ...

	public virtual bool Apply (ProjectBuilder proj, string decl_loc, IWarningLogger log)
	{
	    return false;
	}

	public abstract bool ApplyDefaults (BundleManagerBase bmb, IWarningLogger log);
    }
}
