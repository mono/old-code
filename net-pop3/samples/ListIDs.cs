using System;
using System.Net;
using Mono.Net.POP3;

namespace Mono.Net.POP3.Samples
{
public class Test2
{
	public static void Main(string[] args)
	{
		POP3Connection mails = new POP3Connection(
					args[0], args[1], args[2]);
		
		mails.Open();
		int messcount =  mails.MessageCount();
		Console.WriteLine("MESSAGES COUNT: {0}",messcount);
		
		foreach (short s in mails.List())
			Console.WriteLine(s);
		
	
		mails.Close();
		
	}
}
}

