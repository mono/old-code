using System;
using MDB=Mono.Debugger;

namespace Mono.VisualStudio.Mdb {

public class ServerProcess : IProcess {
	DebuggerServer server;
	MDB.DebuggerConfiguration config;
	MDB.DebuggerSession session;
	MDB.Debugger debugger;
	
	public ServerProcess (DebuggerServer server, string exe, string args, string dir, string env)
	{
		this.server = server;
		
		MDB.DebuggerOptions options = MDB.DebuggerOptions.ParseCommandLine (new string [0]);
		
		config = new MDB.DebuggerConfiguration ();
		config.LoadConfiguration ();
		
		debugger = new MDB.Debugger (config);
		debugger.TargetEvent += OnTargetEvent;
		
		debugger.TargetExitedEvent += OnTargetExited;
		debugger.ThreadCreatedEvent += OnThreadCreated;
		debugger.ThreadExitedEvent += OnThreadExited;
		debugger.MainProcessCreatedEvent += OnMainProcessCreated;
		debugger.ProcessReachedMainEvent += OnProcessReachedMain;
		debugger.ProcessCreatedEvent += OnProcessCreated;
		debugger.ProcessExitedEvent += OnProcessExited;
		debugger.ProcessExecdEvent += OnProcessExeced;
				
		session = new MDB.DebuggerSession (config, options, "main", null);
	}

	public void Resume ()
	{
		debugger.Run (session);
	}
	
	void Abort ()
	{
		Console.WriteLine ("NOT IMPLEMENTED: " + Environment.StackTrace);
		Environment.Exit (1);
	}
	
	void OnThreadCreated (MDB.Debugger debugger, MDB.Thread thread)
	{
		Abort ();
	}

	void OnThreadExited (MDB.Debugger debugger, MDB.Thread thread)
	{
		Abort ();
	}

	void OnMainProcessCreated (MDB.Debugger debugger, MDB.Process process)
	{
		Abort ();
	}

	void OnProcessReachedMain (MDB.Debugger debugger, MDB.Process process)
	{
		Abort ();
	}

	void OnProcessCreated (MDB.Debugger debugger, MDB.Process process)
	{
		Abort ();
	}

	void OnProcessExited (MDB.Debugger debugger, MDB.Process process)
	{
		Abort ();
	}

	void OnProcessExeced (MDB.Debugger debugger, MDB.Process process)
	{
		Abort ();
	}

	void OnTargetExited (MDB.Debugger debugger, MDB.Thread thread)
	{
		Abort ();
	}
	       
	void OnTargetEvent (MDB.Thread thread, MDB.TargetEventArgs args)
	{
		switch (args.Type){
                case MDB.TargetEventType.TargetRunning:
                case MDB.TargetEventType.TargetStopped:
                case MDB.TargetEventType.TargetInterrupted:
                case MDB.TargetEventType.TargetHitBreakpoint:
                case MDB.TargetEventType.TargetSignaled:
                case MDB.TargetEventType.TargetExited:
                case MDB.TargetEventType.FrameChanged:
                case MDB.TargetEventType.Exception:
                case MDB.TargetEventType.UnhandledException:
			Abort ();
		}
	}
}
}