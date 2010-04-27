// OperationScope.cs -- how much of the tree we should span

using System;

namespace Monkeywrench {

	public enum OperationScope {
		HereAndBelow,
		HereOnly,
		Everywhere
	}
}
