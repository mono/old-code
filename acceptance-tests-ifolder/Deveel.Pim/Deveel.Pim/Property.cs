//
// Property.cs:
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
	/// This object represents a property for VCard and ICalendar object
	/// (i.e. its value and its parameters)
	/// </summary>
	public class Property {
		#region .ctor
		public Property()
			: this(null) {
		}

		public Property(string propertyValue) {
			altrep = null;
			cn = null;
			cutype = null;
			delegatedFrom = null;
			delegatedTo = null;
			dir = null;
			encoding = null;
			language = null;
			member = null;
			partstat = null;
			related = null;
			sentby = null;
			value = null;
			xParams = new Hashtable();

			tag = null;
			this.propertyValue = propertyValue;

			group = null;
			chrset = null;
			type = null;
		}
		#endregion

		#region Fields
		private string altrep;
		private string cn;
		private string cutype;
		private string delegatedFrom;
		private string delegatedTo;
		private string dir;
		private string encoding;
		private string language;
		private string member;
		private string partstat;
		private string related;
		private string sentby;
		private string value;
		private Hashtable xParams;

		private string tag;
		private object propertyValue;

		private string group;
		private string chrset;
		private string type;
		#endregion

		#region Properties
		/// <summary>
		/// Return parameter <c>ENCODING</c> type that is used to specify an alternate 
		/// encoding for a value.
		/// </summary>
		public string Encoding {
			get { return encoding; }
			set { encoding = value; }
		}

		/// <summary>
		/// Return parameter <c>LANGUAGE</c> type that is used to identify data in multiple
		/// languages.
		/// </summary>
		public string Language {
			get { return language; }
			set { language = value; }
		}

		/// <summary>
		/// Return parameter <c>VALUE</c> type that is used to identify the value data type and 
		/// format of the value.
		/// </summary>
		public string Value {
			get { return value; }
			set { this.value = value; }
		}

		/// <summary>
		/// Returns the value of this property
		/// </summary>
		public object PropertyValue {
			get { return propertyValue; }
			set { propertyValue = value; }
		}

		public string StringValue {
			get {
				if (propertyValue == null)
					return null;
				if (propertyValue is string)
					return (string) propertyValue;
				return propertyValue.ToString();
			}
		}

		public string AlternateText {
			get { return altrep; }
			set { altrep = value; }
		}

		public string CommonName {
			get { return cn; }
			set { cn = value; }
		}

		public string CalendarUserType {
			get { return cutype; }
			set { cutype = value; }
		}

		public string DelegatedFrom {
			get { return delegatedFrom; }
			set { delegatedFrom = value; }
		}

		/// <summary>
		/// Returns the calendar users to whom the calendar user specified by this 
		/// property has delegated participation.
		/// </summary>
		public string DelegatedTo {
			get { return delegatedTo; }
			set { delegatedTo = value; }
		}

		/// <summary>
		/// Returns the reference to a directory entry associated with the calendar u
		/// ser specified by this property.
		/// </summary>
		public string Directory {
			get { return dir; }
			set { dir = value; }
		}

		/// <summary>
		/// Returns the group or list membership of the calendar user specified by 
		/// this property.
		/// </summary>
		public string Member {
			get { return member; }
			set { member = value; }
		}

		/// <summary>
		/// Returns the partecipation status of the calendar user specified by 
		/// this property.
		/// </summary>
		public string PartecipationStatus {
			get { return partstat; }
			set { partstat = value; }
		}

		/// <summary>
		/// Returns the relationship of the alarm trigger with respect to the start 
		/// or end of the calendar component.
		/// </summary>
		public string Related {
			get { return related; }
			set { related = value; }
		}

		/// <summary>
		/// Returns the calendar user that is acting on behalf of the calendar user
		/// specified by this property
		/// </summary>
		public string SentBy {
			get { return sentby; }
			set { sentby = value; }
		}

		/// <summary>
		/// Returns the group parameter of this property. The group parameter is used
		/// to group related attributes together.
		/// </summary>
		public string Group {
			get { return group; }
			set { group = value; }
		}

		/// <summary>
		/// Returns the default character set used within the body part.
		/// </summary>
		public string Charset {
			get { return chrset; }
			set { chrset = value; }
		}

		/// <summary>
		/// Returns the type parameter of this property
		/// </summary>
		public string PropertyType {
			get { return type; }
			set { type = value; }
		}

		public Hashtable XParams {
			get { return xParams; }
			set {
				if (xParams == null) {
					xParams = new Hashtable();
				}
				xParams.Clear();
				foreach (DictionaryEntry entry in value) {
					xParams.Add(entry.Key, entry.Value);
				}
			}
		}

		public string Tag {
			get { return tag; }
			set { tag = value; }
		}
		#endregion
	}
}