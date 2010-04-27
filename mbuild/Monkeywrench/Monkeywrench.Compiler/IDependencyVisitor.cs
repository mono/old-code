using System;

using Mono.Build.Bundling;

namespace Monkeywrench.Compiler {

    public interface IDependencyVisitor {

	// All these functions return can true to cancel the
	// visit loop.

	bool VisitUnnamed (SingleValue<TargetBuilder> val);
	bool VisitNamed (int arg, SingleValue<TargetBuilder> val);
	bool VisitDefaultOrdered (SingleValue<TargetBuilder> val);
	bool VisitDefaultValue (int arg, SingleValue<TargetBuilder> val);
    }
}

