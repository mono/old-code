namespace Mono.Languages.Logo.Compiler {

	public class Element {
		
		private ElementType type;
		private InstructionList children;
		private object val;
		
		public ElementType Type {
			get { return type; }
			set { type = value; }
		}

		public InstructionList Children {
			get { return children; }
			set { children = value; }
		}

		public object Val {
			get { return val; }
			set { val = value; }
		}

		public Element (ElementType type, object val) {
			this.type = type;
			this.val = val;
			this.children = null;
		}

		public Element (ElementType type, object val, InstructionList children) {
			this.type = type;
			this.val = val;
			this.children = children;
		}
	}
}

