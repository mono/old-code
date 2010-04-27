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

namespace Entagged.Audioformats.Flac.Util {
	public class MetadataBlockHeader {			
		
		public enum BlockTypes {
			StreamInfo,
			Padding,
			Application,
			SeekTable,
			VorbisComment,
			CueSheet,
			Unknown
		};
		
		private BlockTypes blockType;
		private int dataLength;
		private bool lastBlock;
		private byte[] data;
		private byte[] bytes;

		public MetadataBlockHeader (byte[] b) {
			bytes = b;
			
			lastBlock = ( (bytes[0] & 0x80) >> 7 ) == 1;
			
			int type = bytes[0] & 0x7F;
			switch (type) {
				case 0: blockType = BlockTypes.StreamInfo; 
					break;

				case 1: blockType = BlockTypes.Padding; 
					break;

				case 2: blockType = BlockTypes.Application; 
					break;

				case 3: blockType = BlockTypes.SeekTable; 
					break;

				case 4: blockType = BlockTypes.VorbisComment; 
					break;

				case 5: blockType = BlockTypes.CueSheet; 
					break;

				default: blockType = BlockTypes.Unknown; 
					break;
			}
			
			dataLength = (u (bytes[1])<<16) + (u (bytes[2])<<8) + (u (bytes[3]));
			
			data = new byte[4];
			data[0] = (byte) (data[0] & 0x7F);
			for (int i = 1; i < 4; i ++) {
				data[i] = bytes[i];
			}
		}

		public int DataLength {
			get {
				return dataLength;
			}
		}

		public BlockTypes BlockType {
			get {
				return blockType;
			}
		}

		public string BlockTypeString {
			get {
				switch (blockType) {
					case BlockTypes.StreamInfo: return "STREAMINFO";
					case BlockTypes.Padding: return "PADDING";
					case BlockTypes.Application: return "APPLICATION";
					case BlockTypes.SeekTable: return "SEEKTABLE";
					case BlockTypes.VorbisComment: return "VORBIS_COMMENT";
					case BlockTypes.CueSheet: return "CUESHEET";
					default: return "UNKNOWN-RESERVED";
				}
			}
		}

		public bool IsLastBlock {
			get {
				return lastBlock;
			}
		}

		public byte[] Data {
			get {
				return data;
			}
		}

		private int u (int i) {
			return i & 0xFF;
		}
	}
}
