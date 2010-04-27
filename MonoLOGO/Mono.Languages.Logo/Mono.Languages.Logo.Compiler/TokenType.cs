namespace Mono.Languages.Logo.Compiler {
	public enum TokenType {
		Word,
		Number,
		String,
		Minus,
		Infix,
		OpenParens,
		CloseParens,
		OpenBracket,
		CloseBracket,
		Variable,
		Newline,
		QuestionMark,
		PlaceholderGroup,
		PlaceholderElement,
		PlaceholderFunction
	}
}
