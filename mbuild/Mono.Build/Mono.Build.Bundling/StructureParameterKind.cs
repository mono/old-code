namespace Mono.Build.Bundling {

    // We have a member of a structure template that represents
    // some configurable parameter of the structure that the 
    // template instantiates. What kind of parameter is it?

    public enum StructureParameterKind {

	// The parameter is the full name of one particular target.
	// The attribute or property should be a string.

	Target,

	// The parameter is a basis name. The attribute or property
	// should be a string. The basis name should be canonicalized
	// to end with a forward slash. (mb-bundlegen does this
	// canonicalization automatically.)

	Basis,

	// The parameter is another StructureTemplate. The attribute
	// or property should be of a type that is a subclass of
	// StructureTemplate. This argument will most likely be used
	// to obtain the values of its structure arguments.
	// Usually such an argument points to a namespace A.B's 
	// A.B._DefaultStructure type, but this does not necessarily
	// have to be the case.

	Structure

    }
}
