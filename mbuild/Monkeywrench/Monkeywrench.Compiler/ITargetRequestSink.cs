// ITargetRequestSink.cs -- something that accepts target requests

using System;

namespace Monkeywrench.Compiler {

	public interface ITargetRequestSink {
		int RequestTarget (string target);
		int LookupTarget (string target); // error if target not already extant
	}
}
