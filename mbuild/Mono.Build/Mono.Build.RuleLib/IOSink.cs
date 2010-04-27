//
// IOSink.cs -- a mini stream sink that writes to a StreamWriter
//

using System;
using System.IO;

using Mono.Build;

namespace Mono.Build.RuleLib {

	public class IOSink : IMiniStreamSink {
		protected StreamWriter writer;

		public IOSink (StreamWriter writer) {
			if (writer == null)
				throw new ArgumentNullException ();

			this.writer = writer;
		}

		public IOSink (Stream stream) : this (new StreamWriter (stream)) {}

		public IOSink (MBFile file, IBuildContext ctxt) {
			if (file == null)
				throw new ArgumentNullException ();

			writer = new StreamWriter (file.OpenWrite (ctxt));
		}
		
		public StreamWriter Writer { 
			get {
				return writer;
			}
		}

		// static
	       
		public static void DrainStream (StreamReader reader, IMiniStreamSink sink) {
			if (reader == null)
				throw new ArgumentNullException ("reader");
			if (sink == null)
				throw new ArgumentNullException ("sink");

			string line;

			while ((line = reader.ReadLine ()) != null)
				sink.SendLine (line);

			sink.StreamDone ();
		}

		public static void DrainStream (Stream stream, IMiniStreamSink sink) {
			using (StreamReader reader = new StreamReader (stream)) {
				DrainStream (reader, sink);
			}
		}

		public static void DrainStream (MBFile file, IBuildContext ctxt, IMiniStreamSink sink) {
			DrainStream (file.OpenRead (ctxt), sink);
		}

		// ministream!

		public void SendLine (string line) {
			if (writer == null)
				throw new Exception ("SendLine after IOSink closed");

			writer.Write (line);
			writer.WriteLine ();
		}
		
		public void StreamDone () {
			writer.Close ();
			writer = null;
		}

		public bool HasNextSink { get { return false; } }

		public IMiniStreamSink NextSink { 
			get { return null; }

			set {
				throw new InvalidOperationException ();
			}
		}
	}
}
