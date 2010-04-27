//
// PropertyCollection.cs:
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
	public sealed class PropertyCollection : ICollection {
		#region .ctor
		internal PropertyCollection() {
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
		
		public Property this[int index] {
			get { return list[index] as Property; }
		}
		#endregion

		#region ICollection Members
		void ICollection.CopyTo(Array array, int index) {
			list.CopyTo(array, index);
		}

		object ICollection.SyncRoot {
			get { return null; }
		}

		bool ICollection.IsSynchronized {
			get { return false; }
		}
		#endregion

		#region Public Methods
		public void Add(Property value) {
			int index = list.IndexOf(value);
			if (index == -1) {
				list.Add(value);
			}else {
				list[index] = value;
			}
		}
		
		public void RemoveAt(int index) {
			list.RemoveAt(index);
		}
		
		public IEnumerator GetEnumerator() {
			return list.GetEnumerator();
		}
		#endregion
	}
}