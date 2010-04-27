using System;

namespace Xnb
{
	/*
	public class OpAttribute : Attribute
	{
		public OpAttribute (int opcode)
		{
		}
	}
	*/

	[AttributeUsage (AttributeTargets.Class | AttributeTargets.Struct)]
	public class RequestAttribute : Attribute
	{
		public RequestAttribute (int opcode)
		{
		}
	}

	[AttributeUsage (AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method)]
	public class ReplyAttribute : Attribute
	{
		public ReplyAttribute (Type reply)
		{
		}

		public ReplyAttribute (int opcode)
		{
		}
	}

	[AttributeUsage (AttributeTargets.Class | AttributeTargets.Struct)]
	public class EventAttribute : Attribute
	{
		public EventAttribute (int number)
		{
		}
	}

	[AttributeUsage (AttributeTargets.Class | AttributeTargets.Struct)]
	public class ErrorAttribute : Attribute
	{
		public ErrorAttribute (int number)
		{
		}
	}
}
