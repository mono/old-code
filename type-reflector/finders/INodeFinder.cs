//
// INodeFinder.cs: Policy interface to find nodes for a type.
//
// Author: Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002 Jonathan Pryor
//

using System;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Mono.TypeReflector;

namespace Mono.TypeReflector.Finders
{
	[Flags]
	public enum FindMemberTypes {
		Base            = 1 << 1,
		Interfaces      = 1 << 2,
		Fields          = 1 << 3,
		Constructors    = 1 << 4,
		Methods         = 1 << 5,
		Properties      = 1 << 6,
		Events          = 1 << 7,
		TypeProperties  = 1 << 8,
		VerboseOutput   = 1 << 9,
		MonoBroken      = 1 << 10
	}

	public interface INodeFinder : IPolicy {

		NodeInfoCollection GetChildren (NodeInfo root);

		BindingFlags BindingFlags {get; set;}

		FindMemberTypes FindMembers {get; set;}
	}
}

