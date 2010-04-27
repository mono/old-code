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
using Entagged.Audioformats.Ogg.Util;
using Entagged.Audioformats.Ogg;

namespace Entagged.Audioformats.Flac.Util {
	public class FlacTagReader {
		
		private OggTagReader oggTagReader = new OggTagReader();
		
		public OggTag Read( Stream flacStream )
		{
			//Begins tag parsing-------------------------------------
			if ( flacStream.Length==0 ) {
				//Empty File
				throw new CannotReadException("Error: File empty");
			}
			flacStream.Seek( 0 , SeekOrigin.Begin);

			//FLAC Header string
			byte[] b = new byte[4];
			flacStream.Read(b, 0, b.Length);
			string flac = new string(System.Text.Encoding.ASCII.GetChars(b));
			if(flac != "fLaC")
				throw new CannotReadException("fLaC Header not found, not a flac file");
			
			OggTag tag = null;
			
			//Seems like we hava a valid stream
			bool isLastBlock = false;
			while(!isLastBlock) {
				b = new byte[4];
				flacStream.Read(b, 0, b.Length);
				MetadataBlockHeader mbh = new MetadataBlockHeader(b);
			
				switch(mbh.BlockType) {
					//We got a vorbis comment block, parse it
					case MetadataBlockHeader.BlockTypes.VorbisComment : 
						tag = HandleVorbisComment(mbh, flacStream);
						mbh = null;
						return tag; //We have it, so no need to go further
					
					//This is not a vorbis comment block, we skip to next block
					default : 
						flacStream.Seek(flacStream.Position+mbh.DataLength, SeekOrigin.Begin);
						break;
				}

				isLastBlock = mbh.IsLastBlock;
				mbh = null;
			}
			//FLAC not found...
			throw new CannotReadException("FLAC Tag could not be found or read..");
		}
		
		private OggTag HandleVorbisComment(MetadataBlockHeader mbh, Stream flacStream) {
			long oldPos = flacStream.Position;
			
			OggTag tag = oggTagReader.Read(flacStream);
			
			long newPos = flacStream.Position;
			
			if(newPos - oldPos != mbh.DataLength)
				throw new CannotReadException("Tag length do not match with flac comment data length");
			
			return tag;
		}
	}
}
