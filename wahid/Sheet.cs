//
// Cell.cs
//
// Authors:
//   Miguel de Icaza (miguel@novell.com)
//
// Copyright 2008 Novell, Inc (http://www.novell.com).
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
using System.Collections.Generic;

namespace Wahid {

	public class Sheet {
		public string Name;
		public int Id;

		Dictionary<long,Cell> cells;

		static long MakeKey (Cell cell)
		{
			return (cell.Col << 32) | cell.Row;
		}
		
		public Sheet (string name, int id)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			this.Name = name;
			this.Id = id;

			cells = new Dictionary<long,Cell> ();
		}
		

		public Cell CreateCell (int col, int row)
		{
			var cell = new Cell (this, col, row);

			Attach (cell);
			
			return cell;
		}

		public void Attach (Cell cell)
		{
			long k = MakeKey (cell);

			cells [k] = cell;
		}
	}
}

