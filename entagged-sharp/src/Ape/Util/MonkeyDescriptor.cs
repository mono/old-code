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
	public class MonkeyDescriptor {
		
		byte[] b;
		public MonkeyDescriptor(byte[] b) {
			this.b = b;
		}
		
		public int RiffWavOffset {
			get { return DescriptorLength + HeaderLength + SeekTableLength; }
		}
		
		public int DescriptorLength {
			get { return Utils.GetNumber(b, 0,3); }
		}
		
		public int HeaderLength {
			get { return Utils.GetNumber(b, 4,7); }
		}
		
		public int SeekTableLength {
			get { return Utils.GetNumber(b, 8,11); }
		}
		
		public int RiffWavLength {
			get { return Utils.GetNumber(b, 12,15); }
		}
	    
		public long ApeFrameDataLength {
			get { return Utils.GetLongNumber(b, 16,19); }
		}
		
		public long ApeFrameDataHighLength {
			get { return Utils.GetLongNumber(b, 20,23); }
		}
		
		public int TerminatingDataLength {
			get { return Utils.GetNumber(b, 24,27); }
		}
	    
	    //16 bytes cFileMD5 b[28->43]
	}
}
