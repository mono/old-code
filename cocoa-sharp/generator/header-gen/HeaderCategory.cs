//
//  HeaderCategory.cs
//
//  Authors
//    - Kangaroo, Geoff Norton
//    - C.J. Collier, Collier Technologies, <cjcollier@colliertech.org>
//    - Urs C. Muff, Quark Inc., <umuff@quark.com>
//    - Adham Findlay
//
//  Copyright (c) 2004 Quark Inc. and Collier Technologies.  All rights reserved.
//
//	$Header: /home/miguel/third-conversion/public/cocoa-sharp/generator/header-gen/HeaderCategory.cs,v 1.3 2004/09/18 17:30:17 urs Exp $
//

using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;

namespace CocoaSharp {

	public class HeaderCategory : ElementWithMethods {
		private string mClass;
		private string[] mImports;

		public HeaderCategory(string _name, string _class,string _framework) : base(_name,_framework) {
			mClass = _class;
		}

		public string[] Imports {
			get { return mImports; } set { mImports = value; }
		}
		public string Class {
			get { return mClass; } set { mClass = value; }
		}

		public override OutputElement ToOutput() {
			throw new NotSupportedException();
		}
	}
}

//	$Log: HeaderCategory.cs,v $
//	Revision 1.3  2004/09/18 17:30:17  urs
//	Move CS output gen into gen-out
//
//	Revision 1.2  2004/09/11 00:41:22  urs
//	Move Output to gen-out
//	
//	Revision 1.1  2004/09/09 13:18:53  urs
//	Check header generator back in.
//	
//	Revision 1.1  2004/09/06 18:26:58  gnorton
//	Fix HEAD
//	
//	Revision 1.8  2004/06/23 20:45:18  urs
//	Only add category of dependent frameworks, this might be changed in the future, but would require a new class
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
