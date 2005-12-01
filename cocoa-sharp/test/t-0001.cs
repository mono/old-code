using System;
using System.Diagnostics;
using Apple.Foundation;

class T{
	static int Main(string [] args) {
		NSAutoreleasePool p = new NSAutoreleasePool ();

		string str0 = "Round trip a string.";
		NSString str1 = new NSString(str0);
		string str2 = str1.ToString();
		Console.WriteLine ("O: {0}", str0);
		Debug.Assert(str1 != null);
		Console.WriteLine ("OC: {0}", str1);
		Console.WriteLine ("RT: {0}", str2);
		if(str2 != str0)
			return 1;
		return 0;
	}
}
 
