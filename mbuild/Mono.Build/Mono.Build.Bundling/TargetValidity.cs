namespace Mono.Build.Bundling {

    public enum TargetValidity {

	// The target has not been referenced at all so 
	// far.

	Undefined,

	// The target has been explicitly defined
	// in its provider, or it was requested
	// and successfully constructed during fixup

	Defined,

	// Another provider, above this target's
	// provider in the heirarchy, has referenced
	// this target, and matchers will be used to
	// try and automatically construct it

	Requested,

	// Another provider, not above this target's
	// provider in the heirarchy, has referenced
	// this target. If it ends up not being explicitly
	// defined, that is an error. (The above conditions
	// make it so that all the targets in a given
	// provider can be known by parsing only the given
	// provider and all its parents.)

	Referenced
    }

}
