using System;
using Xnb;

using Mono.GetOptions;

public class TestServer
{
	public class TestXServerOptions : XServerOptions
	{
		public TestXServerOptions (string[] args) : base (args) {}
	}

	public static void Main (string[] args)
	{
		TestXServerOptions opts = new TestXServerOptions (args);
		string[] dpy = opts.RemainingArguments;

		XServer xs = new XServer ();
		xs.InitUnix ();
	}
}
