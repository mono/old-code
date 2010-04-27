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

namespace Entagged.Audioformats.Mp3.Util {
	public class XingMPEGFrame {

		/**  the filesize in bytes */
		private int fileSize = 0;

		/**  The number of mpeg frames in the mpeg file */
		private int frameCount = 0;

		/**  Flag to determine if it is a valid Xing Mpeg frame */
		private bool isValidXingMPEGFrame = true;

		/**  the Xing Encoding quality (0-100) */
		private int quality;

		/**  The four flags for this type of mpeg frame */
		private bool[] vbrFlags = new bool[4];

		private bool vbr = false;

		public XingMPEGFrame( byte[] bytesPart1, byte[] bytesPart2 ) {
			string xing = new string( System.Text.Encoding.ASCII.GetChars(bytesPart1, 0, 4) );

			if ( xing ==  "Xing" || xing ==  "Info" ) {
				vbr = (xing ==  "Xing");
				int[] b = u(bytesPart1);
				int[] q = u(bytesPart2);

				UpdateVBRFlags(b[7]);

				if ( vbrFlags[0] )
					frameCount = b[8] * 16777215 + b[9] * 65535 + b[10] * 255 + b[11];
				if ( vbrFlags[1] )
					fileSize = b[12] * 16777215 + b[13] * 65535 + b[14] * 255 + b[15];
				if ( vbrFlags[3] )
					quality = q[0] * 16777215 + q[1] * 65535 + q[2] * 255 + q[3];
			}
			else
				//No frame VBR MP3 XING
				isValidXingMPEGFrame = false;

		}
		
		private int[] u(byte[] b) {
			int[] i = new int[b.Length];
			for(int j = 0; j<i.Length; j++)
				i[j] = b[j] & 0xFF;
			return i;
		}

		public int FrameCount {
			get {
				if ( vbrFlags[0] )
					return frameCount;
				
				return -1;
			}
		}

		public bool Valid {
			get { return isValidXingMPEGFrame; }
		}

		public bool IsVbr {
	    	get { return vbr; }
		}
		
		public int FileSize {
		    get { return this.fileSize; }
		}

		public override string ToString() {
			string output;

			if ( isValidXingMPEGFrame ) {
				output = "\n----XingMPEGFrame--------------------\n";
				output += "Frame count:" + vbrFlags[0] + "\tFile Size:" + vbrFlags[1] + "\tQuality:" + vbrFlags[3] + "\n";
				output += "Frame count:" + frameCount + "\tFile Size:" + fileSize + "\tQuality:" + quality + "\n";
				output += "--------------------------------\n";
			}
			else
				output = "\n!!!No Valid Xing MPEG Frame!!!\n";
			return output;
		}

		private void UpdateVBRFlags(int b) {
			vbrFlags[0] = (b&0x01) == 0x01;
			vbrFlags[1] = (b&0x02) == 0x02;
			vbrFlags[2] = (b&0x04) == 0x04;
			vbrFlags[3] = (b&0x08) == 0x08;
		}
	}
} 
