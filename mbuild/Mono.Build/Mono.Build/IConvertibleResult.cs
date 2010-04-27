using System;
using System.Collections;

namespace Mono.Build {

    public interface IConvertibleResult<T> {

	T Value { get; set; }

    }
}
