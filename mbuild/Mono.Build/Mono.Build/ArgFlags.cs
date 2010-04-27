//
// ArgFlags.cs -- rule argument flags
//

using System;

namespace Mono.Build {

	[Flags]
	public enum ArgFlags {
		// exactly one value, no special flags
		Standard = 0,

		// We can have less than one argument here
		Optional = 1,
		
		// We can have more than one
		Multi    = 2,
		
		// Results of the appropriate type, if not otherwise
		// hinted, should be assigned to this argument
		Default  = 4,

		// The order of the arguments matter. Unnamed dependencies,
		// not specifically applied to any particular argument, will
		// not be applied to this argument. If it is marked with the
		// DefaultOrdered flag, arguments added with the AddDefaultOrdered()
		// function will be added to this argument
		Ordered  = 8,

		// See above
		DefaultOrdered = 16
	}
}
