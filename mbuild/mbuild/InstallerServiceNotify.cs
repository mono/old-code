using System;

using Mono.Build;
using Monkeywrench;

public class InstallerServiceNotify : MarshalByRefObject, IInstallerServiceNotify {

	// construction

	public InstallerServiceNotify () {
	}

	// api

	static IInstallerService iis = null;

	public void NotifyInstallerService (IInstallerService arg) {
		iis = arg;
	}

	static public IInstallerService Service { get { return iis; } }
}
