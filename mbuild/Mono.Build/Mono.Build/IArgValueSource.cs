using System;
using System.Collections;

namespace Mono.Build {

    public interface IArgValueSource {

	string GetTargetName ();

	Result[] GetArgValue (int id);

    }
}
