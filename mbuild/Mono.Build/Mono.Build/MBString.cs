//
// A string result.
//

using System;
using System.Runtime.Serialization;
using System.Xml;

namespace Mono.Build {

	[Serializable]
	    public class MBString : Result, IRefTypeResult<string> {

		string value;

		public string Value { 
			get { return value; } 

			set {
				if (value == null)
					throw new ArgumentNullException ();
				this.value = value;
			}
		}

		public MBString () : base () {
		}

		public MBString (string value) : base () {
			this.value = value;
		}

		// Result

		public override string ToString() {
			return String.Format ("\"{0}\"", value);
		}

		protected override void ExportXml (XmlWriter xw) {
			xw.WriteString (value);
		}

		protected override bool ImportXml (XmlReader xr, IWarningLogger log) {
			value = xr.ReadString ();
			return false;
		}

		// Equality

		protected override bool ContentEquals (Result other)
		{
		    return value == ((MBString) other).value;
		}

		protected override int InternalHash ()
		{
		    return value.GetHashCode ();
		}

		// Clone

		protected override void CloneTo (Result dest) {
			MBString sdest = (MBString) dest;

			sdest.value = (System.String) value.Clone ();
		}

		// Fingerprint

		public override Fingerprint GetFingerprint (IBuildContext ctxt, Fingerprint cached) {
			return Fingerprint.FromText (value);
		}
	}
}
