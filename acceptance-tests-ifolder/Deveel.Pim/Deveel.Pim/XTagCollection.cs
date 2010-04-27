//
// XTagCollection.cs:
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
	public sealed class XTagCollection : ICollection {
		#region .ctor
		internal XTagCollection() {
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
		
		public XTag this[string tagName] {
			get {
				int index = IndexOf(tagName);
				if (index == -1)
					return null;
				return list[index] as XTag;
			}
		}
		
		public XTag this[int index] {
			get { return list[index] as XTag; }
		}
		#endregion

		#region ICollection Members
		bool ICollection.IsSynchronized {
			get { return false; }
		}
		
		object ICollection.SyncRoot {
			get { return null; }
		}
		
		void ICollection.CopyTo(Array array, int index) {
			list.CopyTo(array, index);
		}
		#endregion

		#region Private Methods
		private int IndexOf(string tag) {
			for(int i = 0; i < list.Count; i++) {
				XTag xtag = list[i] as XTag;
				//ISSUE: should be case-sensitive?
				if (String.Compare(xtag.Tag.Tag, tag, true) == 0)
					return i;
			}

			return -1;
		}
		#endregion

		#region Public Methods
		public void Add(XTag value) {
			int index = IndexOf(value.Tag.Tag);
			if (index == -1) {
				list.Add(value);
			} else {
				list[index] = value;
			}
		}
		
		public void Remove(string tagName) {
			int index = IndexOf(tagName);
			if (index != -1)
				list.RemoveAt(index);
		}
		
		public bool Contains(string tagName) {
			return IndexOf(tagName) != -1;
		}
		
		public IEnumerator GetEnumerator() {
			return list.GetEnumerator();
		}
		#endregion
	}
}