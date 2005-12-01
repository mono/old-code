//
//  NSCFString.cs
//
//  Authors
//    - C.J. Collier, Collier Technologies, <cjcollier@colliertech.org>
//    - Urs C. Muff, Quark Inc., <umuff@quark.com>
//    - Kangaroo, Geoff Norton
//    - Adham Findlay
//
//  Copyright (c) 2004 Quark Inc. and Collier Technologies.  All rights reserved.
//
//	$Header: /home/miguel/third-conversion/public/cocoa-sharp/generator/custom/Foundation/NSCFString.cs,v 1.2 2004/06/28 19:18:31 urs Exp $
//

using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace Apple.Foundation {
	public class NSCFString : NSString {
		public static IntPtr NSCFString_class = Apple.Foundation.Class.Get("NSCFString");

		protected internal NSCFString(IntPtr raw,bool release) : base(raw,release) {}

	}
}

//***************************************************************************
//
// $Log: NSCFString.cs,v $
// Revision 1.2  2004/06/28 19:18:31  urs
// Implement latest name bindings changes, and using objective-c reflection to see is a type is a OC class
//
// Revision 1.1  2004/06/24 03:47:30  urs
// initial custom stuff
//
// Revision 1.1  2004/06/19 17:19:27  gnorton
// Broken API fixes.
// Delegates and methods with multi-argument support working.
// Argument parsing and casting working for all our known classes.
//
// Revision 1.9  2004/06/17 15:58:07  urs
// Public API cleanup, making properties and using .Net types rather then NS*
//
// Revision 1.8  2004/06/17 13:06:27  urs
// - release cleanup: only call release when requested
// - loader cleanup
//
// Revision 1.7  2004/06/16 12:20:27  urs
// Add CVS headers comments, authors and Copyright info, feel free to add your name or change what is appropriate
//
//***************************************************************************
