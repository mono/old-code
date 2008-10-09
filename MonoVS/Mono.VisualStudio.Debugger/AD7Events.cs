using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.Debugger.Interop;

// This file contains the private const intious event objects that are sent to the debugger from the sample engine via IDebugEventCallback2::Event.
// These are used in EngineCallback.cs.
// The events are how the engine tells the debugger about what is happening in the debuggee process. 
// There are three base classe the other events derive from: AD7AsynchronousEvent, AD7StoppingEvent, and AD7SynchronousEvent. These 
// each implement the IDebugEvent2.GetAttributes method for the type of event they represent. 
// Most events sent the debugger are asynchronous events.


namespace Mono.VisualStudio.Debugger
{
	#region Event base classes

	class AD7AsynchronousEvent : IDebugEvent2
	{
		public const uint Attributes = (uint) enum_EVENTATTRIBUTES.EVENT_ASYNCHRONOUS;

		public int GetAttributes (out uint attrs)
		{
			attrs = Attributes;
			return COM.S_OK;
		}
	}

	class AD7StoppingEvent : IDebugEvent2
	{
		public const uint Attributes = (uint) enum_EVENTATTRIBUTES.EVENT_ASYNC_STOP;

		public int GetAttributes (out uint attrs)
		{
			attrs = Attributes;
			return COM.S_OK;
		}
	}

	class AD7SynchronousEvent : IDebugEvent2
	{
		public const uint Attributes = (uint) enum_EVENTATTRIBUTES.EVENT_SYNCHRONOUS;

		public int GetAttributes (out uint attrs)
		{
			attrs = Attributes;
			return COM.S_OK;
		}
	}

	class AD7SynchronousStoppingEvent : IDebugEvent2
	{
		public const uint Attributes = (uint) enum_EVENTATTRIBUTES.EVENT_STOPPING | (uint) enum_EVENTATTRIBUTES.EVENT_SYNCHRONOUS;

		public int GetAttributes (out uint attrs)
		{
			attrs = Attributes;
			return COM.S_OK;
		}
	}

	#endregion

	// The debug engine (DE) sends this interface to the session debug manager (SDM) when an instance of the DE is created.
	sealed class EngineCreateEvent : AD7SynchronousEvent, IDebugEngineCreateEvent2
	{
		public const string IID = "FE5B734C-759D-4E59-AB04-F103343BDD06";

		public IDebugEngine2 Engine
		{
			get;
			private set;
		}

		EngineCreateEvent (Engine engine)
		{
			this.Engine = engine;
		}

		public static void Send (Engine engine)
		{
			EngineCreateEvent e = new EngineCreateEvent (engine);
			engine.EngineCallback.Send (e, IID, null, null);
		}

		public int GetEngine (out IDebugEngine2 engine)
		{
			engine = Engine;
			return COM.S_OK;
		}
	}

	// This interface is sent by the debug engine (DE) to the session debug manager (SDM) when a program is attached to.
	sealed class AD7ProgramCreateEvent : AD7SynchronousEvent, IDebugProgramCreateEvent2
	{
		public const string IID = "96CD11EE-ECD4-4E89-957E-B5D496FC4139";

		internal static void Send (Engine engine)
		{
			AD7ProgramCreateEvent e = new AD7ProgramCreateEvent ();
			engine.EngineCallback.Send (e, IID, null);
		}

		internal static void Send (Port port, IDebugProcess2 process, IDebugProgram2 program)
		{
			AD7ProgramCreateEvent e = new AD7ProgramCreateEvent ();
			Guid iid = new Guid (IID);
			port.PortEventsCP.Event (port.PortSupplier.Server, port, process, program, e, ref iid);
		}
	}

	// This interface is sent by the debug engine (DE) to the session debug manager (SDM) when a program has run to completion
	// or is otherwise destroyed.
	sealed class AD7ProgramDestroyEvent : AD7SynchronousEvent, IDebugProgramDestroyEvent2
	{
		public const string IID = "E147E9E3-6440-4073-A7B7-A65592C714B5";

		public uint ExitCode
		{
			get;
			private set;
		}

		public AD7ProgramDestroyEvent (uint exit_code)
		{
			ExitCode = exit_code;
		}

		internal static void Send (Port port, IDebugProcess2 process, IDebugProgram2 program)
		{
			AD7ProgramDestroyEvent e = new AD7ProgramDestroyEvent (0);
			Guid iid = new Guid (IID);
			port.PortEventsCP.Event (port.PortSupplier.Server, port, process, program, e, ref iid);
		}

		#region IDebugProgramDestroyEvent2 Members

		public int GetExitCode (out uint exit_code)
		{
			exit_code = ExitCode;
			return COM.S_OK;
		}

		#endregion
	}

	// This interface is sent by the debug engine (DE) to the session debug manager (SDM) when a thread is created in a program being debugged.
	sealed class AD7ThreadCreateEvent : AD7SynchronousEvent, IDebugThreadCreateEvent2
	{
		public const string IID = "2090CCFC-70C5-491D-A5E8-BAD2DD9EE3EA";
	}

	// This interface is sent by the debug engine (DE) to the session debug manager (SDM) when a thread has exited.
	sealed class AD7ThreadDestroyEvent : AD7AsynchronousEvent, IDebugThreadDestroyEvent2
	{
		public const string IID = "2C3B7532-A36F-4A6E-9072-49BE649B8541";

		public uint ExitCode
		{
			get;
			private set;
		}

		public AD7ThreadDestroyEvent (uint exit_code)
		{
			ExitCode = exit_code;
		}

		#region IDebugThreadDestroyEvent2 Members

		public int GetExitCode (out uint exit_code)
		{
			exit_code = ExitCode;
			return COM.S_OK;
		}

		#endregion
	}

	// This interface is sent by the debug engine (DE) to the session debug manager (SDM) when a program is loaded, but before any code is executed.
	sealed class AD7LoadCompleteEvent : AD7SynchronousEvent, IDebugLoadCompleteEvent2
	{
		public const string IID = "B1844850-1349-45D4-9F12-495212F5EB0B";
	}

	sealed class AD7EntryPointEvent : AD7SynchronousEvent, IDebugEntryPointEvent2
	{
		public const string IID = "e8414a3e-1642-48ec-829e-5f4040e16da9";
	}

	sealed class AD7StepCompleteEvent : AD7SynchronousStoppingEvent, IDebugStepCompleteEvent2
	{
		public const string IID = "0f7f24c1-74d9-4ea6-a3ea-7edb2d81441d";
	}

	// This interface tells the session debug manager (SDM) that an asynchronous break has been successfully completed.
	sealed class AD7AsyncBreakCompleteEvent : AD7StoppingEvent, IDebugBreakEvent2
	{
		public const string IID = "c7405d1d-e24b-44e0-b707-d8a5a4e1641b";
	}

	// This interface is sent by the debug engine (DE) to the session debug manager (SDM) to output a string for debug tracing.
	sealed class AD7OutputDebugStringEvent : AD7AsynchronousEvent, IDebugOutputStringEvent2
	{
		public const string IID = "569c4bb1-7b82-46fc-ae28-4536ddad753e";

		public string OutputString
		{
			get;
			private set;
		}

		public AD7OutputDebugStringEvent (string str)
		{
			OutputString = str;
		}

		#region IDebugOutputStringEvent2 Members

		public int GetString (out string str)
		{
			str = OutputString;
			return COM.S_OK;
		}

		#endregion
	}

	// This interface is sent when a pending breakpoint has been bound in the debuggee.
	sealed class AD7BreakpointBoundEvent : AD7AsynchronousEvent, IDebugBreakpointBoundEvent2
	{
		public const string IID = "1dddb704-cf99-4b8a-b746-dabb01dd13a0";

		public BoundBreakpoint BoundBreakpoint
		{
			get;
			private set;
		}

		public AD7BreakpointBoundEvent (BoundBreakpoint bound)
		{
			BoundBreakpoint = bound;
		}

		#region IDebugBreakpointBoundEvent2 Members

		public int EnumBoundBreakpoints (out IEnumDebugBoundBreakpoints2 e)
		{
			e = new AD7BoundBreakpointsEnum (BoundBreakpoint);
			return COM.S_OK;
		}

		public int GetPendingBreakpoint (out IDebugPendingBreakpoint2 pending)
		{
			pending = BoundBreakpoint.PendingBreakpoint;
			return COM.S_OK;
		}

		#endregion
	}

	sealed class AD7BreakpointErrorEvent : AD7SynchronousEvent, IDebugBreakpointErrorEvent2
	{
		public const string IID = "abb0ca42-f82b-4622-84e4-6903ae90f210";

		public ErrorBreakpoint ErrorBreakpoint
		{
			get;
			private set;
		}

		public AD7BreakpointErrorEvent (ErrorBreakpoint error)
		{
			this.ErrorBreakpoint = error;
		}


		#region IDebugBreakpointErrorEvent2 Members

		public int GetErrorBreakpoint (out IDebugErrorBreakpoint2 error)
		{
			error = ErrorBreakpoint;
			return COM.S_OK;
		}

		#endregion
	}


	// This Event is sent when a breakpoint is hit in the debuggee
	sealed class AD7BreakpointEvent : AD7SynchronousStoppingEvent, IDebugBreakpointEvent2
	{
		public const string IID = "501C1E21-C557-48B8-BA30-A1EAB0BC4A74";

		public IEnumDebugBoundBreakpoints2 BoundBreakpoints
		{
			get;
			private set;
		}

		public AD7BreakpointEvent (IEnumDebugBoundBreakpoints2 bound)
		{
			BoundBreakpoints = bound;
		}

		public AD7BreakpointEvent (BoundBreakpoint bound)
		{
			BoundBreakpoints = new AD7BoundBreakpointsEnum (bound);
		}

		#region IDebugBreakpointEvent2 Members

		public int EnumBreakpoints (out IEnumDebugBoundBreakpoints2 e)
		{
			e = BoundBreakpoints;
			return COM.S_OK;
		}

		#endregion
	}

	sealed class AD7ProcessCreateEvent : AD7SynchronousEvent, IDebugProcessCreateEvent2
	{
		public const string IID = "BAC3780F-04DA-4726-901C-BA6A4633E1CA";

		internal static void Send (Engine engine, Process process)
		{
			AD7ProcessCreateEvent e = new AD7ProcessCreateEvent ();
			engine.EngineCallback.Send (e, IID, process, null);
		}

		internal static void Send (Port port, IDebugProcess2 process)
		{
			AD7ProcessCreateEvent e = new AD7ProcessCreateEvent ();
			Guid iid = new Guid (IID);
			port.PortEventsCP.Event (port.PortSupplier.Server, port, process, null, e, ref iid);
		}
	}

    sealed class AD7ProcessDestroyEvent : AD7SynchronousEvent, IDebugProcessDestroyEvent2
    {
        public const string IID = "3E2A0832-17E1-4886-8C0E-204DA242995F";

        internal static void Send (Engine engine, Process process)
        {
            AD7ProcessDestroyEvent e = new AD7ProcessDestroyEvent ();
            engine.EngineCallback.Send (e, IID, process, null);
        }

        internal static void Send (Port port, IDebugProcess2 process)
        {
            AD7ProcessDestroyEvent e = new AD7ProcessDestroyEvent ();
            Guid iid = new Guid (IID);
            port.PortEventsCP.Event (port.PortSupplier.Server, port, process, null, e, ref iid);
        }
    }

    sealed class AD7ErrorEvent : AD7SynchronousEvent, IDebugErrorEvent2
	{
		public const string IID = "fdb7a36c-8c53-41da-a337-8bd86b14d5cb";

		public string ErrorMessage
		{
			get;
			private set;
		}

		public AD7ErrorEvent (string message)
		{
			this.ErrorMessage = message;
		}

		public AD7ErrorEvent (string message, params object[] args)
		{
			this.ErrorMessage = String.Format (message, args);
		}

		#region IDebugErrorEvent2 Members

		public int GetErrorMessage (out uint message_type, out string format, out int reason,
			out uint severity, out string helper_filename, out uint helper_id)
		{
			message_type = 2; // MT_MESSAGEBOX;
			format = ErrorMessage;
			reason = 0;
			severity = 16; // MB_CRITICAL;
			helper_filename = null;
			helper_id = 0;
			return COM.S_OK;
		}

		#endregion
	}
}
