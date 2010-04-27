using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Mono.Unix;

namespace Xnb
{
	using Protocol.Xnb;
	using Protocol.XProto;

	[StructLayout (LayoutKind.Explicit, Pack=1, Size=4)]
		public struct Request
		{
			[FieldOffset(0)]
				public byte Opcode;
			//[FieldOffset(1)]
			//public byte Data;
			[FieldOffset(2)]
				public ushort Length;
		}

	[StructLayout (LayoutKind.Explicit, Pack=1, Size=4)]
		public struct ExtensionRequest
		{
			[FieldOffset(0)]
				public byte MajorOpcode;
			[FieldOffset(1)]
				public byte MinorOpcode;
			[FieldOffset(2)]
				public ushort Length;
		}

	public abstract class Extension
	{
		protected Extension () {}

		protected Connection c;
		public Connection Connection
		{
			get {
				return c;
			}
		}

		public abstract string XName {get;}

		public byte GlobalId = 0;

		public void Init (Connection connection)
		{
			c = connection;

			//no extension
			if (XName == "")
				return;

			XProto xp = new XProto ();
			xp.Init (c);

			//QueryExtensionReply rep = xp.QueryExtension ((ushort)XName.Length, XName);
			Cookie<QueryExtensionReply> repCookie = xp.QueryExtension ((ushort)XName.Length, XName);
			//QueryExtensionReply rep = new QueryExtensionReply ();
			//repCookie.ReadReply (rep);

			QueryExtensionReply rep = repCookie;
			//req.Length = 2 + (7+1)/4; //4 + 10;
			
			GlobalId = rep.MajorOpcode;
			
			Trace.WriteLine ("Extension present: " + rep.Present + ", major opcode " + rep.MajorOpcode);
			//Trace.WriteLine (rep.FirstEvent);
			//Trace.WriteLine (rep.FirstError);
		}
	}

	[StructLayout (LayoutKind.Explicit, Pack=1, CharSet=CharSet.Ansi)]
	public struct Id
	{
		[FieldOffset (0)]
		private uint Value;

		public Id (uint value)
		{
			this.Value = value;
		}

		public static implicit operator uint (Id x)
		{
			return x.Value;
		}
	}
}
