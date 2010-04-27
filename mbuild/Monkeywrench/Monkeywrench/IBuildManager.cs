//
// IBuildManager.cs -- something that manages the execution of a build
//

using System;

namespace Monkeywrench {

	public interface IBuildManager { 
		// do this as arrays so we can parallelize
		BuiltItem[] EvaluateTargets (int[] targets);
	}
}
