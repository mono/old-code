//
// IBuildContext.cs -- context of a build process (returned as Context by providers)
//

using System;

namespace Mono.Build {

	public interface IBuildContext {
		MBDirectory WorkingDirectory { get; }
		MBDirectory SourceDirectory { get; }

		string PathTo (MBDirectory dir);
		string DistPath (MBDirectory dir);

		IBuildLogger Logger { get; }
	}
}
