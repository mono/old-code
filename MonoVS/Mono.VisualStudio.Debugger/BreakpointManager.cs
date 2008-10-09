using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Debugger.Interop;
using Mono.VisualStudio.Mdb;

namespace Mono.VisualStudio.Debugger
{
	// This class manages breakpoints for the engine. 
	public class BreakpointManager
	{
		public Process Process
		{
			get;
			private set;
		}

		internal IBreakpointManager ServerManager
		{
			get;
			private set;
		}

		List<PendingBreakpoint> pending_breakpoints;
		Dictionary<IServerBreakpoint, BoundBreakpoint> bound_breakpoints;
		Dictionary<IServerBreakpoint, PendingBreakpoint> pending_requests;

		public BreakpointManager (Process process, IDebuggerServer server)
		{
			this.Process = process;

			ServerManager = server.CreateBreakpointManager (new EventSink (this));

			pending_breakpoints = new List<PendingBreakpoint> ();
			bound_breakpoints = new Dictionary<IServerBreakpoint, BoundBreakpoint> ();
			pending_requests = new Dictionary<IServerBreakpoint, PendingBreakpoint> ();
		}

		internal PendingBreakpoint CreatePendingBreakpoint (IDebugBreakpointRequest2 request)
		{
			PendingBreakpoint pending = new PendingBreakpoint (this, request);
			pending_breakpoints.Add (pending);
			return pending;
		}

		internal bool BindBreakpoint (PendingBreakpoint pending, ISourceLocation location)
		{
			IServerBreakpoint handle = ServerManager.InsertBreakpoint (location);
			if (handle == null)
				return false;

			pending_requests.Add (handle, pending);
			return true;
		}

		public BoundBreakpoint GetBoundBreakpoint (IServerBreakpoint handle)
		{
			return bound_breakpoints[handle];
		}

		#region Event Sink

		protected void OnBreakpointBound (IServerBreakpoint handle)
		{
			PendingBreakpoint pending = pending_requests [handle];
			BoundBreakpoint bound = new BoundBreakpoint (
				Process.Engine, pending, new BreakpointResolution (Process), handle);
			pending_requests.Remove (handle);
			bound_breakpoints.Add (handle, bound);
			Process.Engine.EngineCallback.OnBreakpointBound (bound);
		}

		protected void OnBreakpointError (IServerBreakpoint handle, string message)
		{
			PendingBreakpoint pending = pending_requests [handle];
			ErrorBreakpointResolution error = new ErrorBreakpointResolution (Process, message);
			ErrorBreakpoint breakpoint = new ErrorBreakpoint (pending, error);

			pending.ErrorBreakpoint = breakpoint;
			Process.Engine.EngineCallback.OnBreakpointError (breakpoint);
		}

		protected class EventSink : MarshalByRefObject, IBreakpointManagerEventSink
		{
			public BreakpointManager Manager
			{
				get;
				private set;
			}

			public EventSink (BreakpointManager manager)
			{
				this.Manager = manager;
			}

			#region IBreakpointManagerEventSink Members

			void IBreakpointManagerEventSink.OnBreakpointBound (IServerBreakpoint breakpoint)
			{
				Manager.OnBreakpointBound (breakpoint);
			}

			void IBreakpointManagerEventSink.OnBreakpointError (IServerBreakpoint breakpoint, string message)
			{
				Manager.OnBreakpointError (breakpoint, message);
			}

			#endregion
		}

		#endregion
	}
}
