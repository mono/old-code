using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

using System.Runtime.InteropServices;

using Mono.Unix;
using Mono.Unix.Native;

namespace Xnb
{
	using Protocol.Xnb;

	public class Connection
	{
		public const int X_PROTOCOL = 11;
		public const int X_PROTOCOL_REVISION = 0;
		const int X_TCP_PORT = 6000;
		//TODO: get this from environment? windows?
		const string BASE = "/tmp/.X11-unix/X";

		public Connection (int display) : this (null, display) {}

		public Connection (string host, int display)
		{
			string hst;
			int dpy, scr;

			Trace.Listeners.Add (new TextWriterTraceListener (Console.Error));

			ParseDisplay (null, out hst, out dpy, out scr);
			sock = Open (hst, dpy);

			//Auth.Xauth auth = Auth.Xau.GetAuthByAddr (sock.RemoteEndPoint.Serialize (), "0");
			Auth.Xauth auth = Auth.Xau.GetAuthByHack (hst, dpy);
			ConnectToSocket (sock, auth);
		}

		public XResponseReader xrr;
		public XWriter xw;

		//TODO: take into account given values (ref), unit tests
		bool ParseDisplay (string name, out string host, out int display, out int screen)
		{
			host = null;
			display = 0;
			screen = 0;

			if (name == null)
				name = System.Environment.GetEnvironmentVariable ("DISPLAY");

			if (name == null)
				return false;

			string[] parts = name.Split (':');

			if (parts.Length > 2)
				return false;

			if (parts.Length < 1)
				return false;

			if (parts[0] != "")
				host = parts[0];

			if (parts.Length < 2)
				return true;

			parts = parts[1].Split ('.');

			if (parts.Length > 2)
				return false;

			if (parts.Length < 1)
				return true;

			if (parts[0] != "")
				display = Int32.Parse (parts[0]);

			if (parts.Length < 2)
				return true;

			if (parts[1] != "")
				screen = Int32.Parse (parts[1]);

			return true;
		}

		Socket Open (string host, int display)
		{
			if (host != null) {
				int port = X_TCP_PORT + display;
				return OpenTCP (host, port);
			} else {
				string path = BASE + display;
				return OpenUnix (path);
			}
		}

		NetworkStream stream = null;

		TcpClient tcpClient = null;
		Socket OpenTCP (string host, int port)
		{
			TcpClient tc = new TcpClient (host, port);
			tcpClient = tc;
			//Socket client = new Socket (AddressFamily.InterNetwork, SocketType.Stream, 0);

			return tc.Client;
		}

		Socket sock;
		UnixEndPoint remoteEndPoint;

		Socket OpenUnix (string path)
		{
			Socket client = new Socket (AddressFamily.Unix, SocketType.Stream, 0);
			//UnixEndPoint remoteEndPoint = new UnixEndPoint (path);
			remoteEndPoint = new UnixEndPoint (path);
			client.Connect (remoteEndPoint);
		
			return client;
		}

		public void ConnectToSocket (Socket sock, Auth.Xauth auth_info)
		{
			sock.Blocking = false;

			xrr = new XResponseReader (stream);
			xw = new XWriter (stream);
			xrr.sock = sock;
			xw.sock = sock;
			xrr.fd = (int)sock.Handle;
			xw.fd = (int)sock.Handle;

			WriteSetup (auth_info);
			ReadSetup ();
		}

		public int GenerateId ()
		{
			return 0;
		}

		ConnSetupSuccessRep setup = null;

		public ConnSetupSuccessRep Setup
		{
			get {
				if (setup != null)
					return setup;

				return null;

				/*
				int sz = 153*4;

				for (int i = 0 ; i != sz ; i++) {
					byte b = Marshal.ReadByte (ptr, i);
					Trace.WriteLine (i + " : " + b);
				}
				*/

				//setup = new ConnSetupSuccessRep ();
				//setup.Read (ptr);

				return setup;
			}
		}

		private const int FD_CLOEXEC = 1;
		void SetFDFlags (int fd)
		{
			/*
			OpenFlags flags = (OpenFlags) Syscall.fcntl (fd, FcntlCommand.F_GETFL, (long)0);
			flags |= OpenFlags.O_NONBLOCK;
			//flags |= OpenFlags.O_SYNC;
			Syscall.fcntl (fd, FcntlCommand.F_SETFL, (long)flags);
			//Syscall.fcntl (fd, FcntlCommand.F_SETFD, FD_CLOEXEC);
			
			OpenFlags flags = (OpenFlags) Syscall.fcntl (fd, FcntlCommand.F_GETFD, (long)0);
			*/
			Syscall.fcntl (fd, FcntlCommand.F_SETFD, FD_CLOEXEC); //FD_CLOEXEC = 1
		}

		void WriteSetup (Auth.Xauth auth_info)
		{
			ConnSetupReq o = new ConnSetupReq ();

			/* B = 0x42 = MSB first, l = 0x6c = LSB first */
			//MSB
			//o.ByteOrder = 0x42;
			//LSB
			//o.ByteOrder = 0x6c;
			o.ByteOrder = 0x6c;
			o.ProtocolMajorVersion = X_PROTOCOL;
			o.ProtocolMinorVersion = X_PROTOCOL_REVISION;

			if (auth_info != null) {
				o.AuthorizationProtocolNameLen = (ushort)auth_info.Name.Length;
				o.AuthorizationProtocolDataLen = (ushort)auth_info.Data.Length;

				o.AuthorizationProtocolName = auth_info.Name;
				o.AuthorizationProtocolData = auth_info.Data;
			}

			xw.Send (o, false);
		}

		void ReadSetup ()
		{
			//ConnSetupFailedRep frep = new ConnSetupFailedRep ();
			//setup = new ConnSetupSuccessRep ();
			xrr.ReadSetupResponse ();
			setup = xrr.succ;
		}
	}
}
