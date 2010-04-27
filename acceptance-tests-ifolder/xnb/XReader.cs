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
	public abstract class XReader
	{
		public int fd;
		public Socket sock = null;

		//protected Stream stream;
		protected NetworkStream stream;

		//public XReader (Stream stream)
		public XReader (NetworkStream stream)
		{
			this.stream = stream;
		}

		protected const int BLOCK_SIZE = 4;

		protected MemoryStream ms;

		public abstract void ReadMessage ();

		public void Receive (IMessagePart message)
		{
			Trace.WriteLine ("ERROR");

			List<IOVector> vecs = new List<IOVector> ();

			int length = 0;
			//vecs.Append (message);

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

			/*
				 unsafe {
				 Request* rq = (Request*)iovec[0].Start;
				 rq->Length = nBlocks;
				 }
				 */

			Trace.WriteLine ("ReadPoll: " + sock.Poll (-1, SelectMode.SelectRead));
			VectorIO.Read (fd, iovec);
		}

		public void ReadBlocks (int n)
		{
			if (n == 0)
				return;

			int len = n*BLOCK_SIZE;
			byte[] buf = new byte[len];

			//ms.Position += len;
			//byte[] buf = ms.GetBuffer ();

			//int read = stream.Read (buf, (int)ms.Length, len);

			bool ret = sock.Poll (-1, SelectMode.SelectRead);
			Trace.WriteLine ("ReadPoll: " + ret);

			int read = 0;
			//read = stream.Read (buf, 0, len);
			//read = sock.Receive (buf);
			read = sock.Receive (buf, SocketFlags.None);

			Trace.WriteLine ("Read " + read + " of " + buf.Length);

			/*
				 if (read != len)
				 Trace.WriteLine ("Error: Read " + read + " of " + len);
				 */
			ms.Write (buf, 0, len);
		}
	}
}
