using System;
using System.Collections;

namespace Mono.Build {

    public interface IArgInfoSink {

	void WantTargetName (bool required);

	void AddArg (int id, string name, Type type, ArgFlags flags);
    }
}
