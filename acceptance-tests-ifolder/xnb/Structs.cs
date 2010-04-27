using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Xnb
{
	[StructLayout (LayoutKind.Explicit, Pack=1)]
		public struct Response
		{
			[FieldOffset(0)]
				public byte ResponseType;
			//pad[1]
			[FieldOffset(2)]
				public ushort Sequence;
		}

	[StructLayout (LayoutKind.Explicit, Pack=1)]
		public struct Reply
		{
			[FieldOffset(2)]
				public ushort Sequence;
			[FieldOffset(4)]
				public uint Length;
		}
	

	[StructLayout (LayoutKind.Explicit, Pack=1)]
		public struct Error
		{
			[FieldOffset(1)]
				public byte Code;
			[FieldOffset(2)]
				public ushort Sequence;
		}

	/*
	[StructLayout (LayoutKind.Explicit, Pack=1, Size=0)]
		public struct Event : Response
		{
		}
		*/

	/*
	[StructLayout (LayoutKind.Explicit, Pack=1, Size=4)]
		public struct Request
		{
			public Request (byte opcode, ushort length)
			{
				this.Opcode = opcode;
				this.Length = length;
			}

			[FieldOffset(0)]
				public byte Opcode;
			//[FieldOffset(1)]
			//public byte Data;
			[FieldOffset(2)]
				public ushort Length;
		}

	//TODO
	[StructLayout (LayoutKind.Explicit, Pack=1, Size=4)]
		public struct ExtensionRequest
		{
			[FieldOffset(0)]
				public byte Opcode;
			//[FieldOffset(1)]
			//public byte Data;
			[FieldOffset(2)]
				public ushort Length;
		}
		*/

		/*
	[StructLayout (LayoutKind.Explicit, Pack=1, Size=4)]
		public struct ExtensionRequest : Request
		{
			[FieldOffset(0)]
				public byte MajorOpcode;
			[FieldOffset(1)]
				public byte MinorOpcode;
		}
		*/
}
