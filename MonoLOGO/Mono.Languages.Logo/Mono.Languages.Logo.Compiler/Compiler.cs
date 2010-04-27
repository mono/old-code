namespace Mono.Languages.Logo.Compiler {
	using System;
	using System.Collections;
	using System.IO;

	using Mono.Languages.Logo.Runtime;
	
	public abstract class Compiler {
		protected IMessageStoreCollection stores;
		private LogoContext context;

		protected Compiler (IMessageStoreCollection stores) {
			this.stores = stores;
			context = new LogoContext (null);
		}

		protected Compiler () {
		}

		public static Compiler Create (IMessageStoreCollection stores) {
			return new CSharpCompiler (stores);
		}

		protected abstract void CompileBeginUnit ();
		protected abstract void CompileFinishUnit ();
		protected abstract void CompileStatement (Element elem);	
		protected abstract void CompileInfix (Element elem);
		protected abstract void CompileList (Element elem);
		protected abstract void CompileLiteral (Element elem);
		protected abstract void CompileVariable (Element elem);
	
		public void Compile (InstructionList list) {
			CompileBeginUnit ();
			foreach (Element elem in list) {
				Compile (elem);
			}
			CompileFinishUnit ();
		}
		
		public void Compile (Element elem) {
			switch (elem.Type) {
			case ElementType.Literal:
				CompileLiteral (elem);
				break;
			case ElementType.List:
				CompileList (elem);
				break;
			case ElementType.Statement:
				CompileStatement (elem);
				break;
			case ElementType.Infix:
				CompileInfix (elem);
				break;
			case ElementType.Variable:
				CompileVariable (elem);
				break;
			default:
				throw new Exception ();
			}
		}
	}
}
