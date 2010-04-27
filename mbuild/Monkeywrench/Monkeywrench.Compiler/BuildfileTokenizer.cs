//
// Tokenizer.cs -- tokenize a Buildfile
//

// lots of stuff to think about: UTF8 (can't imagine why we'd
// want it.... well, comments in the author's native language),
// different newline conventions, ...

using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Globalization;

namespace Monkeywrench.Compiler {

	// heavily drawn from the MCS C# parser

	public class BuildfileTokenizer : yyParser.yyInput {
		StreamReader reader;
#if MAYBE_LATER
		static NumberFormatInfo finfo;
#endif
		int peek_char;
		int line_num;

		object lexval;
		Hashtable keywords;

		// speeeeed

		const byte alpha_bit = 0x01;
		const byte space_bit = 0x02;
		const byte digit_bit = 0x04;
		const byte hex_bit   = 0x10;
		const byte break_bit = 0x20; // stop reading an identifier on these
		
		byte[] ascii_lut = new byte[128] {
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // 0
			0x00, 0x02, 0x02, 0x02, 0x02, 0x02, 0x00, 0x00,

			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // 16
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,

			0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // 32
			0x20, 0x20, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00,

			0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, // 48
			0x04, 0x04, 0x20, 0x20, 0x00, 0x00, 0x00, 0x00,

			0x00, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x01, // 64
			0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,

			0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, // 80
			0x01, 0x01, 0x01, 0x20, 0x00, 0x20, 0x00, 0x01, // underscore is honorary letter

			0x00, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x01, // 96
			0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,

			0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, // 112
			0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00
		};

#if MAYBE_LATER
		static BuildfileTokenizer () {
			finfo = NumberFormatInfo.InvariantInfo;
		}
#endif

		public BuildfileTokenizer (StreamReader reader) {
			this.reader = reader;
			peek_char = -1;
			line_num = 1;

			keywords = new Hashtable ();
			keywords["using"] = Token.USING;
			keywords["project"] = Token.PROJECT;
			keywords["inside"] = Token.INSIDE;
			keywords["true"] = Token.TRUE;
			keywords["false"] = Token.FALSE;
			keywords["subdirs"] = Token.SUBDIRS;
			keywords["ref"] = Token.REF;
			keywords["with"] = Token.WITH;
			keywords["load"] = Token.LOAD;
			keywords["apply"] = Token.APPLY;
		}

		// StreamReader.Peek () will return -1 on EOS or if the buffer
		// is empty, which confuses us because we want unconditional reads.
		// So we do peeking ourselves.

		int Peek () {
			if (peek_char != -1)
				return peek_char;
			peek_char = reader.Read ();
			return peek_char;
		}

		int Read () {
			if (peek_char == -1)
				return reader.Read ();

			int result = peek_char;
			peek_char = -1;
			return result;
		}

		// here we go

		public bool advance () {
			// ??? ... copied from cs-tokenizer.cs
			return (Peek () != -1);
		}

		public Object value () {
			return lexval;
		}

		public int LineNum { get { return line_num; } }

		public void Cleanup () {
			// check for unfinished token type things???
			// see what MCS does here.
		}

		public int token () {
			chomp ();

			int hint = Peek ();

			if (hint == -1)
				return Token.EOF;

			int val = check_punct ((char) hint);
			if (val != -1) {
				Read ();
				lexval = null;
				return val;
			}

			if (hint == '\"') {
				lexval = do_string ();
				return Token.STRING;
			}

			//if (digit (hint))
			//	return do_number (false);
			
			// ok we have an identifier or keyword

			string id = do_ident (-1);

			if (keywords.Contains (id)) {
				lexval = null;
				return (int) keywords[id];
			}

			lexval = id;
			return Token.IDENTIFIER;
		}

		void chomp () {
			int c;

			while ((c = Peek ()) != -1) {
				//Console.WriteLine ("chomp: {0}", (char) c);

				if (c == '\n') {
					Read ();
					line_num++;
				} else if (space (c))
					Read ();
				else if (c == '#') {
					Read ();

					while ((c = Read()) != '\n') {
					}

					line_num++;
				} else
					return;
			}
		}

		bool space (int c) {
			return (ascii_lut[c] & space_bit) != 0;
		}

#if MAYBE_LATER
		bool alpha (int c) {
			return (ascii_lut[c] & alpha_bit) != 0;
		}
				
		bool digit (int c) {
			return (ascii_lut[c] & digit_bit) != 0;
		}
#endif
						
		bool cbreak (int c) {
			return (ascii_lut[c] & break_bit) != 0;
		}
						
#if MAYBE_LATER
		bool hexletter (int c) {
			return (ascii_lut[c] & hex_bit) != 0;
		}
#endif

		int check_punct (char hint) {
			if (hint == '[')
				return Token.OPEN_BRACKET;
			if (hint == ']')
				return Token.CLOSE_BRACKET;
			if (hint == '(')
				return Token.OPEN_PARENS;
			if (hint == '{')
				return Token.OPEN_BRACE;
			if (hint == ')')
				return Token.CLOSE_PARENS;
			if (hint == '}')
				return Token.CLOSE_BRACE;
			if (hint == ',')
				return Token.COMMA;
			if (hint == ':')
				return Token.COLON;
			if (hint == ';')
				return Token.SEMICOLON;
			if (hint == '=')
				return Token.EQUALS;
			if (hint == '@')
				return Token.ATSIGN;
			if (hint == '!')
				return Token.NOT;
			if (hint == '%')
				return Token.PERCENT;
			if (hint == '?')
				return Token.QUESTION;
			if (hint == '&') {
			    Read ();
			    if ((char) Peek () != '&')
				throw new Exception ("Ampersands must come in pairs in Buildfiles");
			    return Token.BOOL_AND;
			}
			if (hint == '|') {
			    Read ();
			    if ((char) Peek () != '|')
				throw new Exception ("Pipes must come in pairs in Buildfiles");
			    return Token.BOOL_OR;
			}

			return -1;
		}

		string do_string () {
			int c;
			StringBuilder sb = new StringBuilder (16);

			// opening quote
			Read ();
			
			while ((c = Read ()) != -1) {
				int add;

				if (c == '\\') {
					int n = Read ();

					switch (n) {
					case 'n':
						add = '\n';
						break;
					case '"':
						add = '"';
						break;
					case '\'':
						add = '\'';
						break;
					case '\\':
						add = '\\';
						break;
					default:
						throw new Exception (
							String.Format ("Unsupported string escape character " +
								       "\"{0}\".", n));
					}
				} else if (c == '"') {
					return sb.ToString ();
				} else if (c == '\n') {
					throw new Exception ("Newlines in strings not allowed");
					//line_num++;
				} else {
					add = c;
				}
				
				sb.Append ((char) add);
			}

			throw new Exception ("Unexpected EOS while parsing string.");
		}

#if MAYBE_LATER
		int do_number (bool neg) {
			StringBuilder sb = new StringBuilder (8);
			bool hex = false;
			int c;

			c = Peek ();
			if (c == '0') {
				c = Read ();
				c = Peek ();

				if (space (c) || cbreak (c))
					return 0;
				Read ();

				if (c != 'x') 
					throw new Exception ("Octal escapes not allowed in number values: " +
							     "number string starts with 0 but not 0x.");

				hex = true;
			}

			if (neg)
				sb.Append ('-');

			while ((c = Peek ()) != -1) {
				if (cbreak (c))
					break;
				else
					Read ();

				if (c == '\n') {
					line_num++;
					break;
				} else if (space (c)) {
					break;
				} else if (digit (c)) {
					sb.Append ((char) c);
				} else if (hex && hexletter (c)) {
					sb.Append ((char) c);
				} else {
					throw new Exception (
						String.Format ("Unexpected character while parsing number: {0}{1}",
							       sb, (char) c));
				}
			}

			int num;

			//Console.WriteLine ("number parse: {0}", sb.ToString ());

			if (hex)
				num = System.Int32.Parse (sb.ToString (), NumberStyles.HexNumber, finfo);
			else
				num = System.Int32.Parse (sb.ToString (), NumberStyles.Integer, finfo);
			
			return num;
		}
#endif

		string do_ident (int hint) {
			StringBuilder sb = new StringBuilder (16);
			int c;

			if (hint != -1)
				sb.Append ((char) hint);

			while ((c = Peek ()) != -1) {
				if (cbreak (c) || space (c))
					break;
				else
					Read ();

				if (c == '"')
					throw new Exception ("Quote character (\") not allowed inside or at end of atom.");
				sb.Append ((char) c);
			}

			return sb.ToString ();
		}
	}
}
