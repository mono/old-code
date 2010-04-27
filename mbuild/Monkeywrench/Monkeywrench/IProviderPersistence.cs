//
// An interface for persisting the state of a build provider
//

using System;

using Mono.Build;

namespace Monkeywrench {

	public interface IProviderPersistence {
		BuiltItem GetItem (string name);
		void SetItem (string name, BuiltItem value);

		bool Save (string file, IWarningLogger log);
	}

}
