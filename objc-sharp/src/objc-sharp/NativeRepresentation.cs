using System;

namespace ObjCSharp {
	internal class NativeRepresentation {
		private string[] constructors;
		private string[] constructor_signatures;
		private string[] methods;
		private string[] signatures;
		private string[] staticmethods;
		private string[] staticsignatures;
		private NativeMember[] members;

		public string[] Constructors {
			get { return this.constructors; }
			set { this.constructors = value; }
		}
		
		public string[] ConstructorSignatures {
			get { return this.constructor_signatures; }
			set { this.constructor_signatures = value; }
		}
		
		public string[] StaticMethods {
			get { return this.staticmethods; }
			set { this.staticmethods = value; }
		}
		
		public string[] StaticSignatures {
			get { return this.staticsignatures; }
			set { this.staticsignatures = value; }
		}

		public string[] Methods {
			get { return this.methods; }
			set { this.methods = value; }
		}
		
		public string[] Signatures {
			get { return this.signatures; }
			set { this.signatures = value; }
		}
		
		public NativeMember[] Members {
			get { return this.members; }
			set { this.members = value; }
		}
	}
}
