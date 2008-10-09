using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Debugger.Interop;
using System.Runtime.InteropServices.ComTypes;

using Mono.VisualStudio.Mdb;

namespace Mono.VisualStudio.Debugger
{
	public class Process : IDebugProcess2, IDebugProgram3, IDebugProgramNode2
	{
		string Exe;
		Guid guid;

		public Process (Port port, Engine engine, string name, IDebuggerServer server)
		{
			this.Port = port;
			this.Engine = engine;
			this.DebuggerServer = server;

			Exe = name;
			guid = Guid.NewGuid ();

			BreakpointManager = new BreakpointManager (this, server);
		}

		public Guid Guid
		{
			get { return guid; }
		}

		public Port Port
		{
			get;
			private set;
		}

		public Engine Engine
		{
			get;
			private set;
		}

		public BreakpointManager BreakpointManager
		{
			get;
			private set;
		}

		internal IDebuggerServer DebuggerServer
		{
			get;
			private set;
		}

		internal IServerProcess ServerProcess
		{
			get;
			private set;
		}

		IDictionary<IServerThread, Thread> threads = new Dictionary<IServerThread, Thread> ();
		internal IDictionary<IServerThread, Thread> Threads
		{
			get { return threads; }
		}

		public IServerProcess Start ()
		{
			ServerProcess = DebuggerServer.Start (new EventSink (this));
			return ServerProcess;
		}

		bool IsSet (uint f, enum_PROCESS_INFO_FIELDS b)
		{
			return (f & ((uint) b)) != 0;
		}

		public int GetInfo (uint Fields, PROCESS_INFO[] processInfo)
		{
			processInfo[0].Fields = Fields;
			if (IsSet (Fields, enum_PROCESS_INFO_FIELDS.PIF_FILE_NAME))
				processInfo[0].bstrFileName = Exe;

			if (IsSet (Fields, enum_PROCESS_INFO_FIELDS.PIF_BASE_NAME))
				processInfo[0].bstrBaseName = System.IO.Path.GetFileName (Exe);

			if (IsSet (Fields, enum_PROCESS_INFO_FIELDS.PIF_TITLE))
				processInfo[0].bstrTitle = System.IO.Path.GetFileName (Exe);

			if (IsSet (Fields, enum_PROCESS_INFO_FIELDS.PIF_PROCESS_ID))
				throw new NotImplementedException ();
			if (IsSet (Fields, enum_PROCESS_INFO_FIELDS.PIF_SESSION_ID))
				throw new NotImplementedException ();
			if (IsSet (Fields, enum_PROCESS_INFO_FIELDS.PIF_ATTACHED_SESSION_NAME))
				throw new NotImplementedException ();
			if (IsSet (Fields, enum_PROCESS_INFO_FIELDS.PIF_CREATION_TIME))
				throw new NotImplementedException ();
			if (IsSet (Fields, enum_PROCESS_INFO_FIELDS.PIF_FLAGS))
				throw new NotImplementedException ();

			return COM.S_OK;
		}

		public int GetName (uint gnType, out string pbstrName)
		{
			pbstrName = Exe;
			return COM.S_OK;
		}

		public int GetPhysicalProcessId (AD_PROCESS_ID[] pProcessId)
		{
			pProcessId[0].ProcessIdType = 1; // 0 = PID, 1 = GUID
			pProcessId[0].guidProcessId = guid;
			return COM.S_OK;
		}

		public int GetPort (out IDebugPort2 outPort)
		{
			outPort = Port;
			return COM.S_OK;
		}

		public int GetProcessId (out Guid pguidProcessId)
		{
			pguidProcessId = guid;
			return COM.S_OK;
		}

		public int GetServer (out IDebugCoreServer2 ppServer)
		{
			throw new NotImplementedException ();
		}

		public int Terminate ()
		{
			if (DebuggerServer != null) {
				DebuggerServer.Kill ();
				DebuggerServer = null;
			}
			return COM.S_OK;
		}


		#region IDebugProcess2 Members

		public int GetProgramId (out Guid pguidProgramId)
		{
			pguidProgramId = Guid;
			return COM.S_OK;
		}

		public int GetEngineInfo (out string name, out Guid guid)
		{
			name = "Mono";
			guid = new Guid (Engine.Id);
			return COM.S_OK;
		}

		public int GetName (out string name)
		{
			name = "Hello World";
			return COM.S_OK;
		}

		public int EnumThreads (out IEnumDebugThreads2 e)
		{
			e = new AD7ThreadEnum (Threads.Values.Select (a => (IDebugThread2) a));
			return COM.S_OK;
		}

		public int Attach (IDebugEventCallback2 pCallback, Guid[] rgguidSpecificEngines, uint celtSpecificEngines, int[] rghrEngineAttach)
		{
			throw new NotImplementedException ();
		}

		public int CanDetach ()
		{
			return COM.S_FALSE;
		}

		public int CauseBreak ()
		{
            ServerProcess.Stop ();
            return COM.S_OK;
		}

		public int Detach ()
		{
			throw new NotImplementedException ();
		}

		public int EnumPrograms (out IEnumDebugPrograms2 ppEnum)
		{
			throw new NotImplementedException ();
		}

		public int GetAttachedSessionName (out string pbstrSessionName)
		{
			throw new NotImplementedException ();
		}

		#endregion

		#region IDebugProcess3 Members

		public int ExecuteOnThread (IDebugThread2 t)
		{
			Thread thread = (Thread) t;
			Utils.Message ("EXECUTE ON THREAD: {0}", thread.ID);
			thread.ServerThread.Continue ();
			return COM.S_OK;
		}

		public int Step (IDebugThread2 t, uint u_kind, uint u_step)
		{
			Thread thread = (Thread) t;
			enum_STEPUNIT step = (enum_STEPUNIT) u_step;
			enum_STEPKIND kind = (enum_STEPKIND) u_kind;

			Utils.Message ("STEP: {0} {1} {2}", thread.ID, kind, step);

			if (step == enum_STEPUNIT.STEP_INSTRUCTION) {
				Utils.Message ("STEP INSTRUCTION !");
			}

			switch (kind) {
				case enum_STEPKIND.STEP_INTO:
					thread.ServerThread.StepInto ();
					break;
				case enum_STEPKIND.STEP_OVER:
					thread.ServerThread.StepOver ();
					break;
				case enum_STEPKIND.STEP_OUT:
					thread.ServerThread.StepOut ();
					break;
				default:
					Engine.EngineCallback.OnError ("Unknown step kind: {0}", kind);
					return COM.S_FALSE;
			}
			return COM.S_OK;
		}

		public int Continue (IDebugThread2 t)
		{
			Thread thread = (Thread) t;
			Utils.Message ("CONTINUE: {0}", thread.ID);
			thread.ServerThread.Continue ();
			return COM.S_OK;
		}

		#endregion

		#region IDebugProcess2 Members

		int IDebugProcess2.Attach (IDebugEventCallback2 pCallback, Guid[] rgguidSpecificEngines, uint celtSpecificEngines, int[] rghrEngineAttach)
		{
			throw new NotImplementedException ();
		}

		int IDebugProcess2.Detach ()
		{
			throw new NotImplementedException ();
		}

		int IDebugProcess2.EnumPrograms (out IEnumDebugPrograms2 ppEnum)
		{
			throw new NotImplementedException ();
		}

		int IDebugProcess2.GetAttachedSessionName (out string pbstrSessionName)
		{
			throw new NotImplementedException ();
		}

		#endregion



		#region IDebugProgram3 Members

		int IDebugProgram3.Attach (IDebugEventCallback2 pCallback)
		{
			throw new NotImplementedException ();
		}

		int IDebugProgram3.Detach ()
		{
			throw new NotImplementedException ();
		}

		int IDebugProgram3.EnumCodeContexts (IDebugDocumentPosition2 pDocPos, out IEnumDebugCodeContexts2 ppEnum)
		{
			throw new NotImplementedException ();
		}

		int IDebugProgram3.EnumCodePaths (string pszHint, IDebugCodeContext2 pStart, IDebugStackFrame2 pFrame, int fSource, out IEnumCodePaths2 ppEnum, out IDebugCodeContext2 ppSafety)
		{
			throw new NotImplementedException ();
		}

		int IDebugProgram3.EnumModules (out IEnumDebugModules2 ppEnum)
		{
			throw new NotImplementedException ();
		}

		int IDebugProgram3.Execute ()
		{
			throw new NotImplementedException ();
		}

		int IDebugProgram3.GetDebugProperty (out IDebugProperty2 ppProperty)
		{
			throw new NotImplementedException ();
		}

		int IDebugProgram3.GetDisassemblyStream (uint dwScope, IDebugCodeContext2 pCodeContext, out IDebugDisassemblyStream2 ppDisassemblyStream)
		{
			throw new NotImplementedException ();
		}

		int IDebugProgram3.GetENCUpdate (out object ppUpdate)
		{
			throw new NotImplementedException ();
		}

		int IDebugProgram3.GetMemoryBytes (out IDebugMemoryBytes2 ppMemoryBytes)
		{
			throw new NotImplementedException ();
		}

		int IDebugProgram3.GetProcess (out IDebugProcess2 ppProcess)
		{
			throw new NotImplementedException ();
		}

		int IDebugProgram3.WriteDump (uint DUMPTYPE, string pszDumpUrl)
		{
			throw new NotImplementedException ();
		}

		#endregion


		#region IDebugProgram2 Members

		int IDebugProgram2.Attach (IDebugEventCallback2 pCallback)
		{
			throw new NotImplementedException ();
		}

		int IDebugProgram2.Detach ()
		{
			return COM.S_OK;
			throw new NotImplementedException ();
		}

		int IDebugProgram2.EnumCodeContexts (IDebugDocumentPosition2 pDocPos, out IEnumDebugCodeContexts2 ppEnum)
		{
			throw new NotImplementedException ();
		}

		int IDebugProgram2.EnumCodePaths (string pszHint, IDebugCodeContext2 pStart, IDebugStackFrame2 pFrame, int fSource, out IEnumCodePaths2 ppEnum, out IDebugCodeContext2 ppSafety)
		{
			throw new NotImplementedException ();
		}

		int IDebugProgram2.EnumModules (out IEnumDebugModules2 ppEnum)
		{
			throw new NotImplementedException ();
		}

		int IDebugProgram2.Execute ()
		{
			throw new NotImplementedException ();
		}

		int IDebugProgram2.GetDebugProperty (out IDebugProperty2 ppProperty)
		{
			throw new NotImplementedException ();
		}

		int IDebugProgram2.GetDisassemblyStream (uint dwScope, IDebugCodeContext2 pCodeContext, out IDebugDisassemblyStream2 ppDisassemblyStream)
		{
			throw new NotImplementedException ();
		}

		int IDebugProgram2.GetENCUpdate (out object ppUpdate)
		{
			throw new NotImplementedException ();
		}

		int IDebugProgram2.GetMemoryBytes (out IDebugMemoryBytes2 ppMemoryBytes)
		{
			throw new NotImplementedException ();
		}

		int IDebugProgram2.GetProcess (out IDebugProcess2 ppProcess)
		{
			throw new NotImplementedException ();
		}

		int IDebugProgram2.WriteDump (uint DUMPTYPE, string pszDumpUrl)
		{
			throw new NotImplementedException ();
		}

		#endregion

		#region Event Sink

		protected void OnProgramStarted (IServerThread thread)
		{
			Utils.Message ("DEBUGGER READY!");
			AD7ProgramCreateEvent.Send (Engine);

			Thread main_thread = new Thread (this, thread);
			threads.Add (thread, main_thread);

			Engine.EngineCallback.OnThreadStart (main_thread);
			Engine.EngineCallback.OnLoadComplete (main_thread);
		}

		protected void OnThreadCreated (IServerThread st)
		{
			Thread thread = new Thread (this, st);
            Engine.EngineCallback.OnThreadStart (thread);
			threads.Add (st, thread);
		}

        protected void OnThreadExited (IServerThread st)
        {
            if (!threads.ContainsKey (st))
                return;

            Thread thread = threads[st];
            Engine.EngineCallback.OnThreadDestroy (thread);
            threads.Remove (st);
        }

		protected void OnBreakpointHit (IServerThread thread, IServerBreakpoint bpt)
		{
			BoundBreakpoint bound = BreakpointManager.GetBoundBreakpoint (bpt);
			Engine.EngineCallback.OnBreakpointHit (threads[thread], bound);
		}

		protected void OnStepComplete (IServerThread thread)
		{
			Engine.EngineCallback.OnStepComplete (threads[thread]);
		}

		protected void OnTargetOutput (string output)
		{
			Engine.EngineCallback.OnTargetOutput (output);
		}

        protected void OnProcessExited ()
        {
            AD7ProcessDestroyEvent.Send (Engine, this);
        }

		protected class EventSink : MarshalByRefObject, IProcessEventSink
		{
			public readonly Process Process;

			public EventSink (Process process)
			{
				this.Process = process;
			}

			#region IProcessEventSink Members

			void IProcessEventSink.OnProgramStarted (IServerThread thread)
			{
				Process.OnProgramStarted (thread);
			}

			void IProcessEventSink.OnThreadCreated (IServerThread thread)
			{
				Process.OnThreadCreated (thread);
			}

            void IProcessEventSink.OnThreadExited (IServerThread thread)
            {
                Process.OnThreadExited (thread);
            }

			void IProcessEventSink.OnBreakpointHit (IServerThread thread, IServerBreakpoint bpt)
			{
				Process.OnBreakpointHit (thread, bpt);
			}

			void IProcessEventSink.OnStepComplete (IServerThread thread)
			{
				Process.OnStepComplete (thread);
			}

			void IProcessEventSink.OnTargetOutput (string output)
			{
				Process.OnTargetOutput (output);
			}

            void IProcessEventSink.OnProcessExited ()
            {
                Process.OnProcessExited ();
            }

			#endregion
		}

		#endregion

		#region IDebugProgramNode2 Obsolete Members

		int IDebugProgramNode2.Attach_V7 (IDebugProgram2 pMDMProgram, IDebugEventCallback2 pCallback, uint dwReason)
		{
			throw new NotImplementedException ();
		}

		int IDebugProgramNode2.DetachDebugger_V7 ()
		{
			throw new NotImplementedException ();
		}

		int IDebugProgramNode2.GetHostMachineName_V7 (out string pbstrHostMachineName)
		{
			throw new NotImplementedException ();
		}

		#endregion

		#region IDebugProgramNode2 Members

		public int GetHostPid (AD_PROCESS_ID[] info)
		{
			info[0].ProcessIdType = (uint) enum_AD_PROCESS_ID.AD_PROCESS_ID_GUID;
			info[0].guidProcessId = Guid;
			return COM.S_OK;
		}

		public int GetHostName (uint host_name_type, out string process_name)
		{
			process_name = "Mono";
			return COM.S_OK;
		}

		public int GetProgramName (out string program_name)
		{
			program_name = "Mono";
			return COM.S_OK;
		}

		#endregion
	}
}
