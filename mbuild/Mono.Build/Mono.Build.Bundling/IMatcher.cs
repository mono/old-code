//
// IMatcher.cs -- interface for automatically creating targets from their names
//
// This is in Bundling, not just in Mono.Build, because the whole matcher system
// is not essential to the core build system. The only support for matchers enters
// in the bundling layer.

using System;

namespace Mono.Build.Bundling {

	public interface IMatcher {

		// return null if no match, a template type if so
		//
		// quality is a gauge of how good the match is.
		// For RegexMatcher (the only IMatcher implementation
		// I've written yet), it is the length of the matched text.

		TargetTemplate TryMatch (string name, out int quality);
	}
}
