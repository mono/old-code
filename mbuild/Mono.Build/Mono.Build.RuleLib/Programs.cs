//
// Programs.cs -- class for finding external programs, etc
//

using System;
using System.IO;

using Mono.Build;

namespace Mono.Build.RuleLib {

	public class Programs {

		private Programs () { }

		protected static string exe_ext;
		protected static string[] pathbits;

		static Programs () {
			char path_delim;

			switch (Environment.OSVersion.Platform) {
			case PlatformID.Win32S:
			case PlatformID.Win32Windows:
			case PlatformID.Win32NT:
#if NET_1_1
			case PlatformID.WinCE:
#endif
				exe_ext = ".exe";
				path_delim = ';';
				break;
			default:
				exe_ext = "";
				path_delim = ':';
				break;
			}

			pathbits = Environment.GetEnvironmentVariable ("PATH").Split (path_delim);
		}

		public static string FindInPath (string basename) {
			string prog = basename + exe_ext;

			for (int i = 0; i < pathbits.Length; i++) {
				string fullpath = Path.Combine (pathbits[i], prog);

				// can't check for executable bit on Windows?
				if (File.Exists (fullpath))
					return fullpath;
			}

			return null;
		}

		public static string FindInPath (string[] basenames) {
			string result;

			for (int i = 0; i < basenames.Length; i++) {
				result = FindInPath (basenames[i]);
				if (result != null)
					return result;
			}

			return null;
		}

		// don't return the full path
		public static string GetFirstInPath (string[] basenames) {
			for (int i = 0; i < basenames.Length; i++) {
				if (FindInPath (basenames[i]) != null)
					return basenames[i];
			}

			return null;
		}
	}
}
