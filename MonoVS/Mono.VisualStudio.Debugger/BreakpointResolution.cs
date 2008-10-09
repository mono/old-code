using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.Debugger.Interop;
using System.Runtime.InteropServices;

namespace Mono.VisualStudio.Debugger
{
	// This class represents the information that describes a bound breakpoint.
	public class BreakpointResolution : IDebugBreakpointResolution2
	{
		private readonly Process Process;

		public BreakpointResolution (Process process)
		{
			this.Process = process;
		}

		#region IDebugBreakpointResolution2 Members

		// Gets the type of the breakpoint represented by this resolution. 
		public int GetBreakpointType (out uint type)
		{
			type = (uint) enum_BP_TYPE.BPT_CODE;
			return COM.S_OK;
		}

		public int GetResolutionInfo (uint fields, BP_RESOLUTION_INFO[] info)
		{
#if FIXME
			if ((fields & (uint) enum_BPRESI_FIELDS.BPRESI_BPRESLOCATION) != 0) {
				// The sample engine only supports code breakpoints.
				BP_RESOLUTION_LOCATION location = new BP_RESOLUTION_LOCATION ();
				location.bpType = (uint) enum_BP_TYPE.BPT_CODE;

				// The debugger will not QI the IDebugCodeContex2 interface returned here. We must pass the pointer
				// to IDebugCodeContex2 and not IUnknown.
				AD7MemoryAddress codeContext = new AD7MemoryAddress (Engine, m_address);
				codeContext.SetDocumentContext (m_documentContext);
				location.unionmember1 = Marshal.GetComInterfaceForObject (codeContext, typeof (IDebugCodeContext2));
				info[0].bpResLocation = location;
				info[0].dwFields |= (uint) enum_BPRESI_FIELDS.BPRESI_BPRESLOCATION;

			}
#endif

			if ((fields & (uint) enum_BPRESI_FIELDS.BPRESI_PROGRAM) != 0) {
				info[0].pProgram = Process;
				info[0].dwFields |= (uint) enum_BPRESI_FIELDS.BPRESI_PROGRAM;
			}

			return COM.S_OK;
		}

		#endregion
	}

	public class ErrorBreakpointResolution : IDebugErrorBreakpointResolution2
	{
		public Process Process
		{
			get;
			private set;
		}

		public string Message
		{
			get;
			private set;
		}

		public ErrorBreakpointResolution (Process process, string message)
		{
			this.Process = process;
			this.Message = message;
		}

		#region IDebugErrorBreakpointResolution2 Members

		public uint ErrorType
		{
			get
			{
				return (uint) enum_BP_ERROR_TYPE.BPET_GENERAL_ERROR;
			}
		}

		public int GetBreakpointType (out uint type)
		{
			type = ErrorType;
			return COM.S_OK;
		}

		public int GetResolutionInfo (uint fields, BP_ERROR_RESOLUTION_INFO[] info)
		{
			if ((fields & (uint) enum_BPERESI_FIELDS.BPERESI_PROGRAM) != 0) {
				info[0].pProgram = Process;
				info[0].dwFields |= (uint) enum_BPERESI_FIELDS.BPERESI_PROGRAM;
			}
			
			if ((fields & (uint) enum_BPERESI_FIELDS.BPERESI_MESSAGE) != 0) {
				info[0].bstrMessage = Message;
				info[0].dwFields |= (uint) enum_BPERESI_FIELDS.BPERESI_MESSAGE;
			}

			if ((fields & (uint) enum_BPERESI_FIELDS.BPERESI_TYPE) != 0) {
				info[0].dwType = ErrorType;
				info[0].dwFields |= (uint) enum_BPERESI_FIELDS.BPERESI_TYPE;
			}

			return COM.S_OK;
		}

		#endregion
	}
}
