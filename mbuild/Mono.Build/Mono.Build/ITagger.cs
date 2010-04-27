//
// ITagger.cs -- something that can apply tags to other things
//

using System;

namespace Mono.Build {

	public interface ITagger {
		void ApplyTags (ITaggable taggable);
	}

}
