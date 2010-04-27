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
using Entagged.Audioformats.Util;
using Entagged.Audioformats.Exceptions;

namespace Entagged.Audioformats.Ape.Util {
	public class ApeTagWriter {
		
		private ApeTagCreator tc = new ApeTagCreator();
		
		public void Delete(Stream apeStream) {
			if(!TagExists(apeStream))
				return;
			
			apeStream.Seek( -20 , SeekOrigin.End);
			
			byte[] b = new byte[4];
			apeStream.Read( b , 0,  b.Length);
			long tagSize = Utils.GetLongNumber(b, 0, 3);
			
			apeStream.SetLength(apeStream.Length - tagSize);
			
			/* Check for a second header */
			if(!TagExists(apeStream))
				return;
			
			apeStream.SetLength(apeStream.Length - 32);
		}
		
		public void Write(Tag tag, Stream apeStream, Stream tempStream) {
			ByteBuffer tagBuffer = tc.Create(tag);

			if (!TagExists(apeStream)) {
				apeStream.Seek(0, SeekOrigin.End);
				apeStream.Write(tagBuffer.Data, 0, tagBuffer.Capacity);
			} else {
				apeStream.Seek( -32 + 8 , SeekOrigin.End);
			
				//Version
				byte[] b = new byte[4];
				apeStream.Read( b , 0,  b.Length);
				int version = Utils.GetNumber(b, 0, 3);
				if(version != 2000) {
					throw new CannotWriteException("APE Tag other than version 2.0 are not supported");
				}
				
				//Size
				b = new byte[4];
				apeStream.Read( b , 0,  b.Length);
				long oldSize = Utils.GetLongNumber(b, 0, 3) + 32;
				int tagSize = tagBuffer.Capacity;
				
				apeStream.Seek(-oldSize, SeekOrigin.End);
				apeStream.Write(tagBuffer.Data, 0, tagBuffer.Capacity);
					
				if(oldSize > tagSize) {
					//Truncate the file
					apeStream.SetLength(apeStream.Length - (oldSize-tagSize));
				}
			}
		}
		
		private bool TagExists(Stream apeStream) {
			apeStream.Seek( -32 , SeekOrigin.End);
			
			byte[] b = new byte[8];
			apeStream.Read(b, 0, b.Length);
			
			return new string(System.Text.Encoding.ASCII.GetChars(b)) == "APETAGEX";
		}
	}
}
