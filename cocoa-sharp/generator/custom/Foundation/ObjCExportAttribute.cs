using System;

namespace Apple.Foundation {

    [AttributeUsage(AttributeTargets.Method)]
	public class ExportAttribute : Attribute {
		protected string aSelector;
		protected string aSignature;
		protected bool aAutoSync = true;

		public ExportAttribute() {}
		public ExportAttribute(string selector) {
			this.aSelector = selector;
		}

		public string Selector {
			get { return this.aSelector; }
			set { this.aSelector = value; }
		}

		public string Signature {
			get { return this.aSignature; }
			set { this.aSignature = value; }
		}
		
		public bool AutoSync {
			get { return this.aAutoSync; }
			set { this.aAutoSync = value; }
		}
	}
}
