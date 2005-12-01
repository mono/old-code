using System;
using System.Reflection;
using System.Runtime.InteropServices; 
using Apple.Foundation;

namespace Apple.AppKit
{
	public class IBOutlet {
		[DllImport("libobjc.dylib")]
                public static extern IntPtr /*(Method)*/ object_getInstanceVariable(IntPtr aClass, string varName, IntPtr ivar);
                [DllImport("libobjc.dylib")]
                public static extern IntPtr /*(Method)*/ object_setInstanceVariable(IntPtr aClass, string varName, IntPtr ivar);
		
		internal NSObject mParent;
		internal string mName;
		internal Type mType;
		internal IntPtr mVar;

		public void setInstanceVariable(object value) {
		}

		public object getInstanceVariable() {
			object retVal;
                        mVar = Marshal.AllocHGlobal (Marshal.SizeOf(typeof(IntPtr)));
			object_getInstanceVariable(mParent.Raw, mName, mVar);
			retVal = mType.IsPrimitive ? Marshal.PtrToStructure(mVar, mType) : NSObject.NS2Net(Marshal.ReadIntPtr(mVar));
			Marshal.FreeHGlobal(mVar);
			return retVal;
		}

		public IBOutlet(NSObject parent, string name, Type type) {
			mParent = parent;
			mName = name;
			mType = type;
		}

		public object Value {
			get { return getInstanceVariable(); }
			set { setInstanceVariable(value); }
		}
	}
}
