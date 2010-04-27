//
// PersonalDetail.cs:
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
	public sealed class PersonalDetail : ContactDetail {
		#region .ctor
		public PersonalDetail()
			: base() {
			address = new Address();
			otherAddress = new Address();
			photo = new Property();
			spouse = null;
			children = null;
			anniversary = null;
			birthday = null;
			gender = null;
			hobbies = null;
		}
		#endregion
		
		#region Fields
		private Address address;
		private Address otherAddress;
		private Property photo;
		private String spouse;
		private String children;
		private String anniversary;
		private String birthday;
		private String gender;
		private String hobbies;
		#endregion

		#region Properties
		public Address Address {
			get { return address; }
		}

		public Address OtherAddress {
			get { return otherAddress; }
		}

		public string Spouse {
			get { return spouse; }
			set { spouse = value; }
		}

		public string Children {
			get { return children; }
			set { children = value; }
		}

		public string Anniversary {
			get { return anniversary; }
			set { anniversary = value; }
		}

		public string Birthday {
			get { return birthday; }
			set { birthday = value; }
		}

		public string Gender {
			get { return gender; }
			set { gender = value; }
		}

		public Property Photo {
			get { return photo; }
		}

		public string Hobbies {
			get { return hobbies; }
			set { hobbies = value; }
		}
		#endregion
	}
}