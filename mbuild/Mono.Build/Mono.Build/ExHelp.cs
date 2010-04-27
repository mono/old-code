using System;

namespace Mono.Build {

    public static class ExHelp {

	public static ArgumentException Argument (string param, string fmt, params object[] args) 
	{
	    return new ArgumentException (String.Format (fmt, args), param);
	}

	public static ApplicationException App (string fmt, params object[] args) 
	{
	    return new ApplicationException (String.Format (fmt, args));
	}

	public static ArgumentOutOfRangeException Range (string fmt, params object[] args) 
	{
	    return new ArgumentOutOfRangeException (String.Format (fmt, args));
	}

	public static InvalidOperationException InvalidOp (string fmt, params object[] args) 
	{
	    return new InvalidOperationException (String.Format (fmt, args));
	}

    }
}
