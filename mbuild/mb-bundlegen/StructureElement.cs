using System;
using System.Reflection;
using System.Collections.Generic;
using System.CodeDom;

using Mono.Build;
using Mono.Build.Bundling;

namespace MBBundleGen {

    // This is something that plays a part in the structure's templating
    // Apply function.

    public abstract class StructureElement {

	public StructureElement (NamespaceBuilder ns)
	{
	    if (!(ns.Params is StructureBuilder))
		throw new Exception ("Trying to instantiate a structure element in a " +
				     "namespace that has already had its structure template " +
				     "defined.");

	    this.sb = (StructureBuilder) ns.Params;
	    this.ns = ns;

	    sb.AddElement (this);
	}

	NamespaceBuilder ns;
	StructureBuilder sb;

	public NamespaceBuilder NS { get { return ns; } }

	public StructureBuilder Structure { get { return sb; } }

	public abstract void EmitApply (CodeMemberMethod apply, CodeArgumentReferenceExpression proj,
					CodeArgumentReferenceExpression declloc, 
					CodeArgumentReferenceExpression log, 
					CodeVariableReferenceExpression pb,
					CodeVariableReferenceExpression tb);

	public abstract bool Resolve (TypeResolveContext trc, bool errors);
    }
}
