using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ObjC2 {
	public class MethodSignature : Object {
		public MethodSignature (Object obj, string selector) : this (obj, new Selector (selector)) {}

		public MethodSignature (Object obj, Selector selector) {
			handle = Messaging.objc_msgSend (obj.handle, new Selector ("methodSignatureForSelector:").Handle, selector.Handle);
		}
	}
}
