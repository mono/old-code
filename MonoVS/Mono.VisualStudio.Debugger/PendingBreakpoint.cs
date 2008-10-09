using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Debugger.Interop;

using Mono.VisualStudio.Mdb;

namespace Mono.VisualStudio.Debugger
{
	// This class represents a pending breakpoint which is an abstract representation of a breakpoint before it is bound.
	// When a user creates a new breakpoint, the pending breakpoint is created and is later bound. The bound breakpoints
	// become children of the pending breakpoint.
	public class PendingBreakpoint : IDebugPendingBreakpoint2
	{
		public BreakpointManager BreakpointManager
		{
			get;
			private set;
		}

		public IDebugBreakpointRequest2 Request
		{
			get;
			private set;
		}

		public enum_BP_LOCATION_TYPE LocationType;
		private readonly BP_REQUEST_INFO RequestInfo;

		public ErrorBreakpoint ErrorBreakpoint
		{
			get;
			internal set;
		}

		public PendingBreakpoint (BreakpointManager manager, IDebugBreakpointRequest2 request)
		{
			this.BreakpointManager = manager;
			this.Request = request;

			var info = new BP_REQUEST_INFO[1];
			Utils.RequireOk (Request.GetRequestInfo ((uint) enum_BPREQI_FIELDS.BPREQI_BPLOCATION, info));
			RequestInfo = info[0];

			uint type;
			Utils.RequireOk (Request.GetLocationType (out type));
			LocationType = (enum_BP_LOCATION_TYPE) type;
		}

		public bool CanBind ()
		{
			Utils.Message ("CAN BIND: {0} {1:x}", LocationType, LocationType);
			if (LocationType == enum_BP_LOCATION_TYPE.BPLT_CODE_FILE_LINE) {
				if ((RequestInfo.dwFields & (uint) enum_BPREQI_FIELDS.BPREQI_BPLOCATION) != 0)
					return true;
			}

			return false;
		}

		public bool Bind ()
		{
			if (!CanBind ())
				return false;

			string filename;
			IDebugDocumentPosition2 doc = (IDebugDocumentPosition2) Marshal.GetObjectForIUnknown (RequestInfo.bpLocation.unionmember2);
			Utils.RequireOk (doc.GetFileName (out filename));

			TEXT_POSITION[] start = new TEXT_POSITION[1], end = new TEXT_POSITION[1];
			Utils.RequireOk (doc.GetRange (start, end));

			Utils.Message ("BIND: {0} {1} {2} {3} {4}", filename,
				start[0].dwLine, start[0].dwColumn, end[0].dwLine, end[0].dwColumn);

			return BreakpointManager.BindBreakpoint (this, new SourceLocation {
				FileName = filename, Line = (int) start[0].dwLine
			});
		}

		protected class SourceLocation : MarshalByRefObject, ISourceLocation
		{
			public string FileName;
			public int Line;

			string ISourceLocation.FileName
			{
				get { return FileName; }
			}

			int ISourceLocation.Line
			{
				get { return Line; }
			}
		}

		#region IDebugPendingBreakpoint2 Members

		int IDebugPendingBreakpoint2.CanBind (out IEnumDebugErrorBreakpoints2 error)
		{
			error = null;
			return CanBind () ? COM.S_OK : COM.S_FALSE;
		}

		int IDebugPendingBreakpoint2.Bind ()
		{
			try {
				return Bind () ? COM.S_OK : COM.S_FALSE;
			} catch (Exception ex) {
				Utils.Message ("FUCK: {0}", ex);
				return COM.S_FALSE;
			}
		}

		public int Enable (int fEnable)
		{
			return COM.S_OK;
		}

		int IDebugPendingBreakpoint2.Delete ()
		{
			return COM.S_OK;
		}

		int IDebugPendingBreakpoint2.EnumBoundBreakpoints (out IEnumDebugBoundBreakpoints2 ppEnum)
		{
			throw new NotImplementedException ();
		}

		public int EnumErrorBreakpoints (uint type, out IEnumDebugErrorBreakpoints2 e)
		{
			if (ErrorBreakpoint != null) {
				e = new AD7ErrorBreakpointsEnum (ErrorBreakpoint);
				return COM.S_OK;
			}

			e = null;
			return COM.S_FALSE;
		}

		int IDebugPendingBreakpoint2.GetBreakpointRequest (out IDebugBreakpointRequest2 ppBPRequest)
		{
			throw new NotImplementedException ();
		}

		int IDebugPendingBreakpoint2.GetState (PENDING_BP_STATE_INFO[] pState)
		{
			throw new NotImplementedException ();
		}

		int IDebugPendingBreakpoint2.SetCondition (BP_CONDITION bpCondition)
		{
			throw new NotImplementedException ();
		}

		int IDebugPendingBreakpoint2.SetPassCount (BP_PASSCOUNT bpPassCount)
		{
			throw new NotImplementedException ();
		}

		int IDebugPendingBreakpoint2.Virtualize (int fVirtualize)
		{
			return COM.S_OK;
		}

		#endregion
	}
}
