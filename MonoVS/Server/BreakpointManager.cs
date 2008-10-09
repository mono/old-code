using System;
using System.Collections.Generic;
using MDB=Mono.Debugger;

namespace Mono.VisualStudio.Mdb
{
public class BreakpointManager : MarshalByRefObject, IBreakpointManager
{
	public DebuggerServer DebuggerServer {
		get; private set;
	}

	public IBreakpointManagerEventSink Sink {
		get; private set;
	}

	Dictionary<int,ServerBreakpoint> events = new Dictionary<int,ServerBreakpoint> ();

	public BreakpointManager (DebuggerServer server, IBreakpointManagerEventSink sink)
	{
		this.DebuggerServer = server;
		this.Sink = sink;
	}

	public IServerBreakpoint InsertBreakpoint (ISourceLocation location)
	{
		try {
			Console.WriteLine ("INSERT BREAKPOINT!");
			SourceBreakpoint breakpoint = new SourceBreakpoint (this, location);
			DebuggerServer.Session.AddEvent (breakpoint);
			events.Add (breakpoint.Index, breakpoint.Handle);
			Console.WriteLine ("INSERT BREAKPOINT #1: {0}", breakpoint.Index);
			return breakpoint.Handle;
		} catch (Exception ex) {
			Console.WriteLine ("INSERT BREAKPOINT EX: {0}", ex);
			return null;
		}
	}

	public ServerBreakpoint GetBreakpoint (int index)
	{
		return events [index];
	}

	protected class SourceBreakpoint : MDB.SourceBreakpoint
	{
		public BreakpointManager Manager {
			get; private set;
		}

		public ServerBreakpoint Handle {
			get; private set;
		}

		public SourceBreakpoint (BreakpointManager manager, ISourceLocation location)
			: base (manager.DebuggerServer.Session, MDB.ThreadGroup.Global,
				new MDB.SourceLocation (location.FileName, location.Line + 1))
		{
			this.Manager = manager;
			this.Handle = new ServerBreakpoint (this);

			Console.WriteLine ("NEW BREAKPOINT: {0} {1}", location.FileName, location.Line + 1);
		}

		protected override void OnBreakpointBound ()
		{
			Console.WriteLine ("BOUND!");
			Manager.Sink.OnBreakpointBound (Handle);
		}

		protected override void OnBreakpointError (string message)
		{
			Console.WriteLine ("BREAKPOINT ERROR: {0}", message);
			Manager.Sink.OnBreakpointError (Handle, message);
		}
	}
}
}
