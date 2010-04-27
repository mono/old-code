namespace Mono.Languages.Logo.Compiler {
	using System;
	using System.Collections;
	using System.IO;

	using Mono.Languages.Logo.Runtime;

	public class Parser {
		private Tokenizer tokenizer = new Tokenizer ();
		private IMessageStoreCollection stores;
		private LogoMessageTarget funcs;
		private ArrayList to_parse;
		
		public Parser () {
			stores = new IMessageStoreCollection ();
		}

		public Parser (IMessageStoreCollection stores, LogoMessageTarget funcs) {
			this.stores = stores;
			this.funcs = funcs;
		}

		public bool AllowQuestionMark {
			get { return tokenizer.AllowQuestionMark; }
			set { tokenizer.AllowQuestionMark = value; }
		}

		public InstructionList Parse (TextReader reader) {
			return Parse (tokenizer.Parse (reader));
		}

		public InstructionList Parse (string content) {
			return Parse (tokenizer.Parse (content));
		}

		public InstructionList Parse (string[] content) {
			return Parse (tokenizer.Parse (content));
		}

		public InstructionList Parse (TokenList tokens) {
			TokenList partial = ParseForwards (tokens);
			partial = ParseInfix (partial);
			InstructionList tree = ParseBackwards (partial, false);

			if (to_parse != null) {
				// Null out the member before going re-entrant.
				// Otherwise we could go into an infinite loop.
				ArrayList parse_now = to_parse;
				to_parse = null;

				foreach (Function f in parse_now) {
					f.Parse (this);
				}
			}

			return tree;
		}

		private Element ParseList (IEnumerator iterator) {
			InstructionList list = new InstructionList ();
			while (iterator.MoveNext ()) {
				Token token = (Token) iterator.Current;
				if (token.Type == TokenType.CloseBracket) 
					break;
				else if (token.Type == TokenType.OpenBracket) 
					list.Add (ParseList (iterator));
				else 	
					list.Add (new Element (ElementType.Literal, token.Val));
			}
			
			return new Element (ElementType.List, null, list);
		}

		private int CountArgs (string word, bool use_maximum) {
			MessageInfo info = stores.DescribeMessage (word);
			if (use_maximum)
				return info.max_argc;
			else
				return info.default_argc;
		}

		private TokenList ParseForwards (TokenList tokens) {
			TokenList partial = new TokenList ();
			ParseForwards (partial, tokens.GetEnumerator (), 0);
			return partial;
		}

		private TokenList ParseBrackets (IEnumerator iterator) {
			TokenList list = new TokenList ();

			while (iterator.MoveNext ()) {
				Token token = (Token) iterator.Current;
				if (token.Type == TokenType.CloseBracket) 
					break;
				else 	
					list.Add (token);
			}
			
			return list;
		}
		
		private void PrintFunction (TokenList func) {
			Console.Write ("function {0}", (string) func[0].Val);
			foreach (Token token in (TokenList) func[1].Val) {
				if (token.Type == TokenType.Variable)
					Console.Write (" :{0}", token.Val);
				else if (token.Type == TokenType.PlaceholderElement) {
					TokenList arg = (TokenList) ((Element) token.Val).Val;
					Console.Write (" [");
					int j = 0;
					foreach (Token t in arg) {
						if (j == 0)
							Console.Write (":");
						Console.Write (t.Val);
						if (j < (arg.Count - 1))
							Console.Write (" ");
						j++;
					}
					Console.Write ("]");
				}
			}
			Console.WriteLine ();

			bool print_space = false;
			bool print_tab = true;
			foreach (Token token in (TokenList) func[2].Val) {
				if (print_space && token.Type != TokenType.Newline)
					Console.Write (" ");
				else
					print_space = true;

				if (print_tab) {
					Console.Write ("\t");
					print_tab = false;
				}

				if (token.Type == TokenType.Newline) {
					print_tab = true;
					print_space = false;
				}
					
				Console.Write ("{0}", token.Val);
			}

			Console.WriteLine ();
		}

		private Function CreateFunction (TokenList func) {
			string name = (string) func[0].Val;
			
			ArrayList args_list = new ArrayList ();
			foreach (Token token in (TokenList) func[1].Val) {
				ArgumentInfo info = new ArgumentInfo ();

				if (token.Type == TokenType.Variable)
					info = new ArgumentInfo ((string) token.Val, null, false);
				else if (token.Type == TokenType.PlaceholderElement) {
					TokenList arg = (TokenList) ((Element) token.Val).Val;
					// FIXME: What if it is longer than 2 elements?
					if (arg.Count > 1)
						info = new ArgumentInfo ((string) arg[0].Val, arg[1].Val, false);
					else
						info = new ArgumentInfo ((string) arg[0].Val, null, true);
				}
				
				args_list.Add (info);
			}
			ArgumentInfo[] args = (ArgumentInfo[]) args_list.ToArray (typeof (ArgumentInfo)); 
			TokenList tokens = (TokenList) func[2].Val;

			return new Function (name, tokens, args);
		}

		private enum ParseToState {
			FunctionName,
			Argument,
			ArgumentWithValue,
			ArgumentCollector,
			Content
		}

		private ParseToState AdvanceState (ParseToState state) {
			return (ParseToState) ((int) state + 1);
		}
	
		private Token ParseTo (IEnumerator iterator) {
			ParseToState state = ParseToState.FunctionName;
			Token name = new Token (TokenType.Word, null);
			TokenList args = new TokenList (); 
			TokenList def = new TokenList ();
			
			bool reading = true;
			bool prev_newline = false;
			while (reading && iterator.MoveNext ()) {
				Token token = (Token) iterator.Current;

				if (state != ParseToState.Content && token.Type == TokenType.Newline) {
					state = ParseToState.Content;
					prev_newline = true;
					continue;
				}
				
				switch (state) {
				case ParseToState.FunctionName:
					name = token;
					state = AdvanceState (state);
					break;
				case ParseToState.Argument:
				case ParseToState.ArgumentWithValue:
				case ParseToState.ArgumentCollector:
					if (token.Type == TokenType.OpenBracket) {
						TokenList arg = ParseBrackets (iterator);
						if (state == ParseToState.Argument || (state == ParseToState.ArgumentWithValue && arg.Count == 1)) {
							state = AdvanceState (state);
						}
						args.Add (new Token (TokenType.PlaceholderElement, new Element (ElementType.List, arg)));
					} else {
						args.Add (token);
					}
					break;
				case ParseToState.Content:
					if (prev_newline && token.Type == TokenType.Word && String.Compare ((string) token.Val, "end", true) == 0) {
						reading = false;
					} else {
						def.Add (token);
						prev_newline = (token.Type == TokenType.Newline); 
					}
					break;
				}
			}

			TokenList func = new TokenList ();
			func.Add (name);
			func.Add (new Token (TokenType.PlaceholderGroup, args));
			func.Add (new Token (TokenType.PlaceholderGroup, def));

			Function func_obj = CreateFunction (func);
			funcs.AddMessage (func_obj);

			if (to_parse == null)
				to_parse = new ArrayList ();
			to_parse.Add (func_obj);

			return new Token (TokenType.PlaceholderElement, new Element (ElementType.Function, func_obj));
		}

		private void ParseForwards (TokenList partial, IEnumerator iterator, int parens) {
			while (iterator.MoveNext ()) {
				Token token = (Token) iterator.Current;
				switch (token.Type) {
				case TokenType.OpenBracket:
					partial.Add (new Token (TokenType.PlaceholderElement, ParseList (iterator)));
					break;
				case TokenType.OpenParens:
					TokenList inner = new TokenList ();
					ParseForwards (inner, iterator, parens + 1);
					partial.Add (new Token (TokenType.PlaceholderGroup, inner));
					break;
				case TokenType.CloseParens:
					return;
				case TokenType.Word:
					if (String.Compare ((string) token.Val, "to", true) == 0) {
						// FIXME: Usual case is in void context
						// token = ParseTo (iterator);
						ParseTo (iterator);
					} else {
						partial.Add (token);
					}
					break;
				default:
					partial.Add (token);
					break;
				}
			}
		}

		private void ExtendList (TokenList list, Stack stack) {
			Array tokens = stack.ToArray ();
			Array.Reverse (tokens);
			list.Extend (tokens);
		}

		private int Weight (Token token) {
			char c = (char) token.Val;
			if (c == '^') {
				return 4;
			} else if (c == '*' || c == '/' || c == '\\') {
				return 3;
			} else if (c == '+' || c == '-') {
				return 2;
			} else if (c == '=' || c == '<' || c == '>') {
				return 1;
			} else {
				return 0;
			}
		}

		private TokenList ParseInfix (TokenList partial) {
			TokenList output = new TokenList ();
			Stack stack = new Stack ();
			bool last_was_operand = false;

			int length = partial.Count;
			for (int i = length - 1; i >= 0; i--) {
				Token token = partial[i];
				if (token.Type == TokenType.Word) {
					ExtendList (output, stack);
					output.Add (token);
					stack.Clear ();
				} else if (token.Type == TokenType.Infix) {
					if (stack.Count == 0) {
						stack.Push (token);
					} else {
						Token prev = (Token) stack.Pop ();
						if (Weight (prev) <= Weight (token)) {
							stack.Push (token);
							stack.Push (prev);
						} else {
							output.Add (prev);
							stack.Push (token);
						}
					}
					last_was_operand = false;
				} else {
					if (token.Type == TokenType.PlaceholderGroup) {
						token.Val = ParseInfix ((TokenList) token.Val);
					}

					if (i > 0) {
						Token peek = partial[i - 1];
						if (peek.Type == TokenType.Minus) {
							output.Add (token);
							output.Add (peek);
							last_was_operand = true;
							i--;
							continue;
						}
					}
					
					if (last_was_operand) {
						ExtendList (output, stack);
						stack.Clear ();
					}
					output.Add (token);
					last_was_operand = true;
				}
			}

			ExtendList (output, stack);
			output.Reverse ();
			return output;
		}
		
		private InstructionList ParseBackwards (TokenList tokens, bool grouped) {
			Stack stack = new Stack ();
			
			for (int i = tokens.Count - 1; i >= 0; i--) {
				Token token = tokens[i];
				
				switch (token.Type) {
				case TokenType.Number:
				case TokenType.String:
					stack.Push (new Element (ElementType.Literal, token.Val));
					break;
				case TokenType.PlaceholderElement:
					stack.Push (token.Val);
					break;
				case TokenType.PlaceholderGroup:
					InstructionList group = ParseBackwards ((TokenList) token.Val, true);
					if (group.Count == 1) {
						stack.Push (group[0]);
					} else if (group.Count == 0) {
					} else {
						throw new Exception ("Unexpected grouping");
					}
					break;
				case TokenType.Word:
				case TokenType.Infix:
				case TokenType.Minus:
					InstructionList inner = new InstructionList ();

					int count;
					ElementType etype;
					if (token.Type == TokenType.Word) {
						count = CountArgs ((string) token.Val, grouped && i == 0);
						if (count == -1)
							count = stack.Count;
						etype = ElementType.Statement;
					} else if (token.Type == TokenType.Minus) {
						count = 1;
						etype = ElementType.Statement;
						token.Val = "Minus";
					} else {
						count = 2;
						etype = ElementType.Infix;
					}
					
					for (int j = 0; j < count; j++) 
						inner.Add ((Element) stack.Pop ());

					stack.Push (new Element (etype, token.Val, inner));
					break;
				case TokenType.Newline:
					break;
				case TokenType.Variable:
					stack.Push (new Element (ElementType.Variable, token.Val));
					break;
				case TokenType.QuestionMark:
					// Don't need to check AllowQuestionMark here as the
					// tokenizer checks already,
					stack.Push (new Element (ElementType.QuestionMark, token.Val));
					break;
				case TokenType.PlaceholderFunction:
				default:
					throw new Exception ("Unexpected token: " + token.Type + "<" + token.Val + ">");
				}
			}

			InstructionList tree = new InstructionList ();
			object[] toplevel = stack.ToArray ();
			foreach (object o in toplevel) {
				tree.Add ((Element) o);
			}

			return tree;
		}
	}
}

