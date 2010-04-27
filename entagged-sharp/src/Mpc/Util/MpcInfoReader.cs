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

namespace Entagged.Audioformats.Mpc.Util {
	public class MpcInfoReader {
		public EncodingInfo Read( Stream raf ) {
			EncodingInfo info = new EncodingInfo();
			
			//Begin info fetch-------------------------------------------
			if ( raf.Length==0 ) {
				//Empty File
				throw new CannotReadException("File is empty");
			}
			raf.Seek( 0 , SeekOrigin.Begin);
		
			
			//MP+ Header string
			byte[] b = new byte[3];
			raf.Read(b, 0, b.Length);
			string mpc = new string(System.Text.Encoding.ASCII.GetChars(b));
			if (mpc != "MP+" && mpc == "ID3") {
				//TODO Do we have to do this ??
				//we have an ID3v2 tag at the beginning
				//We quickly jump to MPC data
				raf.Seek(6, SeekOrigin.Begin);
				int tagSize = ReadSyncsafeInteger(raf);
				raf.Seek(tagSize+10, SeekOrigin.Begin);
				
				//retry to read MPC stream
				b = new byte[3];
				raf.Read(b, 0, b.Length);
				mpc = new string(System.Text.Encoding.ASCII.GetChars(b));
				if (mpc != "MP+") {
					//We could definitely not go there
					throw new CannotReadException("MP+ Header not found");
				}
			} else if (mpc != "MP+"){
				throw new CannotReadException("MP+ Header not found");
			}
			
			b = new byte[25];
			raf.Read(b, 0, b.Length);
			MpcHeader mpcH = new MpcHeader(b);
			//We only support v7 Stream format, so if it isn't v7, then returned values
			//will be bogus, and the file will be ignored
			
			double pcm = mpcH.SamplesNumber;
			info.Duration = new TimeSpan((long)(pcm * 1152 / mpcH.SamplingRate) * TimeSpan.TicksPerSecond);
			info.ChannelNumber = mpcH.ChannelNumber;
			info.SamplingRate = mpcH.SamplingRate;
			info.EncodingType = mpcH.EncodingType;
			info.ExtraEncodingInfos = mpcH.EncoderInfo;
			info.Bitrate = ComputeBitrate( info.Duration.Seconds, raf.Length );

			return info;
		}
		
		private int ReadSyncsafeInteger(Stream raf)	{
			int value = 0;

			value += (raf.ReadByte()& 0xFF) << 21;
			value += (raf.ReadByte()& 0xFF) << 14;
			value += (raf.ReadByte()& 0xFF) << 7;
			value += raf.ReadByte() & 0xFF;

			return value;
		}

		private int ComputeBitrate( int length, long size ) {
			return (int) ( ( size / 1000 ) * 8 / length );
		}
	}
}
