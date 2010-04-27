// MonoBuildBundleAttribute.cs -- indicates that the assembly is a Mono.Build bundle

using System;

namespace Mono.Build.Bundling {

	[AttributeUsage(AttributeTargets.Assembly)]
	public class MonoBuildBundleAttribute : Attribute {
		public MonoBuildBundleAttribute () {}
	}
}
