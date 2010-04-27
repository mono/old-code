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

using Entagged.Audioformats.Ogg.Util;
using Entagged.Audioformats.Util;

namespace Entagged.Audioformats.Flac.Util {
	public class FlacTagCreator {
		public const int DEFAULT_PADDING = 4000;
		private OggTagCreator creator = new OggTagCreator();
		
		//Creates the ByteBuffer for the ogg tag
		public ByteBuffer Create(Tag tag, int paddingSize) {
			ByteBuffer ogg = creator.Create(tag);
			int tagLength = ogg.Capacity + 4;
			
			ByteBuffer buf = new ByteBuffer( tagLength + paddingSize );

			//CREATION OF CVORBIS COMMENT METADATA BLOCK HEADER
			//If we have padding, the comment is not the last block (bit[0] = 0)
			//If there is no padding, the comment is the last block (bit[0] = 1)
			byte type =  (paddingSize > 0) ? (byte)0x04 : (byte) 0x84;
			buf.Put(type);
			int commentLength = tagLength - 4; //Comment length
			buf.Put( new byte[] { (byte)((commentLength & 0xFF0000) >> 16), (byte)((commentLength & 0xFF00) >> 8) , (byte)(commentLength&0xFF)  } );

			//The actual tag
			buf.Put(ogg);
			
			//PADDING
			if(paddingSize >=4) {
				int paddingDataSize = paddingSize - 4;
				buf.Put((byte)0x81); //Last frame, padding 0x81
				buf.Put(new byte[]{ (byte)((paddingDataSize&0xFF0000)>>16),(byte)((paddingDataSize&0xFF00)>>8),(byte)(paddingDataSize&0xFF) });
				for(int i = 0; i< paddingDataSize; i++)
					buf.Put((byte)0);
			}
			buf.Rewind();
			
			return buf;
		}
		
		public int GetTagLength(Tag tag) {
			ByteBuffer ogg = creator.Create(tag);
			return ogg.Capacity + 4;
		}
	}
}
