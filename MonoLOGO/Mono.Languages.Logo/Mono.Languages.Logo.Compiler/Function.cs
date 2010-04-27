namespace Mono.Languages.Logo.Compiler {
	using System;
	using System.Collections;
	using Mono.Languages.Logo.Runtime;
	
	public class Function {
		string name;
		TokenList tokens;
		InstructionList tree;
		ArgumentInfo[] args;
		MessageInfo desc;

		public string Name {
			get { return name; }
		}

		public MessageInfo Describe () {
			if (desc != null)
				return desc;

			desc = new MessageInfo ();
			desc.message = name;
			desc.min_argc = 0;
			desc.max_argc = args.Length;

			foreach (ArgumentInfo info in args) {
				if (info.val == null && !info.collect)
					desc.min_argc++;
				if (info.collect)
					desc.max_argc = -1;
			}

			desc.default_argc = desc.min_argc;
			return desc;
		}

		public Function (string name, InstructionList tree, ArgumentInfo[] args) {
			this.name = name;
			this.tree = tree;
			this.args = args;
		}

		public Function (string name, TokenList tokens, ArgumentInfo[] args) {
			this.name = name;
			this.tokens = tokens;
			this.args = args;
		}

		public void Parse (Parser parser)
		{
			if (tree == null)
				tree = parser.Parse (tokens);
		}

		private void Debug (ICollection arguments) {
			Console.Write ("Function.cs (Invoke): {0}", name);
			
			foreach (object arg in arguments) {
				Console.Write (" {0}", arg);
			}
			
			Console.WriteLine ();
		}

		public void Invoke (LogoContext context, ICollection arguments, ref object result) {
			// Debug (arguments);

			LogoContext cc = new LogoContext (context);
			Hashtable dict = cc.Dict;

			object[] arguments_list = new object[arguments.Count];
			arguments.CopyTo (arguments_list, 0);

			int i = 0;
			foreach (ArgumentInfo info in args) {
				if (i < arguments_list.Length)
					dict[info.name] = arguments_list[i];
				else if (info.val != null)
					dict[info.name] = info.val;
				i++;
			}

			// Collect remaining arguments
			if (args.Length > 0 && args[args.Length - 1].collect) {
				int col_len = arguments_list.Length - args.Length;
				if (col_len < 0)
					col_len = 0;
				object[] collector = new object[col_len];
				if (col_len > 0)
					Array.Copy (arguments_list, args.Length, collector, 0, col_len);

				dict[args[args.Length - 1].name] = collector;
			}
			
			Interpreter interp = new Interpreter ((Interpreter) context.CallingEngine, cc);
			result = interp.Execute (tree);
		}
	}
}

