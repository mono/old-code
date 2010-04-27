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

using Entagged.Audioformats.Util;

namespace Entagged.Audioformats.Ape.Util {
	public class ApeTagBinaryField : ApeTagField  {
	    
	    private byte[] content;

	    public ApeTagBinaryField(string id, byte[] content) : base(id, true) {
	        this.content = new byte[content.Length];
	        for(int i = 0; i<content.Length; i++)
	            this.content[i] = content[i];
	    }
	    
	    public override bool IsEmpty {
	        get { return this.content.Length == 0; }
	    }
	    
	    public override string ToString() {
	        return Id + " : Cannot represent this";
	    }
	    
	    public override void CopyContent(TagField field) {
	        if(field is ApeTagBinaryField) {
	            this.content = (field as ApeTagBinaryField).Content;
	        }
	    }
	    
	    public byte[] Content {
	        get { return this.content; }
	    }
	    
	    public override byte[] RawContent {
	        get {
		        byte[] idBytes = GetBytes(Id, "ISO-8859-1");
		        byte[] buf = new byte[4 + 4 + idBytes.Length + 1 + content.Length];
				byte[] flags = {0x02,0x00,0x00,0x00};
				
				int offset = 0;
				Copy(GetSize(content.Length), buf, offset);    offset += 4;
				Copy(flags, buf, offset);                      offset += 4;
				Copy(idBytes, buf, offset);                    offset += idBytes.Length;
				buf[offset] = 0;                               offset += 1;
				Copy(content, buf, offset);                    offset += content.Length;
				
				return buf;
			}
	    }
	}
}
