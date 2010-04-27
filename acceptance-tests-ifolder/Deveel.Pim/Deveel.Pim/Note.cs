//
// Note.cs:
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
	public sealed class Note : Property {
		#region .ctor
		public Note()
			: base() {
		}
		#endregion
		
		#region Fields
		private String noteType;
		#endregion

		#region Properties
		public string NoteType {
			get { return noteType; }
			set { noteType = value; }
		}
		#endregion

		#region Public Methods
		public override bool Equals(Object o) {
			Note note = o as Note;
			if (o == null)
				return false;

			if (noteType == null && note.noteType == null)
				return true;
			
			return String.Compare(noteType, note.noteType, true) == 0;
		}

		public override int GetHashCode() {
			if (noteType == null)
				return base.GetHashCode();
			return noteType.GetHashCode();
		}
		#endregion
	}
}