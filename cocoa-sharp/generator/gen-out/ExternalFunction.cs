//
//  Authors
//    - Kangaroo, Geoff Norton
//    - Urs C. Muff, Quark Inc., <umuff@quark.com>
//
//  Copyright (c) 2004 Quark Inc.  All rights reserved.
//
// $Id: ExternalFunction.cs,v 1.2 2004/09/09 03:32:22 urs Exp $
//

using System;
using System.Collections;

namespace CocoaSharp {
	public class ExternalFunction : OutputElement {
		public ExternalFunction(string name, string nameSpace, TypeUsage returnType, ICollection parameters)
			: base(name, nameSpace) {
			this.returnType = returnType;
			this.parameters = parameters;
		}

		// -- Public Properties --
		public TypeUsage ReturnType { get { return returnType; } }
		public ICollection Parameters { get { return parameters; } }

		// -- Members --
		private TypeUsage returnType;
		private ICollection parameters;
	}
}

//
// $Log: ExternalFunction.cs,v $
// Revision 1.2  2004/09/09 03:32:22  urs
// Convert methods from mach-o to out format
//
// Revision 1.1  2004/09/09 01:16:03  urs
// 1st draft of out module of 2nd generation generator
//
//
