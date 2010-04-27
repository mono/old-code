//
// Something that provides targets, dependencies and stuff like that
// to a project.
//
// Target names should be de-prefixed before being passed into a BP.
// (See Project::Add())

using System;
using System.Collections;

namespace Mono.Build {

	public interface IBuildProvider {

		ITarget GetTarget (string name);

		IEnumerable Targets { get; }
		IEnumerable TargetNames { get; }

		// this feels right but doesn't really
		// work out implementation-wise
		//object Context { get; }
	}
}
