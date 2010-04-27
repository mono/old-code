//
// DebugSink.cs -- a sink that writes to console (!)
//

using System;

namespace Mono.Build.RuleLib {

	public class DebugSink : IMiniStreamSink {
		protected string prefix;

		public DebugSink (string prefix) {
			this.prefix = prefix;
		}

		public DebugSink () : this (null) {}

		// ministream!

		public void SendLine (string line) {
			if (prefix == null)
				Console.WriteLine ("{0}", line);
			else
				Console.WriteLine ("{0}: {1}", prefix, line);
		}
		
		public void StreamDone () {}

		public bool HasNextSink { get { return false; } }

		public IMiniStreamSink NextSink { 
			get { return null; }

			set {
				throw new InvalidOperationException ();
			}
		}
	}
}
