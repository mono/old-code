namespace Mono.Build.Bundling {

    public enum MatcherKind {

	// We know the target name. There may be any
	// number of dependencies. The input to the
	// matcher is the target's name.

	Target,

	// Something depends on an implicitly created
	// target whose name we know. There are 0 
	// dependencies. The input to the matcher is
	// the name of the implicitly created target.

	Dependency,

	// There is a target with exactly one dependency.
	// The input to the matcher is the dependency's
	// name, not the target's.

	DirectTransform

    }
}
