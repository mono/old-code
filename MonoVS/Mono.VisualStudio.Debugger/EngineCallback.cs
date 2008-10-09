using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.Debugger.Interop;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace Mono.VisualStudio.Debugger
{
	class EngineCallback
	{
		public IDebugEventCallback2 Callback { get; private set; }
		public Engine Engine { get; private set; }

		public EngineCallback (Engine engine, IDebugEventCallback2 callback)
		{
			Callback = callback;
			Engine = engine;
		}

		public void Send (IDebugEvent2 e, string iid, IDebugProgram2 program, IDebugThread2 thread)
		{
			uint attrs;
			Guid guid = new Guid (iid);

			Utils.RequireOk (e.GetAttributes (out attrs));
			Utils.Message ("SEND EVENT: {0} {1} {2} {4} - {5:x}",
				this, e, iid, program, thread, attrs);

			Utils.RequireOk (Callback.Event (Engine, null, program, thread, e, ref guid, attrs));
		}

		public void Send (IDebugEvent2 e, string iid, IDebugThread2 thread)
		{
			Send (e, iid, Engine.Process, thread);
		}

		public void OnError (string message, params object[] args)
		{
			string text = String.Format (message, args);
			Utils.Message ("ERROR: {0}", text);
			AD7ErrorEvent e = new AD7ErrorEvent (text);
			Send (e, AD7ErrorEvent.IID, null);
		}

		public void OnThreadStart (Thread thread)
		{
			AD7ThreadCreateEvent e = new AD7ThreadCreateEvent ();
			Utils.Message ("THREAD CREATE: {0}", thread.ID);
			Send (e, AD7ThreadCreateEvent.IID, thread);
		}

        public void OnThreadDestroy (Thread thread)
        {
            AD7ThreadDestroyEvent e = new AD7ThreadDestroyEvent (0);
            Utils.Message ("THREAD DESTROY: {0}", thread.ID);
            Send (e, AD7ThreadDestroyEvent.IID, thread);
        }

		public void OnLoadComplete (Thread thread)
		{
			AD7EntryPointEvent e = new AD7EntryPointEvent ();
			Send (e, AD7EntryPointEvent.IID, thread);
		}

		public void OnStepComplete (Thread thread)
		{
			AD7StepCompleteEvent e = new AD7StepCompleteEvent ();
			Send (e, AD7StepCompleteEvent.IID, thread);
		}

		public void OnProgramDestroy (Process process, uint exitCode)
		{
			AD7ProgramDestroyEvent e = new AD7ProgramDestroyEvent (exitCode);
			Send (e, AD7ProgramDestroyEvent.IID, process, null);
		}

		// Engines notify the debugger that a breakpoint has bound through the breakpoint bound event.
		public void OnBreakpointBound (BoundBreakpoint bound)
		{
			AD7BreakpointBoundEvent e = new AD7BreakpointBoundEvent (bound);
			Send (e, AD7BreakpointBoundEvent.IID, null);
		}

		public void OnBreakpointError (ErrorBreakpoint breakpoint)
		{
			AD7BreakpointErrorEvent e = new AD7BreakpointErrorEvent (breakpoint);
			Send (e, AD7BreakpointErrorEvent.IID, null);
		}

		public void OnBreakpointHit (Thread thread, BoundBreakpoint bpt)
		{
			AD7BreakpointEvent e = new AD7BreakpointEvent (bpt);
			Send (e, AD7BreakpointEvent.IID, thread);
		}

		public void OnTargetOutput (string output)
		{
			AD7OutputDebugStringEvent e = new AD7OutputDebugStringEvent (output);
			Send (e, AD7OutputDebugStringEvent.IID, null);
		}
	}
}
