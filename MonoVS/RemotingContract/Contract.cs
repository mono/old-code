using System;
using System.Collections.Generic;
using System.Text;

namespace Mono.VisualStudio.Mdb
{
	public interface IMdbController
	{
		void RegisterDebugger (IMdbManager a);
	}

	public interface IMdbManager
	{
		void AddDirectoryMapping (string source, string target);

		/*
		 * Launch the server, but don't actually start the target.
		 */
		IDebuggerServer Launch (string exe, string args, string dir, string env, IClientEventSink sink);
	}

	public interface IBreakpointManager
	{
		IServerBreakpoint InsertBreakpoint (ISourceLocation location);
	}

	public interface IBreakpointManagerEventSink
	{
		void OnBreakpointBound (IServerBreakpoint breakpoint);

		void OnBreakpointError (IServerBreakpoint breakpoint, string message);
	}

	public interface IServerBreakpoint
	{ }

	public interface IDebuggerServer
	{
		/*
		 * We start the target here.
		 */
		IServerProcess Start (IProcessEventSink sink);

		/*
		 * Get the breakpoint manager.
		 */
		IBreakpointManager CreateBreakpointManager (IBreakpointManagerEventSink sink);

		/*
		 * Kill the target.
		 */
		void Kill ();
	}

	public interface IProcessEventSink
	{
		/*
		 * The target has been started and is now stopped; we're ready to insert breakpoints.
		 */
		void OnProgramStarted (IServerThread thread);

		void OnThreadCreated (IServerThread thread);

        void OnThreadExited (IServerThread thread);

		void OnBreakpointHit (IServerThread thread, IServerBreakpoint breakpoint);

		void OnStepComplete (IServerThread thread);

		void OnTargetOutput (string output);

        void OnProcessExited ();
	}

	public interface IClientEventSink
	{
	}

	public interface IServerProcess
	{
		void ResumeProcess ();

        /*
         * Stop all threads in the target.
         */
        void Stop ();
	}

	public interface IServerThread
	{
		int ID
		{
			get;
		}

		IServerStackFrame GetFrame ();

        IServerBacktrace GetBacktrace ();

		void StepInto ();

		void StepOver ();

		void StepOut ();

		void Continue ();
	}

    public interface IServerBacktrace
    {
        IServerStackFrame[] Frames
        {
            get;
        }
    }

	public interface IServerStackFrame
	{
		ulong Address
		{
			get;
		}

		ulong StackPointer
		{
			get;
		}

		string Name
		{
			get;
		}

		ISourceLocation GetLocation ();
	}

	public interface ISourceLocation
	{
		string FileName
		{
			get;
		}

		int Line
		{
			get;
		}
	}
}
