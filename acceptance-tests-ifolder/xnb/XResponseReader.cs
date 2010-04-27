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
	public class XResponseReader : XReader
	{
		//public XResponseReader (Stream stream) : base (stream)
		public XResponseReader (NetworkStream stream) : base (stream)
		{
			cookies.Add (null);
		}

		public unsafe void ReadSetupResponse ()
		{
			//FIXME: seriously flawed
			ms = new MemoryStream (2048);
			byte[] bs = ms.GetBuffer ();
			fixed (byte* bp = bs) {
				Trace.WriteLine ("try read");
				ReadBlocks (2);
				Trace.WriteLine ("try read worked");

				Protocol.Xnb.ConnSetupGenericRepData* rs = (Protocol.Xnb.ConnSetupGenericRepData*)bp;

				switch (rs->Status) {
					//case SetupStatus.Failure:
					case 0:
						Trace.WriteLine ("Failure");
						Read ((Protocol.Xnb.ConnSetupFailedRepData*)bp);
						break;
					case 1:
						Trace.WriteLine ("Success");
						Read ((Protocol.Xnb.ConnSetupSuccessRepData*)bp);
						break;
					//case SetupStatus.AuthenticationFailure:
					case 2:
						Trace.WriteLine ("Authentication failure");
						break;
				}
			}
		}

		public unsafe void Read (Protocol.Xnb.ConnSetupFailedRepData* e)
		{
			//ReadBlocks (1);
			Trace.WriteLine ("Failure reason len " + e->ReasonLen);
			//ReadBlocks ((int)e->Length);

			Protocol.Xnb.ConnSetupFailedRep frep = new Protocol.Xnb.ConnSetupFailedRep ();
			frep.Read ((IntPtr)e);
			Trace.WriteLine ("Reason: " + frep.Reason);

		}

		public unsafe void Read (Protocol.Xnb.ConnSetupSuccessRepData* s)
		{
			ReadBlocks ((int)s->Length);

			Protocol.Xnb.ConnSetupSuccessRep srep = new Protocol.Xnb.ConnSetupSuccessRep ();
			srep.Read ((IntPtr)s);
			succ = srep;
		}

		public Protocol.Xnb.ConnSetupSuccessRep succ = null;

		public enum SetupStatus
		{
			Failure,
				Success,
				AuthenticationFailure,
		}

		public unsafe override void ReadMessage ()
		{
			//FIXME: seriously flawed
			ms = new MemoryStream (1024);
			byte[] bs = ms.GetBuffer ();

			//TODO: stream ptr might move during reads; temp solution only
			fixed (byte* bp = bs) {
				ReadBlocks (8);

				Response* rs = (Response*)bp;

				switch (rs->ResponseType) {
					case 0: //error
						Read ((Error*)bp);
						break;

					case 1: //reply
						Read ((Reply*)bp);
						break;

					default: //event
						Trace.WriteLine ("Event " + rs->ResponseType);
						break;
				}
			}
		}
		
		public unsafe void Read (Error* e)
		{
			Trace.WriteLine ("Error " + e->Code);
			
			if (cookies.Count > e->Sequence)
				cookies[e->Sequence].Errors.Add (*e);
		}

		public unsafe void Read (Reply* r)
		{
			Trace.WriteLine ("Reply");
			//Trace.WriteLine ("Length: " + r->Length);
			
			ReadBlocks ((int)r->Length);

			//Trace.WriteLine ("For req seqNo: " + r->Sequence);
			
			if (cookies.Count > r->Sequence) {
				//cookies[r->Sequence].reply = *r;
				cookies[r->Sequence].reply = (IntPtr)r;
				cookies[r->Sequence].Done = true;
				/*
			QueryExtensionRep* qerp = (QueryExtensionRep*)r;
			Trace.WriteLine ("Present: " + qerp->Present);
			Trace.WriteLine ("MajorOpcode: " + qerp->MajorOpcode);
			*/
			}

			/*
			GetWindowAttributesReply gwar;
			gwar = *(GetWindowAttributesReply*)r;

			Trace.WriteLine ("vis: " + gwar.Visual);
			Trace.WriteLine ("bs: " + gwar.BackingStore);
			*/

			/*
			QueryTreeReply* qtrp = (QueryTreeReply*)r;
			Trace.WriteLine ("Root: " + qtrp->Root);
			Trace.WriteLine ("Parent: " + qtrp->Parent);
			//Trace.WriteLine ("Parent: " + qtrp->ChildrenLen);
			Trace.WriteLine ("Parent: " + qtrp->Children.Count);
			Trace.WriteLine ("Parent: " + qtrp->Children.Value);
			*/

			/*
			QueryExtensionRep* qerp = (QueryExtensionRep*)r;
			Trace.WriteLine ("Present: " + qerp->Present);
			Trace.WriteLine ("MajorOpcode: " + qerp->MajorOpcode);
			*/
		}

		/*
		public unsafe void Read (Event* e)
		{
		}
		*/


		uint seq = 1;
		public List<Cookie> cookies = new List<Cookie> ();

		public Cookie<T> GenerateCookie<T> () where T: class, IMessagePart, new()

		{
			Cookie<T> cookie = new Cookie<T> (this);
			T t = new T ();
			cookie.Reply = t;
			cookies.Add (cookie);

			seq++;

			return cookie;
		}

		/*
		public void WaitForMessage (uint seq)
		{
		}
		*/
	}

	[Reply (98)]
	[StructLayout (LayoutKind.Explicit, Pack=1, CharSet=CharSet.Ansi)]
	public struct @QueryExtensionRep
	{
		[FieldOffset (8)]
		public byte @Present;
		[FieldOffset (9)]
		public byte @MajorOpcode;
		[FieldOffset (10)]
		public byte @FirstEvent;
		[FieldOffset (11)]
		public byte @FirstError;
	}


	public class Cookie<T> : Cookie where T: class, IMessagePart//, new()
	//public class Cookie<T> : Cookie where T: IMessagePart, new ()
	//public class Cookie<T> : Cookie where T: IMessagePart
	//public class Cookie<T> : Cookie
	{
		XReader xr;

		public Cookie (XReader xr) : base (0)
		{
			this.xr = xr;
		}

		//public T rep;

		public void ReadReply (T r)
		{
				while (!Done)
					xr.ReadMessage ();

				r.Read (reply);
		}

		protected T rep;

		public unsafe T Reply
		{
			get {
				//xr.WaitForReply (c, this.seq, out err[]);
				//TODO: only ever one error per request?
				//while (!Done && Errors.Count == 0)
				while (!Done)
					xr.ReadMessage ();

				//FIXME: use MessageData
				//return (T)Marshal.PtrToStructure(reply, typeof(T));
				//Marshal.PtrToStructure(reply, rep);
				//T r = new T ();
				rep.Read (reply);
				//return r;

				/*
				bool first = true;
				foreach (IOVector v in rep) {
					if (first) {
						first = false;
						continue;
					}
					Trace.WriteLine ("vec " + v.Start + " " + v.Length);
					
				}
				*/

				//TODO:
				//xr.Receive (rep);

				return rep;
			} set {
				rep = value;
			}
		}
		
		public static implicit operator T (Cookie<T> cookie) 
		{
			return cookie.Reply;
		}
	}

	public class Cookie
	{
		public Cookie (uint seq)
		{
			this.Sequence = seq;
		}
		
		public readonly List<Error> Errors = new List<Error> (0);

		public uint Sequence;

		public IntPtr reply;

		public bool Done = false;
	}
}
