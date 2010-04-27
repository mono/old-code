using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

using System.Runtime.InteropServices;

using Mono.Unix;
using Mono.Unix.Native;

namespace Xnb
{
	public class XcbConnection
	{
		public const string libXCB = "libXCB";
		public IntPtr Handle = IntPtr.Zero;

		//[DllImport (libXCB)]
		//	static extern IntPtr XCBConnect (string host, int display);

		[DllImport (libXCB)]
			static extern int XCBGetAuthInfo (int fd, out AuthInfo auth);

		[DllImport (libXCB)]
			static extern IntPtr XCBConnectToFD (int fd, ref AuthInfo auth);

		[DllImport (libXCB)]
			static extern int XCBOpen (string host, int display);

		[DllImport (libXCB)]
			static extern int _xcb_set_fd_flags (int fd);

		[DllImport (libXCB)]
			static extern int XCBGetFileDescriptor (IntPtr c);

		IntPtr XCBConnect (string displayname, int screenp)
		{
			int fd, display = 0;
			string host;

			AuthInfo auth;

			//ParseDisplay (displayname);
			//host = displayname;
			host = "";

			fd = XCBOpen (host, display);
			Trace.WriteLine ("fd: " + fd);

			//IntPtr auth;

			//XCBGetAuthInfo (fd, out auth);
			Authenticator.GetAuthInfoHack (fd, out auth);
			Trace.WriteLine ("authName: " + auth.Name);
			IntPtr c = XCBConnectToFD (fd, ref auth);

			//UnixStream us = new UnixStream (fd);
			//stream = us;

			return c;
		}

		int OpenXcb (string host, int display)
		{
			Handle = XCBConnect (host, display);
			int fd = XCBGetFileDescriptor (Handle);

			//SetFDFlags (fd);

			//UnixStream us = new UnixStream (fd);
			//stream = us;

			return fd;
		}

		int OpenUnixXcb (string fname)
		{
			int fd;
			fd = XCBOpen ("", 0);
			_xcb_set_fd_flags (fd);
			return fd;
		}
	}

		void SetFDFlags (int fd)
		{
			OpenFlags flags = (OpenFlags) Syscall.fcntl (fd, FcntlCommand.F_GETFL, (long)0);
			flags |= OpenFlags.O_NONBLOCK;
			//flags |= OpenFlags.O_SYNC;
			Syscall.fcntl (fd, FcntlCommand.F_SETFL, (long)flags);
			//Syscall.fcntl (fd, FcntlCommand.F_SETFD, FD_CLOEXEC);
		}

				[DllImport (libXCB)]
			static extern int XCBGenerateID (IntPtr c);
		
		public int GenerateId ()
		{
			return 0;
			//return XCBGenerateID (Handle);
		}

		[DllImport (libXCB)]
		//	[return: MarshalAs (UnmanagedType.CustomMarshaler, MarshalTypeRef=typeof (XMarshaler))]
		//	static extern ConnSetupSuccessRep XCBGetSetup (IntPtr c);
			//static extern ConnSetupSuccessRepData XCBGetSetup (IntPtr c);
			static extern IntPtr XCBGetSetup (IntPtr c);
		
		ConnSetupSuccessRep setup = null;

}
