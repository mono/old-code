using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Debugger.Interop;
using System.Diagnostics;
using System.Threading;

using Mono.VisualStudio.Mdb;

namespace Mono.VisualStudio.Debugger
{
	// Engine is the primary entrypoint object for the sample engine. 
	//
	// It implements:
	//
	// IDebugEngine2: This interface represents a debug engine (DE). It is used to manage various aspects of a debugging session, 
	// from creating breakpoints to setting and clearing exceptions.
	//
	// IDebugEngineLaunch2: Used by a debug engine (DE) to launch and terminate programs.
	//
	// IDebugProgram3: This interface represents a program that is running in a process. Since this engine only debugs one process at a time and each 
	// process only contains one program, it is implemented on the engine.
	//
	// IDebugEngineProgram2: This interface provides simultanious debugging of multiple threads in a debuggee.

	[ComVisible (true)]
	[Guid (Guids.EngineClass)]
	public class Engine : IDebugEngine2, IDebugEngineLaunch2
	{
		internal EngineCallback EngineCallback
		{
			get;
			private set;
		}

		public Process Process
		{
			get;
			private set;
		}

		public DebuggerController Controller
		{
			get;
			private set;
		}

		/// <summary>
		/// This is the engine GUID of the sample engine. It needs to be changed here and in the registration
		/// when creating a new engine.
		/// </summary>
		public const string Id = Guids.Engine;

		#region IDebugEngine2 Members

		int IDebugEngine2.Attach (IDebugProgram2[] rgpPrograms, IDebugProgramNode2[] rgpProgramNodes, uint celtPrograms, IDebugEventCallback2 pCallback, uint dwReason)
		{
			Utils.Message ("ATTACH!");
			EngineCreateEvent.Send (this);
			return COM.S_OK;
		}

		int IDebugEngine2.CauseBreak ()
		{
			return Process.CauseBreak ();
		}

		public int ContinueFromSynchronousEvent (IDebugEvent2 e)
		{
			Utils.Message ("CONTINUE FROM SYNC EVENT: {0}", e);
			if ((e is AD7LoadCompleteEvent) || (e is AD7EntryPointEvent)) {
				Utils.Message ("LOAD COMPLETE DONE!");
				Process.ServerProcess.ResumeProcess ();
			}
			return COM.S_OK;
		}

		public int CreatePendingBreakpoint (IDebugBreakpointRequest2 request,
			out IDebugPendingBreakpoint2 pending)
		{
			pending = Process.BreakpointManager.CreatePendingBreakpoint (request);
			return COM.S_OK;
		}

		int IDebugEngine2.DestroyProgram (IDebugProgram2 pProgram)
		{
			throw new NotImplementedException ();
		}

		int IDebugEngine2.EnumPrograms (out IEnumDebugPrograms2 ppEnum)
		{
			ppEnum = new AD7ProgramEnum (new IDebugProgram2[] { Process });
			return COM.S_OK;
		}

		int IDebugEngine2.GetEngineId (out Guid guid)
		{
			guid = new Guid (Id);
			return COM.S_OK;
		}

		int IDebugEngine2.RemoveAllSetExceptions (ref Guid guidType)
		{
			throw new NotImplementedException ();
		}

		int IDebugEngine2.RemoveSetException (EXCEPTION_INFO[] pException)
		{
			throw new NotImplementedException ();
		}

		int IDebugEngine2.SetException (EXCEPTION_INFO[] pException)
		{
			throw new NotImplementedException ();
		}

		int IDebugEngine2.SetLocale (ushort wLangID)
		{
			return COM.S_OK;
		}

		int IDebugEngine2.SetMetric (string pszMetric, object varValue)
		{
			return COM.S_OK;
		}

		int IDebugEngine2.SetRegistryRoot (string pszRegistryRoot)
		{
			// The sample engine does not read settings from the registry.
			return COM.S_OK;
		}

		#endregion

		#region IDebugEngineLaunch2 Members

		public int CanTerminateProcess (IDebugProcess2 process)
		{
			return COM.S_OK;
		}

		public int LaunchSuspended (string pszServer, IDebugPort2 pPort, string exe, string args,
			string dir, string env, string pszOptions, uint dwLaunchFlags,
			uint hStdInput, uint hStdOutput, uint hStdError, IDebugEventCallback2 callback,
			out IDebugProcess2 process)
		{
			Port port = (Port) pPort;
			EngineCallback = new EngineCallback (this, callback);

			IDebuggerServer server;

			try {
				IClientEventSink sink = new ClientEventSink (this);
				Controller = new DebuggerController (port.PortAddress);
				server = Controller.Launch (exe, args, dir, env, sink);
			} catch (Exception ex) {
				EngineCallback.OnError ("Failed to launch debugger: {0}", ex);
				process = null;
				return COM.S_FALSE;
			}

			process = Process = new Process (port, this, exe, server);
			AD7ProcessCreateEvent.Send (this, Process);
			return COM.S_OK;
		}

		public int ResumeProcess (IDebugProcess2 process)
		{
			// This has to be done here and not in LaunchSuspended().  Martin.
			Debug.Assert (process == Process);
			Process.Port.AddProgramNode (Process);
			AD7ProgramCreateEvent.Send (Process.Port, Process, Process);

			Process.Start ();

			return COM.S_OK;
		}

		public int TerminateProcess (IDebugProcess2 process)
		{
            Process.Terminate ();
            Controller.Kill ();

			Process.Port.RemoveProgramNode (Process);
			AD7ProgramDestroyEvent.Send (Process.Port, Process, Process);
			EngineCallback.OnProgramDestroy (Process, 255);

			return COM.S_OK;
		}

		#endregion

		class ClientEventSink : MarshalByRefObject, IClientEventSink
		{
			public readonly Engine Engine;

			public ClientEventSink (Engine engine)
			{
				this.Engine = engine;
			}
		}
	}
}
