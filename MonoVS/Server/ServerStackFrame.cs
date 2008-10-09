using System;
using System.Text;
using System.Collections.Generic;
using MDB=Mono.Debugger;

namespace Mono.VisualStudio.Mdb
{
public class ServerStackFrame : MarshalByRefObject, IServerStackFrame
{
	public ServerThread Thread {
		get; private set;
	}

	public MDB.StackFrame Frame {
		get; private set;
	}

	public ServerStackFrame (ServerThread thread, MDB.StackFrame frame)
	{
		this.Thread = thread;
		this.Frame = frame;
	}

	public ulong Address {
		get { return (ulong) Frame.TargetAddress.Address; }
	}

	public ulong StackPointer {
		get { return (ulong) Frame.StackPointer.Address; }
	}

	public string Name {
		get {
			StringBuilder sb = new StringBuilder ();
			if (Frame == null) {
				Console.WriteLine ("NULL FRAME!");
				return "<null>";
			}
			if (Frame.Method != null) {
				sb.Append (Frame.Method.Name);
				if (Frame.Method.IsLoaded) {
					long offset = Frame.TargetAddress - Frame.Method.StartAddress;
					if (offset > 0)
						sb.Append (String.Format ("+0x{0:x}", offset));
					else if (offset < 0)
						sb.Append (String.Format ("-0x{0:x}", -offset));
				}
			} else if (Frame.Name != null)
				sb.Append (Frame.Name);
			else
				sb.Append (String.Format ("{0}", Frame.TargetAddress));

			return sb.ToString ();
		}
	}

	public ISourceLocation GetLocation ()
	{
		if (Frame.SourceAddress == null)
			return null;

		return new SourceLocation (Frame.SourceAddress);
	}

	protected class SourceLocation : MarshalByRefObject, ISourceLocation
	{
		string filename;
		int line;

		public SourceLocation (MDB.SourceAddress address)
		{
			this.filename = address.SourceFile.FileName;
			this.line = address.Row;
		}

		public string FileName {
			get { return filename; }
		}

		public int Line {
			get { return line; }
		}
	}
}

public class ServerBacktrace : MarshalByRefObject, IServerBacktrace
{
	public ServerThread Thread {
		get; private set;
	}

	public MDB.Backtrace Backtrace {
		get; private set;
	}

	public ServerBacktrace (ServerThread thread, MDB.Backtrace bt)
	{
		this.Thread = thread;
		this.Backtrace = bt;
	}

	IServerStackFrame[] IServerBacktrace.Frames {
		get {
			IServerStackFrame[] frames = new IServerStackFrame [Backtrace.Count];
			for (int i = 0; i < frames.Length; i++)
				frames [i] = new ServerStackFrame (Thread, Backtrace [i]);
			return frames;
		}
	}
}
}
