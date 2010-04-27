//
// SedSink.cs -- a mini stream sink that transforms
// its input with regular expression replacements.
//

using System;
using System.Collections;
using System.Text.RegularExpressions;

namespace Mono.Build.RuleLib {

	public class SedSink : IMiniStreamSink {
		ArrayList regexes;
		ArrayList repls;

		IMiniStreamSink dest;

		protected SedSink (IMiniStreamSink dest) {
			this.dest = dest;
			this.regexes = new ArrayList ();
			this.repls = new ArrayList ();
		}

		public void Add (string regex, string repl) {
			regexes.Add (new Regex (regex));
			repls.Add (repl);
		}
			
		public void Add (string[] regex, string[] repl) {
			Regex[] rs = new Regex[regex.Length];
			for (int i = 0; i < regex.Length; i++)
				rs[i] = new Regex (regex[i]);
			regexes.AddRange (rs);

			repls.AddRange (repl);
		}
			
		// ministream!

		public void SendLine (string line) {
			if (dest == null)
				throw new Exception ("SedStream got line without dest being set.");

			for (int i = 0; i < regexes.Count; i++) {
				Regex rex = (Regex) regexes[i];
				string repl = (string) repls[i];

				line = rex.Replace (line, repl);
			}

			dest.SendLine (line);
		}
		
		public void StreamDone () {
			if (dest == null)
				throw new Exception ("SedStream got done without dest being set.");

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
