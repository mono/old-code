//
// BusinessDetail.cs:
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
using System.Collections;

namespace Deveel.Pim {
	/// <summary>
	/// An object containing the business details of a contact
	/// </summary>
	public class BusinessDetail : ContactDetail {
		#region .ctor
		public BusinessDetail()
			: base() {
			role = new Property();
			titles = null;
			address = new Address();
			company = new Property();
			department = new Property();
			logo = new Property();
			manager = null;
			assistant = null;
			officeLocation = null;
			companies = null;
		}
		#endregion

		#region Fields
		private Address address;
		private Property role;
		private ArrayList titles;
		private Property company;     // the contact's company
		private Property department;
		private Property logo;
		private String manager;
		private String assistant;
		private String officeLocation;
		private String companies;  // this differs from company since it is one or
		// more companies associated to this company 
		#endregion

		#region Properties
		public Property Role {
			get { return role; }
		}

		public ArrayList Titles {
			get { return titles; }
			set { titles = value; }
		}

		public void AddTitle(Title title) {
			if (title == null) {
				return;
			}

			if (titles == null) {
				titles = new ArrayList();
			}

			int pos = titles.IndexOf(title);
			if (pos < 0) {
				titles.Add(title);
			} else {
				titles[pos] = title;
			}
		}

		public Address Address {
			get { return address; }
		}

		public Property Company {
			get { return company; }
		}

		public Property Department {
			get { return department; }
		}

		public string Manager {
			get { return manager; }
			set { manager = value; }
		}

		public string Assistant {
			get { return assistant; }
			set { assistant = value; }
		}

		public Property Logo {
			get { return logo; }
		}

		public string OfficeLocation {
			get { return officeLocation; }
			set { officeLocation = value; }
		}

		public string Companies {
			get { return companies; }
			set { companies = value; }
		}
		#endregion
	}
}