using System;
using System.Text;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

using System.Net;
using System.Net.Sockets;

using Mono.Unix;
using Mono.Unix.Native;

namespace Xnb.Auth
{
	[StructLayout (LayoutKind.Sequential, CharSet=CharSet.Ansi)]
		public class Xauth
		{
			//public ushort Family;
			public AddressFamily Family;
			//public byte[] Address;
			//public SocketAddress Address;

			public string Address;
			
			public string Number;

			public string Name;
			public byte[] Data;
		}

	//System.Net.IPAddress.NetworkToHostOrder ();

	public class Xau : IEnumerable<Xauth>, IEnumerable
	{
		public Xau (string fileName)
		{
			if (File.Exists (fileName))
				this.fileName = fileName;
		}

		public Xau () : this (Xau.GetFileName ()) {}

		public Xauth GetAuthByAddr (AddressFamily family, byte[] address, string number)
		{
			return null;
		}

		//TODO: remove this hack
		public static Xauth GetAuthByHack (string host, int display)
		{
			Console.WriteLine ("GetAuthByHack host: " + host + ", dpy: " + display);
			if (host == null || host == String.Empty)
				host = "localhost.localdomain";

			Xau xau = new Xau ();

			foreach (Xauth auth in xau) {
				Console.WriteLine (auth.Address);
				if (host != auth.Address)
					continue;

				if (display.ToString () != auth.Number)
					continue;

				return auth;
			}

			//no auth found
			return null;
		}

		public static Xauth GetAuthByAddr (SocketAddress address, string number)
		{
			Console.WriteLine ("Looking for: " + address);
			Xau xau = new Xau ();

			foreach (Xauth auth in xau) {
				Console.WriteLine (auth.Address);
				/*
				if (address != auth.Address)
					continue;

				if (number != auth.Number)
					continue;
					*/

				Console.WriteLine (address.Family);
				Console.WriteLine (address.Size);
			}

			return null;
		}

		const string XAUTHFILE = ".Xauthority";

		public static string GetFileName ()
		{
			string name = null;

			name = System.Environment.GetEnvironmentVariable ("XAUTHORITY");

			if (name != null && name != String.Empty)
				return name;

			string home = System.Environment.GetEnvironmentVariable ("HOME");

			if (home != null && home != String.Empty)
				return Path.Combine (home, XAUTHFILE);

			//TODO: Windows xauth

			return null;
		}

		BinaryReader br;

		/*
			 The .Xauthority file is a binary file consisting of a sequence of entries
			 in the following format:

			 2 bytes   Family value (second byte is as in protocol HOST)
			 2 bytes   address length (always MSB first)
			 A bytes   host address (as in protocol HOST)
			 2 bytes   display "number" length (always MSB first)
			 S bytes   display "number" string
			 2 bytes   name length (always MSB first)
			 N bytes   authorization name string
			 2 bytes   data length (always MSB first)
			 D bytes   authorization data string
			 */

		string fileName = null;

		Xauth ReadAuth ()
		{
			Xauth xa = null;

			//Format derived from xauth(1) source and comments
			try {
				xa = new Xauth ();
				//xa.Family = (ushort)ReadInt16 ();
				//xa.Address = ReadBlob ();

				/*
				ushort family = (ushort)br.ReadInt16 ();
				byte[] address = ReadBlob ();
				//xa.Address = new SocketAddress ((AddressFamily)family, address.Length + 2);
				//SocketAddress size includes 2 bytes for AddressFamily
				//and for Unix, an additional 2 bytes for length?
				//Console.WriteLine (family);
				//Console.WriteLine ((ushort)AddressFamily.Unix);
				Console.WriteLine ("addr len: " + address.Length);
				xa.Address = new SocketAddress (AddressFamily.Unix, 2 + address.Length);
				for (int i = 0 ; i != address.Length ; i++)
					xa.Address[i+2] = address[i];

				UnixEndPoint endpoint = new UnixEndPoint (null);
				EndPoint ep = endpoint.Create (xa.Address);
				Console.WriteLine ("ep: " + ep);
				*/

				//xa.Family = (ushort)br.ReadInt16 ();
				xa.Family = (AddressFamily)br.ReadInt16 ();
				xa.Address = ReadString ();

				/*
				SocketAddress sa2 = ep.Serialize ();
				EndPoint ep2 = endpoint.Create (sa2);
				Console.WriteLine ("ep2: " + ep2);
				*/


				xa.Number = ReadString ();

				xa.Name = ReadString ();
				xa.Data = ReadBlob ();
			} catch {
				xa = null;
			}

			return xa;
		}

		IEnumerator IEnumerable.GetEnumerator () { return GetEnumerator (); }

		//TODO: fix use of yield
		public IEnumerator<Xauth> GetEnumerator ()
		{
			br = new BinaryReader (File.Open (fileName, FileMode.Open));

			Xauth xa;

			while ((xa = ReadAuth ()) != null)
				yield return xa;

			br.Close ();
		}

		//TODO: unsigned
		//TODO: writing
		short ReadInt16 ()
		{
			return (short)((short)(br.ReadByte () << 8) + br.ReadByte ());
		}

		byte[] ReadBlob ()
		{
			short len = ReadInt16 ();
			byte[] bytes = br.ReadBytes (len);
			return bytes;
		}

		char[] ReadTextBlob ()
		{
			short len = ReadInt16 ();
			char[] chars = br.ReadChars (len);
			return chars;
		}

		string ReadString ()
		{
			return new string (ReadTextBlob ());
		}

		//From Mono.Security.Cryptography
		//Modified to output lowercase hex
		static public string ToHex (byte[] input) 
		{
			if (input == null)
				return null;

			StringBuilder sb = new StringBuilder (input.Length * 2);
			foreach (byte b in input) {
				sb.Append (b.ToString ("x2", CultureInfo.InvariantCulture));
			}
			return sb.ToString ();
		}

		//From Mono.Security.Cryptography
		static private byte FromHexChar (char c) 
		{
			if ((c >= 'a') && (c <= 'f'))
				return (byte) (c - 'a' + 10);
			if ((c >= 'A') && (c <= 'F'))
				return (byte) (c - 'A' + 10);
			if ((c >= '0') && (c <= '9'))
				return (byte) (c - '0');
			throw new ArgumentException ("Invalid hex char");
		}

		//From Mono.Security.Cryptography
		static public byte[] FromHex (string hex) 
		{
			if (hex == null)
				return null;
			if ((hex.Length & 0x1) == 0x1)
				throw new ArgumentException ("Length must be a multiple of 2");

			byte[] result = new byte [hex.Length >> 1];
			int n = 0;
			int i = 0;
			while (n < result.Length) {
				result [n] = (byte) (FromHexChar (hex [i++]) << 4);
				result [n++] += FromHexChar (hex [i++]);
			}
			return result;
		}
	}
}
