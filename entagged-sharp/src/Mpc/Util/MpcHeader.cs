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

using System.Text;
using Entagged.Audioformats.Util;

namespace Entagged.Audioformats.Mpc.Util {
	public class MpcHeader {
		
		byte[] b;
		public MpcHeader(byte[] b) {
			this.b = b;
		}
		
		public int SamplesNumber {
			get {
				if(b[0] == 7)
					return Utils.GetNumber(b, 1,4);

				return -1;
			}	
		}
		
		public int SamplingRate {
			get {
				if(b[0] == 7) {
					switch (b[6] & 0x02) {
						case 0: return 44100;
						case 1: return 48000;
		                case 2: return 37800;
		                case 3: return 32000;
		                default: return -1;
					}
				}
				
				return -1;
			}
		}
		
		public int ChannelNumber {
			get {
				if(b[0] == 7)
					return 2;
				
				return 2;
			}
		}
		
		public string EncodingType {
			get {
				StringBuilder sb = new StringBuilder().Append("MPEGplus (MPC)");
				if(b[0] == 7) {
					sb.Append(" rev.7, Profile:");
					switch ((b[7] & 0xF0) >> 4) {
						case 0: sb.Append( "No profile"); break;
						case 1: sb.Append( "Unstable/Experimental"); break;
						case 2: sb.Append( "Unused"); break;
						case 3: sb.Append( "Unused"); break;
						case 4: sb.Append( "Unused"); break;
						case 5: sb.Append( "Below Telephone (q= 0.0)"); break;
						case 6: sb.Append( "Below Telephone (q= 1.0)"); break;
						case 7: sb.Append( "Telephone (q= 2.0)"); break;
						case 8: sb.Append( "Thumb (q= 3.0)"); break;
						case 9: sb.Append( "Radio (q= 4.0)"); break;
						case 10: sb.Append( "Standard (q= 5.0)"); break;
						case 11: sb.Append( "Xtreme (q= 6.0)"); break;
						case 12: sb.Append( "Insane (q= 7.0)"); break;
						case 13: sb.Append( "BrainDead (q= 8.0)"); break;
						case 14: sb.Append( "Above BrainDead (q= 9.0)"); break;
						case 15: sb.Append( "Above BrainDead (q=10.0)"); break;
						default: sb.Append("No profile"); break;
					}
				}
				
				return sb.ToString();
			}
		}
		
		public string EncoderInfo {
			get {
				int encoder = b[24];
				StringBuilder sb = new StringBuilder().Append("Mpc encoder v").Append(((double)encoder)/100).Append(" ");
				if(encoder % 10 == 0)
					sb.Append("Release");
				else if(encoder %  2 == 0)
					sb.Append("Beta");
				else if(encoder %  2 == 1)
					sb.Append("Alpha");
				
				return sb.ToString();
			}
		}

	}
}
