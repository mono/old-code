using System;

using Mono.Build;

public class BuildLoggerProxy : MarshalByRefObject, IBuildLogger {

	// construction

	public BuildLoggerProxy (IBuildLogger log) {
		real = log;
	}

	IBuildLogger real;

	// IWarningLogger
	
	public void PushLocation (string loc) {
		real.PushLocation (loc);
	}
	
	public void PopLocation () {
		real.PopLocation ();
	}
	
	public void Warning (int category, string text, string detail) {
		real.Warning (category, text, detail);
	}
	
	public void Error (int category, string text, string detail) {
		real.Error (category, text, detail);
	}

	public string Location { get { return real.Location; } }

	public int NumErrors { get { return real.NumErrors; } }

	// IBuildLogger
	
	public void Log (string category, string text, object extra) {
		real.Log (category, text, extra);
	}
	
	public void Log (string category, string text) {
		real.Log (category, text);
	}
}
