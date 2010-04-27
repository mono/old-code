//
// RegexMatcher.cs -- matcher based on a regular expression
//

using System;
using System.Text.RegularExpressions;

using Mono.Build;
using Mono.Build.Bundling; //IMatcher

namespace Mono.Build.RuleLib {
   
    // Things inheriting from this may bind to structures; 
    // provide a termination of the trail of "do we need
    // to reference a structure?" inquiries.
    [StructureBinding (typeof (StructureTemplate), false)]
    public abstract class RegexMatcher : IMatcher {

	StructureTemplate stmpl;

	public RegexMatcher ()
	{
	    stmpl = null;
	}

	public RegexMatcher (StructureTemplate stmpl)
	{
	    this.stmpl = stmpl;
	}

	// Makes it so subclases don't have to 
	// 'using System.Text.RegularExpressions'

	public abstract string GetRegex ();

	public abstract Type GetMatchType ();

	// IMatcher

	public TargetTemplate TryMatch (string name, out int quality) {
	    Regex r = new Regex (GetRegex ());
	    Match m = r.Match (name);

	    if (m == Match.Empty) {
		quality = 0;
		return null;
	    }

	    quality = m.Groups[0].Length;
	    Type t = GetMatchType ();

	    if (t.IsSubclassOf (typeof (Rule)))
		return new RuleOnlyTemplate (t);

	    if (stmpl == null)
		return (TargetTemplate) Activator.CreateInstance (t);

	    return (TargetTemplate) Activator.CreateInstance (t, stmpl);
	}

	public class RuleOnlyTemplate : TargetTemplate {

	    public RuleOnlyTemplate (Type rtype)
	    {
		this.rtype = rtype;
	    }

	    Type rtype;

	    public override void ApplyTemplate (TargetBuilder tb)
	    {
		tb.RuleType = rtype;
	    }
	}

	// object

	public override string ToString () {
	    return String.Format ("[regex matcher {0} -> {1}]",
				  GetRegex (), GetMatchType ());
	}
    }
}
