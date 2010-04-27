//
// Phone.cs:
//
// Author:
//	Antonello Provenzano (antonello@deveel.com)
//
// (C) 2006, Deveel srl, (http://deveel.com)
//

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

using System;

namespace Deveel.Pim {
	public class Phone : Property {
		#region .ctor
		public Phone()
			: base() {
		}

		public Phone(string value)
			: base(value) {
		}
		#endregion
		
		#region Fields
		private string phoneType;
		#endregion

		#region Properties
		public string PhoneType {
			get { return phoneType; }
			set { phoneType = value; }
		}
		#endregion

		#region Public Methods
		public override bool Equals(object o) {
			Phone p = o as Phone;
			if (o == null)
				return false;

			if (phoneType == null && p.phoneType == null)
				return true;
			
			return String.Compare(phoneType, p.PhoneType, true) == 0;
		}

		public override int GetHashCode() {
			if (phoneType == null)
				return base.GetHashCode();
			return phoneType.GetHashCode();
		}
		#endregion
	}
}