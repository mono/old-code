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
		
		POP3Message[] msgs = mails.GetMessageRange(messcount-21,messcount-1,false);
		
		foreach (POP3Message msg in msgs)
		{
			Console.WriteLine("{0} \n{1} \n{2} \n\n", msg.From.Trim(), msg.Subject.Trim(), msg.Date.Trim());
		}
		
		mails.Close();
		
	}
}
}

