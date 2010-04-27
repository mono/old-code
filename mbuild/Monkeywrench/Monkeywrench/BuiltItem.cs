// BuiltItem.cs -- a tuple of information regarding a built target:
// a result, a fingerprint of the result, and a fingerprint of the 
// build that generated it

using System;

using Mono.Build;

namespace Monkeywrench {

	[Serializable]
	public struct BuiltItem {

		public Result Result;
		public Fingerprint ResultPrint;
		public Fingerprint BuildPrint;

		public BuiltItem (Result r, Fingerprint rp, Fingerprint bp) {
			Result = r;
			ResultPrint = rp;
			BuildPrint = bp;
		}

		public bool IsValid {
			get {
				return Result != null;
			}
		}

		public bool IsFixed {
			get {
				if (BuildPrint == null)
					throw new InvalidOperationException ();

				return BuildPrint == GenericFingerprints.Null;
			}
		}

		public override string ToString () {
			return String.Format ("[BI: {0}, {1}, {2}]", Result, ResultPrint, BuildPrint);
		}

		public static readonly BuiltItem Null = new BuiltItem (null, null, null);
	}
}
