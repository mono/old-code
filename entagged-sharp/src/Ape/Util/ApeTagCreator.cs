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

namespace Entagged.Audioformats.Ape.Util {
	public class ApeTagCreator : AbstractTagCreator {
		protected override void Create(Tag tag, ByteBuffer buf, IList fields, int tagSize, int paddingSize) {
			//APETAGEX------------------
			buf.Put(Utils.GetBytes("APETAGEX"));
			
			//Version 2.0 (aka 2000)
			buf.Put( new byte[] {(byte)0xD0,0x07,0x00,0x00} );
			
			//Tag size
			int size = tagSize - 32;
			buf.Put(Utils.GetNumber(size));
			
			//Number of fields
			int listLength = fields.Count;
			buf.Put(Utils.GetNumber(listLength));
			
			//Flags
			buf.Put( new byte[] {0x00,0x00,0x00,(byte)0xA0} );
			//means: We have a header and a footer, this is the header
			
			//Reserved 8-bytes 0x00
			buf.Put( new byte[] {0,0,0,0,0,0,0,0} );
			
			//Now each field is saved:
			foreach(byte[] field in fields) {
				buf.Put(field);
			}
			
			//APETAGEX------------------
			buf.Put(Utils.GetBytes("APETAGEX"));  //APETAGEX
			
			//Version 2.0 (aka 2000)
			buf.Put( new byte[] {(byte)0xD0,0x07,0x00,0x00} );
			
			//Tag size
			buf.Put(Utils.GetNumber(size));
			
			//Number of fields
			buf.Put(Utils.GetNumber(listLength));
			
			//Flags
			buf.Put( new byte[] {0x00,0x00,0x00,(byte)0x80} );
			//means: We have a header and a footer, this is the footer
			
			//Reserved 8-bytes 0x00
			buf.Put( new byte[] {0,0,0,0,0,0,0,0} );
			
			//----------------------------------------------------------------------------
		}
		
		protected override int GetFixedTagLength(Tag tag) {
		    return 64;
		}
		
		protected override byte[] CreateField(string id, string content) {
			byte[] idBytes = Utils.GetBytes(id, "ISO-8859-1");
	        byte[] contentBytes = Utils.GetBytes(content, "UTF-8");
			byte[] buf = new byte[4 + 4 + idBytes.Length + 1 + contentBytes.Length];
			byte[] flags = {0x00,0x00,0x00,0x00};
			
			int offset = 0;
			Utils.GetNumber(contentBytes.Length).CopyTo(buf, offset);	offset += 4;
			flags.CopyTo(buf, offset);                       			offset += 4;
			idBytes.CopyTo(buf, offset);                     			offset += idBytes.Length;
			buf[offset] = 0;                               				offset += 1;
			contentBytes.CopyTo(buf, offset);                			offset += contentBytes.Length;
			
			return buf;
		}
	}
}
