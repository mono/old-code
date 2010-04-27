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

namespace Entagged.Audioformats.Ogg.Util {
	public class OggInfoReader {
		public EncodingInfo Read( Stream raf )  {
			EncodingInfo info = new EncodingInfo();
			long oldPos = 0;
			
			//Reads the file encoding infos -----------------------------------
			raf.Seek( 0 , SeekOrigin.Begin);
			double PCMSamplesNumber = -1;
			raf.Seek( raf.Length-2, SeekOrigin.Begin);
			while(raf.Position >= 4) {
				if(raf.ReadByte()==0x53) {
					raf.Seek( raf.Position - 4, SeekOrigin.Begin);
					byte[] ogg = new byte[3];
					raf.Read(ogg, 0, 3);
					if(ogg[0]==0x4F && ogg[1]==0x67 && ogg[2]==0x67) {
						raf.Seek( raf.Position - 3, SeekOrigin.Begin);
						
						oldPos = raf.Position;
						raf.Seek(raf.Position + 26, SeekOrigin.Begin);
						int _pageSegments = raf.ReadByte()&0xFF; //Unsigned
						raf.Seek( oldPos , SeekOrigin.Begin);
						
						byte[] _b = new byte[27 + _pageSegments];
						raf.Read( _b, 0, _b.Length );

						OggPageHeader _pageHeader = new OggPageHeader( _b );
						raf.Seek(0, SeekOrigin.Begin);
						PCMSamplesNumber = _pageHeader.AbsoluteGranulePosition;
						break;
					}
				}	
				raf.Seek( raf.Position - 2, SeekOrigin.Begin);
			}
			
			if(PCMSamplesNumber == -1){
				throw new CannotReadException("Error: Could not find the Ogg Setup block");
			}
			

			//Supposing 1st page = codec infos
			//			2nd page = comment+decode info
			//...Extracting 1st page
			byte[] b = new byte[4];
			
			oldPos = raf.Position;
			raf.Seek(26, SeekOrigin.Begin);
			int pageSegments = raf.ReadByte()&0xFF; //Unsigned
			raf.Seek( oldPos , SeekOrigin.Begin);

			b = new byte[27 + pageSegments];
			raf.Read( b , 0,  b .Length);

			OggPageHeader pageHeader = new OggPageHeader( b );

			byte[] vorbisData = new byte[pageHeader.PageLength];

			raf.Read( vorbisData , 0,  vorbisData.Length);

			VorbisCodecHeader vorbisCodecHeader = new VorbisCodecHeader( vorbisData );

			//Populates encodingInfo----------------------------------------------------
			info.Duration = new TimeSpan((long)(PCMSamplesNumber / vorbisCodecHeader.SamplingRate) * TimeSpan.TicksPerSecond);
			info.ChannelNumber = vorbisCodecHeader.ChannelNumber;
			info.SamplingRate = vorbisCodecHeader.SamplingRate;
			info.EncodingType = vorbisCodecHeader.EncodingType;
			info.ExtraEncodingInfos = "";
			if(vorbisCodecHeader.NominalBitrate != 0
		        && vorbisCodecHeader.MaxBitrate == vorbisCodecHeader.NominalBitrate
		        && vorbisCodecHeader.MinBitrate == vorbisCodecHeader.NominalBitrate) {
			    //CBR
			    info.Bitrate = vorbisCodecHeader.NominalBitrate;
			    info.Vbr = false;
			}
			else if(vorbisCodecHeader.NominalBitrate != 0
			        && vorbisCodecHeader.MaxBitrate == 0
			        && vorbisCodecHeader.MinBitrate == 0) {
			    //Average vbr
			    info.Bitrate = vorbisCodecHeader.NominalBitrate;
			    info.Vbr = true;
			}
			else {
				info.Bitrate = ComputeBitrate( (int)info.Duration.TotalSeconds, raf.Length );
				info.Vbr = true;
			}

			return info;
		}

		private int ComputeBitrate( int length, long size ) {
			return (int) ( ( size / 1024 ) * 8 / length );
		}
	}
}
