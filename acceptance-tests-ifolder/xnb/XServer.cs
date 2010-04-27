using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

using System.Runtime.InteropServices;

using Mono.Unix;
using Mono.Unix.Native;

using Mono.GetOptions;

namespace Xnb
{
	using Protocol.Xnb;

	public class XServerOptions : Options
	{
		public XServerOptions (string[] args) : base (args) {}

		[Option("disable access control restrictions", null, "ac")]
			public bool DisableAC = false;

		[Option("select authorization file", null, "auth")]
			public string AuthFile;
	}

	public class XServer
	{
		Socket listener;

		public void InitTCP ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Any, 60005);
			Socket listener = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		}

		public void InitUnix ()
		{
			//TODO: /tmp/.X1-lock
			//string lockFile = "/tmp/.X" + 1 + "-lock";

			string path = "/tmp/.X11-unix/X" + 1;

			UnixFileInfo ufi = new UnixFileInfo (path);

			UnixEndPoint ep = new UnixEndPoint (path);

			listener = new Socket (AddressFamily.Unix, SocketType.Stream, 0);
			
			//Bind creates the socket file right now
			listener.Bind (ep);
			//savedEP = listener.LocalEndPoint;
			
			//listen backlog 1 for now
			listener.Listen (1);

			Socket client = listener.Accept ();

			listener.Shutdown (SocketShutdown.Both);
			listener.Close ();
			ufi.Delete ();
		}
	}

	public class Display
	{
	}
}
