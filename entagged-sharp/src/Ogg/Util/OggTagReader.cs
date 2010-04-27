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
using Entagged.Audioformats.Util;

namespace Entagged.Audioformats.Ogg.Util {
	public class OggTagReader {

		public OggTag Read( Stream oggStream )
		{
			OggTag tag = new OggTag();
			
			byte[] b = new byte[4];
			oggStream.Read( b , 0,  b .Length);
			int vendorstringLength = Utils.GetNumber( b, 0, 3);
			b = new byte[vendorstringLength];
			oggStream.Read( b , 0,  b .Length);
			tag.Add("vendor", new string(Encoding.UTF8.GetChars(b)));
			
			b = new byte[4];
			oggStream.Read( b , 0,  b .Length);
			int userComments = Utils.GetNumber( b, 0, 3);

			for ( int i = 0; i < userComments; i++ ) {
				b = new byte[4];
				oggStream.Read( b , 0,  b .Length);
				int commentLength = Utils.GetNumber( b, 0, 3);

				b = new byte[commentLength];
				oggStream.Read( b , 0,  b .Length);
				tag.AddOggField(b);
			}
			
			return tag;
		}
	}
}
