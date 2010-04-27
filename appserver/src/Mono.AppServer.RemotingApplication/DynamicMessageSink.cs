//
// Mono.AppServer.RemoteServerDynamicProperty
//
// Authors:
//   Brian Ritchie (brianlritchie@hotmail.com)
//
// Copyright (C) Brian Ritchie, 2003
//
using System;
using System.Collections;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Messaging;

namespace Mono.AppServer
{

	/// <summary>
	/// Dynamic properties and Dynamic Sinks proved an extensibility mechanism for
	/// monitoring the calls that are made on a remoting object. An object can
	/// register a dynamic property with context services. This way when remoting
	/// services make a call on the object, depending on the object and contexts,
	/// dynamic sink gets placed in path of the remoting calls.
	/// </summary>
	public class RemoteServerDynamicProperty : IDynamicProperty, IContributeDynamicSink
	{
		string _Name;
		ApplicationLog _Log;

		/// <summary>
		/// NKRemoteServerDynamicProperty constructor.
		/// </summary>
		public RemoteServerDynamicProperty (string appName, ApplicationLog log)
		{
			_Name=appName;
			_Log=log;
		}

		/// <summary>
		/// NKRemoteServerDynamicProperty constructor.
		/// </summary>
		/// <param name="obj"> Represents the remoted object</param>
		/// <param name="ctx"> Represtns the context of call.</param>
		public RemoteServerDynamicProperty (MarshalByRefObject obj, Context ctx)
		{
		}

		/// <summary>
		/// DynamicProperty name property.
		/// </summary>
		/// <value>
		/// DynamicProperty name that will be registered.
		/// </value>
		public String Name
		{
			get
			{
				return _Name+".NKRemoteServerDynamicProperty";
			}
		}

		/// <summary>
		/// Returns the message sink that will be notified of call start/finish events
		/// through the <code>IDynamicMessageSink</code> interface.
		/// </summary>
		public IDynamicMessageSink GetDynamicSink ()
		{
			//Console.WriteLine("System fetched dynamic sink");
			return new RemoteServerDynamicMessageSink (_Name,_Log);
		}
	}

	/// <summary>
	/// <code>IDynamicMessageSink</code> is implemented by message sinks provided
	/// by dynamically registered properties. These sinks are provided notifications
	/// of call-start and call-finish with flags indicating whether the call is
	/// currently on the client-side or server-side (this is useful for the context
	/// level sinks).
	/// </summary>
	public class RemoteServerDynamicMessageSink : IDynamicMessageSink
	{
		string _AppName;
		ApplicationLog _Log;

		public RemoteServerDynamicMessageSink(string AppName, ApplicationLog Log)
		{
			_AppName=AppName;
			_Log=Log;
		}
		/// <summary>
		/// Indicates that a call is starting. The booleans tell whether we are on
		/// the client side or the server side and whether the call is using
		/// <code>AsyncProcessMessage</code>.
		/// </summary>
		/// <param name="reqMsg">Request Message</param>
		/// <param name="bClientSide">True if client side</param>
		/// <param name="bAsync">True if async call</param>
		/// 
		public void ProcessMessageStart (IMessage reqMsg, bool bClientSide, bool bAsync)
		{
			//Console.WriteLine (_AppName+": ProcessMessageStart - ClientSide {0} : Async {1}",
			//	(bClientSide == true) ? "true" : "false", (bAsync == true) ? "true" : "false");
			string LogMsg=string.Format("MessageStart - ClientSide {0} : Async {1}",
				(bClientSide == true) ? "true" : "false", (bAsync == true) ? "true" : "false");
			
			//Console.WriteLine ("Call is starting");

			// Get the properties of the request message.
			IDictionary dt = reqMsg.Properties;
			foreach (object key in dt.Keys)
			{
				LogMsg+="<br>"+key.ToString()+": "+dt[key].ToString();
				//Console.WriteLine(key.ToString()+": "+dt[key].ToString());
			}

			_Log.WriteLine("System", LogMsg);

			//			if (bClientSide == true)
			//			{
			//				Console.WriteLine ("Call starting on client side");
			//			}
			//			else 
			//			{
			//				Console.WriteLine ("Call starting on server side");
			//			}
			//
			//			if (bAsync == true)
			//			{
			//				Console.WriteLine ("Call is Asynchronous");
			//			}
			//			else
			//			{
			//				Console.WriteLine ("Call is synchronous");
			//			}

			// Add a property to dictionary...
			//			dt.Add ("SourceID" + ((bClientSide == true) ? "Client" : "Server"), Guid.NewGuid ());


			//RemotingUtilities.DumpMessageObj (reqMsg);
		}

		/// <summary>
		/// Indicates that a call is returning. The booleans tell whether we are on the
		/// client side or the server side and whether the call is using
		/// <code>AsyncProcessMessage</code>.
		/// </summary>
		/// <param name="replyMsg">Reply Message</param>
		/// <param name="bClientSide">True if client side</param>
		/// <param name="bAsync">True if async call</param>
		/// 
		public void ProcessMessageFinish (IMessage replyMsg, bool bClientSide, bool bAsync)
		{
			//			IDictionary dt = replyMsg.Properties;
			//			dt.Add ("FinishSourceID" + ((bClientSide == true) ? "Client" : "Server"), Guid.NewGuid ());
			//Console.WriteLine ("ProcessMessageFinish - ClientSide {0} : Async {1}",
			//	(bClientSide == true) ? "true" : "false", (bAsync == true) ? "true" : "false");
			
			//RemotingUtilities.DumpMessageObj (replyMsg);
		}
	}

}
