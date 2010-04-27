using TUVienna.CS_CUP.Runtime;
using System;
using System.Reflection;


namespace PHP.Compiler {


	public class Token : Symbol {

		public Token(int id, int line, int column, string text)
			: base(id, line, column, text) {
		}

		public string TokenName() {
			ParserSymbols symbols = new ParserSymbols();
			Type the_type = typeof(ParserSymbols);
			FieldInfo[] field_infos = the_type.GetFields();
			foreach (FieldInfo fi in field_infos) {
				int field_value = (int)fi.GetValue(symbols);
				if (field_value == Id())
					return fi.Name;
			}
			return null;
		}

		public int Id() {
			return sym;
		}

		public int Line() {
			return left;
		}

		public int Column() {
			return right;
		}

		public string Text() {
			try {
				return (string)value;
			}
			catch (Exception) {
				Console.Out.WriteLine("Couldn't cast to string!");
				return "This should be the value!";
			}
		}

		public override string ToString() {
			return "Token " + TokenName() + " (" + Line() + "," + Column() + "): " + Text();
		}

	}


}