// mb-install-client.cs -- Tool to install results fed to
// self on stdin

using System;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

using Mono.Build;
using Monkeywrench;

public class MBInstallClient : MarshalByRefObject, IInstallerService {

	public MBInstallClient () {
	}

	public bool Install (Result installer, Result target, bool backwards, IBuildContext ctxt) {
		IResultInstaller iri = installer as IResultInstaller;
		return iri.InstallResult (target, backwards, ctxt);
	}

	public void DoneInstalling () {
		keep_going = false;
	}

	static TcpChannel channel;
	static bool keep_going = true;
	static IInstallerServiceNotify notify;
	static MBInstallClient singleton;

	public static int Main (string[] args) {
		channel = new TcpChannel (0);
		ChannelServices.RegisterChannel (channel);

		//RemotingConfiguration.RegisterWellKnownServiceType (typeof (MBInstallClient),
		//						    "MBuild.InstallerService", 
		//						    WellKnownObjectMode.Singleton);

		notify = (IInstallerServiceNotify) Activator.GetObject (typeof (IInstallerServiceNotify),
									"tcp://localhost:9414/MBuild.InstallerServiceNotify");

		singleton = new MBInstallClient ();
		notify.NotifyInstallerService (singleton);

		while (keep_going)
			System.Threading.Thread.Sleep (100);

		return 0;
	}
}
