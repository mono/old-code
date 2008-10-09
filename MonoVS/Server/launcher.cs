//
// Accepts incoming connections from remote systems, and launches
// the debugger
//
using System;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using Mono.VisualStudio.Mdb;
using NDesk.Options;

class Launcher {
	static bool embedded = true;
	static Process process = null;

	static void Main (string [] args)
	{
		OptionSet options = new OptionSet (){
			{"s|standalone", s => embedded = false },
		};
		options.Parse (args);
				
		HttpListener l = new HttpListener ();

		l.Prefixes.Add ("http://*:7777/mono-debugger/");
		l.Start ();

		bool ok;
		do {
			Console.WriteLine ("Launcher: Waiting for request (embedded={0})", embedded);
			ok = HandleRequest (l.GetContext ());
			Console.WriteLine ("Launcher: Done handling request{0}",
					   ok ? " (continuing)" : " (exiting)");
		} while (ok);

		l.Stop ();
		Environment.Exit (0);
	}

	static bool HandleRequest (HttpListenerContext lc)
	{
		Console.WriteLine ("Launcher: Handling request");
		
		Uri u = lc.Request.Url;
		Console.WriteLine ("HANDLE REQUEST: {0}", u.AbsolutePath);
		switch (u.AbsolutePath){
		case "/mono-debugger/start":
			StartServer (lc);
			return true;

		case "/mono-debugger/kill":
			Report (lc, "OK", true);
			Kill (lc);
			return true;

		case "/mono-debugger/shutdown":
			Console.WriteLine ("SHUTDOWN!");
			Report (lc, "OK", true);
			return false;

		default:
			Report (lc, "", false);
			return true;
		}
	}

	static void Kill (HttpListenerContext lc)
	{
		int pid;
		if ((lc.Request.QueryString ["pid"] != null) &&
		    Int32.TryParse (lc.Request.QueryString ["pid"], out pid)) {
			Console.WriteLine ("KILL: {0}", pid);
		}

		Console.WriteLine ("KILL!");

		if ((process == null) || process.HasExited)
			return;

		try {
			process.Kill ();
		} catch {
		} finally {
			process = null;
		}
	}

	static void StartServer (HttpListenerContext lc)
	{
		try {
			string reference;
			using (TextReader reader = new StreamReader (lc.Request.InputStream)) {
				reference = reader.ReadToEnd ();
			}

			Console.WriteLine ("START SERVER: {0} {1}", reference.Length, reference);
			if (embedded) {
				Mono.VisualStudio.Mdb.Manager.StartServer (reference);
				Report (lc, "OK", true);
			} else {
				process = Process.Start (
					"mono", "--debug debugserver.exe '" + reference + "'");
				Report (lc, String.Format ("SERVER: {0}", process.Id), true);
			}
		} catch (Exception ex) {
			Report (lc, String.Format ("ERROR: {0}", ex), true);
		}
	}
	
	static void Report (HttpListenerContext lc, string msg, bool ok)
	{
		lc.Response.StatusCode = (int) (ok ? HttpStatusCode.OK : HttpStatusCode.BadRequest);

		using (StreamWriter sw = new StreamWriter (lc.Response.OutputStream)) {
			sw.Write (msg);
		}
	}
}
