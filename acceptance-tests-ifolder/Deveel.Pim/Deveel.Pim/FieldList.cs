//
// FieldList.cs:
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
	// This objects represents a list of vCard fields. The list is based
	// on the informations contained in a list of parser tokens
	class FieldsList {
		#region .ctor
		public FieldsList() {
			list = new ArrayList();
		}
		#endregion

		#region Fields
		private ArrayList list;
		#endregion

		#region Properties
		public int Count {
			get { return list.Count; }
		}

		public string this[int index] {
			get { return (string)list[index]; }
		}

		public string Element {
			get {
				string s = this[0];
				if (s == null)
					return String.Empty;
				return s;
			}
		}
		#endregion

		#region Public Methods
		public void AddValue(string listValues) {
			int index = listValues.IndexOf(';');
			if (index == -1) {
				list.Add(listValues);
			} else {
				string tmp = "";

				// This is for reading the last element too
				listValues = listValues + ';';
				while (index != -1) {
					string token = listValues.Substring(0, index);
					listValues = listValues.Substring(index + 1);

					if (token.EndsWith("\\")) {
						tmp = tmp + token.Substring(0, token.Length - 1) + ";";
					} else {
						if (tmp.Length > 0) {
							tmp = tmp + token;
							list.Add(tmp);
							tmp = "";
						} else {
							list.Add(token);
						}
					}

					index = listValues.IndexOf(';');
				}
			}
		}

		// Method used to insert all the listValues separated by semicolons
		// as a string. It's needed i.e. when a tag like NOTE is parsed. Its argument
		// value is to be consider as a unique string with no token
		public void AddStringValue(string listValues) {
			if (listValues != null)
				list.Add(listValues);
			else
				list.Add(String.Empty);
		}
		#endregion
	}
}