//
// Address.cs:
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
	public sealed class Address {
		#region .ctor
		public Address() {
			postOfficeAddress = new Property();
			roomNumber = new Property();
			street = new Property();
			city = new Property();
			state = new Property();
			postalCode = new Property();
			country = new Property();
			label = new Property();
			extendedAddress = new Property();
		}
		#endregion

		#region Fields
		private Property postOfficeAddress;
		private Property roomNumber;
		private Property street;
		private Property city;
		private Property state;
		private Property postalCode;
		private Property country;
		private Property label;
		private Property extendedAddress;
		#endregion

		#region Properties
		public Property PostOfficeAddress {
			get { return postOfficeAddress; }
		}

		public Property RoomNumber {
			get { return roomNumber; }
		}

		public Property Street {
			get { return street; }
		}

		public Property City {
			get { return city; }
		}

		public Property State {
			get { return state; }
		}

		public Property PostalCode {
			get { return postalCode; }
		}

		public Property Country {
			get { return country; }
		}

		public Property Label {
			get { return label; }
		}

		public Property ExtendedAddress {
			get { return extendedAddress; }
		}
		#endregion
	}
}