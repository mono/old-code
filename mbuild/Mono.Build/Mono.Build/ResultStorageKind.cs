//
// Where a result is stored -- internally in the serialized
// result cache; in the system (absolute path); in the source
// directory (path relative to topsrcdir); in the build directory
// (relative to topbuilddir).
//

using System;

namespace Mono.Build {

	public enum ResultStorageKind {
		System,
		Source,
		Built
	}
}
