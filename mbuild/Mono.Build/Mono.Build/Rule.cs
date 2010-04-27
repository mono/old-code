//
// A class for a rule defining how to build something.
//

using System;
using System.Collections.Generic;

using Mono.Build.Bundling;

namespace Mono.Build {

    public abstract class Rule : IFingerprintable {

	// In general, all of the overrides here are best
	// done with the help of mb-bundlegen. A few rules
	// are written by hand, but it is much easier to implement
	// them using the tool.

	// Subclasses should return base.NumArguments plus
	// the number of arguments defined in that subclass.

	public virtual int NumArguments { get { return 0; } }

	// Subclasses should chain to base.ListArguments, then call
	// AddArg on the sink once for each argument defined in the
	// subclass. The id's should start at base.NumArguments and
	// increase by 1 up until this.NumArguments - 1. (You can,
	// of course, also call sink.WantTargetName if you want to.)

	public virtual void ListArguments (IArgInfoSink sink)
	{
	    return;
	}

	// Subclasses are expected to chain to base.FetchArgValues,
	// then call GetArgValue on the source once for each argument
	// defined by the subclass.  The return value is presumably
	// then stored in a member of the subclass, potentially with
	// type conversions. The member should be protected so that
	// subclasses of the rule can access the argument. Once
	// FetchArgValues has been called, such members should be
	// considered read only.

	public virtual void FetchArgValues (IArgValueSource source)
	{
	    return;
	}

	// Subclasses should chain to base.ClearArgValues, then
	// set to null any members that are set in FetchArgValues.
	// This allows reuse of a single Rule instance for multiple
	// invocations of Build.

	public virtual void ClearArgValues ()
	{
	    return;
	}

	// A type that this rule will always return. Depending on the
	// arguments, however, this rule may return a subclass of this
	// Type.

	public abstract Type GeneralResultType { get; }

	// The specific type that this rule will return. Must always 
	// be a subtype of GeneralResultType. The accessor may assume that
	// FetchArgValues has been called on this instance, and may
	// return a Type that depends on the specific type of one of the
	// arguments. It may not, however, assume that the actual values of
	// the arguments it receives are meaningful. For instance, an MBFile
	// argument might have the specific type of CSharpSource, but the
	// Dir and Name fields may be null. This allows introspection of
	// the specific return type of a rule from knowledge of the types,
	// but not values, of its arguments.
	//
	// See Mono.Build.RuleLib.CloneRule for an example of how all this
	// typing stuff works.

	public virtual Type SpecificResultType 
	{
	    get { return GeneralResultType; }
	}

	// Utility

	protected object CreateResultObject ()
	{
	    return Activator.CreateInstance (SpecificResultType);
	}

	// Set this to true if the specific return type actually depends
	// on the arguments to the rule. If false, SpecificResultType should
	// equal GeneralResultType always. Don't forget to set this member
	// in the constructor of hand-coded Rule subclasses!

	protected bool specific_varies = false;

	public bool SpecificResultTypeVaries { get { return specific_varies; } }

	// Actual building. See mb-bundlegen for the typical skeleton implementation
	// of the Build function, which passes off to a BuildImpl function which takes
	// an object of type GeneralReturnType as an argument.

	public abstract Result Build (IBuildContext ctxt);

	public abstract Fingerprint GetFingerprint (IBuildContext ctxt, Fingerprint cached);

	// Useful functions

	class ArgNameMapHelper : IArgInfoSink {
	    public Dictionary<string,int> map;

	    public ArgNameMapHelper ()
	    {
		map = new Dictionary<string,int> ();
	    }

	    public void WantTargetName (bool required) {}

	    public void AddArg (int id, string name, Type type, string dflt, ArgFlags flags)
	    {
		if (map.ContainsKey (name))
		    throw new Exception ("Bad rule implementation duplicating arg names!");
		if (map.ContainsValue (id))
		    throw new Exception ("Bad rule implementation duplicating arg IDs!");

		map[name] = id;
	    }

	    public void AddArg (int id, string name, Type type, ArgFlags flags)
	    {
		AddArg (id, name, type, null, flags);
	    }
	}

	public Dictionary<string,int> MakeArgNameMap ()
	{
	    ArgNameMapHelper anmh = new ArgNameMapHelper ();
	    ListArguments (anmh);
	    return anmh.map;
	}

	// Various ArgValueSource conversions. First, 0-or-1
	// and exactly-1 array reductions.

	public static Tres AsOptional<Tres> (Result[] r) where Tres : Result
	{
	    if (r == null)
		throw new ArgumentNullException ();
	    if (r.Length > 1)
		throw new ArgumentException ("Array should have 0 or 1 members", "r");

	    if (r.Length == 0)
		return null;

	    if (!(r[0] is Tres))
		throw ExHelp.Argument ("r", "Array member should be a {0} but is instead {1}",
				       typeof (Tres), r[0].GetType ());

	    return (Tres) r[0];
	}

	public static Tres AsSingle<Tres> (Result[] r) where Tres : Result
	{
	    if (r == null)
		throw new ArgumentNullException ();
	    if (r.Length != 1)
		throw new ArgumentException ("Array should have exactly 1 member", "r");

	    if (!(r[0] is Tres))
		throw ExHelp.Argument ("r", "Array member should be a {0} but is instead {1}",
				       typeof (Tres), r[0].GetType ());

	    return (Tres) r[0];
	}

	public static Tres[] AsArray<Tres> (Result[] rarr) where Tres : Result
	{
	    Tres[] res = new Tres[rarr.Length];

	    if (rarr.Length > 0 && !(rarr[0] is Tres))
		throw ExHelp.Argument ("rarr", "Array member should be a {0} but is instead {1}",
				       typeof (Tres), rarr[0].GetType ());

	    rarr.CopyTo (res, 0);

	    return res;
	}

	// Now, the above with conversions. Need some icky stuff for
	// value types in the optional case. Yay nullable types.

	public static Tdat AsOptionalRef<Tdat,Tres> (Result[] rarr) 
	    where Tres : Result, IRefTypeResult<Tdat> where Tdat : class
	{
	    Tres r = AsOptional<Tres> (rarr);

	    if (r == null)
		return null;

	    return r.Value;
	}

	public static Tdat? AsOptionalValue<Tdat,Tres> (Result[] rarr) 
	    where Tres : Result, IValueTypeResult<Tdat> where Tdat : struct
	{
	    Tres r = AsOptional<Tres> (rarr);

	    if (r == null)
		// The code is the same here as above, but here we construct
		// a Nullable<Tdat> and return an empty one.
		return null;

	    return r.Value;
	}

	public static Tdat AsSingleConv<Tdat,Tres> (Result[] rarr) 
	    where Tres : Result, IConvertibleResult<Tdat>
	{
	    Tres r = AsSingle<Tres> (rarr);

	    return r.Value;
	}

	public static Tdat[] AsArrayConv<Tdat,Tres> (Result[] rarr)
	    where Tres : Result, IConvertibleResult<Tdat>
	{
	    Tdat[] res = new Tdat[rarr.Length];

	    for (int i = 0; i < rarr.Length; i++)
		res[i] = ((IConvertibleResult<Tdat>) rarr[i]).Value;

	    return res;
	}

#if MAYBE_NOT
	// Specific variants ... doubt the current CodeDom supports generic
	// types, so write these out. Should eventually do EnumResults
	// dynamically with the above functions.

	public static bool? AsOptionalBool (Result[] rarr)
	{
	    return AsOptionalValue<bool,MBBool> (rarr);
	}

	public static bool AsSingleBool (Result[] rarr)
	{
	    return AsSingleConv<bool,MBBool> (rarr);
	}

	public static bool[] AsArrayBool (Result[] rarr)
	{
	    return AsArrayConv<bool,MBBool> (rarr);
	}

	public static string AsOptionalString (Result[] rarr)
	{
	    return AsOptionalRef<string,MBString> (rarr);
	}

	public static string AsSingleString (Result[] rarr)
	{
	    return AsSingleConv<string,MBString> (rarr);
	}

	public static string[] AsArrayString (Result[] rarr)
	{
	    return AsArrayConv<string,MBString> (rarr);
	}
#endif
    }
}
