namespace Mono.Languages.Logo.Compiler {
	public struct Token {
		public TokenType Type;
		public object Val;

		public Token (TokenType type, object val) {
			Type = type;
			Val = val;
		}
	}
}
