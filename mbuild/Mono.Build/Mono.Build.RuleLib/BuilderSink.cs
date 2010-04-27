//
// BuilderSink.cs -- a sink that concatenates all its lines together
//


using System;
using System.Text;

namespace Mono.Build.RuleLib {

	public class BuilderSink : IMiniStreamSink {
		protected StringBuilder sb;

		public BuilderSink () {
			sb = new StringBuilder ();
		}

		public override string ToString () {
			return sb.ToString ();
		}

		// ministream!

		public void SendLine (string line) {
			sb.Append (line);
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
