namespace Mono.Languages.Logo.Runtime {
	using System;
	using System.Collections;
	using System.Text;
	using Mono.Languages.Logo.Compiler;
	
	public class Funcs {
		// Word primitives
		public static char First (string str) {
			if (str.Length < 1)
				throw new ArgumentException (String.Empty, "str");
			return str[0];
		}

		public static string ButFirst (string str) {
			if (str.Length == 0)
				throw new ArgumentException (String.Empty, "str");
			else if (str.Length == 1)
				return String.Empty;
			else
				return str.Substring (1);
		}

		public static char Last (string str) {
			if (str.Length < 1)
				throw new ArgumentException (String.Empty, "str");
			return str[str.Length - 1];
		}

		public static string ButLast (string str) {
			if (str.Length == 0)
				throw new ArgumentException (String.Empty, "str");
			else if (str.Length == 1)
				return String.Empty;
			else
				return str.Substring (0, str.Length - 1);
		}

		public static char Item (int index, string str) {
			if (index >= str.Length)
				throw new ArgumentOutOfRangeException ("index");
			return str[index];
		}

		public static int Count (string str) {
			return str.Length;
		}

		// List primitives
		public static object First (ICollection list) {
			if (list.Count < 1)
				throw new ArgumentException (String.Empty, "list");
			IEnumerator iterator = list.GetEnumerator ();
			iterator.MoveNext ();
			return iterator.Current;
		}

		public static object First (IList list) {
			if (list.Count < 1)
				throw new ArgumentException (String.Empty, "list");
			return list[0];
		}

		public static object Firsts (ICollection list) {
			object[] fs = new object[list.Count];

			int i = 0;
			foreach (ICollection sublist in list) {
				fs[i] = First (sublist);
				i++;
			}

			return fs;
		}

		public static object ButFirst (ICollection list) {
			if (list.Count == 0)
				throw new ArgumentException (String.Empty, "list");
			else if (list.Count == 1)
				return new object[0];
			else {
				object[] as_array = new object[list.Count];
				object[] bf = new object[list.Count - 1];
				list.CopyTo (as_array, 0);
				Array.Copy (as_array, 1, bf, 0, list.Count - 1);
				return bf;
			}
		}

		public static object ButFirsts (ICollection list) {
			object[] bfs = new object[list.Count];
			
			int i = 0;
			foreach (ICollection sublist in list) {
				bfs[i] = ButFirst (sublist);
				i++;
			}

			return bfs;
		}
		
		public static object Last (ICollection list) {
			if (list.Count < 1)
				throw new ArgumentException (String.Empty, "list");
			int i = 0;
			int end = list.Count - 1;
			foreach (object val in list) {
				if (i == end)
					return val;
				i++;
			}

			return null;
		}

		public static object Last (IList list) {
			if (list.Count < 1)
				throw new ArgumentException (String.Empty, "list");
			return list[list.Count - 1];
		}

		public static object ButLast (ICollection list) {
			if (list.Count == 0)
				throw new ArgumentException (String.Empty, "list");
			else if (list.Count == 1)
				return new object[0];
			else {
				object[] as_array = new object[list.Count];
				object[] bf = new object[list.Count - 1];
				list.CopyTo (as_array, 0);
				Array.Copy (as_array, 0, bf, 1, list.Count - 1);
				return bf;
			}
		}

		public static object Item (int index, ICollection list) {
			if (index >= list.Count)
				throw new ArgumentOutOfRangeException ("index");
			else {
				int i = 0;
				foreach (object val in list) {
					if (i == index)
						return val;
					i++;	
				}
			}

			return null;
		}

		public static object Item (int index, IList list) {
			if (index >= list.Count)
				throw new ArgumentOutOfRangeException ("index");
			else
				return list[index];
		}

		public static int Count (ICollection list) {
			return list.Count;
		}

		// Streams and I/O
		private static void ListToString (StringBuilder sb, ICollection list, int outer_brackets, bool spaces) {
			if (outer_brackets == 0)
				sb.Append ('[');
			
			int i = 0;
			int end = list.Count - 1;
			foreach (object val in list) {
				if (val is ICollection)
					ListToString (sb, (ICollection) val, (outer_brackets == 0) ? 0 : (outer_brackets - 1), true);
				else
					sb.Append (val.ToString ());
				if (spaces && i < end)
					sb.Append (" ");
				i++;
			}

			if (outer_brackets == 0)
				sb.Append (']');
		}

		internal static string ListToString (ICollection list, int outer_brackets, bool spaces) {
			StringBuilder sb = new StringBuilder ();
			ListToString (sb, list, outer_brackets, spaces);
			return sb.ToString ();
		}

		// LAMESPEC: ObjectLOGO seems to differ slightly from UCB Logo
		// in allowed input lengths. As UCB is more permissive, these
		// functions follow it for now

		public static void Print (params object[] vals) {
			Console.WriteLine (ListToString (vals, 2, true));
		}

		public static void Type (params object[] vals) {
			Console.Write (ListToString (vals, 2, false));
		}

		public static void Show (params object[] vals) {
			Console.WriteLine (ListToString (vals, 1, true));
		}

		// Math
		
		[DefaultArgumentCount (2)]
		public static double Sum (params double[] vals) {
			double sum = 0;
			foreach (double val in vals)
				sum += val;

			return sum;
		}

		public static double Minus (double val) {
			return -val;
		}

		// Variables
		[PassContext]
		public static void Make (LogoContext context, string variable, object value) {
			if (!context.Dict.Contains (variable)) {
				context = context.RootContext;
			}

			context.Dict[variable] = value;
		}

		[PassContext]
		public static void LocalMake (LogoContext context, string variable, object value) {
			context.Dict[variable] = value;
		}

		[PassContext]
		public static void Local (LogoContext context, string variable) {
			context.Dict[variable] = LogoContext.NullVariable;
		}

		[PassContext]
		public static void Local (LogoContext context, ICollection variables) {
			foreach (string variable in variables) {
				context.Dict[variable] = LogoContext.NullVariable;
			}
		}

		[PassContext]
		public static object Thing (LogoContext context, string variable) {
			return context.Dict[variable];
		}

		// Workspace
		public static void Bye () {
			System.Environment.Exit (0);
		}

		// Flow control
		[PassContext]
		public static void Output (LogoContext context, object val) {
			context.Output (val);
		}

		[PassContext]
		public static object Run (LogoContext context, ICollection runlist) {
			return Interpreter.Execute (context, runlist);
		}
	}
}

