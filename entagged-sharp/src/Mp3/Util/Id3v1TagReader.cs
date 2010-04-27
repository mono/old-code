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

using System.IO;
using System.Text;
using Entagged.Audioformats.Exceptions;

namespace Entagged.Audioformats.Mp3.Util {
	public class Id3v1TagReader {
		public bool Read(Id3Tag tag, Stream mp3Stream)
		{
			//Check wether the file contains an Id3v1 tag--------------------------------
			mp3Stream.Seek( -128 , SeekOrigin.End);
			
			byte[] b = new byte[3];
			mp3Stream.Read( b, 0, 3 );
			mp3Stream.Seek(0, SeekOrigin.Begin);
			string tagS = Encoding.ASCII.GetString(b);
			if(tagS != "TAG")
				return false;
			
			mp3Stream.Seek( - 128 + 3, SeekOrigin.End );
			//Parse the tag -)------------------------------------------------
			tag.AddTitle(Read(mp3Stream, 30));
			tag.AddArtist(Read(mp3Stream, 30));
			tag.AddAlbum(Read(mp3Stream, 30));
			//------------------------------------------------
			tag.AddYear(Read(mp3Stream, 4));
			tag.AddComment(Read(mp3Stream, 30));

			//string trackNumber;
			mp3Stream.Seek(- 2, SeekOrigin.Current);
			b = new byte[2];
			mp3Stream.Read(b, 0, 2);
			
			if (b[0] == 0)
				tag.AddTrack(b[1].ToString());

			byte genreByte = (byte) mp3Stream.ReadByte();
			mp3Stream.Seek(0, SeekOrigin.Begin);
			tag.AddGenre(TagGenres.Get(genreByte));

			return true;
		}
		
		private string Read(Stream mp3Stream, int length)
		{
			byte[] b = new byte[length];
			mp3Stream.Read( b, 0, b.Length );
			string ret;
			
            if(Entagged.Audioformats.Util.UnicodeValidator.ValidateUtf8(b))
                ret = Encoding.UTF8.GetString(b).Trim();
            else 
                ret = Encoding.GetEncoding("ISO-8859-1").GetString(b).Trim();

			int pos = ret.IndexOf('\0');
			if (pos != -1)
				ret = ret.Substring(0, pos);

			return ret;
		}
	}
}
