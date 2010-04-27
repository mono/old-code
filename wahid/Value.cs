//
// Value.cs
//
// Authors:
//   Miguel de Icaza (miguel@novell.com)
//
// Copyright 2008 Novell, Inc (http://www.novell.com).
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System.Xml.Linq;

namespace Wahid {

	public class Value {
	}

	public class BoolValue : Value {
		public bool Value;
		
		public BoolValue (bool v)
		{
			this.Value = v;
		}

		public static implicit operator BoolValue (bool v)
		{
			return new BoolValue (v);
		}
	}

	public class ErrorValue : Value {
		public string Error;

		public static ErrorValue DivisionByZero = new ErrorValue ("#DIV/0!");
		public static ErrorValue NA = new ErrorValue ("#NA");
		public static ErrorValue Name = new ErrorValue ("#NAME?");
		public static ErrorValue Null = new ErrorValue ("#NULL?");
		public static ErrorValue Num = new ErrorValue ("#NUM!");
		public static ErrorValue Ref = new ErrorValue ("#REF!");
		
		public ErrorValue (string v)
		{
			Error = v;
		}
	}

	public class StringValue : Value {
		public string Value;
		
		public StringValue (string v)
		{
			this.Value = v;
		}

		public static implicit operator StringValue (string s)
		{
			return new StringValue (s);
		}
	}

	public class RichStringValue : Value {
		public XElement Value;

		public RichStringValue (XElement x)
		{
			this.Value = x;
		}

		public static implicit operator RichStringValue (XElement d)
		{
			return new RichStringValue (d);
		}
	}

	public class NumberValue : Value {
		public double Value;

		public NumberValue (double v)
		{
			Value = v;
		}

		public static implicit operator NumberValue (double d)
		{
			return new NumberValue (d);
		}
	}
}