//
//  NSCFArray.cs
//
//  Authors
//    - Kangaroo, Geoff Norton
//
//  Copyright (c) 2004 Quark Inc. and Collier Technologies.  All rights reserved.
//
//	$Header: /home/miguel/third-conversion/public/cocoa-sharp/generator/custom/Foundation/NSCFArray.cs,v 1.1 2004/07/24 16:07:11 gnorton Exp $
//

using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace Apple.Foundation {
	public class NSCFArray : NSArray {
		public static IntPtr NSCFArray_class = Apple.Foundation.Class.Get("NSCFArray");

		protected internal NSCFArray(IntPtr raw,bool release) : base(raw,release) {}

	}
}
