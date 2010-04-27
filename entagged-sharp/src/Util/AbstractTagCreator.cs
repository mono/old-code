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

using System.Collections;

namespace Entagged.Audioformats.Util 
{
    public abstract class AbstractTagCreator 
    {
        public ByteBuffer Create(Tag tag) 
        {
            return Create(tag, 0);
        }
        
        public ByteBuffer Create(Tag tag, int padding) 
        {
            IList fields = CreateFields(tag);
            int tagSize = ComputeTagLength(tag, fields);
            
            ByteBuffer buf = new ByteBuffer(tagSize + padding);
            Create(tag, buf, fields, tagSize, padding);
            buf.Rewind();
            
            return buf;
        }
        
        public int GetTagLength(Tag tag) 
        {
            return ComputeTagLength(tag, CreateFields(tag));
        }
        
        protected IList CreateFields(Tag tag) 
        {
            IList fields = new ArrayList();
            
            foreach(DictionaryEntry entry in tag) {
                foreach(string content in (IList) entry.Value) {
                    fields.Add(CreateField((string) entry.Key, content));
                }
            }
            
            return fields;
        }
        
        //Compute the number of bytes the tag will be.
        protected int ComputeTagLength(Tag tag, IList l) 
        {
            int length = GetFixedTagLength(tag);
            
            foreach(byte[] field in l) {
                length += field.Length;
            }
            
            return length;
        }
                            
        protected abstract int GetFixedTagLength(Tag tag);
        protected abstract void Create(Tag tag, ByteBuffer buf, IList fields, int tagSize, int padding);
        protected abstract byte [] CreateField(string id, string content);
    }
}
