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

namespace Entagged.Audioformats.Util 
{
    public class Utils 
    {
        public static long GetLongNumber(byte [] b, int start, int end) 
        {
            long number = 0;
            
            for(int i = 0; i < (end - start + 1); i++) {
                number += ((b[start + i] & 0xFF) << i * 8);
            }
            
            return number;
        }
        
        public static int GetNumber(byte [] b, int start, int end) 
        {
            int number = 0;
        
            for(int i = 0; i<(end-start+1); i++) {
                number += ((b[start + i] & 0xFF) << i * 8);
            }
            
            return number;
        }
        
        public static byte [] GetNumber(int number) 
        {
            return GetNumber(number, 4);
        }
        
        public static byte [] GetNumber(long number, int size) 
        {
            byte [] b = new byte[size];
            
            for(int i = 0; i < size; i++) {
                b[i] = (byte)((size & (0xFF << 8 * i) ) >> 8 * i);
            }

            return b;
        }
        
        public static int ReadSyncsafeInteger(byte [] b) 
        {
            return ((b[0] & 0xFF) << 21) + ((b[1] & 0xFF) << 14) + ((b[2] & 0xFF) << 7) + (b[3] & 0xFF);
        }
        
        public static byte [] GetSyncSafe(int value) 
        {
            byte[] result = new byte[4];
            
            for(int i = 0; i < 4; i++) {
                result[i] = (byte)(value >> ((3 - i) * 7) & 0x7F);
            }
            
            return result;
        }
    
        public static byte [] GetBytes(string s, string encoding) 
        {
            return System.Text.Encoding.GetEncoding(encoding).GetBytes(s);
        }
        
        public static byte [] GetBytes(string s) 
        {
            return System.Text.Encoding.ASCII.GetBytes(s);
        }
        
        public static string [] FieldListToStringArray(IList taglist)
        {
            string[] ret = new string[taglist.Count];
            int i = 0;
            
            foreach (string field in taglist)
                ret[i++] = (field != null && field.Trim().Length > 0) ?
                    field : null;
            
            return ret;
        }

        public static int [] FieldListToIntArray(IList taglist)
        {
            int [] ret = new int[taglist.Count];
            int i = 0;
            
            foreach(string field in taglist) {
                if(field.Length > 0) {
                    ret[i++] = Convert.ToInt32(field);
                }
            }
            
            return ret;
        }

        public static Tag CombineTags(params Tag [] tags)
        {
            Tag ret = new Tag();

            foreach(Tag tag in tags) {
                if(tag == null) {
                    continue;
                }

                foreach(TagTextField artist in tag.Artist) {
                    ret.AddArtist (artist.Content);
                }
                
                foreach(TagTextField album in tag.Album) {
                    ret.AddAlbum(album.Content);
                }

                foreach(TagTextField title in tag.Title) {
                    ret.AddTitle(title.Content);
                }
                
                foreach(TagTextField track in tag.Track) {
                    ret.AddTrack(track.Content);
                }
                
                foreach(TagTextField trackcount in tag.TrackCount) {
                    ret.AddTrackCount(trackcount.Content);
                }
                
                foreach(TagTextField year in tag.Year) {
                    ret.AddYear(year.Content);
                }

                foreach(TagTextField comment in tag.Comment) {
                    ret.AddComment(comment.Content);
                }
                
                foreach(TagTextField license in tag.License) {
                    ret.AddLicense(license.Content);
                }
                
                foreach(TagTextField genre in tag.Genre) {
                    ret.AddGenre(genre.Content);
                }
            }

            return ret;
        }

        // Splits (e.g.) "1/6" into track 1 of 6.
        public static void SplitTrackNumber(string content, out string num, out string count)
        {
            string [] split = content.Split(new char [] {'/'}, 2);
            
            if(split.Length == 1) {
                num = content;
                count = null;
            } else {
                num = split[0];
                count = split[1];
            }
        }
    }
    
    public static class UnicodeValidator
    {
        public static bool ValidateUtf8(byte [] str) 
        {
            int i, min = 0, val = 0;
            
            try {
                for(i = 0; i < str.Length; i++) {
                    if(str[i] < 128) {
                        continue;
                    }
                    
                    if((str[i] & 0xe0) == 0xc0) { /* 110xxxxx */
                        if((str[i] & 0x1e) == 0) {
                            return false;
                        }
                        
                        if((str[++i] & 0xc0) != 0x80) { /* 10xxxxxx */
                            return false;
                        }
                    } else {
                        bool skip_next_continuation = false;
                        
                        if((str[i] & 0xf0) == 0xe0) { /* 1110xxxx */
                            min = 1 << 11;
                            val = str[i] & 0x0f;
                            skip_next_continuation = true;
                        } else if((str[i] & 0xf8) == 0xf0) { /* 11110xxx */
                            min = 1 << 16;
                            val = str[i] & 0x07;  
                        } else {
                            return false;
                        }
                        
                        if(!skip_next_continuation && !IsContinuationChar(str, ++i, ref val)) {
                            return false;
                        }
                    
                        if(!IsContinuationChar(str, ++i, ref val)) {
                            return false;
                        }
                        
                        if(!IsContinuationChar(str, ++i, ref val)) {
                            return false;
                        }
                        
                        if(val < min || !IsValidUnicode(val)) {
                            return false;
                        }
                    }
                }
            } catch(IndexOutOfRangeException e) {
                return false;
            }

            return true;
        }
            
        private static bool IsContinuationChar(byte [] str, int i, ref int val)
        {
            if((str[i] & 0xc0) != 0x80) { /* 10xxxxxx */
                return false;
            }

            val <<= 6;  
            val |= str[i] & 0x3f;
            
            return true;
        }
        
        private static bool IsValidUnicode(int b)
        {
            return (b < 0x110000 && 
                ((b & 0xFFFFF800) != 0xD800) && 
                (b < 0xFDD0 || b > 0xFDEF) && 
                (b & 0xFFFE) != 0xFFFE);
        }
    }
}
