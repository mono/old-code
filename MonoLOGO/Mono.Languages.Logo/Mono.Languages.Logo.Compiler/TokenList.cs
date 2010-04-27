namespace Mono.Languages.Logo.Compiler {

	using System.Collections;
	
	public class TokenList : CollectionBase {
		public TokenList (ICollection c) {
			foreach (Token token in c) {
				Add (token);
			}
		}
		
		public TokenList () {
		}

		public int Add (Token token) {
			return List.Add (token);
		}

		public Token this[int index] {
			get {
				return (Token) List[index];
			}
			set {
				List[index] = value;
			}
		}

		public void Extend (ICollection tokens) {
			foreach (Token token in tokens) {
				List.Add (token);
			}
		}

		public void Reverse () {
			IList list = List;
			int length = list.Count;
			for (int i = 0; i < length / 2; i++) {
				object tmp = list[i];
				list[i] = list[length - i - 1];
				list[length - i - 1] = tmp;
			}
		}
	}
}

