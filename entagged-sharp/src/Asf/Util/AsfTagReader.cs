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
using System.Text;
 
using Entagged.Audioformats;

namespace Entagged.Audioformats.Asf.Util 
{
    public class AsfTagReader 
    {
        public Tag Read(Stream stream) 
        {
            /* Assuming we are at the start of the ASF header GUID */
            GUID header_guid = GUID.ReadGUID(stream);
            
            if(!GUID.GUID_HEADER.Equals(header_guid)) {
                return null;
            }
            
            Tag tag = new Tag();
            BinaryReader reader = new BinaryReader(stream);
            
            // Skip length of header
            stream.Seek(8, SeekOrigin.Current);

            // Read the number of chunks
            uint chunk_count = reader.ReadUInt32();

            // Skip unknown bytes
            stream.Seek(2, SeekOrigin.Current);

            // Two flags, When both are set, all Information needed has ben read
            bool is_content_parsed = false;
            bool is_extended_parsed = false;

            // Now read the chunks
            for(int i = 0; i < chunk_count && !(is_content_parsed && is_extended_parsed); i++) {
                long chunk_start = stream.Position;
                GUID current_guid = GUID.ReadGUID(stream);
                ulong chunk_len = reader.ReadUInt64();

                if(GUID.GUID_CONTENTDESCRIPTION.Equals(current_guid)) {
                    ReadContentDescription(stream, tag);
                    is_content_parsed = true;
                } else if(GUID.GUID_EXTENDED_CONTENT_DESCRIPTION.Equals(current_guid)) {
                    ReadExtendedDescription(stream, tag);
                    is_extended_parsed = true;
                }

                stream.Seek ((long)((ulong)chunk_start + chunk_len - (ulong)stream.Position), SeekOrigin.Current);
            }
        
            return tag;
        }
        
        private void ReadContentDescription(Stream stream, Tag tag)
        {
            BinaryReader reader = new BinaryReader(stream);
            ushort [] sizes = new ushort[5];
            
            for(int i = 0; i < sizes.Length; i++) {
                sizes[i] = reader.ReadUInt16();
            }
            
            string [] strings = new string[sizes.Length];
            
            for(int i = 0; i < strings.Length; i++) {
                strings[i] = ReadUtf16String(reader, sizes[i]).Trim();
            }
            
            if(sizes[0] > 0 && strings[0] != String.Empty) {
                tag.SetTitle(strings[0]);
            }
            
            if(sizes[1] > 0 && strings[1] != String.Empty) {
                tag.SetArtist(strings[1]);
            }
            
            if(sizes[3] > 0 && strings[3] != String.Empty) {
                tag.SetComment(strings[3]);
            }
        }

        private void ReadExtendedDescription(Stream stream, Tag tag) 
        {
            BinaryReader reader = new BinaryReader(stream);
            ushort field_count = reader.ReadUInt16();
            
            for(int i = 0; i < field_count; i++) {
                ushort field_length = reader.ReadUInt16();
                string property_name = ReadUtf16String(reader, field_length);
                ushort property_type = reader.ReadUInt16();
                string property_value = String.Empty;
                
                switch(property_type) {
                    case 0:
                        property_value = ReadUtf16String(reader, reader.ReadUInt16());
                        break;
                    case 1:
                        stream.Seek(reader.ReadUInt16(), SeekOrigin.Current);
                        break;
                    case 2:
                        reader.ReadUInt16();
                        property_value = (reader.ReadUInt32() == 1).ToString();
                        break;
                    case 3:
                        property_value = reader.ReadUInt32().ToString();
                        break;
                    case 4:
                        property_value = reader.ReadUInt64().ToString();
                        break;
                    case 5:
                        property_value = reader.ReadUInt16().ToString();
                        break;
                }
                
                switch(property_name) {
                    case "WM/AlbumTitle":
                        tag.SetAlbum(property_value);
                        break;
                    case "WM/AlbumArtist": 
                        tag.SetArtist(property_value);
                        break;
                    case "WM/TrackNumber":
                        tag.SetTrack(property_value);
                        break;
                    case "WM/Year": 
                        tag.SetYear(property_value);
                        break;
                    case "WM/Genre": 
                        tag.SetGenre(property_value);
                        break;
                }
            }
        }

        private static readonly UnicodeEncoding Utf16Encoding = new UnicodeEncoding();

        private static String ReadUtf16String(BinaryReader reader, int length) 
        {
            byte [] bytes = new byte[length];
            reader.Read(bytes, 0, bytes.Length);
            
            if(bytes.Length - 2 >= 0 && bytes[bytes.Length - 1] == 0 && bytes[bytes.Length - 2] == 0) {
                byte [] copy = new byte[bytes.Length - 2];
                Array.Copy(bytes, 0, copy, 0, copy.Length);
                bytes = copy;
            }
            
            return Utf16Encoding.GetString(bytes, 0, bytes.Length);
        }
    }
}