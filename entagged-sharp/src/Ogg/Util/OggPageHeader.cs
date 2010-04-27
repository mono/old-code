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
	public class OggPageHeader {
		private double absoluteGranulePosition;
		private byte[] checksum;
		private byte headerTypeFlag;

		private bool isValid = false;
		private int pageLength = 0;
		private int pageSequenceNumber,streamSerialNumber;
		private byte[] segmentTable;

		public OggPageHeader( byte[] b ) {
			int streamStructureRevision = b[4];

			headerTypeFlag = b[5];

			if ( streamStructureRevision == 0 ) {
				this.absoluteGranulePosition = 0;
				for ( int i = 0; i < 8; i++ )
					this.absoluteGranulePosition += u( b[i + 6] ) * System.Math.Pow(2, 8 * i);

				streamSerialNumber = u(b[14]) + ( u(b[15]) << 8 ) + ( u(b[16]) << 16 ) + ( u(b[17]) << 24 );
				
				pageSequenceNumber = u(b[18]) + (u(b[19]) << 8 ) + ( u(b[20]) << 16 ) + ( u(b[21]) << 24 );
				
				checksum = new byte[]{b[22], b[23], b[24], b[25]};

				this.segmentTable = new byte[b.Length - 27];
				
				for ( int i = 0; i < segmentTable.Length; i++ ) {
					segmentTable[i] = b[27 + i];
					this.pageLength += u( b[27 + i] );
				}

				isValid = true;
			}
		}
		
		private int u(int i) {
			return i & 0xFF;
		}


		public double AbsoluteGranulePosition {
			get { return this.absoluteGranulePosition; }
		}


		public byte[] CheckSum {
			get { return checksum; }
		}


		public byte HeaderType {
			get { return headerTypeFlag; }
		}


		public int PageLength {
			get { return this.pageLength; }
		}
		
		public int PageSequence {
			get { return pageSequenceNumber; }
		}
		
		public int SerialNumber {
			get { return streamSerialNumber; }
		}

		public byte[] SegmentTable {
		    get { return this.segmentTable; }
		}

		public bool Valid {
			get { return isValid; }
		}

		public override string ToString() {
			string s = "Ogg Page Header:\n";

			s += "Is valid?: " + isValid + " | page length: " + pageLength + "\n";
			s += "Header type: " + headerTypeFlag;
			return s;
		}
	}
}
