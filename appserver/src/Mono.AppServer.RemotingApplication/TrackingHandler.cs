//
// Mono.AppServer.TrackingHandler
//
// Authors:
//   Brian Ritchie (brianlritchie@hotmail.com)
//
// Copyright (C) Brian Ritchie, 2003
//
using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Services;

namespace Mono.AppServer
{
	/// <summary>
	/// Summary description for TrackingHandler.
	/// </summary>
	public class TrackingHandler : ITrackingHandler
	{
		RemoteLoader loader;

		public TrackingHandler(RemoteLoader loader)
		{
			this.loader=loader;
		}

		public void MarshaledObject(
			object obj,
			ObjRef or
			)
		{
			Console.WriteLine(AppDomain.CurrentDomain.FriendlyName+" Marshalled: "+obj.GetType().FullName);
		}

		public void DisconnectedObject(
			object obj
			)
		{
			Console.WriteLine(AppDomain.CurrentDomain.FriendlyName+" Disconnected: "+obj.GetType().FullName);
		}

		public void UnmarshaledObject(
			object obj,
			ObjRef or
			)
		{
			Console.WriteLine(AppDomain.CurrentDomain.FriendlyName+" Unmarshaled: "+obj.GetType().FullName);
		}

	}
}
