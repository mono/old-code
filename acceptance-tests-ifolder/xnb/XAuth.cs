using System;
using System.Net;
using System.Net.Sockets;

using Mono.Unix;

using Mono.GetOptions;
using Xnb.Auth;

public class XAuthOptions : Options
{
	public XAuthOptions (string[] args) : base (args) {}
	[Option("Authority file name", 'f', "authfile")]
		public string AuthFile;

	[Option("Operate quietly", 'q', "quiet")]
		public bool Quiet;

	[Option("Operate verbosely", 'v', "verbose")]
		public bool Verbose;

	[Option("Ignore authority file locks", 'i', "ignorelocks")]
		public bool IgnoreLocks;

	[Option("Break authority file locks before proceeding", 'b', "breaklocks")]
		public bool BreakLocks;

	[Option("Do not resolve hostnames", 'n', "noresolve")]
		public bool NoResolve;

	protected override void InitializeOtherDefaults ()
	{
		ParsingMode = OptionsParsingMode.GNU_DoubleDash;
		BreakSingleDashManyLettersIntoManyOptions = true;
	}
}

public class Driver
{
	public static void Main (string[] args)
	{
		XAuthOptions opts = new XAuthOptions (args);

		Xau xau = new Xau ();

		foreach (Xauth xa in xau) {
			//Console.Write (xa.Address);
			//Console.Write ((AddressFamily)xa.Family);
			Console.Write (xa.Address + "/" + xa.Family.ToString ().ToLower ());
			Console.Write (":" + xa.Number);
			Console.Write ("  ");
			Console.Write (xa.Name);
			Console.Write ("  ");
			//SocketAddress sa = new SocketAddress ((AddressFamily)xa.Family, xa.Address.Length);
			Console.WriteLine (Xau.ToHex (xa.Data));

			//EndPoint ep = UnixEndPoint.Create (xa.Address);
			//Console.WriteLine (ep);
		}
	}
}

[assembly: System.Reflection.AssemblyTitle("nxauth")]
[assembly: System.Reflection.AssemblyCopyright("Copyright (C) 2006 Alp Toker")]
[assembly: System.Reflection.AssemblyDescription("X authority file utility")]
[assembly: System.Reflection.AssemblyVersion ("0.0.0.0")]

[assembly: Mono.About("X authority file utility")]
[assembly: Mono.Author("Alp Toker")]
[assembly: Mono.UsageComplement("[ -f authfile ] [ -vqibn ] [ command arg ... ]")]
[assembly: Mono.AdditionalInfo("This utility is used to edit, merge and display authorization information \nused in connecting to the X server.")]
[assembly: Mono.ReportBugsTo("alp@ndesk.org")]

