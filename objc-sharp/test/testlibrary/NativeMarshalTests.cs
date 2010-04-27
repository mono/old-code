using System;
using System.Reflection;

namespace TestLibrary {
        public class NativeMarshalTests {

		public NativeMarshalTests () {}

		public bool InvokeCompare (int cmpvalue, object target) {
			return (bool)target.GetType ().InvokeMember ("compareInteger", BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, target, new object[] {cmpvalue});
		}
        }
} 
