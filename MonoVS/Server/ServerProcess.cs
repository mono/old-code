using System;
using System.Threading;
using System.Collections.Generic;
using MDB=Mono.Debugger;

namespace Mono.VisualStudio.Mdb {

public class ServerProcess : MarshalByRefObject, IServerProcess
{
	Dictionary<MDB.Thread,ServerThread> threads = new Dictionary<MDB.Thread,ServerThread> ();
	ManualResetEvent start_event = new ManualResetEvent (false);
	bool initialized = false;

	public DebuggerServer Server {
		get; private set;
	}

	public IProcessEventSink Sink {
		get; private set;
	}

	public MDB.Process Process {
		get; private set;
	}

	public MDB.GUIManager Manager {
		get; private set;
	}

	public ServerThread MainThread {
		get; private set;
	}

	public ServerProcess (DebuggerServer server, MDB.Process process, IProcessEventSink sink)
	{
		this.Server = server;
		this.Process = process;
		this.Sink = sink;

		Manager = process.StartGUIManager ();
		Manager.TargetEvent += OnTargetEvent;
		Manager.ProcessExitedEvent += OnProcessExitedEvent;

		process.TargetOutputEvent += OnTargetOutput;
	}

	void IServerProcess.ResumeProcess ()
	{
		start_event.Set ();
	}

	void IServerProcess.Stop ()
	{
		Manager.Stop (MainThread.Thread);
	}

	internal ServerThread OnThreadCreated (MDB.Thread t)
	{
		ServerThread thread = new ServerThread (this, t);
		threads.Add (t, thread);
		if (initialized)
			Sink.OnThreadCreated (thread);
		return thread;
	}

	internal void OnDebuggerReady (MDB.Thread t)
	{
		MainThread = OnThreadCreated (t);
		Sink.OnProgramStarted (MainThread);
		start_event.WaitOne ();
		initialized = true;
	}

	void OnTargetOutput (bool is_stderr, string output)
	{
		Sink.OnTargetOutput (output);
	}

	internal void OnProcessExitedEvent (MDB.Debugger debugger, MDB.Process process)
	{
		if (Process == process)
			Sink.OnProcessExited ();
	}

	internal void OnTargetEvent (MDB.Thread t, MDB.TargetEventArgs args)
	{
		ServerThread thread = threads [t];

		switch (args.Type){
                case MDB.TargetEventType.TargetHitBreakpoint:
			int idx = (int) args.Data;
			ServerBreakpoint bpt = Server.BreakpointManager.GetBreakpoint (idx);
			Sink.OnBreakpointHit (thread, bpt);
			break;
                case MDB.TargetEventType.TargetStopped:
			Sink.OnStepComplete (thread);
			break;

                case MDB.TargetEventType.TargetExited:
			Sink.OnThreadExited (thread);
			break;

                case MDB.TargetEventType.TargetRunning:
                case MDB.TargetEventType.TargetInterrupted:
                case MDB.TargetEventType.TargetSignaled:
                case MDB.TargetEventType.FrameChanged:
                case MDB.TargetEventType.Exception:
                case MDB.TargetEventType.UnhandledException:
			break;
		}
	}
}
}
