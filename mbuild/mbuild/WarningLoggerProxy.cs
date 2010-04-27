using System;

using Mono.Build;

public class WarningLoggerProxy : MarshalByRefObject, IWarningLogger {

	// construction

	public WarningLoggerProxy () {
		if (singleton != null)
			Console.Error.WriteLine ("huh? making another blp?");

		singleton = this;
	}

	static WarningLoggerProxy singleton = null;

	public static WarningLoggerProxy Singleton { get { return singleton; } }

	// api

	static IWarningLogger real;

	public static IWarningLogger RealLogger {
		get { return real; }

		set { real = value; }
	}

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
}
