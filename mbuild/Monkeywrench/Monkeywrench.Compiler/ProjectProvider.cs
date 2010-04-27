//
// Provides information about the project
//

using System;

using Mono.Build;
using Mono.Build.Bundling;

namespace Monkeywrench.Compiler {

	public class ProjectProvider : SimpleProvider {
		public ProjectProvider (ProjectInfo pinfo) : base (null) {
			AddTarget ("name", pinfo.Name);
			AddTarget ("version", pinfo.Version);
			AddTarget ("compat_code", pinfo.CompatCode);

			// Alias to line up better with how it's declared
			// in the buildfile. (Need the underscore version so
			// we can reference it in .bundles)

			AddTarget ("compat-code", pinfo.CompatCode);
		}
	}
}
