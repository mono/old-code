using System;
using System.Reflection;
using System.Collections.Generic;
using System.CodeDom;

using Mono.Build;
using Mono.Build.Bundling;

namespace MBBundleGen {

    public interface INamespaceParams {

	IEnumerable<string> Parameters { get; }

	bool HasParam (string name);

	StructureParameterKind this[string name] { get; }

	UserType StructParamType (string name);
    }
}
