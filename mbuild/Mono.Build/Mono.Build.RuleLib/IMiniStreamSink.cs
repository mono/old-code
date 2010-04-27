//
// IMiniStreamSink.cs -- a sink for our miniature
// text-line processing tools
//

using System;

namespace Mono.Build.RuleLib {
	public interface IMiniStreamSink {

		void SendLine (string line);
		void StreamDone ();

		// if HasNextSink == false, get_NextSink() = null

		bool HasNextSink { get; }
		IMiniStreamSink NextSink { get; set; }
	}
}
