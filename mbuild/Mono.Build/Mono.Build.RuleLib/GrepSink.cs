//
// GrepSink.cs -- a mini stream sink that selectively forwards
// its input, based on regular expression matching.
//

using System;
using System.Text.RegularExpressions;

namespace Mono.Build.RuleLib {

	public class GrepSink : IMiniStreamSink {
		protected Regex[] regexes;
		protected IMiniStreamSink dest;

		public GrepSink (IMiniStreamSink dest, string regex, RegexOptions options) {
			this.dest = dest;

			this.regexes = new Regex[1];
			this.regexes[0] = new Regex (regex, options);
		}

		public GrepSink (IMiniStreamSink dest, string[] regexes, RegexOptions options) {
			this.dest = dest;

			this.regexes = new Regex[regexes.Length];
			for (int i = 0; i < regexes.Length; i++)
				this.regexes[i] = new Regex (regexes[i], options);
		}

		public GrepSink (IMiniStreamSink dest, Regex[] regexes) {
			this.regexes = new Regex[regexes.Length];
			regexes.CopyTo (this.regexes, 0);
		}

		public GrepSink (IMiniStreamSink dest, string regex) : 
			this (dest, regex, RegexOptions.None) {}

		public GrepSink (IMiniStreamSink dest, string[] regexes) : 
			this (dest, regexes, RegexOptions.None) {}

		public GrepSink (string regex) : this (null, regex) {}

		public GrepSink (string[] regexes) : this (null, regexes) {}

		// Invert = false, AndMode = true: send only if all regexes match
		// Invert = false, AndMode = false: send if any regex matches
		// Invert = true, AndMode = true: send if any regex fails to match
		// Invert = true, AndMode = false: send only if all regexes fail to match

		public bool Invert = false;
		public bool AndMode = true;

		// ministream!

		public void SendLine (string line) {
			if (dest == null)
				throw new Exception ("GrepStream got line without dest being set.");

			if (!Invert) {
				if (AndMode) {
					for (int i = 0; i < regexes.Length; i++) {
						if (!regexes[i].IsMatch (line))
							return;
					}
					
					dest.SendLine (line);
				} else {
					for (int i = 0; i < regexes.Length; i++) {
						if (regexes[i].IsMatch (line)) {
							dest.SendLine (line);
							return;
						}
					}
				}
			} else {
				if (AndMode) {
					for (int i = 0; i < regexes.Length; i++) {
						if (!regexes[i].IsMatch (line)) {
							dest.SendLine (line);
							return;
						}
					}
				} else {
					for (int i = 0; i < regexes.Length; i++) {
						if (regexes[i].IsMatch (line))
							return;
					}

					dest.SendLine (line);
				}
			}
		}
		
		public void StreamDone () {
			if (dest == null)
				throw new Exception ("GrepStream got done without dest being set.");

			dest.StreamDone ();
		}

		public bool HasNextSink { get { return true; } }

		public IMiniStreamSink NextSink { 
			get { return dest; }

			set {
				if (value == null)
					throw new ArgumentNullException ();

				dest = value;
			}
		}
	}
}
