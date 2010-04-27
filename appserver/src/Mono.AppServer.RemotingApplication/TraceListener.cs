//
// Mono.AppServer.RemotingTraceListener
//
// Authors:
//   Brian Ritchie (brianlritchie@hotmail.com)
//
// Copyright (C) Brian Ritchie, 2003
//
using System;
using System.Diagnostics;

namespace Mono.AppServer
{
	/// <summary>
	/// Summary description for TraceListener.
	/// </summary>
	public class RemotingTraceListener : TraceListener
	{
		RemoteLoader loader;

		public RemotingTraceListener() {}

		public RemotingTraceListener(RemoteLoader loader)
		{
			this.loader=loader;
		}

		public override void Write(object o)
		{
			if (AppDomain.CurrentDomain==loader.AppDomain)
			{
				loader.Log.WriteLine(o.ToString());
			}
		}

		public override void WriteLine(object o)
		{
			if (AppDomain.CurrentDomain==loader.AppDomain)
			{
				loader.Log.WriteLine(o.ToString());
			}
		}

		public override void Write(string s)
		{
			if (AppDomain.CurrentDomain==loader.AppDomain)
			{
				loader.Log.WriteLine(s);
			}
		}

		public override void WriteLine(string s)
		{
			if (loader!=null)
			{
				if (AppDomain.CurrentDomain==loader.AppDomain)
				{
					loader.Log.WriteLine(s);
				}
			}
		}

	}
}
