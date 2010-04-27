//
// Contact.cs:
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
using System.IO;
using System.Text;

namespace Deveel.Pim {
	/// <summary>
	/// An object representing a contact.
	/// </summary>
	/// <remarks>
	/// This class can be used to store and retrive such information as contact name,
	/// telephone number, address and so on.
	/// </remarks>
	public sealed class Contact {
		#region .ctor
		public Contact() {
			name = new Name();

			notes = null;
			xTags = null;

			businessDetail = new BusinessDetail();
			personalDetail = new PersonalDetail();
			categories = new Property();
		}
		#endregion
		
		#region Fields
		private Name name;
		private ArrayList notes;
		private string uid;
		private string timezone;
		private string revision;
		private BusinessDetail businessDetail;
		private PersonalDetail personalDetail;
		private ArrayList xTags;
		private Property categories;
		private string languages;
		private short importance;
		private short sensitivity;
		private string subject;
		private int mileage;
		private string folder;
		#endregion

		#region Properties
		public string UID {
			get { return uid; }
			set { uid = value; }
		}

		public string TimeZone {
			get { return timezone; }
			set { timezone = value; }
		}

		public ArrayList Notes {
			get { return notes; }
			set { notes = value; }
		}

		public ArrayList XTags {
			get { return xTags; }
			set { xTags = value; }
		}

		public string Revision {
			get { return revision; }
			set { revision = value; }
		}

		public Name Name {
			get { return name; }
			set { name = value; }
		}

		public BusinessDetail BusinessDetail {
			get { return businessDetail; }
			set { businessDetail = value; }
		}

		public PersonalDetail PersonalDetail {
			get { return personalDetail; }
			set { personalDetail = value; }
		}

		public Property Categories {
			set { categories = value; }
			get { return categories; }
		}

		public string Languages {
			get { return languages; }
			set { languages = value; }
		}

		public short Importance {
			get { return importance; }
			set { importance = value; }
		}

		public short Sensitivity {
			get { return sensitivity; }
			set { sensitivity = value; }
		}

		public string Subject {
			get { return subject; }
			set { subject = value; }
		}

		public int Mileage {
			get { return mileage; }
			set { mileage = value; }
		}

		public string Folder {
			get { return folder; }
			set { folder = value; }
		}
		#endregion

		public void AddNote(Note note) {
			if (note == null) {
				return;
			}

			if (notes == null) {
				notes = new ArrayList();
			}

			int pos = notes.IndexOf(note);
			if (pos < 0) {
				notes.Add(note);
			} else {
				notes[pos] = note;
			}
		}

		public void AddXTag(XTag tag) {
			if (tag == null)
				return;

			if (xTags == null)
				xTags = new ArrayList();

			int pos = xTags.IndexOf(tag);
			if (pos < 0) {
				xTags.Add(tag);
			} else {
				xTags[pos] = tag;
			}
		}

		public string TovCardString(string timeZone, string charset) {
			return TovCardString(Deveel.Pim.TimeZone.GetTimeZone(timeZone), charset);
		}
		
		public string TovCardString(string timeZone) {
			return TovCardString(timeZone, Encoding.Default.EncodingName);
		}
		
		public string TovCardString() {
			return TovCardString("GMT");
		}
		
		public string TovCardString(TimeZone timeZone, string charset) {
			ContactConverter convert = new ContactConverter(timeZone, charset);
			return convert.Convert(this);
		}
		
		public string TovCardString(TimeZone timeZone) {
			return TovCardString(timeZone, Encoding.Default.EncodingName);
		}
		
		public static Contact ParsevCard(string s, string timeZone, string charset) {
			vCardParser parser = new vCardParser(new StringReader(s), timeZone, charset);
			return parser.vCard();
		}
		
		public static Contact ParsevCard(string s, string timeZone) {
			return ParsevCard(s, timeZone, Encoding.Default.EncodingName);
		}
		
		public static Contact ParsevCard(string s) {
			return ParsevCard(s, "GMT");
		}
	}
}