//
// Code to get the fingerprint for a generic object
//

using System;
using System.Collections;

namespace Mono.Build {

	public class GenericFingerprints {
		private GenericFingerprints () { return; }

		public static Fingerprint Constant (byte[] val) {
			if (val.Length * 8 > PreferredHash.Size)
					throw new ArgumentException (String.Format ("Constant fingerprint data is larger ({0}) than the size of the hash ({1})", val.Length * 8, PreferredHash.Size));

			byte[] data = new byte[PreferredHash.Size / 8];
			((IList) data).Clear ();
			val.CopyTo (data, 0);
			return Fingerprint.FromConstant (data);
		}

		public static Fingerprint Null {
			get {
				return Constant (new byte[] {0});
			}
		}

		public static Fingerprint GetFingerprint (object o, IBuildContext ctxt, Fingerprint cached) {
			if (o == null)
				return Null;

			if (o is IFingerprintable)
				return (o as IFingerprintable).GetFingerprint (ctxt, cached);

			if (o is Boolean)
				return Constant (BitConverter.GetBytes ((bool) o));

			if (o is Byte)
				return Constant (BitConverter.GetBytes ((byte) o));

			if (o is Char)
				return Constant (BitConverter.GetBytes ((char) o));

			if (o is Int16)
				return Constant (BitConverter.GetBytes ((short) o));

			if (o is Int32)
				return Constant (BitConverter.GetBytes ((int) o));

			if (o is Int64)
				return Constant (BitConverter.GetBytes ((long) o));

			if (o is UInt16)
				return Constant (BitConverter.GetBytes ((ushort) o));

			if (o is UInt32)
				return Constant (BitConverter.GetBytes ((uint) o));

			if (o is UInt64)
				return Constant (BitConverter.GetBytes ((ulong) o));

			if (o is Single)
				return Constant (BitConverter.GetBytes ((float) o));

			if (o is Double)
				return Constant (BitConverter.GetBytes ((double) o));

			if (o is String)
				return Fingerprint.FromText ((string) o);

			throw new Exception (String.Format ("Don't know how to get a fingerprint from object of type {0}",
							    o.GetType()));
		}
	}
}
