using System;
using System.Collections;

namespace Mono.Build {

    public interface IValueTypeResult<T> : IConvertibleResult<T> 
	where T : struct {
    }
}
