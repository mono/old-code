// CompositeResultFieldAttribute.cs -- indicates that the field is a Result field
// of a CompositeResult

using System;
using System.Reflection;

namespace Mono.Build {

	[AttributeUsage(AttributeTargets.Field)]
	public class CompositeResultFieldAttribute : Attribute {
		public CompositeResultFieldAttribute () {}
	}
}
