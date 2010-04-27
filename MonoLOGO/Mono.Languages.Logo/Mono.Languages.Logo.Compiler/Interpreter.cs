namespace Mono.Languages.Logo.Compiler {
	using System;
	using System.Collections;
	using System.IO;

	using Mono.Languages.Logo.Runtime;
	
	public class Interpreter {
		private IMessageStoreCollection stores;
		private LogoContext context;
		private object[] template_args;

		public Interpreter (IMessageStoreCollection stores) {
			this.stores = stores;
			context = new LogoContext (null);
		}

		public Interpreter (Interpreter interp, LogoContext context) {
			this.stores = interp.stores;
			this.context = context;
		}

		private object[] CollectArgs (InstructionList children) {
			object[] args = new object[children.Count];

			int i = 0;
			foreach (Element subelem in children) {
				object val;
				switch (subelem.Type) {
				case ElementType.Literal:
					val = subelem.Val;
					break;
				case ElementType.Infix:
					val = ExecuteInfix (subelem);
					break;
				case ElementType.List:
					val = ConstructList (subelem);
					break;
				case ElementType.Statement:
					val = ExecuteStatement (subelem);
					break;
				case ElementType.Variable:
					val = Funcs.Thing (context, (string) subelem.Val);
					break;
				default:
					throw new Exception ();
				}

				args[i] = val;
				i++;
			}

			return args;
		}

		private object ExecuteStatement (Element elem) {
			object ret = null;
			object[] args = CollectArgs (elem.Children);
			
			context.CallingEngine = this;
			ret = stores.SendMessage (context, (string) elem.Val, args);
			context.CallingEngine = null;
			return ret;
		}
	
		private object ExecuteInfix (Element elem) {
			object[] args = CollectArgs (elem.Children);
			double a = (double) args[0];
			double b = (double) args[1];
			switch ((char) elem.Val) {
			case '+':
				return a + b;
			case '-':
				return a - b;
			case '*':
				return a * b;
			case '/':
				return a / b;
			case '^':
				return Math.Pow (a, b);
			case '=':
				return a == b;
			case '<':
				return a < b;
			case '>':
				return a > b;
			default:
				throw new Exception ();
			}
		}

		private object ConstructList (Element elem) {
			object[] list = new object[elem.Children.Count];
			int i = 0;
			foreach (Element subelem in elem.Children) {
				if (subelem.Type == ElementType.List)
					list[i] = ConstructList (subelem);
				else
					list[i] = subelem.Val;
				i++;
			}

			return list;
		}
	
		public object Execute (InstructionList list) {
			foreach (Element elem in list) {
				switch (elem.Type) {
				case ElementType.Literal:
					return elem.Val;
				case ElementType.List:
					return ConstructList (elem);
				case ElementType.Statement:
					ExecuteStatement (elem);
					if (context.StopExecution)
						return context.OutputValue;
					break;
				case ElementType.Infix:
					ExecuteInfix (elem);
					break;
				case ElementType.Variable:
					return Funcs.Thing (context, (string) elem.Val);
				default:
					throw new Exception ();
				}
			}

			return null;
		}

		public static object Execute (LogoContext context, ICollection runlist, params object[] template_args) {
			string[] runlist_strs = new string[runlist.Count];
			int i = 0;
			foreach (object o in runlist) {
				if (o is ICollection)
					runlist_strs[i] = Funcs.ListToString ((ICollection) o, 0, true);
				else
					runlist_strs[i] = o.ToString ();
				i++;
			}
			
			Interpreter interp = new Interpreter ((Interpreter) context.CallingEngine, context);
			interp.template_args = template_args;

			Parser parser = new Parser (interp.stores, null);
			parser.AllowQuestionMark = true;
			InstructionList tree = parser.Parse (runlist_strs);
			
			return interp.Execute (tree);
		}
	}
}
