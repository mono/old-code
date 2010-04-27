//
// TeeSink.cs -- a mini stream sink that forks to several others
//

using System;
using System.Collections;

namespace Mono.Build.RuleLib {

	public class TeeSink : IMiniStreamSink {
		protected IMiniStreamSink[] sinks;

		public TeeSink (IMiniStreamSink[] sinks) {
			this.sinks = new IMiniStreamSink[sinks.Length];
			sinks.CopyTo (this.sinks, 0);
		}

		public TeeSink (IMiniStreamSink sink) {
			this.sinks = new IMiniStreamSink[1];
			this.sinks[0] = sink;
		}

		public TeeSink () {
			this.sinks = new IMiniStreamSink[0];
		}

		public void AddSink (IMiniStreamSink sink) {
			if (sink == null)
				throw new ArgumentNullException ();

			IMiniStreamSink[] newsinks = new IMiniStreamSink [sinks.Length + 1];
			sinks.CopyTo (newsinks, 0);
			newsinks[sinks.Length] = sink;
			
			sinks = newsinks;
		}

		public IEnumerable Sinks { get { return sinks; } }

		// ministream!

		public void SendLine (string line) {
			for (int i = 0; i < sinks.Length; i++)
				sinks[i].SendLine (line);
		}
		
		public void StreamDone () {
			for (int i = 0; i < sinks.Length; i++)
				sinks[i].StreamDone ();
		}

		public bool HasNextSink { get { return true; } }

		public IMiniStreamSink NextSink { 
			get { 
				if (sinks.Length == 0)
					return null;

				return sinks[0];
			}

			set {
				if (value == null)
					throw new ArgumentNullException ();

				if (sinks.Length == 0)
					AddSink (value);
				else
					sinks[0] = value;
			}
		}
	}
}
