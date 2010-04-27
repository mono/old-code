//
// IWarningLogger.cs -- system for logging warnings and errors
//

using System;

namespace Mono.Build {

	// large category meanings:
	// * 1000-1999 : internal problem
	// * 2000-2999 : build specification problem
	// * 3000-3999 : build execution problem
	// * 4000-4999 : bundle implementation problem
	// 
	// See doc/error-codes.txt for assigned numbers.

	public interface IWarningLogger {
		void PushLocation (string loc);
		void PopLocation ();
		void Warning (int category, string text, string detail);
		void Error (int category, string text, string detail);

		string Location { get; }

		int NumErrors { get; } // For seeing if errors have been reported.
	}
}
