namespace Mono.Languages.Logo.Runtime {
	using System;

	[AttributeUsage (AttributeTargets.Method)]
	public class DefaultArgumentCountAttribute : Attribute {
		private int def;
		
		public DefaultArgumentCountAttribute (int def) {
			this.def = def;
		}

		public int DefaultCount {
			get { return def; }
		}
	}
}

