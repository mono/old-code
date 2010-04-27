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

using Entagged.Audioformats;
using Entagged.Audioformats.Exceptions;
using Entagged.Audioformats.Ogg;
using System.Collections;
using System.IO;

namespace Entagged.Audioformats.Flac.Util {
	public class FlacTagWriter {
		
		private FlacTagCreator tc = new FlacTagCreator();
		private FlacTagReader reader = new FlacTagReader();
		
		public void Delete(Stream raf, Stream tempRaf) {
			OggTag tag = null;
			try {
				tag = reader.Read(raf);
			} catch(CannotReadException e) {
				Write(new OggTag(), raf, tempRaf);
				return;
			}
			
			OggTag emptyTag = new OggTag();
			if (tag.HasField("vendor"))
				emptyTag.Add("vendor", tag.Get("vendor")[0] as string);
			
			raf.Seek(0, SeekOrigin.Begin);
			tempRaf.Seek(0, SeekOrigin.Begin);
			
			Write(emptyTag, raf, tempRaf);
		}
		
		public void Write( Tag tag, Stream raf, Stream rafTemp ) {
			//Clean up old datas
			ArrayList metadataBlockPadding = new ArrayList(1);
			ArrayList metadataBlocks = new ArrayList();
			
			byte[] b = new byte[4];
			raf.Read(b, 0, b.Length);
			if(new string(System.Text.Encoding.ASCII.GetChars(b)) != "fLaC")
				throw new CannotWriteException("This is not a FLAC file");
			
			bool isLastBlock = false;
			while(!isLastBlock) {
				b = new byte[4];
				raf.Read(b, 0, 4);
				MetadataBlockHeader mbh = new MetadataBlockHeader(b);
									
				if (mbh.BlockType == MetadataBlockHeader.BlockTypes.Padding) {
					raf.Seek(mbh.DataLength, SeekOrigin.Current);
					metadataBlockPadding.Add(mbh.DataLength+4);
				}
				else {
					b = new byte[mbh.DataLength];
					raf.Read(b, 0, b.Length);
					metadataBlocks.Add(new MetadataBlockData(mbh, b));
				}
			
				isLastBlock = mbh.IsLastBlock;
			}
			
			int availableRoom =  ComputeAvailableRoom(metadataBlockPadding, metadataBlocks);
			int newTagSize = tc.GetTagLength(tag);
			int neededRoom = newTagSize + ComputeNeededRoom(metadataBlockPadding, metadataBlocks);
			raf.Seek(0, SeekOrigin.Begin);
			
			if(availableRoom>=neededRoom) {
				//OVERWRITE EXISTING TAG
				raf.Seek(42, SeekOrigin.Begin);
				
				foreach (MetadataBlockData data in metadataBlocks) {
					raf.Write(data.Header.Data, 0, data.Header.Data.Length);
					raf.Write(data.Bytes, 0, data.Bytes.Length);
				}
				
				byte[] newTagBytes = tc.Create(tag, availableRoom-neededRoom).Data;
				raf.Write(newTagBytes, 0, newTagBytes.Length);
			} else {
				//create new tag with padding (we remove the header and keep the audio data)
				b = new byte[42];
				raf.Read(b, 0, b.Length);
				raf.Seek(availableRoom+42, SeekOrigin.Begin);
				
				rafTemp.Write(b, 0, b.Length);
				
				foreach (MetadataBlockData data in metadataBlocks) {
					raf.Write(data.Header.Data, 0, data.Header.Data.Length);
					raf.Write(data.Bytes, 0, data.Bytes.Length);
				}
				
				byte[] newTagBytes = tc.Create(tag, FlacTagCreator.DEFAULT_PADDING).Data;
				rafTemp.Write(newTagBytes, 0, newTagBytes.Length);
				
				b = new byte[65536];
				int read = raf.Read(b, 0, b.Length);
				while (read != 0) {
					rafTemp.Write(b, 0, read);
					read = raf.Read(b, 0, b.Length);
				}
				//tempFC.transferFrom( fc, tempFC.position(), fc.size() );
			}
		}
		
		private int ComputeAvailableRoom(ArrayList metadataBlockPadding, ArrayList metadataBlocks) {
			int length = 0;
			foreach (MetadataBlockData block in metadataBlocks)
				length += block.Length;
						
			foreach (MetadataBlockData block in metadataBlockPadding)
				length += block.Length;
			
			return length;
		}
		
		private int ComputeNeededRoom(ArrayList metadataBlockPadding, ArrayList metadataBlocks) {
			int length = 0;
			foreach (MetadataBlockData block in metadataBlocks)
				length += block.Length;
			
			return length;
		}
	}
}
