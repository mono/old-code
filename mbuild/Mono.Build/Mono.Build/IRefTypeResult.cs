using System;
using System.Collections;

namespace Mono.Build {

    public interface IRefTypeResult<T> : IConvertibleResult<T>
	where T : class {
    }
}
