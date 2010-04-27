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
using System.Collections;
using System.Text;
using System.IO;

namespace Entagged.Audioformats.Asf.Util 
{
    public class GUID 
    {
        public static readonly GUID [] KNOWN_GUIDS = new GUID [] {
            GUID_AUDIO_ERROR_CONCEALEMENT_ABSENT, 
            GUID_CONTENTDESCRIPTION,
            GUID_AUDIOSTREAM, 
            GUID_ENCODING, 
            GUID_FILE, 
            GUID_HEADER,
            GUID_STREAM, 
            GUID_EXTENDED_CONTENT_DESCRIPTION, 
            GUID_VIDEOSTREAM,
            GUID_HEADER_EXTENSION, 
            GUID_STREAM_BITRATE_PROPERTIES 
        };

        public const int GUID_LENGTH = 16;
       
        // GUID for stream chunks describing audio streams, 
        // indicating the the audio stream has no error concealment
        public static readonly GUID GUID_AUDIO_ERROR_CONCEALEMENT_ABSENT = new GUID(
            new int [] { 0x40, 0xA4, 0xF1, 0x49, 0xCE, 0x4E, 0xD0, 0x11, 0xA3,
                    0xAC, 0x00, 0xA0, 0xC9, 0x03, 0x48, 0xF6 },
            "Audio error concealment absent.");

        // GUID indicating that stream type is audio
        public static readonly GUID GUID_AUDIOSTREAM = new GUID(new int [] { 0x40,
            0x9E, 0x69, 0xF8, 0x4D, 0x5B, 0xCF, 0x11, 0xA8, 0xFD, 0x00, 0x80,
            0x5F, 0x5C, 0x44, 0x2B }, " Audio stream");

        // This constant represents the guid for a chunk which contains title,
        // author, copyright, description and rating
        public static readonly GUID GUID_CONTENTDESCRIPTION = new GUID(new int [] {
            0x33, 0x26, 0xB2, 0x75, 0x8E, 0x66, 0xCF, 0x11, 0xA6, 0xD9, 0x00,
            0xAA, 0x00, 0x62, 0xCE, 0x6C }, "Content Description");

        // GUID for Encoding-Info chunks
        public static readonly GUID GUID_ENCODING = new GUID(new int [] { 0x40, 0x52,
            0xD1, 0x86, 0x1D, 0x31, 0xD0, 0x11, 0xA3, 0xA4, 0x00, 0xA0, 0xC9,
            0x03, 0x48, 0xF6 }, "Encoding description");

        // GUID for a WMA "Extended Content Description" chunk
        public static readonly GUID GUID_EXTENDED_CONTENT_DESCRIPTION = new GUID(
            new int [] { 0x40, 0xA4, 0xD0, 0xD2, 0x07, 0xE3, 0xD2, 0x11, 0x97,
                    0xF0, 0x00, 0xA0, 0xC9, 0x5E, 0xA8, 0x50 },
            "Extended Content Description");

        // GUID of ASF File Header
        public static readonly GUID GUID_FILE = new GUID(new int [] { 0xA1, 0xDC,0xAB,
            0x8C, 0x47, 0xA9, 0xCF, 0x11, 0x8E, 0xE4, 0x00, 0xC0, 0x0C, 0x20,
            0x53, 0x65 }, "File header");

        // GUID of a ASF header chunk
        public static readonly GUID GUID_HEADER = new GUID(new int [] { 0x30, 0x26,
            0xb2, 0x75, 0x8e, 0x66, 0xcf, 0x11, 0xa6, 0xd9, 0x00, 0xaa, 0x00,
            0x62, 0xce, 0x6c }, "Asf header");

        // GUID indicating a stream object
        public static readonly GUID GUID_STREAM = new GUID(new int [] { 0x91, 0x07,
            0xDC, 0xB7, 0xB7, 0xA9, 0xCF, 0x11, 0x8E, 0xE6, 0x00, 0xC0, 0x0C,
            0x20, 0x53, 0x65 }, "Stream");

        // Unknown GUID
        public static readonly GUID GUID_HEADER_EXTENSION = new GUID(new int [] { 0xB5, 0x03,
            0xBF, 0x5F, 0x2E, 0xA9, 0xCF, 0x11, 0x8E, 0xE3, 0x00, 0xC0, 0x0C,
            0x20, 0x53, 0x65 }, "Header Extension");

        // GUID indicating a "stream bitrate properties"
        public static readonly GUID GUID_STREAM_BITRATE_PROPERTIES = new GUID(
            new int [] { 0xCE, 0x75, 0xF8, 0x7B, 0x8D, 0x46, 0xD1, 0x11, 0x8D,
                    0x82, 0x00, 0x60, 0x97, 0xC9, 0xA2, 0xB2 },
            "Stream bitrate properties");

        // GUID indicating that stream type is video.
        public static readonly GUID GUID_VIDEOSTREAM = new GUID(new int [] { 0xC0,
            0xEF, 0x19, 0xBC, 0x4D, 0x5B, 0xCF, 0x11, 0xA8, 0xFD, 0x00, 0x80,
            0x5F, 0x5C, 0x44, 0x2B }, "Video stream");

        private string description;
        private int [] guid = null;

        
        public GUID(int [] guid) 
        {
            Guid = guid;
        }

        public GUID(int [] guid, string description) : this(guid) 
        {
            Description = description;
        }

        public static GUID ReadGUID(Stream stream) 
        {
            byte [] guid = new byte[GUID.GUID_LENGTH];
            stream.Read(guid, 0, guid.Length);
            
            int [] tmp = new int[guid.Length];
            
            for(int i = 0; i < guid.Length; i++) {
                tmp[i] = guid[i];
            }   
            
            return new GUID(tmp);
        }

        public static bool AssertGUID(int [] guid) 
        {
            return guid != null && guid.Length == GUID_LENGTH;
        }
        
        public static string GetGuidDescription(GUID guid) 
        {
            foreach(GUID known_guid in KNOWN_GUIDS) {
                if(known_guid.Equals(guid)) {
                    return known_guid.Description;
                }
            }
            
            return null;
        }

        public override bool Equals(object o) 
        {
            if(!(o is GUID)) {
                return false;
            }
            
            GUID compare = o as GUID;
            bool result = true;
            
            for(int i = 0; i < GUID.GUID_LENGTH && result; i++) {
                result &= compare.guid[i] == guid[i];
            }
            
            return result;
        }
        
        public override int GetHashCode() 
        {
            long sum = 0;
            for(int i = 0; i < GUID.GUID_LENGTH; i++) {
                sum += Guid[i];
                sum <<= 1;
            }
            
            return (int)(sum % UInt32.MaxValue);
        }
        
        public override string ToString() 
        {
            StringBuilder result = new StringBuilder ("GUID: ");
            
            foreach(int curr in Guid) {
                result.Append(curr.ToString("x"));
                result.Append(",");
            }
            
            return result.ToString();
        }

        public byte [] Bytes {
            get {
                byte [] result = new byte[GUID.GUID_LENGTH];
                for(int i = 0; i < result.Length; i++) {
                    result[i] = (byte)(guid[i] & 0xFF);
                }
                
                return result;
            }
        }
        
        public int [] Guid {
            get {
                return guid;
            }
            
            set {
                if(!AssertGUID(value)) {
                    throw new ArgumentException("Invalid GUID");
                }
                
                guid = new int[value.Length];
                value.CopyTo(guid, 0);
            }
        }

        public string Description {
            get {
                return description;
            }
            
            set {
                description = value;
            }
        }
    }
}
