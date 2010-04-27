using System;
using System.IO;
using System.Collections;

using Mono.Build;
using Mono.Build.Bundling;

namespace Monkeywrench.Compiler {

    public class GraphCompiler {

	private GraphCompiler () {}

	public static GraphBuilder Compile (SourceSettings pwdsrc, IWarningLogger log)
	{
	    GraphBuilder gb = new GraphBuilder ();

	    Queue toload = new Queue ();
	    toload.Enqueue (new BuildfileProviderLoader ("/", pwdsrc));

	    while (toload.Count > 0) {
		ProviderLoaderBase pl = (ProviderLoaderBase) toload.Dequeue ();
		WrenchProvider wp;

		//Console.WriteLine ("popped off: {0}, {1}, {2}", pl.Basis, pl.DeclarationLoc, pl);
		wp = (WrenchProvider) gb.EnsureProvider (pl.Basis, pl.DeclarationLoc);

		if (pl.Initialize (wp, log, toload))
		    // parse errors or something
		    return null;

		if (wp.Finish (gb.Bundles, log))
		    return null;
	    }

	    // check that all referenced providers were created

	    if (gb.Finish (log))
		return null;

	    return gb;
	}
    }
}

