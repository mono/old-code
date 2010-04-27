using System;
using System.Runtime.InteropServices;

namespace ObjC2 {
	public class Selector {
		internal IntPtr handle;
		internal string name;

		public Selector (IntPtr sel) {
			if (!sel_isMapped (sel))
				throw new ArgumentException ("sel is not a selector handle.");

			this.handle = sel;
			name = sel_getName (this.handle);
		}

		public Selector (string name) {
			this.name = name;
			handle = sel_registerName (this.name);
		}

		public IntPtr Handle {
			get { return handle; }
		}

		public string Name {
			get { return name; }
		}

		public static bool operator!= (Selector a, object b) {
			if (b is Selector)
				return !sel_isEqual (a.handle, ((Selector)b).handle);
			
			return true;
		}

		public static bool operator== (Selector a, object b) {
			if (b is Selector)
				return sel_isEqual (a.handle, ((Selector)b).handle);
			
			return false;
		}

		public override bool Equals (object b) {
			if (b is Selector)
				return sel_isEqual (this.handle, ((Selector)b).handle);
			
			return false;
		}

		public override int GetHashCode () {
			return (int) handle;
		}
		
		[DllImport ("/usr/lib/libobjc.dylib")]
		extern static string sel_getName (IntPtr sel);
		[DllImport ("/usr/lib/libobjc.dylib")]
		extern static IntPtr sel_registerName (string name);
		[DllImport ("/usr/lib/libobjc.dylib")]
		extern static bool sel_isMapped (IntPtr sel);
		[DllImport ("/usr/lib/libobjc.dylib")]
		extern static bool sel_isEqual (IntPtr lhs, IntPtr rhs);
	}
}
