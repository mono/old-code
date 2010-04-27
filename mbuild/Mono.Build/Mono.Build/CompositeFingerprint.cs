//
// A fingerprint made up of several fingerprints
//

using System;
using System.Collections;
using System.Security.Cryptography;
using System.Runtime.Serialization;

namespace Mono.Build {

	[Serializable]
	public class CompositeFingerprint : Fingerprint {

		protected Fingerprint[] children;

		public CompositeFingerprint (ICollection coll, IBuildContext ctxt, Fingerprint cached) : base () {
			Calculate (coll, ctxt, cached);
		}

		protected CompositeFingerprint () {}

		protected override void CloneTo (Result r) {
			base.CloneTo (r);

			CompositeFingerprint fp = (CompositeFingerprint) r;
			fp.children = (Fingerprint[]) children.Clone ();
		}

		// Keep this as a separate function so inheritors can call it in their ctors

		protected void Calculate (ICollection coll, IBuildContext ctxt, Fingerprint cached) {
			CompositeFingerprint cf = cached as CompositeFingerprint;

			children = new Fingerprint[coll.Count];
			int i = 0;

			if (cf != null && cf.children.Length == coll.Count) {
				foreach (IFingerprintable item in coll) {
					children[i] = GenericFingerprints.GetFingerprint (item, ctxt, 
											  cf.children[i]);
					i++;
				}
			} else {
				foreach (IFingerprintable item in coll) {
					children[i] = GenericFingerprints.GetFingerprint (item, ctxt, null);
					i++;
				}
			}

			value = Calculate (children);
		}

		public static byte[] Calculate (ICollection fps) {
			byte[] data  = new byte[fps.Count * PreferredHash.Size];
			int i = 0;

			foreach (Fingerprint item in fps) {
				byte[] fp;

				if (item == null)
					fp = GenericFingerprints.Null.Value;
				else
					fp = item.Value;

				fp.CopyTo (data, PreferredHash.Size * i);
				i++;
			}

			return PreferredHash.Algo.ComputeHash (data);
		}

#if ORDER_INDEP_HASH
		protected byte[] Calculate () {
			int count = children.Length;
			byte[] data = new byte[PreferredHash.Size / 8];

			for (int i = 0; i < count; i++) {
				Fingerprint item = children[i];

				if (item == null)
					continue;

				byte[] v = item.Value;

				for (int j = 0; j < v.Length; j++)
					data[j] ^= v[j];
			}

			return data;
		}
#endif

	}
}
