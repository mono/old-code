using System;
using System.Collections;
using System.IO;
using Mono.Languages.Logo.Runtime;
using Mono.Languages.Logo.Compiler;

class Funcs {
	public static int sum (int a, int b) {
		return a + b;
	}
	
	public static double sum (double a, double b) {
		return a + b;
	}

	public static double sum (double a, double b, double c) {
		return a + b + c;
	}

	public static void type (params object[] args) {
		foreach (object o in args) {
			if (o is ICollection) {
				ICollection o_list = (ICollection) o; 
				int i = o_list.Count - 1;
				Console.Write ("[");
				foreach (object subo in o_list) {
					type (subo);
					if (i > 0)
						Console.Write (" ");
					i--;
				}
				Console.Write ("]");
			} else {
				Console.Write (o);
			}
		}
	}

		
	public static void print (params object[] args) {
		foreach (object o in args) {
			type (o);
			Console.Write (" ");
		}
		Console.WriteLine ();
	}

	public static object first (object[] list) {
		return list[0];
	}

	public static object[] firsts (object[] list) {
		object[] ret = new object[list.Length];
		int i = 0;
		foreach (object[] o in list) {
			ret[i] = first (o);
			i++;
		}
		return ret;
	}
	public static object[] butfirst (object[] list) {
		object[] ret = new object[list.Length - 1];
		Array.Copy (list, 1, ret, 0, list.Length - 1);
		return ret;
	}
	
	public static object[] butfirsts (object[] list) {
		object[] ret = new object[list.Length];
		int i = 0;
		foreach (object[] o in list) {
			ret[i] = butfirst (o);
			i++;
		}
		return ret;
	}
}

class X {
	public static int Main (string[] args) {
		if (args.Length != 1) {
			Console.WriteLine ("Usage: test-interp.exe filename");
			return 1;
		}

		IMessageStoreCollection stores = new IMessageStoreCollection ();
		stores.Add (new CTSMessageTarget (typeof (Funcs)));

		Parser parser = new Parser (stores, new LogoMessageTarget ());
		FileStream stream = new FileStream (args[0], FileMode.Open);
		InstructionList tree = parser.Parse (new StreamReader (stream));
		Interpreter interp = new Interpreter (stores);
		interp.Execute (tree);
		return 0;
	}
}
