using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ObjC2 {
	public class Messaging {
		[DllImport ("/usr/lib/libobjc.dylib")]
		internal extern static IntPtr objc_msgSend (IntPtr cls, IntPtr sel);
		[DllImport ("/usr/lib/libobjc.dylib")]
		internal extern static IntPtr objc_msgSend (IntPtr cls, IntPtr sel, IntPtr arg);
		[DllImport ("/usr/lib/libobjc.dylib")]
		internal extern static IntPtr objc_msgSend (IntPtr cls, IntPtr sel, ref bool arg);
		[DllImport ("/usr/lib/libobjc.dylib")]
		internal extern static IntPtr objc_msgSend (IntPtr cls, IntPtr sel, ref IntPtr arg);
		[DllImport ("/usr/lib/libobjc.dylib")]
		internal extern static IntPtr objc_msgSend (IntPtr cls, IntPtr sel, ref Int32 arg);
		[DllImport ("/usr/lib/libobjc.dylib")]
		internal extern static IntPtr objc_msgSend (IntPtr cls, IntPtr sel, ref float arg);
		[DllImport ("/usr/lib/libobjc.dylib")]
		internal extern static IntPtr objc_msgSend (IntPtr cls, IntPtr sel, ref double arg);
		[DllImport ("/usr/lib/libobjc.dylib")]
		internal extern static IntPtr objc_msgSend (IntPtr cls, IntPtr sel, IntPtr arg, Int32 idx);
		[DllImport ("/usr/lib/libobjc.dylib")]
		internal extern static IntPtr objc_msgSend (IntPtr cls, IntPtr sel, Single arg, Int32 idx);
		[DllImport ("/usr/lib/libobjc.dylib")]
		internal extern static IntPtr objc_msgSend (IntPtr cls, IntPtr sel, Double arg, Int32 idx);
		[DllImport ("/usr/lib/libobjc.dylib")]
		internal extern static IntPtr objc_msgSend (IntPtr cls, IntPtr sel, Int32 arg, Int32 idx);
	}
}
