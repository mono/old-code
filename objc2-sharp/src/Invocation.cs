using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ObjC2 {
	public class Invocation : Object {
		private static Class cls = new Class ("NSInvocation");
 
		private Object target;
		private Selector selector;

		public Invocation (MethodSignature signature) {
			if (signature.handle == IntPtr.Zero)
				throw new ArgumentException ("Invalid MethodSignature.");
			handle = Messaging.objc_msgSend (cls.handle, new Selector ("invocationWithMethodSignature:").Handle, signature.handle);
			Connect (this, handle);
		} 

		internal Object Target {
			set {
				target = value;
				Messaging.objc_msgSend (handle, new Selector ("setTarget:").Handle, target.handle);
			}
		}

		internal Selector Selector {
			set {
				selector = value;
				Messaging.objc_msgSend (handle, new Selector ("setSelector:").Handle, selector.handle);
			}
		}

		public object Invoke (Type returntype, object [] arguments) {
			List <IntPtr> buffers = new List <IntPtr> ();
			for (int i = 0; i < arguments.Length; i++) {
				IntPtr buf = IntPtr.Zero;
				if (arguments [i] is Int32) {
					buf = Marshal.AllocHGlobal (Marshal.SizeOf (typeof (Int32)));
					Marshal.WriteInt32 (buf, (Int32) arguments [i]);
				} else if (arguments [i] is IntPtr) { 
					buf = Marshal.AllocHGlobal (Marshal.SizeOf (typeof (IntPtr)));
					Marshal.WriteIntPtr (buf, (IntPtr) arguments [i]);
				} else if (arguments [i] is Single) { 
					buf = Marshal.AllocHGlobal (Marshal.SizeOf (typeof (Single)));
					byte [] data = BitConverter.GetBytes ((Single) arguments [i]);
					
					Marshal.Copy (data, 0, buf, data.Length);
				} else if (arguments [i] is Double) { 
					buf = Marshal.AllocHGlobal (Marshal.SizeOf (typeof (Double)));
					byte [] data = BitConverter.GetBytes ((Double) arguments [i]);
					
					Marshal.Copy (data, 0, buf, data.Length);
				} else if (arguments [i] is bool) { 
					buf = Marshal.AllocHGlobal (Marshal.SizeOf (typeof (bool)));
					byte [] data = BitConverter.GetBytes ((bool) arguments [i]);
					
					Marshal.Copy (data, 0, buf, data.Length);
				} else if (arguments [i] is Object) { 
					buf = Marshal.AllocHGlobal (Marshal.SizeOf (typeof (IntPtr)));
					Marshal.WriteIntPtr (buf, ((Object) arguments [i]).Handle);
				} else
					throw new ArgumentException ("Cannot marshal argument of type: " + arguments [i].GetType ());
				Messaging.objc_msgSend (handle, new Selector ("setArgument:atIndex:").Handle, buf, i+2);

				buffers.Add (buf);
			}

			Messaging.objc_msgSend (handle, new Selector ("invoke").Handle);

			foreach (IntPtr buf in buffers)
				Marshal.FreeHGlobal (buf);

			if (returntype == typeof (void)) {
				return null;
			} else if (returntype == typeof (bool)) {
				bool v = false;
				Messaging.objc_msgSend (handle, new Selector ("getReturnValue:").Handle, ref v);
				return v;
			} else if (returntype == typeof (Int32)) {
				Int32 v = 0;
				Messaging.objc_msgSend (handle, new Selector ("getReturnValue:").Handle, ref v);
				return v;
			} else if (returntype == typeof (IntPtr)) {
				IntPtr v = IntPtr.Zero;
				Messaging.objc_msgSend (handle, new Selector ("getReturnValue:").Handle, ref v);
				return v;
			} else if (returntype == typeof (Double)) {
				double v = 0.0d; 
				Messaging.objc_msgSend (handle, new Selector ("getReturnValue:").Handle, ref v);
				return v;
			} else if (returntype == typeof (Single)) {
				float v = 0.0f; 
				Messaging.objc_msgSend (handle, new Selector ("getReturnValue:").Handle, ref v);
				return v;
			} else if (returntype == typeof (Object)) {
				IntPtr v = IntPtr.Zero;
				Messaging.objc_msgSend (handle, new Selector ("getReturnValue:").Handle, ref v);
				return Object.Get (v);
			}

			throw new ArgumentException ("Unhandled return type: " + returntype.ToString ());
		}
	}
}
