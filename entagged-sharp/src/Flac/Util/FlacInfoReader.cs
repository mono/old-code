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

using System;
using System.IO;
using Entagged.Audioformats.Exceptions;

namespace Entagged.Audioformats.Flac.Util {
	public class FlacInfoReader {
		public EncodingInfo Read(Stream raf) {
			//Read the infos--------------------------------------------------------
			if (raf.Length == 0) {
				//Empty File
				throw new CannotReadException("Error: File empty");
			}
			raf.Seek(0, SeekOrigin.Begin);

			//FLAC Header string
			byte[] b = new byte[4];
			raf.Read(b, 0, b.Length);
			string flac = new string(System.Text.Encoding.ASCII.GetChars(b));
			if (flac != "fLaC") {
				throw new CannotReadException("fLaC Header not found");
			}

			MetadataBlockDataStreamInfo mbdsi = null;
			bool isLastBlock = false;
			while (!isLastBlock) {
				b = new byte[4];
				raf.Read(b, 0, b.Length);
				MetadataBlockHeader mbh = new MetadataBlockHeader(b);

				if (mbh.BlockType == MetadataBlockHeader.BlockTypes.StreamInfo) {
					b = new byte[mbh.DataLength];
					raf.Read(b, 0, b.Length);

					mbdsi = new MetadataBlockDataStreamInfo(b);
					if (!mbdsi.Valid) {
						throw new CannotReadException("FLAC StreamInfo not valid");
					}
					break;
				}
				raf.Seek(raf.Position + mbh.DataLength, SeekOrigin.Begin);

				isLastBlock = mbh.IsLastBlock;
				mbh = null; //Free memory
			}

			EncodingInfo info = new EncodingInfo();
			info.Duration = new TimeSpan(mbdsi.Length * TimeSpan.TicksPerSecond);
			info.ChannelNumber = mbdsi.ChannelNumber;
			info.SamplingRate = mbdsi.SamplingRate;
			info.EncodingType = mbdsi.EncodingType;
			info.ExtraEncodingInfos = "";
			info.Bitrate = ComputeBitrate(mbdsi.Length, raf.Length);

			return info;
		}

		private int ComputeBitrate(int length, long size) {
			return (int) ((size / 1000) * 8 / length);
		}
	}
}
