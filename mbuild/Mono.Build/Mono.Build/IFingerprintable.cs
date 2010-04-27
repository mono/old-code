//
// A way to get a fingerprint of a non-fundamental object
//

using System;

namespace Mono.Build
{
	public interface IFingerprintable
	{
		// See Result.cs for parameter explanation
		Fingerprint GetFingerprint (IBuildContext ctxt, Fingerprint cached);
	}
}
