using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

using System.Threading;

using System.Runtime.InteropServices;

using Mono.Unix;
using Mono.Unix.Native;


namespace Xnb
{
	public class XWriter
	{
		public int fd;
		public Socket sock = null;

		//protected Stream stream;
		protected NetworkStream stream;

		//public XWriter (Stream stream)
		public XWriter (NetworkStream stream)
		{
			this.stream = stream;
		}

		public void Send (IMessagePart message)
		{
			Send (message, true);
		}

		public void Send (IMessagePart message, bool correctLength)
		{
			List<IOVector> vecs = new List<IOVector> ();

			int length = 0;
			//vecs.AddRange (message);

			foreach (IOVector v in message) {
				vecs.Add (v);
				length += v.Length;
			}

			IOVector[] iovec = vecs.ToArray ();

			Trace.WriteLine ("Vectors: " + iovec.Length + " Length: " + length);

			if (length % 4 != 0)
				Trace.WriteLine ("Error: % 4");

			ushort nBlocks = (ushort)(length / 4);
			Trace.WriteLine ("nBlocks: " + nBlocks);

			if (correctLength) {
				unsafe {
					Request* rq = (Request*)iovec[0].Base;
					rq->Length = nBlocks;
				}
			}

			Trace.WriteLine ("WritePoll: " + sock.Poll (-1, SelectMode.SelectWrite));
			VectorIO.Write (fd, iovec);
			//FallbackWrite (iovec);
			//queue.Enqueue (message);
		}

		public void Flush ()
		{
			if (queue.Count == 0)
				return;

			List<IOVector> vecs = new List<IOVector> ();
			//vecs.AddRange (queue);
			//vecs.Add (queue);

			foreach (IMessagePart m in queue) {
				vecs.AddRange (m);
			}

			FallbackWrite (vecs.ToArray ());

			queue.Clear ();
		}

		public Queue<IMessagePart> queue = new Queue<IMessagePart> ();
		//public Queue<IEnumerable<IOVector>> queue = new Queue<IEnumerable<IOVector>> ();

		int FallbackWrite (IOVector[] vector)
		{
			int len = 0;
			
			for (int i = 0 ; i != vector.Length ; i++) {
				Trace.WriteLine ("vec" + i + ": " + vector[i].Length);
			}

			foreach (IOVector vec in vector)
				len += vec.Length;

			byte[] bytes = new byte[len];

			int pos = 0;

			foreach (IOVector vec in vector) {
				Marshal.Copy (vec.Base, bytes, pos, vec.Length);
				pos += vec.Length;
			}
			
			Trace.WriteLine ("len: " + len + " " + pos);
			int sent = sock.Send (bytes, SocketFlags.None);
			
			//int sent = -1337;
			//stream.Write (bytes, 0, bytes.Length);
			//stream.Flush ();
			Trace.WriteLine ("sent: " + sent);

			return 0;
		}
	}
}
