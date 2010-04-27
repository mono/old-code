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

using System.IO;
using Entagged.Audioformats.Exceptions;

namespace Entagged.Audioformats.Ogg.Util {
	public class VorbisTagReader {
		
		private OggTagReader oggTagReader = new OggTagReader();
		
		public Tag Read( Stream raf ) {
			long oldPos = 0;
			//----------------------------------------------------------
			
			//Check wheter we have an ogg stream---------------
			raf.Seek( 0 , SeekOrigin.Begin);
			byte[] b = new byte[4];
			raf.Read(b, 0, b.Length);
			
			string ogg = new string(System.Text.Encoding.ASCII.GetChars(b));
			if( ogg != "OggS" )
				throw new CannotReadException("OggS Header could not be found, not an ogg stream");
			//--------------------------------------------------
			
			//Parse the tag ------------------------------------
			raf.Seek( 0 , SeekOrigin.Begin);

			//Supposing 1st page = codec infos
			//			2nd page = comment+decode info
			//...Extracting 2nd page
			
			//1st page to get the length
			b = new byte[4];
			oldPos = raf.Position;
			raf.Seek(26, SeekOrigin.Begin);
			int pageSegments = raf.ReadByte()&0xFF; //unsigned
			raf.Seek(oldPos, SeekOrigin.Begin);
			
			b = new byte[27 + pageSegments];
			raf.Read( b , 0,  b .Length);

			OggPageHeader pageHeader = new OggPageHeader( b );

			raf.Seek( raf.Position + pageHeader.PageLength , SeekOrigin.Begin);

			//2nd page extraction
			oldPos = raf.Position;
			raf.Seek(raf.Position + 26, SeekOrigin.Begin);
			pageSegments = raf.ReadByte()&0xFF; //unsigned
			raf.Seek(oldPos, SeekOrigin.Begin);
			
			b = new byte[27 + pageSegments];
			raf.Read( b , 0,  b .Length);
			pageHeader = new OggPageHeader( b );

			b = new byte[7];
			raf.Read( b , 0,  b .Length);
			
			string vorbis = new string(System.Text.Encoding.ASCII.GetChars(b, 1, 6));
			if(b[0] != 3 || vorbis != "vorbis")
				throw new CannotReadException("Cannot find comment block (no vorbis header)");

			//Begin tag reading
			OggTag tag = oggTagReader.Read(raf);
			
			byte isValid = (byte) raf.ReadByte();
			if ( isValid == 0 )
				throw new CannotReadException("Error: The OGG Stream isn't valid, could not extract the tag");
			
			return tag;
		}
	}
}
