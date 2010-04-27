namespace Mono.Languages.Logo.Compiler {
	using System;
	using System.Collections;
	using System.IO;
	using System.Text;
	
	public class Tokenizer {
		private bool allow_question_mark = false;

		private static bool IsWhitespace (char c) {
			return (c == ' ' || c == '\t');
		}

		private static bool IsNewline (char c) {
			return (c == '\n');
		}

		private static bool IsInfix (char c) {
			return (c == '+'  ||
					  c == '-'  ||
					  c == '*'  ||
					  c == '/'  ||
					  c == '\\' ||
					  c == '^'  ||
					  c == '='  ||
					  c == '<'  ||
					  c == '>');
		}

		private static bool IsDelimiter (char c) {
			return (IsWhitespace (c) || IsNewline (c) ||
					  IsInfix (c) ||
					  c == '(' ||
					  c == ')' ||
					  c == '[' ||
					  c == ']' ||
					  c == ';');
		}

		private static bool IsSpecial (char c) {
			return (c == '{' ||
					  c == '}' ||
					  c == '|' ||
					  c == '_');
		}

		private static bool IsNumber (string str, out double num) {
			num = 0;

			if (str.Length == 1 && IsInfix (str[0]))
				return false;
			
			try {
				num = Double.Parse (str);
				return true;
			} catch (Exception e) {
				return false;
			}
		}

		private static bool IsNumber (char c) {
			double num;
			return IsNumber (new String (c, 1), out num);
		}

		private static bool IsVariable (string str, out string name) {
			if (str.Length > 1 && str[0] == ':') {
				name = str.Substring (1);
				return true;
			} else {
				name = String.Empty;
				return false;
			}
		}

		private static bool IsString (string str, out string val) {
			if (str.Length > 1 && str[0] == '"') {
				val = str.Substring (1);
				return true;
			} else {
				val = String.Empty;
				return false;
			}
		}

		private static bool IsSymbolStart (char c) {
			return (c == '"' ||
					  c == '|' ||
					  c == ':' ||
					  !(IsDelimiter (c) || IsSpecial (c) || IsNumber (c)));
		}

		public bool AllowQuestionMark {
			get { return allow_question_mark; }
			set { allow_question_mark = value; }
		}

		private Token TokenForChar (int c_prev_int, int c_peek_int, char c) {
			if (c == '-') {
				TokenType type;
				if (c_prev_int == -1)
					type = TokenType.Minus;
				else if (c_peek_int == -1)
					type = TokenType.Infix;
				else {
					char c_prev = (char) c_prev_int;
					char c_peek = (char) c_peek_int;
					if (IsNumber (c_peek) || IsSymbolStart (c_peek) || c_peek == '(') {
						if (c_prev == ')')
							type = TokenType.Infix;
						else
							type = TokenType.Minus;
					} else {
						type = TokenType.Infix;
					}
				}
				return new Token (type, c);
			} else if (c == '(')
				return new Token (TokenType.OpenParens, c);
			else if (c == ')')
				return new Token (TokenType.CloseParens, c);
			else if (c == '[')
				return new Token (TokenType.OpenBracket, c);
			else if (c == ']')
				return new Token (TokenType.CloseBracket, c);
			else if (c == '\n')
				return new Token (TokenType.Newline, c);
			else if (IsInfix (c)) 
				return new Token (TokenType.Infix, c);
			else if (allow_question_mark && c == '?')
				return new Token (TokenType.QuestionMark, c);
			else
				throw new Exception ("Unexpected input: " + c);
		}
		
		private StringBuilder AddTokens (TokenList tokens, StringBuilder builder, int c_prev_int, int c_peek_int) {
			StringBuilder ret;
			string val = builder.ToString ();
			
			if (val.Length > 0) {
				ret = new StringBuilder ();
			} else {
				ret = builder;
			}

			double as_num;
			if (IsNumber (val, out as_num)) {
				Token token = new Token (TokenType.Number, as_num);
				if (tokens[tokens.Count - 1].Type == TokenType.Minus) {
					token.Val = -((double) token.Val);
					tokens[tokens.Count - 1] = token;
				} else {
					tokens.Add (token);
				}
			} else if (val.Length > 1) {
				Token token;
				
				string var_name, str_val;
				if (IsVariable (val, out var_name)) 
					token = new Token (TokenType.Variable, var_name);
				else if (IsString (val, out str_val)) 
					token = new Token (TokenType.String, str_val);
				else
					token = new Token (TokenType.Word, val);

				tokens.Add (token);
			} else if (val.Length == 1) {
				tokens.Add (TokenForChar (c_prev_int, c_peek_int, val[0]));
			}

			return ret;
		}

		private StringBuilder AddTokens (TokenList tokens, StringBuilder builder, int c_prev_int, int c_peek_int, char c) {
			StringBuilder ret = AddTokens (tokens, builder, c_prev_int, c_peek_int);

			if (!(IsWhitespace (c) || IsSpecial (c)))
				tokens.Add (TokenForChar (c_prev_int, c_peek_int, c));

			return ret;
		}

		public TokenList Parse (string content) {
			return Parse (new StringReader (content));
		}

		public TokenList Parse (string[] content) {
			return Parse (String.Join (" ", content));
		}
		
		public TokenList Parse (TextReader reader) {
			TokenList tokens = new TokenList ();
			
			StringBuilder builder = new StringBuilder ();
			int c_int, c_prev_int = -1;
			bool inside_bars = false;
			bool ignore_bar = false;
			bool inside_comment = false; 
			for (c_int = reader.Read (); c_int != -1; c_prev_int = c_int, c_int = reader.Read ()) {
				char c = (char) c_int;

				if (inside_comment) {
					if (IsNewline (c)) {
						inside_comment = false;
					} else {
						continue;
					}
				}
				
				if (c == '|') {
					if (ignore_bar) {
						ignore_bar = false;
						continue;
					}
					
					if (inside_bars && reader.Peek () == (int) '|') {
						ignore_bar = true;
						builder.Append ('|');
						continue;
					}
					
					inside_bars = !inside_bars;
					continue;
				}

				if (inside_bars) {
					builder.Append (c);
					continue;
				}

				if (c == '_' && tokens.Count > 0 && tokens[tokens.Count - 1].Type == TokenType.Newline) {
					tokens.RemoveAt (tokens.Count - 1);
					continue;
				} else if (c == ';') {
					inside_comment = true;
					continue;
				} else if (!IsDelimiter (c) || c_prev_int == (int) '"') {
					builder.Append (c);
					continue;
				}

				builder = AddTokens (tokens, builder, c_prev_int, reader.Peek (), c);
			}
			
			builder = AddTokens (tokens, builder, c_prev_int, reader.Peek ()); 

			return tokens;
		}
	}
}

