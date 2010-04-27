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
using System.Text;
using Entagged.Audioformats.Util;
using Entagged.Audioformats.Mp3;

namespace Entagged.Audioformats.Mp3.Util.Id3Frames {
	public class CommId3Frame : TextId3Frame {
		
		private string shortDesc;
		private string lang;
		
		/*
		 * 0,1| frame flags
		 * 2| encoding
		 * 3,4,5| lang
		 * 6,..,0x00(0x00)| short descr
		 * x,..| actual comment
		 */
		
		public CommId3Frame(string content) : base("COMM", content) {
			this.shortDesc = "";
			this.lang = "eng";
		}
		
		public CommId3Frame(byte[] rawContent, byte version) : base("COMM", rawContent, version) {}
		
		public string Langage {
			get { return this.lang; }
		}
		
		protected override void Populate(byte[] raw)
		{
			this.encoding = raw[flags.Length];
			if (flags.Length + 1 + 3 > raw.Length - 1) {
				this.lang = "XXX";
				this.content = "";
				this.shortDesc = "";
				return;
			}
			
			this.lang = System.Text.Encoding.GetEncoding("ISO-8859-1").GetString(raw, flags.Length+1, 3);
			
			int commentStart = GetCommentStart(raw,flags.Length + 4, Encoding);
			this.shortDesc = GetString(raw, flags.Length + 4, commentStart - flags.Length - 4, Encoding);
			this.content = GetString(raw, commentStart, raw.Length - commentStart, Encoding);	
		}
		
		/**
		 * This methods interprets content to be a valid comment section. where
		 * first comes a short comment directly after that the comment section.
		 * This method searches for the terminal character of the short description,
		 * and return the index of the first byte of the fulltext comment. 
		 * 
		 * @param content The comment data.
		 * @param offset The offset where the short descriptions is about to start.
		 * @param encoding the encoding of the field.
		 * @return the index (including given offset) for the first byte of the fulltext comment.
		 */
		public int GetCommentStart (byte[] content, int offset, string encoding) {
			int result = 0;
			if (Encoding == "UTF-16") {
				for (result = offset; result < content.Length; result+=2) {
					if (content[result] == 0x00 && content[result+1] == 0x00) {
						result += 2;
						break;
					}
				}
			} else {
				for (result = offset; result < content.Length; result++) {
					if (content[result] == 0x00) {
						result++;
						break;
					}
				}
			}
			return result;
		}
	
		protected override byte[] Build()
		{
			byte[] shortDescData = GetBytes(this.shortDesc, Encoding);
			byte[] contentData = GetBytes(this.content, Encoding);
			byte[] data = new byte[shortDescData.Length + contentData.Length];
			Copy(shortDescData, data, 0);
			Copy(contentData, data, shortDescData.Length);
			
			byte[] lan = System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(this.lang);
			
			//the return byte[]
			byte[] b = new byte[4 + 4 + flags.Length + 1 + 3 + data.Length];
			
			int offset = 0;
			Copy(IdBytes, b, offset);        offset += 4;
			Copy(GetSize(b.Length-10), b, offset); offset += 4;
			Copy(flags, b, offset);               offset += flags.Length;
			
			b[offset] = this.encoding;	offset += 1;
			
			Copy(lan, b, offset);		offset += lan.Length;
			
			Copy(data, b, offset);
			
			return b;
		}
		
		public string ShortDescription {
			get { return shortDesc; }
		}
		
		public override bool IsEmpty {
		    get { return this.content == "" && this.shortDesc == ""; }
		}
		
		public override void CopyContent(TagField field)
		{
		    base.CopyContent(field);
		    if(field is CommId3Frame) {
		        this.shortDesc = (field as CommId3Frame).ShortDescription;
		        this.lang = (field as CommId3Frame).Langage;
		    }
		}
		
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("[").Append(Langage).Append("] ").Append("(").Append(ShortDescription).Append(") ").Append(Content);
			return sb.ToString();
		}
	}
}
