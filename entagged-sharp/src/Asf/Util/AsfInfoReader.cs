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
    public sealed class AsfInfoReader 
    {
        private static readonly string [][] CODEC_DESCRIPTIONS = new string [][] 
        {
            new string [] { "161",  " (Windows Media Audio (ver 7,8,9))" },
            new string [] { "162",  " (Windows Media Audio 9 series (Professional))" },
            new string [] { "163",  " (Windows Media Audio 9 series (Lossless))" },
            new string [] { "7A21", " (GSM-AMR (CBR))" },
            new string [] { "7A22", " (GSM-AMR (VBR))" }
        };

        private static string GetFormatDescription(ushort format) 
        {
            StringBuilder result = new StringBuilder(format.ToString("x"));
            string further = " (UNKOWN)";
            
            for(int i = 0; i < CODEC_DESCRIPTIONS.Length; i++) {
                if(CODEC_DESCRIPTIONS[i][0].ToUpper().Equals(result.ToString().ToUpper())) {
                    further = CODEC_DESCRIPTIONS[i][1];
                    break;
                }
            }
            
            if(result.Length % 2 != 0) {
                result.Insert(0, "0x0");
            } else {
                result.Insert(0, "0x");
            }
            
            result.Append(further);
            return result.ToString();
        }

        /*
        * TODO: Better Implementation of bitrate determination.
        * Somehow first audio stream makes the day. Then Streamnumber must
        * be stored, to read the right info out of an optional stream bitrate properties
        * chunk. Or if that comes first, store all the data and assign it on occurence of the
        * fist audio stream.
        * Where is the info about VBR
        */
        public EncodingInfo Read(Stream stream) 
        {
            EncodingInfo result = new EncodingInfo();
            GUID header_guid = GUID.ReadGUID(stream);
            
            if(!GUID.GUID_HEADER.Equals(header_guid)) {
                return result;
            }
            
            BinaryReader reader = new BinaryReader(stream);
                
            // Skip length of header
            stream.Seek(8, SeekOrigin.Current);

            // Read the number of chunks.
            uint chunk_count = reader.ReadUInt32();
           
            // Skip unknown bytes
            stream.Seek (2, SeekOrigin.Current);
            
            // Two flags, When both are set, all Information needed has ben read
            bool is_file_header_parsed = false;
            bool is_stream_chunk_parsed = false;

           // Now read the chunks
           for(int i = 0; i < chunk_count && !(is_file_header_parsed && is_stream_chunk_parsed); i++) {
               long chunk_start = stream.Position;
               GUID current_guid = GUID.ReadGUID(stream);
               ulong chunk_len = reader.ReadUInt64();
               
               if(GUID.GUID_FILE.Equals(current_guid)) {
                   stream.Seek(48, SeekOrigin.Current);
                   result.Duration = new TimeSpan((long)reader.ReadUInt64());
                   is_file_header_parsed = true;
               } else if(GUID.GUID_STREAM.Equals(current_guid)) {
                   GUID streamTypeGUID = GUID.ReadGUID(stream);
                   if(GUID.GUID_AUDIOSTREAM.Equals(streamTypeGUID)) {
                       // Jump over ignored values.
                       stream.Seek(38, SeekOrigin.Current);
                       result.EncodingType = GetFormatDescription(reader.ReadUInt16());
                       result.ChannelNumber = reader.ReadUInt16();
                       result.SamplingRate = (int)reader.ReadUInt32();
                       result.Bitrate = (int)(reader.ReadUInt32() * 8 / 1000);
                       is_stream_chunk_parsed = true;
                   }
               }
               
               stream.Seek((long)((ulong)chunk_start + chunk_len - (ulong)stream.Position), SeekOrigin.Current);
           }
           
           return result;
        }
    }
} 
