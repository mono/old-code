//
// Email.cs:
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
	public class Email : Property {
		#region .ctor
		public Email()
			: base() {
		}

		public Email(string value)
			: base(value) {
		}
		#endregion
		
		#region Fields
		private string emailType;
		#endregion

		#region Properties
		public string EmailType {
			get { return emailType; }
			set { emailType = value; }
		}
		#endregion

		#region Public Methods
		public override bool Equals(object o) {
			Email email = o as Email;
			if (email == null)
				return false;

			if (emailType == null && email.emailType == null)
				return true;
			
			return String.Compare(emailType, email.EmailType, true) == 0;
		}

		public override int GetHashCode() {
			if (emailType == null) {
				return base.GetHashCode();
			}
			return emailType.GetHashCode();
		}
		#endregion
	}
}