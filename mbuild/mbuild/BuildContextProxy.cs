using System;

using Mono.Build;

public class BuildContextProxy : MarshalByRefObject, IBuildContext {

	// construction

	public BuildContextProxy (IBuildContext real) {
		this.real = real;
	}

	// api

	IBuildContext real;

	public IBuildContext RealContext {
		get { return real; }

		set { real = value; }
	}

	public MBDirectory WorkingDirectory { get { return real.WorkingDirectory; } }

	public MBDirectory SourceDirectory { get { return real.SourceDirectory; } }

	public string PathTo (MBDirectory dir) {
		return real.PathTo (dir);
	}

	public string DistPath (MBDirectory dir) {
		return real.DistPath (dir);
	}

	public IBuildLogger Logger { get { return new BuildLoggerProxy (real.Logger); } }
}
