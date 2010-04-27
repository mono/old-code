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

namespace Entagged.Audioformats.Flac.Util {
	public class MetadataBlockDataStreamInfo {
		
		private int samplingRate,length,bitsPerSample,channelNumber;
		private bool isValid = true;
		
		public MetadataBlockDataStreamInfo(byte[] b) {
			if(b.Length<19) {
				isValid = false;
				return;
			}
			
			samplingRate = ReadSamplingRate(b[10], b[11], b[12] );
			
			channelNumber = ((u(b[12])&0x0E)>>1) + 1;
			samplingRate = samplingRate / channelNumber;
			
			bitsPerSample = ((u(b[12])&0x01)<<4) + ((u(b[13])&0xF0)>>4) + 1;
			
			int sampleNumber = ReadSampleNumber(b[13], b[14], b[15], b[16], b[17]);
			
			length = sampleNumber / samplingRate;
		}
		
		public int Length {
			get { return length; }
		}
		
		public int ChannelNumber {
			get { return channelNumber; }
		}
		
		public int SamplingRate {
			get { return samplingRate; }
		}
		
		public string EncodingType {
			get { return "FLAC "+bitsPerSample+" bits"; }
		}
		
		public bool Valid {
			get { return isValid; }
		}
		

		private int ReadSamplingRate(byte b1, byte b2, byte b3) {
			int rate = (u(b3)&0xF0)>>3;
			rate += u(b2)<<5;
			rate += u(b1)<<13;
			return rate;
		}
		
		private int ReadSampleNumber(byte b1, byte b2, byte b3, byte b4, byte b5) {
			int nb = u(b5);
			nb += u(b4)<<8;
			nb += u(b3)<<16;
			nb += u(b2)<<24;
			nb += (u(b1)&0x0F)<<32;
			return nb;
		}
		
		private int u(int i) {
			return i & 0xFF;
		}
	}
}
