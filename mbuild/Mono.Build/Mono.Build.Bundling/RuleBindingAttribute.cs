using System;

namespace Mono.Build.Bundling {

    // This is applied to TargetTemplates that are associated
    // with a particular structure's defaults for a Rule class.
    // Binding is not optional here.

    [AttributeUsage (AttributeTargets.Class)]
    public class RuleBindingAttribute : Attribute {

	Type rtype;

	public RuleBindingAttribute (Type rtype) 
	{
	    if (rtype == null)
		throw new ArgumentException ();

	    this.rtype = rtype;
	}

	public Type RuleType {
	    get { return rtype; } 
	    set { rtype = value; }
	}
    }
}
