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

namespace Entagged.Audioformats.Util 
{
    public abstract class AudioFileReader 
    {    
        protected abstract EncodingInfo GetEncodingInfo(Stream raf, string mime);
        protected abstract Tag GetTag(Stream raf, string mime);

        public AudioFileContainer Read(string f, string mime) 
        {
            FileStream stream;

            try {
                stream = File.Open(f, FileMode.Open, FileAccess.Read, FileShare.Read);
            } catch (Exception e) {
                throw new CannotReadException("\"" + f + "\": " + e.Message);
            }

            return Read(stream, mime);
        }
        
        public AudioFileContainer Read(Stream stream, string mime) {
            if(! stream.CanSeek) {
                throw new CannotReadException("Stream not seekable");
            }
            
            if(stream.Length <= 150) {
                throw new CannotReadException("Less than 150 byte stream");
            }
            
            try {
                stream.Seek(0, SeekOrigin.Begin);
                
                EncodingInfo info = GetEncodingInfo(stream, mime);
            
                Tag tag = null;
                
                try {
                    stream.Seek(0, SeekOrigin.Begin);
                    tag = GetTag(stream, mime);
                } catch(CannotReadException e) {
                    // Do nothing
                }

                return new AudioFileContainer(info, tag);
            } catch(Exception e) {
                throw new CannotReadException(e.ToString());
            } finally {
                try {
                    if(stream != null) {
                        stream.Close();
                    }
                } catch(Exception ex) {
                    /* We tried everything... */
                }
            }
        }
    }
}
