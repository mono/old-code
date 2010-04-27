//
// Mono.AppServer.RemoteLoader
//
// Authors:
//   Brian Ritchie (brianlritchie@hotmail.com)
//
// Copyright (C) Brian Ritchie, 2003
//
using System;
using System.Collections;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Messaging;


namespace Mono.AppServer
{
	/// <summary>
	/// Summary description for RemoteLoader.
	/// </summary>
	public class RemoteLoader: MarshalByRefObject
	{
		TrackingHandler th;
		public ApplicationLog Log;

		public ArrayList LoadedTypes;

		public RemoteLoader()
		{
			Log=(ApplicationLog) AppDomain.CurrentDomain.GetData("AppLog");
			Context.RegisterDynamicProperty(
				new RemoteServerDynamicProperty(AppDomain.CurrentDomain.FriendlyName,Log),null,null);
			LoadedTypes=new ArrayList();
		}

		public void Configure(string ConfigFile)
		{
			RemotingConfiguration.Configure(ConfigFile);
			th=new TrackingHandler(this);
			System.Runtime.Remoting.Services.TrackingServices.RegisterTrackingHandler(th);
			System.Diagnostics.Trace.Listeners.Add(new RemotingTraceListener(this));
			System.Diagnostics.Trace.WriteLine("App Configured");
		}

		public AppDomain AppDomain
		{
			get
			{
				return AppDomain.CurrentDomain;
			}
		}

		public ApplicationAssembly[] GetLoadedAssemblies()
		{
			ArrayList AssemblyList=new ArrayList();
			foreach (Assembly a in AppDomain.GetAssemblies())
			{
				// ApplicationAssembly assembly=new ApplicationAssembly(a,this);
				ApplicationAssembly assembly=new ApplicationAssembly(a);
				AssemblyList.Add(assembly);
			}
			return (ApplicationAssembly[]) AssemblyList.ToArray(typeof(ApplicationAssembly));
		}

		public void UnregisterChannels()
		{
			foreach (IChannel channel in ChannelServices.RegisteredChannels)
			{
				ChannelServices.UnregisterChannel(channel);
			}
		}
		
		/// <summary>
		/// InitializeLifetimeService() returns "null" which means indefinite lifetime!
		/// http://www.dotnetremoting.cc/FAQs/PUBLISHING_OBJECT.asp
		/// </summary>
		/// <returns></returns>
		public override Object InitializeLifetimeService()
		{
			return null;
		}


	}



}

