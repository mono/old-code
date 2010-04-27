//
// Name.cs:
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
	public class Name {
		#region .ctor
		public Name() {
			salutation = new Property();
			firstName = new Property();
			middleName = new Property();
			lastName = new Property();
			suffix = new Property();
			displayName = new Property();
			nickname = new Property();
			initials = new Property();
		}
		#endregion
		
		#region Fields
		private Property salutation;
		private Property firstName;
		private Property middleName;
		private Property lastName;
		private Property suffix;
		private Property displayName;
		private Property nickname;
		private Property initials;
		#endregion

		#region Properties
		public Property Salutation {
			get { return salutation; }
		}

		public Property FirstName {
			get { return firstName; }
		}

		public Property MiddleName {
			get { return middleName; }
		}

		public Property LastName {
			get { return lastName; }
		}

		public Property Suffix {
			get { return suffix; }
		}

		public Property DisplayName {
			get { return displayName; }
		}

		public Property Nickname {
			get { return nickname; }
		}

		public Property Initials {
			get { return initials; }
			set { initials = value; }
		}
		#endregion
	}
}