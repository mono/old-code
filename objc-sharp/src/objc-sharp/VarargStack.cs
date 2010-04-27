using System;
using System.Runtime.InteropServices;

namespace ObjCSharp {
	[StructLayout (LayoutKind.Explicit)]
	public struct VarargStack {
		[FieldOffset (252)]
		public IntPtr stack;
	}
}
