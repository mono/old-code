using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Mono.VisualStudio.Debugger
{
	public class ErrorBreakpoint : IDebugErrorBreakpoint2
	{
		public PendingBreakpoint PendingBreakpoint
		{
			get;
			private set;
		}

		public ErrorBreakpointResolution ErrorResolution
		{
			get;
			private set;
		}

		public ErrorBreakpoint (PendingBreakpoint pending, ErrorBreakpointResolution error)
		{
			this.PendingBreakpoint = pending;
			this.ErrorResolution = error;
		}

		#region IDebugErrorBreakpoint2 Members

		public int GetBreakpointResolution (out IDebugErrorBreakpointResolution2 error)
		{
			error = ErrorResolution;
			return COM.S_OK;
		}

		public int GetPendingBreakpoint (out IDebugPendingBreakpoint2 pending)
		{
			pending = PendingBreakpoint;
			return COM.S_OK;
		}

		#endregion
	}
}
