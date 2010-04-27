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
using Entagged.Audioformats.Ape.Util;
using Entagged.Audioformats.Util;
//using Entagged.Audioformats.Mp3;

namespace Entagged.Audioformats.Mpc {
	public class MpcFileWriter : AudioFileWriter {
	
		private ApeTagWriter tw = new ApeTagWriter();
//		private Mp3FileWriter mp3tw = new Mp3FileWriter();
		
		protected override void WriteTag(Tag tag, Stream stream, Stream temp) {
			tw.Write(tag, stream, temp);
		}
		protected override void DeleteTag(Stream stream, Stream temp) {
//			mp3tw.Delete(stream, temp);
			if(temp.Length > 0)
				tw.Delete(temp);
			else
				tw.Delete(stream);
		}
	}
}
