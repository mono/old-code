using System;

namespace Mono.Build {

    public interface ITagVisitor<Ttid> {

	// Return true to cancel the visit loop.

	bool VisitTargetTag (string tag, Ttid targ);
	bool VisitResultTag (string tag, Result res);
    }
}
