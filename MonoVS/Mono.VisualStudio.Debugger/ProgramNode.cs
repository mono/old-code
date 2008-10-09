using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.Debugger.Interop;
using System.Diagnostics;

namespace Mono.VisualStudio.Debugger
{
	class ProgramNode : IDebugProgramNode2
	{
		Process process;

		public ProgramNode(Process process)
		{
			this.process = process;
		}

		public int GetEngineInfo(out string engineName, out Guid engineGuid)
		{
			engineName = "Mono";
			engineGuid = new Guid(Engine.Id);

			return COM.S_OK;
		}

		int IDebugProgramNode2.GetHostPid(AD_PROCESS_ID[] pHostProcessId)
		{
			pHostProcessId[0].ProcessIdType = (uint)enum_AD_PROCESS_ID.AD_PROCESS_ID_GUID;
			pHostProcessId[0].guidProcessId = process.Guid;
			return COM.S_OK;
		}

		// Gets the name of the process hosting a program.
		int IDebugProgramNode2.GetHostName(uint dwHostNameType, out string processName)
		{
			processName = "Mono";
			return COM.S_OK;
		}

		// Gets the name of a program.
		int IDebugProgramNode2.GetProgramName(out string programName)
		{
			programName = "Mono";
			return COM.S_OK;
		}

		#region IDebugProgramNode2 Members

		int IDebugProgramNode2.Attach_V7(IDebugProgram2 pMDMProgram, IDebugEventCallback2 pCallback, uint dwReason)
		{
			throw new NotImplementedException();
		}

		int IDebugProgramNode2.DetachDebugger_V7()
		{
			throw new NotImplementedException();
		}

		int IDebugProgramNode2.GetHostMachineName_V7(out string pbstrHostMachineName)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}