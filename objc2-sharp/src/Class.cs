using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ObjC2 {
	public delegate string TypeTranslatorDelegate (string type);

	public class Class {
		internal IntPtr handle;
		private static TypeTranslatorDelegate translator;

		public Class (string name) {
			this.handle = objc_getClass (name);
			
			if (this.handle == IntPtr.Zero)
				throw new ArgumentException ("name is an unknown class", name);
		}

		public Class (Type type) {
			if (type == null)
				throw new ArgumentException ("type cannot be null");

			RegisterType (type);
		}

		public IntPtr Handle {
			get { return this.handle; }
		}

		public static TypeTranslatorDelegate TypeTranslator {
			set {
				translator = value;
			}
		}

		private void RegisterType (Type type) {
			string name = type.Name;

			if (translator != null)
				name = translator (name);

			handle = objc_getClass (name);

			if (handle != IntPtr.Zero)
				return;

			//TODO: Dont derive everything from NSObject
			handle = objc_allocateClassPair (objc_getClass ("NSObject"), name, IntPtr.Zero);

			//TODO: ivars? what do we really want to do here
			
			//TODO: what binding flags do we really want here
			foreach (MethodInfo minfo in type.GetMethods (BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)) {
				if (Attribute.GetCustomAttribute (minfo, typeof (ExportAttribute)) != null) {
					Method method = new Method (minfo);
					if (!class_addMethod (handle, method.Selector, method.Delegate, method.Signature))
						throw new Exception ("Could not add native class representation for: " + minfo.Name);
				}
			}

			objc_registerClassPair (handle);			
		}

		[DllImport ("/usr/lib/libobjc.dylib")]
		extern static IntPtr objc_allocateClassPair (IntPtr superclass, string name, IntPtr extraBytes);
		[DllImport ("/usr/lib/libobjc.dylib")]
		extern static IntPtr objc_getClass (string name);
		[DllImport ("/usr/lib/libobjc.dylib")]
		extern static void objc_registerClassPair (IntPtr cls);
		[DllImport ("/usr/lib/libobjc.dylib")]
		extern static bool class_addMethod (IntPtr cls, IntPtr name, Delegate imp, string types);
	}
}
