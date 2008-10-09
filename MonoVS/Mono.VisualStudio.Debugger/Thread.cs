using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.Debugger.Interop;
using System.Diagnostics;

using Mono.VisualStudio.Mdb;

namespace Mono.VisualStudio.Debugger
{
	public class Thread : IDebugThread2
	{
		public Process Process
		{
			get;
			private set;
		}

		internal IServerThread ServerThread
		{
			get;
			private set;
		}

		public int ID
		{
			get;
			private set;
		}

		public Thread(Process process, IServerThread thread)
		{
			this.Process = process;
			this.ServerThread = thread;
			this.ID = thread.ID;
		}

		#region IDebugThread2 Members

		int IDebugThread2.CanSetNextStatement(IDebugStackFrame2 pStackFrame, IDebugCodeContext2 pCodeContext)
		{
			return COM.S_OK;
		}

        int IDebugThread2.EnumFrameInfo (uint dwFieldSpec, uint nRadix, out IEnumDebugFrameInfo2 ppEnum)
        {
            Utils.Message ("ENUM FRAME INFO!");

            IServerBacktrace server_backtrace = ServerThread.GetBacktrace ();
            if (server_backtrace == null)
            {
                ppEnum = null;
                return COM.S_FALSE;
            }

            IServerStackFrame[] server_frames = server_backtrace.Frames;

            FRAMEINFO[] array = new FRAMEINFO[server_frames.Length];
            for (int i = 0; i < server_frames.Length; i++)
            {
                StackFrame frame = new StackFrame (Process.Engine, server_frames[i]);
                frame.SetFrameInfo (dwFieldSpec, out array[i]);
            }

            ppEnum = new AD7FrameInfoEnum (array);
            return COM.S_OK;
        }

		int IDebugThread2.GetLogicalThread(IDebugStackFrame2 pStackFrame, out IDebugLogicalThread2 ppLogicalThread)
		{
			ppLogicalThread = null;
			return COM.E_NOTIMPL;
		}

        public int GetName (out string pbstrName)
        {
            pbstrName = String.Format ("Thread @{0}", ID);
            return COM.S_OK;

        }

		public int GetProgram (out IDebugProgram2 program)
		{
			program = Process;
			return COM.S_OK;
		}

		public int GetThreadId (out uint thread_id)
		{
			thread_id = (uint) ID;
			return COM.S_OK;
		}

		public int GetThreadProperties (uint fields, THREADPROPERTIES[] tp)
		{
			tp[0].dwFields = 0;

			if ((fields & (uint) enum_THREADPROPERTY_FIELDS.TPF_ID) != 0) {
				tp[0].dwThreadId = (uint) ID;
				tp[0].dwFields |= (uint) enum_THREADPROPERTY_FIELDS.TPF_ID;
			}

			if ((fields & (uint) enum_THREADPROPERTY_FIELDS.TPF_NAME) != 0) {
				tp[0].bstrName = String.Format ("Thread @{0}", ID);
				tp[0].dwFields |= (uint) enum_THREADPROPERTY_FIELDS.TPF_NAME;
			}

			if ((fields & (uint) enum_THREADPROPERTY_FIELDS.TPF_STATE) != 0) {
				tp[0].dwThreadState = (uint) enum_THREADSTATE.THREADSTATE_STOPPED;
				tp[0].dwFields |= (uint) enum_THREADPROPERTY_FIELDS.TPF_STATE;
			}

			if ((fields & (uint) enum_THREADPROPERTY_FIELDS.TPF_PRIORITY) != 0) {
				tp[0].bstrPriority = "Normal";
				tp[0].dwFields |= (uint) enum_THREADPROPERTY_FIELDS.TPF_PRIORITY;
			}

			if ((fields & (uint) enum_THREADPROPERTY_FIELDS.TPF_LOCATION) != 0) {
				try {
					IServerStackFrame frame = ServerThread.GetFrame ();
					if (frame != null) {
						tp[0].bstrLocation = frame.Name;
						tp[0].dwFields |= (uint) enum_THREADPROPERTY_FIELDS.TPF_LOCATION;
					}
				} catch {
				}
			}

			return COM.S_OK;
		}

		int IDebugThread2.Resume(out uint pdwSuspendCount)
		{
			throw new NotImplementedException();
		}

		int IDebugThread2.SetNextStatement(IDebugStackFrame2 pStackFrame, IDebugCodeContext2 pCodeContext)
		{
			throw new NotImplementedException();
		}

		int IDebugThread2.SetThreadName(string pszName)
		{
			throw new NotImplementedException();
		}

		int IDebugThread2.Suspend(out uint pdwSuspendCount)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
