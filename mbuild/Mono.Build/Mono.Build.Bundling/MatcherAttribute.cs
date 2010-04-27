//
// TargetRuleMatcherAttribute.cs -- this matcher will give the rule
// to build an explicitly named target, based on that target's name
//

using System;

namespace Mono.Build.Bundling {

    [AttributeUsage(AttributeTargets.Class)]
    public class MatcherAttribute : Attribute {

	MatcherKind kind;

	public MatcherKind Kind {
	    get { return kind; }
	    set { kind = value; }
	}
	   
	public MatcherAttribute (MatcherKind kind)
	{
	    this.kind = kind;
	}
    }
}
