using System;

namespace Mono.Build.Bundling {

    [AttributeUsage(AttributeTargets.Field|AttributeTargets.Property)]
    public class StructureParameterAttribute : Attribute {
	public StructureParameterAttribute (StructureParameterKind kind) 
	{
	    this.kind = kind;
	}

	StructureParameterKind kind;

	public StructureParameterKind Kind {
	    get { return kind; }
	    set { kind = value; }
	}
    }
}
