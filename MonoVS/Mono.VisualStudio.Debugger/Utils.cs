using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Debugger.Interop;
using System.Runtime.InteropServices;

namespace Mono.VisualStudio.Debugger
{
	public class Utils
	{
		const string FileName = "c:\\Work\\MonoVS\\Server\\logfile";

		static Utils ()
		{
			Debug.AutoFlush = true;
			Trace.AutoFlush = true;
			TextWriterTraceListener file = new TextWriterTraceListener (new FileStream (FileName, FileMode.Append));
			Debug.Listeners.Add (file);
		}

		public static void Message (string message, params object[] args)
		{
			Debug.WriteLine (String.Format (message, args), "MonoVS");
		}

		public static void RequireOk (int hr)
		{
			if (hr != 0) {
				// throw a useful exception
				Exception ex = Marshal.GetExceptionForHR (hr, IntPtr.Zero);
			}
		}

		public static void CheckOk (int hr)
		{
			if (hr != 0) {
				// throw a useful exception
				Exception ex = Marshal.GetExceptionForHR (hr, IntPtr.Zero);
			}
		}

		public static int UnexpectedException (Exception e)
		{
			return COM.RPC_E_SERVERFAULT;
		}
	}

	public class ComponentException : Exception
	{
		int hr;

		public ComponentException (int hr)
			: base ()
		{
			this.hr = hr;
		}

		new public int HResult
		{
			get
			{
				return hr;
			}
		}
	}
}
