using System;
using System.Diagnostics;

namespace RemotingObject
{

	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class Class1 : MarshalByRefObject
	{
		public Class1()
		{
			Trace.WriteLine("Create Class1()");
			//System.Diagnostics.Trace.Listeners.Add(new RemotingTraceListener());
		}

		public string HelloWorld()
		{
			Trace.WriteLine("Returning Hello World");
			return "Hello World1";
		}
	}


	/// <summary>
	/// Summary description for TraceListener.
	/// </summary>
	public class RemotingTraceListener : TraceListener
	{

		public RemotingTraceListener() {}


		public override void Write(object o)
		{

		}

		public override void WriteLine(object o)
		{
		}

		public override void Write(string s)
		{
		}

		public override void WriteLine(string s)
		{
			Console.WriteLine("List: "+s);
		}

	}

}
