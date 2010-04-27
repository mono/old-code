//
// IBuildLogger.cs -- system for logging build operations (as well 
// as warning operations.)
//

using System;

namespace Mono.Build {
	public interface IBuildLogger : IWarningLogger {
		void Log (string category, string text, object extra);
		void Log (string category, string text);
	}
}
