// OperationFunc.cs -- a delegate for operating on a target

using System;

using Mono.Build;

namespace Monkeywrench {

	// return value is "should we give up?"
	public delegate bool OperationFunc (WrenchProject proj, BuildServices bs);

}
