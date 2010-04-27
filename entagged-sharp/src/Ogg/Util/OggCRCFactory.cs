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
	public class OggCRCFactory {

		private static long[] crc_lookup = new long[256];
		private static bool inited = false;


		public static void Init() {
			for ( int i = 0; i < 256; i++ ) {
				long r = i << 24;

				for ( int j = 0; j < 8; j++ )
					if ( ( r & 0x80000000L ) != 0 )
						r = ( r << 1 ) ^ 0x04c11db7L;
					else
						r <<= 1;

				crc_lookup[i] = ( r & 0xffffffff );
			}
			inited = true;
		}


		public bool CheckCRC( byte[] data, byte[] crc ) {
			byte[] computedCrc = ComputeCRC(data);
			if (crc.Length != computedCrc.Length)
				return false;
				
			for (int i = 0; i<computedCrc.Length; i++) {
				if (computedCrc[i] != crc[i])
					return false;
			}
			return true;
		}

		public static byte[] ComputeCRC( byte[] data ) {
			if(!inited)
				Init();
			
			long crc_reg = 0;

			for ( int i = 0; i < data.Length; i++ ) {
				int tmp = (int) ( ( ( crc_reg >> 24 ) & 0xff ) ^ (data[i]&0xff) );

				crc_reg = ( crc_reg << 8 ) ^ crc_lookup[tmp];
				crc_reg &= 0xffffffff;
			}

			byte[] sum = new byte[4];

			sum[0] = (byte) ( crc_reg & 0xffL );
			sum[1] = (byte) ( ( crc_reg >> 8 ) & 0xffL );
			sum[2] = (byte) ( ( crc_reg >> 16 ) & 0xffL );
			sum[3] = (byte) ( ( crc_reg >> 24 ) & 0xffL );

			return sum;
		}
	}
}
