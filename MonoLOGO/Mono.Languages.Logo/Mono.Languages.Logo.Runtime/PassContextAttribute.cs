namespace Mono.Languages.Logo.Runtime {
	using System;

	[AttributeUsage (AttributeTargets.Method)]
	public class PassContextAttribute : Attribute {
			public PassContextAttribute () {
			}
	}
}

