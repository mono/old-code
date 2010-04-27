namespace Mono.Languages.Logo.Compiler {

	using System.Collections;
	
	public class InstructionList : CollectionBase {
		public InstructionList (ICollection c) {
			foreach (Element elem in c) {
				Add (elem);
			}
		}

		public InstructionList () {
		}

		public int Add (Element elem) {
			return List.Add (elem);
		}
		
		public void Insert (int index, Element elem) {
			List.Insert (index, elem);
		}

		public Element this[int index] {
			get {
				return (Element) List[index];
			}
			set {
				List[index] = value;
			}
		}
	}
}

