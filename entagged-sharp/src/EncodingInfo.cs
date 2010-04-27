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
using System.Collections;
using System.Text;

namespace Entagged.Audioformats 
{
    public class EncodingInfo 
    {
        private Hashtable content;
        
        public EncodingInfo() 
        {
            content = new Hashtable();
            content["BITRATE"] =  -1;
            content["CHANNB"] =  -1;
            content["TYPE"] =  "";
            content["INFOS"] =  "";
            content["SAMPLING"] =  -1;
            content["DURATION"] = -1;
            content["VBR"] = true;
        }
        
        //Sets the bitrate in KByte/s
        public int Bitrate {
            set { content["BITRATE"] = value; }
            get { return (int) content["BITRATE"]; }
        }
        
        //Sets the number of channels
        public int ChannelNumber {
            set { content["CHANNB"] = value; }
            get { return (int) content["CHANNB"]; }
        }
        
        //Sets the type of the encoding, this is a bit format specific. eg:Layer I/II/II
        public string EncodingType {
            set { content["TYPE"] = value; }
            get { return (string) content["TYPE"]; }
        }
        
        //A string contianing anything else that might be interesting
        public string ExtraEncodingInfos {
            set { content["INFOS"] = value; }
            get { return (string) content["INFOS"]; }
        }
        
        //Sets the Sampling rate in Hz
        public int SamplingRate {
            set { content["SAMPLING"] = value; }
            get { return (int) content["SAMPLING"]; }
        }
        
        //Sets the length of the song in seconds
        public TimeSpan Duration {
            set { content["DURATION"] = value; }
            get { 
                object o = content["DURATION"];
                if(o is TimeSpan) {
                    return (TimeSpan)o;
                }
                
                int duration = (int)o;
                return duration < 0 ? new TimeSpan(0) : new TimeSpan(duration * TimeSpan.TicksPerSecond);
            }
        }
        
        public bool Vbr {
            set { content["VBR"] = value; }
            get { return (bool) content["VBR"]; }
        }
        
        //Pretty prints this encoding info
        public override string ToString() 
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Encoding infos content:\n");
            
            foreach(DictionaryEntry entry in content) {
              sb.Append("\t");
              sb.Append(entry.Key);
              sb.Append(" : ");
              sb.Append(entry.Value);
              sb.Append("\n");
            }
            
            return sb.ToString().Substring(0,sb.Length-1);
        }
    }
}
