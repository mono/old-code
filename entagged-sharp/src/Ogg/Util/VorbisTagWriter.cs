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
using Entagged.Audioformats.Util;
using Entagged.Audioformats.Ogg;
using System.IO;

namespace Entagged.Audioformats.Ogg.Util {
	public class VorbisTagWriter {
		
		private VorbisTagCreator tc = new VorbisTagCreator();
		private VorbisTagReader reader = new VorbisTagReader();
		
		public void Delete(Stream raf, Stream tempRaf) {
			OggTag tag = null;
			try {
				tag = (OggTag) reader.Read(raf);
			} catch(CannotReadException e) {
				Write(new OggTag(), raf, tempRaf);
				return;
			}
			
			OggTag emptyTag = new OggTag();
			if (tag.HasField("vendor"))
				emptyTag.Set("vendor", tag.Get("vendor")[0] as string);
			
			Write(emptyTag, raf, tempRaf);
		}
		
		public void Write(Tag tag, Stream raf, Stream rafTemp) {
			//Read firstPage----------------------------------------------------
			raf.Seek(26, SeekOrigin.Begin);
			byte[] b = new byte[4];
			int pageSegments = raf.ReadByte()&0xFF; //Unsigned
			raf.Seek(0, SeekOrigin.Begin);

			b = new byte[27 + pageSegments];
			raf.Read(b, 0, b.Length);

			OggPageHeader firstPage = new OggPageHeader(b);
			//------------------------------------------------------------------

			raf.Seek(0, SeekOrigin.Begin);
			//write 1st page (unchanged)----------------------------------------
			byte[] pageBytes = new byte[firstPage.PageLength + 27 + pageSegments];
			raf.Read(pageBytes, 0, pageBytes.Length);
			rafTemp.Write(pageBytes, 0, pageBytes.Length);
			//rafTemp.Seek(firstPage.PageLength + raf.Position, SeekOrigin.Current);
			//------------------------------------------------------------------
			
			//Read 2nd page-----------------------------------------------------
			long pos = raf.Position;
			raf.Seek(raf.Position + 26, SeekOrigin.Begin);
			pageSegments = raf.ReadByte()&0xFF; //Unsigned
			raf.Seek(pos, SeekOrigin.Begin);
			
			b = new byte[27 + pageSegments];
			raf.Read(b, 0, b.Length);
			OggPageHeader secondPage = new OggPageHeader(b);
			long secondPageEndPos = raf.Position;
			//------------------------------------------------------------------

			//Compute old comment length----------------------------------------
			int oldCommentLength = 7; // [.vorbis]
			raf.Seek(raf.Position + 7, SeekOrigin.Begin);

			b = new byte[4];
			raf.Read(b, 0, b.Length);
			int vendorstringLength = Utils.GetNumber(b, 0,3);
			oldCommentLength += 4 + vendorstringLength;

			raf.Seek(raf.Position + vendorstringLength, SeekOrigin.Begin);

			b = new byte[4];
			raf.Read(b, 0, b.Length);
			int userComments = Utils.GetNumber(b, 0,3);
			oldCommentLength += 4;

			for (int i = 0; i < userComments; i++) {
				b = new byte[4];
				raf.Read(b, 0, b.Length);
				int commentLength = Utils.GetNumber(b, 0,3);
				oldCommentLength += 4 + commentLength;

				raf.Seek(raf.Position + commentLength, SeekOrigin.Begin);
			}

			int isValid = raf.ReadByte();
			oldCommentLength += 1;
			if (isValid != 1)
				throw new CannotWriteException("Unable to retreive old tag informations");
			//------------------------------------------------------------------

			//Get the new comment and create the container bytebuffer for 2nd page---
			ByteBuffer newComment = tc.Convert(tag);

			int newCommentLength = newComment.Capacity;
			int newSecondPageLength = secondPage.PageLength - oldCommentLength + newCommentLength;

			byte[] segmentTable = CreateSegmentTable(oldCommentLength,newCommentLength,secondPage);
			int newSecondPageHeaderLength = 27 + segmentTable.Length;

			ByteBuffer secondPageBuffer = new ByteBuffer(newSecondPageLength + newSecondPageHeaderLength);
			//------------------------------------------------------------------

			//Build the new second page header----------------------------------
			//OggS capture
			secondPageBuffer.Put(Utils.GetBytes("OggS"));
			//Stream struct revision
			secondPageBuffer.Put((byte) 0);
			//header_type_flag
			secondPageBuffer.Put((byte) 0);
			//absolute granule position
			secondPageBuffer.Put(
				new byte[] {
					(byte) 0,
					(byte) 0,
					(byte) 0,
					(byte) 0,
					(byte) 0,
					(byte) 0,
					(byte) 0,
					(byte) 0 });

			//stream serial number
			secondPageBuffer.Put(Utils.GetNumber(secondPage.SerialNumber));

			//page sequence no
			secondPageBuffer.Put(Utils.GetNumber(secondPage.PageSequence));

			//CRC (to be computed later)
			secondPageBuffer.Put(new byte[] {(byte) 0, (byte) 0, (byte) 0, (byte) 0 });
			int crcOffset = 22;
			
			if (segmentTable.Length > 255) {
			    throw new CannotWriteException ("In this special case we need to " +
			    		"create a new page, since we still hadn't the time for that " +
			    		"we won't write because it wouldn't create an ogg file.");
			}
			//page_segments nb.
			secondPageBuffer.Put((byte)segmentTable.Length);

			//page segment table
			for (int i = 0; i < segmentTable.Length; i++)
				secondPageBuffer.Put(segmentTable[i]);
			//------------------------------------------------------------------

			//Add to the new second page the new comment------------------------
			secondPageBuffer.Put(newComment);
			//------------------------------------------------------------------

			//Add the remaining old second page (encoding infos, etc)-----------
			raf.Seek(secondPageEndPos+oldCommentLength, SeekOrigin.Begin);
			secondPageBuffer.Put(raf);
			//------------------------------------------------------------------

			//Compute CRC over the new second page------------------------------
			byte[] crc = OggCRCFactory.ComputeCRC(secondPageBuffer.Data);
			for (int i = 0; i < crc.Length; i++) {
				secondPageBuffer.Put(crcOffset + i, crc[i]);
			}
			//------------------------------------------------------------------

			//Transfer the second page bytebuffer content-----------------------
			secondPageBuffer.Rewind();
			rafTemp.Write(secondPageBuffer.Data, 0, secondPageBuffer.Data.Length);
			//------------------------------------------------------------------

			//Write the rest of the original file-------------------------------
			byte[] buf = new byte[65536];
			int read = raf.Read(buf, 0, buf.Length);
			while (read != 0) {
				rafTemp.Write(buf, 0, read);
				read = raf.Read(buf, 0, buf.Length);
			}
			//rafTemp.getChannel().transferFrom(raf.getChannel(), 	rafTemp.Position, raf.Length() - raf.Position);
			//------------------------------------------------------------------
		}

	    /**
	     * This method creates a new segment table for the second page (header).
	     * @param oldCommentLength The lentgh of the old comment section, used to verify
	     * the old secment table
	     * @param newCommentLength Of this value the start of the segment table
	     * will be created.
	     * @param secondPage The second page from the source file.
	     * @return new segment table.
	     */
	    private byte[] CreateSegmentTable(int oldCommentLength, int newCommentLength, OggPageHeader secondPage)  {
	        int totalLenght = secondPage.PageLength;
	        byte[] restShouldBe = CreateSegments(totalLenght-oldCommentLength,false);
	        byte[] newStart = CreateSegments(newCommentLength,true);
	        byte[] result = new byte[newStart.Length+restShouldBe.Length];
	        newStart.CopyTo(result, 0);
	        restShouldBe.CopyTo(result, newStart.Length);
	        return result;
	    }
	    
	    /**
	     * This method easily creates a byte Array of values whose sum should
	     * be the value of <code>length</code>.<br>
	     * 
	     * @param length Size of the page which should be
	     * represented as 255 byte packets.
	     * @param quitStream If true and a length is a multiple of 255 we need another
	     * segment table entry with the value of 0. Else it's the last stream of the 
	     * table which is already ended.
	     * @return Array of packet sizes. However only the last packet will
	     * differ from 255.
	     */
	    private byte[] CreateSegments(int length,bool quitStream) {
	        byte[] result = new byte[length / 255 + 
	                                 ((length % 255 == 0 && !quitStream) ? 0 : 1)];
	        int i = 0;
	        for (; i < result.Length -1 ; i++) {
	            result[i] = (byte)0xFF;
	        }
	        result[result.Length-1] = (byte) (length - (i * 255));
	        return result;
	    }
	}
}
