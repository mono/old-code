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

using System.Collections;
using Entagged.Audioformats.Util;

namespace Entagged.Audioformats.Ogg.Util {
	public class OggTagCreator : AbstractTagCreator {
		
		//Creates the ByteBuffer for the ogg tag
		protected override void Create(Tag tag, ByteBuffer buf, IList fields, int tagSize, int padding) {
			string vendorstring;
			if (tag.HasField("vendor"))
				vendorstring = tag.Get("vendor")[0] as string;
			else
				//FIXME: Do something about this
				vendorstring = "DefaultVendor";
				
			byte[] vendorBytes = Utils.GetBytes(vendorstring, "UTF-8");
			buf.Put( new byte[]{(byte)(vendorBytes.Length&0xFF), (byte)((vendorBytes.Length & 0xFF00) >> 8) , (byte)((vendorBytes.Length & 0xFF0000) >> 16), (byte)((vendorBytes.Length & 0xFF000000) >> 24)  } );
			buf.Put( vendorBytes );

			//[user comment list length]
			buf.Put( Utils.GetNumber(fields.Count) );
			
			foreach(byte[] field in fields) {
				buf.Put(field);
			}
		}
				
		protected override int GetFixedTagLength(Tag tag) {
			string vendorstring;
			if (tag.HasField("vendor"))
				vendorstring = tag.Get("vendor")[0] as string;
			else
				vendorstring = "DefaultVendor";
			
			return 8 + Utils.GetBytes(vendorstring, "UTF-8").Length;
		}
		
		protected override byte[] CreateField(string id, string content) {
	        byte[] idBytes = Utils.GetBytes(id);
	        byte[] contentBytes = Utils.GetBytes(content, "UTF-8");
	        byte[] b = new byte[4 + idBytes.Length + 1 + contentBytes.Length];

	        int offset = 0;
			Utils.GetNumber(idBytes.Length + 1 + contentBytes.Length).CopyTo(b, offset); offset += 4;
	        idBytes.CopyTo(b, offset);			offset += idBytes.Length;
	        b[offset] = (byte) 0x3D;			offset++;// "="
	        contentBytes.CopyTo(b, offset);

        	return b;
		}
	}
}
