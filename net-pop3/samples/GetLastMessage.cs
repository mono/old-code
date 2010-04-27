using System;
using System.Net;
using Mono.Net.POP3;

namespace Mono.Net.POP3.Samples
{
public class Test1
{
	public static void Main(string[] args)
	{
		POP3Connection mails = new POP3Connection(
						args[0], args[1], args[2]);
		mails.Open();
		short messcount =  mails.MessageCount();
		Console.WriteLine("MESSAGES COUNT: {0}",messcount);
		
		short[] q = mails.List();
		POP3Message msg = mails.Retr(q[q.GetUpperBound(0)]);
		
		Console.WriteLine("MESSAGE TO:\t {0}", msg.To);
		Console.WriteLine("MESSAGE FROM:\t {0}", msg.From);
		Console.WriteLine("MESSAGE SUBJECT: {0}", msg.Subject);
		Console.WriteLine("MESSAGE DATE:\t {0}", msg.Date);
		Console.WriteLine("--- MESSAGE ---\n {0}", msg.Message);
		
		mails.Close();
		
	}
}
}

