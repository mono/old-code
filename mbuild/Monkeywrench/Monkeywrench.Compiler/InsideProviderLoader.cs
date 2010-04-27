using System;
using System.Collections;

using Mono.Build;
using Mono.Build.Bundling;

namespace Monkeywrench.Compiler {

    internal class InsideProviderLoader : ProviderLoaderBase {

	NameLookupContext nlc;

	public InsideProviderLoader (string basis, NameLookupContext tmpl)
	    : base (basis, null)
	{
	    this.nlc = (NameLookupContext) tmpl.Clone ();
	}

	public override bool Initialize (WrenchProvider wp, IWarningLogger log, Queue children)
	{
	    wp.NameContext = nlc;
	    return false;
	}
    }
}
