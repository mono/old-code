using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using Mono.Build;

namespace Mono.Build.Bundling {

    public abstract class ProjectBuilder {

	protected abstract ProviderBuilder CreateProvider (string basis);

	Dictionary<string,ProviderBuilder> providers =
	    new Dictionary<string,ProviderBuilder> ();

	// Should only be called if the provider at the given
	// basis is known to exist -- will throw an exception
	// if it doesn't.

	public ProviderBuilder GetProvider (string basis)
	{
	    if (!providers.ContainsKey (basis))
		throw ExHelp.App ("The provider at basis `{0}' should be defined " +
				  "by now, but hasn't been.", basis);

	    return providers[basis];
	}

	// Returns null and logs an error if the provider at the given
	// basis has already been defined.

	public ProviderBuilder DefineProvider (string basis, string decl_loc, IWarningLogger log)
	{
	    if (providers.ContainsKey (basis)) {
		log.Error (2043, "Trying to redefine the provider at " + basis, null);
		return null;
	    }

	    ProviderBuilder pb = CreateProvider (basis);
	    pb.Claim (decl_loc);
	    providers[basis] = pb;
	    return pb;
	}

	ProviderBuilder EnsureProvider (string basis, string decl_loc, bool do_claim)
	{
	    ProviderBuilder pb;

	    if (providers.ContainsKey (basis))
		pb = providers[basis];
	    else {
		pb = CreateProvider (basis);
		providers[basis] = pb;
	    }

	    if (do_claim)
		pb.WeakClaim (decl_loc);

	    return pb;
	}

	public ProviderBuilder EnsureProvider (string basis, string decl_loc)
	{
	    return EnsureProvider (basis, decl_loc, true);
	}

	public IEnumerable<ProviderBuilder> Providers {
	    get { return providers.Values; }
	}

	// Helper type things

	public static string ParseFullName (string fullname, out string basis)
	{
	    basis = null;

	    if (fullname[0] != '/' || fullname[fullname.Length - 1] == '/')
		throw ExHelp.Argument ("fullname", "Invalid absolute target name `{0}'",
				       fullname);

	    int idx = fullname.LastIndexOf ('/');

	    basis = fullname.Substring (0, idx + 1);
	    return fullname.Substring (idx + 1);
	}

	// Throws an exception if the target or its provider is
	// not yet defined; this function should only be called
	// if the target is known to exist.

	public TargetBuilder GetTarget (string fullname)
	{
	    string basis, basename;

	    basename = ParseFullName (fullname, out basis);

	    ProviderBuilder pb = providers[basis];

	    if (pb == null)
		throw ExHelp.Argument ("fullname", "The target `{0}' should exist by " +
				       "now but does not", fullname);

	    return pb.GetTarget (basename);
	}

	// Defines the target within a provider, but NOT
	// the provider itself.

	public TargetBuilder DefineTarget (string fullname, IWarningLogger log)
	{
	    string basis, basename;

	    basename = ParseFullName (fullname, out basis);

	    ProviderBuilder pb = providers[basis];

	    if (pb == null)
		return null;

	    return pb.DefineTarget (basename, log);
	}

	// Requests the target within a provider, AND
	// defines the provider if necessary.

	public TargetBuilder RequestTarget (string fullname)
	{
	    string basis, basename;

	    basename = ParseFullName (fullname, out basis);
	    return EnsureProvider (basis, null, false).RequestTarget (basename);
	}

	// References the target within a provider, AND
	// defines the provider if necessary.

	public TargetBuilder ReferenceTarget (string fullname)
	{
	    string basis, basename;

	    basename = ParseFullName (fullname, out basis);
	    return EnsureProvider (basis, null, false).ReferenceTarget (basename);
	}
    }
}
