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
using Entagged.Audioformats.Util;
using Entagged.Audioformats.Mp3.Util.Id3Frames;

namespace Entagged.Audioformats.Mp3 {

	public class Id3Tag : Tag {
		public static string DEFAULT_ENCODING = "UTF-16";
		
		public static byte ID3V22 = 0;
		public static byte ID3V23 = 1;
		public static byte ID3V24 = 2;
		
		public Id3Tag()
		{
			AddCommonFieldMapping(CommonField.Artist, "TPE1");
			AddCommonFieldMapping(CommonField.Album, "TALB");
			AddCommonFieldMapping(CommonField.Title, "TIT2");
			AddCommonFieldMapping(CommonField.Track, "TRCK");
			AddCommonFieldMapping(CommonField.Year, "TYER");
			AddCommonFieldMapping(CommonField.Genre, "TCON");
			AddCommonFieldMapping(CommonField.License, "TCOP");
			//AddCommonFieldAlias(CommonField.License, CommonField.Comment);
		}
		
		//representedversion stuff
	}
}
