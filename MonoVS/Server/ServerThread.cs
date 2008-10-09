using System;
using System.Collections.Generic;
using MDB=Mono.Debugger;

namespace Mono.VisualStudio.Mdb
{
public class ServerThread : MarshalByRefObject, IServerThread
{
	public ServerProcess Process {
		get; private set;
	}

	public MDB.Thread Thread {
		get; private set;
	}

	public ServerThread (ServerProcess process, MDB.Thread thread)
	{
		this.Process = process;
		this.Thread = thread;
	}

	public int ID {
		get { return Thread.ID; }
	}

	IServerStackFrame IServerThread.GetFrame ()
	{
		try {
			return GetFrame ();
		} catch (Exception ex) {
			Console.WriteLine ("GET FRAME EX: {0}", ex);
			return null;
		}
	}

	IServerBacktrace IServerThread.GetBacktrace ()
	{
		try {
			return GetBacktrace ();
		} catch (Exception ex) {
			Console.WriteLine ("GET BACKTRACE EX: {0}", ex);
			return null;
		}
	}

	void IServerThread.Continue ()
	{
		Process.Manager.Continue (Thread);
	}

	void IServerThread.StepInto ()
	{
		Process.Manager.StepInto (Thread);
	}

	void IServerThread.StepOut ()
	{
		Process.Manager.StepOut (Thread);
	}

	void IServerThread.StepOver ()
	{
		Process.Manager.StepOver (Thread);
	}

	public ServerStackFrame GetFrame ()
	{
		return new ServerStackFrame (this, Thread.CurrentFrame);
	}

	public ServerBacktrace GetBacktrace ()
	{
		MDB.Backtrace bt = Thread.GetBacktrace ();
		return new ServerBacktrace (this, bt);
	}
}
}
