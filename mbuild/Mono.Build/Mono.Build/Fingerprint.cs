//
// A small blob of data representing a target's state.
// Eg, MD5sum of a file or of the string value of a variable.
//
// I think we should say that all fingerprints have to be MD5 sums
// of something.
//

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace Mono.Build {

	[Serializable]
	public class Fingerprint : Result, IComparable {

		protected byte[] value;

		public byte[] Value { get { return value; } }

		protected Fingerprint () : base () {
			value = null;
		}

		protected Fingerprint (byte[] value) : base () {
			this.value = value;
		}

		// construction

		public static Fingerprint FromText (string s, Encoding enc)
		{
		    if (s == null)
			return GenericFingerprints.Null;

		    byte[] bytes = enc.GetBytes (s);

		    return new Fingerprint (PreferredHash.Algo.ComputeHash (bytes));
                }

                public static Fingerprint FromText (string str)
		{
		    return FromText (str, Encoding.UTF8);
		}
		    
		public static Fingerprint FromStream (Stream stream)
		{
		    return new Fingerprint (PreferredHash.Algo.ComputeHash (stream));
		}

		public static Fingerprint FromFile (string path)
		{
		    using (Stream s = File.OpenRead (path)) {
			return FromStream (s);
		    }
		}

		public static Fingerprint FromConstant (byte[] val)
		{
		    if (val.Length != PreferredHash.Size / 8)
			throw new ArgumentException ("input to constant fingerprint must be size of hash");

		    byte[] value = new byte[val.Length];
		    val.CopyTo (value, 0);

		    return new Fingerprint (value);
		}

		// comparison stuff. We need to be IComparable so that we can
		// sort ArgCollector arg fingerprints

		protected override bool ContentEquals (Result other)
		{
		    return CompareTo (other) == 0;
		}

		protected override int InternalHash () 
		{
			return value.GetHashCode ();
		}

		public int CompareTo (object obj) {
			if (obj == null)
				return 1;

			Fingerprint ofp = obj as Fingerprint;

			if (ofp == null)
				throw new ArgumentException ("Can only compare two Fingerprints");

			byte[] other = ofp.value;
			
			int slen = value.Length;

			if (other.Length > slen)
				return -1;
			if (other.Length < slen)
				return 1;

			for (int i = 0; i < slen; i++) {
				if (other[i] > value[i])
					return -1;
				if (other[i] < value[i])
					return 1;
			}

			return 0;
		}

		public override string ToString () {
			StringBuilder sb = new StringBuilder (value.Length * 2);

			const string hextable = "0123456789abcdef";

			foreach (byte b in value) {
				sb.Append (hextable[b >> 4]);
				sb.Append (hextable[b & 0xF]);
			}

			return sb.ToString ();
		}

		// XML. 

		protected override void ExportXml (XmlWriter xw) {
		    xw.WriteBase64 (value, 0, value.Length);
		}

		protected override bool ImportXml (XmlReader xr, IWarningLogger log) {
		    try {
			value = Convert.FromBase64String (xr.ReadString ());
		    } catch (Exception e) {
			log.Error (9999, "Fingerprint XML recovery hit invalid format", e.Message);
			return true;
		    }

		    return false;
		}

		// Result

		public override Fingerprint GetFingerprint (IBuildContext ctxt, Fingerprint cached) {
			return this;
		}

		// Clone

		protected override void CloneTo (Result dest) {
			if (value != null)
				(dest as Fingerprint).value = (byte[]) value.Clone ();
		}
	}
}
