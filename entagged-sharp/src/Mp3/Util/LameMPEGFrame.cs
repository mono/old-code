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
	public class LameMPEGFrame {

		/**  contains the Bitrate of this frame */
		private int bitrate;

		/**  Flag indicating if bitset contains a Lame Frame */
		private bool containsLameMPEGFrame;

		/**  Contains the Filesize in bytes of the frame's File */
		private int fileSize;

		/**  Flag indicating if this is a correct Lame Frame */
		private bool isValidLameMPEGFrame = false;

		/**  Contains the Lame Version number of this frame */
		private string lameVersion;

		/**  Contains the bitset representing this Lame Frame */
		private bool containsLameFrame = false;


		/**
		 *  Creates a Lame Mpeg Frame and checks it's integrity
		 *
		 * @param  lameHeader  a byte array representing the Lame frame
		 */
		public LameMPEGFrame( byte[] lameHeader ) {
			string xing = new string( System.Text.Encoding.ASCII.GetChars(lameHeader, 0, 4) );

			if ( xing == "LAME" ) {
				isValidLameMPEGFrame = true;

				int[] b = u( lameHeader );

				containsLameFrame = ( (b[9]&0xFF) == 0xFF  );

				byte[] version = new byte[5];

				version[0] = lameHeader[4];
				version[1] = lameHeader[5];
				version[2] = lameHeader[6];
				version[3] = lameHeader[7];
				version[4] = lameHeader[8];
				lameVersion = new string( System.Text.Encoding.ASCII.GetChars(version) );

				containsLameMPEGFrame = _containsLameMPEGFrame();

				if ( containsLameMPEGFrame ) {
					bitrate = b[20];
					fileSize = b[28] * 16777215 + b[29] * 65535 + b[30] * 255 + b[31];
				}
			}
			else
				//Pas de frame VBR MP3 Lame
				isValidLameMPEGFrame = false;

		}
		
		private int[] u(byte[] b) {
			int[] i = new int[b.Length];
			for(int j = 0; j<i.Length; j++)
				i[j] = b[j] & 0xFF;
			return i;
		}


		public bool Valid {
			get { return isValidLameMPEGFrame; }
		}


		public override string ToString() {
			string output;

			if ( isValidLameMPEGFrame ) {
				output = "\n----LameMPEGFrame--------------------\n";
				output += "Lame" + lameVersion;
				if ( containsLameMPEGFrame )
					output += "\tMin.Bitrate:" + bitrate + "\tLength:" + fileSize;
				output += "\n--------------------------------\n";
			}
			else
				output = "\n!!!No Valid Lame MPEG Frame!!!\n";
			return output;
		}

		private bool _containsLameMPEGFrame() {
			return containsLameFrame;
		}
	}
}
