using System;
using System.Reflection;

namespace TestLibrary {
        public class ExceptionTests {

		public static void ThrowException () {
			throw new Exception ("Handled exception");
		}
        }
} 
