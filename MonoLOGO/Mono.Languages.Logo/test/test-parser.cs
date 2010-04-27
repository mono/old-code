using System;
using System.IO;
using Mono.Languages.Logo.Runtime;
using Mono.Languages.Logo.Compiler;

class Funcs {
	public static int sum (int a, int b) {
		return a + b;
	}
	
	public static int sum (int a, int b, int c) {
		return a + b + c;
	}

	public static void print (params object[] o) {
		Console.WriteLine (o);
	}

	public static void first (object o) {
		throw new NotImplementedException ();
	}

	public static void firsts (object o) {
		throw new NotImplementedException ();
	}
	public static void butfirst (object o) {
		throw new NotImplementedException ();
	}
	public static void butfirsts (object o) {
		throw new NotImplementedException ();
	}
}

class X {
	public static void PrintTree (InstructionList tree, int indent) {
		foreach (Element elem in tree) {
			Console.WriteLine ("{0}{1}: {2}",
									 new String ('\t', indent),
									 elem.Type,
									 elem.Val);
			if (elem.Children != null) {
				PrintTree (elem.Children, indent + 1);
			}
		}
	}
	
	public static int Main (string[] args) {
		if (args.Length != 1) {
			Console.WriteLine ("Usage: test-parser.exe filename");
			return 1;
		}

		IMessageStoreCollection stores = new IMessageStoreCollection ();
		stores.Add (new CTSMessageTarget (typeof (Funcs)));
		LogoMessageTarget funcs = new LogoMessageTarget ();
		stores.Add (funcs);

		Parser parser = new Parser (stores, funcs);
		FileStream stream = new FileStream (args[0], FileMode.Open);
		InstructionList tree = parser.Parse (new StreamReader (stream));
		PrintTree (tree, 0);
		return 0;
	}
}
