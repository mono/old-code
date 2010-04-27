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

namespace Entagged.Audioformats.Mp3.Util {
	public class Id3v2TagSynchronizer {

	    public ByteBuffer synchronize(ByteBuffer b)
	    {
	        ByteBuffer bb = new ByteBuffer(b.Capacity);
	        
	        while(b.Remaining >= 1) {
	        	byte cur = b.Get();
	            bb.Put(cur);
	            
	            if((cur&0xFF) == 0xFF && b.Remaining >=1 && b.Peek() == 0x00) {
	            	//First part of synchronization
	                b.Get();
	            }
	        }
	        
	        //We have finished filling the new bytebuffer, so set the limit, and rewind
	        bb.Limit = bb.Position;
	        bb.Rewind();
	        
	        return bb;
	    }

	}
}
