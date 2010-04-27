//
// The hash algo that we use
//

using System;
using System.Security.Cryptography;

namespace Mono.Build {

	public static class PreferredHash {

		static int size = 0;
		static HashAlgorithm algo;

		public static HashAlgorithm Algo {
			get {
				if (algo == null)
					algo = MD5.Create ();
				return algo;
			}
		}

		public static int Size {
			get {
				if (size == 0)
					size = Algo.HashSize;

				return size;
			}
		}
	}
}
