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
using Entagged.Audioformats.Util;
using Entagged.Audioformats.Mp3.Util.Id3Frames;

namespace Entagged.Audioformats.Mp3.Util {
	public class Id3v24TagReader {
		
		private Hashtable conversion;
		
		public Id3v24TagReader() {
			InitConversionTable();
		}
		
		public void Read(Id3Tag tag, ByteBuffer data, bool[] ID3Flags, byte version)
		{
			// get the tagsize from the buffers size.
			int tagSize = data.Limit;
			byte[] b;

			// Create a result object
			// Id3v2Tag tag = new Id3v2Tag();
			
			// ---------------------------------------------------------------------
			// If the flags indicate an extended header to be present, read its
			// size and skip it. (It does not contain any useful information, maybe
			// CRC)
			if ((version == Id3Tag.ID3V23 || version == Id3Tag.ID3V24) && ID3Flags[1])
				ProcessExtendedHeader(data, version);
			//----------------------------------------------------------------------------
			/*
			 * Now start the extraction of the text frames.
			 */
			// The frame names differ in lengths between version 2 to 3
			int specSize = (version == Id3Tag.ID3V22) ? 3 : 4;
			
			// As long as we have unread bytes...
			for (int a = 0; a < tagSize; a++) {
				// Create buffer taking the name of the frame.
				b = new byte[specSize];
				
				// Do we still have enough bytes for reading the name?
				if(data.Remaining <= specSize)
					break;
				
				// Read the Name
				data.Get(b);
				
				// Convert the bytes (of the name) into a String.
				string field = Encoding.ASCII.GetString(b);
				// If byte[0] is zero, we have invalid data
				if (b[0] == 0)
					break;
				
				// Now we read the length of the current frame
				int frameSize = ReadInteger(data, version);

				// If the framesize is greater than the bytes we've left to read,
				// or the frame length is zero, abort. Invalid data
				if ((frameSize > data.Remaining) || frameSize <= 0)
					//Ignore empty frames
					break;
				
				b = new byte[frameSize + ((version == Id3Tag.ID3V23 || version == Id3Tag.ID3V24) ? 2 : 0)];
				// Read the complete frame into the byte array.
				data.Get(b);
				
				// Check the frame name once more
				if (field != "") {
					/*
					 * Now catch possible errors occuring in the data
					 * interpretation. Even if a frame is not valid regarding the
					 * spec, the rest of the tag could be read.
					 */
					try {
						// Create the Frame upon the byte array data.
						AddId3Frame(tag, field, b, version);
					} catch (Exception e) {
						//FIXME: do we have to output anything here?
					}
				}
			}
		}
		
		private void AddId3Frame(Id3Tag tag, string field, byte[] b, byte version) {
			if(version == Id3Tag.ID3V22)
				field = ConvertFromId3v22(field);
				
			if (field == "" || field.Length < 4)
				return;
			//FIXME: We do not support non-text frames yet.
			if (field != "COMM" && (field[0] != 'T' || field[1] == 'X'))
				return;

			if (field == "COMM") {
				TextId3Frame f = new CommId3Frame(b, version);
				tag.AddComment(f.Content);
			} else if (field[0] == 'T' && field[1] != 'X') {
				/*
				FIXME: Add support for: they have a special format
				if (field.equalsIgnoreCase("TDRC")) {
					return new TimeId3Frame(field, data, version);
				}
					return new TextId3Frame(field, data, version);
				*/
				TextId3Frame f = new TextId3Frame(field, b, version);
				switch (field) {
				case "TCON": // genre
					tag.AddGenre(TranslateGenre(f.Content));
					break;

				case "TRCK": // track number
					string num, count;
					Utils.SplitTrackNumber(f.Content, out num, out count);
					if (num != null)
						tag.AddTrack(num);
					if (count != null)
						tag.AddTrackCount(count);
					break;

				default:
					tag.Add(field, f.Content);
					break;
				}
			}
		}
		
		private string TranslateGenre(string content)
		{
			if (content == null || content.Length == 0)
				return null;

			// Written as "Name" ?
			if (content[0] != '(')
				return content;

			int pos = content.IndexOf(')');
			if (pos == -1)
				return content;

			// Written as "(id)" ?
			if (pos == content.Length - 1) {
				int num = Convert.ToInt32(content.Substring(1, content.Length - 2));
				return TagGenres.Get(num);
			}

			// Written as "(id)Name" ?
			return content.Substring(pos + 1);
		}

		private void InitConversionTable()
		{

			//TODO: APIC frame must update the mime-type to be converted ??
			//TODO: LINK frame (2.3) has a frame ID of 3-bytes making
			//      it incompatible with 2.3 frame ID of 4bytes, WTF???
			
			this.conversion = new Hashtable();
			string[] v22 = {
					"BUF", "CNT", "COM", "CRA",
					"CRM", "ETC", "EQU", "GEO",
					"IPL", "LNK", "MCI", "MLL",
					"PIC", "POP", "REV", "RVA",
					"SLT", "STC", "TAL", "TBP",
					"TCM", "TCO", "TCR", "TDA",
					"TDY", "TEN","TFT", "TIM",
					"TKE", "TLA", "TLE", "TMT",
					"TOA", "TOF", "TOL", "TOR",
					"TOT", "TP1", "TP2", "TP3",
					"TP4", "TPA", "TPB", "TRC",
					"TRD", "TRK", "TSI", "TSS",
					"TT1", "TT2", "TT3", "TXT",
					"TXX", "TYE", "UFI", "ULT",
					"WAF", "WAR", "WAS", "WCM",
					"WCP", "WPB", "WXX"
				};
			string[] v23 = {
					"RBUF", "PCNT", "COMM", "AENC",
					"", "ETCO", "EQUA", "GEOB",
					"IPLS", "LINK", "MCDI", "MLLT",
					"APIC", "POPM", "RVRB", "RVAD",
					"SYLT", "SYTC", "TALB", "TBPM",
					"TCOM", "TCON", "TCOP", "TDAT",
					"TDLY", "TENC", "TFLT", "TIME",
					"TKEY", "TLAN", "TLEN", "TMED",
					"TOPE", "TOFN", "TOLY", "TORY",
					"TOAL", "TPE1", "TPE2", "TPE3",
					"TPE4", "TPOS", "TPUB", "TSRC",
					"TRDA", "TRCK", "TSIZ", "TSSE",
					"TIT1", "TIT2", "TIT3", "TEXT",
					"TXXX", "TYER", "UFID", "USLT",
					"WOAF", "WOAR", "WOAS", "WCOM",
					"WCOP", "WPUB", "WXXX"			
				};
			
			for(int i = 0; i<v22.Length; i++)
				this.conversion[v22[i]] = v23[i];
		}
		
		private string ConvertFromId3v22(string field)
		{
			string s = this.conversion[field] as string;
			
			if(s == null)
				return "";
			
			return s;
		}

		//Process the Extended Header in the ID3v2 Tag, returns the number of bytes to skip
		private int ProcessExtendedHeader(ByteBuffer data, byte version)
		{
			//TODO Verify that we have an syncsfe int
			int extsize = 0;
			byte[] exthead = new byte [4];
			data.Get(exthead);
			if (version == Id3Tag.ID3V23)
				extsize = ReadInteger(data, Id3Tag.ID3V23);
			else
				extsize = Utils.ReadSyncsafeInteger(new byte[]{data.Get(), data.Get(), data.Get(), data.Get()});

			// The extended header size includes those first four bytes.
			data.Position = data.Position + extsize;
			return extsize;
		}

		private int ReadInteger(ByteBuffer bb, int version)
		{
			int value = 0;
			
			if(version == Id3Tag.ID3V24)
				value = Utils.ReadSyncsafeInteger(new byte[]{bb.Get(), bb.Get(), bb.Get(), bb.Get()});
			else {
				if(version == Id3Tag.ID3V23)
					value += (bb.Get()& 0xFF) << 24;
				value += (bb.Get()& 0xFF) << 16;
				value += (bb.Get()& 0xFF) << 8;
				value += (bb.Get()& 0xFF);
			}

			return value;
		}
	}
}
