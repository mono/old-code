using System;
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
	public class XRequestReader : XReader
	{
		public unsafe void ReadMessage ()
		{
			byte[] bs = ms.GetBuffer ();

			//Trace.WriteLine (ms.Length);

			//TODO: stream ptr might move during reads; temp solution only
			fixed (byte* bp = bs) {
				ReadBlocks (1);

				Request* rq = (Request*)bp;
				Trace.WriteLine (rq->Opcode);

				Trace.WriteLine ("Length: " + rq->Length);
				ReadBlocks ((int)rq->Length);

				if (rq->Opcode >= 128) {
					Trace.WriteLine ("Extension Request");
				}

				switch (rq->Opcode) {
					case 1: //CreateWindow
						Trace.WriteLine ("CreateWindow");
						break;
					case 15: //QueryTree
						Trace.WriteLine ("QueryTree");
						break;
					case 98: //QueryExtension
						Trace.WriteLine ("QueryExtension");
						break;
				}
			}
		}
	}
}
