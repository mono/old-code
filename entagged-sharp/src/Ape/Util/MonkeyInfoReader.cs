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
using Entagged.Audioformats.Util;
using Entagged.Audioformats.Exceptions;

namespace Entagged.Audioformats.Ape.Util {
	public class MonkeyInfoReader {

		public EncodingInfo Read( Stream raf ) {
			EncodingInfo info = new EncodingInfo();
			
			//Begin info fetch-------------------------------------------
			if ( raf.Length == 0 ) {
				//Empty File
				throw new CannotReadException("File is empty");
			}
			raf.Seek( 0 , SeekOrigin.Begin);
		
			//MP+ Header string
			byte[] b = new byte[4];
			raf.Read(b, 0, b.Length);
			string mpc = new string(System.Text.Encoding.ASCII.GetChars(b));
			if (mpc != "MAC ") {
				throw new CannotReadException("'MAC ' Header not found");
			}
			
			b = new byte[4];
			raf.Read(b, 0, b.Length);
			int version = Utils.GetNumber(b, 0,3);
			if(version < 3970)
				throw new CannotReadException("Monkey Audio version <= 3.97 is not supported");
			
			b = new byte[44];
			raf.Read(b, 0, b.Length);
			MonkeyDescriptor md = new MonkeyDescriptor(b);
			
			b = new byte[24];
			raf.Read(b, 0, b.Length);
			MonkeyHeader mh = new MonkeyHeader(b);
			
			raf.Seek(md.RiffWavOffset, SeekOrigin.Begin);
			b = new byte[12];
			raf.Read(b, 0, b.Length);
			WavRIFFHeader wrh = new WavRIFFHeader(b);
			if(!wrh.Valid)
				throw new CannotReadException("No valid RIFF Header found");
			
			b = new byte[24];
			raf.Read(b, 0, b.Length);
			WavFormatHeader wfh = new WavFormatHeader(b);
			if(!wfh.Valid)
				throw new CannotReadException("No valid WAV Header found");
			
			info.Duration = new TimeSpan(mh.Length * TimeSpan.TicksPerSecond);
			info.ChannelNumber = wfh.ChannelNumber;
			info.SamplingRate = wfh.SamplingRate;
			info.Bitrate = ComputeBitrate(mh.Length, raf.Length);
			info.EncodingType = "Monkey Audio v" + (((double)version)/1000)+", compression level "+mh.CompressionLevel;
			info.ExtraEncodingInfos = "";
			
			return info;
		}
		
		private int ComputeBitrate( int length, long size ) {
			return (int) ( ( size / 1000 ) * 8 / length );
		}
	}
}
