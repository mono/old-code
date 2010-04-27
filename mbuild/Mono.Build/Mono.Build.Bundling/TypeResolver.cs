using System;
using System.Collections.Generic;

namespace Mono.Build.Bundling {

    public abstract class TypeResolver {

	public abstract bool ResolveName (string name, out TargetTemplate tmpl, IWarningLogger log);

	public TargetTemplate ResolveName (string name, IWarningLogger log)
	{
	    TargetTemplate tmpl;

	    if (ResolveName (name, out tmpl, log))
		return null;

	    if (tmpl == null) {
		log.Error (2023, "Template lookup failed -- did you forget a using [] directive?", name);
		return null;
	    }
	    
	    return tmpl;
	}

	public abstract IEnumerable<IMatcher> GetMatchers (MatcherKind kind, IWarningLogger log);

	public bool TryMatch (string name, MatcherKind kind, out TargetTemplate tmpl, IWarningLogger log) 
	{
	    tmpl = null;
	    IMatcher which = null;
	    int quality = 0;
	    
	    IEnumerable<IMatcher> mlist = GetMatchers (kind, log);

	    if (mlist == null)
		return true;

	    foreach (IMatcher m in mlist) {
		int q;
		TargetTemplate match = m.TryMatch (name, out q);

		if (match == null)
		    continue;
		
		if (q < quality)
		    continue;
		
		if (tmpl != null && q == quality) {
		    // FIXME: provide a way to specify which one should take priority!
		    string s = String.Format ("Two matches succeed with equal quality: " + 
		    			      "{0} and {1}; going with the first",
		    			      which, m);
		    log.Warning (2026, s, name);
		    break;
		}
		
		which = m;
		tmpl = match;
		quality = q;
	    }
	    
	    return false;
	}
    }
}
