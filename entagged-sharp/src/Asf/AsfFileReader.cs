/***************************************************************************
 *  Copyright 2005 Christian Laireiter <liree@web.de>
 *  Copyright 2005 Novell, Inc
 *  Aaron Bockover <aaron@aaronbock.net>
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

using Entagged.Audioformats;
using Entagged.Audioformats.Asf.Util;
using Entagged.Audioformats.Util;

namespace Entagged.Audioformats.Asf 
{
    // The stream must be seekable and only the information 
    // of the first audio stream is extracted.
    
    [SupportedMimeType("entagged/wma")]
    [SupportedMimeType("audio/x-ms-wma")]
    [SupportedMimeType("video/x-ms-asf")] // stupid gnome-vfs
	public class AsfFileReader : AudioFileReader 
    {
        private static readonly AsfInfoReader info_reader = new AsfInfoReader();
        private static readonly AsfTagReader tag_reader = new AsfTagReader();

        protected override EncodingInfo GetEncodingInfo(Stream stream, string mime) 
        {
            // AsfInfoReader needs the stream to be at position 0
            stream.Seek(0, SeekOrigin.Begin);
            return info_reader.Read(stream);
        }

        protected override Tag GetTag(Stream stream, string mime) 
        {
            stream.Seek(0, SeekOrigin.Begin);
            return tag_reader.Read(stream);
        }
    }
}
