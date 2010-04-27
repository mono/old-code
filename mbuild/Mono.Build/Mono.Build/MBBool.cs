//
// A boolean result.
//

using System;
using System.Runtime.Serialization;
using System.Xml;

namespace Mono.Build {

	[Serializable]
	    public class MBBool : Result, IValueTypeResult<bool> {

		bool value;

		public bool Value { 
			get { return value; } 
			
			set { this.value = value; }
		}

		public MBBool () : base () {
		}

		public MBBool (bool value) : base () {
			this.value = value;
		}

		// Constants

		public static MBBool True { get { return new MBBool (true); } }
		public static MBBool False { get { return new MBBool (false); } }

		public static MBBool FromValue (bool val) {
			if (val)
				return True;
			return False;
		}

		// Result

		public override string ToString() {
			return value.ToString ();
		}

		protected override void ExportXml (XmlWriter xw) {
			if (value)
				xw.WriteString ("true");
			else
				xw.WriteString ("false");
		}

		protected override bool ImportXml (XmlReader xr, IWarningLogger log) {
			string s = xr.ReadString ();

			switch (s) { 
			case "true":
				value = true;
				break;
			case "false":
				value = false;
				break;
			default:
				log.Warning (3019, "Invalid boolean value during XML import", s);
				return true;
			}

			return false;
		}

		// Clone

		protected override void CloneTo (Result dest) {
			MBBool bdest = (MBBool) dest;

			bdest.value = value;
		}

		// Equality

		protected override bool ContentEquals (Result other)
		{
		    return value == ((MBBool) other).value;
		}

		protected override int InternalHash ()
		{
		    return value.GetHashCode ();
		}

		// Fingerprint

		public override Fingerprint GetFingerprint (IBuildContext ctxt, Fingerprint cached) {
			return GenericFingerprints.GetFingerprint (value, ctxt, cached);
		}
	}
}
