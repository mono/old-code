using System;
using System.Collections;
using System.IO;

using Mono.Build;

namespace Monkeywrench.Compiler {

    internal abstract class ProviderLoaderBase {

	protected ProviderLoaderBase (string basis, string decl_loc)
	{
	    if (basis == null || basis[0] != '/' || basis[basis.Length - 1] != '/')
		throw ExHelp.Argument (basis, "Invalid basis string `{0}'", basis);

	    this.basis = basis;

	    if (decl_loc == null)
		this.decl_loc = SourceSettings.BasisToSubpath (basis);
	    else
		this.decl_loc = decl_loc;
	}

	protected string basis;

	public string Basis { get { return basis; } }

	protected string decl_loc;

	public string DeclarationLoc { get { return decl_loc; } }

	public abstract bool Initialize (WrenchProvider wp, IWarningLogger log, Queue children);
    }

}
