using System;

namespace ObjCSharp {
	internal class NativeMember {
		private string name;
		private string type;
		private int size;

		public NativeMember (string name, string type, int size) {
			this.name = name;
			this.type = type;
			this.size = size;
		}

		public string Name {
			get { return this.name; }
			set { this.name = value; }
		}

		public string Type {
			get { return this.type; }
			set { this.type = value; }
		}

		public int Size {
			get { return this.size; }
			set { this.size = value; }
		}
	}
}
