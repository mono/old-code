using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Net;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.InteropServices;
using System.Threading;
using System.Runtime.Remoting.Lifetime;

using MDB=Mono.Debugger;

namespace Mono.VisualStudio.Mdb {

public static class Manager {
	static ManualResetEvent exit_event = new ManualResetEvent (false);

	static void Main (string [] args)
	{
		if (args.Length == 0){
			Console.Error.WriteLine ("Missing Remoting ObjRef (as base64)");
			return;
		}

		Hashtable props = new Hashtable();
		props["port"] = 0;
		props["name"] = "MdbServer";
		ChannelServices.RegisterChannel (new TcpChannel (props, null, null), false);

		DoStartServer (args [0]);

		Console.WriteLine ("SERVER!");

		exit_event.WaitOne ();

		Console.WriteLine ("SERVER DONE!");
	}

	public static void Exit ()
	{
		exit_event.Set ();
	}

	static bool initialized;

	public static void StartServer (string oref)
	{
		if (!initialized) {
			Hashtable props = new Hashtable();
			props["port"] = 0;
			props["name"] = "MdbServer";
			ChannelServices.RegisterChannel (new TcpChannel (props, null, null), false);
			initialized = true;
		}

		DoStartServer (oref);
	}

	static void DoStartServer (string oref)
	{
		Console.WriteLine ("START SERVER: {0} {1}", oref.Length, oref);

		MDB.Report.Initialize ();

		try {		
			// Create the object ref
			byte [] data = Convert.FromBase64String (oref);
			MemoryStream ms = new MemoryStream (data);
			BinaryFormatter bf = new BinaryFormatter();
			IMdbController mdbc = (IMdbController) bf.Deserialize(ms);
			DebuggerManager manager = new DebuggerManager(mdbc);

			Console.WriteLine ("START SERVER #1");
			Console.WriteLine ("START SERVER #1a: {0}", mdbc);

			mdbc.RegisterDebugger (manager);

			Console.WriteLine ("START SERVER #2");
		} catch (Exception ex) {
			Console.WriteLine ("SERVER EX: {0}", ex);
			throw;
		}
	}
}

public class DebuggerManager : MarshalByRefObject, IMdbManager, ISponsor {
	
	IMdbController controller;
	
	public DebuggerManager (IMdbController mdbc)
	{
		controller = mdbc;
		var mbr = (MarshalByRefObject) controller;
		ILease lease = mbr.GetLifetimeService() as ILease;
		
		Console.WriteLine (typeof (IMdbManager).AssemblyQualifiedName);
		Console.WriteLine (typeof (IMdbManager).FullName);
		Console.WriteLine (typeof (IMdbManager).GUID);
		lease.Register(this);
	}

	static Dictionary<string,string> mappings = new Dictionary<string,string> ();

	void IMdbManager.AddDirectoryMapping (string source, string target)
	{
		mappings [source] = target;
	}

	public string Remap (string a)
	{
		foreach (var kp in mappings){
			Console.WriteLine ("--- {0} and {1}", kp.Key, kp.Value);
			a = a.Replace (kp.Key, kp.Value);
			a = a.Replace ("\\", "/");
		}
		return a;
	}
	
	IDebuggerServer IMdbManager.Launch (string exe, string args, string dir, string env,
					    IClientEventSink sink)
	{
		Console.WriteLine ("exe: {0}", exe);		
		exe = Remap (exe);
		Console.WriteLine ("   map: {0}", exe);
		
		Console.WriteLine ("dir: {0}", dir);
		dir = Remap (dir);
		Console.WriteLine ("   map: {0}", dir);
		
		Console.WriteLine ("args: {0}", args);
		Console.WriteLine ("env: {0}", env);
		
		return new DebuggerServer (this, exe, args, dir, env, sink);
	}
	
	public override object InitializeLifetimeService ()
	{
		return null;
	}
	
	TimeSpan ISponsor.Renewal (ILease lease)
	{
		return TimeSpan.FromSeconds (15);
	}
	
	public IMdbController Controller {
		get {
			return controller;
		}
	}
}
}
