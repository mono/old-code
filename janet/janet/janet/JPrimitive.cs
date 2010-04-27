// JPrimitive.cs: lowest-level definitions for the JANET runtime
//
// Author: Steve Newman (steve@snewman.net)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Bitcraft, Inc.


#define TRACE

using System;
using System.IO;
using System.Text;
using System.Diagnostics;


namespace JANET.Runtime {

public class TypeError : ApplicationException
	{
	public TypeError(string message) : base(message) {}
	public TypeError(string message, Exception innerException) :
			base(message, innerException) {}
	} // TypeError


public class ReferenceError : ApplicationException
	{
	public ReferenceError(string message) : base(message) {}
	public ReferenceError(string message, Exception innerException) :
			base(message, innerException) {}
	} // ReferenceError


public class RangeError : ApplicationException
	{
	public RangeError(string message) : base(message) {}
	public RangeError(string message, Exception innerException) :
			base(message, innerException) {}
	} // RangeError


public class ParameterError : ApplicationException
	{
	public ParameterError(string message) : base(message) {}
	public ParameterError(string message, Exception innerException) :
			base(message, innerException) {}
	} // ParameterError


// This class contains support functions used by generated code.
public class PrimSupport
	{
	// If x is a numeric value, set d to that value and return true.  Otherwise
	// set d to 0 and return false.
	public static bool AsNumber(object x, out double d)
		{
		// HACK snewman 8/7/01: flesh this out to check for all numeric types.
		// Maintain JObject.IsNumber as well.
		if (x is double)
			{
			d = (double)x;
			return true;
			}
		else if (x is Int32)
			{
			d = (Int32)x;
			return true;
			}
		else if (x is UInt32)
			{
			d = (UInt32)x;
			return true;
			}
		else
			{
			d = 0;
			return false;
			}
		
		} // AsNumber
	
	} // PrimSupport


} // namespace JANET.Runtime