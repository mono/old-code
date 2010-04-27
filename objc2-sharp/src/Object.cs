using System;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ObjC2 {
	public class Object {
		private static Dictionary <IntPtr, Object> intptr_to_instance = new Dictionary <IntPtr, Object> ();
		private static Dictionary <Object, IntPtr> instance_to_intptr = new Dictionary <Object, IntPtr> ();
		
		internal IntPtr handle;

		static Object () {
			Messaging.objc_msgSend (Messaging.objc_msgSend (new Class ("NSAutoreleasePool").Handle, new Selector ("alloc").Handle), new Selector ("init").Handle);
		}

		public Object () {
			handle = Connect (this);
		}

		public IntPtr Handle {
			get { return handle; }
		}

		public object Call (string method) {
			return Invoke (this, method, new object [0]);
		}
		
		public object Call (string method, object[] args) {
			return Invoke (this, method, args);
		}
		
		public object Call (Object target, string method) {
			return Invoke (target, method, new object [0]);
		}

		public object Call (Object target, string method, object[] args) {
			return Invoke (target, method, args);
		}

		public static MethodInfo GetMethod (IntPtr sel, Object obj) {
			return obj.GetType ().GetMethod (new Selector (sel).Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
		}

		public static IntPtr Allocate (Type type) {
			return Messaging.objc_msgSend (new Class (type).Handle, new Selector ("alloc").Handle);
		}

		public static IntPtr Connect (Object obj) {
			IntPtr instance = Allocate (obj.GetType ());
			Connect (obj, instance);
			return instance;
		}

		public static void Connect (Object obj, IntPtr ptr) {
			intptr_to_instance [ptr] = obj;
			instance_to_intptr [obj] = ptr;
		}

		public static Object Get (IntPtr ptr) {	
			return intptr_to_instance [ptr];
		}
		
		public static IntPtr Get (Object obj) {	
			return instance_to_intptr [obj];
		}

		public static object Invoke (Object obj, string selector) {
			return Invoke (obj, selector, new object [0]);
		}

		public static object Invoke (Object obj, string selector, object [] arguments) {
			MethodInfo minfo = obj.GetType ().GetMethod (selector, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
			Invocation invocation = new Invocation (new MethodSignature (obj, selector)) {Target = obj, Selector = new Selector (selector)} ;

			return invocation.Invoke (minfo.ReturnType, arguments);
		}
	}
}
