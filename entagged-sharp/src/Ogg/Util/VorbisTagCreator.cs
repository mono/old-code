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
 
namespace Entagged.Audioformats.Ogg.Util {
	public class VorbisTagCreator {
		private OggTagCreator creator = new OggTagCreator();
		
		//Creates the ByteBuffer for the ogg tag
		public ByteBuffer Convert(Tag tag) {
			ByteBuffer ogg = creator.Create(tag);
			int tagLength = ogg.Capacity + 8;
			
			ByteBuffer buf = new ByteBuffer(tagLength);
			
			//[packet type=comment0x03]['vorbis']
			buf.Put( new byte[]{(byte) 0x03, (byte) 0x76, (byte) 0x6f, (byte) 0x72, (byte) 0x62, (byte) 0x69, (byte) 0x73} );
			
			//The actual tag
			buf.Put(ogg);

			//Framing bit = 1
			buf.Put( (byte) 0x01 );

			buf.Rewind();
			return buf;
		}
	}
}
