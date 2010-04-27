//
// Formula.cs
//
// Authors:
//   Miguel de Icaza (miguel@novell.com)
//
// Copyright 2008 Novell, Inc (http://www.novell.com).
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Collections;
using System.Text;

namespace Wahid {

	public class Formula {

		class Tokenizer {
			StringBuilder sb = new StringBuilder ();
			int len;
			int pos;
			string s;
			object putback;
			
			public Tokenizer (string s)
			{
				this.s = s.Trim ();
				len = s.Length;
			}

			double GetFraction (StringBuilder sb)
			{
				sb.Append (s[pos++]);
				if (pos < len && Char.IsDigit (s [pos])){
					while (pos < len && Char.IsDigit (s [pos]))
						sb.Append (s [pos++]);
				}
				if (pos < len && s [pos] == 'e' || s [pos] == 'E'){
					sb.Append (s [pos++]);
					if (pos < len && s [pos] == '+' || s [pos] == '-')
						sb.Append (s [pos++]);
					if (pos < len && Char.IsDigit (s [pos]))
						while (pos < len && Char.IsDigit (s [pos]))
							sb.Append (s [pos++]);
				}
				return double.Parse (sb.ToString ());
			}
			
			double GetNumber ()
			{
				sb.Length = 0;
				
				while (pos < len && Char.IsDigit (s [pos]))
					sb.Append (s [pos++]);
				if (pos < len && s [pos] == '.')
					return GetFraction (sb);

				return double.Parse (sb.ToString ());
			}
			
			ErrorValue TryError (string error, ErrorValue ret)
			{
				if (error == null)
					throw new Exception ("Invalid error");
				
				string sub = s.Substring (pos);
				
				if (s.StartsWith (error)){
					pos += error.Length;
					return ret;
				}
				return null;
			}
			
			object GetToken ()
			{
				if (putback != null){
					object p = putback;
					putback = null;
					return p;
				}
				
				while (pos < len){
					char c = s [pos];
					
					if (c >= '0' && c <= '9')
						return GetNumber ();
					
					if (c == '.'){
						sb.Length = 0;
						return GetFraction (sb);
					}
					
					if (c == '#'){
						return TryError ("#DIV/0!", ErrorValue.DivisionByZero) ??
							TryError ("#N/A!",   ErrorValue.NA) ??
							TryError ("#NAME?",  ErrorValue.Name) ??
							TryError ("#NULL?",  ErrorValue.Null) ??
							TryError ("#NUM!",   ErrorValue.Num) ??
							TryError ("#REF!",   ErrorValue.Ref) ??
							TryError (null, null);
					}
					if (c == '"'){
						int start = ++pos;
						while (pos < len && s [pos] != '"')
							pos++;
						if (pos < len)
							return new StringValue (s.Substring (start, pos-start));
						throw new Exception ("Unfinished quote");
					}

					if (c == ':' || c == ',' || c == ' ' || c == '^' || c == '*' || c == '/' || c == '+' ||
					    c == '-' || c == '&' || c == '=' || c == '%' || c == '[' || c == ']' || c == '!'){
						pos++;
						return c;
					}

					if (c == '>'){
						if (pos + 1 < len && s [pos+1] == '='){
							pos += 2;
							return ">=";
						}
						pos++;
						return '>';
					}

					if (c == '<'){
						if (pos + 1 < len && s [pos+1] == '='){
							pos += 2;
							return "<=";
						}

						if (pos + 1 < len && s [pos+1] == '>'){
							pos += 2;
							return "<>";
						}
						pos++;
						return '<';
					}

					if (c == '[' || c == ']'){
						return c;
					}


				}
				return null;
			}

			void PutBack (object a)
			{
				putback = a;
			}
		}


		public Formula (string s)
		{
			// Parse the beast.
		}
	}
}