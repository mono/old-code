using System;
using System.Collections;

using Mono.Build;

namespace Monkeywrench.Compiler {

    internal class ProjectProviderLoader : ProviderLoaderBase {

	ProjectInfo pinfo;

	public ProjectProviderLoader (ProjectInfo pinfo) : base ("/project/", ".") 
	{
	    this.pinfo = pinfo;
	}

	public override bool Initialize (WrenchProvider wp, IWarningLogger log, Queue children)
	{
	    bool ret = false;

	    ret |= wp.DefineConstantTarget ("name", new MBString (pinfo.Name), log);
	    ret |= wp.DefineConstantTarget ("version", new MBString (pinfo.Version), log);
	    ret |= wp.DefineConstantTarget ("compat_code", new MBString (pinfo.CompatCode), log);
	    ret |= wp.DefineConstantTarget ("compat-code", new MBString (pinfo.CompatCode), log);

	    return ret;
	}
    }

}
