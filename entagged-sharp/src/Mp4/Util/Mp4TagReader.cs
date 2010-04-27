/***************************************************************************
 *  Copyright 2005 Novell, Inc.
 *  Aaron Bockover <aaron@aaronbock.net>
 *  Geoff Norton <gnorton@customerdna.com>
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
using System.Net;

using Entagged.Audioformats.Util;
using Entagged.Audioformats.Exceptions;

namespace Entagged.Audioformats.Mp4.Util
{
    public class Mp4TagReader
    {
        private BinaryReader br;
        private Tag tag;
        private EncodingInfo ei;

        public Tag ReadTags(Stream raf)
        {
            Read(raf);
            return tag;
        }

        public EncodingInfo ReadEncodingInfo(Stream raf)
        {
            Read(raf);
            return ei;
        }

        public void Read(Stream raf)
        {
            tag = new Tag();
            ei = new EncodingInfo();
            br = new BinaryReader(raf);
            Parse();
        }

        private void Parse()
        {
            byte[ ] buffer = new byte[4];
            
            br.Read(buffer, 0, 4);
            br.Read(buffer, 0, 4);
            
            if(buffer[0] != (byte)'f' && buffer[1] != (byte)'t' && buffer[2] != (byte)'y' && buffer[3] != (byte)'p') {
                throw new CannotReadException("File does not appear to be an MP4 file (bad header)");
            }
        
            br.BaseStream.Seek(0, SeekOrigin.Begin);
            
            long pos = 0;
            int level = 0;
            long len = br.BaseStream.Length;

            ParseContainer(ref pos, ref len, level, br);
        }

        private void ParseMvhd(ref long pos, ref long len, long size, BinaryReader br)
        {
            byte [] bytes = br.ReadBytes((int)size);
            int scale = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(bytes, 12));
            int duration = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(bytes, 16));
            ei.Duration = new TimeSpan((duration / scale) * TimeSpan.TicksPerSecond);
            pos += size;
        }

        private void ParseData(ref long pos, ref long len, int level, BinaryReader br, DataAtoms dataatom)
        {
            byte [] buffer = new byte[4];
            pos += br.Read(buffer, 0, 4);
            
            long size = BitConverter.ToInt32(new byte [] { buffer[3], buffer[2], buffer[1], buffer[0] }, 0);
            pos += br.Read(buffer, 0, 4);
            pos += br.Read(buffer, 0, 4);
            
            long type = BitConverter.ToInt32(new byte [] { buffer[3], buffer[2], buffer[1], buffer[0] }, 0);
            size -= 16;
            type &= 255;
            pos += br.Read(buffer, 0, 4);
            
            byte [] data = new byte[size];
            pos += br.Read(data, 0, (int)size);
            
            switch(type) {
                case 0:
                    int [] intvals = new int[size / 2];
                    for(int i = 0; i < size / 2; i++) {
                        intvals[i] = BitConverter.ToInt16(new byte[] { 
                          data[1 + (i * 2)], 
                          data[0 + (i * 2)] 
                        }, 0);
                    }
                    
                    switch(dataatom) {
                        case DataAtoms.GNRE:
                            tag.AddGenre(GENRE_MAP[intvals[0]]);
                            break;
                        case DataAtoms.TRKN:
                            tag.AddTrack(Convert.ToString(intvals[1]));
                            break;
                        default:
                            Console.WriteLine("DataAtom: {0}", dataatom);
                            break;
                    }
                    
                    break;
                case 1:
                    CommonField field = 0;
                    switch(dataatom) {
                        case DataAtoms.GEN:
                            field = CommonField.Genre;
                            break;
                        case DataAtoms.NAM:
                            field = CommonField.Title;
                            break;
                        case DataAtoms.ART:
                            field = CommonField.Artist;
                            break;
                        case DataAtoms.ALB:
                            field = CommonField.Album;
                            break;
                        case DataAtoms.DAY:
                            field = CommonField.Year;
                            break;
                        case DataAtoms.CMT:
                            field = CommonField.Comment;
                            break;
                    }
                    
                    tag.Add(field, Encoding.Default.GetString(data));
                    break;
                case 2:
                    // other byte data
                    break;
                default:
                    // non-standard data
                    break;
            }
        }
        
        internal void ParseContainer(ref long pos, ref long len, int level, BinaryReader br)
        {
            byte [] buffer = new byte[4];
            int stuck_count = 0;
            
            level++;
            
            while(pos < len) {
                pos += br.Read(buffer, 0, 4);
                long size = BitConverter.ToInt32(new byte[] { buffer[3], buffer[2], buffer[1], buffer[0] }, 0);
                pos += br.Read(buffer, 0, 4);
                
                string id = Encoding.UTF8.GetString(buffer);
                
                if(size == 1) {
                    pos += br.Read(buffer, 0, 4);
                    long hi = BitConverter.ToInt32(new byte[] { buffer[3], buffer[2], buffer[1], buffer[0] }, 0);
                    pos += br.Read(buffer, 0, 4);
                    long lo = BitConverter.ToInt32(new byte[] { buffer[3], buffer[2], buffer[1], buffer[0] }, 0);
                    size = hi * (2 ^ 32) + lo - 16;
                } else {
                    size -= 8;
                }
                
                if(size <= 0) {
                    if(size != 0 && level != 1) {
                        throw new CannotReadException("MP4 Container Parsing error");
                    }
                }
                
                // deal with potentially corrupt files: hack, maybe we shouldn't even care
                if(size == -8 && level == 1) {
                    if(stuck_count++ >= 100) {
                        throw new CannotReadException(
                            String.Format("MP4 Container Parsing Error: Stuck at position {0}", pos));
                    }
                }

                switch(id.ToUpper()) {
                    case "NAM":
                        ParseData(ref pos, ref len, level, br, DataAtoms.NAM);
                        break;
                    case "ART":
                        ParseData(ref pos, ref len, level, br, DataAtoms.ART);
                        break;
                    case "ALB":
                        ParseData(ref pos, ref len, level, br, DataAtoms.ALB);
                        break;
                    case "DAY":
                        ParseData(ref pos, ref len, level, br, DataAtoms.DAY);
                        break;
                    case "CMT":
                        ParseData(ref pos, ref len, level, br, DataAtoms.CMT);
                        break;
                    case "GEN":
                        ParseData(ref pos, ref len, level, br, DataAtoms.GEN);
                        break;
                    case "GNRE":
                        ParseData(ref pos, ref len, level, br, DataAtoms.GNRE);
                        break;
                    case "TRKN":
                        ParseData(ref pos, ref len, level, br, DataAtoms.TRKN);
                        break;
                    case "META":
                        br.BaseStream.Seek(4, SeekOrigin.Current);
                        pos += 4;
                        ParseContainer(ref pos, ref len, level, br);
                        break;
                    case "MOOV":
                    case "ILST":
                    case "MDIA":
                    case "MNIF":
                    case "STBL":
                    case "TRAK":
                    case "UDTA":
                        ParseContainer(ref pos, ref len, level, br);
                        break;
                    case "MVHD":
                        ParseMvhd (ref pos, ref len, size, br);
                        break;
                    case "MDAT":
                    case "FREE":
                    default:
                        br.BaseStream.Seek(size, SeekOrigin.Current);
                        pos += size;
                        break;
                }
            }
        }
        
        static string [] GENRE_MAP = new string [] {
            "N/A", "Blues", "Classic Rock", "Country", "Dance", "Disco",
            "Funk", "Grunge", "Hip-Hop", "Jazz", "Metal", "New Age", "Oldies",
            "Other", "Pop", "R&B", "Rap", "Reggae", "Rock", "Techno",
            "Industrial", "Alternative", "Ska", "Death Metal", "Pranks",
            "Soundtrack", "Euro-Techno", "Ambient", "Trip-Hop", "Vocal",
            "Jazz+Funk", "Fusion", "Trance", "Classical", "Instrumental",
            "Acid", "House", "Game", "Sound Clip", "Gospel", "Noise",
            "AlternRock", "Bass", "Soul", "Punk", "Space", "Meditative",
            "Instrumental Pop", "Instrumental Rock", "Ethnic", "Gothic",
            "Darkwave", "Techno-Industrial", "Electronic", "Pop-Folk",
            "Eurodance", "Dream", "Southern Rock", "Comedy", "Cult", "Gangsta",
            "Top 40", "Christian Rap", "Pop/Funk", "Jungle", "Native American",
            "Cabaret", "New Wave", "Psychadelic", "Rave", "Showtunes",
            "Trailer", "Lo-Fi", "Tribal", "Acid Punk", "Acid Jazz", "Polka",
            "Retro", "Musical", "Rock & Roll", "Hard Rock", "Folk",
            "Folk/Rock", "National Folk", "Swing", "Fast-Fusion", "Bebob",
            "Latin", "Revival", "Celtic", "Bluegrass", "Avantgarde",
            "Gothic Rock", "Progressive Rock", "Psychedelic Rock",
            "Symphonic Rock", "Slow Rock", "Big Band", "Chorus",
            "Easy Listening", "Acoustic", "Humour", "Speech", "Chanson",
            "Opera", "Chamber Music", "Sonata", "Symphony", "Booty Bass",
            "Primus", "Porn Groove", "Satire", "Slow Jam", "Club", "Tango",
            "Samba", "Folklore", "Ballad", "Power Ballad", "Rhythmic Soul",
            "Freestyle", "Duet", "Punk Rock", "Drum Solo", "A capella",
            "Euro-House", "Dance Hall", "Goa", "Drum & Bass", "Club House",
            "Hardcore", "Terror", "Indie", "BritPop", "NegerPunk",
            "Polsk Punk", "Beat", "Christian Gangsta", "Heavy Metal",
            "Black Metal", "Crossover", "Contemporary C", "Christian Rock",
            "Merengue", "Salsa", "Thrash Metal", "Anime", "JPop", "SynthPop"
        };
        
        private enum DataAtoms {
            AART,
            AKID,
            ALB,
            APID,
            ATID,
            ART,
            CMT,
            CNID,
            CPIL,
            CPRT,
            DAY,
            DISK,
            GEID,
            GEN,
            GNRE,
            GRP,
            NAM,
            PLID,
            RTNG,
            TMPO,
            TOO,
            TRKN,
            WRT
        }
    }
}

