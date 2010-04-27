using System;
using System.Net;
using Mono.Net.POP3;
using System.Collections;
using System.Collections.Specialized;

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
		foreach ( DictionaryEntry de in msg.Headers )
         		Console.WriteLine( "\n***********\n{0}\n-----------\n{1}", de.Key, de.Value );
		
		mails.Close();
		
	}
}
}

