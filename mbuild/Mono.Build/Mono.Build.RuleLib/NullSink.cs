//
// NullSink.cs -- a void mini stream sink
//

using System;

namespace Mono.Build.RuleLib {

	public class NullSink : IMiniStreamSink {
		public NullSink () {}

		// ministream!

		public void SendLine (string line) {}
		
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
