using System;

namespace ObjC2 {
	[AttributeUsage(AttributeTargets.Method|AttributeTargets.Constructor)]
	public class ExportAttribute : Attribute {
		protected string selector;
		protected string signature;
		protected bool autosync = true;

		public ExportAttribute() {}
		public ExportAttribute(string selector) {
			this.selector = selector;
		}

		public string Selector {
			get { return this.selector; }
			set { this.selector = value; }
		}

		public string Signature {
			get { return this.signature; }
			set { this.signature = value; }
		}
		
		public bool AutoSync {
			get { return this.autosync; }
			set { this.autosync = value; }
		}
	}
}
