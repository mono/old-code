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

using Entagged.Audioformats.Exceptions;
using Entagged.Audioformats.Util;
using System.Reflection;
using System.Collections;
using System.IO;
using System;

namespace Entagged.Audioformats 
{
    public class AudioFileIO 
    {        
        //These tables contains all the readers writers associated with extensions/mimetypes
        private static Hashtable readers = new Hashtable();
        
        //Initialize the different readers/writers using reflection
        static AudioFileIO() 
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            foreach(Type type in assembly.GetTypes()) {
                if(!type.IsSubclassOf(typeof(AudioFileReader))) {
                    continue;
                }
                
                AudioFileReader reader = (AudioFileReader)Activator.CreateInstance(type);
                Attribute [] attrs = Attribute.GetCustomAttributes(type, typeof(SupportedMimeType));
                foreach(SupportedMimeType attr in attrs) {
                    readers.Add(attr.MimeType, reader);
                }
            }
        }
        
        public static AudioFileContainer Read(string f) 
        {
            string mimetype = "entagged/" + Path.GetExtension(f).Substring(1);
            return Read(f, mimetype);
        }

        public static AudioFileContainer Read(string f, string mimetype) 
        {
            object afr = readers[mimetype];
            
            if(afr == null) {
                throw new UnsupportedFormatException("No reader associated with MimeType: " + mimetype);
            }
            
            return (afr as AudioFileReader).Read(f, mimetype);
        }

        public static AudioFileContainer Read(Stream stream, string mimetype)
        {
            object afr = readers[mimetype];
            
            if(afr == null) {
                throw new UnsupportedFormatException("No reader associated with MimeType: " + mimetype);
            }
            
            return (afr as AudioFileReader).Read(stream, mimetype);
        }
    }
}
