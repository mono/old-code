namespace Mono.Languages.Logo.Compiler {
	using System;
	using System.Collections;
	using System.IO;

	using Mono.Languages.Logo.Runtime;
	
	public class CSharpCompiler : Compiler {
		private IndentingWriter writer = new IndentingWriter (Console.Out);
		private int statement_level = 0;

		internal CSharpCompiler (IMessageStoreCollection stores) : base (stores) {
		}

		protected override void CompileStatement (Element elem) {
			statement_level++;

			Type[] arg_types = CollectTypes (elem);
			if (stores.SupportsMessage ((string) elem.Val, arg_types)) {
				CompileTypedStatement (elem, arg_types);
			} else {
				CompileGenericStatement (elem);
			}
			statement_level--;

			if (statement_level == 0) {
				writer.WriteLine (";");
			}
		}

		private Type[] CollectTypes (Element elem) {
			return null;
		}
		
		private void CompileGenericStatement (Element elem) {
			writer.Write ("_funcs.SendMessage (_context, \"{0}\", ", elem.Val);
			CompileList (elem);
			writer.Write (")");
		}

		private void CompileTypedStatement (Element elem, Type[] arg_types) {
			writer.Write ("Funcs.{0} (", elem.Val);
			CompileBareList (elem);
			writer.Write (")");
		}


		private void CompileInfixArg (Element elem) {
			writer.Write ("((double) ");
			Compile (elem);
			writer.Write (")");
		}

		protected override void CompileInfix (Element elem) {
			char op = (char) elem.Val;

			if (op == '^') {
				writer.Write ("System.Math.Pow (");
				CompileInfixArg (elem.Children[0]);
				writer.Write (", ");
				CompileInfixArg (elem.Children[1]);
				writer.Write (")");
				return;
			}

			CompileInfixArg (elem.Children[0]);
			if (op == '=')
				writer.Write ("==");
			else
				writer.Write (op);
			CompileInfixArg (elem.Children[1]);
		}

		protected override void CompileList (Element elem)
		{
			writer.Write ("new object[] {");
			CompileBareList (elem);
			writer.Write ("}");
		}

		private void CompileBareList (Element elem)
		{
			int i = elem.Children.Count - 1;
			foreach (Element subelem in elem.Children) {
				Compile (subelem);
				if (i > 0)
					writer.Write (", ");
				i--;
			}
		}
		
		protected override void CompileLiteral (Element elem)
		{
			object val = elem.Val;
			
			if (val is string)
				writer.Write ("\"{0}\"", val);
			else if (val is char)
				writer.Write ("'{0}'", val);
			else
				writer.Write (val);
		}
		
		protected override void CompileVariable (Element elem)
		{
			writer.Write ("Funcs.Thing (_context, \"{0}\")", (string) elem.Val);
		}
	
		protected override void CompileBeginUnit () {
			writer.WriteLine ("using Mono.Languages.Logo.Runtime;");
			writer.WriteLine ("class X {");
			writer.Indent ();
			writer.WriteLine ("public static void Main () {");
			writer.Indent ();
			writer.WriteLine ("LogoContext _context = new LogoContext (null);");
			writer.WriteLine ("IMessageStoreCollection _funcs = new IMessageStoreCollection ();");
			writer.WriteLine ("_funcs.Add (new CTSMessageTarget (typeof (Funcs), false));");
		}
		
		protected override void CompileFinishUnit () {
			writer.Deindent ();
			writer.WriteLine ("}");
			writer.Deindent ();
			writer.WriteLine ("}");
		}
	}
}

