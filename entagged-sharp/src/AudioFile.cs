/***************************************************************************
 *  Copyright 2005 Novell, Inc.
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
using Entagged.Audioformats;
using Entagged.Audioformats.Util;

namespace Entagged
{
    public class AudioFile
    {
        private AudioFileContainer container;
        private string filename;
        private string mimetype;
        
        public AudioFile(string filename)
        {
            this.filename = filename;
            container = AudioFileIO.Read(filename);
        }

        public AudioFile(string filename, string mimetype)
        {
            this.filename = filename;
            this.mimetype = mimetype;
            container = AudioFileIO.Read(filename, mimetype);
        }

        public AudioFile(Stream stream, string mimetype)
        {
            this.mimetype = mimetype;
            container = AudioFileIO.Read(stream, mimetype);
        }
        
        public int Bitrate {
            get { 
                return container.EncodingInfo.Bitrate; 
            }
        }

        public int Channels {
            get { 
                return container.EncodingInfo.ChannelNumber; 
            }
        }
        
        public string EncodingType {
            get { 
                return container.EncodingInfo.EncodingType; 
            }
        }

        public string ExtraEncodingInfo {
            get { 
                return container.EncodingInfo.ExtraEncodingInfos; 
            }
        }

        public int SampleRate {
            get { 
                return container.EncodingInfo.SamplingRate; 
            }
        }

        public TimeSpan Duration {
            get { 
                return container.EncodingInfo.Duration; 
            }
        }
        
        public bool IsVbr {
            get { 
                return container.EncodingInfo.Vbr; 
            }
        }

        public string [] Genres {
            get { 
               return Utils.FieldListToStringArray(Tag.Genre); 
            }
        }

        public string [] Titles
        {
            get { 
                return Utils.FieldListToStringArray(Tag.Title); 
            }
        }

        public int [] TrackNumbers
        {
            get { 
                return Utils.FieldListToIntArray(Tag.Track); 
            }
        }

        public int [] TrackCounts
        {
            get { 
                return Utils.FieldListToIntArray(Tag.TrackCount); 
            }
        }

        public int [] Years
        {
            get { 
                return Utils.FieldListToIntArray(Tag.Year); 
            }
        }

        public string [] Albums {
            get { 
                return Utils.FieldListToStringArray(Tag.Album); 
            }
        }

        public string [] Artists {
            get { 
                return Utils.FieldListToStringArray(Tag.Artist); 
            }
        }

        public string [] Comments {
            get { 
                return Utils.FieldListToStringArray(Tag.Comment); 
            }
        }

        public string [] Licenses {
            get { 
                return Utils.FieldListToStringArray(Tag.License);
            }
        }

        public string Genre {
            get {
                return Tag.Genre.Count > 0 ? Tag.Genre[0] as string : null;
            }
        }
        
        public string Title {
            get {
                return Tag.Title.Count > 0 ? Tag.Title[0] as string : null;
            }
        }
        
        public int TrackNumber {
            get {
                try {
                    return Convert.ToInt32(Tag.Track[0] as string);
                } catch(Exception) {
                    return 0;
                }
            }
        }

        public int TrackCount {
            get {
                try {
                    return Convert.ToInt32(Tag.TrackCount[0] as string);
                } catch(Exception) {
                    return 0;
                }
            }
        }

        public int Year {
            get {
                try {
                    return Convert.ToInt32(Tag.Year[0] as string);
                } catch(Exception) {
                    return 0;
                }
            }
        }

        public string Album {
            get {
                return Tag.Album.Count > 0 ? Tag.Album[0] as string : null;
            }
        }
        
        public string Artist {
            get {
                return Tag.Artist.Count > 0 ? Tag.Artist[0] as string : null;
            }
        } 
        
        public string Comment {
            get {
                return Tag.Comment.Count > 0 ? Tag.Comment[0] as string : null;
            }
        }
        
        public string License {
            get {
                return Tag.License.Count > 0 ? Tag.License[0] as string : null;
            }
        }
        
        public Tag Tag {
            get { 
                return container.Tag; 
            }
        }
        
        public string FileName {
            get { 
                return filename; 
            }
        }

        public string MimeType {
            get { 
                return mimetype; 
            }
        }
        
        public EncodingInfo EncodingInfo
        {
            get { 
                return container.EncodingInfo; 
            }
        }
        
        public override string ToString() 
        {
            return container.ToString();
        }
    }
}
