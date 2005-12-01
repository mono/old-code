using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace Apple.Foundation {
        public class Protocol : NSObject {
		private Protocol() : this(IntPtr.Zero,false) {}

		protected internal Protocol(IntPtr raw,bool release) : base(raw,release) {}
        }
}

