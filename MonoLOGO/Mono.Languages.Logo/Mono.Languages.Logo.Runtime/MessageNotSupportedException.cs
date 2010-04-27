namespace Mono.Languages.Logo.Runtime {
	using System;

	public class MessageNotSupportedException : Exception {
		string requested_message;

		public string RequestedMessage { get { return requested_message; } } 

		public MessageNotSupportedException (string message)
			: base ("I don't know how to " + message)
		{  
			requested_message = message;
		}
	}
}

