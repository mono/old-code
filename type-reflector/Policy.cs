//
// NodePolicy.cs: 
//
// Default implementation of the INodePolicy interface.
//
// Author: Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2003 Jonathan Pryor
//

using System;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Mono.TypeReflector
{
	public class Policy : IPolicy {

		private string description, key;

		public string Description {
			get {return description;}
			set {description = value;}
		}

		public string FactoryKey {
			get {return key;}
			set {key = value;}
		}

		public virtual object Clone ()
		{
			return MemberwiseClone ();
		}
	}
}

