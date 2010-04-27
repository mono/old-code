using System;
using System.Reflection;

namespace TestLibrary {
        public class StaticTests {
	
		public static int ReturnInt32 () {
			return -1;
		}
		
		public static float ReturnFloat () {
			return -1.0f;
		}

                public static void StaticMethod () {
			Console.WriteLine ("TestLibrary.StaticTests.StaticMethod ()");
                }
        }
} 
