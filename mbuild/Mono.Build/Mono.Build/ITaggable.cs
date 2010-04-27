//
// ITaggable.cs -- something that can have tags applied to it
// FIXME: move to SingleValue.
//

using System;

namespace Mono.Build {

    public interface ITaggable {

	bool HasTag (string name);

	void AddTargetTag (string name, string value);
	void AddResultTag (string name, Result value);

    }
}
