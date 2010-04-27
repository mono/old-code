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

namespace Entagged.Audioformats.Util 
{
    public class ByteBuffer 
    {
        private byte[] buf;
        private int pointer;
        
        public ByteBuffer(int capacity) 
        {
            this.buf = new byte[capacity];
            this.pointer = 0;
        }
        
        public ByteBuffer(byte [] data) 
        {
            this.buf = data;
            this.pointer = 0;
        }
        
        public byte [] Data {
            get { 
                return buf;
            }
        }
        
        public int Capacity {
            get { 
                return buf.Length; 
            }
        }
        
        public int Position {
            get { return pointer; }
            set { this.pointer = value; }
        }
        
        public int Limit {
            get { return buf.Length; }
            set {
                byte [] newbuf = new byte[value];
                
                for(int i = 0; i < newbuf.Length; i++) {
                    newbuf[i] = buf[i];
                }
                
                this.buf = newbuf;
            }
        }
        
        public int Remaining {
            get { 
                return buf.Length - pointer; 
            }
        }
        
        public byte Get() 
        {
            return buf[pointer++];
        }
        
        public byte Peek() 
        {
            return buf[pointer];
        }
        
        public void Get(byte[] data) 
        {
            for(int i = 0; i<data.Length; i++) {
                data[i] = buf[pointer++];
            }
        }
        
        public void Put(byte b) 
        {
            buf[pointer++] = b;
        }
        
        public void Put(byte[] bytes) 
        {
            foreach(byte b in bytes) {
                buf[pointer++] = b;
            }
        }
        
        public void Put(ByteBuffer buf) 
        {
            Put(buf.Data);
        }
        
        public void Put(int offset, byte b) 
        {
            buf[offset] = b;
        }
        
        public void Put(Stream raf) 
        {
            //FIXME: this is ugly and inefficient
            while(pointer < buf.Length)
                Put((byte)raf.ReadByte());
        }
        
        public void Rewind() 
        {
            this.pointer = 0;
        }
    }
}
