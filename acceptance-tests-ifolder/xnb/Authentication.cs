using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

using System.Net.Sockets;

using Mono.Unix;
using Mono.Unix.Native;

namespace Xnb
{
	using Auth;

	//System.Net.IPAddress.NetworkToHostOrder();
	public class Authenticator
	{
		public static void GetAuthInfoHack (int fd, out AuthInfo authinfo)
		{
			/*
				 string fname = UnixMarshal.PtrToStringUnix (XauFileName ());
				 Trace.WriteLine ("auth fname: " + fname);
				 */

			Xau xau = new Xau ();
			//string fname = xau.GetFileName ();

			//foreach (Xauth xauth in xau.ReadAuths (fname))
			foreach (Xauth xauth in xau) {
				Trace.WriteLine (xauth.Name);
			}

			/*
				 UnixFileInfo ufi = new UnixFileInfo (fname);

				 UnixStream us = ufi.OpenRead ();

				 Trace.WriteLine ("a: " + us.Handle);
				 IntPtr auth = XauReadAuth (us.Handle);
				 Trace.WriteLine ("auth: " + auth);

				 us.Close ();
				 */

			IntPtr authPtr = XauGetAuthByAddr ((ushort)AddressFamily.Unix, (ushort) 5, "rover", 1, "1");
			Trace.WriteLine ("authPtr: " + authPtr);

			authinfo = new AuthInfo ();
		}

		//TODO
		public static AuthInfo GetAuthInfo (Socket sock)
		{
			return new AuthInfo ();
		}

		public AuthInfo GetAuthInfo (int fd)
		{
			string sockName = "";
			IntPtr authPtr = GetAuthPtr (sockName);

			//return null;
			return new AuthInfo ();
		}

		string[] authnames = {"MIT-MAGIC-COOKIE-1"};

		[DllImport ("Xau")]
			//[return: MarshalAs (UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof (Mono.Unix.Native.FileNameMarshaler))]
			//static extern string XauFileName ();
			static extern IntPtr XauFileName ();
		//[DllImport ("X11")]
		[DllImport ("Xau")]
			static extern IntPtr XauReadAuth (int fd);
		//static extern IntPtr XauReadAuth (int fd);

		[DllImport ("Xau")]
			//Xauth *XauGetAuthByAddr (unsigned short family, unsigned short address_length, char *address, unsigned short number_length, char *number);
			static extern IntPtr XauGetAuthByAddr (ushort family, ushort address_length, string address, ushort number_length, string number);

		[DllImport ("Xau")]
			//Xauth *XauGetBestAuthByAddr (unsigned short family, unsigned short address_length, char *address, unsigned short number_length, char *number, int types_length, char **types, int *type_lengths);
			static extern IntPtr XauGetBestAuthByAddr (ushort family, ushort address_length, string address, ushort number_length, string number, int types_length, string[] types, int[] type_lengths);

		public IntPtr GetAuthPtr (string sockName)
		{
			int[] authnamelens = new int[authnames.Length];

			for (int i = 0 ; i != authnames.Length ; i++)
				authnamelens[i] = authnames[i].Length;

			IntPtr authPtr = XauGetBestAuthByAddr (0, 0, "", 0, "", authnames.Length, authnames, authnamelens);

			return authPtr;
		}
	}

	//TODO: maybe this is an IMessagePart?
	[StructLayout (LayoutKind.Sequential, CharSet=CharSet.Ansi)]
		public struct AuthInfo
		{
			public int NameLen;
			public string Name;
			public int DataLen;
			//public byte[] Data;
			public IntPtr Data;
		}
}

