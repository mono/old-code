//
// A result table serialized to/from a file
//

using System;
using System.Runtime.Serialization;

using Mono.Build;

namespace Monkeywrench {

	[Serializable]
	    public class FileStateTable : StateTable, IProviderPersistence {
		FileStateTable () : base () {}

		public static FileStateTable Load (string path, IWarningLogger log) {
			FileStateTable fst;

			fst = (FileStateTable) SafeFileSerializer.Load (path, log);
			if (fst == null)
				fst = new FileStateTable ();

			return fst;
		}

		public bool Save (string path, IWarningLogger log) {
			return SafeFileSerializer.Save (path, this, log);
		}
	}
}
