//
// Calendar.cs:
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
using System.IO;

namespace Deveel.Pim {
	public class Calendar {
		#region .ctor
		public Calendar() {
			calScale = new Property();
			method = new Property();
			prodId = new Property();
			version = new Property();
			calEvent = new Event();

			xTags = new XTagCollection();
		}
		#endregion
		
		#region Fields
		private Property calScale;
		private Property method;
		private Property prodId;
		private Property version;
		private XTagCollection xTags;
		private Event calEvent;
		#endregion

		#region Properties
		public Property ProductId {
			get { return prodId; }
			set { prodId = value; }
		}

		public Property Version {
			get { return version; }
			set { version = value; }
		}

		public Property CalendarScale {
			get { return calScale; }
			set { calScale = value; }
		}

		public Property Method {
			get { return method; }
			set { method = value; }
		}

		public Event Event {
			get { return calEvent; }
			set { calEvent = value; }
		}

		public XTagCollection XTags {
			get { return xTags; }
		}
		#endregion
		
		public static Calendar ParseiCal(string s, string timeZone, string charset) {
			iCalParser parser = new iCalParser(new StringReader(s), timeZone, charset);
			return parser.iCal();
		}
	}
}