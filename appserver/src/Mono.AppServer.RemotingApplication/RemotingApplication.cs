//
// Mono.AppServer.RemotingApplication
//
// Authors:
//   Brian Ritchie (brianlritchie@hotmail.com)
//
// Copyright (C) Brian Ritchie, 2003
//
using System;
using System.Threading;

namespace Mono.AppServer
{
	/// <summary>
	/// Summary description for RemotingApplication.
	/// </summary>
	public class RemotingApplication : ApplicationBase
	{
		protected RemoteLoader remoteLoader;

		public RemotingApplication() : base()
		{
		}

		public override void Load()
		{
			AppDomainSetup setup = new AppDomainSetup();
			setup.ApplicationBase = BaseDirectory;
			setup.PrivateBinPath = BaseDirectory;
			setup.ApplicationName = Name ;
			setup.ShadowCopyFiles = "true";
			setup.ShadowCopyDirectories = BaseDirectory;
			setup.LoaderOptimization=LoaderOptimization.MultiDomainHost;
			setup.ConfigurationFile="app.config";
			_Domain = AppDomain.CreateDomain(
				setup.ApplicationName, null, setup);

			Domain.SetData("AppLog",log);

			remoteLoader = (RemoteLoader) Domain.CreateInstanceFromAndUnwrap(
				"Mono.AppServer.RemotingApplication.dll", 
				"Mono.AppServer.RemoteLoader");
			remoteLoader.Configure(BaseDirectory+@"\app.config");
			_Loaded=true;
		}

		public override void Unload()
		{
			_Loaded=false;
			if (Domain!=null)
			{
				remoteLoader.UnregisterChannels();
				Thread UnloaderThread=new Thread(
					new System.Threading.ThreadStart(UnloadDelegate));
				UnloaderThread.Start();
			}
		}

		public override ApplicationAssembly[] GetLoadedAssemblies()
		{
			return remoteLoader.GetLoadedAssemblies();
		}

		public override ApplicationType[] GetLoadedTypes()
		{
			return (ApplicationType[]) remoteLoader.LoadedTypes.ToArray(typeof(ApplicationType));
		}


	}
}
