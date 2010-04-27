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

namespace Entagged.Audioformats.Ogg.Util {
	public class VorbisCodecHeader {
		private int audioChannels;
		private bool isValid = false;
		
		private int vorbisVersion, audioSampleRate;
		private int bitrateMinimal ,bitrateNominal,bitrateMaximal;

		public VorbisCodecHeader( byte[] vorbisData ) {
			GenerateCodecHeader( vorbisData );
		}


		public int ChannelNumber {
			get { return audioChannels; }
		}


		public string EncodingType {
			get { return "Ogg Vorbis Version " + vorbisVersion; }
		}


		public int SamplingRate {
			get { return audioSampleRate; }
		}


		public bool Valid {
			get { return isValid; }
		}

		public int NominalBitrate {
		    get { return bitrateNominal; }
		}
	
		public int MaxBitrate {
		    get { return bitrateMaximal; }
		}
		
		public int MinBitrate {
		    get { return bitrateMinimal; }
		}

		public void GenerateCodecHeader( byte[] b ) {
			int packetType = b[0];

			string vorbis = new string( System.Text.Encoding.ASCII.GetChars(b, 1, 6 ));

			if ( packetType == 1 && vorbis == "vorbis" ) {
				this.vorbisVersion = b[7] + ( b[8] << 8 ) + ( b[9] << 16 ) + ( b[10] << 24 );

				this.audioChannels = u( b[11] );

				this.audioSampleRate = u( b[12] ) + ( u( b[13] ) << 8 ) + ( u( b[14] ) << 16 ) + ( u( b[15] ) << 24 );
				
				this.bitrateMinimal = u(b[16]) + ( u(b[17]) << 8 ) + ( u(b[18]) << 16 ) + ( u(b[19]) << 24 );
				this.bitrateNominal = u(b[20]) + ( u(b[21]) << 8 ) + ( u(b[22]) << 16 ) + ( u(b[23]) << 24 );
				this.bitrateMaximal = u(b[24]) + ( u(b[25]) << 8 ) + ( u(b[26]) << 16 ) + ( u(b[27]) << 24 );
			
			
				int framingFlag = b[29];

				if ( framingFlag != 0 )
					isValid = true;

			}
		}
		
		private int u(int i) {
			return i & 0xFF;
		}
	}
}
