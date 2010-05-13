//
// INodePolicy.cs: Root Policy interface to describe other policies.
//
// Author: Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2003 Jonathan Pryor
//

using System;

namespace Mono.TypeReflector
{
	public interface IPolicy : ICloneable {

		string Description {get; set;}
		string FactoryKey {get; set;}

	}
}

