// Many aspects of the MBuild API involve something that has
// a value that can be specified either as a Result directly,
// or as the value of some target. The SingleValue struct makes
// it easier to handle the general case of 'something that has
// a value' type-safely. We identify targets differently in different
// stages of the program: as strings, as TargetBuilders, or as ints, 
// so this struct is generic

using System;

namespace Mono.Build.Bundling {

    public struct SingleValue<Ttarg> {

	object val;

	public SingleValue (Ttarg target)
	{
	    val = target;
	}

	public SingleValue (Result result)
	{
	    val = result;
	}

	public bool IsTarget
	{
	    get { return val is Ttarg; }
	}

	public bool IsResult
	{
	    get { return val is Result; }
	}

	public static explicit operator Ttarg (SingleValue<Ttarg> sv)
	{
	    return (Ttarg) sv.val;
	}

	public static explicit operator Result (SingleValue<Ttarg> sv)
	{
	    return (Result) sv.val;
	}

	public override string ToString ()
	{
	    return val.ToString ();
	}
    }

#if ILLEGAL
    // Typically, a user-provided input will identify a target
    // with a string name.

    public struct SingleValueIn : SingleValue<string> {
	public SingleValueIn (string t) : base (t) {}
	public SingleValueIn (Result r) : base (r) {}
    }

    // After the input has been processed, the target name will
    // have been mapped to a TargetBuilder instance.

    public struct SingleValueOut : SingleValue<TargetBuilder> {
	public SingleValueOut (TargetBuilder t) : base (t) {}
	public SingleValueOut (Result r) : base (r) {}
    }
#endif

}

