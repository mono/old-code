//
// INodeFormatter.cs: Formats a NodeInfo object for display
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

namespace Mono.TypeReflector.Formatters
{
	public interface INodeFormatter : IPolicy {

		string GetDescription (NodeInfo value);
	}
}

