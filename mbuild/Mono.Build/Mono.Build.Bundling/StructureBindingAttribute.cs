using System;

namespace Mono.Build.Bundling {

    // Marks a type that has operations that may require 
    // the context of a structure: that is, they depend
    // on the values of the structure's arguments in some way.
    // Null *is* a valid value for stype; this indicates that
    // the particular class in question does not exhibit any
    // behavior that is structure-dependent (even though, in
    // the general case, it could). The lack of this attribute
    // is to be interpreted as the 'stype = null' case.

    [AttributeUsage (AttributeTargets.Class)]
    public class StructureBindingAttribute : Attribute {

	Type stype;
	bool uses_structure;

	public StructureBindingAttribute (Type stype, bool uses_structure) 
	{
	    this.stype = stype;
	    this.uses_structure = uses_structure;
	}

	public Type StructureType {
	    get { return stype; } 
	    set { stype = value; }
	}

	public bool UsesStructure { 
	    get { return uses_structure; }
	    set { uses_structure = value; }
	}
    }
}
