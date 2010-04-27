/***************************************************************************
 *  Copyright 2005 Raphaël Slinckx <raphael@slinckx.net> 
 ****************************************************************************/

/*  THIS FILE IS LICENSED UNDER THE MIT LICENSE AS OUTLINED IMMEDIATELY BELOW: 
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a
 *  copy of this software and associated documentation files (the "Software"),  
 *  to deal in the Software without restriction, including without limitation  
 *  the rights to use, copy, modify, merge, publish, distribute, sublicense,  
 *  and/or sell copies of the Software, and to permit persons to whom the  
 *  Software is furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in 
 *  all copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 *  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 *  DEALINGS IN THE SOFTWARE.
 */

using System;
using Entagged.Audioformats.Util;
using Entagged.Audioformats.Mp3;

namespace Entagged.Audioformats.Mp3.Util.Id3Frames {
	public class GenericId3Frame : Id3Frame {
		
		private byte[] data;
		private string id;
		
		/*
		 * 0,1| frame flags
		 * 2,..,0X00| Owner ID
		 * xx,...| identifier (binary)
		 */
		
		public GenericId3Frame(string id, byte[] raw, byte version) : base(raw, version) {
			this.id = id;
		}
		
		public override string Id {
			get { return this.id; }
		}
		
		public override bool IsBinary {
			get { return true; }
			set { /* NA */ }
		}
		
		public byte[] Data {
			get { return data; }
		}
		
		public override bool IsCommon {
			get { return false; }
		}
		
		public override void CopyContent(TagField field) {
		    if(field is GenericId3Frame)
		        this.data = (field as GenericId3Frame).Data;
		}
		
		public override bool IsEmpty {
		    get { return this.data.Length == 0; }
		}
		
		protected override void Populate(byte[] raw)
		{
			this.data = new byte[raw.Length - flags.Length];
			for(int i = 0; i<data.Length; i++)
				data[i] = raw[i + flags.Length];
		}
		
		protected override byte[] Build()
		{
			byte[] b = new byte[4 + 4 + data.Length + flags.Length];
			
			int offset = 0;
			Copy(IdBytes, b, offset);
			offset += 4;
			
			Copy(GetSize(b.Length-10), b, offset);
			offset += 4;
			
			Copy(flags, b, offset);
			offset += flags.Length;
			
			Copy(data, b, offset);
			
			return b;
		}
		
		public override string ToString() {
			return this.id+" : No associated view";
		}
	}
}
