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
using System.Text;
using Entagged.Audioformats.Exceptions;
using Entagged.Audioformats.Util;

namespace Entagged.Audioformats.Ape.Util {
	public class ApeTagReader {

		public Tag Read(Stream apeStream)
		{
			Tag tag = new Tag();
			
			//Check wether the file contains an APE tag--------------------------------
			apeStream.Seek( apeStream.Length - 32 , SeekOrigin.Begin);
			
			byte[] b = new byte[8];
			apeStream.Read(b, 0, b.Length);
			
			string tagS = new string( System.Text.Encoding.ASCII.GetChars(b) );
			if(tagS != "APETAGEX" ){
				throw new CannotReadException("There is no APE Tag in this file");
			}
			//Parse the tag -)------------------------------------------------
			//Version
			b = new byte[4];
			apeStream.Read( b , 0,  b .Length);
			int version = Utils.GetNumber(b, 0,3);
			if(version != 2000) {
				throw new CannotReadException("APE Tag other than version 2.0 are not supported");
			}
			
			//Size
			b = new byte[4];
			apeStream.Read( b , 0,  b .Length);
			long tagSize = Utils.GetLongNumber(b, 0,3);

			//Number of items
			b = new byte[4];
			apeStream.Read( b , 0,  b .Length);
			int itemNumber = Utils.GetNumber(b, 0,3);
			
			//Tag Flags
			b = new byte[4];
			apeStream.Read( b , 0,  b .Length);
			//TODO handle these
			
			apeStream.Seek(apeStream.Length - tagSize, SeekOrigin.Begin);
			
			for(int i = 0; i<itemNumber; i++) {
				//Content length
				b = new byte[4];
				apeStream.Read( b , 0,  b .Length);
				int contentLength = Utils.GetNumber(b, 0,3);
				if(contentLength > 500000)
					throw new CannotReadException("Item size is much too large: "+contentLength+" bytes");
				
				//Item flags
				b = new byte[4];
				apeStream.Read( b , 0,  b .Length);
				//TODO handle these
				bool binary = ((b[0]&0x06) >> 1) == 1;
				
				int j = 0;
				while(apeStream.ReadByte() != 0)
					j++;
				apeStream.Seek(apeStream.Position - j -1, SeekOrigin.Begin);
				int fieldSize = j;
				
				//Read Item key
				b = new byte[fieldSize];
				apeStream.Read( b , 0,  b .Length);
				apeStream.Seek(1, SeekOrigin.Current);
				string field = new string(System.Text.Encoding.GetEncoding("ISO-8859-1").GetChars(b));

				//Read Item content
				b = new byte[contentLength];
				apeStream.Read( b , 0,  b .Length);
				if(!binary) {
					string content = Encoding.UTF8.GetString(b);
					switch (field) {
					case "Track":
						string num, count;
						Utils.SplitTrackNumber(content, out num, out count);
						if (num != null)
							tag.AddTrack(num);
						if (count != null)
							tag.AddTrackCount(count);
						break;

					default:
					    tag.Add(field, content);
						break;
					}
					
				} //else FIXME
				   // tag.Add(new ApeTagBinaryField(field, b));
			}
			
			return tag;
		} 
	}
}
