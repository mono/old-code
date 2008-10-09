using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.Debugger.Interop;
using System.Diagnostics;

using Mono.VisualStudio.Mdb;
namespace Mono.VisualStudio.Debugger
{
	// This class represents a breakpoint that has been bound to a location in the debuggee. It is a child of the pending breakpoint
	// that creates it. Unless the pending breakpoint only has one bound breakpoint, each bound breakpoint is displayed as a child of the
	// pending breakpoint in the breakpoints window. Otherwise, only one is displayed.
	public class BoundBreakpoint : IDebugBoundBreakpoint2
	{
		public PendingBreakpoint PendingBreakpoint {
			get; private set;
		}

		public BreakpointResolution Resolution {
			get; private set;
		}

		public Engine Engine {
			get; private set;
		}

		internal IServerBreakpoint Handle {
			get; private set;
		}

		private uint m_address;

		private bool enabled;
		private bool deleted;

		public BoundBreakpoint (Engine engine, PendingBreakpoint pending, BreakpointResolution resolution,
			IServerBreakpoint handle)
		{
			this.Engine = engine;
			this.PendingBreakpoint = pending;
			this.Resolution = resolution;
			this.Handle = handle;

			enabled = true;
			deleted = false;
		}

		#region IDebugBoundBreakpoint2 Members

		// Called when the breakpoint is being deleted by the user.
		int IDebugBoundBreakpoint2.Delete ()
		{
			return COM.S_OK;
			throw new NotImplementedException ();
		}

		// Called by the debugger UI when the user is enabling or disabling a breakpoint.
		public int Enable (int enable)
		{
			bool new_enabled = enable == 0 ? false : true;
			if (new_enabled != enabled) {
				// A production debug engine would remove or add the underlying int3 here. The sample engine does not support true disabling
				// of breakpionts.
			}
			new_enabled = enabled;
			return COM.S_OK;
		}

		// Return the breakpoint resolution which describes how the breakpoint bound in the debuggee.
		public int GetBreakpointResolution (out IDebugBreakpointResolution2 resolution)
		{
			resolution = Resolution;
			return COM.S_OK;
		}

		// Return the pending breakpoint for this bound breakpoint.
		public int GetPendingBreakpoint (out IDebugPendingBreakpoint2 pending)
		{
			pending = PendingBreakpoint;
			return COM.S_OK;
		}

		// 
		public int GetState (out uint state)
		{
			state = 0;
			if (deleted)
				state = (uint) enum_BP_STATE.BPS_DELETED;
			else if (enabled)
				state = (uint) enum_BP_STATE.BPS_ENABLED;
			else if (!enabled)
				state = (uint) enum_BP_STATE.BPS_DISABLED;
			return COM.S_OK;
		}

		// The sample engine does not support hit counts on breakpoints. A real-world debugger will want to keep track 
		// of how many times a particular bound breakpoint has been hit and return it here.
		int IDebugBoundBreakpoint2.GetHitCount (out uint pdwHitCount)
		{
			pdwHitCount = 0;
			return COM.S_OK;
			throw new NotImplementedException ();
		}

		// The sample engine does not support conditions on breakpoints.
		// A real-world debugger will use this to specify when a breakpoint will be hit
		// and when it should be ignored.
		int IDebugBoundBreakpoint2.SetCondition (BP_CONDITION bpCondition)
		{
			throw new NotImplementedException ();
		}

		// The sample engine does not support hit counts on breakpoints. A real-world debugger will want to keep track 
		// of how many times a particular bound breakpoint has been hit. The debugger calls SetHitCount when the user 
		// resets a breakpoint's hit count.
		int IDebugBoundBreakpoint2.SetHitCount (uint dwHitCount)
		{
			throw new NotImplementedException ();
		}

		// The sample engine does not support pass counts on breakpoints.
		// This is used to specify the breakpoint hit count condition.
		int IDebugBoundBreakpoint2.SetPassCount (BP_PASSCOUNT bpPassCount)
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}
