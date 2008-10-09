using System;
using System.Collections.Generic;
using MDB=Mono.Debugger;

namespace Mono.VisualStudio.Mdb
{
public class DebuggerServer : MarshalByRefObject, IDebuggerServer
{
	public DebuggerManager DebuggerManager {
		get; private set;
	}

	public MDB.DebuggerSession Session {
		get; private set;
	}

	public BreakpointManager BreakpointManager {
		get; private set;
	}

	internal IClientEventSink Sink {
		get; private set;
	}

	protected MDB.DebuggerConfiguration config;
	protected MDB.Debugger debugger;

	protected ServerProcess main_process;
	Dictionary<MDB.Process,ServerProcess> processes = new Dictionary<MDB.Process,ServerProcess> ();

	public DebuggerServer (DebuggerManager manager, string exe, string args,
			       string dir, string env, IClientEventSink sink)
	{
		this.DebuggerManager = manager;
		this.Sink = sink;

		MDB.DebuggerOptions options = MDB.DebuggerOptions.ParseCommandLine (new string [1] { exe });
		options.StopInMain = false;

		config = new MDB.DebuggerConfiguration ();
		config.LoadConfiguration ();
		config.OpaqueFileNames = true;

		debugger = new MDB.Debugger (config);
		debugger.ProcessCreatedEvent += OnProcessCreated;
		debugger.ProcessReachedMainEvent += OnReachedMainEvent;
		debugger.ThreadCreatedEvent += OnThreadCreated;

		Session = new MDB.DebuggerSession (config, options, "main", null);
	}

	public IServerProcess Start (IProcessEventSink sink)
	{
		try {
			MDB.Process process = debugger.Run (Session);
			if (process == null) {
				Abort ();
				return null;
			}

			main_process = new ServerProcess (this, process, sink);
			processes.Add (process, main_process);
			return main_process;
		} catch (Exception e) {
			Console.WriteLine (e.ToString ());
			Abort ();
			return null;
		}
	}

	public IBreakpointManager CreateBreakpointManager (IBreakpointManagerEventSink sink)
	{
		if (BreakpointManager != null)
			throw new InvalidOperationException ();

		BreakpointManager = new BreakpointManager (this, sink);
		return BreakpointManager;
	}

	public void Kill ()
	{
		debugger.Kill ();
		debugger.Dispose ();
		debugger = null;
		Manager.Exit ();
	}

	public void Abort ()
	{
		Console.WriteLine ("FUCK: " + Environment.StackTrace);
		Environment.Exit (1);
	}

	void OnProcessCreated (MDB.Debugger debugger, MDB.Process process)
	{
		Console.WriteLine ("ON PROCESS CREATED: {0}", process);
	}

	void OnThreadCreated (MDB.Debugger debugger, MDB.Thread thread)
	{
		ServerProcess process = processes [thread.Process];
		Console.WriteLine ("ON THREAD CREATED: {0} {1}", process, thread);
		process.OnThreadCreated (thread);
	}

	void OnReachedMainEvent (MDB.Debugger debugger, MDB.Process process)
	{
		Console.WriteLine ("ON REACHED MAIN: {0} {1}", process, process.MainThread);

		if (process != main_process.Process) {
			Abort ();
			throw new InvalidOperationException ();
		}

		main_process.OnDebuggerReady (process.MainThread);
	}
}
}
