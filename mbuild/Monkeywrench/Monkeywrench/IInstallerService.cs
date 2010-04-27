// IInstallerService.cs -- interfaces for having
// a service to perform result installation over
// remoting

using Mono.Build;

namespace Monkeywrench {

    public interface IInstallerService {
	bool Install (Result installer, Result target, bool backwards, 
		      IBuildContext ctxt);
	void DoneInstalling ();
    }
    
    public interface IInstallerServiceNotify {
	void NotifyInstallerService (IInstallerService service);
    }

}
