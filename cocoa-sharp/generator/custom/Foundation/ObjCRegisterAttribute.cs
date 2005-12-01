using System;

namespace Apple.Foundation {

    [AttributeUsage(AttributeTargets.Class)]
	public class RegisterAttribute : Attribute {
		protected string aName;

		public RegisterAttribute() {}
		public RegisterAttribute(string name) {
			this.aName = name;
		}

		public string Name {
			get { return this.aName; }
			set { this.aName = value; }
		}
	}
}
