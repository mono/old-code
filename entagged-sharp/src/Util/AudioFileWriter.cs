/***************************************************************************
 *  Copyright 2005 Raphal Slinckx <raphael@slinckx.net> 
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

namespace Entagged.Audioformats.Util {
    public abstract class AudioFileWriter {
        private enum Action {
            Write,
            Delete
        }
        
        protected abstract void WriteTag(Tag tag, Stream stream, Stream temp);
        protected abstract void DeleteTag(Stream stream, Stream temp);
        
        public void Delete(string f) {
            CreateStreams(f, Action.Delete, null);
        }
        
        public void Write(string f, AudioFileContainer af) {
            //Preliminary checks
            if (af.Tag.IsEmpty) {
                Delete(f);
                return;
            }
            
            CreateStreams(f, Action.Write, af.Tag);
        }
                    
        private void CreateStreams(string f, Action action, Tag tag) {
            FileStream stream;
            FileStream temp;
            string tempFile;
            Exception exception = null;
                
            try {
                stream = File.Open (f, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            } catch (Exception e) {
                throw new CannotWriteException("\""+f+"\": "+e.Message);
            }
            
            using (stream) {                
                if (!stream.CanWrite)
                    throw new CannotWriteException("Can't write to file \""+f+"\"");
                
                if(stream.Length <= 150)
                    throw new CannotWriteException("Less than 150 byte \""+f+"\"");
                
                try {
                    //Fixme !
                    tempFile = "entagged.tmp";
                    temp = File.Open (tempFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                } catch (Exception e) {
                    throw new CannotWriteException("\""+f+"\": "+e.Message);
                }
                
                using (temp) {
                    try {
                        switch(action) {
                            case Action.Delete:
                                DeleteTag(stream, temp);
                                break;
                            case Action.Write:
                                WriteTag(tag, stream, temp);
                                break;
                        }
                    }
                    catch ( Exception e ) {
                        exception = e;
                    }
                }
                
                if(temp.Length > 0) {
                    File.Delete(f);
                    File.Move(tempFile, f);
                } else {
                    File.Delete(tempFile);
                }
                
                if (exception != null)
                    throw new CannotWriteException("\""+f+"\" :"+exception.Message);
            }
        }
    }
}
