using System;
using System.Collections;
using System.IO;
using Mono.Languages.Logo.Compiler;

class X {
	public static void Main (string[] args)
	{
		Tokenizer tokenizer = new Tokenizer ();
		FileStream stream = new FileStream (args[0], FileMode.Open);
		//ICollection tokens = tokenizer.Parse ("PRINT 1 + 2 -3\nPRINT \"|HELLO  WORLD| ;a |comme|nt\n_PRINT \"|HELLO || BARS|\nPRINT \"-QUOTES");
		ICollection tokens = tokenizer.Parse (new StreamReader (stream));
		foreach (Token token in tokens) {
			Console.WriteLine ("{0}: {1}", token.Type, token.Val);
		}
	}
}

