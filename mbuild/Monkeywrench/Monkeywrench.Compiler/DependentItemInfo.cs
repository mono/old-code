using System;
using System.Runtime.Serialization;

using Mono.Build;

namespace Monkeywrench.Compiler {

    [Serializable]
    public struct DependentItemInfo {
	public string Name; // relative to top_srcdir
	public Fingerprint Fingerprint;
    }

}
