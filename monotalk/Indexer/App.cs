using System;
using Monotalk.Indexer;

public class App
{
	public static int Main (string[] args)
	{
		Indexer indexer = new Indexer ();

		foreach (string filename in args) {
			Console.WriteLine ("\nparse: " + filename);
			indexer.Parse (filename);
		}

		return 0;
	}
}
