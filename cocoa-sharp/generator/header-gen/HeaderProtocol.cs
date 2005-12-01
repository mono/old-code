//
//  HeaderProtocol.cs
//
//  Authors
//    - Kangaroo, Geoff Norton
//    - C.J. Collier, Collier Technologies, <cjcollier@colliertech.org>
//    - Urs C. Muff, Quark Inc., <umuff@quark.com>
//    - Adham Findlay
//
//  Copyright (c) 2004 Quark Inc. and Collier Technologies.  All rights reserved.
//
//	$Header: /home/miguel/third-conversion/public/cocoa-sharp/generator/header-gen/HeaderProtocol.cs,v 1.3 2004/09/18 17:30:17 urs Exp $
//

using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;

namespace CocoaSharp {

	public class HeaderProtocol : ElementWithMethods {
		private string[] mChildren;
		private Protocol mProtocol;

		public HeaderProtocol(string _name, string _children, string _framework) : base(_name,_framework) {
			mProtocol = (Protocol)Type.RegisterType("@" + _name, this.NameSpace, typeof(Protocol));
			mChildren = _children.Split(new char[]{' ', ','});
		}

		public string[] Children {
			get { return mChildren; } set { mChildren = value; }
		}

		public override OutputElement ToOutput() {
			mProtocol.Initialize(
				HeaderMethod.ToMethods(this, this.Methods.Values, false),
				HeaderMethod.ToMethods(this, this.Methods.Values, true));
			return mProtocol;
		}
	}
}

//	$Log: HeaderProtocol.cs,v $
//	Revision 1.3  2004/09/18 17:30:17  urs
//	Move CS output gen into gen-out
//
//	Revision 1.2  2004/09/11 00:41:22  urs
//	Move Output to gen-out
//	
//	Revision 1.1  2004/09/09 13:18:53  urs
//	Check header generator back in.
//	
//	Revision 1.13  2004/09/07 20:51:21  urs
//	Fix line endings
//	
//	Revision 1.12  2004/06/29 03:32:58  urs
//	Cleanup mapping usage: only one bug left
//	
//	Revision 1.11  2004/06/28 22:07:43  gnorton
//	Updates/bugfixes
//	
//	Revision 1.10  2004/06/28 19:18:31  urs
//	Implement latest name bindings changes, and using objective-c reflection to see is a type is a OC class
//	
//	Revision 1.9  2004/06/25 02:49:14  gnorton
//	Sample 2 now runs.
//	
//	Revision 1.8  2004/06/24 18:56:53  gnorton
//	AppKit compiles
//	Foundation compiles
//	Output setMethod() for protocols not just the property so Interfaces are met.
//	Ignore static protocol methods (.NET doesn't support static in interfaces).
//	Resolve compiler errors.
//	
//	Revision 1.7  2004/06/23 17:14:20  gnorton
//	Custom addins supported on a per file basis.
//	
//	Revision 1.6  2004/06/23 15:29:29  urs
//	Major refactor, allow inheriting parent constructors
//	
//	Revision 1.5  2004/06/22 13:38:59  urs
//	More cleanup and refactoring start
//	Make output actually compile (diverse fixes)
//	
//	Revision 1.4  2004/06/22 12:04:12  urs
//	Cleanup, Headers, -out:[CS|OC], VS proj
//	
//
