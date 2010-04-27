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
using Entagged.Audioformats.Util;

namespace Entagged.Audioformats 
{
   public class AudioFileContainer 
   {
        private EncodingInfo info;
        private Tag tag;
        private AudioFileReader reader;
        private AudioFileWriter writer;
        
        public AudioFileContainer(EncodingInfo info, Tag tag) 
        {
            this.info = info;
            this.tag = tag;
        }
        
        public AudioFileContainer(EncodingInfo info) : this(info, new Tag()) 
        {
        }
        
        public AudioFileReader Reader {
            get {
                return reader;
            }
            
            set {
                reader = value;
            }
        }
        
        public AudioFileWriter Writer {
            get {
                return writer;
            }
            
            set {
                writer = value;
            }
        }
        
        public int Bitrate {
            get { 
                return info.Bitrate; 
            }
        }

        public int ChannelNumber {
            get { 
                return info.ChannelNumber; 
            }
        }
        
        public string EncodingType {
            get { 
                return info.EncodingType; 
            }
        }

        public string ExtraEncodingInfos {
            get { 
                return info.ExtraEncodingInfos; 
            }
        }

        public int SamplingRate {
            get { 
                return info.SamplingRate; 
            }
        }

        public TimeSpan Duration {
            get { 
                return info.Duration; 
            }
        }
        
        public bool IsVbr {
            get { 
                return info.Vbr;
            }
        }
        
        public Tag Tag {
            get { 
                return (tag == null) ? new Tag() : tag; 
            }
        }
        
        public EncodingInfo EncodingInfo {
            get {
                return info;
            }
        }
        
        public override string ToString() 
        {
            return "AudioFile --------\n" + 
                info.ToString() + "\n"+ 
                ((tag == null) ? "" : 
                tag.ToString()) +
                "\n-------------------";
        }
    }
}
