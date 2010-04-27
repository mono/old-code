//
// MiniStream.cs -- a Stream feeding to an IMiniStreamSink
//

using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

namespace Mono.Build.RuleLib {

	public class MiniStream : Stream {
		IMiniStreamSink sink;

		StringBuilder sb;
		Decoder decoder;

		char[] buf;

		public MiniStream (IMiniStreamSink sink, Encoding encoding, int buf_size) {
			if (sink == null)
				throw new ArgumentNullException ();

			this.sink = sink;
			this.sb = new StringBuilder ();
			this.decoder = encoding.GetDecoder ();

			this.buf = new char[buf_size];
		}

		public MiniStream (IMiniStreamSink sink, Encoding encoding)
			: this (sink, encoding, 2048) {}

		public MiniStream (IMiniStreamSink sink) : this (sink, Encoding.Default) {}

		// too many members...

		public override bool CanRead { get { return false; } }

		public override bool CanSeek { get { return false; } }

		public override bool CanWrite { get { return true; } }

		// ?? proper handling ?
		public override long Length { get { return 0; } }

		// ?? proper handling ?
		public override long Position { 
			get { return 0; }
			set { throw new InvalidOperationException (); } 
		}

		public override void Flush () { }

		public override int Read ([In, Out] byte[] buffer, int offset, int count) {
			throw new InvalidOperationException ();
		}

		public override long Seek (long offset, SeekOrigin origin) {
			throw new InvalidOperationException ();
		}

		public override void SetLength (long value) {
			throw new InvalidOperationException ();
		}

		public override void Write (byte[] buffer, int offset, int count) {
			// maybe replace this check with a try/catch, since I 
			// assume we have to decode the buffer twice right now.

			int n = decoder.GetCharCount (buffer, offset, count);

			if (n > buf.Length)
				buf = new char[n];

			n = decoder.GetChars (buffer, offset, count, buf, 0);

			for (int i = 0; i < n; i++) {
				if (buf[i] == '\n') {
					sink.SendLine (sb.ToString ());
					sb.Length = 0;
				} else if (buf[i] == '\r') {
					// anything wrong with ignoring these?
					// ... didn't think so.
				} else {
					sb.Append (buf[i]);
				}
			}
		}

		public override void Close () {
			base.Close ();
			sink.StreamDone ();
			sink = null;
		}
	}
}
		
