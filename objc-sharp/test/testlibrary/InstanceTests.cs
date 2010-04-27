using System;
using System.Reflection;

namespace TestLibrary {
        public class InstanceTests {
		private int intvalue;
		private float floatvalue; 

		public InstanceTests () {
			intvalue = -1;
			floatvalue = -1.0f;
		}

		public InstanceTests (int intvalue) {
			this.intvalue = intvalue;
		}
		
		public InstanceTests (float floatvalue) {
			this.floatvalue = floatvalue;
		}
		
		public InstanceTests (int intvalue, float floatvalue) {
			this.intvalue = intvalue;
			this.floatvalue = floatvalue;
		}

		public int IntValue {
			get {
				return intvalue;
			}
			set {
				intvalue = value;
			}
		}

		public float FloatValue {
			get {
				return floatvalue;
			}
			set {
				floatvalue = value;
			}
		}

		public int ReturnIntValue () {
			return intvalue;
		}

		public float ReturnFloatValue () {
			return floatvalue;
		}
		
		public bool CompareTo (InstanceTests target) {
			if (this.IntValue == target.IntValue && this.FloatValue == target.FloatValue)
				return true;
			return false;
		}
        }
} 
