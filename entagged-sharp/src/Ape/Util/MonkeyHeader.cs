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
	public class MonkeyHeader {
		
		byte[] b;
		public MonkeyHeader(byte[] b) {
			this.b = b;
		}
		
		public int CompressionLevel {
			get { return Utils.GetNumber(b, 0, 1); }
		}
		
		public int FormatFlags {
			get { return Utils.GetNumber(b, 2,3); }
		}
		
		public long BlocksPerFrame {
			get { return Utils.GetLongNumber(b, 4,7); }
		}
		
		public long FinalFrameBlocks {
			get { return Utils.GetLongNumber(b, 8,11); }
		}
		
		public long TotalFrames {
			get { return Utils.GetLongNumber(b, 12,15); }
		}
	    
		public int Length {
			get { return (int) (BlocksPerFrame * (TotalFrames - 1.0) + FinalFrameBlocks) / SamplingRate; }
		}

		public int BitsPerSample {
			get { return Utils.GetNumber(b, 16,17); }
		}
		
		public int ChannelNumber {
			get { return Utils.GetNumber(b, 18,19); }
		}
		
		public int SamplingRate {
			get { return Utils.GetNumber(b, 20,23); }
			
		}
	}
}
