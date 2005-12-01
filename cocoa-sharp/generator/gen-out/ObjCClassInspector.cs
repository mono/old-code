using System;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices; 

namespace CocoaSharp {

	public class ObjCClassInspector {
		#region -- Public API --
		public static void AddBundle(string bundleName) {
#if !WINDOWS
			if(!ObjCBundles.Contains(bundleName)) {
				IntPtr objcBundleName = CreateObjCString("/System/Library/Frameworks/" + bundleName + ".framework");
				IntPtr bundle = objc_msgSend(objc_getClass("NSBundle"), sel_registerName("bundleWithPath:"), objcBundleName);
				objc_msgSend(bundle, sel_registerName("load"));
				ObjCBundles[bundleName] = true;
			}
#endif
		}

		public static bool IsObjCClass(string className) {
#if !WINDOWS
			if(ObjCClasses.Contains(className))
				return (bool)ObjCClasses[className];

			IntPtr /*(Class)*/ classPtr = GetObjCClass(className);

			ObjCClasses[className] = classPtr != IntPtr.Zero;
			return classPtr != IntPtr.Zero;
#else
			return true;
#endif
		}

		public static string GetSignature(string className,string selector) {
			IntPtr /*(Method)*/ methodPtr;
			IntPtr /*(Class)*/ classPtr = GetObjCClass(className);
			IntPtr /*(SEL)*/ selectorPtr = GetObjCSelector(selector.Substring(1));
			if (selector.StartsWith("-"))
				methodPtr = class_getInstanceMethod(classPtr,selectorPtr);
			else
				methodPtr = class_getClassMethod(classPtr,selectorPtr);

			if (methodPtr == IntPtr.Zero) {
				if (classPtr != IntPtr.Zero)
					Console.WriteLine("WARNING: method not found {0}({1}) @ {2}({3}) --> {4}",className,classPtr,selector,selectorPtr,methodPtr);
				return null;
			}
				
			objc_method method = (objc_method)Marshal.PtrToStructure(methodPtr,typeof(objc_method));
			return Marshal.PtrToStringAnsi(method.method_types);
		}
		#endregion

		#region -- Core Objective-C functions --		
		public static IntPtr /*(NSString*)*/ CreateObjCString(string toConvert) {
			if (sPool == IntPtr.Zero) sPool = objc_msgSend(objc_getClass("NSAutoreleasePool"), GetObjCSelector("new"));

			return objc_msgSend(objc_getClass("NSString"), GetObjCSelector("stringWithCString:"), toConvert);
		}
		public static void ReleaseObjCObject(IntPtr /*(id)*/ toRelease) {
			objc_msgSend(toRelease, GetObjCSelector("release"));
		}
		public static IntPtr /*(Class)*/ GetObjCClass(string className) {
			return NSClassFromString(CreateObjCString(className));
		}
		public static IntPtr /*(SEL)*/ GetObjCSelector(string selector) {
			return sel_registerName(selector);
		}
		public static string GetObjCSelectorName(IntPtr /*(SEL)*/ selector) {
			return Marshal.PtrToStringAnsi(sel_getName(selector));
		}
		#endregion
		
		#region -- Members --
		private static IDictionary ObjCClasses = new Hashtable();
		private static IDictionary ObjCBundles = new Hashtable();
		private static IntPtr sPool;
		#endregion

		#region -- Objective-C structures --
		public struct objc_method {
			public IntPtr /*(SEL)*/ method_name;
			public IntPtr /*(char *)*/ method_types;
			public IntPtr /*(IMP)*/ method_imp;
		}
		#endregion

		#region -- PInvoke bindings --
		[DllImport("libobjc.dylib")]
		public static extern IntPtr /*(Class)*/ objc_getClass(string className);
		[DllImport("libobjc.dylib")]
		public static extern IntPtr /*(id)*/ objc_msgSend(IntPtr /*(id)*/ basePtr, IntPtr /*(SEL)*/ selector);
		[DllImport("libobjc.dylib")]
		public static extern IntPtr /*(id)*/ objc_msgSend(IntPtr /*(id)*/ basePtr, IntPtr /*(SEL)*/ selector, IntPtr /*(id)*/ argument);
		[DllImport("libobjc.dylib")]
		public static extern IntPtr /*(id)*/ objc_msgSend(IntPtr /*(id)*/ basePtr, IntPtr /*(SEL)*/ selector, string argument);
		
		[DllImport("libobjc.dylib")]
		public static extern IntPtr /*(SEL)*/ sel_registerName(string selectorName);
		[DllImport("libobjc.dylib")]
		public static extern IntPtr /*(const char*)*/ sel_getName(IntPtr /*(SEL)*/ aSelector);
		
		[DllImport("libobjc.dylib")]
		public static extern IntPtr /*(Method)*/ class_getInstanceMethod(IntPtr /*(Class)*/ aClass, IntPtr /*(SEL)*/ aSelector);
		[DllImport("libobjc.dylib")]
		public static extern IntPtr /*(Method)*/ class_getClassMethod(IntPtr /*(Class)*/ aClass, IntPtr /*(SEL)*/ aSelector);
		
		[DllImport("libobjc.dylib")]
		public static extern uint method_getNumberOfArguments(IntPtr /*(Method)*/ method);
		[DllImport("libobjc.dylib")]
		public static extern uint method_getSizeOfArguments(IntPtr /*(Method)*/ method);
		[DllImport("libobjc.dylib")]
		public static extern uint method_getArgumentInfo(IntPtr /*(Method)*/ method, int argIndex, ref IntPtr /*(const char**)*/ type, ref int /*(int*)*/ offset);

		[DllImport("/System/Library/Frameworks/Foundation.framework/Foundation")]
		protected static extern IntPtr /*(Class)*/ NSClassFromString(IntPtr /*(NSString*)*/ classNamePtr);
		#endregion
	}
}
