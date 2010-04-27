// created on 3/9/2002 at 9:57 PM
// Test program for PInvoke and other misc assembly bits
// For LCC writing and stuff.

using System;
using System.Runtime.InteropServices;

class TestApp
{
	[DllImport("user32.dll")]
	static extern int MessageBoxA(int hWnd,
	                              string msg,
	                              string caption,
	                              int type);
	
	public static void Main()
	{
		double x;
		float y;
		int i;
		Random myRand = new Random();
		
		i = myRand.Next();
		y = 4.3232F;
		x = 54.423;
		
		y = i * y;
		x = i * x;
		
		Console.WriteLine("y: {0:G}", y);
		Console.WriteLine("x: {0:G}", x);
		Console.WriteLine("Hi");
		
		MessageBoxA(0,
		            "Hello World!",
		            "I was called from a C# app!",
		            0);
	}
	
}
